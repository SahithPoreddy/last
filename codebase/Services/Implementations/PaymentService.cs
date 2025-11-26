using codebase.Common;
using codebase.Common.Exceptions;
using codebase.Models.DTOs;
using codebase.Models.Entities;
using codebase.Models.Enums;
using codebase.Repositories.Interfaces;
using codebase.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using codebase.Hubs;

namespace codebase.Services.Implementations;

/// <summary>
/// Implementation of payment service
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IPaymentAttemptRepository _paymentRepository;
    private readonly IAuctionRepository _auctionRepository;
    private readonly IBidRepository _bidRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<PaymentService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<DashboardHub> _hubContext;

    public PaymentService(
        IPaymentAttemptRepository paymentRepository,
        IAuctionRepository auctionRepository,
        IBidRepository bidRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        ILogger<PaymentService> logger,
        IConfiguration configuration,
        IHubContext<DashboardHub> hubContext)
    {
        _paymentRepository = paymentRepository;
        _auctionRepository = auctionRepository;
        _bidRepository = bidRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _logger = logger;
        _configuration = configuration;
        _hubContext = hubContext;
    }

    public async Task<PaymentAttemptResponse> ConfirmPaymentAsync(
        int userId, int productId, ConfirmPaymentRequest request, bool testInstantFail = false)
    {
        _logger.LogInformation("Processing payment confirmation for Product: {ProductId} by User: {UserId}", 
            productId, userId);

        var auction = await _auctionRepository.GetByProductIdAsync(productId);
        if (auction == null)
        {
            throw new NotFoundException(AppConstants.AuctionNotFoundError);
        }

        var currentAttempt = await _paymentRepository.GetCurrentPendingAttemptAsync(auction.AuctionId);
        if (currentAttempt == null)
        {
            throw new BadRequestException("No pending payment attempt found for this auction");
        }

        if (currentAttempt.BidderId != userId)
        {
            throw new ForbiddenException("You are not authorized to confirm payment for this auction");
        }

        if (currentAttempt.WindowExpiryTime.HasValue && DateTime.UtcNow > currentAttempt.WindowExpiryTime.Value)
        {
            throw new BadRequestException("Payment window has expired");
        }

        if (testInstantFail)
        {
            _logger.LogInformation("Test instant fail triggered for Payment: {PaymentId}", currentAttempt.PaymentId);
            currentAttempt.Status = PaymentStatus.Failed;
            currentAttempt.CompletedAt = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(currentAttempt);
            await ProcessRetryAsync(auction.AuctionId, currentAttempt.AttemptNumber);
            
            return MapToPaymentAttemptResponse(currentAttempt);
        }

        var highestBid = await _bidRepository.GetHighestBidAsync(auction.AuctionId);
        if (highestBid == null)
        {
            throw new BadRequestException("No bids found for this auction");
        }

        if (request.ConfirmedAmount != highestBid.Amount)
        {
            _logger.LogWarning("Payment amount mismatch. Expected: {Expected}, Got: {Actual}", 
                highestBid.Amount, request.ConfirmedAmount);
            
            currentAttempt.Status = PaymentStatus.Failed;
            currentAttempt.ConfirmedAmount = request.ConfirmedAmount;
            currentAttempt.CompletedAt = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(currentAttempt);
            
            await ProcessRetryAsync(auction.AuctionId, currentAttempt.AttemptNumber);
            
            return MapToPaymentAttemptResponse(currentAttempt);
        }

        currentAttempt.Status = PaymentStatus.Success;
        currentAttempt.ConfirmedAmount = request.ConfirmedAmount;
        currentAttempt.CompletedAt = DateTime.UtcNow;
        await _paymentRepository.UpdateAsync(currentAttempt);

        auction.Status = AuctionStatus.Completed;
        await _auctionRepository.UpdateAsync(auction);

        _logger.LogInformation("Payment confirmed successfully for Auction: {AuctionId}", auction.AuctionId);

        // Send payment confirmation email
        var user = await _userRepository.GetByIdAsync(userId);
        var auctionWithDetails = await _auctionRepository.GetByIdWithDetailsAsync(auction.AuctionId);
        
        if (user != null && auctionWithDetails?.Product != null)
        {
            await _emailService.SendPaymentConfirmationAsync(
                user.Email,
                auctionWithDetails.Product.Name,
                request.ConfirmedAmount
            );
            
            _logger.LogInformation("Payment confirmation email sent to {Email} for Auction: {AuctionId}", 
                user.Email, auction.AuctionId);
        }

        // Notify dashboard via SignalR
        await NotifyDashboardUpdate();

        return MapToPaymentAttemptResponse(currentAttempt);
    }

    public async Task<List<PaymentAttemptResponse>> GetTransactionsAsync()
    {
        var attempts = await _paymentRepository.GetAllAsync();

        return attempts.Select(MapToPaymentAttemptResponse).ToList();
    }

    public async Task<DashboardResponse> GetDashboardMetricsAsync()
    {
        var activeCount = await _auctionRepository.GetActiveCountAsync();
        var pendingPayment = await _auctionRepository.GetPendingPaymentCountAsync();
        var completedCount = await _auctionRepository.GetCompletedCountAsync();
        var failedCount = await _auctionRepository.GetFailedCountAsync();

        // Get completed auctions with their highest bids and bidder information
        var completedAuctions = await _auctionRepository.GetAuctionsByStatusAsync(AuctionStatus.Completed);
        
        // Calculate top bidders based on completed auctions only
        var topBidders = completedAuctions
            .Where(a => a.HighestBidId.HasValue && a.HighestBid != null && a.HighestBid.Bidder != null)
            .GroupBy(a => a.HighestBid!.BidderId)
            .Select(g => new TopBidder
            {
                UserId = g.Key,
                Email = g.First().HighestBid!.Bidder!.Email,
                TotalBids = g.Count(),
                AuctionsWon = g.Count(),
                TotalAmountSpent = g.Sum(a => a.HighestBid!.Amount)
            })
            .OrderByDescending(b => b.AuctionsWon)
            .ThenByDescending(b => b.TotalAmountSpent)
            .Take(10)
            .ToList();

        return new DashboardResponse
        {
            ActiveCount = activeCount,
            PendingPayment = pendingPayment,
            CompletedCount = completedCount,
            FailedCount = failedCount,
            TopBidders = topBidders
        };
    }

    public async Task ProcessRetryAsync(int auctionId, int failedAttemptNumber)
    {
        _logger.LogInformation("Processing retry for Auction: {AuctionId} after attempt: {AttemptNumber}", 
            auctionId, failedAttemptNumber);

        var maxAttempts = _configuration.GetValue<int>("AuctionSettings:MaxPaymentAttempts", 
            AppConstants.MaxPaymentAttempts);

        if (failedAttemptNumber >= maxAttempts)
        {
            var auction = await _auctionRepository.GetByIdAsync(auctionId);
            if (auction != null)
            {
                auction.Status = AuctionStatus.Failed;
                await _auctionRepository.UpdateAsync(auction);
                _logger.LogInformation("Auction {AuctionId} marked as Failed after {Attempts} attempts", 
                    auctionId, maxAttempts);
            }
            return;
        }

        var bids = await _bidRepository.GetByAuctionIdAsync(auctionId);
        var sortedBids = bids.OrderByDescending(b => b.Amount).ThenByDescending(b => b.Timestamp).ToList();

        if (sortedBids.Count <= failedAttemptNumber)
        {
            var auction = await _auctionRepository.GetByIdAsync(auctionId);
            if (auction != null)
            {
                auction.Status = AuctionStatus.Failed;
                await _auctionRepository.UpdateAsync(auction);
                _logger.LogInformation("Auction {AuctionId} marked as Failed - no more bidders available", 
                    auctionId);
            }
            return;
        }

        var nextBidder = sortedBids[failedAttemptNumber];
        await CreatePaymentAttemptAsync(auctionId, nextBidder.BidderId, failedAttemptNumber + 1);
    }

    public async Task CreatePaymentAttemptAsync(int auctionId, int bidderId, int attemptNumber)
    {
        var paymentWindowMinutes = _configuration.GetValue<int>("AuctionSettings:PaymentWindowMinutes", 
            AppConstants.PaymentWindowMinutes);

        var paymentAttempt = new PaymentAttempt
        {
            AuctionId = auctionId,
            BidderId = bidderId,
            Status = PaymentStatus.Pending,
            AttemptNumber = attemptNumber,
            AttemptTime = DateTime.UtcNow,
            WindowExpiryTime = DateTime.UtcNow.AddMinutes(paymentWindowMinutes)
        };

        await _paymentRepository.CreateAsync(paymentAttempt);

        var user = await _userRepository.GetByIdAsync(bidderId);
        var auction = await _auctionRepository.GetByIdWithDetailsAsync(auctionId);
        var highestBid = await _bidRepository.GetHighestBidAsync(auctionId);

        if (user != null && auction != null && highestBid != null)
        {
            await _emailService.SendPaymentNotificationAsync(
                user.Email,
                auctionId,
                highestBid.Amount,
                paymentAttempt.WindowExpiryTime!.Value
            );
        }

        _logger.LogInformation("Payment attempt {AttemptNumber} created for Auction: {AuctionId}, Bidder: {BidderId}", 
            attemptNumber, auctionId, bidderId);
    }

    private PaymentAttemptResponse MapToPaymentAttemptResponse(PaymentAttempt attempt)
    {
        return new PaymentAttemptResponse
        {
            PaymentId = attempt.PaymentId,
            AuctionId = attempt.AuctionId,
            BidderId = attempt.BidderId,
            BidderEmail = attempt.Bidder?.Email ?? string.Empty,
            Status = attempt.Status.ToString(),
            AttemptNumber = attempt.AttemptNumber,
            AttemptTime = attempt.AttemptTime,
            WindowExpiryTime = attempt.WindowExpiryTime,
            ConfirmedAmount = attempt.ConfirmedAmount
        };
    }

    private async Task NotifyDashboardUpdate()
    {
        try
        {
            await _hubContext.Clients.Group("DashboardSubscribers")
                .SendAsync("DashboardUpdate", new { timestamp = DateTime.UtcNow });
            _logger.LogInformation("Dashboard update notification sent via SignalR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending dashboard update notification");
        }
    }
}

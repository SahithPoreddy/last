using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using codebase.Models.DTOs;
using codebase.Services.Interfaces;
using codebase.Repositories.Interfaces;

namespace codebase.Controllers;

/// <summary>
/// Controller for payment operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IAuctionRepository _auctionRepository;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentService paymentService,
        IAuctionRepository auctionRepository,
        ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _auctionRepository = auctionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Confirm payment for an auction by providing auction ID and payment amount
    /// </summary>
    /// <param name="request">Payment confirmation details including auction ID and amount</param>
    /// <returns>Payment attempt response with updated status</returns>
    [Authorize(Roles = "User")]
    [HttpPost("confirm")]
    [ProducesResponseType(typeof(PaymentAttemptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentAttemptResponse>> ConfirmPaymentByAuction(
        [FromBody] ConfirmPaymentByAuctionRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        _logger.LogInformation("Payment confirmation request for Auction: {AuctionId}, Amount: {Amount}, User: {UserId}",
            request.AuctionId, request.Amount, userId);

        // Get the auction to find the product ID
        var auction = await _auctionRepository.GetByIdAsync(request.AuctionId);
        if (auction == null)
        {
            return NotFound(new { message = "Auction not found" });
        }

        // Use the existing payment service with product ID
        var paymentRequest = new ConfirmPaymentRequest
        {
            ConfirmedAmount = request.Amount
        };

        var response = await _paymentService.ConfirmPaymentAsync(userId, auction.ProductId, paymentRequest);
        return Ok(response);
    }

    /// <summary>
    /// Get payment status for a specific auction
    /// </summary>
    /// <param name="auctionId">The auction ID</param>
    /// <returns>Current payment attempt details</returns>
    [Authorize]
    [HttpGet("auction/{auctionId}")]
    [ProducesResponseType(typeof(PaymentAttemptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentAttemptResponse>> GetPaymentStatus(int auctionId)
    {
        var paymentRepository = HttpContext.RequestServices
            .GetRequiredService<IPaymentAttemptRepository>();
        
        var currentAttempt = await paymentRepository.GetCurrentPendingAttemptAsync(auctionId);
        
        if (currentAttempt == null)
        {
            return NotFound(new { message = "No payment attempt found for this auction" });
        }

        var response = new PaymentAttemptResponse
        {
            PaymentId = currentAttempt.PaymentId,
            AuctionId = currentAttempt.AuctionId,
            BidderId = currentAttempt.BidderId,
            BidderEmail = currentAttempt.Bidder?.Email ?? string.Empty,
            Status = currentAttempt.Status.ToString(),
            AttemptNumber = currentAttempt.AttemptNumber,
            AttemptTime = currentAttempt.AttemptTime,
            WindowExpiryTime = currentAttempt.WindowExpiryTime,
            ConfirmedAmount = currentAttempt.ConfirmedAmount
        };

        return Ok(response);
    }

    /// <summary>
    /// Get all payment attempts for a specific auction (Admin only)
    /// </summary>
    /// <param name="auctionId">The auction ID</param>
    /// <returns>List of all payment attempts for the auction</returns>
    [Authorize(Roles = "Admin")]
    [HttpGet("auction/{auctionId}/history")]
    [ProducesResponseType(typeof(List<PaymentAttemptResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PaymentAttemptResponse>>> GetPaymentHistory(int auctionId)
    {
        var paymentRepository = HttpContext.RequestServices
            .GetRequiredService<IPaymentAttemptRepository>();
        
        var attempts = await paymentRepository.GetByAuctionIdAsync(auctionId);
        
        var responses = attempts.Select(attempt => new PaymentAttemptResponse
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
        }).ToList();

        return Ok(responses);
    }
}

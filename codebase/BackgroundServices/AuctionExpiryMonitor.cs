using codebase.Common;
using codebase.Models.Enums;
using codebase.Repositories.Interfaces;
using codebase.Services.Interfaces;
using codebase.Services.Implementations;
using Microsoft.AspNetCore.SignalR;
using codebase.Data;
using codebase.Hubs;

namespace codebase.BackgroundServices;

/// <summary>
/// Background service to monitor auction expiry and finalize expired auctions
/// </summary>
public class AuctionExpiryMonitor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuctionExpiryMonitor> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<DashboardHub> _hubContext;

    public AuctionExpiryMonitor(
        IServiceProvider serviceProvider,
        ILogger<AuctionExpiryMonitor> logger,
        IConfiguration configuration,
        IHubContext<DashboardHub> hubContext)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Auction Expiry Monitor started");

        var intervalSeconds = _configuration.GetValue<int>("AuctionSettings:AuctionMonitorIntervalSeconds",
            AppConstants.AuctionMonitorIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MonitorExpiredAuctionsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while monitoring expired auctions");
            }

            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Auction Expiry Monitor stopped");
    }

    private async Task MonitorExpiredAuctionsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var auctionRepository = scope.ServiceProvider.GetRequiredService<IAuctionRepository>();
        var bidRepository = scope.ServiceProvider.GetRequiredService<IBidRepository>();
        var paymentService = scope.ServiceProvider.GetRequiredService<PaymentService>();

        // Get active auctions and filter expired ones
        var activeAuctions = await auctionRepository.GetAuctionsByStatusAsync(AuctionStatus.Active);
        var expiredAuctions = activeAuctions.Where(a => a.ExpiryTime <= DateTime.UtcNow).ToList();

        foreach (var auction in expiredAuctions)
        {
            try
            {
                _logger.LogInformation("Processing expired auction: {AuctionId}", auction.AuctionId);

                var highestBid = await bidRepository.GetHighestBidAsync(auction.AuctionId);
                if (highestBid == null)
                {
                    _logger.LogWarning("No bids found for expired auction: {AuctionId}. Marking as Failed",
                        auction.AuctionId);
                    auction.Status = AuctionStatus.Failed;
                    await auctionRepository.UpdateAsync(auction);

                    // Notify dashboard
                    await NotifyDashboardUpdate();
                    continue;
                }

                auction.Status = AuctionStatus.PendingPayment;
                auction.HighestBidId = highestBid.BidId;
                await auctionRepository.UpdateAsync(auction);

                await paymentService.CreatePaymentAttemptAsync(auction.AuctionId, highestBid.BidderId, 1);

                _logger.LogInformation("Auction {AuctionId} finalized. Winner: {BidderId}, Amount: {Amount}",
                    auction.AuctionId, highestBid.BidderId, highestBid.Amount);

                // Notify dashboard
                await NotifyDashboardUpdate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing expired auction: {AuctionId}", auction.AuctionId);
            }
        }
    }

    private async Task NotifyDashboardUpdate()
    {
        try
        {
            await _hubContext.Clients.Group("DashboardSubscribers")
                .SendAsync("DashboardUpdate", new { timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending dashboard update notification");
        }
    }
}

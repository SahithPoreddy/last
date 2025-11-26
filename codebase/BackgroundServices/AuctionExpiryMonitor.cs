using codebase.Common;
using codebase.Models.Enums;
using codebase.Repositories.Interfaces;
using codebase.Services.Interfaces;
using codebase.Services.Implementations;

namespace codebase.BackgroundServices;

/// <summary>
/// Background service to monitor auction expiry and finalize expired auctions
/// </summary>
public class AuctionExpiryMonitor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuctionExpiryMonitor> _logger;
    private readonly IConfiguration _configuration;

    public AuctionExpiryMonitor(
        IServiceProvider serviceProvider,
        ILogger<AuctionExpiryMonitor> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
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
    }

    private async Task MonitorExpiredAuctionsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var auctionRepository = scope.ServiceProvider.GetRequiredService<IAuctionRepository>();
        var paymentService = scope.ServiceProvider.GetRequiredService<PaymentService>();

        var expiredAuctions = await auctionRepository.GetExpiredAuctionsAsync();

        foreach (var auction in expiredAuctions)
        {
            try
            {
                _logger.LogInformation("Processing expired auction: {AuctionId}", auction.AuctionId);

                if (auction.HighestBidId == null)
                {
                    auction.Status = AuctionStatus.Failed;
                    auction.CompletedAt = DateTime.UtcNow;
                    await auctionRepository.UpdateAsync(auction);
                    _logger.LogInformation("Auction {AuctionId} marked as Failed (no bids)", auction.AuctionId);
                }
                else
                {
                    auction.Status = AuctionStatus.PendingPayment;
                    auction.CompletedAt = DateTime.UtcNow;
                    await auctionRepository.UpdateAsync(auction);

                    await paymentService.CreatePaymentAttemptAsync(
                        auction.AuctionId,
                        auction.HighestBid!.BidderId,
                        1
                    );

                    _logger.LogInformation("Auction {AuctionId} moved to PendingPayment", auction.AuctionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing expired auction: {AuctionId}", auction.AuctionId);
            }
        }
    }
}

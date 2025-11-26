using Microsoft.AspNetCore.SignalR;
using codebase.Common;
using codebase.Hubs;
using codebase.Models.Enums;
using codebase.Repositories.Interfaces;
using codebase.Services.Interfaces;
using codebase.Services.Implementations;

namespace codebase.BackgroundServices;

/// <summary>
/// Background service that monitors payment windows and processes retries
/// </summary>
public class PaymentRetryService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentRetryService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<DashboardHub> _hubContext;

    public PaymentRetryService(
        IServiceProvider serviceProvider,
        ILogger<PaymentRetryService> logger,
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
        _logger.LogInformation("Payment Retry Service started");

        var intervalSeconds = _configuration.GetValue<int>("AuctionSettings:PaymentMonitorIntervalSeconds",
            AppConstants.PaymentMonitorIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MonitorExpiredPaymentWindowsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while monitoring payment windows");
            }

            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Payment Retry Service stopped");
    }

    private async Task MonitorExpiredPaymentWindowsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var paymentRepository = scope.ServiceProvider.GetRequiredService<IPaymentAttemptRepository>();
        var auctionRepository = scope.ServiceProvider.GetRequiredService<IAuctionRepository>();
        var paymentService = scope.ServiceProvider.GetRequiredService<PaymentService>();

        var expiredPayments = await paymentRepository.GetExpiredPendingAttemptsAsync();

        foreach (var payment in expiredPayments)
        {
            try
            {
                _logger.LogInformation("Processing expired payment window: PaymentId: {PaymentId}, AuctionId: {AuctionId}",
                    payment.PaymentId, payment.AuctionId);

                payment.Status = PaymentStatus.Failed;
                payment.CompletedAt = DateTime.UtcNow;
                await paymentRepository.UpdateAsync(payment);

                var maxAttempts = _configuration.GetValue<int>("AuctionSettings:MaxPaymentAttempts",
                    AppConstants.MaxPaymentAttempts);

                if (payment.AttemptNumber >= maxAttempts)
                {
                    var auction = await auctionRepository.GetByIdAsync(payment.AuctionId);
                    if (auction != null)
                    {
                        auction.Status = AuctionStatus.Failed;
                        await auctionRepository.UpdateAsync(auction);
                        _logger.LogInformation("Auction {AuctionId} marked as Failed after {Attempts} payment attempts",
                            payment.AuctionId, maxAttempts);
                    }
                }
                else
                {
                    await paymentService.ProcessRetryAsync(payment.AuctionId, payment.AttemptNumber);
                }

                _logger.LogInformation("Expired payment processed: PaymentId: {PaymentId}",
                    payment.PaymentId);
                
                // Notify dashboard
                await NotifyDashboardUpdate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing expired payment: PaymentId: {PaymentId}",
                    payment.PaymentId);
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

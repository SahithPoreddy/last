using codebase.Common;
using codebase.Repositories.Interfaces;
using codebase.Services.Implementations;

namespace codebase.BackgroundServices;

/// <summary>
/// Background service to handle payment retry queue
/// </summary>
public class PaymentRetryService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentRetryService> _logger;
    private readonly IConfiguration _configuration;

    public PaymentRetryService(
        IServiceProvider serviceProvider,
        ILogger<PaymentRetryService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
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
                await ProcessExpiredPaymentWindowsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing payment retries");
            }

            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
        }
    }

    private async Task ProcessExpiredPaymentWindowsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var paymentRepository = scope.ServiceProvider.GetRequiredService<IPaymentAttemptRepository>();
        var paymentService = scope.ServiceProvider.GetRequiredService<PaymentService>();

        var expiredAttempts = await paymentRepository.GetExpiredPendingAttemptsAsync();

        foreach (var attempt in expiredAttempts)
        {
            try
            {
                _logger.LogInformation("Processing expired payment window: {PaymentId} for Auction: {AuctionId}",
                    attempt.PaymentId, attempt.AuctionId);

                attempt.Status = Models.Enums.PaymentStatus.Failed;
                attempt.CompletedAt = DateTime.UtcNow;
                await paymentRepository.UpdateAsync(attempt);

                await paymentService.ProcessRetryAsync(attempt.AuctionId, attempt.AttemptNumber);

                _logger.LogInformation("Payment retry initiated for Auction: {AuctionId}", attempt.AuctionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing expired payment: {PaymentId}", attempt.PaymentId);
            }
        }
    }
}

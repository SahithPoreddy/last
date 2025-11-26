namespace codebase.Services.Interfaces;

/// <summary>
/// Service for sending email notifications
/// </summary>
public interface IEmailService
{
    Task SendPaymentNotificationAsync(string recipientEmail, int auctionId, decimal amount, DateTime expiryTime);
    Task SendAuctionWonNotificationAsync(string recipientEmail, string productName, decimal amount);
    Task SendPaymentConfirmationAsync(string recipientEmail, string productName, decimal amount);
}

using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using codebase.Services.Interfaces;

namespace codebase.Services.Implementations;

/// <summary>
/// Implementation of email service
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendPaymentNotificationAsync(
        string recipientEmail, int auctionId, decimal amount, DateTime expiryTime)
    {
        var subject = $"Payment Required - Auction #{auctionId}";
        var body = $@"
            <h2>Congratulations! You won the auction #{auctionId}</h2>
            <p>You are the current highest bidder with an amount of <strong>${amount:F2}</strong>.</p>
            <p>Please confirm your payment within <strong>1 minute</strong> before <strong>{expiryTime:yyyy-MM-dd HH:mm:ss} UTC</strong>.</p>
            <p>If payment is not confirmed, the auction will be offered to the next highest bidder.</p>
            <p>Thank you for using BidSphere!</p>
        ";

        await SendEmailAsync(recipientEmail, subject, body);
    }

    public async Task SendAuctionWonNotificationAsync(string recipientEmail, string productName, decimal amount)
    {
        var subject = $"You Won: {productName}";
        var body = $@"
            <h2>Congratulations!</h2>
            <p>You have won the auction for <strong>{productName}</strong>!</p>
            <p>Winning bid: <strong>${amount:F2}</strong></p>
            <p>Please proceed to complete your payment.</p>
            <p>Thank you for using BidSphere!</p>
        ";

        await SendEmailAsync(recipientEmail, subject, body);
    }

    public async Task SendPaymentConfirmationAsync(string recipientEmail, string productName, decimal amount)
    {
        var subject = $"Payment Confirmed - {productName}";
        var body = $@"
            <h2>Payment Confirmed!</h2>
            <p>Your payment for <strong>{productName}</strong> has been successfully confirmed.</p>
            <p>Amount: <strong>${amount:F2}</strong></p>
            <p>Thank you for your purchase!</p>
            <p>BidSphere Team</p>
        ";

        await SendEmailAsync(recipientEmail, subject, body);
    }

    private async Task SendEmailAsync(string recipientEmail, string subject, string htmlBody)
    {
        try
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var smtpHost = emailSettings["SmtpHost"];
            var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
            var senderEmail = emailSettings["SenderEmail"];
            var senderName = emailSettings["SenderName"];
            var username = emailSettings["Username"];
            var password = emailSettings["Password"];
            var enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("Email credentials not configured. Email not sent to {Email}", recipientEmail);
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress("", recipientEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {Email}", recipientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", recipientEmail);
        }
    }
}

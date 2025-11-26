namespace codebase.Common;

public static class AppConstants
{
    // Auction Settings
    public const int AntiSnipingThresholdSeconds = 60; // 1 minute
    public const int AuctionExtensionMinutes = 1;
    public const int MaxPaymentAttempts = 3;
    public const int PaymentWindowMinutes = 1;

    // Monitoring Settings
    public const int AuctionMonitorIntervalSeconds = 10;
    public const int PaymentMonitorIntervalSeconds = 5;

    // Pagination
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    // JWT Settings
    public const string JwtIssuer = "BidSphere";
    public const string JwtAudience = "BidSphere.Users";
    public const int JwtExpiryHours = 24;

    // Excel Upload
    public const int MaxExcelRows = 1000;
    public const string ExcelDateFormat = "yyyy-MM-dd";

    // Error Messages
    public const string UnauthorizedError = "Unauthorized access";
    public const string AuctionNotFoundError = "Auction not found";
    public const string ProductNotFoundError = "Product not found";
    public const string BidTooLowError = "Bid amount must be higher than current highest bid";
    public const string AuctionNotActiveError = "Auction is not active";
    public const string CannotBidOwnProductError = "Cannot bid on your own product";
    public const string ProductHasBidsError = "Cannot modify/delete product with active bids";
}

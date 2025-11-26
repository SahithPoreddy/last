using System.ComponentModel.DataAnnotations;

namespace codebase.Models.DTOs;

public class ConfirmPaymentRequest
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Confirmed amount must be greater than 0")]
    public decimal ConfirmedAmount { get; set; }
}

public class ConfirmPaymentByAuctionRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Auction ID must be valid")]
    public int AuctionId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be greater than 0")]
    public decimal Amount { get; set; }
}

public class PaymentAttemptResponse
{
    public int PaymentId { get; set; }
    public int AuctionId { get; set; }
    public int BidderId { get; set; }
    public string BidderEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int AttemptNumber { get; set; }
    public DateTime AttemptTime { get; set; }
    public DateTime? WindowExpiryTime { get; set; }
    public decimal? ConfirmedAmount { get; set; }
}

public class DashboardResponse
{
    public int ActiveCount { get; set; }
    public int PendingPayment { get; set; }
    public int CompletedCount { get; set; }
    public int FailedCount { get; set; }
    public List<TopBidder> TopBidders { get; set; } = new();
}

public class TopBidder
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public int TotalBids { get; set; }
    public int AuctionsWon { get; set; }
    public decimal TotalAmountSpent { get; set; }
}

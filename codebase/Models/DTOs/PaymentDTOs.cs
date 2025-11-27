using System.ComponentModel.DataAnnotations;

namespace codebase.Models.DTOs;

public class ConfirmPaymentRequest
{
    public decimal ConfirmedAmount { get; set; }
}

public class ConfirmPaymentByAuctionRequest
{
    public int AuctionId { get; set; }
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

using System.ComponentModel.DataAnnotations;

namespace codebase.Models.DTOs;

public class PlaceBidRequest
{
    [Required]
    public int AuctionId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Bid amount must be greater than 0")]
    public decimal Amount { get; set; }
}

public class BidResponse
{
    public int BidId { get; set; }
    public int AuctionId { get; set; }
    public int BidderId { get; set; }
    public string BidderEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
}

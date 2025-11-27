using System.ComponentModel.DataAnnotations;

namespace codebase.Models.DTOs;

public class PlaceBidRequest
{
    public int AuctionId { get; set; }
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

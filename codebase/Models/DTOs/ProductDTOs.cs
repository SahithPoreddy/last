using System.ComponentModel.DataAnnotations;

namespace codebase.Models.DTOs;

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public int AuctionDuration { get; set; }
}

public class UpdateProductRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public decimal? StartingPrice { get; set; }
    public int? AuctionDuration { get; set; }
}

public class ProductResponse
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public int AuctionDuration { get; set; }
    public int OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public AuctionResponse? Auction { get; set; }
}

public class AuctionResponse
{
    public int AuctionId { get; set; }
    public int ProductId { get; set; }
    public DateTime ExpiryTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? HighestBidAmount { get; set; }
    public int? HighestBidderId { get; set; }
    public int ExtensionCount { get; set; }
    public TimeSpan? TimeRemaining { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AuctionDetailResponse
{
    public int AuctionId { get; set; }
    public ProductResponse? Product { get; set; }
    public DateTime ExpiryTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? HighestBidAmount { get; set; }
    public int? HighestBidderId { get; set; }
    public int ExtensionCount { get; set; }
    public TimeSpan? TimeRemaining { get; set; }
    public List<BidResponse> Bids { get; set; } = new();
}

public class ExcelUploadResult
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();
}

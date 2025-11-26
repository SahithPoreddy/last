using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using codebase.Models.Enums;

namespace codebase.Models.Entities;

/// <summary>
/// Represents an auction lifecycle management
/// </summary>
public class Auction
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AuctionId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    public DateTime ExpiryTime { get; set; }

    [Required]
    public AuctionStatus Status { get; set; } = AuctionStatus.Active;

    public int? HighestBidId { get; set; }

    public int ExtensionCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ProductId))]
    public virtual Product? Product { get; set; }

    [ForeignKey(nameof(HighestBidId))]
    public virtual Bid? HighestBid { get; set; }

    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();
    public virtual ICollection<PaymentAttempt> PaymentAttempts { get; set; } = new List<PaymentAttempt>();
}

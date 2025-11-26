using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace codebase.Models.Entities;

/// <summary>
/// Represents an individual bid record
/// </summary>
public class Bid
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BidId { get; set; }

    [Required]
    public int AuctionId { get; set; }

    [Required]
    public int BidderId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Bid amount must be greater than 0")]
    public decimal Amount { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(AuctionId))]
    public virtual Auction? Auction { get; set; }

    [ForeignKey(nameof(BidderId))]
    public virtual User? Bidder { get; set; }
}

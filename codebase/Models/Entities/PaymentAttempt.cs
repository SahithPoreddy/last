using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using codebase.Models.Enums;

namespace codebase.Models.Entities;

/// <summary>
/// Represents a payment confirmation tracking record
/// </summary>
public class PaymentAttempt
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PaymentId { get; set; }

    [Required]
    public int AuctionId { get; set; }

    [Required]
    public int BidderId { get; set; }

    [Required]
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [Required]
    public int AttemptNumber { get; set; }

    public DateTime AttemptTime { get; set; } = DateTime.UtcNow;

    public DateTime? WindowExpiryTime { get; set; }

    public decimal? ConfirmedAmount { get; set; }

    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(AuctionId))]
    public virtual Auction? Auction { get; set; }

    [ForeignKey(nameof(BidderId))]
    public virtual User? Bidder { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace codebase.Models.Entities;

/// <summary>
/// Represents an auction product
/// </summary>
public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Starting price must be greater than 0")]
    public decimal StartingPrice { get; set; }

    [Required]
    [Range(2, 1440, ErrorMessage = "Auction duration must be between 2 minutes and 24 hours (1440 minutes)")]
    public int AuctionDuration { get; set; } // in minutes

    [Required]
    public int OwnerId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(OwnerId))]
    public virtual User? Owner { get; set; }

    public virtual Auction? Auction { get; set; }
}

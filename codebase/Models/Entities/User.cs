using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace codebase.Models.Entities;

/// <summary>
/// Represents a registered system user
/// </summary>
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();
    public virtual ICollection<PaymentAttempt> PaymentAttempts { get; set; } = new List<PaymentAttempt>();
}

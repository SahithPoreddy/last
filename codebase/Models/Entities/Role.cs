using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace codebase.Models.Entities;

/// <summary>
/// Represents a user role assignment
/// </summary>
public class Role
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int RoleId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string RoleName { get; set; } = string.Empty;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
}

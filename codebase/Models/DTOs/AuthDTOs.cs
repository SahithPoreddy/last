using System.ComponentModel.DataAnnotations;

namespace codebase.Models.DTOs;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Admin|User|Guest)$", ErrorMessage = "Role must be Admin, User, or Guest")]
    public string Role { get; set; } = "User";
}

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public string Token { get; set; } = string.Empty;
}

public class UserProfileResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class UpdateProfileRequest
{
    [EmailAddress]
    public string? Email { get; set; }
}

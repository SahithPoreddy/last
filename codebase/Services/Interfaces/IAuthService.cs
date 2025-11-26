using codebase.Models.DTOs;

namespace codebase.Services.Interfaces;

/// <summary>
/// Service for authentication operations
/// </summary>
public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<UserProfileResponse> GetProfileAsync(int userId);
    Task<UserProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    string GenerateJwtToken(int userId, string email, List<string> roles);
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using codebase.Common.Exceptions;
using codebase.Models.DTOs;
using codebase.Models.Entities;
using codebase.Repositories.Interfaces;
using codebase.Services.Interfaces;

namespace codebase.Services.Implementations;

/// <summary>
/// Implementation of authentication service
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        _logger.LogInformation("Registering new user: {Email} with role: {Role}", request.Email, request.Role);

        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            throw new ConflictException("Email already exists");
        }

        // Validate role
        if (!new[] { "Admin", "User", "Guest" }.Contains(request.Role))
        {
            throw new BadRequestException("Invalid role. Must be Admin, User, or Guest");
        }

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        user = await _userRepository.CreateAsync(user);

        // Create role assignment
        var role = new Role
        {
            UserId = user.UserId,
            RoleName = request.Role,
            AssignedAt = DateTime.UtcNow
        };
        await _roleRepository.CreateAsync(role);

        var roles = new List<string> { request.Role };
        var token = GenerateJwtToken(user.UserId, user.Email, roles);

        _logger.LogInformation("User registered successfully: {UserId} with role: {Role}", user.UserId, request.Role);

        return new AuthResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            Roles = roles,
            Token = token
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("User login attempt: {Email}", request.Email);

        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            throw new UnauthorizedException("Invalid email or password");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password");
        }

        var roles = await _roleRepository.GetUserRolesAsync(user.UserId);
        if (!roles.Any())
        {
            roles = new List<string> { "User" }; // Default role if none assigned
        }

        var token = GenerateJwtToken(user.UserId, user.Email, roles);

        _logger.LogInformation("User logged in successfully: {UserId}", user.UserId);

        return new AuthResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            Roles = roles,
            Token = token
        };
    }

    public async Task<UserProfileResponse> GetProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        var roles = await _roleRepository.GetUserRolesAsync(userId);

        return new UserProfileResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            Roles = roles,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
        {
            if (await _userRepository.EmailExistsAsync(request.Email))
            {
                throw new ConflictException("Email already exists");
            }
            user.Email = request.Email;
        }

        user = await _userRepository.UpdateAsync(user);

        var roles = await _roleRepository.GetUserRolesAsync(userId);

        return new UserProfileResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            Roles = roles,
            CreatedAt = user.CreatedAt
        };
    }

    public string GenerateJwtToken(int userId, string email, List<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add each role as a separate claim
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(int.Parse(jwtSettings["ExpiryHours"] ?? "24")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using codebase.Models.DTOs;
using codebase.Services.Interfaces;

namespace codebase.Controllers;

/// <summary>
/// Controller for authentication operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var response = await _authService.RegisterAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Login to the system
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [Authorize]
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileResponse>> GetProfile()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var response = await _authService.GetProfileAsync(userId);
        return Ok(response);
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    [Authorize]
    [HttpPut("profile")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileResponse>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var response = await _authService.UpdateProfileAsync(userId, request);
        return Ok(response);
    }
}

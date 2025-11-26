using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using codebase.Models.DTOs;
using codebase.Services.Interfaces;

namespace codebase.Controllers;

/// <summary>
/// Controller for dashboard analytics
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IPaymentService paymentService, ILogger<DashboardController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Get dashboard metrics and statistics (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    [ProducesResponseType(typeof(DashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DashboardResponse>> GetDashboard()
    {
        var dashboard = await _paymentService.GetDashboardMetricsAsync();
        return Ok(dashboard);
    }
}

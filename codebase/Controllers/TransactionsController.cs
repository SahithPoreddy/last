using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using codebase.Models.DTOs;
using codebase.Models.Common;
using codebase.Services.Interfaces;

namespace codebase.Controllers;

/// <summary>
/// Controller for payment transactions
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(IPaymentService paymentService, ILogger<TransactionsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Get all payment transactions with pagination (Admin/User)
    /// </summary>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PaymentAttemptResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<PaymentAttemptResponse>>> GetTransactions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var paginationParams = new PaginationParams
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _paymentService.GetTransactionsAsync(paginationParams);
        return Ok(result);
    }
}

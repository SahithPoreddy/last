using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using codebase.Models.DTOs;
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
    /// Get all payment transactions (Admin/User)
    /// </summary>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(List<PaymentAttemptResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<PaymentAttemptResponse>>> GetTransactions()
    {
        var transactions = await _paymentService.GetTransactionsAsync();
        return Ok(transactions);
    }
}

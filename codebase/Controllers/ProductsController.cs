using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using codebase.Models.DTOs;
using codebase.Models.Common;
using codebase.Services.Interfaces;

namespace codebase.Controllers;

/// <summary>
/// Controller for product and auction operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new product (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProductResponse>> CreateProduct([FromBody] CreateProductRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var response = await _productService.CreateProductAsync(userId, request);
        return CreatedAtAction(nameof(GetAuctionDetails), new { id = response.ProductId }, response);
    }

    /// <summary>
    /// Upload multiple products via Excel file (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("upload")]
    [ProducesResponseType(typeof(ExcelUploadResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ExcelUploadResult>> UploadProducts(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only .xlsx files are supported");
        }

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        using var stream = file.OpenReadStream();
        var result = await _productService.UploadProductsFromExcelAsync(userId, stream);

        return Ok(result);
    }

    /// <summary>
    /// Get all products with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductResponse>>> GetAllProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var paginationParams = new PaginationParams
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        
        var result = await _productService.GetAllProductsAsync(paginationParams);
        return Ok(result);
    }

    /// <summary>
    /// Get all active auctions with pagination
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(PagedResult<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductResponse>>> GetActiveAuctions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var paginationParams = new PaginationParams
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        
        var result = await _productService.GetActiveAuctionsAsync(paginationParams);
        return Ok(result);
    }

    /// <summary>
    /// Get auction details by product ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AuctionDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuctionDetailResponse>> GetAuctionDetails(int id)
    {
        var auction = await _productService.GetAuctionDetailsAsync(id);
        return Ok(auction);
    }

    /// <summary>
    /// Update a product (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        var response = await _productService.UpdateProductAsync(id, request);
        return Ok(response);
    }

    /// <summary>
    /// Force finalize an auction (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/finalize")]
    [ProducesResponseType(typeof(AuctionDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuctionDetailResponse>> FinalizeAuction(int id)
    {
        var response = await _productService.FinalizeAuctionAsync(id);
        return Ok(response);
    }

    /// <summary>
    /// Delete a product (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        await _productService.DeleteProductAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Confirm payment for won auction (User only)
    /// </summary>
    [Authorize(Roles = "User")]
    [HttpPut("{id}/confirm")]
    [ProducesResponseType(typeof(PaymentAttemptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentAttemptResponse>> ConfirmPayment(
        int id, 
        [FromBody] ConfirmPaymentRequest request,
        [FromHeader(Name = "X-Test-Instant-Fail")] bool testInstantFail = false)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var paymentService = HttpContext.RequestServices.GetRequiredService<IPaymentService>();
        var response = await paymentService.ConfirmPaymentAsync(userId, id, request, testInstantFail);
        return Ok(response);
    }
}

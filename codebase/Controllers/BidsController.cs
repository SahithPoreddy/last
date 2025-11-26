using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using codebase.Models.DTOs;
using codebase.Services.Interfaces;

namespace codebase.Controllers;

/// <summary>
/// Controller for bid operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BidsController : ControllerBase
{
    private readonly IBidService _bidService;
    private readonly ILogger<BidsController> _logger;

    public BidsController(IBidService bidService, ILogger<BidsController> logger)
    {
        _bidService = bidService;
        _logger = logger;
    }

    /// <summary>
    /// Place a bid on an active auction (User only)
    /// </summary>
    [Authorize(Roles = "User")]
    [HttpPost]
    [ProducesResponseType(typeof(BidResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BidResponse>> PlaceBid([FromBody] PlaceBidRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var response = await _bidService.PlaceBidAsync(userId, request);
        return CreatedAtAction(nameof(GetBidsByAuction), new { auctionId = response.AuctionId }, response);
    }

    /// <summary>
    /// Get all bids for a specific auction
    /// </summary>
    [HttpGet("{auctionId}")]
    [ProducesResponseType(typeof(List<BidResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BidResponse>>> GetBidsByAuction(int auctionId)
    {
        var bids = await _bidService.GetBidsByAuctionAsync(auctionId);
        return Ok(bids);
    }

    /// <summary>
    /// Get all bids with optional filters (Admin/User)
    /// </summary>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(List<BidResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<BidResponse>>> GetAllBids()
    {
        var bids = await _bidService.GetAllBidsAsync();
        return Ok(bids);
    }
}

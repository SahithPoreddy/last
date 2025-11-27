using codebase.Models.DTOs;
using codebase.Models.Common;

namespace codebase.Services.Interfaces;

/// <summary>
/// Service for bid operations
/// </summary>
public interface IBidService
{
    Task<BidResponse> PlaceBidAsync(int bidderId, PlaceBidRequest request);
    Task<List<BidResponse>> GetBidsByAuctionAsync(int auctionId);
    Task<PagedResult<BidResponse>> GetAllBidsAsync(PaginationParams paginationParams);
}

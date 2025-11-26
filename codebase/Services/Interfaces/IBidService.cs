using codebase.Models.DTOs;

namespace codebase.Services.Interfaces;

/// <summary>
/// Service for bid operations
/// </summary>
public interface IBidService
{
    Task<BidResponse> PlaceBidAsync(int bidderId, PlaceBidRequest request);
    Task<List<BidResponse>> GetBidsByAuctionAsync(int auctionId);
    Task<List<BidResponse>> GetAllBidsAsync();
}

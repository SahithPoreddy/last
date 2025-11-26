using codebase.Models.Entities;
using codebase.Models.Enums;

namespace codebase.Repositories.Interfaces;

public interface IAuctionRepository
{
    Task<Auction?> GetByIdAsync(int auctionId);
    Task<Auction?> GetByIdWithDetailsAsync(int auctionId);
    Task<Auction?> GetByProductIdAsync(int productId);
    Task<List<Auction>> GetActiveAuctionsAsync();
    Task<List<Auction>> GetExpiredAuctionsAsync();
    Task<List<Auction>> GetAuctionsByStatusAsync(AuctionStatus status);
    Task<Auction> CreateAsync(Auction auction);
    Task<Auction> UpdateAsync(Auction auction);
    Task<int> GetActiveCountAsync();
    Task<int> GetPendingPaymentCountAsync();
    Task<int> GetCompletedCountAsync();
    Task<int> GetFailedCountAsync();
}

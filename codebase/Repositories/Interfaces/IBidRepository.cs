using codebase.Models.Entities;

namespace codebase.Repositories.Interfaces;

public interface IBidRepository
{
    Task<Bid?> GetByIdAsync(int bidId);
    Task<List<Bid>> GetByAuctionIdAsync(int auctionId);
    Task<List<Bid>> GetByUserIdAsync(int userId);
    Task<Bid?> GetHighestBidAsync(int auctionId);
    Task<List<Bid>> GetAllBidsAsync();
    Task<Bid> CreateAsync(Bid bid);
}

using Microsoft.EntityFrameworkCore;
using codebase.Data;
using codebase.Models.Entities;
using codebase.Repositories.Interfaces;

namespace codebase.Repositories.Implementations;

public class BidRepository : IBidRepository
{
    private readonly ApplicationDbContext _context;

    public BidRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Bid?> GetByIdAsync(int bidId)
    {
        return await _context.Bids.FindAsync(bidId);
    }

    public async Task<List<Bid>> GetByAuctionIdAsync(int auctionId)
    {
        return await _context.Bids
            .Include(b => b.Bidder)
            .Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.Timestamp)
            .ToListAsync();
    }

    public async Task<List<Bid>> GetByUserIdAsync(int userId)
    {
        return await _context.Bids
            .Include(b => b.Auction)
                .ThenInclude(a => a.Product)
            .Where(b => b.BidderId == userId)
            .OrderByDescending(b => b.Timestamp)
            .ToListAsync();
    }

    public async Task<Bid?> GetHighestBidAsync(int auctionId)
    {
        return await _context.Bids
            .Include(b => b.Bidder)
            .Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.Amount)
            .ThenByDescending(b => b.Timestamp)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Bid>> GetAllBidsAsync()
    {
        return await _context.Bids
            .Include(b => b.Bidder)
            .Include(b => b.Auction)
                .ThenInclude(a => a.Product)
            .OrderByDescending(b => b.Timestamp)
            .ToListAsync();
    }

    public async Task<Bid> CreateAsync(Bid bid)
    {
        await _context.Bids.AddAsync(bid);
        await _context.SaveChangesAsync();
        return bid;
    }
}

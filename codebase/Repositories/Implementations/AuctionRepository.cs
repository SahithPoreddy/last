using Microsoft.EntityFrameworkCore;
using codebase.Data;
using codebase.Models.Entities;
using codebase.Models.Enums;
using codebase.Repositories.Interfaces;

namespace codebase.Repositories.Implementations;

public class AuctionRepository : IAuctionRepository
{
    private readonly ApplicationDbContext _context;

    public AuctionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Auction?> GetByIdAsync(int auctionId)
    {
        return await _context.Auctions.FindAsync(auctionId);
    }

    public async Task<Auction?> GetByIdWithDetailsAsync(int auctionId)
    {
        return await _context.Auctions
            .Include(a => a.Product)
            .Include(a => a.HighestBid)
            .Include(a => a.Bids)
                .ThenInclude(b => b.Bidder)
            .FirstOrDefaultAsync(a => a.AuctionId == auctionId);
    }

    public async Task<Auction?> GetByProductIdAsync(int productId)
    {
        return await _context.Auctions
            .Include(a => a.HighestBid)
            .FirstOrDefaultAsync(a => a.ProductId == productId);
    }

    public async Task<List<Auction>> GetActiveAuctionsAsync()
    {
        return await _context.Auctions
            .Include(a => a.Product)
            .Include(a => a.HighestBid)
            .Where(a => a.Status == AuctionStatus.Active)
            .ToListAsync();
    }

    public async Task<List<Auction>> GetExpiredAuctionsAsync()
    {
        return await _context.Auctions
            .Include(a => a.HighestBid)
            .Where(a => a.Status == AuctionStatus.Active && a.ExpiryTime <= DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<List<Auction>> GetAuctionsByStatusAsync(AuctionStatus status)
    {
        return await _context.Auctions
            .Include(a => a.Product)
            .Include(a => a.HighestBid)
                .ThenInclude(b => b.Bidder)
            .Where(a => a.Status == status)
            .ToListAsync();
    }

    public async Task<Auction> CreateAsync(Auction auction)
    {
        await _context.Auctions.AddAsync(auction);
        await _context.SaveChangesAsync();
        return auction;
    }

    public async Task<Auction> UpdateAsync(Auction auction)
    {
        _context.Auctions.Update(auction);
        await _context.SaveChangesAsync();
        return auction;
    }

    public async Task<int> GetActiveCountAsync()
    {
        return await _context.Auctions.CountAsync(a => a.Status == AuctionStatus.Active);
    }

    public async Task<int> GetPendingPaymentCountAsync()
    {
        return await _context.Auctions.CountAsync(a => a.Status == AuctionStatus.PendingPayment);
    }

    public async Task<int> GetCompletedCountAsync()
    {
        return await _context.Auctions.CountAsync(a => a.Status == AuctionStatus.Completed);
    }

    public async Task<int> GetFailedCountAsync()
    {
        return await _context.Auctions.CountAsync(a => a.Status == AuctionStatus.Failed);
    }
}

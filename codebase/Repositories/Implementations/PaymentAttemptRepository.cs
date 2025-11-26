using Microsoft.EntityFrameworkCore;
using codebase.Data;
using codebase.Models.Entities;
using codebase.Models.Enums;
using codebase.Repositories.Interfaces;

namespace codebase.Repositories.Implementations;

public class PaymentAttemptRepository : IPaymentAttemptRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentAttemptRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentAttempt?> GetByIdAsync(int paymentId)
    {
        return await _context.PaymentAttempts.FindAsync(paymentId);
    }

    public async Task<List<PaymentAttempt>> GetByAuctionIdAsync(int auctionId)
    {
        return await _context.PaymentAttempts
            .Include(p => p.Bidder)
            .Where(p => p.AuctionId == auctionId)
            .OrderBy(p => p.AttemptNumber)
            .ToListAsync();
    }

    public async Task<PaymentAttempt?> GetCurrentPendingAttemptAsync(int auctionId)
    {
        return await _context.PaymentAttempts
            .Include(p => p.Bidder)
            .Where(p => p.AuctionId == auctionId && p.Status == PaymentStatus.Pending)
            .OrderByDescending(p => p.AttemptNumber)
            .FirstOrDefaultAsync();
    }

    public async Task<List<PaymentAttempt>> GetExpiredPendingAttemptsAsync()
    {
        return await _context.PaymentAttempts
            .Include(p => p.Auction)
            .Include(p => p.Bidder)
            .Where(p => p.Status == PaymentStatus.Pending 
                     && p.WindowExpiryTime.HasValue 
                     && p.WindowExpiryTime.Value <= DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<PaymentAttempt> CreateAsync(PaymentAttempt paymentAttempt)
    {
        await _context.PaymentAttempts.AddAsync(paymentAttempt);
        await _context.SaveChangesAsync();
        return paymentAttempt;
    }

    public async Task<PaymentAttempt> UpdateAsync(PaymentAttempt paymentAttempt)
    {
        _context.PaymentAttempts.Update(paymentAttempt);
        await _context.SaveChangesAsync();
        return paymentAttempt;
    }

    public async Task<List<PaymentAttempt>> GetAllAsync()
    {
        return await _context.PaymentAttempts
            .Include(p => p.Bidder)
            .Include(p => p.Auction)
                .ThenInclude(a => a.Product)
            .OrderByDescending(p => p.AttemptTime)
            .ToListAsync();
    }
}

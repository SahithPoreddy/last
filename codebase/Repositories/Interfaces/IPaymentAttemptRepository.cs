using codebase.Models.Entities;
using codebase.Models.Enums;

namespace codebase.Repositories.Interfaces;

public interface IPaymentAttemptRepository
{
    Task<PaymentAttempt?> GetByIdAsync(int paymentId);
    Task<List<PaymentAttempt>> GetByAuctionIdAsync(int auctionId);
    Task<PaymentAttempt?> GetCurrentPendingAttemptAsync(int auctionId);
    Task<List<PaymentAttempt>> GetExpiredPendingAttemptsAsync();
    Task<PaymentAttempt> CreateAsync(PaymentAttempt paymentAttempt);
    Task<PaymentAttempt> UpdateAsync(PaymentAttempt paymentAttempt);
    Task<List<PaymentAttempt>> GetAllAsync();
}

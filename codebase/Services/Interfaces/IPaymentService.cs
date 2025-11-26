using codebase.Models.DTOs;

namespace codebase.Services.Interfaces;

/// <summary>
/// Service for payment and dashboard operations
/// </summary>
public interface IPaymentService
{
    Task<PaymentAttemptResponse> ConfirmPaymentAsync(int userId, int productId, ConfirmPaymentRequest request, bool testInstantFail = false);
    Task<List<PaymentAttemptResponse>> GetTransactionsAsync();
    Task<DashboardResponse> GetDashboardMetricsAsync();
}

using codebase.Models.DTOs;
using codebase.Models.Common;

namespace codebase.Services.Interfaces;

/// <summary>
/// Service for payment and dashboard operations
/// </summary>
public interface IPaymentService
{
    Task<PaymentAttemptResponse> ConfirmPaymentAsync(int userId, int productId, ConfirmPaymentRequest request, bool testInstantFail = false);
    Task<PagedResult<PaymentAttemptResponse>> GetTransactionsAsync(PaginationParams paginationParams);
    Task<DashboardResponse> GetDashboardMetricsAsync();
}

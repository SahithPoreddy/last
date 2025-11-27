using codebase.Models.DTOs;
using codebase.Models.Common;

namespace codebase.Services.Interfaces;

/// <summary>
/// Service for product and auction operations
/// </summary>
public interface IProductService
{
    Task<ProductResponse> CreateProductAsync(int ownerId, CreateProductRequest request);
    Task<PagedResult<ProductResponse>> GetAllProductsAsync(PaginationParams paginationParams);
    Task<PagedResult<ProductResponse>> GetActiveAuctionsAsync(PaginationParams paginationParams);
    Task<AuctionDetailResponse> GetAuctionDetailsAsync(int productId);
    Task<ProductResponse> UpdateProductAsync(int productId, UpdateProductRequest request);
    Task DeleteProductAsync(int productId);
    Task<AuctionDetailResponse> FinalizeAuctionAsync(int productId);
    Task<ExcelUploadResult> UploadProductsFromExcelAsync(int ownerId, Stream fileStream);
}

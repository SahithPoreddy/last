using codebase.Models.DTOs;

namespace codebase.Services.Interfaces;

/// <summary>
/// Service for product and auction operations
/// </summary>
public interface IProductService
{
    Task<ProductResponse> CreateProductAsync(int ownerId, CreateProductRequest request);
    Task<List<ProductResponse>> GetAllProductsAsync();
    Task<List<ProductResponse>> GetActiveAuctionsAsync();
    Task<AuctionDetailResponse> GetAuctionDetailsAsync(int productId);
    Task<ProductResponse> UpdateProductAsync(int productId, UpdateProductRequest request);
    Task DeleteProductAsync(int productId);
    Task<AuctionDetailResponse> FinalizeAuctionAsync(int productId);
    Task<ExcelUploadResult> UploadProductsFromExcelAsync(int ownerId, Stream fileStream);
}

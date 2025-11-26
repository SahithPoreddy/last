using codebase.Models.Entities;
using codebase.Models.Enums;

namespace codebase.Repositories.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int productId);
    Task<Product?> GetByIdWithAuctionAsync(int productId);
    Task<List<Product>> GetAllAsync();
    Task<List<Product>> GetAllWithAuctionsAsync();
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task DeleteAsync(Product product);
    Task<bool> HasActiveBidsAsync(int productId);
}

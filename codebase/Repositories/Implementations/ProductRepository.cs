using Microsoft.EntityFrameworkCore;
using codebase.Data;
using codebase.Models.Entities;
using codebase.Repositories.Interfaces;

namespace codebase.Repositories.Implementations;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int productId)
    {
        return await _context.Products.FindAsync(productId);
    }

    public async Task<Product?> GetByIdWithAuctionAsync(int productId)
    {
        return await _context.Products
            .Include(p => p.Auction)
                .ThenInclude(a => a!.HighestBid)
            .FirstOrDefaultAsync(p => p.ProductId == productId);
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<List<Product>> GetAllWithAuctionsAsync()
    {
        return await _context.Products
            .Include(p => p.Auction)
                .ThenInclude(a => a!.HighestBid)
            .ToListAsync();
    }

    public async Task<Product> CreateAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task DeleteAsync(Product product)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasActiveBidsAsync(int productId)
    {
        return await _context.Auctions
            .AnyAsync(a => a.ProductId == productId && a.Bids.Any());
    }
}

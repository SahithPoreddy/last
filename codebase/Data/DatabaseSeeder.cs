using Microsoft.EntityFrameworkCore;
using codebase.Data;
using codebase.Models.Entities;
using codebase.Models.Enums;

namespace codebase.Data;

/// <summary>
/// Database seeder for initial data
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Check if data already exists
        if (await context.Users.AnyAsync())
        {
            return; // Database already seeded
        }

        // Create Admin User
        var admin = new User
        {
            Email = "admin@bidsphere.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            CreatedAt = DateTime.UtcNow
        };
        await context.Users.AddAsync(admin);
        await context.SaveChangesAsync();

        // Assign Admin role
        await context.Roles.AddAsync(new Role
        {
            UserId = admin.UserId,
            RoleName = "Admin",
            AssignedAt = DateTime.UtcNow
        });

        // Create Regular Users
        var user1 = new User
        {
            Email = "user1@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
            CreatedAt = DateTime.UtcNow
        };
        await context.Users.AddAsync(user1);
        await context.SaveChangesAsync();

        await context.Roles.AddAsync(new Role
        {
            UserId = user1.UserId,
            RoleName = "User",
            AssignedAt = DateTime.UtcNow
        });

        var user2 = new User
        {
            Email = "user2@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
            CreatedAt = DateTime.UtcNow
        };
        await context.Users.AddAsync(user2);
        await context.SaveChangesAsync();

        await context.Roles.AddAsync(new Role
        {
            UserId = user2.UserId,
            RoleName = "User",
            AssignedAt = DateTime.UtcNow
        });

        var guest = new User
        {
            Email = "guest@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Guest@123"),
            CreatedAt = DateTime.UtcNow
        };
        await context.Users.AddAsync(guest);
        await context.SaveChangesAsync();

        await context.Roles.AddAsync(new Role
        {
            UserId = guest.UserId,
            RoleName = "Guest",
            AssignedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        // Create Sample Products
        var products = new[]
        {
            new Product
            {
                Name = "Vintage Rolex Watch",
                Description = "Authentic vintage Rolex watch from 1960s in excellent condition",
                Category = "Collectibles",
                StartingPrice = 5000.00m,
                AuctionDuration = 60,
                OwnerId = admin.UserId,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "MacBook Pro 16-inch",
                Description = "Brand new MacBook Pro 16-inch with M3 Max chip, 32GB RAM, 1TB SSD",
                Category = "Electronics",
                StartingPrice = 2500.00m,
                AuctionDuration = 120,
                OwnerId = admin.UserId,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Original Oil Painting",
                Description = "Beautiful landscape oil painting by renowned local artist",
                Category = "Art",
                StartingPrice = 1200.00m,
                AuctionDuration = 180,
                OwnerId = admin.UserId,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "Louis Vuitton Handbag",
                Description = "Authentic Louis Vuitton designer handbag, limited edition",
                Category = "Fashion",
                StartingPrice = 800.00m,
                AuctionDuration = 90,
                OwnerId = admin.UserId,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Name = "First Edition Harry Potter Book",
                Description = "First edition Harry Potter and the Philosopher's Stone, signed",
                Category = "Books",
                StartingPrice = 3000.00m,
                AuctionDuration = 240,
                OwnerId = admin.UserId,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        // Create Auctions for each product
        foreach (var product in products)
        {
            var auction = new Auction
            {
                ProductId = product.ProductId,
                ExpiryTime = DateTime.UtcNow.AddMinutes(product.AuctionDuration),
                Status = AuctionStatus.Active,
                ExtensionCount = 0,
                CreatedAt = DateTime.UtcNow
            };
            await context.Auctions.AddAsync(auction);
        }

        await context.SaveChangesAsync();
    }
}

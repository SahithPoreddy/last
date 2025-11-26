using Microsoft.EntityFrameworkCore;
using codebase.Models.Entities;

namespace codebase.Data;

/// <summary>
/// Database context for BidSphere application
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Auction> Auctions { get; set; }
    public DbSet<Bid> Bids { get; set; }
    public DbSet<PaymentAttempt> PaymentAttempts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.UserId);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        });

        // Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.RoleId);
            entity.Property(e => e.RoleName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AssignedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Roles)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.RoleName }).IsUnique();
        });

        // Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.ProductId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.StartingPrice).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.AuctionDuration).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Owner)
                  .WithMany(u => u.Products)
                  .HasForeignKey(e => e.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Auction)
                  .WithOne(a => a.Product)
                  .HasForeignKey<Auction>(a => a.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Auction entity
        modelBuilder.Entity<Auction>(entity =>
        {
            entity.ToTable("auctions");
            entity.HasKey(e => e.AuctionId);
            entity.Property(e => e.ExpiryTime).IsRequired();
            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.Property(e => e.ExtensionCount).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

            entity.HasMany(e => e.Bids)
                  .WithOne(b => b.Auction)
                  .HasForeignKey(b => b.AuctionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.PaymentAttempts)
                  .WithOne(p => p.Auction)
                  .HasForeignKey(p => p.AuctionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Bid entity
        modelBuilder.Entity<Bid>(entity =>
        {
            entity.ToTable("bids");
            entity.HasKey(e => e.BidId);
            entity.Property(e => e.Amount).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.Timestamp).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Bidder)
                  .WithMany(u => u.Bids)
                  .HasForeignKey(e => e.BidderId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.AuctionId, e.Timestamp });
        });

        // PaymentAttempt entity
        modelBuilder.Entity<PaymentAttempt>(entity =>
        {
            entity.ToTable("payment_attempts");
            entity.HasKey(e => e.PaymentId);
            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.Property(e => e.AttemptNumber).IsRequired();
            entity.Property(e => e.AttemptTime).HasDefaultValueSql("NOW()");
            entity.Property(e => e.ConfirmedAmount).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Bidder)
                  .WithMany(u => u.PaymentAttempts)
                  .HasForeignKey(e => e.BidderId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.AuctionId, e.AttemptNumber });
        });
    }
}

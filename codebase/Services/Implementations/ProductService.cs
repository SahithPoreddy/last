using OfficeOpenXml;
using codebase.Common;
using codebase.Common.Exceptions;
using codebase.Models.DTOs;
using codebase.Models.Entities;
using codebase.Models.Enums;
using codebase.Models.Common;
using codebase.Repositories.Interfaces;
using codebase.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using codebase.Hubs;

namespace codebase.Services.Implementations;

/// <summary>
/// Implementation of product service
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IAuctionRepository _auctionRepository;
    private readonly IBidRepository _bidRepository;
    private readonly ILogger<ProductService> _logger;
    private readonly IHubContext<DashboardHub> _hubContext;

    public ProductService(
        IProductRepository productRepository,
        IAuctionRepository auctionRepository,
        IBidRepository bidRepository,
        ILogger<ProductService> logger,
        IHubContext<DashboardHub> hubContext)
    {
        _productRepository = productRepository;
        _auctionRepository = auctionRepository;
        _bidRepository = bidRepository;
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task<ProductResponse> CreateProductAsync(int ownerId, CreateProductRequest request)
    {
        _logger.LogInformation("Creating product: {ProductName} by Owner: {OwnerId}", request.Name, ownerId);

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            StartingPrice = request.StartingPrice,
            AuctionDuration = request.AuctionDuration,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow
        };

        product = await _productRepository.CreateAsync(product);

        var expiryTime = DateTime.UtcNow.AddMinutes(request.AuctionDuration);
        var auction = new Auction
        {
            ProductId = product.ProductId,
            ExpiryTime = expiryTime,
            Status = AuctionStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        auction = await _auctionRepository.CreateAsync(auction);

        _logger.LogInformation("Product created successfully: {ProductId} with Auction: {AuctionId}", 
            product.ProductId, auction.AuctionId);

        // Notify dashboard via SignalR
        await NotifyDashboardUpdate();

        return MapToProductResponse(product, auction);
    }

    public async Task<PagedResult<ProductResponse>> GetAllProductsAsync(PaginationParams paginationParams)
    {
        var products = await _productRepository.GetAllWithAuctionsAsync();
        
        var totalCount = products.Count;
        var items = products
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .Select(p => MapToProductResponse(p, p.Auction))
            .ToList();

        return new PagedResult<ProductResponse>
        {
            Items = items,
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<ProductResponse>> GetActiveAuctionsAsync(PaginationParams paginationParams)
    {
        var auctions = await _auctionRepository.GetActiveAuctionsAsync();
        
        var totalCount = auctions.Count;
        var items = auctions
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .Select(a => MapToProductResponse(a.Product!, a))
            .ToList();

        return new PagedResult<ProductResponse>
        {
            Items = items,
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<AuctionDetailResponse> GetAuctionDetailsAsync(int productId)
    {
        var product = await _productRepository.GetByIdWithAuctionAsync(productId);
        if (product == null)
        {
            throw new NotFoundException(AppConstants.ProductNotFoundError);
        }

        if (product.Auction == null)
        {
            throw new NotFoundException(AppConstants.AuctionNotFoundError);
        }

        var bids = await _bidRepository.GetByAuctionIdAsync(product.Auction.AuctionId);

        return new AuctionDetailResponse
        {
            AuctionId = product.Auction.AuctionId,
            Product = MapToProductResponse(product, null),
            ExpiryTime = product.Auction.ExpiryTime,
            Status = product.Auction.Status.ToString(),
            HighestBidAmount = product.Auction.HighestBid?.Amount,
            HighestBidderId = product.Auction.HighestBid?.BidderId,
            ExtensionCount = product.Auction.ExtensionCount,
            TimeRemaining = product.Auction.Status == AuctionStatus.Active && product.Auction.ExpiryTime > DateTime.UtcNow
                ? product.Auction.ExpiryTime - DateTime.UtcNow
                : null,
            Bids = bids.Select(b => new BidResponse
            {
                BidId = b.BidId,
                AuctionId = b.AuctionId,
                BidderId = b.BidderId,
                BidderEmail = b.Bidder?.Email ?? string.Empty,
                Amount = b.Amount,
                Timestamp = b.Timestamp
            }).ToList()
        };
    }

    public async Task<ProductResponse> UpdateProductAsync(int productId, UpdateProductRequest request)
    {
        var product = await _productRepository.GetByIdWithAuctionAsync(productId);
        if (product == null)
        {
            throw new NotFoundException(AppConstants.ProductNotFoundError);
        }

        if (await _productRepository.HasActiveBidsAsync(productId))
        {
            throw new BadRequestException(AppConstants.ProductHasBidsError);
        }

        if (!string.IsNullOrEmpty(request.Name))
            product.Name = request.Name;

        if (!string.IsNullOrEmpty(request.Description))
            product.Description = request.Description;

        if (!string.IsNullOrEmpty(request.Category))
            product.Category = request.Category;

        if (request.StartingPrice.HasValue)
            product.StartingPrice = request.StartingPrice.Value;

        if (request.AuctionDuration.HasValue && product.Auction != null)
        {
            product.AuctionDuration = request.AuctionDuration.Value;
            product.Auction.ExpiryTime = DateTime.UtcNow.AddMinutes(request.AuctionDuration.Value);
            await _auctionRepository.UpdateAsync(product.Auction);
        }

        product = await _productRepository.UpdateAsync(product);

        _logger.LogInformation("Product updated successfully: {ProductId}", productId);

        // Notify dashboard via SignalR
        await NotifyDashboardUpdate();

        return MapToProductResponse(product, product.Auction);
    }

    public async Task DeleteProductAsync(int productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw new NotFoundException(AppConstants.ProductNotFoundError);
        }

        if (await _productRepository.HasActiveBidsAsync(productId))
        {
            throw new BadRequestException(AppConstants.ProductHasBidsError);
        }

        await _productRepository.DeleteAsync(product);

        _logger.LogInformation("Product deleted successfully: {ProductId}", productId);

        // Notify dashboard via SignalR
        await NotifyDashboardUpdate();
    }

    public async Task<AuctionDetailResponse> FinalizeAuctionAsync(int productId)
    {
        var product = await _productRepository.GetByIdWithAuctionAsync(productId);
        if (product == null || product.Auction == null)
        {
            throw new NotFoundException(AppConstants.AuctionNotFoundError);
        }

        var auction = product.Auction;
        if (auction.Status != AuctionStatus.Active)
        {
            throw new BadRequestException("Auction is not active");
        }

        if (auction.HighestBidId == null)
        {
            auction.Status = AuctionStatus.Failed;
            auction.CompletedAt = DateTime.UtcNow;
        }
        else
        {
            auction.Status = AuctionStatus.PendingPayment;
            auction.CompletedAt = DateTime.UtcNow;
        }

        await _auctionRepository.UpdateAsync(auction);

        _logger.LogInformation("Auction finalized: {AuctionId} with status: {Status}", 
            auction.AuctionId, auction.Status);

        // Notify dashboard via SignalR
        await NotifyDashboardUpdate();

        return await GetAuctionDetailsAsync(productId);
    }

    public async Task<ExcelUploadResult> UploadProductsFromExcelAsync(int ownerId, Stream fileStream)
    {
        _logger.LogInformation("Starting Excel upload for Owner: {OwnerId}", ownerId);

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var result = new ExcelUploadResult();

        using var package = new ExcelPackage(fileStream);
        var worksheet = package.Workbook.Worksheets.FirstOrDefault();

        if (worksheet == null)
        {
            throw new BadRequestException("No worksheet found in Excel file");
        }

        var rowCount = worksheet.Dimension?.Rows ?? 0;
        if (rowCount <= 1)
        {
            throw new BadRequestException("Excel file is empty or has no data rows");
        }

        for (int row = 2; row <= rowCount; row++)
        {
            try
            {
                var name = worksheet.Cells[row, 2].Text;
                var startingPriceText = worksheet.Cells[row, 3].Text;
                var description = worksheet.Cells[row, 4].Text;
                var category = worksheet.Cells[row, 5].Text;
                var durationText = worksheet.Cells[row, 6].Text;

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(startingPriceText) ||
                    string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(category) ||
                    string.IsNullOrWhiteSpace(durationText))
                {
                    result.Errors.Add($"Row {row}: Missing required fields");
                    result.FailureCount++;
                    continue;
                }

                if (!decimal.TryParse(startingPriceText, out var startingPrice) || startingPrice <= 0)
                {
                    result.Errors.Add($"Row {row}: Invalid starting price");
                    result.FailureCount++;
                    continue;
                }

                if (!int.TryParse(durationText, out var duration) || duration < 2 || duration > 1440)
                {
                    result.Errors.Add($"Row {row}: Invalid auction duration (must be 2-1440 minutes)");
                    result.FailureCount++;
                    continue;
                }

                var request = new CreateProductRequest
                {
                    Name = name,
                    Description = description,
                    Category = category,
                    StartingPrice = startingPrice,
                    AuctionDuration = duration
                };

                await CreateProductAsync(ownerId, request);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {row}: {ex.Message}");
                result.FailureCount++;
            }
        }

        _logger.LogInformation("Excel upload completed. Success: {Success}, Failed: {Failed}", 
            result.SuccessCount, result.FailureCount);

        // Notify dashboard via SignalR after bulk upload
        if (result.SuccessCount > 0)
        {
            await NotifyDashboardUpdate();
        }

        return result;
    }

    private ProductResponse MapToProductResponse(Product product, Auction? auction)
    {
        return new ProductResponse
        {
            ProductId = product.ProductId,
            Name = product.Name,
            Description = product.Description,
            Category = product.Category,
            StartingPrice = product.StartingPrice,
            AuctionDuration = product.AuctionDuration,
            OwnerId = product.OwnerId,
            CreatedAt = product.CreatedAt,
            Auction = auction == null ? null : new AuctionResponse
            {
                AuctionId = auction.AuctionId,
                ProductId = auction.ProductId,
                ExpiryTime = auction.ExpiryTime,
                Status = auction.Status.ToString(),
                HighestBidAmount = auction.HighestBid?.Amount,
                HighestBidderId = auction.HighestBid?.BidderId,
                ExtensionCount = auction.ExtensionCount,
                TimeRemaining = auction.Status == AuctionStatus.Active && auction.ExpiryTime > DateTime.UtcNow
                    ? auction.ExpiryTime - DateTime.UtcNow
                    : null,
                CreatedAt = auction.CreatedAt
            }
        };
    }

    private async Task NotifyDashboardUpdate()
    {
        try
        {
            await _hubContext.Clients.Group("DashboardSubscribers")
                .SendAsync("DashboardUpdate", new { timestamp = DateTime.UtcNow });
            _logger.LogInformation("Dashboard update notification sent via SignalR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending dashboard update notification");
        }
    }
}

using codebase.Common;
using codebase.Common.Exceptions;
using codebase.Models.DTOs;
using codebase.Models.Entities;
using codebase.Models.Enums;
using codebase.Models.Common;
using codebase.Repositories.Interfaces;
using codebase.Services.Interfaces;

namespace codebase.Services.Implementations;

/// <summary>
/// Implementation of bid service
/// </summary>
public class BidService : IBidService
{
    private readonly IBidRepository _bidRepository;
    private readonly IAuctionRepository _auctionRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<BidService> _logger;
    private readonly IConfiguration _configuration;

    public BidService(
        IBidRepository bidRepository,
        IAuctionRepository auctionRepository,
        IProductRepository productRepository,
        ILogger<BidService> logger,
        IConfiguration configuration)
    {
        _bidRepository = bidRepository;
        _auctionRepository = auctionRepository;
        _productRepository = productRepository;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<BidResponse> PlaceBidAsync(int bidderId, PlaceBidRequest request)
    {
        _logger.LogInformation("Placing bid on Auction: {AuctionId} by Bidder: {BidderId}", 
            request.AuctionId, bidderId);

        var auction = await _auctionRepository.GetByIdWithDetailsAsync(request.AuctionId);
        if (auction == null)
        {
            throw new NotFoundException(AppConstants.AuctionNotFoundError);
        }

        if (auction.Status != AuctionStatus.Active)
        {
            throw new BadRequestException(AppConstants.AuctionNotActiveError);
        }

        if (auction.ExpiryTime <= DateTime.UtcNow)
        {
            throw new BadRequestException("Auction has already expired");
        }

        if (auction.Product?.OwnerId == bidderId)
        {
            throw new BadRequestException(AppConstants.CannotBidOwnProductError);
        }

        var highestBid = await _bidRepository.GetHighestBidAsync(request.AuctionId);
        var minimumBid = highestBid?.Amount ?? auction.Product!.StartingPrice;

        if (request.Amount <= minimumBid)
        {
            throw new BadRequestException(
                $"Bid amount must be greater than current highest bid of {minimumBid}");
        }

        var bid = new Bid
        {
            AuctionId = request.AuctionId,
            BidderId = bidderId,
            Amount = request.Amount,
            Timestamp = DateTime.UtcNow
        };

        bid = await _bidRepository.CreateAsync(bid);

        auction.HighestBidId = bid.BidId;

        var antiSnipingThreshold = _configuration.GetValue<int>("AuctionSettings:AntiSnipingThresholdSeconds", 
            AppConstants.AntiSnipingThresholdSeconds);
        var extensionMinutes = _configuration.GetValue<int>("AuctionSettings:AuctionExtensionMinutes", 
            AppConstants.AuctionExtensionMinutes);

        var timeRemaining = (auction.ExpiryTime - DateTime.UtcNow).TotalSeconds;
        if (timeRemaining < antiSnipingThreshold)
        {
            auction.ExpiryTime = auction.ExpiryTime.AddMinutes(extensionMinutes);
            auction.ExtensionCount++;
            _logger.LogInformation("Auction {AuctionId} extended by {Minutes} minutes. Extension count: {Count}", 
                auction.AuctionId, extensionMinutes, auction.ExtensionCount);
        }

        await _auctionRepository.UpdateAsync(auction);

        _logger.LogInformation("Bid placed successfully: {BidId} on Auction: {AuctionId}", 
            bid.BidId, auction.AuctionId);

        return new BidResponse
        {
            BidId = bid.BidId,
            AuctionId = bid.AuctionId,
            BidderId = bid.BidderId,
            BidderEmail = string.Empty,
            Amount = bid.Amount,
            Timestamp = bid.Timestamp
        };
    }

    public async Task<List<BidResponse>> GetBidsByAuctionAsync(int auctionId)
    {
        var bids = await _bidRepository.GetByAuctionIdAsync(auctionId);

        return bids.Select(b => new BidResponse
        {
            BidId = b.BidId,
            AuctionId = b.AuctionId,
            BidderId = b.BidderId,
            BidderEmail = b.Bidder?.Email ?? string.Empty,
            Amount = b.Amount,
            Timestamp = b.Timestamp
        }).ToList();
    }

    public async Task<PagedResult<BidResponse>> GetAllBidsAsync(PaginationParams paginationParams)
    {
        var bids = await _bidRepository.GetAllBidsAsync();
        
        var totalCount = bids.Count;
        var items = bids
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .Select(b => new BidResponse
            {
                BidId = b.BidId,
                AuctionId = b.AuctionId,
                BidderId = b.BidderId,
                BidderEmail = b.Bidder?.Email ?? string.Empty,
                Amount = b.Amount,
                Timestamp = b.Timestamp
            }).ToList();

        return new PagedResult<BidResponse>
        {
            Items = items,
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize,
            TotalCount = totalCount
        };
    }
}

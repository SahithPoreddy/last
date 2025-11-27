using FluentValidation;
using codebase.Models.DTOs;

namespace codebase.Validators;

/// <summary>
/// Validator for place bid requests
/// </summary>
public class PlaceBidRequestValidator : AbstractValidator<PlaceBidRequest>
{
    public PlaceBidRequestValidator()
    {
        RuleFor(x => x.AuctionId)
            .GreaterThan(0).WithMessage("Auction ID must be greater than 0");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Bid amount must be greater than 0")
            .ScalePrecision(2, 18).WithMessage("Bid amount must have at most 2 decimal places");
    }
}

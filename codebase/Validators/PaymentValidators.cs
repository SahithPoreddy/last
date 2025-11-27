using FluentValidation;
using codebase.Models.DTOs;

namespace codebase.Validators;

/// <summary>
/// Validator for payment confirmation requests
/// </summary>
public class ConfirmPaymentRequestValidator : AbstractValidator<ConfirmPaymentRequest>
{
    public ConfirmPaymentRequestValidator()
    {
        RuleFor(x => x.ConfirmedAmount)
            .GreaterThan(0).WithMessage("Confirmed amount must be greater than 0")
            .ScalePrecision(2, 18).WithMessage("Confirmed amount must have at most 2 decimal places");
    }
}

/// <summary>
/// Validator for payment confirmation by auction ID requests
/// </summary>
public class ConfirmPaymentByAuctionRequestValidator : AbstractValidator<ConfirmPaymentByAuctionRequest>
{
    public ConfirmPaymentByAuctionRequestValidator()
    {
        RuleFor(x => x.AuctionId)
            .GreaterThan(0).WithMessage("Auction ID must be greater than 0");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than 0")
            .ScalePrecision(2, 18).WithMessage("Payment amount must have at most 2 decimal places");
    }
}

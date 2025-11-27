using FluentValidation;
using codebase.Models.DTOs;

namespace codebase.Validators;

/// <summary>
/// Validator for product creation requests
/// </summary>
public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(255).WithMessage("Product name cannot exceed 255 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Product description is required")
            .MaximumLength(2000).WithMessage("Product description cannot exceed 2000 characters");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Product category is required")
            .MaximumLength(100).WithMessage("Product category cannot exceed 100 characters");

        RuleFor(x => x.StartingPrice)
            .GreaterThan(0).WithMessage("Starting price must be greater than 0")
            .ScalePrecision(2, 18).WithMessage("Starting price must have at most 2 decimal places");

        RuleFor(x => x.AuctionDuration)
            .InclusiveBetween(2, 1440)
            .WithMessage("Auction duration must be between 2 minutes and 24 hours (1440 minutes)");
    }
}

/// <summary>
/// Validator for product update requests
/// </summary>
public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        When(x => x.Name != null, () =>
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name cannot be empty")
                .MaximumLength(255).WithMessage("Product name cannot exceed 255 characters");
        });

        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Product description cannot be empty")
                .MaximumLength(2000).WithMessage("Product description cannot exceed 2000 characters");
        });

        When(x => x.Category != null, () =>
        {
            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Product category cannot be empty")
                .MaximumLength(100).WithMessage("Product category cannot exceed 100 characters");
        });

        When(x => x.StartingPrice.HasValue, () =>
        {
            RuleFor(x => x.StartingPrice!.Value)
                .GreaterThan(0).WithMessage("Starting price must be greater than 0")
                .ScalePrecision(2, 18).WithMessage("Starting price must have at most 2 decimal places");
        });

        When(x => x.AuctionDuration.HasValue, () =>
        {
            RuleFor(x => x.AuctionDuration!.Value)
                .InclusiveBetween(2, 1440)
                .WithMessage("Auction duration must be between 2 minutes and 24 hours (1440 minutes)");
        });
    }
}

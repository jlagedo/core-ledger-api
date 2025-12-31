using CoreLedger.Application.UseCases.Transactions.Commands;
using FluentValidation;

namespace CoreLedger.Application.Validators;

/// <summary>
///     Validator for CreateTransactionCommand.
/// </summary>
public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.FundId)
            .GreaterThan(0)
            .WithMessage("FundId must be a valid positive identifier");

        RuleFor(x => x.TransactionSubTypeId)
            .GreaterThan(0)
            .WithMessage("TransactionSubTypeId must be a valid positive identifier");

        RuleFor(x => x.StatusId)
            .GreaterThan(0)
            .WithMessage("StatusId must be a valid positive identifier");

        RuleFor(x => x.TradeDate)
            .LessThanOrEqualTo(x => x.SettleDate)
            .WithMessage("Trade date must be on or before settle date");

        RuleFor(x => x.SettleDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddYears(1))
            .WithMessage("Settle date cannot be more than 1 year in the future");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price cannot be negative")
            .Must(price => Math.Abs(price) <= 9999999999.99999999m)
            .WithMessage("Price exceeds maximum precision of decimal(18,8)");

        RuleFor(x => x.Quantity)
            .Must(quantity => Math.Abs(quantity) <= 9999999999.99999999m)
            .WithMessage("Quantity exceeds maximum precision of decimal(18,8)");

        RuleFor(x => x.Amount)
            .Must(amount => Math.Abs(amount) <= 9999999999999999.99m)
            .WithMessage("Amount exceeds maximum precision of decimal(18,2)");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency must be exactly 3 characters")
            .Matches("^[A-Z]{3}$")
            .WithMessage("Currency must be 3 uppercase letters (ISO code)");
    }
}
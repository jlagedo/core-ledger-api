using System.Text.RegularExpressions;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Entities;

/// <summary>
///     Transaction domain entity representing trade transactions with business rules and invariants.
/// </summary>
public class Transaction : BaseEntity
{
    private Transaction()
    {
    }

    public int FundId { get; private set; }
    public Fund? Fund { get; private set; }
    public int? SecurityId { get; private set; }
    public Security? Security { get; private set; }
    public int TransactionSubTypeId { get; private set; }
    public TransactionSubType? TransactionSubType { get; private set; }
    public DateTime TradeDate { get; private set; }
    public DateTime SettleDate { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal Price { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public int StatusId { get; private set; }
    public TransactionStatus? Status { get; private set; }

    /// <summary>
    ///     Identifier of the user who created this transaction.
    /// </summary>
    public string CreatedByUserId { get; private set; } = string.Empty;

    /// <summary>
    ///     Factory method to create a new Transaction with validation.
    /// </summary>
    public static Transaction Create(
        int fundId,
        int? securityId,
        int transactionSubTypeId,
        DateTime tradeDate,
        DateTime settleDate,
        decimal quantity,
        decimal price,
        decimal amount,
        string currency,
        int statusId,
        string createdByUserId)
    {
        ValidateFundId(fundId);
        ValidateTransactionSubTypeId(transactionSubTypeId);
        ValidateStatusId(statusId);
        ValidateDates(tradeDate, settleDate);
        ValidateQuantity(quantity);
        ValidatePrice(price);
        ValidateAmount(amount);
        ValidateCurrency(currency);
        ValidateCreatedByUserId(createdByUserId);

        return new Transaction
        {
            FundId = fundId,
            SecurityId = securityId,
            TransactionSubTypeId = transactionSubTypeId,
            TradeDate = DateTime.SpecifyKind(tradeDate.Date, DateTimeKind.Utc),
            SettleDate = DateTime.SpecifyKind(settleDate.Date, DateTimeKind.Utc),
            Quantity = quantity,
            Price = price,
            Amount = amount,
            Currency = currency.Trim().ToUpperInvariant(),
            StatusId = statusId,
            CreatedByUserId = createdByUserId.Trim()
        };
    }

    /// <summary>
    ///     Updates the transaction with validation.
    /// </summary>
    public void Update(
        int fundId,
        int? securityId,
        int transactionSubTypeId,
        DateTime tradeDate,
        DateTime settleDate,
        decimal quantity,
        decimal price,
        decimal amount,
        string currency,
        int statusId)
    {
        ValidateFundId(fundId);
        ValidateTransactionSubTypeId(transactionSubTypeId);
        ValidateStatusId(statusId);
        ValidateDates(tradeDate, settleDate);
        ValidateQuantity(quantity);
        ValidatePrice(price);
        ValidateAmount(amount);
        ValidateCurrency(currency);

        FundId = fundId;
        SecurityId = securityId;
        TransactionSubTypeId = transactionSubTypeId;
        TradeDate = DateTime.SpecifyKind(tradeDate.Date, DateTimeKind.Utc);
        SettleDate = DateTime.SpecifyKind(settleDate.Date, DateTimeKind.Utc);
        Quantity = quantity;
        Price = price;
        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
        StatusId = statusId;
        SetUpdated();
    }

    private static void ValidateFundId(int fundId)
    {
        if (fundId <= 0)
            throw new DomainValidationException("FundId must be a positive number");
    }

    private static void ValidateTransactionSubTypeId(int transactionSubTypeId)
    {
        if (transactionSubTypeId <= 0)
            throw new DomainValidationException("TransactionSubTypeId must be a positive number");
    }

    private static void ValidateStatusId(int statusId)
    {
        if (statusId <= 0)
            throw new DomainValidationException("StatusId must be a positive number");
    }

    private static void ValidateDates(DateTime tradeDate, DateTime settleDate)
    {
        if (tradeDate > settleDate)
            throw new DomainValidationException("Trade date must be on or before settle date");

        if (settleDate > DateTime.UtcNow.AddYears(1))
            throw new DomainValidationException("Settle date cannot be more than 1 year in the future");
    }

    private static void ValidatePrice(decimal price)
    {
        if (price < 0)
            throw new DomainValidationException("Price cannot be negative");

        if (Math.Abs(price) > 9999999999.99999999m)
            throw new DomainValidationException("Price exceeds maximum allowed value of 9,999,999,999.99999999");
    }

    private static void ValidateQuantity(decimal quantity)
    {
        if (Math.Abs(quantity) > 9999999999.99999999m)
            throw new DomainValidationException("Quantity exceeds maximum allowed value of 9,999,999,999.99999999");
    }

    private static void ValidateAmount(decimal amount)
    {
        if (Math.Abs(amount) > 9999999999999999.99m)
            throw new DomainValidationException("Amount exceeds maximum allowed value of 9,999,999,999,999,999.99");
    }

    private static void ValidateCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainValidationException("Currency cannot be empty");

        if (currency.Length != 3)
            throw new DomainValidationException("Currency must be a 3-letter ISO code");

        if (!Regex.IsMatch(currency, "^[A-Z]{3}$", RegexOptions.IgnoreCase))
            throw new DomainValidationException("Currency must contain only letters (A-Z)");
    }

    private static void ValidateCreatedByUserId(string createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(createdByUserId))
            throw new DomainValidationException("CreatedByUserId cannot be empty");
    }
}
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.UnitTests.Domain.Entities;

/// <summary>
/// Unit tests for Transaction domain entity business rules and invariants.
/// </summary>
public class TransactionTests
{
    #region Create Tests - Happy Path

    [Fact]
    public void Create_WithValidData_ShouldCreateTransaction()
    {
        // Arrange
        var fundId = 1;
        int? securityId = 10;
        var transactionSubTypeId = 5;
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);
        var quantity = 100.5m;
        var price = 50.25m;
        var amount = 5050.125m;
        var currency = "USD";
        var statusId = 1;

        // Act
        var transaction = Transaction.Create(
            fundId, securityId, transactionSubTypeId,
            tradeDate, settleDate, quantity, price, amount,
            currency, statusId);

        // Assert
        Assert.NotNull(transaction);
        Assert.Equal(fundId, transaction.FundId);
        Assert.Equal(securityId, transaction.SecurityId);
        Assert.Equal(transactionSubTypeId, transaction.TransactionSubTypeId);
        Assert.Equal(tradeDate, transaction.TradeDate);
        Assert.Equal(settleDate, transaction.SettleDate);
        Assert.Equal(quantity, transaction.Quantity);
        Assert.Equal(price, transaction.Price);
        Assert.Equal(amount, transaction.Amount);
        Assert.Equal(currency, transaction.Currency);
        Assert.Equal(statusId, transaction.StatusId);
        Assert.True((DateTime.UtcNow - transaction.CreatedAt).TotalSeconds < 1);
        Assert.Null(transaction.UpdatedAt);
    }

    [Fact]
    public void Create_WithNullSecurityId_ShouldSucceed()
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);

        // Act
        var transaction = Transaction.Create(
            fundId: 1, securityId: null, transactionSubTypeId: 5,
            tradeDate, settleDate,
            quantity: 100m, price: 50m, amount: 5000m,
            currency: "USD", statusId: 1);

        // Assert
        Assert.NotNull(transaction);
        Assert.Null(transaction.SecurityId);
    }

    [Fact]
    public void Create_WithSameTradeDateAndSettleDate_ShouldSucceed()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;

        // Act
        var transaction = Transaction.Create(
            fundId: 1, securityId: 10, transactionSubTypeId: 5,
            tradeDate: date, settleDate: date,
            quantity: 100m, price: 50m, amount: 5000m,
            currency: "EUR", statusId: 1);

        // Assert
        Assert.NotNull(transaction);
        Assert.Equal(date, transaction.TradeDate);
        Assert.Equal(date, transaction.SettleDate);
    }

    [Fact]
    public void Create_WithZeroPrice_ShouldSucceed()
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);

        // Act
        var transaction = Transaction.Create(
            fundId: 1, securityId: 10, transactionSubTypeId: 5,
            tradeDate, settleDate,
            quantity: 100m, price: 0m, amount: 0m,
            currency: "GBP", statusId: 1);

        // Assert
        Assert.NotNull(transaction);
        Assert.Equal(0m, transaction.Price);
    }

    #endregion

    #region Create Tests - FundId Validation

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithInvalidFundId_ShouldThrowDomainValidationException(int fundId)
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Transaction.Create(
                fundId, securityId: 10, transactionSubTypeId: 5,
                tradeDate, settleDate,
                quantity: 100m, price: 50m, amount: 5000m,
                currency: "USD", statusId: 1));
        Assert.Equal("FundId must be a positive number", exception.Message);
    }

    #endregion

    #region Create Tests - TransactionSubTypeId Validation

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithInvalidTransactionSubTypeId_ShouldThrowDomainValidationException(int transactionSubTypeId)
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Transaction.Create(
                fundId: 1, securityId: 10, transactionSubTypeId,
                tradeDate, settleDate,
                quantity: 100m, price: 50m, amount: 5000m,
                currency: "USD", statusId: 1));
        Assert.Equal("TransactionSubTypeId must be a positive number", exception.Message);
    }

    #endregion

    #region Create Tests - StatusId Validation

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithInvalidStatusId_ShouldThrowDomainValidationException(int statusId)
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Transaction.Create(
                fundId: 1, securityId: 10, transactionSubTypeId: 5,
                tradeDate, settleDate,
                quantity: 100m, price: 50m, amount: 5000m,
                currency: "USD", statusId));
        Assert.Equal("StatusId must be a positive number", exception.Message);
    }

    #endregion

    #region Create Tests - Date Validation

    [Fact]
    public void Create_WithTradeDateAfterSettleDate_ShouldThrowDomainValidationException()
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(-1);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Transaction.Create(
                fundId: 1, securityId: 10, transactionSubTypeId: 5,
                tradeDate, settleDate,
                quantity: 100m, price: 50m, amount: 5000m,
                currency: "USD", statusId: 1));
        Assert.Equal("Trade date must be on or before settle date", exception.Message);
    }

    [Fact]
    public void Create_WithSettleDateMoreThanOneYearInFuture_ShouldThrowDomainValidationException()
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddYears(1).AddDays(1);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Transaction.Create(
                fundId: 1, securityId: 10, transactionSubTypeId: 5,
                tradeDate, settleDate,
                quantity: 100m, price: 50m, amount: 5000m,
                currency: "USD", statusId: 1));
        Assert.Equal("Settle date cannot be more than 1 year in the future", exception.Message);
    }

    [Fact]
    public void Create_WithSettleDateExactlyOneYearInFuture_ShouldSucceed()
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddYears(1);

        // Act
        var transaction = Transaction.Create(
            fundId: 1, securityId: 10, transactionSubTypeId: 5,
            tradeDate, settleDate,
            quantity: 100m, price: 50m, amount: 5000m,
            currency: "USD", statusId: 1);

        // Assert
        Assert.NotNull(transaction);
        Assert.Equal(settleDate, transaction.SettleDate);
    }

    #endregion

    #region Create Tests - Price Validation

    [Fact]
    public void Create_WithNegativePrice_ShouldThrowDomainValidationException()
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Transaction.Create(
                fundId: 1, securityId: 10, transactionSubTypeId: 5,
                tradeDate, settleDate,
                quantity: 100m, price: -0.01m, amount: 5000m,
                currency: "USD", statusId: 1));
        Assert.Equal("Price cannot be negative", exception.Message);
    }

    #endregion

    #region Create Tests - Currency Validation

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyCurrency_ShouldThrowDomainValidationException(string? currency)
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Transaction.Create(
                fundId: 1, securityId: 10, transactionSubTypeId: 5,
                tradeDate, settleDate,
                quantity: 100m, price: 50m, amount: 5000m,
                currency: currency!, statusId: 1));
        Assert.Equal("Currency cannot be empty", exception.Message);
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("U")]
    [InlineData("ABCDE")]
    public void Create_WithInvalidCurrencyLength_ShouldThrowDomainValidationException(string currency)
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Transaction.Create(
                fundId: 1, securityId: 10, transactionSubTypeId: 5,
                tradeDate, settleDate,
                quantity: 100m, price: 50m, amount: 5000m,
                currency, statusId: 1));
        Assert.Equal("Currency must be a 3-letter ISO code", exception.Message);
    }

    [Theory]
    [InlineData("US$")]
    [InlineData("123")]
    [InlineData("AB1")]
    public void Create_WithInvalidCurrencyFormat_ShouldThrowDomainValidationException(string currency)
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Transaction.Create(
                fundId: 1, securityId: 10, transactionSubTypeId: 5,
                tradeDate, settleDate,
                quantity: 100m, price: 50m, amount: 5000m,
                currency, statusId: 1));
        Assert.Equal("Currency must contain only letters (A-Z)", exception.Message);
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    [InlineData("JPY")]
    [InlineData("BRL")]
    public void Create_WithValidCurrencyCodes_ShouldSucceed(string currency)
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);

        // Act
        var transaction = Transaction.Create(
            fundId: 1, securityId: 10, transactionSubTypeId: 5,
            tradeDate, settleDate,
            quantity: 100m, price: 50m, amount: 5000m,
            currency, statusId: 1);

        // Assert
        Assert.NotNull(transaction);
        Assert.Equal(currency, transaction.Currency);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidData_ShouldUpdateAndSetUpdatedAt()
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);
        var transaction = Transaction.Create(
            fundId: 1, securityId: 10, transactionSubTypeId: 5,
            tradeDate, settleDate,
            quantity: 100m, price: 50m, amount: 5000m,
            currency: "USD", statusId: 1);

        var originalCreatedAt = transaction.CreatedAt;
        var newFundId = 2;
        var newSecurityId = 20;
        var newSubTypeId = 6;
        var newTradeDate = tradeDate.AddDays(1);
        var newSettleDate = settleDate.AddDays(1);

        // Act
        transaction.Update(
            newFundId, newSecurityId, newSubTypeId,
            newTradeDate, newSettleDate,
            quantity: 200m, price: 75m, amount: 15000m,
            currency: "EUR", statusId: 2);

        // Assert
        Assert.Equal(newFundId, transaction.FundId);
        Assert.Equal(newSecurityId, transaction.SecurityId);
        Assert.Equal(newSubTypeId, transaction.TransactionSubTypeId);
        Assert.Equal(newTradeDate, transaction.TradeDate);
        Assert.Equal(newSettleDate, transaction.SettleDate);
        Assert.Equal(200m, transaction.Quantity);
        Assert.Equal(75m, transaction.Price);
        Assert.Equal(15000m, transaction.Amount);
        Assert.Equal("EUR", transaction.Currency);
        Assert.Equal(2, transaction.StatusId);
        Assert.NotNull(transaction.UpdatedAt);
        Assert.True((DateTime.UtcNow - transaction.UpdatedAt.Value).TotalSeconds < 1);
        Assert.Equal(originalCreatedAt, transaction.CreatedAt);
    }

    [Fact]
    public void Update_WithInvalidData_ShouldThrowDomainValidationException()
    {
        // Arrange
        var tradeDate = DateTime.UtcNow.Date;
        var settleDate = tradeDate.AddDays(2);
        var transaction = Transaction.Create(
            fundId: 1, securityId: 10, transactionSubTypeId: 5,
            tradeDate, settleDate,
            quantity: 100m, price: 50m, amount: 5000m,
            currency: "USD", statusId: 1);

        // Act & Assert - Invalid FundId
        var exception = Assert.Throws<DomainValidationException>(() =>
            transaction.Update(
                fundId: 0, securityId: 10, transactionSubTypeId: 5,
                tradeDate, settleDate,
                quantity: 100m, price: 50m, amount: 5000m,
                currency: "USD", statusId: 1));
        Assert.Equal("FundId must be a positive number", exception.Message);
    }

    #endregion
}

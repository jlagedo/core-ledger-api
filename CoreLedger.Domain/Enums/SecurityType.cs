namespace CoreLedger.Domain.Enums;

/// <summary>
/// Represents the type of a security based on fund accounting standards.
/// </summary>
public enum SecurityType
{
    Equity = 1,          // Stock
    Bond = 2,            // Fixed Income
    Cash = 3,
    MoneyMarket = 4,
    MutualFund = 5,
    ETF = 6,             // Exchange Traded Fund
    REIT = 7,            // Real Estate Investment Trust
    Derivative = 8,
    Hybrid = 9,
    Future = 10,
    OptionOnEquity = 11,
    OptionOnFuture = 12,
    Forward = 13,
    Fund = 14,
    Receipt = 15,
    FX = 16,             // Foreign Exchange
    Commodity = 17,
    Index = 18
}

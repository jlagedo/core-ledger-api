namespace CoreLedger.Application.Models;

/// <summary>
/// Query parameters for RFC-8040 compliant GET operations.
/// </summary>
public class QueryParameters
{
    /// <summary>
    /// Maximum number of items to return (hard limit: 100).
    /// </summary>
    private int _limit = 100;
    
    /// <summary>
    /// Number of items to return.
    /// </summary>
    public int Limit 
    { 
        get => _limit;
        set => _limit = Math.Min(value, 100);
    }
    
    /// <summary>
    /// Number of items to skip (for pagination).
    /// </summary>
    public int Offset { get; set; } = 0;
    
    /// <summary>
    /// Field to sort by.
    /// </summary>
    public string? SortBy { get; set; }
    
    /// <summary>
    /// Sort direction (asc or desc).
    /// </summary>
    public string SortDirection { get; set; } = "asc";
    
    /// <summary>
    /// Filter expression (simple field=value format).
    /// </summary>
    public string? Filter { get; set; }
    
    /// <summary>
    /// Fields to include in response (comma-separated).
    /// </summary>
    public string? Fields { get; set; }
}

/// <summary>
/// Paged result wrapper for RFC-8040 compliance.
/// </summary>
/// <typeparam name="T">Type of items in the result</typeparam>
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Limit,
    int Offset
);

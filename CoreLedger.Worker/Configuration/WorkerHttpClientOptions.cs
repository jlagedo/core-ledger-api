namespace CoreLedger.Worker.Configuration;

/// <summary>
/// Configuration options for HTTP client used by Worker to call API endpoints.
/// Supports mock JWT authentication for development (Client Credentials Flow simulation).
/// </summary>
public class WorkerHttpClientOptions
{
    /// <summary>
    /// Base URL of the Core Ledger API.
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://localhost:7109";

    /// <summary>
    /// HTTP client timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Mock JWT token for development authentication.
    /// Used to simulate Client Credentials Flow without Auth0 in development.
    /// </summary>
    public string MockJwtToken { get; set; } = string.Empty;

    /// <summary>
    /// User-Agent header value for HTTP requests.
    /// </summary>
    public string UserAgent { get; set; } = "CoreLedgerWorker/1.0";
}

namespace core_ledger_api.Models;

public record ErrorResponse(
    string ErrorCode,
    string Message,
    string? CorrelationId,
    Dictionary<string, string[]>? Errors = null,
    string? TraceId = null
);

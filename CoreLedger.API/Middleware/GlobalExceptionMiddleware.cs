using CoreLedger.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace CoreLedger.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString();
        var traceId = context.TraceIdentifier;

        _logger.LogError(exception, 
            "Unhandled exception occurred. CorrelationId: {CorrelationId}, TraceId: {TraceId}",
            correlationId, traceId);

        var (statusCode, errorCode, message, errors) = exception switch
        {
            EntityNotFoundException notFound => 
                ((HttpStatusCode)HttpStatusCode.NotFound, notFound.ErrorCode, notFound.Message, (Dictionary<string, string[]>?)null),
            
            DomainValidationException validation => 
                ((HttpStatusCode)HttpStatusCode.BadRequest, validation.ErrorCode, validation.Message, (Dictionary<string, string[]>?)null),
            
            DomainException domain => 
                ((HttpStatusCode)HttpStatusCode.BadRequest, domain.ErrorCode, domain.Message, (Dictionary<string, string[]>?)null),
            
            _ => ((HttpStatusCode)HttpStatusCode.InternalServerError, "ERR-INTERNAL-001", 
                  "An internal server error occurred", (Dictionary<string, string[]>?)null)
        };

        var response = new ErrorResponse(
            ErrorCode: errorCode,
            Message: _environment.IsDevelopment() ? message : GetSafeMessage(exception),
            CorrelationId: correlationId,
            Errors: errors,
            TraceId: _environment.IsDevelopment() ? traceId : null
        );

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }

    private static string GetSafeMessage(Exception exception)
    {
        return exception is DomainException 
            ? exception.Message 
            : "An error occurred while processing your request. Please contact support with the correlation ID.";
    }
}

public record ErrorResponse(
    string ErrorCode,
    string Message,
    string? CorrelationId,
    Dictionary<string, string[]>? Errors = null,
    string? TraceId = null
);

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}

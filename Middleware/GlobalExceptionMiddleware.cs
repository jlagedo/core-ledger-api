using core_ledger_api.Exceptions;
using core_ledger_api.Models;
using System.Net;
using System.Text.Json;

namespace core_ledger_api.Middleware;

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
            NotFoundException notFound => 
                (HttpStatusCode.NotFound, notFound.ErrorCode, notFound.Message, null),
            
            ValidationException validation => 
                (HttpStatusCode.BadRequest, validation.ErrorCode, validation.Message, validation.Errors),
            
            ConcurrencyException concurrency => 
                (HttpStatusCode.Conflict, concurrency.ErrorCode, concurrency.Message, null),
            
            DomainException domain => 
                (HttpStatusCode.BadRequest, domain.ErrorCode, domain.Message, null),
            
            _ => (HttpStatusCode.InternalServerError, "ERR-INTERNAL-001", 
                  "An internal server error occurred", null)
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

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}

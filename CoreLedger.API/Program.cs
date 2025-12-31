using CoreLedger.Application;
using CoreLedger.Application.Configuration;
using CoreLedger.Infrastructure;
using CoreLedger.API.Middleware;
using CoreLedger.API.Extensions;
using Serilog;
using Serilog.Events;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

// Build configuration to read Serilog settings before creating logger
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "CoreLedgerApi")
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("Starting Core Ledger API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Configure pagination options
    builder.Services.Configure<PaginationOptions>(builder.Configuration.GetSection("Pagination"));
    var paginationOptions = builder.Configuration.GetSection("Pagination").Get<PaginationOptions>() ?? new PaginationOptions();
    PaginationDefaults.Initialize(paginationOptions);

    builder.Services.AddControllers();
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();
    
    builder.Services.AddSwaggerDocumentation();
    
    if(!builder.Environment.IsDevelopment())
        builder.Services.AddAuth0Authentication(builder.Configuration);
    else
        builder.Services.AddDevelopmentAuthentication(builder.Configuration);

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddHealthChecks()
        .AddNpgSql(connectionString ?? throw new InvalidOperationException("DefaultConnection not configured"))
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseSecurityHeaders();
    app.UseGlobalExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.Use(async (ctx, next) =>
        {
            var auth = ctx.Request.Headers["Authorization"].FirstOrDefault();
            Console.WriteLine($"Raw Authorization header: {auth}");
            await next();
        });
    }

    // Authentication must come before correlation ID middleware to ensure user claims are available
    //app.UseAuthentication();
    app.UseCorrelationId();

    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress);

            // Add authenticated user information to request logs (only 'sub' claim available in access token)
            var userId = httpContext.User.FindFirst("sub")?.Value;
            var isAuthenticated = httpContext.User.Identity?.IsAuthenticated ?? false;

            diagnosticContext.Set("UserId", userId ?? "anonymous");
            diagnosticContext.Set("IsAuthenticated", isAuthenticated);
        };
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerDocumentation();
    }

    app.UseAuthorization();
    
    app.MapControllers();
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready");
    app.MapHealthChecks("/health/live");

    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStarted.Register(() =>
    {
        var addresses = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()?.Addresses;
        if (addresses != null && addresses.Any())
        {
            Log.Information("Core Ledger API is now listening on:");
            foreach (var address in addresses)
            {
                Log.Information("  → {Address}", address);
            }
        }
    });

    Log.Information("Core Ledger API started successfully");


    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Partial Program class to expose entry point for integration tests.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public partial class Program { }

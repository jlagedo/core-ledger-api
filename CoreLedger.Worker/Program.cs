using CoreLedger.Application;
using CoreLedger.Infrastructure;
using CoreLedger.Infrastructure.Configuration;
using CoreLedger.Worker.Configuration;
using CoreLedger.Worker.Services;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

// Build configuration to read Serilog settings before creating logger
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "CoreLedgerWorker")
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("Starting Core Ledger Worker");

    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog(Log.Logger);

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Configure options
    builder.Services.Configure<RabbitMQOptions>(builder.Configuration.GetSection("RabbitMQ"));
    builder.Services.Configure<TestConnectionOptions>(builder.Configuration.GetSection("TestConnection"));

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddHealthChecks()
        .AddNpgSql(connectionString ?? throw new InvalidOperationException("DefaultConnection not configured"))
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

    builder.Services.AddHostedService<B3ImportConsumer>();
    builder.Services.AddHostedService<TestConnectionConsumer>();

    var host = builder.Build();

    Log.Information("Core Ledger Worker started successfully");

    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

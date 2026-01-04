using CoreLedger.Application;
using CoreLedger.Infrastructure;
using CoreLedger.Infrastructure.Configuration;
using CoreLedger.Worker.Configuration;
using CoreLedger.Worker.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;

// Build configuration to read Serilog settings before creating logger
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", true,
        true)
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
    builder.Services.Configure<OutboxProcessorOptions>(builder.Configuration.GetSection("OutboxProcessor"));
    builder.Services.Configure<QueueNamesOptions>(builder.Configuration.GetSection("QueueNames"));
    builder.Services.Configure<WorkerHttpClientOptions>(builder.Configuration.GetSection("WorkerHttpClient"));

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddHealthChecks()
        .AddNpgSql(connectionString ?? throw new InvalidOperationException("DefaultConnection not configured"))
        .AddCheck("self", () => HealthCheckResult.Healthy());

    // Configure HttpClient for Worker -> API communication
    var workerHttpClientOptions = builder.Configuration.GetSection("WorkerHttpClient")
        .Get<WorkerHttpClientOptions>() ?? new WorkerHttpClientOptions();

    builder.Services.AddHttpClient("WorkerHttpClient", client =>
    {
        client.BaseAddress = new Uri(workerHttpClientOptions.ApiBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(workerHttpClientOptions.TimeoutSeconds);
        client.DefaultRequestHeaders.Add("User-Agent", workerHttpClientOptions.UserAgent);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {workerHttpClientOptions.MockJwtToken}");
    });

    builder.Services.AddHostedService<B3ImportConsumer>();
    builder.Services.AddHostedService<TestConnectionConsumer>();
    builder.Services.AddHostedService<TransactionOutboxProcessor>();
    builder.Services.AddHostedService<TransactionProcessingConsumer>();

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
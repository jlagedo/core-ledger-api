using CoreLedger.Application;
using CoreLedger.Infrastructure;
using CoreLedger.Worker.Services;
using Serilog;
using Serilog.Events;

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
    .WriteTo.File(
        path: "logs/core-ledger-worker-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting Core Ledger Worker");

    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog(Log.Logger);

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

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

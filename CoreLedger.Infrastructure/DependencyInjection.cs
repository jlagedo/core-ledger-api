using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Infrastructure.Configuration;
using CoreLedger.Infrastructure.Persistence;
using CoreLedger.Infrastructure.Persistence.Repositories;
using CoreLedger.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreLedger.Infrastructure;

/// <summary>
///     Extension methods for registering Infrastructure layer services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure options
        services.Configure<DatabaseOptions>(configuration.GetSection("Database"));
        services.Configure<HttpClientOptions>(configuration.GetSection("HttpClient"));
        services.Configure<RabbitMQOptions>(configuration.GetSection("RabbitMQ"));
        services.Configure<B3ImportOptions>(configuration.GetSection("B3Import"));

        // Get database options for DbContext configuration
        var databaseOptions = configuration.GetSection("Database").Get<DatabaseOptions>() ?? new DatabaseOptions();

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            options.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions
                    .EnableRetryOnFailure(
                        databaseOptions.MaxRetryCount,
                        TimeSpan.FromSeconds(databaseOptions.MaxRetryDelaySeconds),
                        null)
                    .CommandTimeout(databaseOptions.CommandTimeoutSeconds)
                    .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        });

        services.AddScoped<IToDoRepository, ToDoRepository>();
        services.AddScoped<IAccountTypeRepository, AccountTypeRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ICoreJobRepository, CoreJobRepository>();
        services.AddScoped<IFundRepository, FundRepository>();
        services.AddScoped<ISecurityRepository, SecurityRepository>();
        services.AddScoped<ITransactionStatusRepository, TransactionStatusRepository>();
        services.AddScoped<ITransactionTypeRepository, TransactionTypeRepository>();
        services.AddScoped<ITransactionSubTypeRepository, TransactionSubTypeRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Get HTTP client options for Auth0 service configuration
        var httpClientOptions =
            configuration.GetSection("HttpClient").Get<HttpClientOptions>() ?? new HttpClientOptions();

        // HttpClient for Auth0 API calls
        services.AddHttpClient<IAuth0Service, Auth0Service>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(httpClientOptions.Auth0TimeoutSeconds);
            client.DefaultRequestHeaders.Add("User-Agent", httpClientOptions.UserAgent);
        });

        // User management service
        services.AddScoped<IUserService, UserService>();

        services.AddSingleton<IMessagePublisher, RabbitMQPublisher>();
        services.AddScoped<IB3ImportProcessor, B3ImportProcessor>();

        return services;
    }
}
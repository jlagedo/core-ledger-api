using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Infrastructure.Persistence;
using CoreLedger.Infrastructure.Persistence.Repositories;
using CoreLedger.Infrastructure.Services;
using StackExchange.Redis;

namespace CoreLedger.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure layer services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            options.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions
                    .EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null)
                    .CommandTimeout(30)
                    .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        });

        services.AddScoped<IToDoRepository, ToDoRepository>();
        services.AddScoped<IAccountTypeRepository, AccountTypeRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ICoreJobRepository, CoreJobRepository>();
        services.AddScoped<IFundRepository, FundRepository>();
        services.AddScoped<ISecurityRepository, SecurityRepository>();

        services.AddSingleton<IMessagePublisher, RabbitMQPublisher>();
        services.AddScoped<IB3ImportProcessor, B3ImportProcessor>();

        // Register Redis connection as singleton (thread-safe, connection pooling)
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var connectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379";
            return ConnectionMultiplexer.Connect(connectionString);
        });

        // Register job notification service as singleton (stateless, thread-safe)
        services.AddSingleton<IJobNotificationService, RedisJobNotificationService>();

        return services;
    }
}

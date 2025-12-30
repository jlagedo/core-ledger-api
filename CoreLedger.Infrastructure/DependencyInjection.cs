using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Interfaces;
using CoreLedger.Infrastructure.Persistence;
using CoreLedger.Infrastructure.Persistence.Repositories;
using CoreLedger.Infrastructure.Services;

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
        services.AddScoped<ITransactionStatusRepository, TransactionStatusRepository>();
        services.AddScoped<ITransactionTypeRepository, TransactionTypeRepository>();
        services.AddScoped<ITransactionSubTypeRepository, TransactionSubTypeRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddSingleton<IMessagePublisher, RabbitMQPublisher>();
        services.AddScoped<IB3ImportProcessor, B3ImportProcessor>();

        return services;
    }
}

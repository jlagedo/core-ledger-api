using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace CoreLedger.API.Extensions;

/// <summary>
/// Extension methods for configuring JWT Bearer authentication with Auth0.
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Adds JWT Bearer authentication configured for Auth0.
    /// </summary>
    public static IServiceCollection AddAuth0Authentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var domain = configuration["Auth0:Domain"]
            ?? throw new InvalidOperationException("Auth0:Domain configuration is missing");
        var audience = configuration["Auth0:Audience"]
            ?? throw new InvalidOperationException("Auth0:Audience configuration is missing");

        // Ensure domain has proper format
        if (!domain.StartsWith("https://"))
        {
            domain = $"https://{domain}";
        }
        if (!domain.EndsWith("/"))
        {
            domain = $"{domain}/";
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = domain;
            options.Audience = audience;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = domain,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            // Optional: Log authentication failures for debugging
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();
                    logger.LogWarning(
                        "JWT authentication failed: {Message}",
                        context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();
                    var userId = context.Principal?.FindFirst("sub")?.Value;
                    logger.LogInformation(
                        "JWT token validated for user: {UserId}",
                        userId);
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();

        return services;
    }
}

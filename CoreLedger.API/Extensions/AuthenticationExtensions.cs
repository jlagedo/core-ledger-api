using System.IdentityModel.Tokens.Jwt;
using CoreLedger.API.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace CoreLedger.API.Extensions;

public static class AuthenticationExtensions
{
    /// <summary>
    /// Adds JWT Bearer authentication configured for Auth0.
    /// </summary>
    public static IServiceCollection AddAuth0Authentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure Auth0 options
        services.Configure<Auth0Options>(configuration.GetSection("Auth0"));

        var auth0Options = configuration.GetSection("Auth0").Get<Auth0Options>()
                           ?? throw new InvalidOperationException("Auth0 configuration is missing");

        // Disable legacy WS-Fed claim mappings
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

        var domain = auth0Options.Domain
                     ?? throw new InvalidOperationException("Auth0:Domain configuration is missing");
        var audience = auth0Options.Audience
                       ?? throw new InvalidOperationException("Auth0:Audience configuration is missing");

        if (!domain.StartsWith("https://")) domain = $"https://{domain}";
        if (!domain.EndsWith("/")) domain = $"{domain}/";

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
                    ClockSkew = TimeSpan.FromMinutes(auth0Options.ClockSkewMinutes),

                    NameClaimType = "sub",
                    RoleClaimType = "permissions"
                };

                options.MapInboundClaims = false;
            });

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Adds simplified development authentication.
    /// </summary>
    public static IServiceCollection AddDevelopmentAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = false,
                    RequireSignedTokens = false,

                    // Preserve claim types (don't map 'sub' to ClaimTypes.NameIdentifier)
                    NameClaimType = "sub",
                    RoleClaimType = "permissions"
                };

                // Don't map inbound claims (preserve 'sub' as-is)
                options.MapInboundClaims = false;
            });

        services.AddAuthorization();

        return services;
    }
}
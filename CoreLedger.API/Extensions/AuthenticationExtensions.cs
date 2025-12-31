using System.IdentityModel.Tokens.Jwt;
using CoreLedger.API.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace CoreLedger.API.Extensions;

/// <summary>
///     Extension methods for configuring JWT Bearer authentication with Auth0.
/// </summary>
public static class AuthenticationExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Adds JWT Bearer authentication configured for Auth0.
        /// </summary>
        public IServiceCollection AddAuth0Authentication(IConfiguration configuration)
        {
            // Configure Auth0 options
            services.Configure<Auth0Options>(configuration.GetSection("Auth0"));

            // Get Auth0 options
            var auth0Options = configuration.GetSection("Auth0").Get<Auth0Options>()
                               ?? throw new InvalidOperationException("Auth0 configuration is missing");

            // Clear default claim type mappings to preserve original JWT claim names
            // Without this, "sub" gets mapped to "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            var domain = auth0Options.Domain
                         ?? throw new InvalidOperationException("Auth0:Domain configuration is missing");
            var audience = auth0Options.Audience
                           ?? throw new InvalidOperationException("Auth0:Audience configuration is missing");

            // Ensure domain has proper format
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
                        // Map Auth0's claim names to standard .NET claim types
                        NameClaimType = "sub",
                        RoleClaimType = "permissions"
                    };

                    options.MapInboundClaims = false;

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

                            // Log the sub claim (user ID)
                            var userId = context.Principal?.FindFirst("sub")?.Value;
                            logger.LogInformation(
                                "JWT token validated for user: {UserId}",
                                userId);

                            // Debug: Log all claims to help troubleshoot
                            if (userId == null && context.Principal?.Claims != null)
                                logger.LogWarning(
                                    "Sub claim not found. Available claims: {Claims}",
                                    string.Join(", ", context.Principal.Claims.Select(c => $"{c.Type}={c.Value}")));

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization();

            return services;
        }

        public IServiceCollection AddDevelopmentAuthentication(IConfiguration configuration)
        {
            {
                services.AddAuthentication("Dev")
                    .AddJwtBearer("Dev", options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateLifetime = false,
                            ValidateIssuerSigningKey = false,
                            LogValidationExceptions = true,
                            LogTokenId = true,
                            RequireSignedTokens = false
                        };
                        options.Events = new JwtBearerEvents
                        {
                            OnMessageReceived = context =>
                            {
                                var logger = context.HttpContext.RequestServices
                                    .GetRequiredService<ILoggerFactory>()
                                    .CreateLogger("JwtDebug");

                                logger.LogInformation("Token received: {Token}", context.Token);
                                return Task.CompletedTask;
                            },

                            OnTokenValidated = context =>
                            {
                                var logger = context.HttpContext.RequestServices
                                    .GetRequiredService<ILoggerFactory>()
                                    .CreateLogger("JwtDebug");

                                logger.LogInformation("Token validated successfully");
                                if (context.Principal != null)
                                    logger.LogInformation("Claims: {Claims}",
                                        string.Join(", ", context.Principal.Claims.Select(c => $"{c.Type}={c.Value}")));

                                return Task.CompletedTask;
                            },

                            OnAuthenticationFailed = context =>
                            {
                                var logger = context.HttpContext.RequestServices
                                    .GetRequiredService<ILoggerFactory>()
                                    .CreateLogger("JwtDebug");

                                logger.LogError(context.Exception, "Authentication failed");
                                return Task.CompletedTask;
                            },

                            OnChallenge = context =>
                            {
                                var logger = context.HttpContext.RequestServices
                                    .GetRequiredService<ILoggerFactory>()
                                    .CreateLogger("JwtDebug");

                                logger.LogWarning("JWT challenge triggered. Error: {Error}, Description: {Description}",
                                    context.Error, context.ErrorDescription);

                                return Task.CompletedTask;
                            }
                        };
                    });

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Dev";
                    options.DefaultChallengeScheme = "Dev";
                });

                return services;
            }
        }
    }
}
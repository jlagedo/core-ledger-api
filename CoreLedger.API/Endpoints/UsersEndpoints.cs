using CoreLedger.API.Extensions;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;

namespace CoreLedger.API.Endpoints;

/// <summary>
///     Minimal API endpoints for managing authenticated user profile.
/// </summary>
public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithSummary("Retrieves the current authenticated user's profile")
            .WithDescription("Creates user record if first login, or updates profile from Auth0 on subsequent logins")
            .Produces<UserDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status503ServiceUnavailable)
            ;

        return routes;
    }

    private static async Task<IResult> GetCurrentUser(
        IUserService userService,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(typeof(UsersEndpoints));

        // Extract claims from JWT
        var authProviderId = context.GetUserId();
        if (string.IsNullOrEmpty(authProviderId))
        {
            logger.LogWarning("User claim 'sub' not found in token");
            return Results.Unauthorized();
        }

        // Extract provider - for Auth0, we'll use "auth0" as the provider
        var provider = "auth0";

        // Get access token from Authorization header
        var accessToken = context.GetBearerToken();
        if (string.IsNullOrEmpty(accessToken))
        {
            logger.LogWarning("Access token not found in Authorization header");
            return Results.Unauthorized();
        }

        logger.LogInformation(
            "GetCurrentUser called for AuthProviderId: {AuthProviderId}",
            authProviderId);

        // This will create or update the user as needed
        var userDto = await userService.EnsureUserExistsAsync(
            authProviderId,
            provider,
            accessToken,
            cancellationToken);

        return Results.Ok(userDto);
    }
}

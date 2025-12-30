using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;

namespace CoreLedger.API.Controllers;

/// <summary>
/// Controller for managing authenticated user profile.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserService userService,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the current authenticated user's profile.
    /// Creates user record if first login, or updates profile from Auth0 on subsequent logins.
    /// </summary>
    /// <response code="200">User profile retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="503">Auth0 service unavailable</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        // Extract claims from JWT
        var authProviderId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(authProviderId))
        {
            _logger.LogWarning("User claim 'sub' not found in token");
            return Unauthorized(new { message = "Invalid authentication token" });
        }

        // Extract provider - for Auth0, we'll use "auth0" as the provider
        var provider = "auth0";

        // Get access token from Authorization header
        var accessToken = Request.Headers["Authorization"]
            .FirstOrDefault()?
            .Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogWarning("Access token not found in Authorization header");
            return Unauthorized(new { message = "Access token required" });
        }

        _logger.LogInformation(
            "GetCurrentUser called for AuthProviderId: {AuthProviderId}",
            authProviderId);

        // This will create or update the user as needed
        var userDto = await _userService.EnsureUserExistsAsync(
            authProviderId,
            provider,
            accessToken,
            cancellationToken);

        return Ok(userDto);
    }
}

using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Infrastructure.Services;

/// <summary>
///     Service for managing user lifecycle and synchronization with Auth0.
/// </summary>
public class UserService : IUserService
{
    private readonly IAuth0Service _auth0Service;
    private readonly ILogger<UserService> _logger;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;

    public UserService(
        IUserRepository userRepository,
        IAuth0Service auth0Service,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _auth0Service = auth0Service;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserDto> EnsureUserExistsAsync(
        string authProviderId,
        string provider,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Ensuring user exists: AuthProviderId={AuthProviderId}, Provider={Provider}",
            authProviderId,
            provider);

        // Check if user already exists
        var existingUser = await _userRepository.GetByAuthProviderIdAsync(
            authProviderId,
            provider,
            cancellationToken);

        if (existingUser != null)
        {
            _logger.LogInformation(
                "User found in database: Id={UserId}, Email={Email}",
                existingUser.Id,
                existingUser.Email);

            // User exists - fetch fresh profile from Auth0 to keep in sync
            var auth0Profile = await _auth0Service.GetUserProfileAsync(
                accessToken,
                cancellationToken);

            existingUser.UpdateLoginInfo(auth0Profile.Email, auth0Profile.Name);
            await _userRepository.UpdateAsync(existingUser, cancellationToken);

            _logger.LogInformation(
                "Updated existing user from Auth0: Id={UserId}, Email={Email}",
                existingUser.Id,
                existingUser.Email);

            return _mapper.Map<UserDto>(existingUser);
        }

        // User doesn't exist - create new user from Auth0 profile
        _logger.LogInformation(
            "User not found in database, creating from Auth0: {AuthProviderId}",
            authProviderId);

        var profile = await _auth0Service.GetUserProfileAsync(accessToken, cancellationToken);

        var newUser = User.Create(
            profile.Sub,
            provider,
            profile.Sub, // User creates themselves on first login
            profile.Email,
            profile.Name);

        await _userRepository.AddAsync(newUser, cancellationToken);

        _logger.LogInformation(
            "Created new user from Auth0: Id={UserId}, Email={Email}",
            newUser.Id,
            newUser.Email);

        return _mapper.Map<UserDto>(newUser);
    }

    public async Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        return user == null ? null : _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto?> GetByAuthProviderIdAsync(
        string authProviderId,
        string provider,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByAuthProviderIdAsync(
            authProviderId,
            provider,
            cancellationToken);
        return user == null ? null : _mapper.Map<UserDto>(user);
    }
}
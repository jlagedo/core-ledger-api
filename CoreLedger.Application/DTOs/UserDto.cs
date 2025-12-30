namespace CoreLedger.Application.DTOs;

/// <summary>
/// Data transfer object for User entity.
/// </summary>
public record UserDto(
    int Id,
    string AuthProviderId,
    string Provider,
    string? Email,
    string? Name,
    DateTime LastLoginAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

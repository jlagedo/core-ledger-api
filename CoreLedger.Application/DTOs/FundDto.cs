using System.ComponentModel.DataAnnotations;
using CoreLedger.Domain.Enums;

namespace CoreLedger.Application.DTOs;

/// <summary>
/// Data transfer object for Fund entity.
/// </summary>
public record FundDto(
    int Id,
    string Code,
    string Name,
    string BaseCurrency,
    DateTime InceptionDate,
    ValuationFrequency ValuationFrequency,
    string ValuationFrequencyDescription,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// DTO for creating a new fund.
/// </summary>
public record CreateFundDto
{
    [Required]
    [MaxLength(10)]
    [RegularExpression("^[A-Z0-9]+$", ErrorMessage = "Fund code must contain only alphanumeric characters (A-Z, 0-9)")]
    public string Code { get; init; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string BaseCurrency { get; init; } = string.Empty;

    [Required]
    public DateTime InceptionDate { get; init; }

    [Required]
    public ValuationFrequency ValuationFrequency { get; init; }
}

/// <summary>
/// DTO for updating an existing Fund.
/// </summary>
public record UpdateFundDto
{
    [Required]
    [MaxLength(10)]
    [RegularExpression("^[A-Z0-9]+$", ErrorMessage = "Fund code must contain only alphanumeric characters (A-Z, 0-9)")]
    public string Code { get; init; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string BaseCurrency { get; init; } = string.Empty;

    [Required]
    public DateTime InceptionDate { get; init; }

    [Required]
    public ValuationFrequency ValuationFrequency { get; init; }
}

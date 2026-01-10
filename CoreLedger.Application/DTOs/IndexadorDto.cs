using System.ComponentModel.DataAnnotations;
using CoreLedger.Domain.Enums;

namespace CoreLedger.Application.DTOs;

/// <summary>
///     Data transfer object for Indexador entity.
/// </summary>
public record IndexadorDto(
    int Id,
    string Codigo,
    string Nome,
    IndexadorTipo Tipo,
    string TipoDescricao,
    string? Fonte,
    Periodicidade Periodicidade,
    string PeriodicidadeDescricao,
    decimal? FatorAcumulado,
    DateTime? DataBase,
    string? UrlFonte,
    bool ImportacaoAutomatica,
    bool Ativo,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
///     DTO for creating a new indexador.
/// </summary>
public record CreateIndexadorDto
{
    [Required]
    [MaxLength(20)]
    [RegularExpression("^[A-Z0-9]+$",
        ErrorMessage = "Código must contain only alphanumeric characters (A-Z, 0-9)")]
    public string Codigo { get; init; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Nome { get; init; } = string.Empty;

    [Required] public IndexadorTipo Tipo { get; init; }

    [MaxLength(100)] public string? Fonte { get; init; }

    [Required] public Periodicidade Periodicidade { get; init; }

    public decimal? FatorAcumulado { get; init; }

    public DateTime? DataBase { get; init; }

    [MaxLength(500)] public string? UrlFonte { get; init; }

    public bool ImportacaoAutomatica { get; init; }

    public bool Ativo { get; init; } = true;
}

/// <summary>
///     DTO for updating an existing indexador.
/// </summary>
public record UpdateIndexadorDto
{
    [Required]
    [MaxLength(100)]
    public string Nome { get; init; } = string.Empty;

    [Required] public IndexadorTipo Tipo { get; init; }

    [MaxLength(100)] public string? Fonte { get; init; }

    [Required] public Periodicidade Periodicidade { get; init; }

    public decimal? FatorAcumulado { get; init; }

    public DateTime? DataBase { get; init; }

    [MaxLength(500)] public string? UrlFonte { get; init; }

    public bool ImportacaoAutomatica { get; init; }

    public bool Ativo { get; init; }
}

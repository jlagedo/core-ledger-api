using CoreLedger.Domain.Enums;

namespace CoreLedger.Application.DTOs;

/// <summary>
///     DTO for Calendario entity.
/// </summary>
public record CalendarioDto(
    int Id,
    DateOnly Data,
    bool DiaUtil,
    TipoDia TipoDia,
    string TipoDiaDescricao,
    Praca Praca,
    string PracaDescricao,
    string? Descricao,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
///     DTO for creating a new Calendario entry.
/// </summary>
public record CreateCalendarioDto(
    DateOnly Data,
    TipoDia TipoDia,
    Praca Praca,
    string? Descricao
);

/// <summary>
///     DTO for updating an existing Calendario entry.
/// </summary>
public record UpdateCalendarioDto(
    TipoDia TipoDia,
    string? Descricao
);

/// <summary>
///     DTO for business day check result.
/// </summary>
public record DiaUtilResultDto(
    DateOnly Data,
    bool DiaUtil,
    TipoDia TipoDia,
    string? Descricao
);

/// <summary>
///     DTO for D+X calculation result.
/// </summary>
public record CalculoDMaisResultDto(
    DateOnly DataInicial,
    int DiasUteis,
    DateOnly DataFinal,
    Praca Praca
);

/// <summary>
///     DTO for calendar import result.
/// </summary>
public record ImportarCalendarioResultDto(
    int Ano,
    int DiasImportados,
    int DiasAtualizados
);

/// <summary>
///     DTO for calendar health check result (CAL-002/CAL-003).
/// </summary>
public record CalendarioHealthDto(
    bool NacionalPreenchido,
    bool Proximo30DiasOk,
    List<string> Alertas
);

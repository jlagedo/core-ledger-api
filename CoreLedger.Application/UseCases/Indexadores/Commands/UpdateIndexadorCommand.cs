using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Enums;
using MediatR;

namespace CoreLedger.Application.UseCases.Indexadores.Commands;

/// <summary>
///     Command to update an existing Indexador.
/// </summary>
public record UpdateIndexadorCommand(
    int Id,
    string Nome,
    IndexadorTipo Tipo,
    string? Fonte,
    Periodicidade Periodicidade,
    decimal? FatorAcumulado,
    DateTime? DataBase,
    string? UrlFonte,
    bool ImportacaoAutomatica,
    bool Ativo
) : IRequest<IndexadorDto>;

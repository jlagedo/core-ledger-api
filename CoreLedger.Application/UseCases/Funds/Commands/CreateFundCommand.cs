using MediatR;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Enums;

namespace CoreLedger.Application.UseCases.Funds.Commands;

/// <summary>
/// Command to create a new Fund.
/// </summary>
public record CreateFundCommand(
    string Code,
    string Name,
    string BaseCurrency,
    DateTime InceptionDate,
    ValuationFrequency ValuationFrequency
) : IRequest<FundDto>;

using MediatR;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.Funds.Queries;

/// <summary>
/// Query to retrieve a specific Fund by ID.
/// </summary>
public record GetFundByIdQuery(int Id) : IRequest<FundDto>;

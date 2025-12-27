using MediatR;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.Securities.Queries;

/// <summary>
/// Query to retrieve a specific Security by ID.
/// </summary>
public record GetSecurityByIdQuery(int Id) : IRequest<SecurityDto>;

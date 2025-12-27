using MediatR;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.UseCases.Accounts.Queries;

public record GetAccountsByTypeReportQuery() : IRequest<IReadOnlyList<AccountsByTypeReportDto>>;

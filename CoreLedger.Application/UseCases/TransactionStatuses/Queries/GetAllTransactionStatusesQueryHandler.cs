using Microsoft.Extensions.Logging;

using AutoMapper;
using MediatR;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Interfaces;

namespace CoreLedger.Application.UseCases.TransactionStatuses.Queries;

public class GetAllTransactionStatusesQueryHandler : IRequestHandler<GetAllTransactionStatusesQuery, IReadOnlyList<TransactionStatusDto>>
{
    private readonly ITransactionStatusRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllTransactionStatusesQueryHandler> _logger;

    public GetAllTransactionStatusesQueryHandler(
        ITransactionStatusRepository repository,
        IMapper mapper,
        ILogger<GetAllTransactionStatusesQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TransactionStatusDto>> Handle(GetAllTransactionStatusesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all transaction statuses");
        var statuses = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<TransactionStatusDto>>(statuses);
    }
}

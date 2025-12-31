using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.TransactionStatuses.Queries;

public class
    GetAllTransactionStatusesQueryHandler : IRequestHandler<GetAllTransactionStatusesQuery,
    IReadOnlyList<TransactionStatusDto>>
{
    private readonly ILogger<GetAllTransactionStatusesQueryHandler> _logger;
    private readonly IMapper _mapper;
    private readonly ITransactionStatusRepository _repository;

    public GetAllTransactionStatusesQueryHandler(
        ITransactionStatusRepository repository,
        IMapper mapper,
        ILogger<GetAllTransactionStatusesQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TransactionStatusDto>> Handle(GetAllTransactionStatusesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all transaction statuses");
        var statuses = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<TransactionStatusDto>>(statuses);
    }
}
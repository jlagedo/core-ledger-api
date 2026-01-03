using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Application.UseCases.Securities.Queries;

/// <summary>
///     Handler for retrieving all SecurityType enum values.
/// </summary>
public class GetAllSecurityTypesQueryHandler : IRequestHandler<GetAllSecurityTypesQuery, IReadOnlyList<SecurityTypeDto>>
{
    private readonly ILogger<GetAllSecurityTypesQueryHandler> _logger;

    public GetAllSecurityTypesQueryHandler(ILogger<GetAllSecurityTypesQueryHandler> logger)
    {
        _logger = logger;
    }

    public Task<IReadOnlyList<SecurityTypeDto>> Handle(
        GetAllSecurityTypesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all SecurityType enum values");

        var securityTypes = Enum.GetValues<SecurityType>()
            .Select(type => new SecurityTypeDto(
                (int)type,
                type.ToString(),
                type.ToString()
            ))
            .ToList();

        _logger.LogInformation("Retrieved {Count} SecurityType values", securityTypes.Count);

        return Task.FromResult<IReadOnlyList<SecurityTypeDto>>(securityTypes);
    }
}
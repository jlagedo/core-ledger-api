using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Entities;

namespace CoreLedger.Application.Mappings;

/// <summary>
///     AutoMapper profile for Fund entity mappings.
/// </summary>
public class FundMappingProfile : Profile
{
    public FundMappingProfile()
    {
        CreateMap<Fund, FundDto>()
            .ConstructUsing(src => new FundDto(
                src.Id,
                src.Code,
                src.Name,
                src.BaseCurrency,
                src.InceptionDate,
                src.ValuationFrequency,
                src.ValuationFrequency.ToString(),
                src.CreatedAt,
                src.UpdatedAt
            ));
    }
}
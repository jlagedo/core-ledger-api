using AutoMapper;
using CoreLedger.Domain.Entities;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.Mappings;

/// <summary>
/// AutoMapper profile for Account entity mappings.
/// </summary>
public class AccountMappingProfile : Profile
{
    public AccountMappingProfile()
    {
        CreateMap<Account, AccountDto>()
            .ForMember(dest => dest.TypeDescription, 
                opt => opt.MapFrom(src => src.Type != null ? src.Type.Description : string.Empty))
            .ForMember(dest => dest.StatusDescription,
                opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.NormalBalanceDescription,
                opt => opt.MapFrom(src => src.NormalBalance.ToString()));
    }
}

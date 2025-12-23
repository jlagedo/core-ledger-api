using AutoMapper;
using CoreLedger.Domain.Entities;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.Mappings;

/// <summary>
/// AutoMapper profile for AccountType entity mappings.
/// </summary>
public class AccountTypeMappingProfile : Profile
{
    public AccountTypeMappingProfile()
    {
        CreateMap<AccountType, AccountTypeDto>();
    }
}

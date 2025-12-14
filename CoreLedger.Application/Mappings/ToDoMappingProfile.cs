using AutoMapper;
using CoreLedger.Domain.Entities;
using CoreLedger.Application.DTOs;

namespace CoreLedger.Application.Mappings;

/// <summary>
/// AutoMapper profile for ToDo entity mappings.
/// </summary>
public class ToDoMappingProfile : Profile
{
    public ToDoMappingProfile()
    {
        CreateMap<ToDo, ToDoDto>();
    }
}

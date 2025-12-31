using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Domain.Entities;

namespace CoreLedger.Application.Mappings;

/// <summary>
///     AutoMapper profile for ToDo entity mappings.
/// </summary>
public class ToDoMappingProfile : Profile
{
    public ToDoMappingProfile()
    {
        CreateMap<ToDo, ToDoDto>();
    }
}
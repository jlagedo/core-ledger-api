using AutoMapper;
using coreledger.model;
using core_ledger_api.Dtos;

namespace core_ledger_api.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ToDo, ToDoDto>();
            CreateMap<ToDoDto, ToDo>();
            CreateMap<CreateToDoDto, ToDo>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CompletedAt, opt => opt.Ignore());
            CreateMap<UpdateToDoDto, ToDo>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedAt, opt => opt.Ignore());
        }
    }
}

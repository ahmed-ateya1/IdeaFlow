using AutoMapper;
using MindMapGenerator.Core.Domain.Entities;
using MindMapGenerator.Core.Dtos.DiagramDto;

namespace MindMapGenerator.Core.MappingProfile
{
    public class DiagramMappingProfile : Profile
    {
        public DiagramMappingProfile()
        {
            CreateMap<DiagramAddRequest, Diagram>()
                .ForMember(dest => dest.DiagramID, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ReverseMap();

            CreateMap<DiagramUpdateRequest, Diagram>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ReverseMap();

            CreateMap<Diagram, DiagramResponse>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.NumberOfFavorites, opt => opt.MapFrom(src => src.Favorites.Count))
                .ForMember(dest=>dest.BaseDiagramID, opt => opt.MapFrom(src => src.BaseDiagramID))
                .ForMember(dest => dest.BaseDiagramTitle, opt => opt.MapFrom(src => src.BaseDiagram.Title))
                .ForMember(dest => dest.BaseDiagramContentJson, opt => opt.MapFrom(src => src.BaseDiagram.ContentJson))
                .ReverseMap();
        }
    }
}

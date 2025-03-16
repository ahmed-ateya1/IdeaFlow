using AutoMapper;
using MindMapGenerator.Core.Domain.Entities;
using MindMapGenerator.Core.Dtos.FavoriteDto;

namespace MindMapGenerator.Core.MappingProfile
{
    public class FavoriteMappingProfile : Profile
    {
        public FavoriteMappingProfile()
        {
            CreateMap<FavoriteAddRequest, Favorite>()
                .ForMember(dest=>dest.FavouriteID, opt=>opt.MapFrom(src => Guid.NewGuid()))
                .ReverseMap();

            CreateMap<FavoriteUpdateRequest , Favorite>()
                .ReverseMap();

            CreateMap<Favorite, FavoriteResponse>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Diagram.Title))
                .ForMember(dest => dest.ContentJson, opt => opt.MapFrom(src => src.Diagram.ContentJson))
                .ReverseMap();
        }
    }
}

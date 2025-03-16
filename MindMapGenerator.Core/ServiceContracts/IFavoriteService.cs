using MindMapGenerator.Core.Domain.Entities;
using MindMapGenerator.Core.Dtos;
using MindMapGenerator.Core.Dtos.FavoriteDto;
using System.Linq.Expressions;

namespace MindMapGenerator.Core.ServiceContracts
{
    public interface IFavoriteService
    {
        Task<FavoriteResponse> CreateAsync(FavoriteAddRequest? request);
        Task<FavoriteResponse> UpdateAsync(FavoriteUpdateRequest? request);
        Task<bool> DeleteAsync(Guid id);
        Task<FavoriteResponse?> GetByAsync(Expression<Func<Favorite, bool>> predicate , bool isTracked=false);
        Task<PaginatedResponse<FavoriteResponse>> GetAllAsync(Expression<Func<Favorite, bool>>? predicate = null , PaginationDto? pagination = null);
    }
}

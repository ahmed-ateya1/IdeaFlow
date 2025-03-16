using MindMapGenerator.Core.Domain.Entities;
using MindMapGenerator.Core.Dtos;
using MindMapGenerator.Core.Dtos.DiagramDto;
using System.Linq.Expressions;

namespace MindMapGenerator.Core.ServiceContracts
{
    public interface IDiagramService
    {
        Task<DiagramResponse> CreateAsync(DiagramAddRequest? request);
        Task<DiagramResponse> UpdateAsync(DiagramUpdateRequest? request);
        Task<bool> DeleteAsync(Guid id);
        Task<DiagramResponse?> GetByAsync(Expression<Func<Diagram , bool>> expression , bool isTracked=false);
        Task<PaginatedResponse<DiagramResponse>> GetAllAsync(Expression<Func<Diagram, bool>>? expression=null , PaginationDto? pagination = null);

    }
}

using MindMapGenerator.Core.Domain.IdentityEntities;

namespace MindMapGenerator.Core.Domain.RepositoryContract
{
    public interface IUserContext
    {
        Task<ApplicationUser?> GetCurrentUserAsync();
    }
}

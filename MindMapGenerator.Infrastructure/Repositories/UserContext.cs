using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MindMapGenerator.Core.Domain.IdentityEntities;
using MindMapGenerator.Core.Domain.RepositoryContract;
using System.Security.Claims;

namespace MindMapGenerator.Infrastructure.Repositories
{
    public class UserContext : IUserContext
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserContext> _logger;

        public UserContext(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ILogger<UserContext> logger)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            var email = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("No user is authenticated.");
                return null;
            }

            var user = await _unitOfWork.Repository<ApplicationUser>()
                .GetByAsync(x => x.Email == email);

            return user;
        }
    }
}

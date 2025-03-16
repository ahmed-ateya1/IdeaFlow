using AutoMapper;
using Microsoft.Extensions.Logging;
using MindMapGenerator.Core.Domain.Entities;
using MindMapGenerator.Core.Domain.IdentityEntities;
using MindMapGenerator.Core.Domain.RepositoryContract;
using MindMapGenerator.Core.Dtos;
using MindMapGenerator.Core.Dtos.FavoriteDto;
using MindMapGenerator.Core.ServiceContracts;
using System.Linq.Expressions;

namespace MindMapGenerator.Core.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;
        private readonly ILogger<FavoriteService> _logger;
        public FavoriteService(IMapper mapper, IUserContext userContext, IUnitOfWork unitOfWork, ILogger<FavoriteService> logger)
        {
            _mapper = mapper;
            _userContext = userContext;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        private async Task<(ApplicationUser? , Diagram?)> GetUserAndDiagramAsync(Guid diagramId)
        {
            var user = await _userContext.GetCurrentUserAsync();
            var diagram = await _unitOfWork.Repository<Diagram>().
                GetByAsync(d => d.DiagramID == diagramId);
            return (user, diagram);
        }
        private async Task ExecuteWithTransactionAsync(Func<Task> action)
        {
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    _logger.LogInformation("Starting transaction {TransactionId}", transaction.TransactionId);
                    await action();
                    await _unitOfWork.CommitTransactionAsync();
                    _logger.LogInformation("Transaction {TransactionId} committed successfully", transaction.TransactionId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Transaction {TransactionId} failed. Rolling back...", transaction.TransactionId);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
        }
        public async Task<FavoriteResponse> CreateAsync(FavoriteAddRequest? request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            var result = await GetUserAndDiagramAsync(request.DiagramID);
            var user = result.Item1;
            var diagram = result.Item2;
            if (user == null || diagram == null)
            {
                throw new KeyNotFoundException("User or Diagram not found");
            }
            var favorite = _mapper.Map<Favorite>(request);
            favorite.User = user;
            favorite.Diagram = diagram;
            await ExecuteWithTransactionAsync(async () =>
            {
                await _unitOfWork.Repository<Favorite>().CreateAsync(favorite);
            });
            return _mapper.Map<FavoriteResponse>(favorite);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var favorite = await _unitOfWork.Repository<Favorite>()
                .GetByAsync(f => f.FavouriteID == id);
            if (favorite == null)
            {
                return false;
            }
            await ExecuteWithTransactionAsync(async () =>
            {
                await _unitOfWork.Repository<Favorite>().DeleteAsync(favorite);
            });
            return true;
        }

        public async Task<PaginatedResponse<FavoriteResponse>> GetAllAsync(Expression<Func<Favorite, bool>>? predicate = null, PaginationDto? pagination = null)
        {
            pagination ??= new PaginationDto();
            var favorites = await _unitOfWork.Repository<Favorite>()
                .GetAllAsync(predicate, includeProperties: "User,Diagram",
                sortBy: pagination.SortBy,
                sortDirection: pagination.SortDirection,
                pageIndex: pagination.PageIndex,
                pageSize: pagination.PageSize);

            if (favorites == null || !favorites.Any())
            {
                return new PaginatedResponse<FavoriteResponse>
                {
                    Items = new List<FavoriteResponse>(),
                    TotalCount = 0,
                    PageIndex = pagination.PageIndex,
                    PageSize = pagination.PageSize
                };
            }
            var favoriteResponses = _mapper.Map<List<FavoriteResponse>>(favorites);
            return new PaginatedResponse<FavoriteResponse>
            {
                Items = favoriteResponses,
                TotalCount = favorites.Count(),
                PageIndex = pagination.PageIndex,
                PageSize = pagination.PageSize
            };
        }

        public async Task<FavoriteResponse?> GetByAsync(Expression<Func<Favorite, bool>> predicate, bool isTracked = false)
        {
            var favorite = await _unitOfWork.Repository<Favorite>()
                .GetByAsync(predicate,isTracked, includeProperties: "User,Diagram");
            if (favorite == null)
            {
                return null;
            }
            return _mapper.Map<FavoriteResponse>(favorite);
        }

        public async Task<FavoriteResponse> UpdateAsync(FavoriteUpdateRequest? request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            var result = await GetUserAndDiagramAsync(request.DiagramID);
            var user = result.Item1;
            var diagram = result.Item2;

            if (user == null || diagram == null)
            {
                throw new KeyNotFoundException("User or Diagram not found");
            }
            var favorite = await _unitOfWork.Repository<Favorite>()
                .GetByAsync(f => f.FavouriteID == request.FavoriteID,includeProperties:"User,Diagram");
            if (favorite == null)
            {
                throw new KeyNotFoundException("Favorite not found");
            }

            _mapper.Map(request, favorite);

            await ExecuteWithTransactionAsync(async () =>
            {
                await _unitOfWork.Repository<Favorite>().UpdateAsync(favorite);
            });

            return _mapper.Map<FavoriteResponse>(favorite);
        }
    }
}

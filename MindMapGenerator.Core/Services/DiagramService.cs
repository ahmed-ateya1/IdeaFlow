using AutoMapper;
using Microsoft.Extensions.Logging;
using MindMapGenerator.Core.Domain.Entities;
using MindMapGenerator.Core.Domain.RepositoryContract;
using MindMapGenerator.Core.Dtos;
using MindMapGenerator.Core.Dtos.DiagramDto;
using MindMapGenerator.Core.ServiceContracts;
using System.Linq.Expressions;

namespace MindMapGenerator.Core.Services
{
    public class DiagramService : IDiagramService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<DiagramService> _logger;
        private readonly IUserContext _userContext;

        public DiagramService(IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<DiagramService> logger,
            IUserContext userContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _userContext = userContext;
            _logger.LogDebug("DiagramService initialized");
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

        public async Task<DiagramResponse> CreateAsync(DiagramAddRequest? request)
        {
            _logger.LogInformation("Attempting to create new diagram");

            if (request == null)
            {
                _logger.LogWarning("CreateAsync called with null request");
                throw new ArgumentNullException(nameof(request));
            }

            var user = await _userContext.GetCurrentUserAsync();
            if (user == null)
            {
                _logger.LogWarning("No authenticated user found for diagram creation");
                throw new UnauthorizedAccessException();
            }

            _logger.LogDebug("Mapping request to Diagram entity for user {UserId}", user.Id);
            var diagram = _mapper.Map<Diagram>(request);
            diagram.UserID = user.Id;
            diagram.User = user;

            await ExecuteWithTransactionAsync(async () =>
            {
                await _unitOfWork.Repository<Diagram>().CreateAsync(diagram);
                _logger.LogInformation("Diagram {DiagramId} created successfully for user {UserId}",
                    diagram.DiagramID, user.Id);
            });

            return _mapper.Map<DiagramResponse>(diagram);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Attempting to delete diagram {DiagramId}", id);

            var diagram = await _unitOfWork.Repository<Diagram>()
                .GetByAsync(x => x.DiagramID == id);

            if (diagram == null)
            {
                _logger.LogWarning("Diagram {DiagramId} not found for deletion", id);
                return false;
            }

            await ExecuteWithTransactionAsync(async () =>
            {
                await _unitOfWork.Repository<Diagram>().DeleteAsync(diagram);
                _logger.LogInformation("Diagram {DiagramId} deleted successfully", id);
            });

            return true;
        }

        public async Task<PaginatedResponse<DiagramResponse>> GetAllAsync(
            Expression<Func<Diagram, bool>>? expression = null,
            PaginationDto? pagination = null)
        {
            _logger.LogInformation("Fetching diagrams with pagination {PageIndex}/{PageSize}",
                pagination?.PageIndex, pagination?.PageSize);

            pagination ??= new PaginationDto();

            var diagrams = await _unitOfWork.Repository<Diagram>()
                .GetAllAsync(expression,
                    includeProperties: "User",
                    sortBy: pagination.SortBy,
                    sortDirection: pagination.SortDirection,
                    pageIndex: pagination.PageIndex,
                    pageSize: pagination.PageSize);

            if (diagrams == null || !diagrams.Any())
            {
                _logger.LogInformation("No diagrams found for the given criteria");
                return new PaginatedResponse<DiagramResponse>
                {
                    Items = new List<DiagramResponse>(),
                    PageIndex = pagination.PageIndex,
                    PageSize = pagination.PageSize,
                    TotalCount = 0
                };
            }

            _logger.LogDebug("Found {Count} diagrams, mapping to response", diagrams.Count());
            var response = _mapper.Map<IEnumerable<DiagramResponse>>(diagrams);

            return new PaginatedResponse<DiagramResponse>
            {
                Items = response,
                PageIndex = pagination.PageIndex,
                PageSize = pagination.PageSize,
                TotalCount = diagrams.Count()
            };
        }

        public async Task<DiagramResponse?> GetByAsync(
            Expression<Func<Diagram, bool>> expression,
            bool isTracked = false)
        {
            _logger.LogInformation("Fetching diagram with specific criteria");

            var diagram = await _unitOfWork.Repository<Diagram>()
                .GetByAsync(expression,
                    isTracked: isTracked,
                    includeProperties: "User");

            if (diagram == null)
            {
                _logger.LogWarning("No diagram found matching the criteria");
                return null;
            }

            _logger.LogDebug("Diagram {DiagramId} retrieved successfully", diagram.DiagramID);
            return _mapper.Map<DiagramResponse>(diagram);
        }

        public async Task<DiagramResponse> UpdateAsync(DiagramUpdateRequest? request)
        {
            _logger.LogInformation("Attempting to update diagram");

            if (request == null)
            {
                _logger.LogWarning("UpdateAsync called with null request");
                throw new ArgumentNullException(nameof(request));
            }

            var user = await _userContext.GetCurrentUserAsync();
            if (user == null)
            {
                _logger.LogWarning("No authenticated user found for diagram update");
                throw new UnauthorizedAccessException();
            }

            var diagram = await _unitOfWork.Repository<Diagram>()
                .GetByAsync(x => x.DiagramID == request.DiagramID);

            if (diagram == null)
            {
                _logger.LogWarning("Diagram {DiagramId} not found for update", request.DiagramID);
                throw new KeyNotFoundException();
            }

            _logger.LogDebug("Mapping update request to diagram {DiagramId}", diagram.DiagramID);
            diagram = _mapper.Map(request, diagram);
            diagram.UserID = user.Id;
            diagram.User = user;

            await ExecuteWithTransactionAsync(async () =>
            {
                await _unitOfWork.Repository<Diagram>().UpdateAsync(diagram);
                _logger.LogInformation("Diagram {DiagramId} updated successfully for user {UserId}",
                    diagram.DiagramID, user.Id);
            });

            return _mapper.Map<DiagramResponse>(diagram);
        }
    }
}
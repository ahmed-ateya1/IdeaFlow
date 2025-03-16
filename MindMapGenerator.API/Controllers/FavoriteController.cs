using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindMapGenerator.Core.Domain.RepositoryContract;
using MindMapGenerator.Core.Dtos;
using MindMapGenerator.Core.Dtos.FavoriteDto;
using MindMapGenerator.Core.ServiceContracts;
using System.Net;

namespace MindMapGenerator.API.Controllers
{
    /// <summary>
    /// Controller for managing favorite operations in the MindMapGenerator application.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;
        private readonly ILogger<FavoriteController> _logger;
        private readonly IUserContext _userContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="FavoriteController"/> class.
        /// </summary>
        /// <param name="favoriteService">The favorite service instance.</param>
        /// <param name="logger">The logger instance for logging operations.</param>
        /// <param name="userContext">The user context for retrieving current user information.</param>
        public FavoriteController(
            IFavoriteService favoriteService,
            ILogger<FavoriteController> logger,
            IUserContext userContext)
        {
            _favoriteService = favoriteService;
            _logger = logger;
            _userContext = userContext;
            _logger.LogDebug("FavoriteController initialized");
        }

        /// <summary>
        /// Adds a new favorite for the authenticated user.
        /// </summary>
        /// <param name="request">The request containing favorite details.</param>
        /// <returns>An <see cref="ApiResponse"/> indicating the result of the operation.</returns>
        /// <response code="200">Returns when the favorite is added successfully.</response>
        /// <response code="400">Returns when the request is invalid.</response>
        /// <response code="500">Returns when an internal server error occurs.</response>
        [HttpPost("addFavorite")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> AddFavorite(FavoriteAddRequest request)
        {
            _logger.LogInformation("AddFavorite called with request: {@Request}", request);

            var response = await _favoriteService.CreateAsync(request);
            if (response == null)
            {
                _logger.LogWarning("Favorite creation returned null for request: {@Request}", request);
                return BadRequest(new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Failed to add favorite",
                    StatusCode = HttpStatusCode.BadRequest
                });
            }

            _logger.LogInformation("Favorite added successfully: {@Response}", response);
            return Ok(new ApiResponse
            {
                IsSuccess = true,
                Message = "Favorite added successfully",
                StatusCode = HttpStatusCode.OK,
                Result = response
            });
        }

        /// <summary>
        /// Deletes a favorite by its ID.
        /// </summary>
        /// <param name="id">The ID of the favorite to delete.</param>
        /// <returns>An <see cref="ApiResponse"/> indicating the result of the operation.</returns>
        /// <response code="200">Returns when the favorite is deleted successfully.</response>
        /// <response code="400">Returns when the favorite is not found.</response>
        /// <response code="500">Returns when an internal server error occurs.</response>
        [HttpDelete("deleteFavorite/{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeleteFavorite(Guid id)
        {
            _logger.LogInformation("DeleteFavorite called with id: {Id}", id);

            var response = await _favoriteService.DeleteAsync(id);
            if (!response)
            {
                _logger.LogWarning("Favorite not found for deletion with id: {Id}", id);
                return BadRequest(new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Favorite not found",
                    StatusCode = HttpStatusCode.BadRequest
                });
            }

            _logger.LogInformation("Favorite deleted successfully with id: {Id}", id);
            return Ok(new ApiResponse
            {
                IsSuccess = true,
                Message = "Favorite deleted successfully",
                StatusCode = HttpStatusCode.OK
            });
        }

        /// <summary>
        /// Retrieves a favorite by its ID.
        /// </summary>
        /// <param name="id">The ID of the favorite to retrieve.</param>
        /// <returns>An <see cref="ApiResponse"/> containing the favorite details.</returns>
        /// <response code="200">Returns when the favorite is found.</response>
        /// <response code="404">Returns when the favorite is not found.</response>
        /// <response code="500">Returns when an internal server error occurs.</response>
        [HttpGet("getFavorite/{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> GetFavorite(Guid id)
        {
            _logger.LogInformation("GetFavorite called with id: {Id}", id);

            var response = await _favoriteService.GetByAsync(f => f.FavouriteID == id);
            if (response == null)
            {
                _logger.LogDebug("Favorite not found with id: {Id}", id);
                return NotFound(new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Favorite not found",
                    StatusCode = HttpStatusCode.NotFound
                });
            }

            _logger.LogInformation("Favorite retrieved successfully: {@Response}", response);
            return Ok(new ApiResponse
            {
                IsSuccess = true,
                Message = "Favorite found successfully",
                StatusCode = HttpStatusCode.OK,
                Result = response
            });
        }

        /// <summary>
        /// Retrieves all favorites.
        /// </summary>
        /// <returns>An <see cref="ApiResponse"/> containing all favorites.</returns>
        /// <response code="200">Returns when favorites are found.</response>
        /// <response code="404">Returns when no favorites are found.</response>
        /// <response code="500">Returns when an internal server error occurs.</response>
        [HttpGet("getFavorites")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> GetFavorites()
        {
            _logger.LogInformation("GetFavorites called");

            var response = await _favoriteService.GetAllAsync();
            if (response == null || !response.Items.Any())
            {
                _logger.LogDebug("No favorites found");
                return NotFound(new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Favorites not found",
                    StatusCode = HttpStatusCode.NotFound
                });
            }

            _logger.LogInformation("Favorites retrieved successfully, count: {Count}", response.Items.Count());
            return Ok(new ApiResponse
            {
                IsSuccess = true,
                Message = "Favorites found successfully",
                StatusCode = HttpStatusCode.OK,
                Result = response
            });
        }

        /// <summary>
        /// Retrieves all favorites for the authenticated user.
        /// </summary>
        /// <returns>An <see cref="ApiResponse"/> containing the user's favorites.</returns>
        /// <response code="200">Returns when favorites are found.</response>
        /// <response code="404">Returns when no favorites are found.</response>
        /// <response code="401">Returns when the user is not authenticated.</response>
        /// <response code="500">Returns when an internal server error occurs.</response>
        [HttpGet("getFavoritesByUser")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse>> GetFavoritesByUser()
        {
            _logger.LogInformation("GetFavoritesByUser called");

            var user = await _userContext.GetCurrentUserAsync();
            if (user == null)
            {
                _logger.LogWarning("User not found in context");
                return Unauthorized(new ApiResponse
                {
                    IsSuccess = false,
                    Message = "User not found",
                    StatusCode = HttpStatusCode.Unauthorized
                });
            }

            _logger.LogDebug("Retrieving favorites for user: {UserId}", user.Id);
            var response = await _favoriteService.GetAllAsync(f => f.UserID == user.Id);
            if (response == null || !response.Items.Any())
            {
                _logger.LogDebug("No favorites found for user: {UserId}", user.Id);
                return NotFound(new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Favorites not found",
                    StatusCode = HttpStatusCode.NotFound
                });
            }

            _logger.LogInformation("Favorites retrieved successfully for user {UserId}, count: {Count}",
                user.Id, response.Items.Count());
            return Ok(new ApiResponse
            {
                IsSuccess = true,
                Message = "Favorites found successfully",
                StatusCode = HttpStatusCode.OK,
                Result = response
            });
        }

        /// <summary>
        /// Retrieves all favorites for a specific diagram.
        /// </summary>
        /// <param name="diagramId">The ID of the diagram to retrieve favorites for.</param>
        /// <returns>An <see cref="ApiResponse"/> containing the diagram's favorites.</returns>
        /// <response code="200">Returns when favorites are found.</response>
        /// <response code="404">Returns when no favorites are found.</response>
        /// <response code="500">Returns when an internal server error occurs.</response>
        [HttpGet("getFavoritesByDiagram/{diagramId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> GetFavoritesByDiagram(Guid diagramId)
        {
            _logger.LogInformation("GetFavoritesByDiagram called with diagramId: {DiagramId}", diagramId);

            var response = await _favoriteService.GetAllAsync(f => f.DiagramID == diagramId);
            if (response == null || !response.Items.Any())
            {
                _logger.LogDebug("No favorites found for diagramId: {DiagramId}", diagramId);
                return NotFound(new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Favorites not found",
                    StatusCode = HttpStatusCode.NotFound
                });
            }

            _logger.LogInformation("Favorites retrieved successfully for diagram {DiagramId}, count: {Count}",
                diagramId, response.Items.Count());
            return Ok(new ApiResponse
            {
                IsSuccess = true,
                Message = "Favorites found successfully",
                StatusCode = HttpStatusCode.OK,
                Result = response
            });
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindMapGenerator.Core.Domain.RepositoryContract;
using MindMapGenerator.Core.Dtos;
using MindMapGenerator.Core.Dtos.DiagramDto;
using MindMapGenerator.Core.ServiceContracts;
using System.Net;

namespace MindMapGenerator.API.Controllers
{
    /// <summary>
    /// Controller responsible for handling CRUD operations and retrieval of diagrams in the MindMapGenerator application.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DiagramController : ControllerBase
    {
        private readonly IDiagramService _diagramService;
        private readonly ILogger<DiagramController> _logger;
        private readonly IUserContext _userContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramController"/> class.
        /// </summary>
        /// <param name="diagramService">The service used to manage diagram-related operations.</param>
        /// <param name="logger">The logger instance for logging controller activities.</param>
        /// <param name="userContext">The user context to retrieve information about the current authenticated user.</param>
        public DiagramController(IDiagramService diagramService, ILogger<DiagramController> logger, IUserContext userContext)
        {
            _diagramService = diagramService;
            _logger = logger;
            _userContext = userContext;
        }

        /// <summary>
        /// Creates a new diagram based on the provided request data.
        /// </summary>
        /// <param name="request">The data required to create a new diagram.</param>
        /// <returns>An <see cref="ActionResult{T}"/> containing an <see cref="ApiResponse"/> with the result of the operation.</returns>
        /// <response code="200">Returns the created diagram details if successful.</response>
        /// <response code="400">Returned if the diagram creation fails.</response>
        /// <response code="500">Returned if an internal server error occurs.</response>
        [HttpPost("addDiagram")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> AddDiagram([FromBody] DiagramAddRequest request)
        {
            var response = await _diagramService.CreateAsync(request);
            if (response == null)
            {
                return BadRequest(new ApiResponse
                {
                    Message = "Failed to create diagram",
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false
                });
            }
            return Ok(new ApiResponse
            {
                Message = "Diagram created successfully",
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = response
            });
        }

        /// <summary>
        /// Updates an existing diagram with the provided request data.
        /// </summary>
        /// <param name="request">The data required to update an existing diagram.</param>
        /// <returns>An <see cref="ActionResult{T}"/> containing an <see cref="ApiResponse"/> with the result of the operation.</returns>
        /// <response code="200">Returns the updated diagram details if successful.</response>
        /// <response code="400">Returned if the diagram update fails.</response>
        /// <response code="500">Returned if an internal server error occurs.</response>
        [HttpPut("updateDiagram")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> UpdateDiagram([FromBody] DiagramUpdateRequest request)
        {
            var response = await _diagramService.UpdateAsync(request);
            if (response == null)
            {
                return BadRequest(new ApiResponse
                {
                    Message = "Failed to update diagram",
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false
                });
            }
            return Ok(new ApiResponse
            {
                Message = "Diagram updated successfully",
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = response
            });
        }

        /// <summary>
        /// Deletes a diagram by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the diagram to delete.</param>
        /// <returns>An <see cref="ActionResult{T}"/> containing an <see cref="ApiResponse"/> with the result of the operation.</returns>
        /// <response code="200">Returns a success message if the diagram is deleted.</response>
        /// <response code="400">Returned if the diagram deletion fails.</response>
        /// <response code="500">Returned if an internal server error occurs.</response>
        [HttpDelete("deleteDiagram/{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeleteDiagram(Guid id)
        {
            var response = await _diagramService.DeleteAsync(id);
            if (!response)
            {
                return BadRequest(new ApiResponse
                {
                    Message = "Failed to delete diagram",
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false
                });
            }
            return Ok(new ApiResponse
            {
                Message = "Diagram deleted successfully",
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true
            });
        }

        /// <summary>
        /// Retrieves a specific diagram by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the diagram to retrieve.</param>
        /// <returns>An <see cref="ActionResult{T}"/> containing an <see cref="ApiResponse"/> with the diagram details.</returns>
        /// <response code="200">Returns the requested diagram if found.</response>
        /// <response code="400">Returned if the diagram is not found or retrieval fails.</response>
        /// <response code="500">Returned if an internal server error occurs.</response>
        [HttpGet("getDiagram/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> GetDiagram(Guid id)
        {
            var response = await _diagramService.GetByAsync(x => x.DiagramID == id);
            if (response == null)
            {
                return BadRequest(new ApiResponse
                {
                    Message = "Failed to get diagram",
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false
                });
            }
            return Ok(new ApiResponse
            {
                Message = "Diagram retrieved successfully",
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = response
            });
        }

        /// <summary>
        /// Retrieves a paginated list of all diagrams.
        /// </summary>
        /// <param name="pagination">The pagination parameters (e.g., page number, page size).</param>
        /// <returns>An <see cref="ActionResult{T}"/> containing an <see cref="ApiResponse"/> with the list of diagrams.</returns>
        /// <response code="200">Returns the list of all diagrams if successful.</response>
        /// <response code="400">Returned if the retrieval fails.</response>
        /// <response code="500">Returned if an internal server error occurs.</response>
        [HttpGet("getAllDiagrams")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> GetAllDiagrams([FromQuery] PaginationDto pagination)
        {
            var response = await _diagramService.GetAllAsync(pagination: pagination);
            if (response == null)
            {
                return BadRequest(new ApiResponse
                {
                    Message = "Failed to get diagrams",
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false
                });
            }
            return Ok(new ApiResponse
            {
                Message = "Diagrams retrieved successfully",
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = response
            });
        }

        /// <summary>
        /// Retrieves a paginated list of public diagrams.
        /// </summary>
        /// <param name="pagination">The pagination parameters (e.g., page number, page size).</param>
        /// <returns>An <see cref="ActionResult{T}"/> containing an <see cref="ApiResponse"/> with the list of public diagrams.</returns>
        /// <response code="200">Returns the list of public diagrams if successful.</response>
        /// <response code="400">Returned if the retrieval fails.</response>
        /// <response code="500">Returned if an internal server error occurs.</response>
        [HttpGet("getPublicDiagrams")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> GetPublicDiagrams([FromQuery] PaginationDto pagination)
        {
            var response = await _diagramService.GetAllAsync(x => x.IsPublic, pagination);
            if (response == null)
            {
                return BadRequest(new ApiResponse
                {
                    Message = "Failed to get public diagrams",
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false
                });
            }
            return Ok(new ApiResponse
            {
                Message = "Public diagrams retrieved successfully",
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = response
            });
        }

        /// <summary>
        /// Retrieves a paginated list of diagrams belonging to the authenticated user.
        /// </summary>
        /// <param name="pagination">The pagination parameters (e.g., page number, page size).</param>
        /// <returns>An <see cref="ActionResult{T}"/> containing an <see cref="ApiResponse"/> with the list of user-specific diagrams.</returns>
        /// <response code="200">Returns the list of user diagrams if successful.</response>
        /// <response code="400">Returned if the retrieval fails.</response>
        /// <response code="401">Returned if the user is not authenticated.</response>
        /// <response code="500">Returned if an internal server error occurs.</response>
        [HttpGet("getUserDiagrams/{userID}")]
        //[Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> GetUserDiagrams(Guid userID,[FromQuery] PaginationDto pagination)
        {
            //var user = await _userContext.GetCurrentUserAsync();
            //if (user == null)
            //{
            //    return Unauthorized(new ApiResponse
            //    {
            //        Message = "user not authenticated",
            //        StatusCode = HttpStatusCode.Unauthorized,
            //        IsSuccess = false
            //    });
            //}
            var response = await _diagramService.GetAllAsync(x => x.UserID == userID, pagination);
            if (response == null)
            {
                return BadRequest(new ApiResponse
                {
                    Message = "Failed to get user diagrams",
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false
                });
            }
            return Ok(new ApiResponse
            {
                Message = "User diagrams retrieved successfully",
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = response
            });
        }
        /// <summary>
        /// Retrieves a list of diagrams whose titles contain the specified search string, ignoring case sensitivity.
        /// </summary>
        /// <param name="title">The string to search for within diagram titles. The search is case-insensitive.</param>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing an <see cref="ApiResponse"/> with the list of matching diagrams.
        /// </returns>
        /// <response code="200">Returns the list of diagrams whose titles match the search string if successful.</response>
        /// <response code="400">Returned if no diagrams are found or the retrieval fails.</response>
        /// <response code="500">Returned if an internal server error occurs during the operation.</response>
        /// <remarks>
        /// This method performs a case-insensitive search by converting both the diagram title and the search string to uppercase
        /// before checking for containment. If no diagrams match the criteria, a bad request response is returned.
        /// </remarks>
        [HttpGet("getDiagrams/{title}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> GetDiagrams(string title)
        {
            var response = await _diagramService.GetAllAsync(x => x.Title.ToUpper().Contains(title.ToUpper()) && x.IsPublic);
            if (response == null)
            {
                return BadRequest(new ApiResponse
                {
                    Message = "Failed to get diagrams",
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false
                });
            }
            return Ok(new ApiResponse
            {
                Message = "Diagrams retrieved successfully",
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = response
            });
        }
    }
    
}
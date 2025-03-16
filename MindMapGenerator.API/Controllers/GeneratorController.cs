using Microsoft.AspNetCore.Mvc;
using MindMapGenerator.Core.Dtos;
using MindMapGenerator.Core.HttpClients;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MindMapGenerator.Core.Dtos.ExternalDto;

namespace MindMapGenerator.API.Controllers
{
    /// <summary>
    /// Controller responsible for generating mind maps or text responses using external AI services (Gemini and DeepSeek).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GeneratorController : ControllerBase
    {
        private readonly IDeepSeekService _deepSeekService; // Service for interacting with DeepSeek API
        private readonly IGeminiService _geminiService;     // Service for interacting with Gemini API
        private readonly ILogger<GeneratorController> _logger; // Logger instance for logging events and errors

        /// <summary>
        /// Initializes a new instance of the GeneratorController with dependency injection.
        /// </summary>
        /// <param name="deepSeekService">The service to interact with DeepSeek API.</param>
        /// <param name="geminiService">The service to interact with Gemini API.</param>
        /// <param name="logger">The logger instance for logging controller events.</param>
        public GeneratorController(IDeepSeekService deepSeekService, IGeminiService geminiService, ILogger<GeneratorController> logger)
        {
            _deepSeekService = deepSeekService;
            _geminiService = geminiService;
            _logger = logger;
        }

        /// <summary>
        /// Generates a mind map or text response based on the provided prompt and model option.
        /// </summary>
        /// <param name="request">The request object containing the prompt and model option (GEMINI or DEEPSEEK).</param>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing an <see cref="ApiResponse"/> with the result or error details.
        /// Returns 200 OK on success, 400 BadRequest on invalid input or empty response.
        /// </returns>
        /// <remarks>
        /// This method logs warnings when an empty response is received from the selected model.
        /// It uses local validation and error handling instead of relying on a global exception handler.
        /// </remarks>
        [HttpPost("generate")]
        public async Task<ActionResult<ApiResponse>> Generate([FromBody] GenerateTextRequest request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse
                {
                    Message = "Request body cannot be null.",
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                });
            }

            string response = request.ModelOption switch
            {
                ModelOption.GEMINI => await _geminiService.GenerateMindMapAsync(request.Prompt),
                ModelOption.DEEPSEEK => await _deepSeekService.GetDeepSeekResponseAsync(request.Prompt),
                _ => throw new ArgumentException("Invalid model option") 
            };

            if (string.IsNullOrWhiteSpace(response) || response == "{}")
            {
                _logger.LogWarning("Empty response received from {Model}", request.ModelOption);

                return BadRequest(new ApiResponse
                {
                    Message = $"No results from {request.ModelOption}.",
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                });
            }

            return Ok(new ApiResponse
            {
                Message = $"{request.ModelOption} response received successfully.",
                IsSuccess = true,
                Result = JsonSerializer.Deserialize<object>(response, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true 
                }),
                StatusCode = HttpStatusCode.OK
            });
        }
    }
}
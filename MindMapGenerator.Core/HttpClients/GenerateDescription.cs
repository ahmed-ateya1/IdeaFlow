
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MindMapGenerator.Core.Helper;
using Mscc.GenerativeAI;

namespace MindMapGenerator.Core.HttpClients
{
    public class GenerateDescription : IGenerateDescription
    {
        private readonly ILogger<GenerateDescription> _logger;
        private readonly string _apiKey;
        private readonly IConfiguration _configuration;
        private const string ModelName = "gemini-2.0-flash";

        public GenerateDescription(ILogger<GenerateDescription> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _apiKey = _configuration["Gemini:ApiKey"] ?? throw new ArgumentNullException(nameof(_apiKey), "API Key is missing in configuration.");
        }
        public async Task<string> GenerateDescriptionAsync(string prompt)
        {
            var model = new GenerativeModel
            {
                ApiKey = _apiKey,
                Model = ModelName
            };

            string message = GenerateMessageHelper.GenerateDescription(prompt);
            _logger.LogDebug("Generated prompt message: {Message}", message);

            var response = await model.GenerateContent(message);
            _logger.LogInformation("Received response from Gemini API");

            if (response?.Candidates == null || response.Candidates.Count == 0)
            {
                _logger.LogWarning("No candidates returned from Gemini API for prompt: {Prompt}", prompt);
                return null;
            }

            string rawResult = response.Candidates[0].Content.Parts[0].Text;
            _logger.LogDebug("Raw response text: {RawResult}", rawResult);

            string cleanedResult = CleanResponseText(rawResult);
            return cleanedResult;
        }

        private string CleanResponseText(string text)
        {
            return text.Replace("\n", " ").Replace("\r", " ").Trim();
        }

    }
}

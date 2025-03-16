using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MindMapGenerator.Core.Helper;
using Mscc.GenerativeAI;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MindMapGenerator.Core.HttpClients
{
    public class GeminiService : IGeminiService
    {
        private readonly ILogger<GeminiService> _logger;
        private readonly string _apiKey;
        private readonly IConfiguration _configuration;

        private const string ModelName = "gemini-2.0-flash";

        public GeminiService(ILogger<GeminiService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _apiKey = _configuration["Gemini:ApiKey"] ?? throw new ArgumentNullException(nameof(_apiKey), "API Key is missing in configuration.");
        }

        public async Task<string> GenerateMindMapAsync(string prompt)
        {
            _logger.LogInformation("Starting mind map generation for prompt: {Prompt}", prompt);

            try
            {
                var model = new GenerativeModel
                {
                    ApiKey = _apiKey,
                    Model = ModelName
                };

                string message = GenerateMessageHelper.GenerateMessage(prompt);
                _logger.LogDebug("Generated prompt message: {Message}", message);

                var response = await model.GenerateContent(message);
                _logger.LogInformation("Received response from Gemini API");

                if (response?.Candidates == null || response.Candidates.Count == 0)
                {
                    _logger.LogWarning("No candidates returned from Gemini API for prompt: {Prompt}", prompt);
                    return "{}";
                }

                string rawResult = response.Candidates[0].Content.Parts[0].Text;
                _logger.LogDebug("Raw response text: {RawResult}", rawResult);

                string extractedJson = ExtractJson(rawResult);
                _logger.LogDebug("Extracted JSON: {ExtractedJson}", extractedJson);

                try
                {
                    using JsonDocument doc = JsonDocument.Parse(extractedJson);
                    if (doc.RootElement.TryGetProperty("nodes", out JsonElement nodes) &&
                        doc.RootElement.TryGetProperty("edges", out JsonElement edges))
                    {
                        var cleanedResult = new
                        {
                            nodes = nodes.Clone(),
                            edges = edges.Clone()
                        };

                        string jsonResponse = JsonSerializer.Serialize(cleanedResult);
                        _logger.LogInformation("Successfully generated mind map JSON");

                        return jsonResponse;
                    }
                    else
                    {
                        _logger.LogWarning("Extracted JSON does not contain expected 'nodes' and 'edges' properties.");
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Invalid JSON format from Gemini response: {ExtractedJson}", extractedJson);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating mind map for prompt: {Prompt}", prompt);
            }

            _logger.LogWarning("Returning empty JSON due to errors.");
            return "{}";
        }

        private string ExtractJson(string rawText)
        {
            _logger.LogDebug("Attempting to extract JSON from raw text.");

            string jsonPattern = "```json\\n(.*?)\\n```";
            Match match = Regex.Match(rawText, jsonPattern, RegexOptions.Singleline);

            if (match.Success)
            {
                _logger.LogDebug("Successfully extracted JSON content.");
                return match.Groups[1].Value;
            }

            _logger.LogWarning("No JSON content found in the raw response.");
            return "{}";
        }
    }
}

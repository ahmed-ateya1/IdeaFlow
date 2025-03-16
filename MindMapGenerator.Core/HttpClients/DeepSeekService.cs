using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MindMapGenerator.Core.Dtos.ExternalDto;
using MindMapGenerator.Core.Helper;
using Polly;
using Polly.Extensions.Http;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MindMapGenerator.Core.HttpClients
{
    public class DeepSeekService : IDeepSeekService
    {
        private readonly HttpClient _httpClient;
        private readonly DeepSeekSettings _settings;
        private readonly ILogger<DeepSeekService> _logger;

        public DeepSeekService(HttpClient httpClient, IOptions<DeepSeekSettings> settings, ILogger<DeepSeekService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
            _logger = logger;
        }
        public async Task<string> GetDeepSeekResponseAsync(string prompt)
        {
            string message = GenerateMessageHelper.GenerateMessage(prompt);

            var requestBody = new
            {
                model = "deepseek/deepseek-chat:free",
                messages = new[] { new { role = "user", content = message } }
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

            var endpoint = $"{_settings.BaseUrl}chat/completions";
            _logger.LogInformation("Sending request to OpenRouter API: {Endpoint}", endpoint);

            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(2));

            var stopwatch = Stopwatch.StartNew();
            var response = await retryPolicy.ExecuteAsync(() => _httpClient.PostAsync(endpoint, content));
            stopwatch.Stop();
            _logger.LogInformation("API response time: {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("API error: {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                throw new Exception($"API error: {response.StatusCode}, Content: {errorContent}");
            }

            using var responseStream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(responseStream, Encoding.UTF8);
            string responseContent = await reader.ReadToEndAsync();

            _logger.LogInformation("Raw API response: {ResponseContent}", responseContent);

            if (!responseContent.Trim().StartsWith("{"))
            {
                _logger.LogError("Invalid JSON response: {ResponseContent}", responseContent);
                throw new Exception("Invalid JSON response from API.");
            }

            try
            {
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var generatedContent = jsonResponse.GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                _logger.LogInformation("Generated Content: {GeneratedContent}", generatedContent);

                if (!string.IsNullOrWhiteSpace(generatedContent) && generatedContent.StartsWith("{"))
                {
                    return generatedContent;
                }
                return JsonSerializer.Deserialize<string>(generatedContent);
            }
            catch (JsonException ex)
            {
                _logger.LogError("JSON deserialization error: {Message}", ex.Message);
                throw new Exception("Failed to deserialize API response.", ex);
            }
        }
    }
    
}
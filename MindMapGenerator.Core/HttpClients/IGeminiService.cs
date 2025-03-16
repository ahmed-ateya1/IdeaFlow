namespace MindMapGenerator.Core.HttpClients
{
    public interface IGeminiService
    {
        Task<string> GenerateMindMapAsync(string prompt);
    }
}

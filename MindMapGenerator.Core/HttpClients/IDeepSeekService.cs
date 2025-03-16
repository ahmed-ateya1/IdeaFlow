using System.Text.Json;

namespace MindMapGenerator.Core.HttpClients
{
    public interface IDeepSeekService
    {
        Task<string> GetDeepSeekResponseAsync(string prompt);
    }
}
namespace MindMapGenerator.Core.HttpClients
{
    public interface IGenerateDescription
    {
        Task<string> GenerateDescriptionAsync(string prompt);
    }
}

using System.ClientModel.Primitives;

namespace MindMapGenerator.Core.Dtos.ExternalDto
{
    public class GenerateTextRequest
    {
        public string Prompt { get; set; }
        public ModelOption ModelOption { get; set; } = ModelOption.GEMINI;
    }
}

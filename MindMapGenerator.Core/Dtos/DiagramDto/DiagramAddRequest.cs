namespace MindMapGenerator.Core.Dtos.DiagramDto
{
    public class DiagramAddRequest
    {
        public string Title { get; set; }
        public string ContentJson { get; set; }
        public bool IsPublic { get; set; } = false;
    }
}

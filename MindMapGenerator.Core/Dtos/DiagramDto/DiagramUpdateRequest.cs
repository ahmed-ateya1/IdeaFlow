namespace MindMapGenerator.Core.Dtos.DiagramDto
{
    public class DiagramUpdateRequest
    {
        public Guid DiagramID { get; set; }
        public string Title { get; set; }
        public string ContentJson { get; set; }
        public bool IsPublic { get; set; }
    }
}

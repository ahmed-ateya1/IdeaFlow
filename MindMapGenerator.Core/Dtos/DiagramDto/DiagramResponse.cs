namespace MindMapGenerator.Core.Dtos.DiagramDto
{
    public class DiagramResponse
    {
        public Guid DiagramID { get; set; }
        public string Title { get; set; }
        public string ContentJson { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsPublic { get; set; }
        public Guid UserID { get; set; }
        public string FullName { get; set; }
    }
}

namespace MindMapGenerator.Core.Dtos.DiagramDto
{
    public class DiagramResponse
    {
        public Guid DiagramID { get; set; }
        public string Title { get; set; }
        public string ContentJson { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsPublic { get; set; }
        public Guid UserID { get; set; }

        public Guid? BaseDiagramID { get; set; }
        public string? BaseDiagramTitle { get; set; }
        public string? BaseDiagramContentJson { get; set; }
        public string FullName { get; set; }
        public long NumberOfFavorites { get; set; }
        public bool IsInFavorite { get; set; }
        public bool IsClone { get; set; }
    }
}

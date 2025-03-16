namespace MindMapGenerator.Core.Dtos.FavoriteDto
{
    public class FavoriteUpdateRequest
    {
        public Guid FavoriteID { get; set; }
        public Guid DiagramID { get; set; }
    }
}

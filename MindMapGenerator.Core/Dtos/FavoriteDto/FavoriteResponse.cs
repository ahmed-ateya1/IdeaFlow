namespace MindMapGenerator.Core.Dtos.FavoriteDto
{
    public class FavoriteResponse
    {
        public Guid FavouriteID { get; set; }
        public Guid UserID { get; set; }
        public string FullName { get; set; }
        public Guid DiagramID { get; set; }
        public string Title { get; set; }
        public string ContentJson { get; set; }

    }
}

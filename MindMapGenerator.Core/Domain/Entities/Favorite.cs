using MindMapGenerator.Core.Domain.IdentityEntities;

namespace MindMapGenerator.Core.Domain.Entities
{
    public class Favorite
    {
        public Guid FavouriteID { get; set; }
        public Guid UserID { get; set; }
        public ApplicationUser User { get; set; }
        public Guid DiagramID { get; set; }
        public Diagram Diagram { get; set; }
    }
}

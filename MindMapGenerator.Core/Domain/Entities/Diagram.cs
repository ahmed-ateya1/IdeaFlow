using MindMapGenerator.Core.Domain.IdentityEntities;

namespace MindMapGenerator.Core.Domain.Entities
{
    public class Diagram
    {
        public Guid DiagramID { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string ContentJson { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsPublic { get; set; }
        public Guid? BaseDiagramID { get; set; }
        public virtual Diagram BaseDiagram { get; set; }
        public virtual ICollection<Diagram>? DerivedDiagrams { get; set; } = [];
        public Guid UserID { get; set; }
        public virtual ApplicationUser User { get; set; } 
        public virtual ICollection<Favorite> Favorites { get; set; } = [];
    }
}

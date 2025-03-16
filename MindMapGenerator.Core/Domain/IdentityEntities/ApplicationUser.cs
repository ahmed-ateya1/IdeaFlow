using Microsoft.AspNetCore.Identity;
using MindMapGenerator.Core.Domain.Entities;

namespace MindMapGenerator.Core.Domain.IdentityEntities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? OTPCode { get; set; }
        public DateTime? OTPExpiration { get; set; }
        public string FullName { get; set; }
        public virtual ICollection<Diagram> Diagrams { get; set; } = [];

        public virtual ICollection<Favorite> Favorites { get; set; } = [];
        public virtual ICollection<RefreshToken>? RefreshTokens { get; set; }


    }
}

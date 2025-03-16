using Microsoft.EntityFrameworkCore;

namespace MindMapGenerator.Core.Domain.IdentityEntities
{
    [Owned]
    public class RefreshToken
    {
        public string Token { get; set; }
        public DateTime ExpiredOn { get; set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiredOn;
        public DateTime CreatedOn { get; set; }
        public DateTime? RevokedOn { get; set; }
        public bool IsActive => !RevokedOn.HasValue && !IsExpired;
    }
}

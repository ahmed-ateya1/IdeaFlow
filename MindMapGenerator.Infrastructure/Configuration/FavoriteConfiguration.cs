using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MindMapGenerator.Core.Domain.Entities;

namespace MindMapGenerator.Infrastructure.Configuration
{
    public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
    {
        public void Configure(EntityTypeBuilder<Favorite> builder)
        {
            builder.HasKey(f => f.FavouriteID);

            builder.Property(f => f.FavouriteID)
                .ValueGeneratedNever();

            builder.HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserID)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(f => f.Diagram)
                .WithMany(m => m.Favorites)
                .HasForeignKey(f => f.DiagramID)
                .OnDelete(DeleteBehavior.NoAction);

            builder.ToTable("Favorites");
        }
    }
}

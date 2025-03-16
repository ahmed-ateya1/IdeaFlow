using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MindMapGenerator.Core.Domain.Entities;

namespace MindMapGenerator.Infrastructure.Configuration
{
    public class DiagramConfiguration : IEntityTypeConfiguration<Diagram>
    {
        public void Configure(EntityTypeBuilder<Diagram> builder)
        {
            builder.HasKey(x=>x.DiagramID);

            builder.Property(x => x.DiagramID)
                .ValueGeneratedNever();

            builder.Property(x => x.Title)
                .IsRequired();

            builder.Property(x=>x.IsPublic)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            builder.Property(x=>x.ContentJson)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.Diagrams)
                .HasForeignKey(x => x.UserID)
                .OnDelete(DeleteBehavior.Cascade);


            builder.ToTable("Diagrams");
        }
    }
}

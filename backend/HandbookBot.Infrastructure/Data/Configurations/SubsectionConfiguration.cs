using HandbookBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HandbookBot.Infrastructure.Data.Configurations;

internal sealed class SubsectionConfiguration : IEntityTypeConfiguration<Subsection>
{
    public void Configure(EntityTypeBuilder<Subsection> builder)
    {
        builder.ToTable("subsections");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.SectionId).HasColumnName("section_id").IsRequired();
        builder.Property(s => s.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(s => s.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(s => s.Order).HasColumnName("order").HasDefaultValue(0);
        builder.Property(s => s.CreatedAt).HasColumnName("created_at");
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");

        builder.HasMany(s => s.Topics).WithOne().HasForeignKey(t => t.SubsectionId).OnDelete(DeleteBehavior.Cascade);
    }
}

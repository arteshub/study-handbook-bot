using HandbookBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HandbookBot.Infrastructure.Data.Configurations;

internal sealed class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.ToTable("topics");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.SubsectionId).HasColumnName("subsection_id").IsRequired();
        builder.Property(t => t.Title).HasColumnName("title").HasMaxLength(300).IsRequired();
        builder.Property(t => t.Content).HasColumnName("content").IsRequired();
        builder.Property(t => t.Summary).HasColumnName("summary").HasMaxLength(500);
        builder.Property(t => t.Order).HasColumnName("order").HasDefaultValue(0);
        builder.Property(t => t.CreatedAt).HasColumnName("created_at");
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at");
    }
}

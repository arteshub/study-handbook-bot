using HandbookBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HandbookBot.Infrastructure.Data.Configurations;

internal sealed class TestResultConfiguration : IEntityTypeConfiguration<TestResult>
{
    public void Configure(EntityTypeBuilder<TestResult> builder)
    {
        builder.ToTable("test_results");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.SessionId).HasColumnName("session_id").IsRequired();
        builder.Property(r => r.TopicId).HasColumnName("topic_id").IsRequired();
        builder.Property(r => r.Question).HasColumnName("question").IsRequired();
        builder.Property(r => r.CorrectAnswer).HasColumnName("correct_answer").IsRequired();
        builder.Property(r => r.UserAnswer).HasColumnName("user_answer");
        builder.Property(r => r.IsCorrect).HasColumnName("is_correct");
        builder.Property(r => r.AiFeedback).HasColumnName("ai_feedback");
        builder.Property(r => r.Order).HasColumnName("order");
        builder.Property(r => r.CreatedAt).HasColumnName("created_at");
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at");
    }
}

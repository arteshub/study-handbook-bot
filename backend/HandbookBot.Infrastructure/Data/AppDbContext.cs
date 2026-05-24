using System.Reflection;
using HandbookBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HandbookBot.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<Subsection> Subsections => Set<Subsection>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<TestSession> TestSessions => Set<TestSession>();
    public DbSet<TestResult> TestResults => Set<TestResult>();

    protected override void OnModelCreating(ModelBuilder builder) =>
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
}

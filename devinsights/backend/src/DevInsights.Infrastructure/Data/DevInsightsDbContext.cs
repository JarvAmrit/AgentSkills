using DevInsights.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DevInsights.Infrastructure.Data;

public class DevInsightsDbContext : DbContext
{
    public DevInsightsDbContext(DbContextOptions<DevInsightsDbContext> options) : base(options) { }

    public DbSet<Core.Models.Repository> Repositories => Set<Core.Models.Repository>();
    public DbSet<Developer> Developers => Set<Developer>();
    public DbSet<CommitAnalysis> CommitAnalyses => Set<CommitAnalysis>();
    public DbSet<AnalysisRun> AnalysisRuns => Set<AnalysisRun>();
    public DbSet<TechnologySummary> TechnologySummaries => Set<TechnologySummary>();
    public DbSet<AIWorkSummary> AIWorkSummaries => Set<AIWorkSummary>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Core.Models.Repository>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.AzDoOrganization, e.AzDoProject, e.RepoName }).IsUnique();
        });

        modelBuilder.Entity<Developer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AzDoId).IsUnique();
        });

        modelBuilder.Entity<CommitAnalysis>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CommitId).IsUnique();
            entity.HasOne(e => e.Repository).WithMany().HasForeignKey(e => e.RepositoryId);
            entity.HasOne(e => e.Developer).WithMany().HasForeignKey(e => e.DeveloperId);
        });

        modelBuilder.Entity<AnalysisRun>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Repository).WithMany().HasForeignKey(e => e.RepositoryId);
        });

        modelBuilder.Entity<TechnologySummary>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Developer).WithMany().HasForeignKey(e => e.DeveloperId);
            entity.HasOne(e => e.Repository).WithMany().HasForeignKey(e => e.RepositoryId);
        });

        modelBuilder.Entity<AIWorkSummary>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Developer).WithMany().HasForeignKey(e => e.DeveloperId);
            entity.HasOne(e => e.Repository).WithMany().HasForeignKey(e => e.RepositoryId);
        });
    }
}

public class DevInsightsDbContextFactory : Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory<DevInsightsDbContext>
{
    public DevInsightsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DevInsightsDbContext>();
        optionsBuilder.UseSqlite("Data Source=devinsights.db");
        return new DevInsightsDbContext(optionsBuilder.Options);
    }
}

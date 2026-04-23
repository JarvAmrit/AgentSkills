using DevInsights.Core.Interfaces;
using DevInsights.Core.Models;
using DevInsights.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DevInsights.Infrastructure.Repositories;

public class AnalysisRepository : IAnalysisRepository
{
    private readonly DevInsightsDbContext _context;
    private readonly ILogger<AnalysisRepository> _logger;

    public AnalysisRepository(DevInsightsDbContext context, ILogger<AnalysisRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Core.Models.Repository?> GetRepositoryAsync(string org, string project, string repoName, CancellationToken cancellationToken = default)
        => await _context.Repositories.FirstOrDefaultAsync(r => r.AzDoOrganization == org && r.AzDoProject == project && r.RepoName == repoName, cancellationToken);

    public async Task<Core.Models.Repository> UpsertRepositoryAsync(Core.Models.Repository repository, CancellationToken cancellationToken = default)
    {
        var existing = await GetRepositoryAsync(repository.AzDoOrganization, repository.AzDoProject, repository.RepoName, cancellationToken);
        if (existing is null)
        {
            _context.Repositories.Add(repository);
            await _context.SaveChangesAsync(cancellationToken);
            return repository;
        }
        return existing;
    }

    public async Task<Developer> UpsertDeveloperAsync(Developer developer, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Developers.FirstOrDefaultAsync(d => d.AzDoId == developer.AzDoId, cancellationToken);
        if (existing is null)
        {
            _context.Developers.Add(developer);
            await _context.SaveChangesAsync(cancellationToken);
            return developer;
        }
        if (existing.DisplayName != developer.DisplayName || existing.Email != developer.Email)
        {
            existing.DisplayName = developer.DisplayName;
            existing.Email = developer.Email;
            await _context.SaveChangesAsync(cancellationToken);
        }
        return existing;
    }

    public async Task<bool> CommitAnalysisExistsAsync(string commitId, CancellationToken cancellationToken = default)
        => await _context.CommitAnalyses.AnyAsync(c => c.CommitId == commitId, cancellationToken);

    public async Task SaveCommitAnalysisAsync(CommitAnalysis analysis, CancellationToken cancellationToken = default)
    {
        _context.CommitAnalyses.Add(analysis);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateRepositorySyncTimeAsync(int repositoryId, DateTime syncedAt, CancellationToken cancellationToken = default)
    {
        var repo = await _context.Repositories.FindAsync(new object[] { repositoryId }, cancellationToken);
        if (repo is not null)
        {
            repo.LastSyncedAt = syncedAt;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<AnalysisRun> CreateAnalysisRunAsync(AnalysisRun run, CancellationToken cancellationToken = default)
    {
        _context.AnalysisRuns.Add(run);
        await _context.SaveChangesAsync(cancellationToken);
        return run;
    }

    public async Task UpdateAnalysisRunAsync(AnalysisRun run, CancellationToken cancellationToken = default)
    {
        _context.AnalysisRuns.Update(run);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Core.Models.Repository>> GetAllRepositoriesAsync(CancellationToken cancellationToken = default)
        => await _context.Repositories.OrderBy(r => r.RepoName).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Developer>> GetAllDevelopersAsync(CancellationToken cancellationToken = default)
        => await _context.Developers.OrderBy(d => d.DisplayName).ToListAsync(cancellationToken);

    public async Task<IEnumerable<CommitAnalysis>> GetCommitsByDeveloperAsync(int developerId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        => await _context.CommitAnalyses
            .Include(c => c.Repository)
            .Where(c => c.DeveloperId == developerId && c.CommitDate >= from && c.CommitDate <= to)
            .OrderByDescending(c => c.CommitDate)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<AnalysisRun>> GetAnalysisRunsAsync(CancellationToken cancellationToken = default)
        => await _context.AnalysisRuns
            .Include(r => r.Repository)
            .OrderByDescending(r => r.StartedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

    public async Task RefreshSummaryTablesAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-90);
        var commits = await _context.CommitAnalyses
            .Where(c => c.CommitDate >= cutoff)
            .ToListAsync(cancellationToken);

        var techGroups = commits
            .SelectMany(c =>
            {
                var techs = System.Text.Json.JsonSerializer.Deserialize<List<string>>(c.TechnologiesDetected) ?? new List<string>();
                return techs.Select(t => new { c.DeveloperId, c.RepositoryId, Technology = t });
            })
            .GroupBy(x => new { x.DeveloperId, x.RepositoryId, x.Technology })
            .Select(g => new TechnologySummary
            {
                DeveloperId = g.Key.DeveloperId,
                RepositoryId = g.Key.RepositoryId,
                Technology = g.Key.Technology,
                CommitCount = g.Count(),
                LastUpdated = DateTime.UtcNow,
                PeriodStart = cutoff,
                PeriodEnd = DateTime.UtcNow
            });

        var existingTech = await _context.TechnologySummaries.Where(t => t.PeriodStart >= cutoff).ToListAsync(cancellationToken);
        _context.TechnologySummaries.RemoveRange(existingTech);
        await _context.TechnologySummaries.AddRangeAsync(techGroups, cancellationToken);

        var aiGroups = commits
            .Where(c => c.IsAIRelatedWork)
            .GroupBy(c => new { c.DeveloperId, c.RepositoryId, AIWorkType = c.AIWorkDescription ?? "General AI Work" })
            .Select(g => new AIWorkSummary
            {
                DeveloperId = g.Key.DeveloperId,
                RepositoryId = g.Key.RepositoryId,
                AIWorkType = g.Key.AIWorkType,
                CommitCount = g.Count(),
                LastUpdated = DateTime.UtcNow,
                PeriodStart = cutoff,
                PeriodEnd = DateTime.UtcNow
            });

        var existingAI = await _context.AIWorkSummaries.Where(a => a.PeriodStart >= cutoff).ToListAsync(cancellationToken);
        _context.AIWorkSummaries.RemoveRange(existingAI);
        await _context.AIWorkSummaries.AddRangeAsync(aiGroups, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }
}

using DevInsights.Core.Models;

namespace DevInsights.Core.Interfaces;

public interface IAnalysisRepository
{
    Task<Models.Repository?> GetRepositoryAsync(string org, string project, string repoName, CancellationToken cancellationToken = default);
    Task<Models.Repository> UpsertRepositoryAsync(Models.Repository repository, CancellationToken cancellationToken = default);
    Task<Developer> UpsertDeveloperAsync(Developer developer, CancellationToken cancellationToken = default);
    Task<bool> CommitAnalysisExistsAsync(string commitId, CancellationToken cancellationToken = default);
    Task SaveCommitAnalysisAsync(CommitAnalysis analysis, CancellationToken cancellationToken = default);
    Task UpdateRepositorySyncTimeAsync(int repositoryId, DateTime syncedAt, CancellationToken cancellationToken = default);
    Task<AnalysisRun> CreateAnalysisRunAsync(AnalysisRun run, CancellationToken cancellationToken = default);
    Task UpdateAnalysisRunAsync(AnalysisRun run, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.Repository>> GetAllRepositoriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Developer>> GetAllDevelopersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CommitAnalysis>> GetCommitsByDeveloperAsync(int developerId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IEnumerable<AnalysisRun>> GetAnalysisRunsAsync(CancellationToken cancellationToken = default);
    Task RefreshSummaryTablesAsync(CancellationToken cancellationToken = default);
}

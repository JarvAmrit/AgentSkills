using DevInsights.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace DevInsights.Infrastructure.Services;

public class AzureDevOpsService : IAzureDevOpsService
{
    private readonly string _pat;
    private readonly ILogger<AzureDevOpsService> _logger;

    public AzureDevOpsService(string pat, ILogger<AzureDevOpsService> logger)
    {
        _pat = pat;
        _logger = logger;
    }

    private VssConnection CreateConnection(string organization)
    {
        var orgUrl = new Uri($"https://dev.azure.com/{organization}");
        var credentials = new VssBasicCredential(string.Empty, _pat);
        return new VssConnection(orgUrl, credentials);
    }

    public async Task<IEnumerable<CommitInfo>> GetCommitsAsync(string organization, string project, string repoName, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = CreateConnection(organization);
            var gitClient = await connection.GetClientAsync<GitHttpClient>(cancellationToken);

            var searchCriteria = new GitQueryCommitsCriteria
            {
                FromDate = from.ToString("o"),
                ToDate = to.ToString("o"),
                ItemVersion = new GitVersionDescriptor { VersionType = GitVersionType.Branch, Version = "main" }
            };

            var commits = await gitClient.GetCommitsAsync(project, repoName, searchCriteria, cancellationToken: cancellationToken);

            return commits.Select(c => new CommitInfo
            {
                CommitId = c.CommitId,
                AuthorName = c.Author?.Name ?? "Unknown",
                AuthorEmail = c.Author?.Email ?? string.Empty,
                AuthorId = c.Author?.Email ?? string.Empty,
                CommitDate = c.Author?.Date ?? DateTime.UtcNow,
                Message = c.Comment ?? string.Empty
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get commits for {Org}/{Project}/{Repo}", organization, project, repoName);
            return Enumerable.Empty<CommitInfo>();
        }
    }

    public async Task<string> GetCommitDiffAsync(string organization, string project, string repoName, string commitId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = CreateConnection(organization);
            var gitClient = await connection.GetClientAsync<GitHttpClient>(cancellationToken);

            var changes = await gitClient.GetChangesAsync(commitId, repoName, project, cancellationToken: cancellationToken);

            var diffBuilder = new System.Text.StringBuilder();
            foreach (var change in changes.Changes ?? Enumerable.Empty<GitChange>())
            {
                diffBuilder.AppendLine($"[{change.ChangeType}] {change.Item?.Path}");
            }
            return diffBuilder.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get diff for commit {CommitId}", commitId);
            return string.Empty;
        }
    }
}

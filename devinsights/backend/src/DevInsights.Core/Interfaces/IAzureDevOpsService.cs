namespace DevInsights.Core.Interfaces;

public interface IAzureDevOpsService
{
    Task<IEnumerable<CommitInfo>> GetCommitsAsync(string organization, string project, string repoName, string branch, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<string> GetCommitDiffAsync(string organization, string project, string repoName, string commitId, CancellationToken cancellationToken = default);
}

public class CommitInfo
{
    public string CommitId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public DateTime CommitDate { get; set; }
    public string Message { get; set; } = string.Empty;
}

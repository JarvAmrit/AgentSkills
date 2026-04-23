namespace DevInsights.Core.Interfaces;

public interface ICommitAnalysisService
{
    Task<CommitAnalysisResult> AnalyzeCommitAsync(string commitId, string diff, string message, CancellationToken cancellationToken = default);
}

public class CommitAnalysisResult
{
    public List<string> Technologies { get; set; } = new();
    public bool IsAIRelatedWork { get; set; }
    public string? AIWorkDescription { get; set; }
    public double AIConfidenceScore { get; set; }
    public string? Summary { get; set; }
}

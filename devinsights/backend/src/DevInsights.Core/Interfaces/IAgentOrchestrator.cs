namespace DevInsights.Core.Interfaces;

public interface IAgentOrchestrator
{
    Task<CommitAnalysisResult> OrchestrateAsync(string commitId, string diff, string message, CancellationToken cancellationToken = default);
}

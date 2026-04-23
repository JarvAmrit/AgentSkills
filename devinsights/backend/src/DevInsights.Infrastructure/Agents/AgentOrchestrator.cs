using DevInsights.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace DevInsights.Infrastructure.Agents;

public class AgentOrchestrator : IAgentOrchestrator
{
    private readonly TechnologyClassifierAgent _techAgent;
    private readonly AIWorkClassifierAgent _aiAgent;
    private readonly CommitSummaryAgent _summaryAgent;
    private readonly ILogger<AgentOrchestrator> _logger;

    public AgentOrchestrator(
        TechnologyClassifierAgent techAgent,
        AIWorkClassifierAgent aiAgent,
        CommitSummaryAgent summaryAgent,
        ILogger<AgentOrchestrator> logger)
    {
        _techAgent = techAgent;
        _aiAgent = aiAgent;
        _summaryAgent = summaryAgent;
        _logger = logger;
    }

    public async Task<CommitAnalysisResult> OrchestrateAsync(string commitId, string diff, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Orchestrating analysis for commit {CommitId}", commitId);

        var techTask = _techAgent.ClassifyAsync(diff, message, cancellationToken);
        var aiTask = _aiAgent.ClassifyAsync(diff, message, cancellationToken);
        var summaryTask = _summaryAgent.SummarizeAsync(diff, message, cancellationToken);

        await Task.WhenAll(techTask, aiTask, summaryTask);

        var aiResult = await aiTask;
        return new CommitAnalysisResult
        {
            Technologies = await techTask,
            IsAIRelatedWork = aiResult.IsAIRelated,
            AIWorkDescription = aiResult.Description,
            AIConfidenceScore = aiResult.ConfidenceScore,
            Summary = await summaryTask
        };
    }
}

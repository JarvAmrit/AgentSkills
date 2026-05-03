using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace DevInsights.Infrastructure.Agents;

public class CommitSummaryAgent
{
    private readonly ChatClientAgent _agent;
    private readonly ILogger<CommitSummaryAgent> _logger;

    public CommitSummaryAgent(ChatClient chatClient, ILogger<CommitSummaryAgent> logger)
    {
        _agent = chatClient.AsAIAgent(
            instructions: "You are a commit summarizer. Create a concise 1-2 sentence summary of what the developer did in this commit. Focus on the business/technical impact.",
            name: "CommitSummarizer");
        _logger = logger;
    }

    public async Task<string> SummarizeAsync(string diff, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = $"Commit message: {message}\n\nDiff preview:\n{diff?.Substring(0, Math.Min(diff?.Length ?? 0, 2000))}";
            var response = await _agent.RunAsync(prompt, cancellationToken: cancellationToken);
            return response.Text ?? message;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Commit summary failed, using original message");
            return message;
        }
    }
}

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace DevInsights.Infrastructure.Agents;

public class CommitSummaryAgent
{
    private readonly Kernel _kernel;
    private readonly ILogger<CommitSummaryAgent> _logger;

    public CommitSummaryAgent(Kernel kernel, ILogger<CommitSummaryAgent> logger)
    {
        _kernel = kernel;
        _logger = logger;
    }

    public async Task<string> SummarizeAsync(string diff, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var chatService = _kernel.GetRequiredService<IChatCompletionService>();
            var history = new ChatHistory();
            history.AddSystemMessage("You are a commit summarizer. Create a concise 1-2 sentence summary of what the developer did in this commit. Focus on the business/technical impact.");
            history.AddUserMessage($"Commit message: {message}\n\nDiff preview:\n{diff?.Substring(0, Math.Min(diff?.Length ?? 0, 2000))}");

            var response = await chatService.GetChatMessageContentAsync(history, cancellationToken: cancellationToken);
            return response.Content ?? message;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Commit summary failed, using original message");
            return message;
        }
    }
}

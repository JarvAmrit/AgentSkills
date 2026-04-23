using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace DevInsights.Infrastructure.Agents;

public class TechnologyClassifierAgent
{
    private readonly Kernel _kernel;
    private readonly ILogger<TechnologyClassifierAgent> _logger;

    public TechnologyClassifierAgent(Kernel kernel, ILogger<TechnologyClassifierAgent> logger)
    {
        _kernel = kernel;
        _logger = logger;
    }

    public async Task<List<string>> ClassifyAsync(string diff, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var chatService = _kernel.GetRequiredService<IChatCompletionService>();
            var history = new ChatHistory();
            history.AddSystemMessage(@"You are a technology classifier. Analyze the commit diff and message to identify technologies used.
Return ONLY a JSON array of technology names. Examples: [""C#"", ""React"", ""TypeScript"", ""SQL"", ""Python"", ""Docker"", ""Kubernetes"", ""Azure"", ""AWS""].
Focus on: file extensions, imports, package references, framework usage, config files.");

            history.AddUserMessage($"Commit message: {message}\n\nDiff:\n{diff?.Substring(0, Math.Min(diff?.Length ?? 0, 3000))}");

            var response = await chatService.GetChatMessageContentAsync(history, cancellationToken: cancellationToken);
            var content = response.Content ?? "[]";

            var start = content.IndexOf('[');
            var end = content.LastIndexOf(']');
            if (start >= 0 && end > start)
            {
                content = content.Substring(start, end - start + 1);
            }

            return JsonSerializer.Deserialize<List<string>>(content) ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Technology classification failed, using heuristic fallback");
            return ClassifyByHeuristics(diff ?? string.Empty);
        }
    }

    private static List<string> ClassifyByHeuristics(string diff)
    {
        var technologies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (diff.Contains(".cs") || diff.Contains("using System")) technologies.Add("C#");
        if (diff.Contains(".tsx") || diff.Contains(".jsx") || diff.Contains("import React")) technologies.Add("React");
        if (diff.Contains(".ts") && !diff.Contains(".tsx")) technologies.Add("TypeScript");
        if (diff.Contains(".py") || diff.Contains("import ") && diff.Contains("def ")) technologies.Add("Python");
        if (diff.Contains(".sql") || diff.Contains("SELECT ") || diff.Contains("CREATE TABLE")) technologies.Add("SQL");
        if (diff.Contains("Dockerfile") || diff.Contains("docker-compose")) technologies.Add("Docker");
        if (diff.Contains(".java")) technologies.Add("Java");
        if (diff.Contains(".go") || diff.Contains("func main()")) technologies.Add("Go");
        return technologies.ToList();
    }
}

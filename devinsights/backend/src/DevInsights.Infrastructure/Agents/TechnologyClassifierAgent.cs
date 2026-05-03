using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using System.Text.Json;

namespace DevInsights.Infrastructure.Agents;

public class TechnologyClassifierAgent
{
    private readonly ChatClientAgent _agent;
    private readonly ILogger<TechnologyClassifierAgent> _logger;

    public TechnologyClassifierAgent(ChatClient chatClient, ILogger<TechnologyClassifierAgent> logger)
    {
        _agent = chatClient.AsAIAgent(
            instructions: @"You are a technology classifier. Analyze the commit diff and message to identify technologies used.
Return ONLY a JSON array of technology names. Examples: [""C#"", ""React"", ""TypeScript"", ""SQL"", ""Python"", ""Docker"", ""Kubernetes"", ""Azure"", ""AWS""].
Focus on: file extensions, imports, package references, framework usage, config files.",
            name: "TechnologyClassifier");
        _logger = logger;
    }

    public async Task<List<string>> ClassifyAsync(string diff, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = $"Commit message: {message}\n\nDiff:\n{diff?.Substring(0, Math.Min(diff?.Length ?? 0, 3000))}";
            var response = await _agent.RunAsync(prompt, cancellationToken: cancellationToken);
            var content = response.Text ?? "[]";

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

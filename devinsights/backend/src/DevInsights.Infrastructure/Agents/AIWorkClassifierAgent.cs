using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using System.Text.Json;

namespace DevInsights.Infrastructure.Agents;

public class AIWorkClassifierResult
{
    public bool IsAIRelated { get; set; }
    public string Description { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
}

public class AIWorkClassifierAgent
{
    private readonly ChatClientAgent _agent;
    private readonly ILogger<AIWorkClassifierAgent> _logger;

    public AIWorkClassifierAgent(ChatClient chatClient, ILogger<AIWorkClassifierAgent> logger)
    {
        _agent = chatClient.AsAIAgent(
            instructions: @"You are an AI work detector. Analyze commits to determine if they involve AI/LLM-related work.
AI-related indicators: Copilot usage patterns, AI library imports (OpenAI, LangChain, SemanticKernel, Hugging Face, etc.), 
prompt engineering files, model configurations, AI service integrations, embeddings, vector databases, AI-related comments/docs.
Return JSON: {""isAIRelated"": bool, ""description"": ""brief description or empty string"", ""confidenceScore"": 0.0-1.0}",
            name: "AIWorkClassifier");
        _logger = logger;
    }

    public async Task<AIWorkClassifierResult> ClassifyAsync(string diff, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = $"Commit message: {message}\n\nDiff:\n{diff?.Substring(0, Math.Min(diff?.Length ?? 0, 3000))}";
            var response = await _agent.RunAsync(prompt, cancellationToken: cancellationToken);
            var content = response.Text ?? "{}";

            var start = content.IndexOf('{');
            var end = content.LastIndexOf('}');
            if (start >= 0 && end > start)
            {
                content = content.Substring(start, end - start + 1);
            }

            return JsonSerializer.Deserialize<AIWorkClassifierResult>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new AIWorkClassifierResult();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI work classification failed, using heuristic fallback");
            return ClassifyByHeuristics(diff ?? string.Empty, message);
        }
    }

    private static AIWorkClassifierResult ClassifyByHeuristics(string diff, string message)
    {
        var aiKeywords = new[] { "openai", "gpt", "copilot", "llm", "embedding", "prompt", "semantic kernel", "langchain", "huggingface", "ai model", "vector", "chatgpt" };
        var combined = (diff + " " + message).ToLowerInvariant();
        var matchCount = aiKeywords.Count(k => combined.Contains(k));

        return new AIWorkClassifierResult
        {
            IsAIRelated = matchCount > 0,
            Description = matchCount > 0 ? "AI-related keywords detected" : string.Empty,
            ConfidenceScore = Math.Min(matchCount * 0.3, 1.0)
        };
    }
}

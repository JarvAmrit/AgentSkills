namespace DevInsights.Core.Models;

public class CommitAnalysis
{
    public int Id { get; set; }
    public string CommitId { get; set; } = string.Empty;
    public int RepositoryId { get; set; }
    public int DeveloperId { get; set; }
    public DateTime CommitDate { get; set; }
    public string Message { get; set; } = string.Empty;
    public string TechnologiesDetected { get; set; } = "[]";
    public bool IsAIRelatedWork { get; set; }
    public string? AIWorkDescription { get; set; }
    public double AIConfidenceScore { get; set; }
    public string? RawDiff { get; set; }
    public DateTime AnalyzedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Repository? Repository { get; set; }
    public Developer? Developer { get; set; }
}

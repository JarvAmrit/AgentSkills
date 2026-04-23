namespace DevInsights.Core.Models;

public class AnalysisRun
{
    public int Id { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "Running";
    public int RepositoryId { get; set; }
    public int CommitsAnalyzed { get; set; }
    public string? ErrorMessage { get; set; }
    public Repository? Repository { get; set; }
}

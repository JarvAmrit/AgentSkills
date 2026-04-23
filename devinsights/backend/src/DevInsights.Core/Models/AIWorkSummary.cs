namespace DevInsights.Core.Models;

public class AIWorkSummary
{
    public int Id { get; set; }
    public int DeveloperId { get; set; }
    public int RepositoryId { get; set; }
    public string AIWorkType { get; set; } = string.Empty;
    public int CommitCount { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public Developer? Developer { get; set; }
    public Repository? Repository { get; set; }
}

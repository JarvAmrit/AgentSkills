namespace DevInsights.API.DTOs;

public record AnalysisRunDto(int Id, string RepositoryName, DateTime StartedAt, DateTime? CompletedAt, string Status, int CommitsAnalyzed, string? ErrorMessage);

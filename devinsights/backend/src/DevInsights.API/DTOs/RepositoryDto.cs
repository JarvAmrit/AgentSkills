namespace DevInsights.API.DTOs;

public record RepositoryDto(int Id, string Organization, string Project, string RepoName, DateTime? LastSyncedAt, DateTime CreatedAt);

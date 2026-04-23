namespace DevInsights.API.DTOs;

public record DeveloperDto(int Id, string DisplayName, string Email, DateTime CreatedAt);
public record DeveloperDetailDto(int Id, string DisplayName, string Email, List<TechCommitDto> Technologies, List<AIWorkDto> AIWork, List<CommitDto> RecentCommits);
public record TechCommitDto(string Technology, int CommitCount);
public record AIWorkDto(string AIWorkType, int CommitCount, double AvgConfidence);
public record CommitDto(string CommitId, string Message, DateTime CommitDate, string RepositoryName, List<string> Technologies, bool IsAIRelated);

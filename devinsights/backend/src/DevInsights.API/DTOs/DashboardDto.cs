namespace DevInsights.API.DTOs;

public record DashboardSummaryDto(
    int TotalCommits,
    int TotalDevelopers,
    int TotalRepositories,
    double AIWorkPercentage,
    List<DeveloperActivityDto> TopDevelopers,
    List<TechDistributionDto> TechDistribution,
    List<DailyActivityDto> DailyActivity
);

public record DeveloperActivityDto(int Id, string DisplayName, string Email, int CommitCount, double AIWorkPercentage, List<string> TopTechnologies);
public record TechDistributionDto(string Technology, int CommitCount);
public record DailyActivityDto(string Date, int CommitCount, int AICommitCount);

using DevInsights.Core.Interfaces;
using DevInsights.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DevInsights.Infrastructure.BackgroundServices;

public class RepositoryConfig
{
    public string Organization { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public string RepoName { get; set; } = string.Empty;
}

public class CommitAnalysisBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CommitAnalysisBackgroundService> _logger;

    public CommitAnalysisBackgroundService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<CommitAnalysisBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CommitAnalysisBackgroundService starting");

        var intervalHours = _configuration.GetValue<int>("AnalysisSettings:ScheduleIntervalHours", 24);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunAnalysisAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Analysis run failed");
            }

            await Task.Delay(TimeSpan.FromHours(intervalHours), stoppingToken);
        }
    }

    private async Task RunAnalysisAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var azDoService = scope.ServiceProvider.GetRequiredService<IAzureDevOpsService>();
        var analysisRepo = scope.ServiceProvider.GetRequiredService<IAnalysisRepository>();
        var orchestrator = scope.ServiceProvider.GetRequiredService<IAgentOrchestrator>();

        var lookbackDays = _configuration.GetValue<int>("AnalysisSettings:LookbackDays", 90);
        var maxConcurrent = _configuration.GetValue<int>("AnalysisSettings:MaxConcurrentAnalyses", 5);
        var repoConfigs = _configuration.GetSection("AzureDevOps:Repositories").Get<List<RepositoryConfig>>() ?? new List<RepositoryConfig>();

        _logger.LogInformation("Starting analysis for {Count} repositories", repoConfigs.Count);

        foreach (var repoConfig in repoConfigs)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var repo = await analysisRepo.UpsertRepositoryAsync(new Core.Models.Repository
            {
                AzDoOrganization = repoConfig.Organization,
                AzDoProject = repoConfig.Project,
                RepoName = repoConfig.RepoName
            }, cancellationToken);

            var from = repo.LastSyncedAt?.AddMinutes(-5) ?? DateTime.UtcNow.AddDays(-lookbackDays);
            var to = DateTime.UtcNow;

            var run = await analysisRepo.CreateAnalysisRunAsync(new AnalysisRun
            {
                StartedAt = DateTime.UtcNow,
                Status = "Running",
                RepositoryId = repo.Id
            }, cancellationToken);

            try
            {
                var commits = (await azDoService.GetCommitsAsync(repoConfig.Organization, repoConfig.Project, repoConfig.RepoName, from, to, cancellationToken)).ToList();
                _logger.LogInformation("Found {Count} commits in {Repo}", commits.Count, repoConfig.RepoName);

                var semaphore = new SemaphoreSlim(maxConcurrent);
                var commitsAnalyzed = 0;
                var tasks = commits.Select(async commit =>
                {
                    await semaphore.WaitAsync(cancellationToken);
                    try
                    {
                        if (await analysisRepo.CommitAnalysisExistsAsync(commit.CommitId, cancellationToken)) return;

                        var developer = await analysisRepo.UpsertDeveloperAsync(new Developer
                        {
                            AzDoId = commit.AuthorId,
                            DisplayName = commit.AuthorName,
                            Email = commit.AuthorEmail
                        }, cancellationToken);

                        var diff = await azDoService.GetCommitDiffAsync(repoConfig.Organization, repoConfig.Project, repoConfig.RepoName, commit.CommitId, cancellationToken);
                        var result = await orchestrator.OrchestrateAsync(commit.CommitId, diff, commit.Message, cancellationToken);

                        await analysisRepo.SaveCommitAnalysisAsync(new CommitAnalysis
                        {
                            CommitId = commit.CommitId,
                            RepositoryId = repo.Id,
                            DeveloperId = developer.Id,
                            CommitDate = commit.CommitDate,
                            Message = commit.Message,
                            TechnologiesDetected = JsonSerializer.Serialize(result.Technologies),
                            IsAIRelatedWork = result.IsAIRelatedWork,
                            AIWorkDescription = result.AIWorkDescription,
                            AIConfidenceScore = result.AIConfidenceScore,
                            RawDiff = diff?.Length > 10000 ? diff.Substring(0, 10000) : diff,
                            AnalyzedAt = DateTime.UtcNow
                        }, cancellationToken);

                        Interlocked.Increment(ref commitsAnalyzed);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);

                run.CommitsAnalyzed = commitsAnalyzed;
                run.Status = "Completed";
                run.CompletedAt = DateTime.UtcNow;
                await analysisRepo.UpdateAnalysisRunAsync(run, cancellationToken);
                await analysisRepo.UpdateRepositorySyncTimeAsync(repo.Id, DateTime.UtcNow, cancellationToken);
                await analysisRepo.RefreshSummaryTablesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Analysis failed for {Repo}", repoConfig.RepoName);
                run.Status = "Failed";
                run.ErrorMessage = ex.Message;
                run.CompletedAt = DateTime.UtcNow;
                await analysisRepo.UpdateAnalysisRunAsync(run, cancellationToken);
            }
        }
    }
}

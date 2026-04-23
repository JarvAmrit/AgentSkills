using DevInsights.API.DTOs;
using DevInsights.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DevInsights.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IAnalysisRepository _repo;

    public DashboardController(IAnalysisRepository repo) => _repo = repo;

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var developers = (await _repo.GetAllDevelopersAsync(cancellationToken)).ToList();
        var repositories = (await _repo.GetAllRepositoriesAsync(cancellationToken)).ToList();
        var cutoff = DateTime.UtcNow.AddDays(-90);
        var to = DateTime.UtcNow;

        var allCommits = new List<Core.Models.CommitAnalysis>();
        foreach (var dev in developers)
        {
            allCommits.AddRange(await _repo.GetCommitsByDeveloperAsync(dev.Id, cutoff, to, cancellationToken));
        }

        var totalCommits = allCommits.Count;
        var aiCommits = allCommits.Count(c => c.IsAIRelatedWork);
        var aiPct = totalCommits > 0 ? (double)aiCommits / totalCommits * 100 : 0;

        var techDist = allCommits
            .SelectMany(c => JsonSerializer.Deserialize<List<string>>(c.TechnologiesDetected) ?? new List<string>())
            .GroupBy(t => t)
            .Select(g => new TechDistributionDto(g.Key, g.Count()))
            .OrderByDescending(t => t.CommitCount)
            .Take(15)
            .ToList();

        var topDevs = developers.Select(d =>
        {
            var devCommits = allCommits.Where(c => c.DeveloperId == d.Id).ToList();
            var techs = devCommits.SelectMany(c => JsonSerializer.Deserialize<List<string>>(c.TechnologiesDetected) ?? new List<string>())
                .GroupBy(t => t).OrderByDescending(g => g.Count()).Take(3).Select(g => g.Key).ToList();
            var devAiPct = devCommits.Count > 0 ? (double)devCommits.Count(c => c.IsAIRelatedWork) / devCommits.Count * 100 : 0;
            return new DeveloperActivityDto(d.Id, d.DisplayName, d.Email, devCommits.Count, devAiPct, techs);
        }).OrderByDescending(d => d.CommitCount).ToList();

        var dailyActivity = allCommits
            .GroupBy(c => c.CommitDate.Date)
            .OrderBy(g => g.Key)
            .Select(g => new DailyActivityDto(g.Key.ToString("yyyy-MM-dd"), g.Count(), g.Count(c => c.IsAIRelatedWork)))
            .ToList();

        return Ok(new DashboardSummaryDto(totalCommits, developers.Count, repositories.Count, aiPct, topDevs, techDist, dailyActivity));
    }

    [HttpGet("developer/{id}")]
    public async Task<ActionResult<DeveloperActivityDto>> GetDeveloperSummary(int id, CancellationToken cancellationToken)
    {
        var developers = await _repo.GetAllDevelopersAsync(cancellationToken);
        var dev = developers.FirstOrDefault(d => d.Id == id);
        if (dev is null) return NotFound();

        var commits = (await _repo.GetCommitsByDeveloperAsync(id, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow, cancellationToken)).ToList();
        var techs = commits.SelectMany(c => JsonSerializer.Deserialize<List<string>>(c.TechnologiesDetected) ?? new List<string>())
            .GroupBy(t => t).OrderByDescending(g => g.Count()).Take(5).Select(g => g.Key).ToList();
        var aiPct = commits.Count > 0 ? (double)commits.Count(c => c.IsAIRelatedWork) / commits.Count * 100 : 0;

        return Ok(new DeveloperActivityDto(dev.Id, dev.DisplayName, dev.Email, commits.Count, aiPct, techs));
    }
}

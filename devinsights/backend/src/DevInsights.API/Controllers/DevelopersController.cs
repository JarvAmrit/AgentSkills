using DevInsights.API.DTOs;
using DevInsights.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DevInsights.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DevelopersController : ControllerBase
{
    private readonly IAnalysisRepository _repo;

    public DevelopersController(IAnalysisRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeveloperDto>>> GetAll(CancellationToken cancellationToken)
    {
        var devs = await _repo.GetAllDevelopersAsync(cancellationToken);
        return Ok(devs.Select(d => new DeveloperDto(d.Id, d.DisplayName, d.Email, d.CreatedAt)));
    }

    [HttpGet("{id}/commits")]
    public async Task<ActionResult<IEnumerable<CommitDto>>> GetCommits(int id, [FromQuery] int days = 90, CancellationToken cancellationToken = default)
    {
        var commits = await _repo.GetCommitsByDeveloperAsync(id, DateTime.UtcNow.AddDays(-days), DateTime.UtcNow, cancellationToken);
        return Ok(commits.Select(c => new CommitDto(
            c.CommitId, c.Message, c.CommitDate,
            c.Repository?.RepoName ?? "Unknown",
            JsonSerializer.Deserialize<List<string>>(c.TechnologiesDetected) ?? new List<string>(),
            c.IsAIRelatedWork)));
    }

    [HttpGet("{id}/technologies")]
    public async Task<ActionResult<IEnumerable<TechCommitDto>>> GetTechnologies(int id, [FromQuery] int days = 90, CancellationToken cancellationToken = default)
    {
        var commits = await _repo.GetCommitsByDeveloperAsync(id, DateTime.UtcNow.AddDays(-days), DateTime.UtcNow, cancellationToken);
        var result = commits
            .SelectMany(c => JsonSerializer.Deserialize<List<string>>(c.TechnologiesDetected) ?? new List<string>())
            .GroupBy(t => t)
            .Select(g => new TechCommitDto(g.Key, g.Count()))
            .OrderByDescending(t => t.CommitCount)
            .ToList();
        return Ok(result);
    }

    [HttpGet("{id}/aiwork")]
    public async Task<ActionResult<IEnumerable<AIWorkDto>>> GetAIWork(int id, [FromQuery] int days = 90, CancellationToken cancellationToken = default)
    {
        var commits = await _repo.GetCommitsByDeveloperAsync(id, DateTime.UtcNow.AddDays(-days), DateTime.UtcNow, cancellationToken);
        var result = commits
            .Where(c => c.IsAIRelatedWork)
            .GroupBy(c => c.AIWorkDescription ?? "General AI Work")
            .Select(g => new AIWorkDto(g.Key, g.Count(), g.Average(c => c.AIConfidenceScore)))
            .OrderByDescending(a => a.CommitCount)
            .ToList();
        return Ok(result);
    }
}

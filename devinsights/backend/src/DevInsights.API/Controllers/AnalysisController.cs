using DevInsights.API.DTOs;
using DevInsights.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevInsights.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalysisController : ControllerBase
{
    private readonly IAnalysisRepository _repo;

    public AnalysisController(IAnalysisRepository repo) => _repo = repo;

    [HttpGet("runs")]
    public async Task<ActionResult<IEnumerable<AnalysisRunDto>>> GetRuns(CancellationToken cancellationToken)
    {
        var runs = await _repo.GetAnalysisRunsAsync(cancellationToken);
        return Ok(runs.Select(r => new AnalysisRunDto(
            r.Id,
            r.Repository?.RepoName ?? "Unknown",
            r.StartedAt,
            r.CompletedAt,
            r.Status,
            r.CommitsAnalyzed,
            r.ErrorMessage)));
    }

    [HttpPost("trigger")]
    public IActionResult TriggerAnalysis()
    {
        return Accepted(new { message = "Analysis will run on next scheduled cycle" });
    }
}

using DevInsights.API.DTOs;
using DevInsights.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevInsights.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RepositoriesController : ControllerBase
{
    private readonly IAnalysisRepository _repo;

    public RepositoriesController(IAnalysisRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RepositoryDto>>> GetAll(CancellationToken cancellationToken)
    {
        var repos = await _repo.GetAllRepositoriesAsync(cancellationToken);
        return Ok(repos.Select(r => new RepositoryDto(r.Id, r.AzDoOrganization, r.AzDoProject, r.RepoName, r.LastSyncedAt, r.CreatedAt)));
    }

    [HttpPost("sync")]
    public async Task<IActionResult> TriggerSync([FromBody] SyncRequest request, CancellationToken cancellationToken)
    {
        var repo = await _repo.UpsertRepositoryAsync(new Core.Models.Repository
        {
            AzDoOrganization = request.Organization,
            AzDoProject = request.Project,
            RepoName = request.RepoName
        }, cancellationToken);

        return Accepted(new { message = "Sync triggered", repositoryId = repo.Id });
    }
}

public record SyncRequest(string Organization, string Project, string RepoName);

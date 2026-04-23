namespace DevInsights.Core.Models;

public class Repository
{
    public int Id { get; set; }
    public string AzDoOrganization { get; set; } = string.Empty;
    public string AzDoProject { get; set; } = string.Empty;
    public string RepoName { get; set; } = string.Empty;
    public DateTime? LastSyncedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

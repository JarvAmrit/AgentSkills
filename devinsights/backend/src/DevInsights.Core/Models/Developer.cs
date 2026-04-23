namespace DevInsights.Core.Models;

public class Developer
{
    public int Id { get; set; }
    public string AzDoId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

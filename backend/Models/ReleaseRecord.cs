namespace backend.Models;

public sealed class ReleaseRecord
{
    public Guid Id { get; init; }
    public string GameName { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public string BuildVersion { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ReleaseNotes { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}

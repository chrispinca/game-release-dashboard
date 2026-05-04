namespace backend.Models;

public sealed class ReleaseQueryParameters
{
    public string? Environment { get; init; }
    public string? Status { get; init; }
}

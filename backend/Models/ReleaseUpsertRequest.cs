using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public sealed class ReleaseUpsertRequest
{
    [Required]
    [StringLength(100)]
    public string GameName { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string TeamName { get; init; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string BuildVersion { get; init; } = string.Empty;

    [Required]
    [RegularExpression("^(Dev|QA|Staging|Prod)$")]
    public string Environment { get; init; } = string.Empty;

    [Required]
    [RegularExpression("^(Pending|Success|Failed)$")]
    public string Status { get; init; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string ReleaseNotes { get; init; } = string.Empty;
}

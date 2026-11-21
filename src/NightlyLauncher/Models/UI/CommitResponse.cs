using System.Text.Json.Serialization;

namespace NightlyLauncher.Models.UI;

internal sealed record CommitResponse(
    [property: JsonPropertyName("sha")] string? Sha);

using System.Text.Json.Serialization;

namespace NightlyLauncher.Models;

internal sealed record CommitResponse(
    [property: JsonPropertyName("sha")] string? Sha);

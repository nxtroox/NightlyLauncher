using System.Text.Json.Serialization;
using NightlyLauncher.Core.Models;

namespace NightlyLauncher.RepositoryGenerator.Serialization;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    WriteIndented = true)]
[JsonSerializable(typeof(Repository))]
internal sealed partial class AppJsonContext : JsonSerializerContext {
}

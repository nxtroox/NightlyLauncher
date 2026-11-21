using System.Text.Json.Serialization;
using NightlyLauncher.Core.Models;
using NightlyLauncher.Models;

namespace NightlyLauncher.Serialization;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    WriteIndented = true)]
[JsonSerializable(typeof(Config))]
[JsonSerializable(typeof(List<InstalledApp>))]
[JsonSerializable(typeof(Repository))]
internal sealed partial class AppJsonContext : JsonSerializerContext {
}

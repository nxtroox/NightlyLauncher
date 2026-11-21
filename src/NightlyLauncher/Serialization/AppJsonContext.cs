using System.Text.Json.Serialization;
using NightlyLauncher.Core.Models;
using NightlyLauncher.Models;
using NightlyLauncher.Models.UI;

namespace NightlyLauncher.Serialization;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    WriteIndented = true)]
[JsonSerializable(typeof(CommitResponse))]
[JsonSerializable(typeof(Config))]
[JsonSerializable(typeof(List<InstalledApp>))]
[JsonSerializable(typeof(Repository))]
[JsonSerializable(typeof(List<Repository>))]
internal sealed partial class AppJsonContext : JsonSerializerContext {
}

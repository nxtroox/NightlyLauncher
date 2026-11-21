using System.Runtime.InteropServices;
using NightlyLauncher.Core.Models;

namespace NightlyLauncher.RepositoryGenerator.Repositories;

internal sealed class EmulationRepositoryGenerator : IRepositoryGenerator {
    public string RepositoryFileName => "emulation.json";

    public Repository GenerateRepository() {
        var appManifests = new List<AppManifest> {
            new(Guid.Parse("860fe852-e59a-459c-af6c-789faa8533a5"),
                "Cemu",
                [
                    new InstallOption(Guid.Parse("2194f0bf-26d1-4c19-9cb8-5a1a5cf120e5"),
                        new Uri(
                            "https://nightly.link/cemu-project/Cemu/workflows/build_check/main/cemu-bin-windows-x64.zip"),
                        new Uri("https://api.github.com/repos/cemu-project/Cemu/commits/main"),
                        "Nightly (Windows (x64))",
                        new Platform(OsType.Windows,
                            Architecture.X64),
                        new AppOptions(new LaunchOptions("Cemu.exe"),
                            true),
                        new PortableOptions("portable", PortableType.Folder))
                ],
                new Uri("https://raw.githubusercontent.com/cemu-project/Cemu/main/dist/linux/info.cemu.Cemu.png"))
        };

        return new Repository(Guid.Parse("90c67dcd-8897-4a92-959a-0c293d9850e3"),
            "NightlyLauncher Emulation",
            new Uri("https://raw.githubusercontent.com/nxtroox/NightlyLauncher/main/repositories/emulation.json"),
            appManifests);
    }
}

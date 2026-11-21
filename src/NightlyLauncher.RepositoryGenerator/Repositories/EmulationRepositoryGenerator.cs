using System.Runtime.InteropServices;
using NightlyLauncher.Core.Models;

namespace NightlyLauncher.RepositoryGenerator.Repositories;

internal sealed class EmulationRepositoryGenerator : IRepositoryGenerator {
    public string RepositoryFileName => "emulation.json";

    public Repository GenerateRepository() {
        var appManifests = new List<AppManifest> {
            new(Guid.Parse("860fe852-e59a-459c-af6c-789faa8533a5"),
                new Uri("https://api.github.com/repos/cemu-project/Cemu/commits/main"),
                "Cemu",
                [
                    new InstallOption(Guid.Parse("2194f0bf-26d1-4c19-9cb8-5a1a5cf120e5"),
                        new Uri(
                            "https://nightly.link/cemu-project/Cemu/workflows/build_check/main/cemu-bin-windows-x64.zip"),
                        "Nightly (Windows (x64))",
                        new Platform(OsType.Windows,
                            Architecture.X64),
                        new AppOptions(new LaunchOptions("Cemu.exe"),
                            true),
                        new PortableOptions("portable", PortableType.Folder))
                ],
                new Uri("https://raw.githubusercontent.com/cemu-project/Cemu/main/dist/linux/info.cemu.Cemu.png")),
            new(Guid.Parse("d9923d21-75d4-4af3-b10f-299dc5b0f36b"),
                new Uri("https://api.github.com/repos/melonDS-emu/melonDS/commits/master"),
                "melonDS",
                [
                    new InstallOption(Guid.Parse("c44044ff-e56e-450e-95b6-e2f21cd1955c"),
                        new Uri(
                            "https://nightly.link/melonDS-emu/melonDS/workflows/build-windows/master/melonDS-windows-x86_64.zip"),
                        "Nightly (Windows (x64))",
                        new Platform(OsType.Windows,
                            Architecture.X64),
                        new AppOptions(new LaunchOptions("melonDS.exe"))),
                    new InstallOption(Guid.Parse("e7d55ec7-29f6-4745-9d4c-2cac085486d6"),
                        new Uri(
                            "https://nightly.link/melonDS-emu/melonDS/workflows/build-windows/master/melonDS-windows-x86_64.zip"),
                        "Nightly (Windows (arm64))",
                        new Platform(OsType.Windows,
                            Architecture.Arm64),
                        new AppOptions(new LaunchOptions("melonDS.exe")))
                ],
                new Uri("https://raw.githubusercontent.com/melonDS-emu/melonDS/master/res/icon/melon_128x128.png")
            )
        };

        return new Repository(Guid.Parse("90c67dcd-8897-4a92-959a-0c293d9850e3"),
            "NightlyLauncher Emulation",
            new Uri("https://raw.githubusercontent.com/nxtroox/NightlyLauncher/main/repositories/emulation.json"),
            appManifests);
    }
}

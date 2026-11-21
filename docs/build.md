# :hammer: Building from Source

You can build NightlyLauncher from source very easily by doing the following things:

## :toolbox: Requirements

> [!NOTE]
> Building NightlyLauncher generally requires the [.NET 11 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/11.0) and your operating system's native development tools (for AOT). Check out [this article by Microsoft](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/) for more details and operating systems.

<details>
<summary>:window: Windows</summary>

1. [.NET 11 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/11.0)
2. [Visual Studio (Build Tools) 2026 with C++ enabled](https://visualstudio.microsoft.com/downloads/)

</details>

<details>
<summary>:apple: MacOS</summary>

1. [.NET 11 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/11.0)
2. [Command Line Tools for Xcode](https://developer.apple.com/download/)

</details>

<details>
<summary>:penguin: Linux</summary>

Concrete packages depend on your specific distribution. Generally, you need a compiler toolchain (e.g. [clang](https://clang.llvm.org/)) and the [.NET 11 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/11.0).

Check your specific distribution for specific instructions. For more details see the article linked above.

</details>

## :arrow_down: Downloading Source Code via Git

<details>
<summary>Via HTTPS</summary>

```bash
git clone https://github.com/nxtroox/NightlyLauncher.git
```

</details>

<details>
<summary>Via SSH</summary>

```bash
git clone git@github.com:nxtroox/NightlyLauncher.git
```

</details>

## :hammer: Starting the Build Process

> [!IMPORTANT]
> To find out your runtime identifier, check out [this article by Microsoft](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog#known-rids).

> [!NOTE]
> Even if we don't provide binaries for your specific CPU/OS combination, it might still be possible to build NightlyLauncher. Check out the article above for all supported OS/CPU combinations.

Inside the NightlyLauncher repository, run:

```bash
dotnet publish -r <your runtime identifier> -o publish
```

The binaries will be located in the `publish` folder at the root of the NightlyLauncher repository.

### :cd: Building the Installer

> ![WARNING]
> Building the Installer requires Windows.

> [!TIP]
> `iscc.exe` will not be added to the PATH variable by default. Use its absolute path if it cannot be found.

1. Install the latest version of [Inno Setup 7](https://jrsoftware.org/inno-setup/downloads).

2. Inside the root of the NightlyLauncher repository, run the following command to start the compilation process:

    ```bash
    iscc.exe installer/setup.iss
    ```

3. The installer will be located under `installer/Output/NightlyLauncherSetup-win-x64.exe`.

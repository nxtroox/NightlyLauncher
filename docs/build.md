# :hammer: Building from Source

You can build NightlyLauncher from source very easily by doing the following things:

## :toolbox: Installing Build Requirements

Install the [.NET 11 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/11.0).

## :open_file_folder: Pulling Source from GitHub

Via HTTPS:

```bash
git clone https://github.com/nxtroox/NightlyLauncher.git
```

Via SSH:

```bash
git clone git@github.com:nxtroox/NightlyLauncher.git
```

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

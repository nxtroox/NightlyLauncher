using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal sealed class RepositoryService(
    IConfigService configService,
    HttpClient httpClient,
    JsonSerializerOptions jsonSerializerOptions,
    IStringLocalizer<RepositoryService> localizer,
    IPathService pathService)
    : IRepositoryService {
    private List<Repository>? _repositories;

    public async Task UpdateRepositoriesAsync(Func<Exception, Task> errorCallback,
        CancellationToken cancellationToken = default) {
        await LoadRepositoriesIfNeededAsync(cancellationToken);

        var updated = false;
        for (var index = 0; index < _repositories!.Count; index++) {
            var repo = _repositories[index];

            try {
                var updatedRepository = await GetRepositoryFromUriAsync(repo.UpdateUri, cancellationToken);

                _repositories[index] = updatedRepository ??
                                       throw new InvalidOperationException(
                                           localizer["RepositoryNotFetchedExceptionMessage"]);

                updated = true;
            } catch (Exception exception) {
                var detailedException = new InvalidOperationException(
                    localizer["RepositoryNotUpdatedExceptionMessage",
                        repo.DisplayName,
                        exception.Message]);

                await errorCallback(detailedException);
            }
        }

        if (updated) {
            await UpdateConfig(cancellationToken);
            await WriteRepositoriesAsync(cancellationToken);
        }
    }

    public async Task AddRepositoryAsync(Uri uri, CancellationToken cancellationToken = default) {
        await LoadRepositoriesIfNeededAsync(cancellationToken);

        await AddRepositoryCoreAsync(uri, cancellationToken);

        await UpdateConfig(cancellationToken);
        await WriteRepositoriesAsync(cancellationToken);
    }

    public async Task RemoveRepositoryAsync(Repository repository, CancellationToken cancellationToken = default) {
        await LoadRepositoriesIfNeededAsync(cancellationToken);

        var found = _repositories!.FirstOrDefault(r => r.Guid == repository.Guid);

        if (found is not null) {
            _repositories!.Remove(found);
            await UpdateConfig(cancellationToken);
            await WriteRepositoriesAsync(cancellationToken);
        } else
            throw new KeyNotFoundException(localizer["RepositoryNotFoundExceptionMessage", repository.DisplayName]);
    }

    public async Task<List<AppManifest>> GetAvailableAppsAsync(CancellationToken cancellationToken = default) {
        await LoadRepositoriesIfNeededAsync(cancellationToken);

        return [
            .. _repositories!.SelectMany(repo => repo.Apps)
                .Where(manifest => manifest.InstallOptions.Any(option =>
                    CheckAppCompatibility(option.Platform.OsType, option.Platform.Architecture)))
        ];
    }

    public async Task<List<Repository>> GetRepositoriesAsync(CancellationToken cancellationToken = default) {
        await LoadRepositoriesIfNeededAsync(cancellationToken);
        return _repositories!;
    }

    public bool CheckAppCompatibility(OsType requestedOsType, Architecture requestedArchitecture) {
        var os = GetCurrentOperatingSystem();
        if (os != requestedOsType) return false;

        var osArchitecture = RuntimeInformation.OSArchitecture;

        if (os is not OsType.MacOs && osArchitecture is Architecture.X64 &&
            requestedArchitecture is Architecture.X86)
            return true;

        if (os is not OsType.Linux && osArchitecture is Architecture.Arm64 &&
            requestedArchitecture is Architecture.X64)
            return true;

        if (os is OsType.Windows && osArchitecture is Architecture.Arm64 &&
            requestedArchitecture is Architecture.X86)
            return true;

        return osArchitecture == requestedArchitecture;
    }

    private async Task AddRepositoryCoreAsync(Uri uri, CancellationToken cancellationToken, Guid? guid = null) {
        var repo = guid is null
            ? await GetRepositoryFromUriAsync(uri, cancellationToken)
            : await GetRepositoryFromFileAsync(
                guid ?? throw new InvalidOperationException(localizer["RepositoryGuidIsNullExceptionMessage"]),
                cancellationToken);

        if (repo is not null) {
            if (_repositories!.Any(r => r.UpdateUri == repo.UpdateUri || r.Guid == repo.Guid))
                throw new InvalidOperationException(localizer["RepositoryAlreadyInstalledExceptionMessage"]);

            _repositories!.Add(repo);
        } else
            throw new InvalidOperationException(localizer["RepositoryNotFetchedExceptionMessage"]);
    }

    private Task LoadRepositoriesIfNeededAsync(CancellationToken cancellationToken) =>
        _repositories is null ? LoadRepositoriesAsync(cancellationToken) : Task.CompletedTask;

    private async Task LoadRepositoriesAsync(CancellationToken cancellationToken) {
        _repositories = [];

        var config = configService.Config;
        if (config.Repositories is null or []) return;

        foreach (var entry in config.Repositories)
            await AddRepositoryCoreAsync(entry.Uri, cancellationToken, entry.Guid);
    }

    private Task<Repository?> GetRepositoryFromUriAsync(Uri uri, CancellationToken cancellationToken) =>
        httpClient.GetFromJsonAsync<Repository>(uri, cancellationToken);

    private async Task<Repository?> GetRepositoryFromFileAsync(Guid guid, CancellationToken cancellationToken) {
        var path = Path.Combine(pathService.RepositoryPath, $"{guid.ToString()}.json");
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<Repository>(stream, jsonSerializerOptions, cancellationToken);
    }

    private async Task WriteRepositoriesAsync(CancellationToken cancellationToken) {
        if (_repositories is null || _repositories.Count is 0) return;

        var repositoryCachePath = pathService.RepositoryPath;

        var directory = Path.GetDirectoryName(repositoryCachePath);
        if (directory is not null)
            Directory.CreateDirectory(directory);

        foreach (var path in _repositories.Select(repository =>
                     Path.Combine(repositoryCachePath, $"{repository.Guid.ToString()}.json"))) {
            await using var stream = File.Open(path, FileMode.Create, FileAccess.Write);
            await JsonSerializer.SerializeAsync(stream, _repositories, jsonSerializerOptions, cancellationToken);
        }
    }

    private Task UpdateConfig(CancellationToken cancellationToken) =>
        configService.UpdateAsync(configService.Config with {
                Repositories = [
                    .. _repositories!.Select(repository => new RepositoryEntry(repository.UpdateUri, repository.Guid))
                ]
            },
            cancellationToken);

    private OsType GetCurrentOperatingSystem() =>
        OperatingSystem.IsWindows() ? OsType.Windows
        : OperatingSystem.IsMacOS() ? OsType.MacOs
        : OperatingSystem.IsLinux() ? OsType.Linux
        : throw new PlatformNotSupportedException(localizer["UnsupportedOperatingSystemExceptionMessage"]);
}

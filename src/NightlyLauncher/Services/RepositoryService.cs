using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Models;
using OperatingSystem = NightlyLauncher.Models.OperatingSystem;

namespace NightlyLauncher.Services;

internal class RepositoryService(
    IConfigService configService,
    HttpClient httpClient,
    IStringLocalizer<RepositoryService> localizer)
    : IRepositoryService {
    private List<Repository> _repositories = [];

    public async Task UpdateRepositoriesAsync(Func<Exception, Task> errorCallback,
        CancellationToken cancellationToken = default) {
        await GetRepositoriesAsync(cancellationToken);

        var updated = false;
        for (var index = 0; index < _repositories.Count; index++) {
            var repo = _repositories[index];
            if (repo.UpdateUri is null) continue;

            try {
                var updatedRepository = await GetRepositoryAsync(repo.UpdateUri, cancellationToken);

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

        if (updated)
            await UpdateConfig(cancellationToken);
    }

    public async Task AddRepositoryAsync(Uri uri, CancellationToken cancellationToken = default) {
        await GetRepositoriesAsync(cancellationToken);

        var repo = await GetRepositoryAsync(uri, cancellationToken);

        if (repo is not null) {
            if (_repositories.Any(r => r.UpdateUri == repo.UpdateUri || r.Guid == repo.Guid))
                throw new InvalidOperationException(localizer["RepositoryAlreadyInstalledExceptionMessage"]);

            _repositories.Add(repo);
            await UpdateConfig(cancellationToken);
        } else
            throw new InvalidOperationException(localizer["RepositoryNotFetchedExceptionMessage"]);
    }

    public async Task RemoveRepositoryAsync(Repository repository, CancellationToken cancellationToken = default) {
        await GetRepositoriesAsync(cancellationToken);

        var found = _repositories.FirstOrDefault(r => r.Guid == repository.Guid);

        if (found is not null) {
            _repositories.Remove(found);
            await UpdateConfig(cancellationToken);
        } else
            throw new KeyNotFoundException(localizer["RepositoryNotFoundExceptionMessage", repository.DisplayName]);
    }

    public async Task<List<AppManifest>> GetAvailableAppsAsync(CancellationToken cancellationToken = default) {
        await GetRepositoriesAsync(cancellationToken);

        return [
            .. _repositories.SelectMany(repo => repo.Apps)
                .Where(manifest => manifest.InstallOptions.Any(option =>
                    CheckAppCompatibility(option.Platform.OperatingSystem, option.Platform.Architecture)))
        ];
    }

    public bool CheckAppCompatibility(OperatingSystem requestedOperatingSystem, Architecture requestedArchitecture) {
        var os = GetCurrentOperatingSystem();
        if (os != requestedOperatingSystem) return false;

        var osArchitecture = RuntimeInformation.OSArchitecture;

        if (os is not OperatingSystem.MacOs && osArchitecture is Architecture.X64 &&
            requestedArchitecture is Architecture.X86)
            return true;

        if (os is not OperatingSystem.Linux && osArchitecture is Architecture.Arm64 &&
            requestedArchitecture is Architecture.X64)
            return true;

        if (os is OperatingSystem.Windows && osArchitecture is Architecture.Arm64 &&
            requestedArchitecture is Architecture.X86)
            return true;

        return osArchitecture == requestedArchitecture;
    }

    private async Task GetRepositoriesAsync(CancellationToken cancellationToken) {
        if (_repositories.Count == 0)
            await LoadRepositoriesAsync(cancellationToken);
    }

    private async Task LoadRepositoriesAsync(CancellationToken cancellationToken) {
        _repositories = [];

        var config = await configService.GetConfigAsync(cancellationToken);
        if (config.Repositories is null or []) return;

        _repositories.AddRange(config.Repositories);
    }

    private Task<Repository?> GetRepositoryAsync(Uri uri, CancellationToken cancellationToken) =>
        httpClient.GetFromJsonAsync<Repository>(uri, cancellationToken);

    private Task UpdateConfig(CancellationToken cancellationToken) =>
        configService.UpdateConfigAsync(configService.Config with { Repositories = _repositories },
            cancellationToken);

    private OperatingSystem GetCurrentOperatingSystem() =>
        System.OperatingSystem.IsWindows() ? OperatingSystem.Windows
        : System.OperatingSystem.IsMacOS() ? OperatingSystem.MacOs
        : System.OperatingSystem.IsLinux() ? OperatingSystem.Linux
        : throw new PlatformNotSupportedException(localizer["UnsupportedOperatingSystemExceptionMessage"]);
}

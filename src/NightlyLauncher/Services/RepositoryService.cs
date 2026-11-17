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

namespace NightlyLauncher.Services;

internal sealed class RepositoryService(
    IConfigService configService,
    HttpClient httpClient,
    IStringLocalizer<RepositoryService> localizer)
    : IRepositoryService {
    private List<Repository>? _repositories;

    public async Task UpdateRepositoriesAsync(Func<Exception, Task> errorCallback,
        CancellationToken cancellationToken = default) {
        await LoadRepositoriesIfNeededAsync(cancellationToken);

        var updated = false;
        for (var index = 0; index < _repositories!.Count; index++) {
            var repo = _repositories[index];

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
        await LoadRepositoriesIfNeededAsync(cancellationToken);

        await AddRepositoryCoreAsync(uri, cancellationToken);
        await UpdateConfig(cancellationToken);
    }

    public async Task RemoveRepositoryAsync(Repository repository, CancellationToken cancellationToken = default) {
        await LoadRepositoriesIfNeededAsync(cancellationToken);

        var found = _repositories!.FirstOrDefault(r => r.Guid == repository.Guid);

        if (found is not null) {
            _repositories!.Remove(found);
            await UpdateConfig(cancellationToken);
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

    private async Task AddRepositoryCoreAsync(Uri uri, CancellationToken cancellationToken) {
        var repo = await GetRepositoryAsync(uri, cancellationToken);

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

        var config = await configService.GetConfigAsync(cancellationToken);
        if (config.Repositories is null or []) return;

        foreach (var uri in config.Repositories)
            await AddRepositoryCoreAsync(uri, cancellationToken);
    }

    private Task<Repository?> GetRepositoryAsync(Uri uri, CancellationToken cancellationToken) =>
        httpClient.GetFromJsonAsync<Repository>(uri, cancellationToken);

    private Task UpdateConfig(CancellationToken cancellationToken) =>
        configService.UpdateConfigAsync(
            configService.Config with { Repositories = [.. _repositories!.Select(repository => repository.UpdateUri)] },
            cancellationToken);

    private OsType GetCurrentOperatingSystem() =>
        OperatingSystem.IsWindows() ? OsType.Windows
        : OperatingSystem.IsMacOS() ? OsType.MacOs
        : OperatingSystem.IsLinux() ? OsType.Linux
        : throw new PlatformNotSupportedException(localizer["UnsupportedOperatingSystemExceptionMessage"]);
}

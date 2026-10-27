using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal class RepositoryService(
    IConfigService configService,
    HttpClient httpClient,
    IStringLocalizer<RepositoryService> localizer)
    : IRepositoryService {
    private List<Repository> _repositories = [];

    public async Task<List<Repository>?> GetRepositoriesAsync(CancellationToken cancellationToken = default) {
        if (_repositories.Count == 0)
            await LoadRepositoriesAsync(cancellationToken);

        return _repositories.Count > 0 ? _repositories : null;
    }

    public async Task UpdateRepositoriesAsync(CancellationToken cancellationToken = default) {
        await GetRepositoriesAsync(cancellationToken);

        foreach (var repo in _repositories) {
            if (repo.UpdateUri is null) return;
            _repositories.Remove(repo);

            await AddRepositoryAsync(repo.UpdateUri, cancellationToken);
        }
    }

    public async Task AddRepositoryAsync(Uri uri, CancellationToken cancellationToken = default) {
        await GetRepositoriesAsync(cancellationToken);

        var repo = await GetRepositoryAsync(uri, cancellationToken);

        if (repo is not null) {
            _repositories.Add(repo);
            await UpdateConfig(cancellationToken);
        } else
            throw new InvalidOperationException(localizer["RepositoryNotFetched"]);
    }

    public async Task RemoveRepositoryAsync(Repository repository, CancellationToken cancellationToken = default) {
        await GetRepositoriesAsync(cancellationToken);

        var found = _repositories.FirstOrDefault(r => r.Guid == repository.Guid);

        if (found is not null) {
            _repositories.Remove(found);
            await UpdateConfig(cancellationToken);
        } else
            throw new KeyNotFoundException(localizer["RepositoryNotFound", repository.ReadableName]);
    }

    private async Task LoadRepositoriesAsync(CancellationToken cancellationToken) {
        _repositories = [];

        var config = await configService.GetConfigAsync(cancellationToken);
        if (config.Repositories is null or []) return;

        _repositories.AddRange(config.Repositories);
    }

    private async Task<Repository?> GetRepositoryAsync(Uri uri, CancellationToken cancellationToken) =>
        await httpClient.GetFromJsonAsync<Repository>(uri, cancellationToken);

    private async Task UpdateConfig(CancellationToken cancellationToken) =>
        await configService.UpdateConfigAsync(configService.Config with { Repositories = _repositories },
            cancellationToken);
}

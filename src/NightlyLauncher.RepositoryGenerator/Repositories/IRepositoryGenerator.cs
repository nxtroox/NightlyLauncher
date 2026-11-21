using NightlyLauncher.Core.Models;

namespace NightlyLauncher.RepositoryGenerator.Repositories;

internal interface IRepositoryGenerator {
    string RepositoryFileName { get; }
    Repository GenerateRepository();
}

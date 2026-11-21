using System.Text.Json;
using NightlyLauncher.RepositoryGenerator.Repositories;
using NightlyLauncher.RepositoryGenerator.Serialization;

var generators = new List<IRepositoryGenerator> { new EmulationRepositoryGenerator() };

const string repositoriesPath = "repositories";
Directory.CreateDirectory(repositoriesPath);

foreach (var generator in generators) {
    var repository = generator.GenerateRepository();
    var path = Path.Combine(repositoriesPath, generator.RepositoryFileName);

    await using var stream = new FileStream(
        path,
        FileMode.Create,
        FileAccess.Write,
        FileShare.None,
        4096,
        true
    );

    await JsonSerializer.SerializeAsync(stream, repository, AppJsonContext.Default.Repository);
}

using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NightlyLauncher.Services;
using NightlyLauncher.ViewModels;

namespace NightlyLauncher.Extensions;

public static class ServiceCollectionExtensions {
    public static void AddCommonServices(this IServiceCollection collection) {
        collection.AddTransient<MainViewModel>();
        collection.AddTransient<SettingsViewModel>();

        collection.AddHttpClient<AppUpdaterService>(client => {
            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue(
                    "NightlyLauncher", Assembly.GetExecutingAssembly().GetName().Version?.ToString())
            );
        });
        collection.AddTransient<IAppUpdaterService, AppUpdaterService>();

        collection.AddSingleton<IConfigService, ConfigService>();

        collection.AddScoped<IDatabaseService, DatabaseService>();

        collection.AddTransient<IPathService, PathService>();

        collection.AddTransient<IDialogService, DialogService>();

        collection.AddSingleton(new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, WriteIndented = true
        });

        collection.AddLocalization();
    }
}

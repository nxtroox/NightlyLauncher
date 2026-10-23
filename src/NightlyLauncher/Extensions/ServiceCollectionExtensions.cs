using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
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

        collection.AddSingleton<IDatabaseService, DatabaseService>();

        collection.AddTransient<IPathService, PathService>();

        collection.AddTransient<ILauncherService, LauncherService>();

        collection.AddSingleton<IDialogService>(sp => new DialogService(
            new DialogManager(
                new ViewLocator(),
                new DialogFactory().AddMessageBox()),
            sp.GetService));

        collection.AddSingleton(new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, WriteIndented = true
        });

        collection.AddLocalization(options => options.ResourcesPath = "Resources");
    }
}

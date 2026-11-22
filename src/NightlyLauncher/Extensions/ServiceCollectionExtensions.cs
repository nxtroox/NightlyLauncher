using System.Net.Http.Headers;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using NightlyLauncher.Localization;
using NightlyLauncher.Services;
using NightlyLauncher.ViewModels;

namespace NightlyLauncher.Extensions;

internal static class ServiceCollectionExtensions {
    internal static void AddCommonServices(this IServiceCollection collection) {
        collection.AddTransient<AboutDialogViewModel>();
        collection.AddTransient<AppBrowserViewModel>();
        collection.AddTransient<EditDialogViewModel>();
        collection.AddTransient<InputDialogViewModel>();
        collection.AddTransient<LibraryViewModel>();
        collection.AddTransient<MainWindowViewModel>();
        collection.AddTransient<RepositoryManagerViewModel>();
        collection.AddTransient<SelectionViewModel>();
        collection.AddTransient<SettingsViewModel>();
        collection.AddTransient<UpdaterViewModel>();

        collection.AddSingleton<Func<Type, ViewModelBase>>(sp =>
            viewModelType => (ViewModelBase)sp.GetRequiredService(viewModelType));

        collection.AddHttpClient<IAppUpdaterService, AppUpdaterService>(client => {
            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("NightlyLauncher",
                    typeof(Program).Assembly.GetName().Version?.ToString()));
        });

        collection.AddHttpClient<IRepositoryService, RepositoryService>(client => {
            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("NightlyLauncher",
                    typeof(Program).Assembly.GetName().Version?.ToString()));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        collection.AddSingleton<IConfigService, ConfigService>();

        collection.AddSingleton<IDatabaseService, DatabaseService>();

        collection.AddTransient<IPathService, PathService>();

        collection.AddTransient<ILauncherService, LauncherService>();

        collection.AddSingleton<IDialogService>(sp => new DialogService(
            new DialogManager(
                new ViewLocator(),
                new DialogFactory().AddFluent(FluentMessageBoxType.ContentDialog)),
            sp.GetService));

        collection.AddLocalization(options => options.ResourcesPath = "Resources");
        collection.AddSingleton<LocalizationManager>();

        collection.AddSingleton<DialogOwner>();
    }
}

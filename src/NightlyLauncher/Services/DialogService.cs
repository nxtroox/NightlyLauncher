using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using NightlyLauncher.ViewModels;

namespace NightlyLauncher.Services;

internal sealed class DialogService(IServiceProvider serviceProvider) : IDialogService {
    public async Task<TResult?> ShowDialogAsync<TView, TViewModel, TResult>(Window owner)
        where TView : Window, new()
        where TViewModel : ViewModelBase {
        var window = new TView { DataContext = serviceProvider.GetRequiredService<TViewModel>() };
        return await window.ShowDialog<TResult?>(owner);
    }
}

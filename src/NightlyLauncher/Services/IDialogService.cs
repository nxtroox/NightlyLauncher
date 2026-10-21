using System.Threading.Tasks;
using Avalonia.Controls;
using NightlyLauncher.ViewModels;

namespace NightlyLauncher.Services;

internal interface IDialogService {
    Task<TResult?> ShowDialogAsync<TView, TViewModel, TResult>(Window owner)
        where TView : Window, new()
        where TViewModel : ViewModelBase;
}

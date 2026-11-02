using System;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;

namespace NightlyLauncher.ViewModels;

internal abstract partial class DialogViewModelBase : ViewModelBase, IModalDialogViewModel, ICloseable {
    public event EventHandler? RequestClose;
    public bool? DialogResult { get; private set; }

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private void Ok() {
        DialogResult = true;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel() {
        DialogResult = false;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    protected abstract bool CanSubmit();
}

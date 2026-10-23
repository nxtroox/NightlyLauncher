using System;
using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;

namespace NightlyLauncher.ViewModels;

internal partial class UpdaterViewModel : ViewModelBase, IModalDialogViewModel, ICloseable {
    [ObservableProperty] public partial string? StatusText { get; set; }

    [ObservableProperty] public partial double ProgressValue { get; set; } = -1;

    [ObservableProperty] public partial bool ProgressIsIndeterminate { get; set; } = true;

    public event EventHandler? RequestClose;

    public bool? DialogResult { get; private set; }

    internal void Close(bool result) {
        DialogResult = result;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}

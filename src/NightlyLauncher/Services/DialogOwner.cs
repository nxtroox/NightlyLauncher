using System.ComponentModel;

namespace NightlyLauncher.Services;

internal sealed class DialogOwner {
    internal INotifyPropertyChanged? Window { get; set; }
}

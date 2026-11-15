using System.ComponentModel;
using Microsoft.Extensions.Localization;

namespace NightlyLauncher.Localization;

public sealed class LocalizedSource(IStringLocalizer localizer) : INotifyPropertyChanged {
    public string this[string key] => localizer[key].Value;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Refresh()
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
}

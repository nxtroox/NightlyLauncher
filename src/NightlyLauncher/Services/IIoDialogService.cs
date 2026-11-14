using System;
using System.Threading.Tasks;

namespace NightlyLauncher.Services;

internal interface IIoDialogService {
    Task<bool> OpenFolderAsync(string path);
    Task<bool> OpenUriAsync(Uri uri);
}

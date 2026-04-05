using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace MT32Edit.Avalonia;

/// <summary>
/// Avalonia implementation of IFileDialogProvider using StorageProvider API.
/// </summary>
internal class AvaloniaPlatformFileDialog : IFileDialogProvider
{
    private readonly Window _owner;

    public AvaloniaPlatformFileDialog(Window owner)
    {
        _owner = owner;
    }

    public string? ShowOpenFileDialog(string title, string filter, bool checkFileExists = true)
    {
        string? result = null;
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var storageProvider = _owner.StorageProvider;
            var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = ParseFilter(filter)
            });

            if (files.Count > 0)
            {
                result = files[0].Path.LocalPath;
            }
        }).Wait();
        return result;
    }

    public string? ShowSaveFileDialog(string title, string filter, string defaultFileName = "")
    {
        string? result = null;
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var storageProvider = _owner.StorageProvider;
            var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName = defaultFileName,
                FileTypeChoices = ParseFilter(filter)
            });

            if (file is not null)
            {
                result = file.Path.LocalPath;
            }
        }).Wait();
        return result;
    }

    public string? ShowFolderBrowserDialog()
    {
        string? result = null;
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var storageProvider = _owner.StorageProvider;
            var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Folder",
                AllowMultiple = false
            });

            if (folders.Count > 0)
            {
                result = folders[0].Path.LocalPath;
            }
        }).Wait();
        return result;
    }

    /// <summary>
    /// Parses a WinForms-style filter string into Avalonia FilePickerFileType list.
    /// e.g. "MIDI files|*.mid|SysEx files|*.syx" → list of FilePickerFileType
    /// </summary>
    private static List<FilePickerFileType> ParseFilter(string filter)
    {
        var types = new List<FilePickerFileType>();
        if (string.IsNullOrEmpty(filter)) return types;

        var parts = filter.Split('|');
        for (int i = 0; i + 1 < parts.Length; i += 2)
        {
            var name = parts[i].Trim();
            var patterns = parts[i + 1].Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToList();

            types.Add(new FilePickerFileType(name) { Patterns = patterns });
        }
        return types;
    }
}

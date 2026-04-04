namespace MT32Edit;

/// <summary>
/// Abstraction for file/folder selection dialogs.
/// Implemented by each UI project (WinForms, Avalonia).
/// </summary>
public interface IFileDialogProvider
{
    /// <summary>
    /// Shows an Open File dialog.
    /// </summary>
    /// <returns>Selected file path, or null if cancelled.</returns>
    string? ShowOpenFileDialog(string title, string filter, bool checkFileExists = true);

    /// <summary>
    /// Shows a Save File dialog.
    /// </summary>
    /// <returns>Selected file path, or null if cancelled.</returns>
    string? ShowSaveFileDialog(string title, string filter, string defaultFileName = "");

    /// <summary>
    /// Shows a folder browser dialog.
    /// </summary>
    /// <returns>Selected folder path, or null if cancelled.</returns>
    string? ShowFolderBrowserDialog();
}

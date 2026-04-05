namespace MT32Edit;

/// <summary>
/// Abstraction for displaying messages and confirmation dialogs to the user.
/// Implemented by each UI project (WinForms, Avalonia).
/// </summary>
public interface IUserNotification
{
    /// <summary>
    /// Show an informational message to the user.
    /// </summary>
    void ShowMessage(string message, string title = "MT-32 Editor");

    /// <summary>
    /// Show a confirmation dialog with OK/Cancel buttons.
    /// </summary>
    /// <returns>true if user confirmed, false otherwise.</returns>
    bool AskUserToConfirm(string prompt, string title = "MT-32 Editor");
}

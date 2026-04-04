namespace MT32Edit;

/// <summary>
/// Abstraction for the SysEx loading progress form/window.
/// Implemented by each UI project (WinForms, Avalonia).
/// </summary>
public interface ISysExLoadForm
{
    /// <summary>
    /// Shows the SysEx loading progress dialog for the given memory state.
    /// </summary>
    /// <returns>true if loading completed successfully, false otherwise.</returns>
    bool ShowLoadDialog(MT32State memoryState, bool requestClearMemory);
}

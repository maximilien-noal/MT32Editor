namespace MT32Edit;

/// <summary>
/// Abstraction for screen/display information.
/// Implemented by each UI project (WinForms, Avalonia).
/// </summary>
public interface IScreenInfo
{
    /// <summary>
    /// Gets the working area width of the primary screen.
    /// </summary>
    int PrimaryScreenWidth { get; }

    /// <summary>
    /// Gets the working area height of the primary screen.
    /// </summary>
    int PrimaryScreenHeight { get; }
}

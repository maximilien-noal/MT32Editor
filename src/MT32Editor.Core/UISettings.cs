namespace MT32Edit;

/// <summary>
/// Shared UI settings that both WinForms and Avalonia projects access.
/// These are the non-WinForms-specific parts of UITools.
/// </summary>
public static class UISettings
{
    public const int UI_REFRESH_INTERVAL = 200;
    public static bool DarkMode { get; set; } = true;
    public static bool PrioritiseTimbreEditor { get; set; } = false;
    public static bool SaveWindowSizeAndPosition { get; set; } = false;
    public static int[] WindowLocation { get; set; } = { 0, 0 };
    public static int[] WindowSize { get; set; } = { 0, 0 };
    public static bool WindowMaximised { get; set; } = false;
}

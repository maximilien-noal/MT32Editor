using Avalonia.Controls;

namespace MT32Edit.Avalonia;

/// <summary>
/// Main application window - Avalonia equivalent of WinForms FormMainMenu.
/// Uses tabbed layout instead of MDI.
/// </summary>
public partial class WindowMainMenu : Window
{
    public WindowMainMenu()
    {
        Title = "MT-32 Editor";
        Width = 1774;
        Height = 1038;
    }

    public void Initialize(string[] args)
    {
        // Will be fully implemented when porting FormMainMenu
    }
}

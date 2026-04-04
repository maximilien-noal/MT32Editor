using Avalonia.Controls;

namespace MT32Edit.Avalonia;

/// <summary>
/// SysEx loading progress window - Avalonia equivalent of WinForms FormLoadSysEx.
/// </summary>
public class WindowLoadSysEx : Window
{
    public WindowLoadSysEx(MT32State memoryState, bool requestClearMemory)
    {
        Title = "Uploading SysEx Data";
        Width = 336;
        Height = 103;
    }
}

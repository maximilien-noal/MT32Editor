using Avalonia.Controls;
using Avalonia.Threading;

namespace MT32Edit.Avalonia;

/// <summary>
/// Avalonia implementation of ISysExLoadForm - displays SysEx loading progress.
/// </summary>
internal class AvaloniaPlatformSysExLoadForm : ISysExLoadForm
{
    private readonly Window _owner;

    public AvaloniaPlatformSysExLoadForm(Window owner)
    {
        _owner = owner;
    }

    public bool ShowLoadDialog(MT32State memoryState, bool requestClearMemory)
    {
        bool result = false;
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var loadWindow = new WindowLoadSysEx(memoryState, requestClearMemory);
            await loadWindow.ShowDialog(_owner);
            result = true;
        }).Wait();
        return result;
    }
}

using Avalonia.Controls;

namespace MT32Edit.Avalonia;

/// <summary>
/// Avalonia implementation of IScreenInfo.
/// </summary>
internal class AvaloniaPlatformScreenInfo : IScreenInfo
{
    private readonly Window _owner;

    public AvaloniaPlatformScreenInfo(Window owner)
    {
        _owner = owner;
    }

    public int PrimaryScreenWidth
    {
        get
        {
            var screen = _owner.Screens.Primary;
            return screen is not null ? (int)screen.WorkingArea.Width : 1920;
        }
    }

    public int PrimaryScreenHeight
    {
        get
        {
            var screen = _owner.Screens.Primary;
            return screen is not null ? (int)screen.WorkingArea.Height : 1080;
        }
    }
}

using Avalonia.Controls;

namespace MT32Edit.Avalonia;

/// <summary>
/// Stub for PanelTimbreEditor - will be replaced with full implementation.
/// Avalonia equivalent of WinForms FormTimbreEditor.
/// </summary>
public class PanelTimbreEditor : UserControl
{
    private TimbreStructure timbre = new TimbreStructure(createAudibleTimbre: false);

    public TimbreStructure TimbreData
    {
        get { return timbre; }
        set { timbre = value; }
    }
}

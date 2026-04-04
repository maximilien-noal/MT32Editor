using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace MT32Edit.Avalonia;

/// <summary>
/// Panel showing visual representation of MT-32's 64 memory banks — allows custom timbres to be mapped.
/// Avalonia equivalent of WinForms FormMemoryBankEditor.
/// </summary>
public class PanelMemoryBankEditor : UserControl
{
    // MT32Edit: PanelMemoryBankEditor
    // S.Fryers Mar 2026

    private MT32State memoryState;
    private PanelTimbreEditor timbreEditor;
    private TimbreStructure? copiedTimbre;
    private DateTime lastGlobalUpdate = DateTime.Now;
    private bool darkMode = !UISettings.DarkMode;

    // Controls
    private readonly TextBlock labelHeading;
    private readonly TextBlock labelTimbreNo;
    private readonly TextBlock labelTimbreName;
    private readonly TextBlock labelCopy;
    private readonly TextBlock labelPaste;
    private readonly TextBlock labelClearSelected;
    private readonly TextBlock labelClearAll;
    private readonly NumericUpDown numericUpDownTimbreNo;
    private readonly ListBox listBoxTimbres;
    private readonly Button buttonCopyTimbre;
    private readonly Button buttonPasteTimbre;
    private readonly Button buttonClearTimbre;
    private readonly Button buttonClearAll;
    private readonly DispatcherTimer timer;

    // List data model
    private readonly List<string> timbreItems = new();

    public PanelMemoryBankEditor(MT32State parentMemoryState, PanelTimbreEditor parentTimbreEditorPanel)
    {
        memoryState = parentMemoryState;
        timbreEditor = parentTimbreEditorPanel;

        var foreground = AvaloniaUITools.GetForegroundBrush();

        var mainPanel = new DockPanel { Margin = new Thickness(10, 8, 10, 8) };

        // Top controls area
        var topPanel = new StackPanel { Spacing = 4 };
        DockPanel.SetDock(topPanel, Dock.Top);

        // Heading
        labelHeading = new TextBlock
        {
            Text = "Timbre Memory Area",
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            Foreground = AvaloniaUITools.GetTitleBrush(),
            Margin = new Thickness(0, 0, 0, 4)
        };
        topPanel.Children.Add(labelHeading);

        // Memory No. row
        var timbreNoRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 4, 0, 4) };
        labelTimbreNo = new TextBlock { Text = "Memory No.", Foreground = foreground, VerticalAlignment = VerticalAlignment.Center };
        timbreNoRow.Children.Add(labelTimbreNo);
        numericUpDownTimbreNo = new NumericUpDown { Minimum = 1, Maximum = 64, Value = 1, Width = 70, FontSize = 14, FontWeight = FontWeight.Bold };
        numericUpDownTimbreNo.ValueChanged += NumericUpDownTimbreNo_ValueChanged;
        timbreNoRow.Children.Add(numericUpDownTimbreNo);
        topPanel.Children.Add(timbreNoRow);

        // Timbre name display
        labelTimbreName = new TextBlock
        {
            Text = "[none]",
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            Foreground = foreground,
            Margin = new Thickness(0, 2, 0, 4)
        };
        topPanel.Children.Add(labelTimbreName);

        // Button rows
        var copyPasteRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 4, 0, 2) };

        buttonCopyTimbre = new Button
        {
            Content = "📋",
            Width = 28,
            Height = 28,
            Background = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
            Foreground = Brushes.Black
        };
        ToolTip.SetTip(buttonCopyTimbre, "Copy the selected timbre");
        buttonCopyTimbre.Click += ButtonCopyTimbre_Click;
        copyPasteRow.Children.Add(buttonCopyTimbre);

        labelCopy = new TextBlock { Text = "Copy", Foreground = foreground, VerticalAlignment = VerticalAlignment.Center };
        copyPasteRow.Children.Add(labelCopy);

        buttonPasteTimbre = new Button
        {
            Content = "📄",
            Width = 28,
            Height = 28,
            IsEnabled = false,
            Background = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
            Foreground = Brushes.Black
        };
        ToolTip.SetTip(buttonPasteTimbre, "Paste timbre into the selected position");
        buttonPasteTimbre.Click += ButtonPasteTimbre_Click;
        copyPasteRow.Children.Add(buttonPasteTimbre);

        labelPaste = new TextBlock { Text = "Paste", Foreground = foreground, VerticalAlignment = VerticalAlignment.Center };
        copyPasteRow.Children.Add(labelPaste);

        topPanel.Children.Add(copyPasteRow);

        // Clear Selected row
        var clearSelectedRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 2, 0, 2) };

        buttonClearTimbre = new Button
        {
            Content = "✕",
            Width = 28,
            Height = 28,
            Background = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
            Foreground = Brushes.Black
        };
        ToolTip.SetTip(buttonClearTimbre, "Clear the selected timbre");
        buttonClearTimbre.Click += ButtonClearTimbre_Click;
        clearSelectedRow.Children.Add(buttonClearTimbre);

        labelClearSelected = new TextBlock { Text = "Clear Selected Timbre", Foreground = foreground, VerticalAlignment = VerticalAlignment.Center };
        clearSelectedRow.Children.Add(labelClearSelected);

        topPanel.Children.Add(clearSelectedRow);

        // Clear All row
        var clearAllRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 2, 0, 6) };

        buttonClearAll = new Button
        {
            Content = "✕✕",
            Width = 28,
            Height = 28,
            Background = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
            Foreground = Brushes.Black
        };
        ToolTip.SetTip(buttonClearAll, "Clear all memory timbres");
        buttonClearAll.Click += ButtonClearAll_Click;
        clearAllRow.Children.Add(buttonClearAll);

        labelClearAll = new TextBlock { Text = "Clear All Timbres", Foreground = foreground, VerticalAlignment = VerticalAlignment.Center };
        clearAllRow.Children.Add(labelClearAll);

        topPanel.Children.Add(clearAllRow);

        mainPanel.Children.Add(topPanel);

        // ListBox (replaces WinForms ListView)
        listBoxTimbres = new ListBox
        {
            Background = AvaloniaUITools.GetListViewBackgroundBrush(),
            Foreground = AvaloniaUITools.GetForegroundBrush(),
            MinHeight = 28,
            SelectionMode = SelectionMode.Single
        };
        listBoxTimbres.SelectionChanged += ListBoxTimbres_SelectionChanged;
        mainPanel.Children.Add(listBoxTimbres);

        Content = mainPanel;

        SynchroniseTimbreEditor(0);
        PopulateMemoryBankListBox(0);

        timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(UISettings.UI_REFRESH_INTERVAL) };
        timer.Tick += Timer_Tick;
        timer.Start();

        SetTheme();
    }

    // ------- Timer -------

    private void Timer_Tick(object? sender, EventArgs e)
    {
        int selectedTimbre = (int)(numericUpDownTimbreNo.Value ?? 1) - 1;
        if (memoryState.patchEditorActive)
        {
            selectedTimbre = FindPatchTimbreInMemoryBank(selectedTimbre);
            memoryState.memoryBankEditorActive = false;
        }
        else if (memoryState.rhythmEditorActive)
        {
            selectedTimbre = FindRhythmTimbreInMemoryBank(selectedTimbre);
            memoryState.memoryBankEditorActive = false;
        }
        if (memoryState.returnFocusToMemoryBankList)
        {
            ReturnFocusToMemoryBankList();
        }
        else
        {
            SynchroniseTimbreEditor(selectedTimbre);
        }
        RefreshMemoryBankListBox(selectedTimbre);
        SetTheme();
    }

    // ------- Theme -------

    private void SetTheme()
    {
        if (darkMode == UISettings.DarkMode)
        {
            return;
        }
        TextBlock[] labels = { labelTimbreName, labelTimbreNo, labelClearAll, labelClearSelected, labelCopy, labelPaste };
        AvaloniaUITools.ApplyTheme(labelHeading, labels, warningLabels: null);
        Background = AvaloniaUITools.GetBackgroundBrush(alternate: true);
        listBoxTimbres.Background = AvaloniaUITools.GetListViewBackgroundBrush();
        listBoxTimbres.Foreground = AvaloniaUITools.GetForegroundBrush();
        darkMode = UISettings.DarkMode;
    }

    // ------- ListBox population -------

    /// <summary>
    /// Populates the Memory Timbre ListBox.
    /// </summary>
    private void PopulateMemoryBankListBox(int selectedTimbre)
    {
        timbreItems.Clear();
        for (int timbreNo = 0; timbreNo < MT32State.NO_OF_MEMORY_TIMBRES; timbreNo++)
        {
            timbreItems.Add(FormatTimbreListItem(timbreNo));
        }
        listBoxTimbres.ItemsSource = null;
        listBoxTimbres.ItemsSource = timbreItems;
        SelectTimbreInListBox(selectedTimbre);
    }

    /// <summary>
    /// Formats a timbre entry as "NN: TimbreName" for display in the ListBox.
    /// </summary>
    private string FormatTimbreListItem(int timbreNo)
    {
        string name;
        if (memoryState.GetMemoryTimbre(timbreNo) == null)
        {
            name = memoryState.GetTimbreNames().Get(timbreNo, 2);
        }
        else
        {
            name = memoryState.GetMemoryTimbre(timbreNo).GetTimbreName();
        }
        return $"{(timbreNo + 1):D2}: {name}";
    }

    /// <summary>
    /// Highlights the desired item in the Memory Timbre ListBox.
    /// </summary>
    private void SelectTimbreInListBox(int selectedTimbre)
    {
        if (selectedTimbre >= 0 && selectedTimbre < timbreItems.Count)
        {
            listBoxTimbres.SelectedIndex = selectedTimbre;
            listBoxTimbres.ScrollIntoView(selectedTimbre);
        }
    }

    // ------- Timbre editor sync -------

    /// <summary>
    /// Sets the Timbre Editor to show the currently selected Memory Timbre.
    /// </summary>
    private void SynchroniseTimbreEditor(int selectedTimbre)
    {
        timbreEditor.TimbreData = memoryState.GetMemoryTimbre(selectedTimbre);
        string timbreName = timbreEditor.TimbreData.GetTimbreName();
        memoryState.GetTimbreNames().SetMemoryTimbreName(timbreName, selectedTimbre);
    }

    // ------- Populate form parameters -------

    /// <summary>
    /// Updates controls with selected timbre parameter values.
    /// </summary>
    private void PopulateTimbreFormParameters(int selectedTimbre)
    {
        numericUpDownTimbreNo.Value = selectedTimbre + 1;
        labelTimbreName.Text = memoryState.GetMemoryTimbreName(selectedTimbre);
        MT32SysEx.PreviewTimbre(selectedTimbre, memoryState.GetMemoryTimbre(selectedTimbre));
    }

    // ------- Cross-editor focus tracking -------

    /// <summary>
    /// Highlights memory timbre matching the currently selected patch, if it uses a memory timbre.
    /// </summary>
    private int FindPatchTimbreInMemoryBank(int selectedTimbreNo)
    {
        int requiredTimbreNo = selectedTimbreNo;
        int selectedPatchNo = memoryState.GetSelectedPatchNo();
        Patch patchData = memoryState.GetPatch(selectedPatchNo);
        if (patchData.GetTimbreGroupType() == "Memory")
        {
            FindTimbre();
        }
        else
        {
            memoryState.SetTimbreIsEditable(false);
        }
        return requiredTimbreNo;

        void FindTimbre()
        {
            requiredTimbreNo = patchData.GetTimbreNo();
            if (requiredTimbreNo != selectedTimbreNo)
            {
                SetTimbreAsEditable(requiredTimbreNo);
            }
        }
    }

    /// <summary>
    /// Highlights memory timbre matching the currently selected rhythm key, if it uses a memory timbre.
    /// </summary>
    private int FindRhythmTimbreInMemoryBank(int selectedTimbreNo)
    {
        int requiredTimbreNo = selectedTimbreNo;
        int bankNo = memoryState.GetSelectedBank();
        Rhythm rhythmBank = memoryState.GetRhythmBank(bankNo);
        if (rhythmBank.GetTimbreGroupType() == "Memory")
        {
            FindTimbre();
        }
        else
        {
            memoryState.SetTimbreIsEditable(false);
        }
        return requiredTimbreNo;

        void FindTimbre()
        {
            requiredTimbreNo = rhythmBank.GetTimbreNo();
            if (requiredTimbreNo != selectedTimbreNo)
            {
                SetTimbreAsEditable(requiredTimbreNo);
            }
        }
    }

    private void SetTimbreAsEditable(int selectedTimbre)
    {
        numericUpDownTimbreNo.Value = selectedTimbre + 1;
        memoryState.SetTimbreIsEditable(true);
        SynchroniseTimbreEditor(selectedTimbre);
        memoryState.returnFocusToPatchEditor = true;
    }

    /// <summary>
    /// Set focus to Memory Timbre ListBox and clear flag.
    /// </summary>
    private void ReturnFocusToMemoryBankList()
    {
        listBoxTimbres.Focus();
        memoryState.returnFocusToMemoryBankList = false;
    }

    // ------- Refresh -------

    /// <summary>
    /// Refreshes Memory Timbre ListBox if memoryState has recently been updated.
    /// </summary>
    private void RefreshMemoryBankListBox(int selectedTimbre)
    {
        TimbreStructure timbre = memoryState.GetMemoryTimbre(selectedTimbre);
        TimbreNames timbreNames = new TimbreNames();
        string timbreName = timbre.GetTimbreName();
        if (lastGlobalUpdate < timbre.GetUpdateTime())
        {
            PopulateMemoryBankListBox(selectedTimbre);
            lastGlobalUpdate = DateTime.Now;
        }
        timbreNames.SetMemoryTimbreName(timbreName, selectedTimbre);
        labelTimbreName.Text = timbreNames.Get(selectedTimbre, 2);
        if (listBoxTimbres.SelectedIndex >= 0 && listBoxTimbres.SelectedIndex < timbreItems.Count)
        {
            timbreItems[listBoxTimbres.SelectedIndex] = $"{(selectedTimbre + 1):D2}: {timbreNames.Get(selectedTimbre, 2)}";
            listBoxTimbres.ItemsSource = null;
            listBoxTimbres.ItemsSource = timbreItems;
            listBoxTimbres.SelectedIndex = selectedTimbre;
        }
    }

    // ------- Event handlers -------

    private void NumericUpDownTimbreNo_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        int selectedTimbre = (int)(numericUpDownTimbreNo.Value ?? 1) - 1;
        string timbreName = memoryState.GetMemoryTimbreName(selectedTimbre);
        memoryState.SetSelectedMemoryTimbre(selectedTimbre);
        labelTimbreName.Text = timbreName;
        SelectTimbreInListBox(selectedTimbre);
        SynchroniseTimbreEditor(selectedTimbre);
        if (timbreName == MT32Strings.EMPTY)
        {
            timbreName = TimbreStructure.NEW_TIMBRE;
        }
        MT32SysEx.SendText("Editing " + timbreName);
    }

    private void ListBoxTimbres_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (listBoxTimbres.SelectedIndex < 0)
        {
            return;
        }
        PopulateTimbreFormParameters(listBoxTimbres.SelectedIndex);
    }

    private void ButtonCopyTimbre_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        int selectedTimbre = (int)(numericUpDownTimbreNo.Value ?? 1) - 1;
        copiedTimbre = memoryState.GetMemoryTimbre(selectedTimbre).Clone();
        buttonPasteTimbre.IsEnabled = true;
    }

    private void ButtonPasteTimbre_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        int selectedTimbre = (int)(numericUpDownTimbreNo.Value ?? 1) - 1;
        if (copiedTimbre is null)
        {
            return;
        }
        if (labelTimbreName.Text != MT32Strings.EMPTY && CancelOverwrite())
        {
            return;
        }
        memoryState.SetMemoryTimbre(copiedTimbre.Clone(), selectedTimbre);
        PopulateMemoryBankListBox(selectedTimbre);
        SynchroniseTimbreEditor(selectedTimbre);

        bool CancelOverwrite()
        {
            string currentTimbreName = memoryState.GetMemoryTimbre(selectedTimbre).GetTimbreName();
            return !PlatformServices.Notification.AskUserToConfirm($"Overwrite {currentTimbreName} with copied timbre {copiedTimbre.GetTimbreName()}?", "MT-32 Editor");
        }
    }

    private void ButtonClearTimbre_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (labelTimbreName.Text == MT32Strings.EMPTY || !PlatformServices.Notification.AskUserToConfirm("Clear selected memory timbre?", "MT-32 Editor"))
        {
            return;
        }
        int selectedTimbre = (int)(numericUpDownTimbreNo.Value ?? 1) - 1;
        string timbreName = memoryState.GetMemoryTimbre(selectedTimbre).GetTimbreName();
        memoryState.SetMemoryTimbre(new TimbreStructure(createAudibleTimbre: false), selectedTimbre);
        memoryState.GetTimbreNames().SetMemoryTimbreName(timbreName, selectedTimbre);
        PopulateMemoryBankListBox(selectedTimbre);
        SynchroniseTimbreEditor(selectedTimbre);
        MT32SysEx.SendMemoryTimbre(selectedTimbre, memoryState.GetMemoryTimbre(selectedTimbre));
        MT32SysEx.PreviewTimbre(selectedTimbre, memoryState.GetMemoryTimbre(selectedTimbre));
    }

    private void ButtonClearAll_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!PlatformServices.Notification.AskUserToConfirm("Clear all memory timbres?", "MT-32 Editor"))
        {
            return;
        }
        memoryState.SetMemoryTimbreArray(new TimbreStructure[MT32State.NO_OF_MEMORY_TIMBRES]);
        InitialiseMemoryTimbreArray();
        memoryState.GetTimbreNames().ResetAllMemoryTimbreNames();
        memoryState.SetSelectedMemoryTimbre(0);
        memoryState.requestPatchRefresh = true;
        memoryState.requestRhythmRefresh = true;
        PopulateMemoryBankListBox(0);
        SynchroniseTimbreEditor(0);
        PlatformServices.SysExLoadForm.ShowLoadDialog(memoryState, requestClearMemory: true);
        MT32SysEx.PreviewTimbre(0, memoryState.GetMemoryTimbre(0));
    }

    /// <summary>
    /// Initialises all 64 memory timbre slots with blank timbres.
    /// </summary>
    public void InitialiseMemoryTimbreArray()
    {
        for (int timbreNo = 0; timbreNo < MT32State.NO_OF_MEMORY_TIMBRES; timbreNo++)
        {
            memoryState.SetMemoryTimbre(new TimbreStructure(createAudibleTimbre: false), timbreNo);
        }
    }

    /// <summary>
    /// Called when this panel becomes active (e.g. tab selected).
    /// </summary>
    public void OnPanelActivated()
    {
        int selectedTimbre = (int)(numericUpDownTimbreNo.Value ?? 1) - 1;
        memoryState.SetMemoryTimbre(timbreEditor.TimbreData, selectedTimbre);
        memoryState.rhythmEditorActive = false;
        memoryState.patchEditorActive = false;
        memoryState.memoryBankEditorActive = true;
        memoryState.SetTimbreIsEditable(true);
        MT32SysEx.SendMemoryTimbre(selectedTimbre, memoryState.GetMemoryTimbre(selectedTimbre));
        MT32SysEx.PreviewTimbre(selectedTimbre, memoryState.GetMemoryTimbre(selectedTimbre));
    }
}

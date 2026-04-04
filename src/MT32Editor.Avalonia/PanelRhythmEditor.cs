using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace MT32Edit.Avalonia;

/// <summary>
/// Rhythm bank editor panel - Avalonia equivalent of WinForms FormRhythmEditor.
/// Displayed as a tab in the main window. Allows custom rhythm instruments to be configured.
/// </summary>
public class PanelRhythmEditor : UserControl
{
    private const int BANK_OFFSET = 2;

    private MT32State memoryState = null!;
    private DateTime lastGlobalUpdate = DateTime.Now;
    private bool thisFormIsActive = false;
    private bool darkMode = !UISettings.DarkMode;
    private int pressedKey = -1;

    // Controls
    private readonly TextBlock labelHeading;
    private readonly TextBlock labelNoChannelAssigned;
    private readonly TextBlock labelUnitNoWarning;
    private readonly TextBlock labelMT32ModeWarning1;
    private readonly TextBlock labelMT32ModeWarning2;
    private readonly ListBox listBoxRhythmBank;
    private readonly NumericUpDown numericUpDownKeyNo;
    private readonly ComboBox comboBoxTimbreGroup;
    private readonly ComboBox comboBoxTimbreName;
    private readonly Slider sliderLevel;
    private readonly Slider sliderPanPot;
    private readonly RadioButton radioButtonReverbOn;
    private readonly RadioButton radioButtonReverbOff;
    private readonly Button buttonPlayNote;
    private readonly DispatcherTimer timer;

    // Rhythm bank list data
    private readonly List<RhythmListItem> rhythmItems = new();

    public PanelRhythmEditor()
    {
        var foreground = AvaloniaUITools.GetForegroundBrush();

        var mainPanel = new DockPanel { Margin = new Thickness(10, 8, 10, 8) };

        // Top controls area
        var topPanel = new StackPanel { Spacing = 4 };
        DockPanel.SetDock(topPanel, Dock.Top);

        // Heading
        labelHeading = new TextBlock
        {
            Text = "Rhythm Bank Editor",
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            Foreground = AvaloniaUITools.GetTitleBrush(),
            Margin = new Thickness(0, 0, 0, 4)
        };
        topPanel.Children.Add(labelHeading);

        // Warning labels
        labelUnitNoWarning = new TextBlock
        {
            Text = "⚠ Unit No. set to non-default value! To revert, edit/delete MT32Edit.ini and reload.",
            Foreground = AvaloniaUITools.GetWarningBrush(),
            IsVisible = false,
            Margin = new Thickness(0, 0, 0, 2)
        };
        topPanel.Children.Add(labelUnitNoWarning);

        labelNoChannelAssigned = new TextBlock
        {
            Text = "Rhythm part muted- no MIDI channel assigned. Open System Area Settings to resolve.",
            Foreground = AvaloniaUITools.GetWarningBrush(),
            IsVisible = false,
            Margin = new Thickness(0, 0, 0, 2)
        };
        topPanel.Children.Add(labelNoChannelAssigned);

        // Key No row
        var keyNoRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 4, 0, 4) };
        keyNoRow.Children.Add(new TextBlock { Text = "Key No.", Foreground = foreground, VerticalAlignment = VerticalAlignment.Center });
        numericUpDownKeyNo = new NumericUpDown { Minimum = 24, Maximum = 108, Value = 24, Width = 70, FontSize = 14, FontWeight = FontWeight.Bold };
        numericUpDownKeyNo.ValueChanged += NumericUpDownKeyNo_ValueChanged;
        keyNoRow.Children.Add(numericUpDownKeyNo);
        topPanel.Children.Add(keyNoRow);

        // Timbre Group / Timbre Name row
        var timbreRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, Margin = new Thickness(0, 2, 0, 4) };

        var timbreGroupPanel = new StackPanel { Spacing = 2 };
        timbreGroupPanel.Children.Add(new TextBlock { Text = "Timbre Group", Foreground = foreground });
        comboBoxTimbreGroup = new ComboBox { Width = 130 };
        comboBoxTimbreGroup.ItemsSource = new[] { "Memory", "Rhythm" };
        comboBoxTimbreGroup.SelectionChanged += ComboBoxTimbreGroup_SelectionChanged;
        timbreGroupPanel.Children.Add(comboBoxTimbreGroup);
        timbreRow.Children.Add(timbreGroupPanel);

        var timbreNamePanel = new StackPanel { Spacing = 2 };
        timbreNamePanel.Children.Add(new TextBlock { Text = "Timbre Name", Foreground = foreground });
        comboBoxTimbreName = new ComboBox { Width = 150 };
        comboBoxTimbreName.SelectionChanged += ComboBoxTimbreName_SelectionChanged;
        timbreNamePanel.Children.Add(comboBoxTimbreName);
        timbreRow.Children.Add(timbreNamePanel);

        topPanel.Children.Add(timbreRow);

        // Level, Reverb, Pan controls row
        var controlsRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 20, Margin = new Thickness(0, 4, 0, 4) };

        // Level slider (vertical)
        var levelPanel = new StackPanel { Spacing = 2, HorizontalAlignment = HorizontalAlignment.Center };
        levelPanel.Children.Add(new TextBlock { Text = "Level", Foreground = foreground, HorizontalAlignment = HorizontalAlignment.Center });
        sliderLevel = new Slider { Minimum = 0, Maximum = 100, Value = 75, Orientation = Orientation.Vertical, Height = 100, HorizontalAlignment = HorizontalAlignment.Center };
        sliderLevel.PropertyChanged += SliderLevel_ValueChanged;
        levelPanel.Children.Add(sliderLevel);
        controlsRow.Children.Add(levelPanel);

        // Reverb On/Off
        var reverbPanel = new StackPanel { Spacing = 2 };
        reverbPanel.Children.Add(new TextBlock { Text = "Reverb", Foreground = foreground });
        radioButtonReverbOn = new RadioButton { Content = "On", Foreground = foreground, GroupName = "RhythmReverb" };
        radioButtonReverbOn.IsCheckedChanged += RadioButtonReverbOn_CheckedChanged;
        radioButtonReverbOff = new RadioButton { Content = "Off", Foreground = foreground, GroupName = "RhythmReverb" };
        reverbPanel.Children.Add(radioButtonReverbOn);
        reverbPanel.Children.Add(radioButtonReverbOff);
        controlsRow.Children.Add(reverbPanel);

        // Pan Pot slider (horizontal)
        var panPanel = new StackPanel { Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
        sliderPanPot = new Slider { Minimum = -7, Maximum = 7, Value = 0, Width = 120 };
        sliderPanPot.PropertyChanged += SliderPanPot_ValueChanged;
        panPanel.Children.Add(sliderPanPot);
        panPanel.Children.Add(new TextBlock { Text = "L         Pan         R", Foreground = foreground, HorizontalAlignment = HorizontalAlignment.Center, FontSize = 11 });
        controlsRow.Children.Add(panPanel);

        topPanel.Children.Add(controlsRow);

        // Play button and warnings row
        var playRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 2, 0, 2) };
        buttonPlayNote = new Button
        {
            Content = "▶",
            Width = 41,
            Height = 30,
            FontSize = 16
        };
        ToolTip.SetTip(buttonPlayNote, "Play selected sound on device");
        buttonPlayNote.PointerPressed += ButtonPlayNote_PointerPressed;
        buttonPlayNote.PointerReleased += ButtonPlayNote_PointerReleased;
        playRow.Children.Add(buttonPlayNote);
        topPanel.Children.Add(playRow);

        // MT-32 mode warnings
        labelMT32ModeWarning1 = new TextBlock
        {
            Text = "MT-32 mode selected: Unsupported timbres shown in red.",
            Foreground = new SolidColorBrush(Color.FromRgb(205, 92, 92)),
            Margin = new Thickness(0, 2, 0, 0)
        };
        topPanel.Children.Add(labelMT32ModeWarning1);

        labelMT32ModeWarning2 = new TextBlock
        {
            Text = "Unsupported rhythm banks shown in grey.",
            Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128)),
            Margin = new Thickness(0, 0, 0, 4)
        };
        topPanel.Children.Add(labelMT32ModeWarning2);

        mainPanel.Children.Add(topPanel);

        // Rhythm bank ListBox (fills remaining space)
        listBoxRhythmBank = new ListBox
        {
            Background = AvaloniaUITools.GetListViewBackgroundBrush(),
            Foreground = AvaloniaUITools.GetForegroundBrush(),
            SelectionMode = SelectionMode.Single
        };
        listBoxRhythmBank.SelectionChanged += ListBoxRhythmBank_SelectionChanged;
        mainPanel.Children.Add(listBoxRhythmBank);

        Content = mainPanel;

        // Timer for periodic updates
        timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        timer.Tick += Timer_Tick;
    }

    /// <summary>
    /// Initialize with memory state. Must be called after construction.
    /// </summary>
    public void Initialize(MT32State parentMemoryState)
    {
        memoryState = parentMemoryState;
        SetTheme();
        InitialiseRhythmBank();
        ConfigureWarnings();
        memoryState.changesMade = false;
        timer.Start();
    }

    private void SetTheme()
    {
        if (darkMode == UISettings.DarkMode) return;
        labelHeading.Foreground = AvaloniaUITools.GetTitleBrush();
        labelNoChannelAssigned.Foreground = AvaloniaUITools.GetWarningBrush();
        labelUnitNoWarning.Foreground = AvaloniaUITools.GetWarningBrush();
        Background = AvaloniaUITools.GetBackgroundBrush(alternate: true);
        darkMode = UISettings.DarkMode;
        InitialiseRhythmBank();
    }

    private void InitialiseRhythmBank()
    {
        if (memoryState is null) return;
        rhythmItems.Clear();
        for (int keyNo = RhythmConstants.KEY_OFFSET; keyNo < RhythmConstants.NO_OF_RHYTHM_KEYS + RhythmConstants.KEY_OFFSET; keyNo++)
        {
            rhythmItems.Add(CreateRhythmListItem(keyNo));
        }
        RefreshListBox();
        SelectKeyInListBox(RhythmConstants.KEY_OFFSET);
        PopulateRhythmFormParameters(RhythmConstants.KEY_OFFSET);
        comboBoxTimbreName.ItemsSource = memoryState.GetTimbreNames().GetAll(comboBoxTimbreGroup.SelectedIndex + BANK_OFFSET);
    }

    private void ConfigureWarnings()
    {
        if (MT32SysEx.DeviceID != MT32SysEx.DEFAULT_DEVICE_ID)
        {
            labelUnitNoWarning.IsVisible = true;
        }
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (!thisFormIsActive)
        {
            int selectedTimbre = memoryState.GetSelectedMemoryTimbre();
            CheckForMemoryStateUpdates();
            UpdateMemoryTimbreNames();
            FindMemoryTimbreInRhythmList(selectedTimbre);
            if (comboBoxTimbreGroup.SelectedItem?.ToString() == "Memory")
            {
                SyncMemoryTimbreNames();
            }
        }
        if (memoryState.returnFocusToRhythmEditor)
        {
            ReturnFocusToRhythmEditor();
        }
        CheckPartStatus();
        SetTheme();
    }

    private RhythmListItem CreateRhythmListItem(int keyNo)
    {
        int bankNo = keyNo - RhythmConstants.KEY_OFFSET;
        Rhythm rhythmKey = memoryState.GetRhythmBank(bankNo);
        int timbreNo = rhythmKey.GetTimbreNo();
        string timbreGroup = rhythmKey.GetTimbreGroupType();
        string timbreName = memoryState.GetTimbreNames().Get(timbreNo, rhythmKey.GetTimbreGroup() + BANK_OFFSET);

        return new RhythmListItem
        {
            KeyNo = keyNo.ToString(),
            Note = MT32Strings.PitchNote(keyNo),
            TimbreGroup = timbreGroup,
            TimbreName = timbreName,
            Reverb = MT32Strings.OnOffStatus(rhythmKey.GetReverbEnabled()),
            PanPot = rhythmKey.GetPanPot().ToString(),
            Level = rhythmKey.GetOutputLevel().ToString()
        };
    }

    private void RefreshListBox()
    {
        var displayItems = new List<string>();
        // Header
        displayItems.Add($"{"Key#",-6} {"Note",-8} {"Group",-10} {"Name",-14} {"Reverb",-8} {"Pan",-6} {"Level"}");
        foreach (var item in rhythmItems)
        {
            displayItems.Add($"{item.KeyNo,-6} {item.Note,-8} {item.TimbreGroup,-10} {item.TimbreName,-14} {item.Reverb,-8} {item.PanPot,-6} {item.Level}");
        }
        listBoxRhythmBank.ItemsSource = displayItems;
    }

    private void SelectKeyInListBox(int keyNo)
    {
        int index = keyNo - RhythmConstants.KEY_OFFSET + 1; // +1 for header
        if (index >= 0 && index < listBoxRhythmBank.ItemCount)
        {
            listBoxRhythmBank.SelectedIndex = index;
            listBoxRhythmBank.ScrollIntoView(listBoxRhythmBank.SelectedIndex);
        }
    }

    private void RefreshRhythmBankList()
    {
        rhythmItems.Clear();
        for (int keyNo = RhythmConstants.KEY_OFFSET; keyNo < RhythmConstants.NO_OF_RHYTHM_KEYS + RhythmConstants.KEY_OFFSET; keyNo++)
        {
            rhythmItems.Add(CreateRhythmListItem(keyNo));
        }
        RefreshListBox();
        int selectedKey = memoryState.GetSelectedKey();
        SelectKeyInListBox(selectedKey);
    }

    private void PopulateRhythmFormParameters(int keyNo)
    {
        int bankNo = keyNo - RhythmConstants.KEY_OFFSET;
        MT32SysEx.blockSysExMessages = true;
        numericUpDownKeyNo.Value = keyNo;
        Rhythm rhythmKey = memoryState.GetRhythmBank(bankNo);
        SetComboBoxTimbreGroupByName(rhythmKey.GetTimbreGroupType());
        comboBoxTimbreName.SelectedItem = memoryState.GetTimbreNames().Get(rhythmKey.GetTimbreNo(), rhythmKey.GetTimbreGroup() + BANK_OFFSET);
        sliderLevel.Value = rhythmKey.GetOutputLevel();
        sliderPanPot.Value = rhythmKey.GetPanPot();
        ToolTip.SetTip(sliderLevel, $"Level = {rhythmKey.GetOutputLevel()}");
        ToolTip.SetTip(sliderPanPot, $"Pan = {rhythmKey.GetPanPot()}");
        radioButtonReverbOn.IsChecked = rhythmKey.GetReverbEnabled();
        radioButtonReverbOff.IsChecked = !rhythmKey.GetReverbEnabled();
        MT32SysEx.blockSysExMessages = false;
    }

    private void SetComboBoxTimbreGroupByName(string timbreGroupType)
    {
        var items = comboBoxTimbreGroup.ItemsSource as string[];
        if (items is null) return;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == timbreGroupType)
            {
                comboBoxTimbreGroup.SelectedIndex = i;
                return;
            }
        }
    }

    private void SetListViewWarnings(int keyNo)
    {
        if (MT32SysEx.cm32LMode)
        {
            labelMT32ModeWarning1.IsVisible = false;
            labelMT32ModeWarning2.IsVisible = false;
            return;
        }
        labelMT32ModeWarning1.IsVisible = true;
        labelMT32ModeWarning2.IsVisible = true;
    }

    private void DoFullRefresh()
    {
        UpdateMemoryTimbreNames();
        RefreshRhythmBankList();
        RefreshTimbreNamesList();
    }

    private void UpdateMemoryTimbreNames()
    {
        for (int timbreNo = 0; timbreNo < MT32State.NO_OF_MEMORY_TIMBRES; timbreNo++)
        {
            memoryState.GetTimbreNames().SetMemoryTimbreName(memoryState.GetMemoryTimbre(timbreNo).GetTimbreName(), timbreNo);
        }
    }

    private void UpdateTimbreName()
    {
        int bankNo = memoryState.GetSelectedKey() - RhythmConstants.KEY_OFFSET;
        Rhythm rhythmData = memoryState.GetRhythmBank(bankNo);
        int selectedIndex = comboBoxTimbreName.SelectedIndex;
        if (selectedIndex < 0) return;
        rhythmData.SetTimbreNo(selectedIndex);
        comboBoxTimbreName.SelectedItem = memoryState.GetTimbreNames().Get(rhythmData.GetTimbreNo(), rhythmData.GetTimbreGroup());
    }

    private void CheckPartStatus()
    {
        if (memoryState.GetSystem().GetUIMidiChannel(8) == 0)
        {
            buttonPlayNote.IsEnabled = false;
            labelNoChannelAssigned.IsVisible = true;
            return;
        }
        buttonPlayNote.IsEnabled = true;
        labelNoChannelAssigned.IsVisible = false;
    }

    private void RefreshTimbreNamesList(int selectedBank = 0)
    {
        int groupIndex = comboBoxTimbreGroup.SelectedIndex;
        if (groupIndex < 0) groupIndex = 0;
        string[] memoryTimbreNameArray = memoryState.GetTimbreNames().GetAll(groupIndex + BANK_OFFSET);
        comboBoxTimbreName.ItemsSource = memoryTimbreNameArray;
        int timbreNo = memoryState.GetRhythmBank(selectedBank).GetTimbreNo();
        if (timbreNo >= 0 && timbreNo < memoryTimbreNameArray.Length)
        {
            comboBoxTimbreName.SelectedIndex = timbreNo;
        }
    }

    private void FindMemoryTimbreInRhythmList(int selectedTimbreNo)
    {
        for (int bankNo = 0; bankNo < RhythmConstants.NO_OF_RHYTHM_KEYS; bankNo++)
        {
            Rhythm rhythmData = memoryState.GetRhythmBank(bankNo);
            if (rhythmData.GetTimbreGroupType() == "Memory" && rhythmData.GetTimbreNo() == selectedTimbreNo)
            {
                int keyNo = bankNo + RhythmConstants.KEY_OFFSET;
                if (memoryState.rhythmEditorActive) return;
                if (numericUpDownKeyNo.Value == keyNo) return;
                numericUpDownKeyNo.Value = keyNo;
                memoryState.returnFocusToMemoryBankList = true;
                return;
            }
        }
    }

    private void CheckForMemoryStateUpdates()
    {
        if (memoryState.requestRhythmRefresh || lastGlobalUpdate < memoryState.GetUpdateTime())
        {
            ConsoleMessage.SendVerboseLine("Updating Rhythm Bank List");
            DoFullRefresh();
            lastGlobalUpdate = DateTime.Now;
            memoryState.requestRhythmRefresh = false;
        }
    }

    private void SyncMemoryTimbreNames()
    {
        int selectedBank = memoryState.GetSelectedBank();
        int selectedKey = selectedBank + RhythmConstants.KEY_OFFSET;
        string newTimbreName = memoryState.GetTimbreNames().Get(memoryState.GetRhythmBank(selectedBank).GetTimbreNo(), 2);

        if (selectedBank < rhythmItems.Count && rhythmItems[selectedBank].TimbreName != newTimbreName)
        {
            rhythmItems[selectedBank].TimbreName = newTimbreName;
        }
        SetListViewWarnings(selectedKey);

        string currentTimbreName = comboBoxTimbreName.SelectedItem?.ToString() ?? string.Empty;
        if (currentTimbreName != newTimbreName)
        {
            ConsoleMessage.SendVerboseLine("Updating Memory Timbre Names List");
            RefreshTimbreNamesList(selectedBank);
            // Select the matching item
            var items = comboBoxTimbreName.ItemsSource as string[];
            if (items is not null)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i] == newTimbreName)
                    {
                        comboBoxTimbreName.SelectedIndex = i;
                        break;
                    }
                }
            }
        }
    }

    private void ReturnFocusToRhythmEditor()
    {
        listBoxRhythmBank.Focus();
        memoryState.returnFocusToRhythmEditor = false;
    }

    // Event handlers

    private void ListBoxRhythmBank_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        int selectedIndex = listBoxRhythmBank.SelectedIndex;
        if (selectedIndex > 0) // Skip header row
        {
            int keyNo = selectedIndex - 1 + RhythmConstants.KEY_OFFSET;
            PopulateRhythmFormParameters(keyNo);
        }
    }

    private void NumericUpDownKeyNo_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (memoryState is null) return;
        int selectedKey = (int)(numericUpDownKeyNo.Value ?? 24);
        memoryState.SetSelectedKey(selectedKey);
        SelectKeyInListBox(selectedKey);
        PopulateRhythmFormParameters(selectedKey);
        RefreshTimbreNamesList(selectedKey - RhythmConstants.KEY_OFFSET);
    }

    private void ComboBoxTimbreGroup_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (memoryState is null) return;
        int selectedKey = memoryState.GetSelectedKey();
        int bankNo = selectedKey - RhythmConstants.KEY_OFFSET;
        int groupIndex = comboBoxTimbreGroup.SelectedIndex;
        if (groupIndex < 0) return;
        Rhythm rhythmData = memoryState.GetRhythmBank(bankNo);
        rhythmData.SetTimbreGroup(groupIndex);
        comboBoxTimbreName.ItemsSource = memoryState.GetTimbreNames().GetAll(groupIndex + BANK_OFFSET);
        UpdateRhythmItemInList(selectedKey);
        SetListViewWarnings(selectedKey);
        PopulateRhythmFormParameters(selectedKey);
        MT32SysEx.SendRhythmKey(rhythmData, selectedKey);
        memoryState.changesMade = true;
    }

    private void ComboBoxTimbreName_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (memoryState is null) return;
        int selectedIndex = comboBoxTimbreName.SelectedIndex;
        if (selectedIndex < 0) return;
        int selectedKey = memoryState.GetSelectedKey();
        Rhythm rhythmData = memoryState.GetRhythmKey(selectedKey);
        rhythmData.SetTimbreNo(selectedIndex);
        UpdateRhythmItemInList(selectedKey);
        SetListViewWarnings(selectedKey);
        MT32SysEx.SendRhythmKey(rhythmData, selectedKey);
        memoryState.changesMade = true;
    }

    private void RadioButtonReverbOn_CheckedChanged(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (memoryState is null) return;
        int selectedKey = memoryState.GetSelectedKey();
        int bankNo = selectedKey - RhythmConstants.KEY_OFFSET;
        Rhythm rhythmData = memoryState.GetRhythmBank(bankNo);
        bool reverbOn = radioButtonReverbOn.IsChecked == true;
        rhythmData.SetReverbEnabled(reverbOn);
        UpdateRhythmItemInList(selectedKey);
        MT32SysEx.SendRhythmKey(rhythmData, selectedKey);
    }

    private void SliderPanPot_ValueChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != Slider.ValueProperty || memoryState is null) return;
        int selectedKey = memoryState.GetSelectedKey();
        int bankNo = selectedKey - RhythmConstants.KEY_OFFSET;
        Rhythm rhythmData = memoryState.GetRhythmBank(bankNo);
        int value = (int)sliderPanPot.Value;
        rhythmData.SetPanPot(value);
        ToolTip.SetTip(sliderPanPot, $"Pan = {value}");
        UpdateRhythmItemInList(selectedKey);
        MT32SysEx.SendRhythmKey(rhythmData, selectedKey);
        memoryState.changesMade = true;
    }

    private void SliderLevel_ValueChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != Slider.ValueProperty || memoryState is null) return;
        int selectedKey = memoryState.GetSelectedKey();
        int bankNo = selectedKey - RhythmConstants.KEY_OFFSET;
        Rhythm rhythmData = memoryState.GetRhythmBank(bankNo);
        int value = (int)sliderLevel.Value;
        rhythmData.SetOutputLevel(value);
        ToolTip.SetTip(sliderLevel, $"Level = {value}");
        UpdateRhythmItemInList(selectedKey);
        MT32SysEx.SendRhythmKey(rhythmData, selectedKey);
        memoryState.changesMade = true;
    }

    private void ButtonPlayNote_PointerPressed(object? sender, global::Avalonia.Input.PointerPressedEventArgs e)
    {
        if (memoryState is null) return;
        int selectedKey = memoryState.GetSelectedKey();
        int midiChannel = memoryState.GetSystem().GetSysExMidiChannel(8);
        Rhythm rhythmData = memoryState.GetRhythmKey(selectedKey);
        if (rhythmData.GetTimbreGroupType() == "Memory")
        {
            int timbreNo = rhythmData.GetTimbreNo();
            MT32SysEx.SendMemoryTimbre(timbreNo, memoryState.GetMemoryTimbre(timbreNo));
        }
        Midi.NoteOn(selectedKey, midiChannel);
        pressedKey = selectedKey;
    }

    private void ButtonPlayNote_PointerReleased(object? sender, global::Avalonia.Input.PointerReleasedEventArgs e)
    {
        if (memoryState is null) return;
        int midiChannel = memoryState.GetSystem().GetSysExMidiChannel(8);
        if (pressedKey >= 0)
        {
            Midi.NoteOff(pressedKey, midiChannel);
        }
        pressedKey = -1;
    }

    private void UpdateRhythmItemInList(int keyNo)
    {
        int bankNo = keyNo - RhythmConstants.KEY_OFFSET;
        if (bankNo >= 0 && bankNo < rhythmItems.Count)
        {
            rhythmItems[bankNo] = CreateRhythmListItem(keyNo);
            RefreshListBox();
            SelectKeyInListBox(keyNo);
        }
    }

    /// <summary>
    /// Called when this panel is activated (tab selected).
    /// </summary>
    public void OnActivated()
    {
        thisFormIsActive = true;
        if (memoryState is null) return;
        memoryState.rhythmEditorActive = true;
        memoryState.patchEditorActive = false;
    }

    /// <summary>
    /// Called when this panel is deactivated (another tab selected).
    /// </summary>
    public void OnDeactivated()
    {
        thisFormIsActive = false;
    }

    /// <summary>
    /// Simple data model for rhythm list items.
    /// </summary>
    private class RhythmListItem
    {
        public string KeyNo { get; set; } = "";
        public string Note { get; set; } = "";
        public string TimbreGroup { get; set; } = "";
        public string TimbreName { get; set; } = "";
        public string Reverb { get; set; } = "";
        public string PanPot { get; set; } = "";
        public string Level { get; set; } = "";
    }
}

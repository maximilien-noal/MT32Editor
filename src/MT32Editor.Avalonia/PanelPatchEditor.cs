using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace MT32Edit.Avalonia;

/// <summary>
/// Patch editor panel - Avalonia equivalent of WinForms FormPatchEditor.
/// Displayed as a tab in the main window. Shows 128 patch banks and per-patch editing controls.
/// </summary>
public class PanelPatchEditor : UserControl
{
    private MT32State memoryState = null!;
    private DateTime lastGlobalUpdate = DateTime.Now;
    private bool thisFormIsActive = true;
    private bool darkMode = !UISettings.DarkMode;

    private const string TEXT_EDIT_PRESET = "Edit Preset Timbre";
    private const string TEXT_RESTORE_PRESET = "Restore Preset Timbre";

    // Controls
    private readonly TextBlock labelHeading;
    private readonly TextBlock labelNoChannelAssigned;
    private readonly TextBlock labelUnitNoWarning;
    private readonly TextBlock labelMT32ModeWarning;
    private readonly ListBox listBoxPatches;
    private readonly NumericUpDown numericUpDownPatchNo;
    private readonly ComboBox comboBoxTimbreGroup;
    private readonly ComboBox comboBoxTimbreName;
    private readonly Slider sliderKeyShift;
    private readonly Slider sliderFineTune;
    private readonly Slider sliderBenderRange;
    private readonly RadioButton radioButtonReverbOn;
    private readonly RadioButton radioButtonReverbOff;
    private readonly ComboBox comboBoxAssignMode;
    private readonly Button buttonEditPreset;
    private readonly DispatcherTimer timer;

    // Patch list data model
    private readonly List<PatchListItem> patchItems = new();

    public PanelPatchEditor()
    {
        var foreground = AvaloniaUITools.GetForegroundBrush();

        var mainPanel = new DockPanel { Margin = new Thickness(10, 8, 10, 8) };

        // Top controls area
        var topPanel = new StackPanel { Spacing = 4 };
        DockPanel.SetDock(topPanel, Dock.Top);

        // Heading
        labelHeading = new TextBlock
        {
            Text = "Patch Editor",
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            Foreground = AvaloniaUITools.GetTitleBrush(),
            Margin = new Thickness(0, 0, 0, 4)
        };
        topPanel.Children.Add(labelHeading);

        // Warning labels
        labelNoChannelAssigned = new TextBlock
        {
            Text = "Part 1 muted- no MIDI channel assigned. Assign a channel in System Settings.",
            Foreground = AvaloniaUITools.GetWarningBrush(),
            IsVisible = false,
            Margin = new Thickness(0, 0, 0, 2)
        };
        topPanel.Children.Add(labelNoChannelAssigned);

        labelUnitNoWarning = new TextBlock
        {
            Text = "⚠ Unit No. set to non-default value! To revert, edit/delete MT32Edit.ini and reload.",
            Foreground = AvaloniaUITools.GetWarningBrush(),
            IsVisible = false,
            Margin = new Thickness(0, 0, 0, 2)
        };
        topPanel.Children.Add(labelUnitNoWarning);

        // Patch No row
        var patchNoRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 4, 0, 4) };
        patchNoRow.Children.Add(new TextBlock { Text = "Patch No.", Foreground = foreground, VerticalAlignment = VerticalAlignment.Center });
        numericUpDownPatchNo = new NumericUpDown { Minimum = 1, Maximum = 128, Value = 1, Width = 70, FontSize = 14, FontWeight = FontWeight.Bold };
        numericUpDownPatchNo.ValueChanged += NumericUpDownPatchNo_ValueChanged;
        patchNoRow.Children.Add(numericUpDownPatchNo);
        topPanel.Children.Add(patchNoRow);

        // Timbre Group / Timbre Name / Edit button row
        var timbreRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, Margin = new Thickness(0, 2, 0, 4) };

        var timbreGroupPanel = new StackPanel { Spacing = 2 };
        timbreGroupPanel.Children.Add(new TextBlock { Text = "Timbre Group", Foreground = foreground });
        comboBoxTimbreGroup = new ComboBox { Width = 130 };
        comboBoxTimbreGroup.ItemsSource = new[] { "Preset A", "Preset B", "Memory", "Rhythm" };
        comboBoxTimbreGroup.SelectionChanged += ComboBoxTimbreGroup_SelectionChanged;
        timbreGroupPanel.Children.Add(comboBoxTimbreGroup);
        timbreRow.Children.Add(timbreGroupPanel);

        var timbreNamePanel = new StackPanel { Spacing = 2 };
        timbreNamePanel.Children.Add(new TextBlock { Text = "Timbre Name", Foreground = foreground });
        comboBoxTimbreName = new ComboBox { Width = 150 };
        comboBoxTimbreName.SelectionChanged += ComboBoxTimbreName_SelectionChanged;
        timbreNamePanel.Children.Add(comboBoxTimbreName);
        timbreRow.Children.Add(timbreNamePanel);

        buttonEditPreset = new Button
        {
            Content = TEXT_EDIT_PRESET,
            Width = 150,
            Height = 25,
            Background = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
            Foreground = Brushes.Black,
            VerticalAlignment = VerticalAlignment.Bottom
        };
        buttonEditPreset.Click += ButtonEditPreset_Click;
        timbreRow.Children.Add(buttonEditPreset);

        topPanel.Children.Add(timbreRow);

        // Sliders row: Key Shift, Fine Tune, Bend Range, Reverb, Assign Mode
        var slidersRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16, Margin = new Thickness(0, 4, 0, 4) };

        // Key Shift
        var keyShiftPanel = new StackPanel { Spacing = 2, HorizontalAlignment = HorizontalAlignment.Center };
        keyShiftPanel.Children.Add(new TextBlock { Text = "Key Shift", Foreground = foreground, HorizontalAlignment = HorizontalAlignment.Center });
        sliderKeyShift = new Slider { Minimum = -24, Maximum = 24, Value = 0, Orientation = Orientation.Vertical, Height = 100, HorizontalAlignment = HorizontalAlignment.Center };
        sliderKeyShift.PropertyChanged += SliderKeyShift_ValueChanged;
        keyShiftPanel.Children.Add(sliderKeyShift);
        slidersRow.Children.Add(keyShiftPanel);

        // Fine Tune
        var fineTunePanel = new StackPanel { Spacing = 2, HorizontalAlignment = HorizontalAlignment.Center };
        fineTunePanel.Children.Add(new TextBlock { Text = "Fine Tune", Foreground = foreground, HorizontalAlignment = HorizontalAlignment.Center });
        sliderFineTune = new Slider { Minimum = -50, Maximum = 50, Value = 0, Orientation = Orientation.Vertical, Height = 100, HorizontalAlignment = HorizontalAlignment.Center };
        sliderFineTune.PropertyChanged += SliderFineTune_ValueChanged;
        fineTunePanel.Children.Add(sliderFineTune);
        slidersRow.Children.Add(fineTunePanel);

        // Bend Range
        var bendRangePanel = new StackPanel { Spacing = 2, HorizontalAlignment = HorizontalAlignment.Center };
        bendRangePanel.Children.Add(new TextBlock { Text = "Bend Range", Foreground = foreground, HorizontalAlignment = HorizontalAlignment.Center });
        sliderBenderRange = new Slider { Minimum = 0, Maximum = 24, Value = 12, Orientation = Orientation.Vertical, Height = 100, HorizontalAlignment = HorizontalAlignment.Center };
        sliderBenderRange.PropertyChanged += SliderBenderRange_ValueChanged;
        bendRangePanel.Children.Add(sliderBenderRange);
        slidersRow.Children.Add(bendRangePanel);

        // Reverb On/Off
        var reverbPanel = new StackPanel { Spacing = 2 };
        reverbPanel.Children.Add(new TextBlock { Text = "Reverb", Foreground = foreground });
        radioButtonReverbOn = new RadioButton { Content = "On", Foreground = foreground, GroupName = "PatchReverb" };
        radioButtonReverbOn.IsCheckedChanged += RadioButtonReverbOn_CheckedChanged;
        radioButtonReverbOff = new RadioButton { Content = "Off", Foreground = foreground, GroupName = "PatchReverb" };
        reverbPanel.Children.Add(radioButtonReverbOn);
        reverbPanel.Children.Add(radioButtonReverbOff);
        slidersRow.Children.Add(reverbPanel);

        // Assign Mode
        var assignModePanel = new StackPanel { Spacing = 2 };
        assignModePanel.Children.Add(new TextBlock { Text = "Assign Mode", Foreground = foreground });
        comboBoxAssignMode = new ComboBox { Width = 160 };
        comboBoxAssignMode.ItemsSource = new[] { "1: Single Assign", "2: Multi Assign", "3: First In, First Out", "4: First In, Last Out" };
        comboBoxAssignMode.SelectionChanged += ComboBoxAssignMode_SelectionChanged;
        assignModePanel.Children.Add(comboBoxAssignMode);
        slidersRow.Children.Add(assignModePanel);

        topPanel.Children.Add(slidersRow);

        // MT-32 mode warning
        labelMT32ModeWarning = new TextBlock
        {
            Text = "MT-32 mode selected: Timbres containing CM-32L specific samples are shown in red.",
            Foreground = new SolidColorBrush(Color.FromRgb(255, 90, 90)),
            Margin = new Thickness(0, 2, 0, 4)
        };
        topPanel.Children.Add(labelMT32ModeWarning);

        mainPanel.Children.Add(topPanel);

        // Patch ListBox (fills remaining space)
        listBoxPatches = new ListBox
        {
            Background = AvaloniaUITools.GetListViewBackgroundBrush(),
            Foreground = AvaloniaUITools.GetForegroundBrush(),
            SelectionMode = SelectionMode.Single
        };
        listBoxPatches.SelectionChanged += ListBoxPatches_SelectionChanged;
        mainPanel.Children.Add(listBoxPatches);

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
        InitialisePatchArray();
        ConfigureWarnings();
        memoryState.changesMade = false;
        timer.Start();
    }

    private void SetTheme()
    {
        if (darkMode == UISettings.DarkMode) return;
        var foreground = AvaloniaUITools.GetForegroundBrush();
        labelHeading.Foreground = AvaloniaUITools.GetTitleBrush();
        labelNoChannelAssigned.Foreground = AvaloniaUITools.GetWarningBrush();
        labelUnitNoWarning.Foreground = AvaloniaUITools.GetWarningBrush();
        Background = AvaloniaUITools.GetBackgroundBrush(alternate: true);
        darkMode = UISettings.DarkMode;
    }

    private void InitialisePatchArray()
    {
        patchItems.Clear();
        for (int patchNo = 0; patchNo < MT32State.NO_OF_PATCHES; patchNo++)
        {
            patchItems.Add(CreatePatchListItem(patchNo));
        }
        RefreshListBox();
        int selectedPatch = memoryState.GetSelectedPatchNo();
        SelectPatchInListBox(selectedPatch);
        PopulatePatchFormParameters(selectedPatch);
        PopulateTimbreNamesList();
        int midiChannel = memoryState.GetSystem().GetSysExMidiChannel(0);
        Midi.SendProgramChange(selectedPatch, midiChannel);
    }

    private void ConfigureWarnings()
    {
        if (MT32SysEx.DeviceID != MT32SysEx.DEFAULT_DEVICE_ID)
        {
            labelUnitNoWarning.IsVisible = true;
        }
        if (MT32SysEx.cm32LMode)
        {
            labelMT32ModeWarning.IsVisible = false;
            return;
        }
        labelMT32ModeWarning.IsVisible = true;
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (!thisFormIsActive)
        {
            int selectedTimbre = memoryState.GetSelectedMemoryTimbre();
            CheckForMemoryStateUpdates();
            UpdateMemoryTimbreNames();
            FindMemoryTimbreInPatchList(selectedTimbre);
            if (comboBoxTimbreGroup.SelectedItem?.ToString() == "Memory")
            {
                SyncMemoryTimbreNames();
            }
        }
        if (memoryState.returnFocusToPatchEditor)
        {
            ReturnFocusToPatchEditor();
        }
        CheckPartStatus();
        SetTheme();
        ColourListBoxItems();
    }

    private PatchListItem CreatePatchListItem(int patchNo)
    {
        Patch memoryPatch = memoryState.GetPatch(patchNo);
        return new PatchListItem
        {
            PatchNo = (patchNo + 1).ToString(),
            TimbreGroup = memoryPatch.GetTimbreGroupType(),
            TimbreName = memoryState.GetTimbreNames().Get(memoryPatch.GetTimbreNo(), memoryPatch.GetTimbreGroup()),
            KeyShift = memoryPatch.GetKeyShift().ToString(),
            FineTune = memoryPatch.GetFineTune().ToString(),
            BendRange = memoryPatch.GetBenderRange().ToString(),
            Reverb = MT32Strings.OnOffStatus(memoryPatch.GetReverbEnabled()),
            AssignMode = (memoryPatch.GetAssignMode() + 1).ToString()
        };
    }

    private void RefreshListBox()
    {
        var displayItems = new List<string>();
        foreach (var item in patchItems)
        {
            displayItems.Add($"{item.PatchNo,-6} {item.TimbreGroup,-12} {item.TimbreName,-14} {item.KeyShift,-6} {item.FineTune,-6} {item.BendRange,-6} {item.Reverb,-8} {item.AssignMode}");
        }

        // Add header
        var allItems = new List<string>();
        allItems.Add($"{"Patch",-6} {"Group",-12} {"Name",-14} {"Key",-6} {"Fine",-6} {"Bend",-6} {"Reverb",-8} {"Mode"}");
        allItems.AddRange(displayItems);
        listBoxPatches.ItemsSource = allItems;
    }

    private void SelectPatchInListBox(int patchNo)
    {
        if (listBoxPatches.ItemCount > patchNo + 1)
        {
            listBoxPatches.SelectedIndex = patchNo + 1; // +1 for header row
            listBoxPatches.ScrollIntoView(listBoxPatches.SelectedIndex);
        }
    }

    private void PopulatePatchFormParameters(int patchNo)
    {
        MT32SysEx.blockSysExMessages = true;
        memoryState.SetSelectedPatchNo(patchNo);
        Patch memoryPatch = memoryState.GetPatch(patchNo);
        numericUpDownPatchNo.Value = patchNo + 1;
        string timbreGroupType = memoryPatch.GetTimbreGroupType();
        comboBoxTimbreName.SelectedItem = memoryState.GetTimbreNames().Get(memoryPatch.GetTimbreNo(), memoryPatch.GetTimbreGroup());
        SetComboBoxTimbreGroupByName(timbreGroupType);
        sliderBenderRange.Value = memoryPatch.GetBenderRange();
        sliderFineTune.Value = memoryPatch.GetFineTune();
        sliderKeyShift.Value = memoryPatch.GetKeyShift();
        ToolTip.SetTip(sliderBenderRange, $"Bend Range = {memoryPatch.GetBenderRange()}");
        ToolTip.SetTip(sliderFineTune, $"Fine Tune = {memoryPatch.GetFineTune()}");
        ToolTip.SetTip(sliderKeyShift, $"Key Shift = {memoryPatch.GetKeyShift()}");
        comboBoxAssignMode.SelectedIndex = memoryPatch.GetAssignMode();
        radioButtonReverbOn.IsChecked = memoryPatch.GetReverbEnabled();
        radioButtonReverbOff.IsChecked = !memoryPatch.GetReverbEnabled();
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

    private void RefreshPatchList()
    {
        patchItems.Clear();
        for (int patchNo = 0; patchNo < MT32State.NO_OF_PATCHES; patchNo++)
        {
            patchItems.Add(CreatePatchListItem(patchNo));
        }
        RefreshListBox();
        SelectPatchInListBox(memoryState.GetSelectedPatchNo());
    }

    private void SendPatch(int patchNo, bool sendSysExMessage)
    {
        if (sendSysExMessage)
        {
            MT32SysEx.SendPatchData(memoryState.GetPatchArray(), patchNo);
        }
        int midiChannel = memoryState.GetSystem().GetSysExMidiChannel(0);
        Patch memoryPatch = memoryState.GetPatch(patchNo);
        int selectedTimbre = memoryPatch.GetTimbreNo();
        Midi.SendProgramChange(patchNo, midiChannel);
        if (memoryPatch.GetTimbreGroupType() == "Memory" && sendSysExMessage)
        {
            MT32SysEx.PreviewTimbre(selectedTimbre, memoryState.GetMemoryTimbre(selectedTimbre));
        }
        MT32SysEx.SendText($"Patch {patchNo + 1}|{memoryState.GetTimbreNames().Get(memoryPatch.GetTimbreNo(), memoryPatch.GetTimbreGroup())}");
    }

    private void SendPatchParameterChange(int patchNo, int parameterNo)
    {
        MT32SysEx.SendPatchParameterData(memoryState.GetPatchArray(), patchNo, parameterNo);
    }

    private void UpdateTimbreName()
    {
        int selectedPatch = memoryState.GetSelectedPatchNo();
        int selectedIndex = comboBoxTimbreName.SelectedIndex;
        if (selectedIndex < 0) return;
        memoryState.GetPatch(selectedPatch).SetTimbreNo(selectedIndex);
        PopulatePatchFormParameters(selectedPatch);
        MT32SysEx.SendPatchData(memoryState.GetPatchArray(), selectedPatch);
    }

    private void UpdateMemoryTimbreNames()
    {
        for (int timbreNo = 0; timbreNo < MT32State.NO_OF_MEMORY_TIMBRES; timbreNo++)
        {
            string timbreName = memoryState.GetMemoryTimbre(timbreNo).GetTimbreName();
            memoryState.GetTimbreNames().SetMemoryTimbreName(timbreName, timbreNo);
        }
    }

    private void ColourListBoxItems()
    {
        // Color coding is handled in the list display text; for a more advanced UI
        // we could use custom item templates. The warning label serves the same purpose.
    }

    private void PopulateTimbreNamesList()
    {
        int groupIndex = comboBoxTimbreGroup.SelectedIndex;
        if (groupIndex < 0) groupIndex = 0;
        comboBoxTimbreName.ItemsSource = memoryState.GetTimbreNames().GetAll(groupIndex);
    }

    private void RefreshTimbreNamesList()
    {
        PopulateTimbreNamesList();
    }

    private void DoFullRefresh(int patchNo)
    {
        UpdateMemoryTimbreNames();
        RefreshPatchList();
        SendPatch(patchNo, sendSysExMessage: true);
        RefreshTimbreNamesList();
        ConfigureEditButton();
    }

    private void ConfigureEditButton()
    {
        int selectedPatch = (int)(numericUpDownPatchNo.Value ?? 1) - 1;
        string timbreGroupType = memoryState.GetPatch(selectedPatch).GetTimbreGroupType();
        if (timbreGroupType == "Preset A" || timbreGroupType == "Preset B")
        {
            buttonEditPreset.Content = TEXT_EDIT_PRESET;
            buttonEditPreset.IsEnabled = true;
        }
        else
        {
            buttonEditPreset.Content = TEXT_RESTORE_PRESET;
            buttonEditPreset.IsEnabled = true;
        }
    }

    private void CheckPartStatus()
    {
        if (memoryState.GetSystem().GetUIMidiChannel(0) == 0)
        {
            labelNoChannelAssigned.IsVisible = true;
            return;
        }
        labelNoChannelAssigned.IsVisible = false;
    }

    private void FindMemoryTimbreInPatchList(int selectedTimbreNo)
    {
        for (int patchNo = 0; patchNo < MT32State.NO_OF_PATCHES; patchNo++)
        {
            Patch patchData = memoryState.GetPatch(patchNo);
            if (patchData.GetTimbreGroupType() == "Memory" && patchData.GetTimbreNo() == selectedTimbreNo)
            {
                if (memoryState.patchEditorActive) return;
                if (numericUpDownPatchNo.Value == patchNo + 1) return;
                numericUpDownPatchNo.Value = patchNo + 1;
                memoryState.returnFocusToMemoryBankList = true;
                return;
            }
        }
    }

    private void CheckForMemoryStateUpdates()
    {
        if (memoryState.requestPatchRefresh || lastGlobalUpdate < memoryState.GetUpdateTime())
        {
            ConsoleMessage.SendVerboseLine("Updating Patch List");
            DoFullRefresh(memoryState.GetSelectedPatchNo());
            lastGlobalUpdate = DateTime.Now;
            memoryState.requestPatchRefresh = false;
        }
    }

    private void SyncMemoryTimbreNames()
    {
        int selectedPatch = memoryState.GetSelectedPatchNo();
        string newTimbreName = memoryState.GetTimbreNames().Get(memoryState.GetPatch(selectedPatch).GetTimbreNo(), 2);

        if (selectedPatch < patchItems.Count)
        {
            patchItems[selectedPatch].TimbreName = newTimbreName;
        }

        string currentTimbreName = comboBoxTimbreName.SelectedItem?.ToString() ?? string.Empty;
        if (currentTimbreName != newTimbreName)
        {
            RefreshTimbreNamesList();
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

    private void ReturnFocusToPatchEditor()
    {
        listBoxPatches.Focus();
        memoryState.returnFocusToPatchEditor = false;
    }

    // Event handlers

    private void ListBoxPatches_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        int selectedIndex = listBoxPatches.SelectedIndex;
        if (selectedIndex > 0) // Skip header row
        {
            int selectedPatch = selectedIndex - 1;
            PopulatePatchFormParameters(selectedPatch);
            SendPatch(selectedPatch, sendSysExMessage: false);
        }
        RefreshTimbreNamesList();
    }

    private void NumericUpDownPatchNo_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (memoryState is null) return;
        int selectedPatch = (int)(numericUpDownPatchNo.Value ?? 1) - 1;
        memoryState.SetSelectedPatchNo(selectedPatch);
        SendPatch(selectedPatch, sendSysExMessage: false);
        SelectPatchInListBox(selectedPatch);
        RefreshTimbreNamesList();
        string timbreGroupType = memoryState.GetPatch(selectedPatch).GetTimbreGroupType();
        memoryState.SetTimbreIsEditable(timbreGroupType == "Memory");
        ConfigureEditButton();
    }

    private bool _suppressTimbreGroupChange = false;

    private void ComboBoxTimbreGroup_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (memoryState is null || _suppressTimbreGroupChange) return;
        int selectedPatch = memoryState.GetSelectedPatchNo();
        Patch memoryPatch = memoryState.GetPatch(selectedPatch);
        int groupIndex = comboBoxTimbreGroup.SelectedIndex;
        if (groupIndex < 0) return;
        memoryPatch.SetTimbreGroup(groupIndex);
        memoryPatch.SetTimbreNo(0);
        PopulateTimbreNamesList();
        comboBoxTimbreName.SelectedIndex = 0;
        ConfigureEditButton();
        UpdatePatchItemInList(selectedPatch);
        SendPatch(selectedPatch, sendSysExMessage: true);
        memoryState.changesMade = true;
    }

    private void ComboBoxTimbreName_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (memoryState is null) return;
        int selectedIndex = comboBoxTimbreName.SelectedIndex;
        if (selectedIndex < 0) return;
        int selectedPatch = memoryState.GetSelectedPatchNo();
        memoryState.GetPatch(selectedPatch).SetTimbreNo(selectedIndex);
        UpdatePatchItemInList(selectedPatch);
        SendPatch(selectedPatch, sendSysExMessage: false);
        memoryState.changesMade = true;
    }

    private void SliderKeyShift_ValueChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != Slider.ValueProperty || memoryState is null) return;
        int selectedPatch = memoryState.GetSelectedPatchNo();
        int value = (int)sliderKeyShift.Value;
        memoryState.GetPatch(selectedPatch).SetKeyShift(value);
        ToolTip.SetTip(sliderKeyShift, $"Key Shift = {value}");
        UpdatePatchItemInList(selectedPatch);
        SendPatchParameterChange(selectedPatch, 0x02);
        memoryState.changesMade = true;
    }

    private void SliderFineTune_ValueChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != Slider.ValueProperty || memoryState is null) return;
        int selectedPatch = memoryState.GetSelectedPatchNo();
        int value = (int)sliderFineTune.Value;
        memoryState.GetPatch(selectedPatch).SetFineTune(value);
        ToolTip.SetTip(sliderFineTune, $"Fine Tune = {value}");
        UpdatePatchItemInList(selectedPatch);
        SendPatchParameterChange(selectedPatch, 0x03);
        memoryState.changesMade = true;
    }

    private void SliderBenderRange_ValueChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != Slider.ValueProperty || memoryState is null) return;
        int selectedPatch = memoryState.GetSelectedPatchNo();
        int value = (int)sliderBenderRange.Value;
        memoryState.GetPatch(selectedPatch).SetBenderRange(value);
        ToolTip.SetTip(sliderBenderRange, $"Bend Range = {value}");
        UpdatePatchItemInList(selectedPatch);
        SendPatchParameterChange(selectedPatch, 0x04);
        memoryState.changesMade = true;
    }

    private void ComboBoxAssignMode_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (memoryState is null) return;
        int index = comboBoxAssignMode.SelectedIndex;
        if (index < 0) return;
        int selectedPatch = memoryState.GetSelectedPatchNo();
        memoryState.GetPatch(selectedPatch).SetAssignMode(index);
        UpdatePatchItemInList(selectedPatch);
        SendPatchParameterChange(selectedPatch, 0x05);
        memoryState.changesMade = true;
    }

    private void RadioButtonReverbOn_CheckedChanged(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (memoryState is null) return;
        int selectedPatch = memoryState.GetSelectedPatchNo();
        bool reverbOn = radioButtonReverbOn.IsChecked == true;
        memoryState.GetPatch(selectedPatch).SetReverbEnabled(reverbOn);
        UpdatePatchItemInList(selectedPatch);
        SendPatchParameterChange(selectedPatch, 0x06);
        memoryState.changesMade = true;
    }

    private void ButtonEditPreset_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        int selectedPatch = memoryState.GetSelectedPatchNo();
        if ((string?)buttonEditPreset.Content == TEXT_EDIT_PRESET)
        {
            EditPresetTimbre(selectedPatch);
        }
        else
        {
            RestorePresetTimbre(selectedPatch);
        }
        ConfigureEditButton();
        DoFullRefresh(selectedPatch);
    }

    private void EditPresetTimbre(int patchNo)
    {
        Patch currentPatch = memoryState.GetPatch(patchNo);
        string patchName = memoryState.GetTimbreNames().Get(currentPatch.GetTimbreNo(), currentPatch.GetTimbreGroup());
        var selectMemoryBank = new WindowSelectMemoryBank(memoryState, patchName);
        var parentWindow = TopLevel.GetTopLevel(this) as Window;
        if (parentWindow is not null)
        {
            selectMemoryBank.ShowDialog(parentWindow);
        }
    }

    private void RestorePresetTimbre(int selectedPatch)
    {
        if (selectedPatch < MT32State.NO_OF_MEMORY_TIMBRES)
        {
            memoryState.GetPatch(selectedPatch).SetTimbreGroup(0);
            memoryState.GetPatch(selectedPatch).SetTimbreNo(selectedPatch);
        }
        else
        {
            memoryState.GetPatch(selectedPatch).SetTimbreGroup(1);
            memoryState.GetPatch(selectedPatch).SetTimbreNo(selectedPatch - MT32State.NO_OF_MEMORY_TIMBRES);
        }
    }

    private void UpdatePatchItemInList(int patchNo)
    {
        if (patchNo >= 0 && patchNo < patchItems.Count)
        {
            patchItems[patchNo] = CreatePatchListItem(patchNo);
            RefreshListBox();
            SelectPatchInListBox(patchNo);
        }
    }

    /// <summary>
    /// Called when this panel is activated (tab selected).
    /// </summary>
    public void OnActivated()
    {
        thisFormIsActive = true;
        if (memoryState is null) return;
        memoryState.patchEditorActive = true;
        memoryState.rhythmEditorActive = false;
        int midiChannel = memoryState.GetSystem().GetSysExMidiChannel(0);
        Midi.SendProgramChange(memoryState.GetSelectedPatchNo(), midiChannel);
    }

    /// <summary>
    /// Called when this panel is deactivated (another tab selected).
    /// </summary>
    public void OnDeactivated()
    {
        thisFormIsActive = false;
    }

    /// <summary>
    /// Simple data model for patch list items.
    /// </summary>
    private class PatchListItem
    {
        public string PatchNo { get; set; } = "";
        public string TimbreGroup { get; set; } = "";
        public string TimbreName { get; set; } = "";
        public string KeyShift { get; set; } = "";
        public string FineTune { get; set; } = "";
        public string BendRange { get; set; } = "";
        public string Reverb { get; set; } = "";
        public string AssignMode { get; set; } = "";
    }
}

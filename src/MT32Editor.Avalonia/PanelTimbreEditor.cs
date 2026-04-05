using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace MT32Edit.Avalonia;

/// <summary>
/// Timbre editor panel - Avalonia equivalent of WinForms FormTimbreEditor.
/// Builds all UI programmatically (no XAML).
/// </summary>
public class PanelTimbreEditor : UserControl
{
    private TimbreStructure timbre = new TimbreStructure(createAudibleTimbre: false);
    private TimbreHistory timbreHistory;
    private int activePartial = 0;
    private bool blockParameterUpdates = false;
    private readonly DispatcherTimer timer;

    // Controls
    private readonly TextBox textBoxTimbreName;
    private readonly RadioButton[] radioPartial = new RadioButton[4];
    private readonly CheckBox[] checkBoxMute = new CheckBox[4];
    private readonly CheckBox checkBoxSustain;
    private readonly CheckBox checkBoxPitchBend;
    private readonly CheckBox checkBoxShowLabels;
    private readonly CheckBox checkBoxShowAllPartials;
    private readonly ComboBox comboBoxPart12Structure;
    private readonly ComboBox comboBoxPart34Structure;
    private readonly ComboBox comboBoxWaveform;
    private readonly ComboBox comboBoxPCMSample;
    private readonly RadioButton radioButtonPCMBankA;
    private readonly RadioButton radioButtonPCMBankB;
    private readonly Button buttonUndo;
    private readonly Button buttonRedo;
    private readonly Button buttonCopyPartial;
    private readonly Button buttonPastePartial;
    private readonly Button buttonQuickSave;
    private byte[]? partialClipboard;
    private string? lastSavedTimbreFilePath;
    private bool allowQuickSave = false;

    // TVF disabled overlay (shown when PCM waveform is selected)
    private readonly TextBlock labelTVFDisabled;

    // Parameter sliders: indexed by parameter number (0x00-0x39)
    private readonly Slider?[] sliders = new Slider?[0x3A];
    private readonly TextBlock?[] sliderLabels = new TextBlock?[0x3A];

    // Envelope graphs
    private readonly AvaloniaEnvelopeGraph pitchGraph;
    private readonly AvaloniaEnvelopeGraph tvfGraph;
    private readonly AvaloniaEnvelopeGraph tvaGraph;

    // Labels for headers
    private readonly TextBlock labelHeading;

    public TimbreStructure TimbreData
    {
        get => timbre;
        set
        {
            timbre = value;
            if (!blockParameterUpdates) SetAllControlValues();
        }
    }

    public static TimbreStructure returnTimbre { get; set; } = new TimbreStructure(createAudibleTimbre: false);

    public PanelTimbreEditor()
    {
        timbreHistory = new TimbreHistory(new TimbreStructure(createAudibleTimbre: false));

        // Root layout
        var rootPanel = new DockPanel();

        // Top bar: timbre name + buttons
        var topBar = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5, Margin = new Thickness(5) };

        topBar.Children.Add(new TextBlock { Text = "Timbre:", VerticalAlignment = VerticalAlignment.Center, Foreground = AvaloniaUITools.GetForegroundBrush() });
        textBoxTimbreName = new TextBox { Width = 120 };
        textBoxTimbreName.TextChanged += (_, _) =>
        {
            if (!blockParameterUpdates && textBoxTimbreName.Text is not null)
            {
                timbre.SetTimbreName(textBoxTimbreName.Text);
                MT32SysEx.SendText("Editing " + timbre.GetTimbreName());
            }
        };
        topBar.Children.Add(textBoxTimbreName);

        var buttonNew = new Button { Content = "New" };
        buttonNew.Click += (_, _) => { InitialiseTimbreParameters(false); };
        topBar.Children.Add(buttonNew);

        var buttonLoad = new Button { Content = "Load" };
        buttonLoad.Click += (_, _) =>
        {
            string result = TimbreFile.Load(timbre);
            if (FileTools.Success(result))
            {
                lastSavedTimbreFilePath = result;
                allowQuickSave = true;
                if (buttonQuickSave is not null) buttonQuickSave.IsEnabled = true;
                SetAllControlValues();
            }
        };
        topBar.Children.Add(buttonLoad);

        var buttonSave = new Button { Content = "Save As" };
        buttonSave.Click += (_, _) => SaveTimbreAs();
        topBar.Children.Add(buttonSave);

        buttonQuickSave = new Button { Content = "Quick Save", IsEnabled = false };
        buttonQuickSave.Click += (_, _) => QuickSaveTimbre();
        topBar.Children.Add(buttonQuickSave);

        buttonUndo = new Button { Content = "Undo", IsEnabled = false };
        buttonUndo.Click += (_, _) =>
        {
            if (timbreHistory.GetLatestActionNo() > 0) { timbre = timbreHistory.Undo(); SetAllControlValues(); }
        };
        topBar.Children.Add(buttonUndo);

        buttonRedo = new Button { Content = "Redo", IsEnabled = false };
        buttonRedo.Click += (_, _) =>
        {
            if (timbreHistory.GetLatestActionNo() < timbreHistory.GetTopOfStack()) { timbre = timbreHistory.Redo(); SetAllControlValues(); }
        };
        topBar.Children.Add(buttonRedo);

        // Copy/Paste partial buttons
        buttonCopyPartial = new Button { Content = "Copy Partial" };
        buttonCopyPartial.Click += (_, _) =>
        {
            partialClipboard = timbre.CopyPartial(activePartial);
            if (buttonPastePartial is not null) buttonPastePartial.IsEnabled = true;
        };
        topBar.Children.Add(buttonCopyPartial);

        buttonPastePartial = new Button { Content = "Paste Partial", IsEnabled = false };
        buttonPastePartial.Click += (_, _) =>
        {
            if (partialClipboard is not null)
            {
                timbre.PastePartial(activePartial, partialClipboard);
                MT32SysEx.ApplyPartialParameters(timbre, activePartial);
                UpdatePartialControls();
                UpdateAllGraphs();
            }
        };
        topBar.Children.Add(buttonPastePartial);

        // Refresh button - resends all parameters to MIDI device
        var buttonRefresh = new Button { Content = "↻ Refresh" };
        buttonRefresh.Click += (_, _) =>
        {
            for (int p = 0; p < TimbreConstants.NO_OF_PARTIALS; p++)
            {
                MT32SysEx.ApplyPartialParameters(timbre, p);
            }
            MT32SysEx.SendTimbreName(timbre.GetTimbreName());
        };
        topBar.Children.Add(buttonRefresh);

        checkBoxSustain = new CheckBox { Content = "Sustain" };
        checkBoxSustain.IsCheckedChanged += (_, _) =>
        {
            if (!blockParameterUpdates) timbre.SetSustainStatus(checkBoxSustain.IsChecked == true);
        };
        topBar.Children.Add(checkBoxSustain);

        checkBoxPitchBend = new CheckBox { Content = "Pitch Bend" };
        checkBoxPitchBend.IsCheckedChanged += (_, _) =>
        {
            if (!blockParameterUpdates)
            {
                int pitchBendState = LogicTools.BoolToInt(checkBoxPitchBend.IsChecked == true);
                timbre.SetUIParameter(activePartial, 0x03, pitchBendState);
                MT32SysEx.SendPartialParameter(activePartial, 0x03, pitchBendState);
            }
        };
        topBar.Children.Add(checkBoxPitchBend);

        checkBoxShowLabels = new CheckBox { Content = "Labels" };
        checkBoxShowLabels.IsCheckedChanged += (_, _) => UpdateAllGraphs();
        topBar.Children.Add(checkBoxShowLabels);

        checkBoxShowAllPartials = new CheckBox { Content = "All Partials" };
        checkBoxShowAllPartials.IsCheckedChanged += (_, _) => UpdateAllGraphs();
        topBar.Children.Add(checkBoxShowAllPartials);

        labelHeading = new TextBlock { Text = "Timbre Editor", FontWeight = FontWeight.Bold, FontSize = 14, VerticalAlignment = VerticalAlignment.Center, Foreground = AvaloniaUITools.GetTitleBrush() };
        topBar.Children.Add(labelHeading);

        DockPanel.SetDock(topBar, Dock.Top);
        rootPanel.Children.Add(topBar);

        // Partial selection bar
        var partialBar = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5, Margin = new Thickness(5, 0) };
        partialBar.Children.Add(new TextBlock { Text = "Active Partial:", VerticalAlignment = VerticalAlignment.Center, Foreground = AvaloniaUITools.GetForegroundBrush() });

        for (int i = 0; i < 4; i++)
        {
            int partialIndex = i;
            radioPartial[i] = new RadioButton { Content = $"P{i + 1}", GroupName = "Partial", IsChecked = (i == 0) };
            radioPartial[i].IsCheckedChanged += (_, _) =>
            {
                if (radioPartial[partialIndex].IsChecked == true && !blockParameterUpdates)
                {
                    activePartial = partialIndex;
                    timbre.SetActivePartial(partialIndex);
                    UpdatePartialControls();
                }
            };
            partialBar.Children.Add(radioPartial[i]);

            checkBoxMute[i] = new CheckBox { Content = $"Mute {i + 1}" };
            int mi = i;
            checkBoxMute[i].IsCheckedChanged += (_, _) =>
            {
                if (!blockParameterUpdates)
                {
                    timbre.SetPartialMuteStatus(mi, checkBoxMute[mi].IsChecked == true);
                    UpdateAllGraphs();
                }
            };
            partialBar.Children.Add(checkBoxMute[i]);
        }

        // Structure combo boxes
        partialBar.Children.Add(new TextBlock { Text = "  Struct 1-2:", VerticalAlignment = VerticalAlignment.Center, Foreground = AvaloniaUITools.GetForegroundBrush() });
        comboBoxPart12Structure = new ComboBox { Width = 120 };
        for (int i = 0; i < 13; i++) comboBoxPart12Structure.Items.Add($"{i + 1}: {MT32Strings.partialConfig[i]}");
        comboBoxPart12Structure.SelectedIndex = 0;
        comboBoxPart12Structure.SelectionChanged += (_, _) =>
        {
            if (!blockParameterUpdates && comboBoxPart12Structure.SelectedIndex >= 0)
            {
                timbre.SetPart12Structure(comboBoxPart12Structure.SelectedIndex);
                MT32SysEx.SendTimbreName(timbre.GetTimbreName());
            }
        };
        partialBar.Children.Add(comboBoxPart12Structure);

        partialBar.Children.Add(new TextBlock { Text = "  Struct 3-4:", VerticalAlignment = VerticalAlignment.Center, Foreground = AvaloniaUITools.GetForegroundBrush() });
        comboBoxPart34Structure = new ComboBox { Width = 120 };
        for (int i = 0; i < 13; i++) comboBoxPart34Structure.Items.Add($"{i + 1}: {MT32Strings.partialConfig[i]}");
        comboBoxPart34Structure.SelectedIndex = 0;
        comboBoxPart34Structure.SelectionChanged += (_, _) =>
        {
            if (!blockParameterUpdates && comboBoxPart34Structure.SelectedIndex >= 0)
            {
                timbre.SetPart34Structure(comboBoxPart34Structure.SelectedIndex);
                MT32SysEx.SendTimbreName(timbre.GetTimbreName());
            }
        };
        partialBar.Children.Add(comboBoxPart34Structure);

        DockPanel.SetDock(partialBar, Dock.Top);
        rootPanel.Children.Add(partialBar);

        // Waveform controls bar
        var waveBar = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5, Margin = new Thickness(5, 2) };
        waveBar.Children.Add(new TextBlock { Text = "Waveform:", VerticalAlignment = VerticalAlignment.Center, Foreground = AvaloniaUITools.GetForegroundBrush() });
        comboBoxWaveform = new ComboBox { Width = 100 };
        comboBoxWaveform.Items.Add("LA Synth");
        comboBoxWaveform.Items.Add("PCM");
        comboBoxWaveform.SelectedIndex = 0;
        comboBoxWaveform.SelectionChanged += (_, _) =>
        {
            if (!blockParameterUpdates && comboBoxWaveform.SelectedIndex >= 0)
            {
                timbre.SetUIParameter(activePartial, 0x04, comboBoxWaveform.SelectedIndex);
                MT32SysEx.SendPartialParameter(activePartial, 0x04, timbre.GetUIParameter(activePartial, 0x04));
                UpdatePCMLASynthControls();
            }
        };
        waveBar.Children.Add(comboBoxWaveform);

        waveBar.Children.Add(new TextBlock { Text = "  PCM Sample:", VerticalAlignment = VerticalAlignment.Center, Foreground = AvaloniaUITools.GetForegroundBrush() });
        comboBoxPCMSample = new ComboBox { Width = 160 };
        PopulatePCMSamples();
        comboBoxPCMSample.SelectionChanged += (_, _) =>
        {
            if (!blockParameterUpdates && comboBoxPCMSample.SelectedIndex >= 0)
            {
                timbre.SetUIParameter(activePartial, 0x05, comboBoxPCMSample.SelectedIndex);
                MT32SysEx.SendPartialParameter(activePartial, 0x05, timbre.GetUIParameter(activePartial, 0x05));
            }
        };
        waveBar.Children.Add(comboBoxPCMSample);

        radioButtonPCMBankA = new RadioButton { Content = "Bank A", GroupName = "PCMBank", IsChecked = true };
        radioButtonPCMBankB = new RadioButton { Content = "Bank B", GroupName = "PCMBank" };
        radioButtonPCMBankA.IsCheckedChanged += (_, _) => { if (!blockParameterUpdates) PopulatePCMSamples(); };
        waveBar.Children.Add(radioButtonPCMBankA);
        waveBar.Children.Add(radioButtonPCMBankB);

        // TVF Disabled label (shown when PCM waveform selected, matching WinForms labelTVFDisabled)
        labelTVFDisabled = new TextBlock
        {
            Text = "TVF disabled (PCM waveform)",
            Foreground = Brushes.Red,
            FontWeight = FontWeight.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0),
            IsVisible = false
        };
        waveBar.Children.Add(labelTVFDisabled);

        DockPanel.SetDock(waveBar, Dock.Top);
        rootPanel.Children.Add(waveBar);

        // Envelope graphs at the bottom
        pitchGraph = new AvaloniaEnvelopeGraph(5, 5, 200, 80);
        tvfGraph = new AvaloniaEnvelopeGraph(5, 5, 200, 80);
        tvaGraph = new AvaloniaEnvelopeGraph(5, 5, 200, 80);

        var graphPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, Margin = new Thickness(5), Height = 100 };
        var pitchBorder = new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1), Child = pitchGraph };
        var tvfBorder = new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1), Child = tvfGraph };
        var tvaBorder = new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1), Child = tvaGraph };
        graphPanel.Children.Add(new StackPanel { Children = { new TextBlock { Text = "Pitch Envelope", Foreground = AvaloniaUITools.GetForegroundBrush(), FontSize = 10 }, pitchBorder } });
        graphPanel.Children.Add(new StackPanel { Children = { new TextBlock { Text = "TVF Envelope", Foreground = AvaloniaUITools.GetForegroundBrush(), FontSize = 10 }, tvfBorder } });
        graphPanel.Children.Add(new StackPanel { Children = { new TextBlock { Text = "TVA Envelope", Foreground = AvaloniaUITools.GetForegroundBrush(), FontSize = 10 }, tvaBorder } });

        DockPanel.SetDock(graphPanel, Dock.Bottom);
        rootPanel.Children.Add(graphPanel);

        // Main parameter area — scrollable 4-column grid matching WinForms layout
        var scrollViewer = new ScrollViewer { HorizontalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled };
        var paramGrid = new Grid { Margin = new Thickness(5) };
        paramGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        paramGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        paramGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        paramGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        // Column 0: Pitch + LFO (matching WinForms groupBoxPitch + groupBoxLFO)
        // All 58 partial parameters mapped exactly as per WinForms FormTimbreEditor.cs
        var pitchCol = CreateSliderColumn("Pitch", new SolidColorBrush(Color.FromRgb(221, 160, 221)), new[] {
            (0x00, "Coarse Pitch", 0, 96),
            (0x01, "Fine Pitch", 0, 100),
            (0x02, "Pitch Key Follow", 0, 16),
            (0x06, "Pulse Width", 0, 100),
            (0x07, "PW Velo Sens", 0, 14),
            (0x08, "Pitch Env Depth", 0, 10),
            (0x14, "LFO Rate", 0, 100),
            (0x15, "LFO Depth", 0, 100),
            (0x16, "LFO Mod Sens", 0, 100),
        });
        Grid.SetColumn(pitchCol, 0);
        paramGrid.Children.Add(pitchCol);

        // Column 1: Pitch Envelope (matching WinForms groupBoxPitchEnvelope)
        var pitchEnvCol = CreateSliderColumn("Pitch Envelope", new SolidColorBrush(Color.FromRgb(221, 160, 221)), new[] {
            (0x09, "Velo Sensitivity", 0, 100),
            (0x0A, "Time Key Follow", 0, 4),
            (0x0B, "T1", 0, 100), (0x0C, "T2", 0, 100), (0x0D, "T3", 0, 100), (0x0E, "T4", 0, 100),
            (0x0F, "L0", 0, 100), (0x10, "L1", 0, 100), (0x11, "L2", 0, 100),
            (0x12, "Sustain Level", 0, 100), (0x13, "Release Level", 0, 100),
        });
        Grid.SetColumn(pitchEnvCol, 1);
        paramGrid.Children.Add(pitchEnvCol);

        // Column 2: TVF (Filter) (matching WinForms groupBoxTVF + TVF envelope)
        var tvfCol = CreateSliderColumn("TVF (Filter)", new SolidColorBrush(Color.FromRgb(240, 230, 140)), new[] {
            (0x17, "Cutoff Freq", 0, 100), (0x18, "Resonance", 0, 30), (0x19, "Key Follow", 0, 16),
            (0x1A, "Bias Point", 0, 127), (0x1B, "Bias Level", 0, 14),
            (0x1C, "Env Depth", 0, 100), (0x1D, "Velo Sensitivity", 0, 100),
            (0x1E, "Depth Key Follow", 0, 4), (0x1F, "Time Key Follow", 0, 4),
            (0x20, "T1", 0, 100), (0x21, "T2", 0, 100), (0x22, "T3", 0, 100), (0x23, "T4", 0, 100), (0x24, "T5", 0, 100),
            (0x25, "L1", 0, 100), (0x26, "L2", 0, 100), (0x27, "L3", 0, 100), (0x28, "Sustain", 0, 100),
        });
        Grid.SetColumn(tvfCol, 2);
        paramGrid.Children.Add(tvfCol);

        // Column 3: TVA (Amplifier) (matching WinForms groupBoxTVA + groupBoxTVABias + TVA envelope)
        var tvaCol = CreateSliderColumn("TVA (Amplifier)", new SolidColorBrush(Color.FromRgb(72, 209, 204)), new[] {
            (0x29, "Level", 0, 100), (0x2A, "Velo Sensitivity", 0, 100),
            (0x2B, "Bias Point 1", 0, 127), (0x2C, "Bias Level 1", 0, 12),
            (0x2D, "Bias Point 2", 0, 127), (0x2E, "Bias Level 2", 0, 12),
            (0x2F, "Time Key Follow", 0, 4), (0x30, "Velo Key Follow", 0, 4),
            (0x31, "T1", 0, 100), (0x32, "T2", 0, 100), (0x33, "T3", 0, 100), (0x34, "T4", 0, 100), (0x35, "T5", 0, 100),
            (0x36, "L1", 0, 100), (0x37, "L2", 0, 100), (0x38, "L3", 0, 100), (0x39, "Sustain", 0, 100),
        });
        Grid.SetColumn(tvaCol, 3);
        paramGrid.Children.Add(tvaCol);

        scrollViewer.Content = paramGrid;
        rootPanel.Children.Add(scrollViewer);

        Content = rootPanel;
        Background = AvaloniaUITools.GetBackgroundBrush();

        // Timer
        timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(UISettings.UI_REFRESH_INTERVAL) };
        timer.Tick += Timer_Tick;
        timer.Start();
    }

    private StackPanel CreateSliderColumn(string groupName, IBrush headerBrush, (int paramNo, string name, int min, int max)[] parameters)
    {
        var groupPanel = new StackPanel { Margin = new Thickness(3) };
        groupPanel.Children.Add(new TextBlock { Text = groupName, FontWeight = FontWeight.Bold, Foreground = headerBrush, Margin = new Thickness(0, 2), FontSize = 13 });

        foreach (var (paramNo, name, min, max) in parameters)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4, Margin = new Thickness(0, 1) };

            var label = new TextBlock { Text = name, Width = 90, FontSize = 11, VerticalAlignment = VerticalAlignment.Center, Foreground = AvaloniaUITools.GetForegroundBrush() };
            sliderLabels[paramNo] = label;
            row.Children.Add(label);

            var slider = new Slider { Minimum = min, Maximum = max, Width = 120, Height = 20 };
            int pNo = paramNo;
            slider.PropertyChanged += (s, e) =>
            {
                if (e.Property == Slider.ValueProperty && !blockParameterUpdates)
                {
                    timbre.SetUIParameter(activePartial, pNo, (int)slider.Value);
                    MT32SysEx.SendPartialParameter(activePartial, (byte)pNo, timbre.GetUIParameter(activePartial, pNo));
                    timbreHistory.AddTo(timbre);
                    UpdateAllGraphs();
                }
            };
            sliders[paramNo] = slider;
            row.Children.Add(slider);

            var valueLabel = new TextBlock { Width = 30, FontSize = 11, VerticalAlignment = VerticalAlignment.Center, Foreground = AvaloniaUITools.GetForegroundBrush() };
            slider.PropertyChanged += (s, e) =>
            {
                if (e.Property == Slider.ValueProperty) valueLabel.Text = ((int)slider.Value).ToString();
            };
            row.Children.Add(valueLabel);

            groupPanel.Children.Add(row);
        }

        return groupPanel;
    }

    private void PopulatePCMSamples()
    {
        comboBoxPCMSample.Items.Clear();
        int bankNo = radioButtonPCMBankB?.IsChecked is true ? 1 : 0;
        string[] sampleNames = MT32Strings.GetAllSampleNames(bankNo);
        foreach (string name in sampleNames)
            comboBoxPCMSample.Items.Add(name);
        if (comboBoxPCMSample.Items.Count > 0)
            comboBoxPCMSample.SelectedIndex = 0;
    }

    public void InitialiseTimbreParameters(bool editExisting)
    {
        blockParameterUpdates = true;
        if (!editExisting)
        {
            timbre = new TimbreStructure(createAudibleTimbre: false);
            timbreHistory = new TimbreHistory(timbre);
        }
        SetAllControlValues();
        blockParameterUpdates = false;
    }

    public void SetAllControlValues()
    {
        blockParameterUpdates = true;

        textBoxTimbreName.Text = timbre.GetTimbreName();
        checkBoxSustain.IsChecked = timbre.GetSustainStatus();
        comboBoxPart12Structure.SelectedIndex = Math.Clamp(timbre.GetPart12Structure(), 0, 12);
        comboBoxPart34Structure.SelectedIndex = Math.Clamp(timbre.GetPart34Structure(), 0, 12);

        bool[] muteStatus = timbre.GetPartialMuteStatus();
        for (int i = 0; i < 4; i++)
            checkBoxMute[i].IsChecked = muteStatus[i];

        UpdatePartialControls();
        blockParameterUpdates = false;
    }

    private void UpdatePartialControls()
    {
        blockParameterUpdates = true;

        // Waveform is parameter 0x04 (LA Synth/PCM), PCM sample is 0x05
        int waveform = timbre.GetUIParameter(activePartial, 0x04);
        if (waveform >= 0 && waveform < comboBoxWaveform.Items.Count)
            comboBoxWaveform.SelectedIndex = Math.Clamp(waveform, 0, 1);

        PopulatePCMSamples();
        int pcmSample = timbre.GetUIParameter(activePartial, 0x05);
        if (pcmSample >= 0 && pcmSample < comboBoxPCMSample.Items.Count)
            comboBoxPCMSample.SelectedIndex = pcmSample;

        // Pitch Bend is parameter 0x03 (checkbox)
        checkBoxPitchBend.IsChecked = LogicTools.IntToBool(timbre.GetUIParameter(activePartial, 0x03));

        // Update all parameter sliders for the active partial
        for (int paramNo = 0; paramNo < sliders.Length; paramNo++)
        {
            if (sliders[paramNo] is not null)
            {
                int value = timbre.GetUIParameter(activePartial, paramNo);
                sliders[paramNo]!.Value = Math.Clamp(value, (int)sliders[paramNo]!.Minimum, (int)sliders[paramNo]!.Maximum);
            }
        }

        UpdateAllGraphs();
        UpdatePCMLASynthControls();
        blockParameterUpdates = false;
    }

    private void UpdateAllGraphs()
    {
        bool showLabels = checkBoxShowLabels.IsChecked == true;
        bool showAll = checkBoxShowAllPartials.IsChecked == true;
        pitchGraph.SetData(timbre, AvaloniaEnvelopeGraph.PITCH_GRAPH, activePartial, showAll, showLabels);
        tvfGraph.SetData(timbre, AvaloniaEnvelopeGraph.TVF_GRAPH, activePartial, showAll, showLabels);
        tvaGraph.SetData(timbre, AvaloniaEnvelopeGraph.TVA_GRAPH, activePartial, showAll, showLabels);
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        buttonUndo.IsEnabled = timbreHistory.GetLatestActionNo() > 0;
        buttonRedo.IsEnabled = timbreHistory.GetLatestActionNo() < timbreHistory.GetTopOfStack();
    }

    /// <summary>
    /// Toggles PCM vs LA Synth controls based on current waveform selection.
    /// Matches WinForms ShowOnlyPCMControls() and ShowOnlyLASynthControls().
    /// </summary>
    private void UpdatePCMLASynthControls()
    {
        bool isPCM = comboBoxWaveform.SelectedIndex == 1; // 0=LA Synth, 1=PCM
        if (isPCM)
            ShowOnlyPCMControls();
        else
            ShowOnlyLASynthControls();
    }

    /// <summary>
    /// Enables PCM-specific controls and disables LA synth/TVF controls.
    /// Matches WinForms FormTimbreEditor.ShowOnlyPCMControls().
    /// </summary>
    private void ShowOnlyPCMControls()
    {
        comboBoxWaveform.IsEnabled = false;
        comboBoxPCMSample.IsEnabled = true;
        radioButtonPCMBankA.IsEnabled = true;
        radioButtonPCMBankB.IsEnabled = true;
        labelTVFDisabled.IsVisible = true;

        // Disable all TVF sliders (parameters 0x17 through 0x28)
        for (int p = 0x17; p <= 0x28; p++)
        {
            if (sliders[p] is not null)
                sliders[p]!.IsEnabled = false;
        }

        UpdateAllGraphs();
    }

    /// <summary>
    /// Enables LA synth/TVF controls and disables PCM-specific controls.
    /// Matches WinForms FormTimbreEditor.ShowOnlyLASynthControls().
    /// </summary>
    private void ShowOnlyLASynthControls()
    {
        comboBoxWaveform.IsEnabled = true;
        comboBoxPCMSample.IsEnabled = false;
        radioButtonPCMBankA.IsEnabled = false;
        radioButtonPCMBankB.IsEnabled = false;
        labelTVFDisabled.IsVisible = false;

        // Enable all TVF sliders (parameters 0x17 through 0x28)
        for (int p = 0x17; p <= 0x28; p++)
        {
            if (sliders[p] is not null)
                sliders[p]!.IsEnabled = true;
        }

        UpdateAllGraphs();
    }

    /// <summary>
    /// Shows Save As dialog and saves timbre file.
    /// Matches WinForms FormTimbreEditor.SaveTimbreAs().
    /// </summary>
    private void SaveTimbreAs()
    {
        string? filePath = PlatformServices.FileDialog.ShowSaveFileDialog("Save Timbre File", "Timbre file|*.timbre", timbre.GetTimbreName());
        if (filePath is not null)
        {
            var fs = File.Create(filePath);
            TimbreFile.SaveTimbreParameters(timbre, fs);
            TimbreFile.SavePartials(timbre, fs);
            fs.Close();
            lastSavedTimbreFilePath = filePath;
            allowQuickSave = true;
            buttonQuickSave.IsEnabled = true;
        }
    }

    /// <summary>
    /// Saves timbre to last used file path without showing dialog.
    /// If no file has been previously saved, falls back to SaveTimbreAs().
    /// Matches WinForms FormTimbreEditor.QuickSaveTimbre().
    /// </summary>
    private void QuickSaveTimbre()
    {
        if (!allowQuickSave || string.IsNullOrEmpty(lastSavedTimbreFilePath))
        {
            SaveTimbreAs();
            return;
        }

        string action = File.Exists(lastSavedTimbreFilePath) ? "Overwrite" : "Save";
        if (PlatformServices.Notification.AskUserToConfirm($"{action} file {lastSavedTimbreFilePath}?", "MT-32 Editor"))
        {
            var fs = File.Create(lastSavedTimbreFilePath);
            TimbreFile.SaveTimbreParameters(timbre, fs);
            TimbreFile.SavePartials(timbre, fs);
            fs.Close();
            timbreHistory.Clear(timbre);
        }
    }
}

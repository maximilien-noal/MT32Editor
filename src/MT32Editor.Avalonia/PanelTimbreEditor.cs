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
    private bool initialised = false;
    private readonly DispatcherTimer timer;

    // Controls
    private readonly TextBox textBoxTimbreName;
    private readonly RadioButton[] radioPartial = new RadioButton[4];
    private readonly CheckBox[] checkBoxMute = new CheckBox[4];
    private readonly CheckBox checkBoxSustain;
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
            if (FileTools.Success(result)) SetAllControlValues();
        };
        topBar.Children.Add(buttonLoad);

        var buttonSave = new Button { Content = "Save" };
        buttonSave.Click += (_, _) =>
        {
            string? filePath = PlatformServices.FileDialog.ShowSaveFileDialog("Save Timbre File", "Timbre file|*.timbre", timbre.GetTimbreName());
            if (filePath is not null)
            {
                var fs = File.Create(filePath);
                TimbreFile.SaveTimbreParameters(timbre, fs);
                TimbreFile.SavePartials(timbre, fs);
                fs.Close();
            }
        };
        topBar.Children.Add(buttonSave);

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

        checkBoxSustain = new CheckBox { Content = "Sustain" };
        checkBoxSustain.IsCheckedChanged += (_, _) =>
        {
            if (!blockParameterUpdates) timbre.SetSustainStatus(checkBoxSustain.IsChecked == true);
        };
        topBar.Children.Add(checkBoxSustain);

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
                timbre.SetUIParameter(activePartial, 0x00, comboBoxWaveform.SelectedIndex);
                MT32SysEx.SendPartialParameter(activePartial, 0x00, timbre.GetUIParameter(activePartial, 0x00));
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
                timbre.SetUIParameter(activePartial, 0x06, comboBoxPCMSample.SelectedIndex);
                MT32SysEx.SendPartialParameter(activePartial, 0x06, timbre.GetUIParameter(activePartial, 0x06));
            }
        };
        waveBar.Children.Add(comboBoxPCMSample);

        radioButtonPCMBankA = new RadioButton { Content = "Bank A", GroupName = "PCMBank", IsChecked = true };
        radioButtonPCMBankB = new RadioButton { Content = "Bank B", GroupName = "PCMBank" };
        radioButtonPCMBankA.IsCheckedChanged += (_, _) => { if (!blockParameterUpdates) PopulatePCMSamples(); };
        waveBar.Children.Add(radioButtonPCMBankA);
        waveBar.Children.Add(radioButtonPCMBankB);

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

        // Main parameter area — scrollable grid of sliders
        var scrollViewer = new ScrollViewer { HorizontalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled };
        var paramPanel = new WrapPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };

        // Create parameter sliders organized by group
        CreateSliderGroup(paramPanel, "Pitch", new[] {
            (0x01, "Fine Pitch", 0, 100),
            (0x02, "Key Follow", 0, 16),
            (0x07, "Pitch Coarse", 0, 96),
            (0x08, "Pitch Fine", 0, 100),
            (0x09, "Pitch Bend", 0, 1),
        });

        CreateSliderGroup(paramPanel, "Pitch Envelope", new[] {
            (0x0B, "T1", 0, 100), (0x0C, "T2", 0, 100), (0x0D, "T3", 0, 100), (0x0E, "T4", 0, 100),
            (0x0F, "L0", 0, 100), (0x10, "L1", 0, 100), (0x11, "L2", 0, 100),
            (0x12, "Sustain Level", 0, 100), (0x13, "Release Level", 0, 100),
        });

        CreateSliderGroup(paramPanel, "TVF (Filter)", new[] {
            (0x14, "Cutoff Freq", 0, 100), (0x15, "Resonance", 0, 30), (0x16, "Key Follow", 0, 14),
            (0x17, "Bias Point", 0, 127), (0x18, "Bias Level", 0, 14),
            (0x19, "Env Depth", 0, 100), (0x1A, "Env V-Sens", 0, 100),
            (0x1B, "Dep. Key Fol", 0, 4), (0x1C, "Env T-Key Fol", 0, 4),
            (0x1D, "Env V-Time", 0, 4),
            (0x20, "T1", 0, 100), (0x21, "T2", 0, 100), (0x22, "T3", 0, 100), (0x23, "T4", 0, 100), (0x24, "T5", 0, 100),
            (0x25, "L1", 0, 100), (0x26, "L2", 0, 100), (0x27, "L3", 0, 100), (0x28, "Sustain", 0, 100),
        });

        CreateSliderGroup(paramPanel, "TVA (Amplifier)", new[] {
            (0x29, "Level", 0, 100), (0x2A, "V-Sensitivity", 0, 100),
            (0x2B, "Bias Point 1", 0, 127), (0x2C, "Bias Level 1", 0, 12),
            (0x2D, "Bias Point 2", 0, 127), (0x2E, "Bias Level 2", 0, 12),
            (0x2F, "Env T-Key Fol", 0, 4), (0x30, "Env V-Time", 0, 4),
            (0x31, "T1", 0, 100), (0x32, "T2", 0, 100), (0x33, "T3", 0, 100), (0x34, "T4", 0, 100), (0x35, "T5", 0, 100),
            (0x36, "L1", 0, 100), (0x37, "L2", 0, 100), (0x38, "L3", 0, 100), (0x39, "Sustain", 0, 100),
        });

        scrollViewer.Content = paramPanel;
        rootPanel.Children.Add(scrollViewer);

        Content = rootPanel;
        Background = AvaloniaUITools.GetBackgroundBrush();

        // Timer
        timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(UISettings.UI_REFRESH_INTERVAL) };
        timer.Tick += Timer_Tick;
        timer.Start();

        initialised = true;
    }

    private void CreateSliderGroup(WrapPanel parent, string groupName, (int paramNo, string name, int min, int max)[] parameters)
    {
        var groupPanel = new StackPanel { Margin = new Thickness(3), MinWidth = 160 };
        groupPanel.Children.Add(new TextBlock { Text = groupName, FontWeight = FontWeight.Bold, Foreground = AvaloniaUITools.GetTitleBrush(), Margin = new Thickness(0, 2) });

        foreach (var (paramNo, name, min, max) in parameters)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5, Margin = new Thickness(0, 1) };

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

        parent.Children.Add(groupPanel);
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

        int waveform = timbre.GetUIParameter(activePartial, 0x00);
        if (waveform >= 0 && waveform < comboBoxWaveform.Items.Count)
            comboBoxWaveform.SelectedIndex = waveform;

        int pcmSample = timbre.GetUIParameter(activePartial, 0x06);
        if (pcmSample >= 0 && pcmSample < comboBoxPCMSample.Items.Count)
            comboBoxPCMSample.SelectedIndex = pcmSample;

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
}

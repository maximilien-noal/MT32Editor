using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace MT32Edit.Avalonia;

/// <summary>
/// System settings dialog - Avalonia equivalent of WinForms FormSystemSettings.
/// Allows configuration of master volume, master tuning, reverb, MIDI channels,
/// partial reserve settings, and custom text messages.
/// </summary>
public class WindowSystemSettings : Window
{
    private readonly SystemLevel system;

    // Master controls
    private readonly Slider sliderMasterLevel;
    private readonly TextBlock labelMasterLevelValue;
    private readonly Slider sliderMasterTune;
    private readonly TextBlock labelMasterTuneValue;

    // Reverb controls
    private readonly ComboBox comboBoxReverbType;
    private readonly Slider sliderReverbLevel;
    private readonly TextBlock labelReverbLevelValue;
    private readonly Slider sliderReverbRate;
    private readonly TextBlock labelReverbRateValue;

    // MIDI channel NumericUpDowns
    private readonly NumericUpDown[] numericUpDownMIDIParts = new NumericUpDown[9];

    // Partial reserve NumericUpDowns
    private readonly NumericUpDown[] numericUpDownPartReserves = new NumericUpDown[9];

    // Radio buttons for MIDI channel presets
    private readonly RadioButton radioButtonChannels2to9;
    private readonly RadioButton radioButtonChannels1to8;
    private readonly RadioButton radioButtonChannelCustom;

    // Text messages
    private readonly TextBox textBoxMessage1;
    private readonly TextBox textBoxMessage2;

    // Export checkboxes
    private readonly CheckBox checkBoxMasterLevel;
    private readonly CheckBox checkBoxMasterTune;
    private readonly CheckBox checkBoxReverb;
    private readonly CheckBox checkBoxMIDIChannel;
    private readonly CheckBox checkBoxPartialReserve;
    private readonly CheckBox checkBoxTextMessages;

    public WindowSystemSettings(SystemLevel systemInput)
    {
        system = systemInput;

        Title = "MT-32 System Settings";
        Width = 580;
        Height = 620;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = AvaloniaUITools.GetBackgroundBrush();

        var foreground = AvaloniaUITools.GetForegroundBrush();

        var mainPanel = new StackPanel
        {
            Margin = new Thickness(14, 14, 14, 10),
            Spacing = 8
        };

        // === Master Level & Master Tune row ===
        var masterRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 30 };

        // Master Level
        var masterLevelPanel = new StackPanel { Spacing = 4, Width = 120, HorizontalAlignment = HorizontalAlignment.Center };
        masterLevelPanel.Children.Add(new TextBlock { Text = "Master Level", Foreground = foreground, HorizontalAlignment = HorizontalAlignment.Center });
        sliderMasterLevel = new Slider { Minimum = 0, Maximum = 100, Value = 80, Orientation = Orientation.Vertical, Height = 104, HorizontalAlignment = HorizontalAlignment.Center };
        sliderMasterLevel.PropertyChanged += SliderMasterLevel_ValueChanged;
        masterLevelPanel.Children.Add(sliderMasterLevel);
        labelMasterLevelValue = new TextBlock { Text = "80", Foreground = foreground, HorizontalAlignment = HorizontalAlignment.Center };
        masterLevelPanel.Children.Add(labelMasterLevelValue);
        masterRow.Children.Add(masterLevelPanel);

        // Master Tune
        var masterTunePanel = new StackPanel { Spacing = 4, Width = 120, HorizontalAlignment = HorizontalAlignment.Center };
        masterTunePanel.Children.Add(new TextBlock { Text = "Master Tune", Foreground = foreground, HorizontalAlignment = HorizontalAlignment.Center });
        sliderMasterTune = new Slider { Minimum = 0, Maximum = 127, Value = 63, Orientation = Orientation.Vertical, Height = 104, HorizontalAlignment = HorizontalAlignment.Center };
        sliderMasterTune.PropertyChanged += SliderMasterTune_ValueChanged;
        masterTunePanel.Children.Add(sliderMasterTune);
        labelMasterTuneValue = new TextBlock { Text = "440.0Hz", Foreground = foreground, HorizontalAlignment = HorizontalAlignment.Center };
        masterTunePanel.Children.Add(labelMasterTuneValue);
        masterRow.Children.Add(masterTunePanel);

        mainPanel.Children.Add(masterRow);

        // === Reverb Settings GroupBox ===
        var reverbBorder = new Border
        {
            BorderBrush = foreground,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8),
            Margin = new Thickness(0, 4, 0, 0)
        };
        var reverbPanel = new StackPanel { Spacing = 6 };
        reverbPanel.Children.Add(new TextBlock { Text = "Reverb settings", Foreground = foreground, FontWeight = FontWeight.SemiBold });

        // Reverb Type row
        var reverbTypeRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        reverbTypeRow.Children.Add(new TextBlock { Text = "Reverb Type", Foreground = foreground, VerticalAlignment = VerticalAlignment.Center });
        comboBoxReverbType = new ComboBox { Width = 100 };
        comboBoxReverbType.ItemsSource = new[] { "Room", "Hall", "Plate", "Delay" };
        comboBoxReverbType.SelectionChanged += ComboBoxReverbType_SelectionChanged;
        reverbTypeRow.Children.Add(comboBoxReverbType);
        reverbPanel.Children.Add(reverbTypeRow);

        // Reverb Level & Rate sliders
        var reverbSlidersRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 30 };

        var reverbLevelPanel = new StackPanel { Spacing = 4, HorizontalAlignment = HorizontalAlignment.Center };
        reverbLevelPanel.Children.Add(new TextBlock { Text = "Level", Foreground = foreground, HorizontalAlignment = HorizontalAlignment.Center });
        sliderReverbLevel = new Slider { Minimum = 0, Maximum = 7, Value = 4, Orientation = Orientation.Vertical, Height = 80, HorizontalAlignment = HorizontalAlignment.Center };
        sliderReverbLevel.PropertyChanged += SliderReverbLevel_ValueChanged;
        reverbLevelPanel.Children.Add(sliderReverbLevel);
        labelReverbLevelValue = new TextBlock { Text = "0", Foreground = foreground, HorizontalAlignment = HorizontalAlignment.Center };
        reverbLevelPanel.Children.Add(labelReverbLevelValue);
        reverbSlidersRow.Children.Add(reverbLevelPanel);

        var reverbRatePanel = new StackPanel { Spacing = 4, HorizontalAlignment = HorizontalAlignment.Center };
        reverbRatePanel.Children.Add(new TextBlock { Text = "Rate", Foreground = foreground, HorizontalAlignment = HorizontalAlignment.Center });
        sliderReverbRate = new Slider { Minimum = 0, Maximum = 7, Value = 4, Orientation = Orientation.Vertical, Height = 80, HorizontalAlignment = HorizontalAlignment.Center };
        sliderReverbRate.PropertyChanged += SliderReverbRate_ValueChanged;
        reverbRatePanel.Children.Add(sliderReverbRate);
        labelReverbRateValue = new TextBlock { Text = "0", Foreground = foreground, HorizontalAlignment = HorizontalAlignment.Center };
        reverbRatePanel.Children.Add(labelReverbRateValue);
        reverbSlidersRow.Children.Add(reverbRatePanel);

        reverbPanel.Children.Add(reverbSlidersRow);
        reverbBorder.Child = reverbPanel;
        mainPanel.Children.Add(reverbBorder);

        // === MIDI Channels & Partial Reserve section ===
        var midiGrid = new Grid { Margin = new Thickness(0, 4, 0, 0) };
        midiGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        midiGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(80)));
        midiGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(80)));

        // Headers
        var headerMidiChannel = new TextBlock { Text = "MIDI channel", Foreground = foreground, FontWeight = FontWeight.SemiBold };
        Grid.SetColumn(headerMidiChannel, 1);
        midiGrid.Children.Add(headerMidiChannel);

        var headerPartialReserve = new TextBlock { Text = "Partial reserve", Foreground = foreground, FontWeight = FontWeight.SemiBold };
        Grid.SetColumn(headerPartialReserve, 2);
        midiGrid.Children.Add(headerPartialReserve);

        var midiOffLabel = new TextBlock { Text = "(0 = off)", Foreground = foreground, FontSize = 11 };
        Grid.SetColumn(midiOffLabel, 1);
        Grid.SetRow(midiOffLabel, 1);
        midiGrid.Children.Add(midiOffLabel);

        string[] partLabels = { "Part 1", "Part 2", "Part 3", "Part 4", "Part 5", "Part 6", "Part 7", "Part 8", "Rhythm" };
        int[] defaultMidiValues = { 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        for (int i = 0; i < 9; i++)
        {
            midiGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }
        // Add extra rows for headers and (0=off) label
        midiGrid.RowDefinitions.Insert(0, new RowDefinition(GridLength.Auto));
        midiGrid.RowDefinitions.Insert(1, new RowDefinition(GridLength.Auto));

        for (int i = 0; i < 9; i++)
        {
            int row = i + 2;

            var label = new TextBlock { Text = partLabels[i], Foreground = foreground, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 2) };
            Grid.SetColumn(label, 0);
            Grid.SetRow(label, row);
            midiGrid.Children.Add(label);

            var nud = new NumericUpDown { Minimum = 0, Maximum = 16, Value = defaultMidiValues[i], Width = 65, Margin = new Thickness(4, 2) };
            int partIndex = i;
            nud.ValueChanged += (s, e) => NumericUpDownMIDIPart_ValueChanged(partIndex);
            numericUpDownMIDIParts[i] = nud;
            Grid.SetColumn(nud, 1);
            Grid.SetRow(nud, row);
            midiGrid.Children.Add(nud);

            var reserve = new NumericUpDown { Minimum = 0, Maximum = 32, Value = 0, Width = 65, Margin = new Thickness(4, 2) };
            int reserveIndex = i;
            reserve.ValueChanged += (s, e) => NumericUpDownPartReserve_ValueChanged(reserveIndex);
            numericUpDownPartReserves[i] = reserve;
            Grid.SetColumn(reserve, 2);
            Grid.SetRow(reserve, row);
            midiGrid.Children.Add(reserve);
        }
        mainPanel.Children.Add(midiGrid);

        // === Channel Presets radio buttons ===
        var radioPanel = new StackPanel { Margin = new Thickness(14, 4, 0, 0), Spacing = 4 };
        radioButtonChannels2to9 = new RadioButton { Content = "2-9, 10 (default)", Foreground = foreground, GroupName = "MidiPreset", IsChecked = true };
        radioButtonChannels2to9.IsCheckedChanged += RadioButtonChannels2to9_CheckedChanged;
        radioPanel.Children.Add(radioButtonChannels2to9);
        radioButtonChannels1to8 = new RadioButton { Content = "1-8, 10 (improved General MIDI compatibility)", Foreground = foreground, GroupName = "MidiPreset" };
        radioButtonChannels1to8.IsCheckedChanged += RadioButtonChannels1to8_CheckedChanged;
        radioPanel.Children.Add(radioButtonChannels1to8);
        radioButtonChannelCustom = new RadioButton { Content = "Custom mapping", Foreground = foreground, GroupName = "MidiPreset" };
        radioPanel.Children.Add(radioButtonChannelCustom);
        mainPanel.Children.Add(radioPanel);

        // === Text Messages GroupBox ===
        var messageBorder = new Border
        {
            BorderBrush = foreground,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8),
            Margin = new Thickness(0, 4, 0, 0)
        };
        var messagePanel = new StackPanel { Spacing = 6 };
        messagePanel.Children.Add(new TextBlock { Text = "Text messages", Foreground = foreground, FontWeight = FontWeight.SemiBold });

        var messageRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };

        messageRow.Children.Add(new TextBlock { Text = "Message 1", Foreground = foreground, VerticalAlignment = VerticalAlignment.Center });
        textBoxMessage1 = new TextBox { Width = 175, MaxLength = 20 };
        ToolTip.SetTip(textBoxMessage1, "Enter a custom message to be shown on MT-32 display when loading SysEx file");
        textBoxMessage1.TextChanged += TextBoxMessage1_TextChanged;
        textBoxMessage1.GotFocus += (_, _) => MT32SysEx.SendText(textBoxMessage1.Text ?? string.Empty);
        messageRow.Children.Add(textBoxMessage1);

        messageRow.Children.Add(new TextBlock { Text = "Message 2", Foreground = foreground, VerticalAlignment = VerticalAlignment.Center });
        textBoxMessage2 = new TextBox { Width = 180, MaxLength = 20 };
        ToolTip.SetTip(textBoxMessage2, "Enter a custom message to be shown on MT-32 display after loading SysEx file");
        textBoxMessage2.TextChanged += TextBoxMessage2_TextChanged;
        textBoxMessage2.GotFocus += (_, _) => MT32SysEx.SendText(textBoxMessage2.Text ?? string.Empty);
        messageRow.Children.Add(textBoxMessage2);

        messagePanel.Children.Add(messageRow);
        messageBorder.Child = messagePanel;
        mainPanel.Children.Add(messageBorder);

        // === Export System Settings GroupBox ===
        var exportBorder = new Border
        {
            BorderBrush = foreground,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8),
            Margin = new Thickness(0, 4, 0, 0)
        };
        var exportPanel = new StackPanel { Spacing = 4 };
        exportPanel.Children.Add(new TextBlock { Text = "Save system settings", Foreground = foreground, FontWeight = FontWeight.SemiBold });
        exportPanel.Children.Add(new TextBlock { Text = "Include parameters:", Foreground = foreground });

        var checkBoxRow1 = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16 };
        checkBoxMasterLevel = new CheckBox { Content = "Master level", IsChecked = true, Foreground = foreground };
        checkBoxMasterTune = new CheckBox { Content = "Master tune", IsChecked = true, Foreground = foreground };
        checkBoxMIDIChannel = new CheckBox { Content = "MIDI channel assignments", IsChecked = true, Foreground = foreground };
        checkBoxRow1.Children.Add(checkBoxMasterLevel);
        checkBoxRow1.Children.Add(checkBoxMasterTune);
        checkBoxRow1.Children.Add(checkBoxMIDIChannel);
        exportPanel.Children.Add(checkBoxRow1);

        var checkBoxRow2 = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16 };
        checkBoxReverb = new CheckBox { Content = "Reverb settings", IsChecked = true, Foreground = foreground };
        checkBoxPartialReserve = new CheckBox { Content = "Partial reserve settings", IsChecked = true, Foreground = foreground };
        checkBoxTextMessages = new CheckBox { Content = "Custom text messages", IsChecked = true, Foreground = foreground };
        checkBoxRow2.Children.Add(checkBoxReverb);
        checkBoxRow2.Children.Add(checkBoxPartialReserve);
        checkBoxRow2.Children.Add(checkBoxTextMessages);
        exportPanel.Children.Add(checkBoxRow2);

        var buttonSave = new Button
        {
            Content = "Save",
            Width = 54,
            Height = 23,
            Background = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
            Foreground = Brushes.Black
        };
        ToolTip.SetTip(buttonSave, "Create a SysEx file containing only these System settings");
        buttonSave.Click += ButtonSave_Click;
        exportPanel.Children.Add(buttonSave);

        exportBorder.Child = exportPanel;
        mainPanel.Children.Add(exportBorder);

        Content = new ScrollViewer { Content = mainPanel };

        SetSystemControls();
    }

    private void SetSystemControls()
    {
        MT32SysEx.blockSysExMessages = true;
        sliderMasterLevel.Value = system.GetMasterLevel();
        labelMasterLevelValue.Text = system.GetMasterLevel().ToString();
        sliderMasterTune.Value = system.GetMasterTune();
        labelMasterTuneValue.Text = system.GetMasterTuneFrequency();
        comboBoxReverbType.SelectedIndex = system.GetReverbMode();
        sliderReverbLevel.Value = system.GetReverbLevel();
        labelReverbLevelValue.Text = ((int)sliderReverbLevel.Value).ToString();
        sliderReverbRate.Value = system.GetReverbTime();
        labelReverbRateValue.Text = ((int)sliderReverbRate.Value).ToString();

        for (int i = 0; i < 9; i++)
        {
            numericUpDownMIDIParts[i].Value = system.GetUIMidiChannel(i);
            numericUpDownPartReserves[i].Value = system.GetPartialReserve(i);
        }

        textBoxMessage1.Text = system.GetMessage(0);
        textBoxMessage2.Text = system.GetMessage(1);
        MT32SysEx.blockSysExMessages = false;
        MT32SysEx.SendSystemParameters(system);
    }

    private void SliderMasterLevel_ValueChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != Slider.ValueProperty) return;
        int value = (int)sliderMasterLevel.Value;
        system.SetMasterLevel(value);
        labelMasterLevelValue.Text = value.ToString();
        MT32SysEx.SendSystemParameters(system);
        MT32SysEx.SendText($"Master Level: {value}");
    }

    private void SliderMasterTune_ValueChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != Slider.ValueProperty) return;
        int value = (int)sliderMasterTune.Value;
        system.SetMasterTune(value);
        labelMasterTuneValue.Text = system.GetMasterTuneFrequency();
        MT32SysEx.SendSystemParameters(system);
        MT32SysEx.SendText($"Master Tune: {labelMasterTuneValue.Text}");
    }

    private void ComboBoxReverbType_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (comboBoxReverbType.SelectedIndex < 0) return;
        system.SetReverbMode(comboBoxReverbType.SelectedIndex);
        MT32SysEx.SendSystemParameters(system);
        MT32SysEx.SendText($"Reverb Type: {comboBoxReverbType.SelectedItem}");
    }

    private void SliderReverbLevel_ValueChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != Slider.ValueProperty) return;
        int value = (int)sliderReverbLevel.Value;
        system.SetReverbLevel(value);
        labelReverbLevelValue.Text = value.ToString();
        MT32SysEx.SendSystemParameters(system);
        MT32SysEx.SendText($"Reverb Level: {value}");
    }

    private void SliderReverbRate_ValueChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != Slider.ValueProperty) return;
        int value = (int)sliderReverbRate.Value;
        system.SetReverbTime(value);
        labelReverbRateValue.Text = value.ToString();
        MT32SysEx.SendSystemParameters(system);
        MT32SysEx.SendText($"Reverb Rate: {value}");
    }

    private void NumericUpDownMIDIPart_ValueChanged(int partIndex)
    {
        int value = (int)(numericUpDownMIDIParts[partIndex].Value ?? 0);
        system.SetUIMidiChannel(partIndex, value);
        MT32SysEx.SendSystemParameters(system);
        string partName = partIndex < 8 ? $"Part {partIndex + 1}" : "Rhythm";
        MT32SysEx.SendText($"{partName} channel: {value}");
        SetRadioButtons();
    }

    private void NumericUpDownPartReserve_ValueChanged(int reserveIndex)
    {
        int value = (int)(numericUpDownPartReserves[reserveIndex].Value ?? 0);
        system.SetPartialReserve(reserveIndex, value);
        MT32SysEx.SendSystemParameters(system);
        string partName = reserveIndex < 8 ? $"Pt.{reserveIndex + 1}" : "Pt.R";
        MT32SysEx.SendText($"{partName} Reserve: {value}");
    }

    private void RadioButtonChannels2to9_CheckedChanged(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (radioButtonChannels2to9.IsChecked == true)
        {
            SetMidiChannels();
            MT32SysEx.SendText("Channels 2-10");
        }
    }

    private void RadioButtonChannels1to8_CheckedChanged(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (radioButtonChannels1to8.IsChecked == true)
        {
            SetMidiChannels();
            MT32SysEx.SendText("Channels 1-8 & 10");
        }
    }

    private void SetMidiChannels()
    {
        if (radioButtonChannels2to9.IsChecked == true)
        {
            system.SetMidiChannels2to9();
        }
        else if (radioButtonChannels1to8.IsChecked == true)
        {
            system.SetMidiChannels1to8();
        }
        SetSystemControls();
    }

    private void SetRadioButtons()
    {
        if (system.MidiChannelsAreSet1to8())
        {
            radioButtonChannels1to8.IsChecked = true;
        }
        else if (system.MidiChannelsAreSet2to9())
        {
            radioButtonChannels2to9.IsChecked = true;
        }
        else
        {
            radioButtonChannelCustom.IsChecked = true;
        }
    }

    private void TextBoxMessage1_TextChanged(object? sender, TextChangedEventArgs e)
    {
        string text = textBoxMessage1.Text ?? string.Empty;
        system.SetMessage(0, text);
        MT32SysEx.SendText(text);
    }

    private void TextBoxMessage2_TextChanged(object? sender, TextChangedEventArgs e)
    {
        string text = textBoxMessage2.Text ?? string.Empty;
        system.SetMessage(1, text);
        MT32SysEx.SendText(text);
    }

    private void ButtonSave_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        SaveSysExFile.SaveSystemOnly(
            system,
            checkBoxMasterLevel.IsChecked == true,
            checkBoxMasterTune.IsChecked == true,
            checkBoxReverb.IsChecked == true,
            checkBoxMIDIChannel.IsChecked == true,
            checkBoxPartialReserve.IsChecked == true,
            checkBoxTextMessages.IsChecked == true);
    }
}

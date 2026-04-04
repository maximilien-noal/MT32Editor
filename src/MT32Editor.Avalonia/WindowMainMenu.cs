using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace MT32Edit.Avalonia;

/// <summary>
/// Main application window - Avalonia equivalent of WinForms FormMainMenu.
/// Uses tabbed layout instead of MDI.
/// </summary>
public partial class WindowMainMenu : Window
{
    private const string VERSION_NO = "v1.1.0";
    private const string FRAMEWORK = "Avalonia";
    private const string RELEASE_DATE = "March 2026";

    private readonly MT32State memoryState = new MT32State();
    private PanelTimbreEditor? timbreEditor;
    private PanelPatchEditor? patchEditor;
    private PanelRhythmEditor? rhythmEditor;
    private PanelMemoryBankEditor? memoryBankEditor;
    private string titleBarFileName = "Untitled";
    private string? loadedSysExFileName;
    private int auditionNote = 60;
    private readonly DispatcherTimer timer;
    private readonly DispatcherTimer timerAutoSave;
    private TabControl? tabControl;
    private ComboBox? comboBoxMidiIn;
    private ComboBox? comboBoxMidiOut;

    public WindowMainMenu()
    {
        Title = "MT-32 Editor";
        Width = 1774;
        Height = 1038;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = AvaloniaUITools.GetBackgroundBrush();

        timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(UISettings.UI_REFRESH_INTERVAL) };
        timer.Tick += Timer_Tick;

        timerAutoSave = new DispatcherTimer { Interval = TimeSpan.FromMinutes(5) };
        timerAutoSave.Tick += TimerAutoSave_Tick;
    }

    public void Initialize(string[] args)
    {
        MT32SysEx.blockMT32text = true;
        Console.WriteLine($"Welcome to MT32 Editor {ParseTools.GetVersion(VERSION_NO)} ({FRAMEWORK})");

        string[] midiDeviceNames = ConfigFile.Load();
        SetROMCapabilities();
        BuildUI();
        SetWindowSizeAndPosition();
        SetupMidiDevices(midiDeviceNames);

        MT32SysEx.SendText($"MT32 Editor {ParseTools.TrimToLength(VERSION_NO, 8)}");
        timer.Start();
        if (SaveSysExFile.autoSave) timerAutoSave.Start();
        MT32SysEx.blockMT32text = false;

        ProcessShellArguments(args);
    }

    private void BuildUI()
    {
        var rootPanel = new DockPanel();

        // Menu bar
        var menuBar = new Menu();

        // File menu
        var fileMenu = new MenuItem { Header = "_File" };
        var loadSysEx = new MenuItem { Header = "Load SysEx File..." };
        loadSysEx.Click += (_, _) => LoadSysExFileAction();
        var saveSysExAs = new MenuItem { Header = "Save SysEx File As..." };
        saveSysExAs.Click += (_, _) => SaveSysExFileAsAction();
        var saveSysEx = new MenuItem { Header = "Save SysEx File" };
        saveSysEx.Click += (_, _) => SaveSysExFileAction();
        var sep1 = new Separator();
        var loadTimbre = new MenuItem { Header = "Load Timbre File..." };
        loadTimbre.Click += (_, _) =>
        {
            if (timbreEditor is not null)
            {
                string result = TimbreFile.Load(timbreEditor.TimbreData);
                if (FileTools.Success(result)) { UpdateTitleBar(result); timbreEditor.SetAllControlValues(); }
            }
        };
        var saveAllTimbres = new MenuItem { Header = "Save All Timbres..." };
        saveAllTimbres.Click += (_, _) => TimbreFile.SaveAll(memoryState.GetMemoryTimbreArray());
        var sep2 = new Separator();
        var exportInsDef = new MenuItem { Header = "Export Instrument Definition..." };
        exportInsDef.Click += (_, _) =>
        {
            string result = InstrumentDefinition.Save(memoryState, titleBarFileName);
            if (FileTools.Success(result)) UpdateTitleBar(result);
        };
        var sep3 = new Separator();
        var close = new MenuItem { Header = "Close" };
        close.Click += (_, _) => Close();
        fileMenu.Items.Add(loadSysEx);
        fileMenu.Items.Add(saveSysExAs);
        fileMenu.Items.Add(saveSysEx);
        fileMenu.Items.Add(sep1);
        fileMenu.Items.Add(loadTimbre);
        fileMenu.Items.Add(saveAllTimbres);
        fileMenu.Items.Add(sep2);
        fileMenu.Items.Add(exportInsDef);
        fileMenu.Items.Add(sep3);
        fileMenu.Items.Add(close);
        menuBar.Items.Add(fileMenu);

        // Options menu
        var optionsMenu = new MenuItem { Header = "_Options" };
        var systemSettings = new MenuItem { Header = "Master Settings..." };
        systemSettings.Click += (_, _) =>
        {
            var dlg = new WindowSystemSettings(memoryState.GetSystem());
            dlg.ShowDialog(this);
        };
        optionsMenu.Items.Add(systemSettings);

        var autosave = new MenuItem { Header = "Autosave Every 5 Minutes" };
        // Toggle icon would go here
        autosave.Click += (_, _) =>
        {
            SaveSysExFile.autoSave = !SaveSysExFile.autoSave;
            if (SaveSysExFile.autoSave) timerAutoSave.Start(); else timerAutoSave.Stop();
        };
        optionsMenu.Items.Add(autosave);

        var darkMode = new MenuItem { Header = "Dark Mode" };
        darkMode.Click += (_, _) => { UISettings.DarkMode = !UISettings.DarkMode; };
        optionsMenu.Items.Add(darkMode);

        var hwConnected = new MenuItem { Header = "Hardware MT-32 Connected" };
        hwConnected.Click += (_, _) => { MT32SysEx.hardwareMT32Connected = !MT32SysEx.hardwareMT32Connected; };
        optionsMenu.Items.Add(hwConnected);

        var cm32l = new MenuItem { Header = "CM-32L Mode" };
        cm32l.Click += (_, _) => { MT32SysEx.cm32LMode = !MT32SysEx.cm32LMode; };
        optionsMenu.Items.Add(cm32l);

        var sendMessages = new MenuItem { Header = "Send Messages to MT-32 Display" };
        sendMessages.Click += (_, _) => { MT32SysEx.sendTextToMT32 = !MT32SysEx.sendTextToMT32; };
        optionsMenu.Items.Add(sendMessages);

        var allowReset = new MenuItem { Header = "Allow MT-32 Reset" };
        allowReset.Click += (_, _) => { MT32SysEx.allowReset = !MT32SysEx.allowReset; };
        optionsMenu.Items.Add(allowReset);
        menuBar.Items.Add(optionsMenu);

        // Help menu
        var helpMenu = new MenuItem { Header = "_Help" };
        var about = new MenuItem { Header = "About" };
        about.Click += async (_, _) =>
        {
            var aboutWindow = new WindowAbout(VERSION_NO, FRAMEWORK, RELEASE_DATE);
            await aboutWindow.ShowDialog(this);
        };
        helpMenu.Items.Add(about);
        menuBar.Items.Add(helpMenu);

        // MIDI device selectors in toolbar
        var toolBar = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5, Margin = new Thickness(5, 2) };
        toolBar.Children.Add(new TextBlock { Text = "MIDI In:", VerticalAlignment = VerticalAlignment.Center, Foreground = AvaloniaUITools.GetForegroundBrush() });
        comboBoxMidiIn = new ComboBox { Width = 200 };
        comboBoxMidiIn.SelectionChanged += (_, _) =>
        {
            if (comboBoxMidiIn.SelectedIndex >= 0)
                Midi.OpenInputDevice(comboBoxMidiIn.SelectedIndex);
        };
        toolBar.Children.Add(comboBoxMidiIn);

        toolBar.Children.Add(new TextBlock { Text = "  MIDI Out:", VerticalAlignment = VerticalAlignment.Center, Foreground = AvaloniaUITools.GetForegroundBrush() });
        comboBoxMidiOut = new ComboBox { Width = 200 };
        comboBoxMidiOut.SelectionChanged += (_, _) =>
        {
            if (comboBoxMidiOut.SelectedIndex >= 0)
                Midi.OpenOutputDevice(comboBoxMidiOut.SelectedIndex);
        };
        toolBar.Children.Add(comboBoxMidiOut);

        // Audition button
        var auditionButton = new Button { Content = "♪ Audition" };
        auditionButton.PointerPressed += (_, e) =>
        {
            Midi.NoteOn(auditionNote, 0);
        };
        auditionButton.PointerReleased += (_, e) =>
        {
            Midi.NoteOff(auditionNote, 0);
        };
        toolBar.Children.Add(auditionButton);

        var toolBarPanel = new StackPanel { Spacing = 0 };
        toolBarPanel.Children.Add(menuBar);
        toolBarPanel.Children.Add(toolBar);
        DockPanel.SetDock(toolBarPanel, Dock.Top);
        rootPanel.Children.Add(toolBarPanel);

        // Create editor panels
        timbreEditor = new PanelTimbreEditor();
        timbreEditor.InitialiseTimbreParameters(editExisting: false);

        memoryBankEditor = new PanelMemoryBankEditor(memoryState, timbreEditor);
        patchEditor = new PanelPatchEditor();
        patchEditor.Initialize(memoryState);
        rhythmEditor = new PanelRhythmEditor();
        rhythmEditor.Initialize(memoryState);

        // Tabbed layout replacing MDI
        tabControl = new TabControl();
        tabControl.Items.Add(new TabItem { Header = "Timbre Editor", Content = timbreEditor });
        tabControl.Items.Add(new TabItem { Header = "Memory Bank", Content = memoryBankEditor });
        tabControl.Items.Add(new TabItem { Header = "Patch Editor", Content = patchEditor });
        tabControl.Items.Add(new TabItem { Header = "Rhythm Editor", Content = rhythmEditor });
        tabControl.SelectionChanged += (_, _) =>
        {
            // Notify panels of activation
            if (tabControl.SelectedItem is TabItem tab)
            {
                if (tab.Content == memoryBankEditor)
                    memoryBankEditor.OnPanelActivated();
            }
        };

        rootPanel.Children.Add(tabControl);
        Content = rootPanel;
    }

    private void SetupMidiDevices(string[] savedDeviceNames)
    {
        if (comboBoxMidiIn is null || comboBoxMidiOut is null) return;

        string[] inDevices = Midi.ListInputDevices();
        string[] outDevices = Midi.ListOutputDevices();

        comboBoxMidiIn.Items.Clear();
        foreach (string name in inDevices) comboBoxMidiIn.Items.Add(name);

        comboBoxMidiOut.Items.Clear();
        foreach (string name in outDevices) comboBoxMidiOut.Items.Add(name);

        // Try to select saved devices
        if (savedDeviceNames.Length >= 2)
        {
            for (int i = 0; i < inDevices.Length; i++)
                if (inDevices[i] == savedDeviceNames[0]) { comboBoxMidiIn.SelectedIndex = i; break; }
            for (int i = 0; i < outDevices.Length; i++)
                if (outDevices[i] == savedDeviceNames[1]) { comboBoxMidiOut.SelectedIndex = i; break; }
        }

        if (comboBoxMidiIn.SelectedIndex < 0 && comboBoxMidiIn.Items.Count > 0) comboBoxMidiIn.SelectedIndex = 0;
        if (comboBoxMidiOut.SelectedIndex < 0 && comboBoxMidiOut.Items.Count > 0) comboBoxMidiOut.SelectedIndex = 0;
    }

    private void SetROMCapabilities()
    {
        if (!MT32SysEx.cm32LMode)
        {
            memoryState.SetDefaultRhythmBanks();
        }
    }

    private void SetWindowSizeAndPosition()
    {
        if (UISettings.SaveWindowSizeAndPosition)
        {
            if (UISettings.WindowSize[0] > 0 && UISettings.WindowSize[1] > 0)
            {
                Width = UISettings.WindowSize[0];
                Height = UISettings.WindowSize[1];
            }
            if (UISettings.WindowLocation[0] > 0 || UISettings.WindowLocation[1] > 0)
            {
                Position = new PixelPoint(UISettings.WindowLocation[0], UISettings.WindowLocation[1]);
                WindowStartupLocation = WindowStartupLocation.Manual;
            }
            if (UISettings.WindowMaximised)
            {
                WindowState = WindowState.Maximized;
            }
        }
    }

    private void ProcessShellArguments(string[] args)
    {
        if (args.Length > 0 && FileTools.IsSysExOrMidi(args[0]))
        {
            string result = LoadSysExFile.Load(memoryState, args[0]);
            if (FileTools.Success(result)) UpdateTitleBar(result);
        }
    }

    private void LoadSysExFileAction()
    {
        string result = LoadSysExFile.Load(memoryState);
        if (FileTools.Success(result))
        {
            loadedSysExFileName = result;
            UpdateTitleBar(result);
            memoryState.changesMade = false;
        }
    }

    private void SaveSysExFileAsAction()
    {
        string result = SaveSysExFile.SaveAs(memoryState, titleBarFileName);
        if (FileTools.Success(result))
        {
            loadedSysExFileName = result;
            UpdateTitleBar(result);
            memoryState.changesMade = false;
        }
    }

    private void SaveSysExFileAction()
    {
        if (loadedSysExFileName is not null)
        {
            SaveSysExFile.Save(memoryState, loadedSysExFileName, checkBeforeOverwriting: false);
            memoryState.changesMade = false;
        }
        else
        {
            SaveSysExFileAsAction();
        }
    }

    private void UpdateTitleBar(string fileName)
    {
        titleBarFileName = fileName;
        string message = memoryState.GetSystem().GetMessage(0);
        Title = UITools.TitleBarText(fileName, titleBarFileName, message, memoryState.changesMade);
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        // Periodic UI refresh
        Title = UITools.TitleBarText(titleBarFileName, titleBarFileName, memoryState.GetSystem().GetMessage(0), memoryState.changesMade);
    }

    private void TimerAutoSave_Tick(object? sender, EventArgs e)
    {
        if (SaveSysExFile.autoSave)
        {
            SaveSysExFile.Save(memoryState, FileTools.autoSaveFileLocation, checkBeforeOverwriting: false);
            ConsoleMessage.SendVerboseLine("Autosaved.");
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        // Save config on exit
        if (UISettings.SaveWindowSizeAndPosition)
        {
            UISettings.WindowSize[0] = (int)Width;
            UISettings.WindowSize[1] = (int)Height;
            UISettings.WindowLocation[0] = Position.X;
            UISettings.WindowLocation[1] = Position.Y;
            UISettings.WindowMaximised = (WindowState == WindowState.Maximized);
        }
        ConfigFile.Save();
        Midi.CloseOutputDevice();
        Midi.CloseInputDevice();
        base.OnClosing(e);
    }
}

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
    private const int MINIMUM_WIDTH = 1220;
    private const int MINIMUM_HEIGHT = 1036;

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
    private MenuItem? saveSysExMenuItem;

    // Toggle menu items that need checkmark state
    private MenuItem? menuAutosave;
    private MenuItem? menuDarkMode;
    private MenuItem? menuHwConnected;
    private MenuItem? menuCm32l;
    private MenuItem? menuSendMessages;
    private MenuItem? menuAllowReset;
    private MenuItem? menuSaveWindowSize;
    private MenuItem? menuIgnoreSysConfigOnLoad;
    private MenuItem? menuExcludeSysConfigOnSave;

    public WindowMainMenu()
    {
        Title = "Untitled - MT-32 Editor";
        Width = 1774;
        Height = 1038;
        MinWidth = MINIMUM_WIDTH;
        MinHeight = MINIMUM_HEIGHT;
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
        SetOptionMenuFlags();
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

        // --- File menu ---
        var fileMenu = new MenuItem { Header = "_File" };
        var loadSysEx = new MenuItem { Header = "Load SysEx File..." };
        loadSysEx.Click += (_, _) => LoadSysExFileAction();
        var saveSysExAs = new MenuItem { Header = "Save SysEx File As..." };
        saveSysExAs.Click += (_, _) => SaveSysExFileAsAction();
        saveSysExMenuItem = new MenuItem { Header = "Save SysEx File", IsEnabled = false };
        saveSysExMenuItem.Click += (_, _) => SaveSysExFileAction();
        var sep1 = new Separator();
        var loadTimbre = new MenuItem { Header = "Load Timbre File..." };
        loadTimbre.Click += (_, _) =>
        {
            if (timbreEditor is not null)
            {
                string result = TimbreFile.Load(timbreEditor.TimbreData);
                if (FileTools.Success(result))
                {
                    UpdateTitleBar(result);
                    timbreEditor.SetAllControlValues();
                    memoryState.enableTimbreSaveButton = true;
                    // Switch to timbre editor tab
                    if (tabControl is not null) tabControl.SelectedIndex = 0;
                }
            }
        };
        var saveTimbre = new MenuItem { Header = "Save Timbre File" };
        saveTimbre.Click += (_, _) =>
        {
            if (timbreEditor is not null)
            {
                string? filePath = PlatformServices.FileDialog.ShowSaveFileDialog("Save Timbre File", "Timbre file|*.timbre", timbreEditor.TimbreData.GetTimbreName());
                if (filePath is not null)
                {
                    var fs = File.Create(filePath);
                    TimbreFile.SaveTimbreParameters(timbreEditor.TimbreData, fs);
                    TimbreFile.SavePartials(timbreEditor.TimbreData, fs);
                    fs.Close();
                }
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
        fileMenu.Items.Add(saveSysExMenuItem);
        fileMenu.Items.Add(sep1);
        fileMenu.Items.Add(loadTimbre);
        fileMenu.Items.Add(saveTimbre);
        fileMenu.Items.Add(saveAllTimbres);
        fileMenu.Items.Add(sep2);
        fileMenu.Items.Add(exportInsDef);
        fileMenu.Items.Add(sep3);
        fileMenu.Items.Add(close);
        menuBar.Items.Add(fileMenu);

        // --- Options menu ---
        var optionsMenu = new MenuItem { Header = "_Options" };
        var systemSettings = new MenuItem { Header = "Master Settings..." };
        systemSettings.Click += (_, _) =>
        {
            var dlg = new WindowSystemSettings(memoryState.GetSystem());
            dlg.ShowDialog(this);
        };
        optionsMenu.Items.Add(systemSettings);

        menuAutosave = CreateToggleMenuItem("Autosave Every 5 Minutes", SaveSysExFile.autoSave);
        menuAutosave.Click += (_, _) =>
        {
            SaveSysExFile.autoSave = !SaveSysExFile.autoSave;
            UpdateToggleMenuHeader(menuAutosave, "Autosave Every 5 Minutes", SaveSysExFile.autoSave);
            if (SaveSysExFile.autoSave) timerAutoSave.Start(); else timerAutoSave.Stop();
            ConfigFile.Save();
        };
        optionsMenu.Items.Add(menuAutosave);

        menuSaveWindowSize = CreateToggleMenuItem("Save Window Size and Position", UISettings.SaveWindowSizeAndPosition);
        menuSaveWindowSize.Click += (_, _) =>
        {
            UISettings.SaveWindowSizeAndPosition = !UISettings.SaveWindowSizeAndPosition;
            UpdateToggleMenuHeader(menuSaveWindowSize, "Save Window Size and Position", UISettings.SaveWindowSizeAndPosition);
            ConfigFile.Save();
        };
        optionsMenu.Items.Add(menuSaveWindowSize);

        menuIgnoreSysConfigOnLoad = CreateToggleMenuItem("Ignore System Config on Load", LoadSysExFile.ignoreSystemArea);
        menuIgnoreSysConfigOnLoad.Click += (_, _) =>
        {
            LoadSysExFile.ignoreSystemArea = !LoadSysExFile.ignoreSystemArea;
            UpdateToggleMenuHeader(menuIgnoreSysConfigOnLoad, "Ignore System Config on Load", LoadSysExFile.ignoreSystemArea);
            ConfigFile.Save();
        };
        optionsMenu.Items.Add(menuIgnoreSysConfigOnLoad);

        menuExcludeSysConfigOnSave = CreateToggleMenuItem("Exclude System Config on Save", SaveSysExFile.excludeSystemArea);
        menuExcludeSysConfigOnSave.Click += (_, _) =>
        {
            SaveSysExFile.excludeSystemArea = !SaveSysExFile.excludeSystemArea;
            UpdateToggleMenuHeader(menuExcludeSysConfigOnSave, "Exclude System Config on Save", SaveSysExFile.excludeSystemArea);
            ConfigFile.Save();
        };
        optionsMenu.Items.Add(menuExcludeSysConfigOnSave);

        optionsMenu.Items.Add(new Separator());

        menuHwConnected = CreateToggleMenuItem("Hardware MT-32 Connected", MT32SysEx.hardwareMT32Connected);
        menuHwConnected.Click += (_, _) =>
        {
            MT32SysEx.hardwareMT32Connected = !MT32SysEx.hardwareMT32Connected;
            UpdateToggleMenuHeader(menuHwConnected, "Hardware MT-32 Connected", MT32SysEx.hardwareMT32Connected);
            ConfigFile.Save();
        };
        optionsMenu.Items.Add(menuHwConnected);

        menuSendMessages = CreateToggleMenuItem("Send Messages to MT-32 Display", MT32SysEx.sendTextToMT32);
        menuSendMessages.Click += (_, _) =>
        {
            MT32SysEx.sendTextToMT32 = !MT32SysEx.sendTextToMT32;
            UpdateToggleMenuHeader(menuSendMessages, "Send Messages to MT-32 Display", MT32SysEx.sendTextToMT32);
            ConfigFile.Save();
        };
        optionsMenu.Items.Add(menuSendMessages);

        menuAllowReset = CreateToggleMenuItem("Allow MT-32 Reset", MT32SysEx.allowReset);
        menuAllowReset.Click += (_, _) =>
        {
            MT32SysEx.allowReset = !MT32SysEx.allowReset;
            UpdateToggleMenuHeader(menuAllowReset, "Allow MT-32 Reset", MT32SysEx.allowReset);
            ConfigFile.Save();
        };
        optionsMenu.Items.Add(menuAllowReset);

        menuCm32l = CreateToggleMenuItem("CM-32L Mode (requires restart)", MT32SysEx.cm32LMode);
        menuCm32l.Click += (_, _) =>
        {
            MT32SysEx.cm32LMode = !MT32SysEx.cm32LMode;
            UpdateToggleMenuHeader(menuCm32l, "CM-32L Mode (requires restart)", MT32SysEx.cm32LMode);
            ConfigFile.Save();
        };
        optionsMenu.Items.Add(menuCm32l);

        optionsMenu.Items.Add(new Separator());

        menuDarkMode = CreateToggleMenuItem("Dark Mode", UISettings.DarkMode);
        menuDarkMode.Click += (_, _) =>
        {
            UISettings.DarkMode = !UISettings.DarkMode;
            UpdateToggleMenuHeader(menuDarkMode, "Dark Mode", UISettings.DarkMode);
            ConfigFile.Save();
        };
        optionsMenu.Items.Add(menuDarkMode);
        menuBar.Items.Add(optionsMenu);

        // --- Help menu ---
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
            {
                Midi.OpenInputDevice(comboBoxMidiIn.SelectedIndex);
                ConfigFile.Save();
            }
        };
        toolBar.Children.Add(comboBoxMidiIn);

        toolBar.Children.Add(new TextBlock { Text = "  MIDI Out:", VerticalAlignment = VerticalAlignment.Center, Foreground = AvaloniaUITools.GetForegroundBrush() });
        comboBoxMidiOut = new ComboBox { Width = 200 };
        comboBoxMidiOut.SelectionChanged += (_, _) =>
        {
            if (comboBoxMidiOut.SelectedIndex >= 0)
            {
                Midi.OpenOutputDevice(comboBoxMidiOut.SelectedIndex);
                ConfigFile.Save();
            }
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
                memoryState.patchEditorActive = (tab.Content == patchEditor);
                memoryState.rhythmEditorActive = (tab.Content == rhythmEditor);
                memoryState.memoryBankEditorActive = (tab.Content == memoryBankEditor);

                if (tab.Content == memoryBankEditor)
                    memoryBankEditor.OnPanelActivated();
            }
        };

        rootPanel.Children.Add(tabControl);
        Content = rootPanel;
    }

    /// <summary>
    /// Creates a menu item with a checkmark prefix indicating toggle state.
    /// </summary>
    private static MenuItem CreateToggleMenuItem(string text, bool isChecked)
    {
        return new MenuItem { Header = FormatToggleHeader(text, isChecked) };
    }

    /// <summary>
    /// Updates menu item header text with checkmark indicator.
    /// </summary>
    private static void UpdateToggleMenuHeader(MenuItem item, string text, bool isChecked)
    {
        item.Header = FormatToggleHeader(text, isChecked);
    }

    /// <summary>
    /// Formats a menu header with a ✓ or ☐ prefix to indicate toggle state.
    /// </summary>
    private static string FormatToggleHeader(string text, bool isChecked)
    {
        return isChecked ? $"✓ {text}" : $"☐ {text}";
    }

    /// <summary>
    /// Syncs all toggle menu item headers with current property values.
    /// Called after loading config file.
    /// </summary>
    private void SetOptionMenuFlags()
    {
        if (menuAutosave is not null) UpdateToggleMenuHeader(menuAutosave, "Autosave Every 5 Minutes", SaveSysExFile.autoSave);
        if (menuSaveWindowSize is not null) UpdateToggleMenuHeader(menuSaveWindowSize, "Save Window Size and Position", UISettings.SaveWindowSizeAndPosition);
        if (menuIgnoreSysConfigOnLoad is not null) UpdateToggleMenuHeader(menuIgnoreSysConfigOnLoad, "Ignore System Config on Load", LoadSysExFile.ignoreSystemArea);
        if (menuExcludeSysConfigOnSave is not null) UpdateToggleMenuHeader(menuExcludeSysConfigOnSave, "Exclude System Config on Save", SaveSysExFile.excludeSystemArea);
        if (menuHwConnected is not null) UpdateToggleMenuHeader(menuHwConnected, "Hardware MT-32 Connected", MT32SysEx.hardwareMT32Connected);
        if (menuSendMessages is not null) UpdateToggleMenuHeader(menuSendMessages, "Send Messages to MT-32 Display", MT32SysEx.sendTextToMT32);
        if (menuAllowReset is not null) UpdateToggleMenuHeader(menuAllowReset, "Allow MT-32 Reset", MT32SysEx.allowReset);
        if (menuCm32l is not null) UpdateToggleMenuHeader(menuCm32l, "CM-32L Mode (requires restart)", MT32SysEx.cm32LMode);
        if (menuDarkMode is not null) UpdateToggleMenuHeader(menuDarkMode, "Dark Mode", UISettings.DarkMode);
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
            memoryState.SetDefaultMT32RhythmBanks();
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
            if (FileTools.Success(result))
            {
                loadedSysExFileName = result;
                UpdateTitleBar(result);
                if (saveSysExMenuItem is not null) saveSysExMenuItem.IsEnabled = true;
            }
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
            if (saveSysExMenuItem is not null) saveSysExMenuItem.IsEnabled = true;
            // Refresh editor panels
            timbreEditor?.SetAllControlValues();
            patchEditor?.RefreshDisplay();
            rhythmEditor?.RefreshDisplay();
            memoryBankEditor?.OnPanelActivated();
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
            if (saveSysExMenuItem is not null) saveSysExMenuItem.IsEnabled = true;
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
        Title = AvaloniaUITools.TitleBarText(fileName, titleBarFileName, message, memoryState.changesMade);
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        // Periodic UI refresh - mirrors WinForms timer_Tick
        Title = AvaloniaUITools.TitleBarText(titleBarFileName, titleBarFileName, memoryState.GetSystem().GetMessage(0), memoryState.changesMade);

        // Sync editor panel enabled states based on timbre editability
        if (timbreEditor is not null)
        {
            timbreEditor.IsEnabled = (!memoryState.patchEditorActive && !memoryState.rhythmEditorActive)
                                     || memoryState.TimbreIsEditable();
        }

        // Handle focus requests from core logic
        if (memoryState.returnFocusToPatchEditor && tabControl is not null)
        {
            tabControl.SelectedIndex = 2; // Patch Editor tab
            memoryState.returnFocusToPatchEditor = false;
        }
        if (memoryState.returnFocusToRhythmEditor && tabControl is not null)
        {
            tabControl.SelectedIndex = 3; // Rhythm Editor tab
            memoryState.returnFocusToRhythmEditor = false;
        }
        if (memoryState.returnFocusToMemoryBankList && tabControl is not null)
        {
            tabControl.SelectedIndex = 1; // Memory Bank tab
            memoryState.returnFocusToMemoryBankList = false;
        }
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

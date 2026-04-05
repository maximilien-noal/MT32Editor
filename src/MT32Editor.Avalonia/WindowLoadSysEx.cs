using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace MT32Edit.Avalonia;

/// <summary>
/// SysEx loading progress window - Avalonia equivalent of WinForms FormLoadSysEx.
/// </summary>
public class WindowLoadSysEx : Window
{
    private readonly MT32State memoryState;
    private readonly ProgressBar progressBar;
    private readonly TextBlock labelLoadProgress;
    private readonly TextBlock labelMT32Text1;
    private readonly TextBlock labelMT32Text2;
    private readonly Button buttonClose;
    private readonly DispatcherTimer timer;

    private int timbreNo = 0;
    private int patchNo = 0;
    private int keyNo = 24;
    private const int PATCHES_PER_BLOCK = 32;
    private const int RHYTHM_BANKS_PER_BLOCK = 42;
    private int stepNo = 0;
    private readonly bool clearMemory;

    public WindowLoadSysEx(MT32State inputMemoryState, bool requestClearMemory)
    {
        Title = "Uploading SysEx Data";
        Width = 336;
        Height = 130;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = new SolidColorBrush(Color.FromRgb(48, 48, 48));

        MT32SysEx.blockSysExMessages = true;
        clearMemory = requestClearMemory;
        memoryState = inputMemoryState;

        var mainPanel = new Grid
        {
            Margin = new Thickness(12, 9, 12, 10),
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto")
        };

        // Row 0: MT-32 text messages
        var textPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        labelMT32Text1 = new TextBlock
        {
            Text = ParseTools.RemoveLeadingSpaces(memoryState.GetSystem().GetMessage(0)),
            Foreground = Brushes.White
        };
        labelMT32Text2 = new TextBlock
        {
            Text = ParseTools.RemoveLeadingSpaces(memoryState.GetSystem().GetMessage(1)),
            Foreground = Brushes.White
        };
        textPanel.Children.Add(labelMT32Text1);
        textPanel.Children.Add(labelMT32Text2);
        Grid.SetRow(textPanel, 0);
        mainPanel.Children.Add(textPanel);

        // Row 1: Progress bar
        int maxValue = 66 + (88 / RHYTHM_BANKS_PER_BLOCK) + (128 / PATCHES_PER_BLOCK);
        progressBar = new ProgressBar
        {
            Minimum = 0,
            Maximum = maxValue,
            Value = 0,
            Height = 23,
            Margin = new Thickness(0, 8, 0, 5)
        };
        Grid.SetRow(progressBar, 1);
        mainPanel.Children.Add(progressBar);

        // Row 2: Status + close button
        var statusPanel = new DockPanel();
        labelLoadProgress = new TextBlock
        {
            Text = clearMemory ? "Clearing Memory Timbres" : "Uploading SysEx data",
            Foreground = new SolidColorBrush(Color.FromRgb(240, 240, 240))
        };
        buttonClose = new Button
        {
            Content = "Close",
            Width = 58,
            Height = 26,
            IsVisible = false
        };
        buttonClose.Click += (_, _) => Close();
        DockPanel.SetDock(buttonClose, Dock.Right);
        statusPanel.Children.Add(buttonClose);
        statusPanel.Children.Add(labelLoadProgress);
        Grid.SetRow(statusPanel, 2);
        mainPanel.Children.Add(statusPanel);

        Content = mainPanel;

        // Timer setup
        int interval = MT32SysEx.hardwareMT32Connected ? MT32SysEx.MT32_DELAY : 5;
        timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(interval) };
        timer.Tick += Timer_Tick;
        MT32SysEx.blockSysExMessages = false;
        timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        switch (stepNo)
        {
            case 0:
                SendSystemArea();
                break;
            case 1:
                if (!clearMemory && patchNo < 128)
                    SendNextPatchBlock();
                else
                    stepNo++;
                break;
            case 2:
                if (!clearMemory && keyNo < 104)
                    SendNextRhythmBankBlock();
                else
                    stepNo++;
                break;
            case 3:
                stepNo++;
                break;
            case 4:
                stepNo++;
                break;
            default:
                if (timbreNo < 64)
                    SendNextMemoryTimbre();
                else
                    Finish();
                break;
        }
    }

    private void Finish()
    {
        labelLoadProgress.Text = "SysEx load completed";
        timer.Stop();
        Close();
    }

    private void SendSystemArea()
    {
        if (!clearMemory)
        {
            labelLoadProgress.Text = "Loading system memory area";
            MT32SysEx.SendSystemParameters(memoryState.GetSystem());
        }
        progressBar.Value++;
        stepNo++;
    }

    private void SendNextPatchBlock()
    {
        labelLoadProgress.Text = "Loading patch data";
        MT32SysEx.SendPatchBlock(memoryState.GetPatchArray(), patchNo, patchNo + PATCHES_PER_BLOCK - 1);
        labelLoadProgress.Text = $"Loading patches {patchNo + 1}-{patchNo + PATCHES_PER_BLOCK}";
        patchNo += PATCHES_PER_BLOCK;
        progressBar.Value++;
    }

    private void SendNextRhythmBankBlock()
    {
        labelLoadProgress.Text = "Loading rhythm data";
        MT32SysEx.SendRhythmKeyBlock(memoryState.GetRhythmBankArray(), keyNo, keyNo + RHYTHM_BANKS_PER_BLOCK - 1);
        keyNo += RHYTHM_BANKS_PER_BLOCK;
        progressBar.Value++;
    }

    private void SendNextMemoryTimbre()
    {
        MT32SysEx.SendMemoryTimbre(timbreNo, memoryState.GetMemoryTimbre(timbreNo));
        if (clearMemory)
            labelLoadProgress.Text = $"Clearing timbre memory {timbreNo + 1} of 64";
        else
            labelLoadProgress.Text = $"Loading {memoryState.GetMemoryTimbre(timbreNo).GetTimbreName()} ({timbreNo} of 64)";
        timbreNo++;
        progressBar.Value++;
    }
}

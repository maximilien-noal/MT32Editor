using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace MT32Edit.Avalonia;

/// <summary>
/// Memory bank selection dialog - Avalonia equivalent of WinForms FormSelectMemoryBank.
/// </summary>
public class WindowSelectMemoryBank : Window
{
    private const int MEMORY_GROUP = 2;

    private readonly MT32State memoryState;
    private readonly string presetTimbreName;
    private readonly ComboBox comboBoxMemoryBank;
    private readonly Button buttonOK;

    public WindowSelectMemoryBank(MT32State memoryStateInput, string timbreNameInput)
    {
        memoryState = memoryStateInput;
        presetTimbreName = timbreNameInput;

        Title = "Select Memory Bank";
        Width = 308;
        Height = 140;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = new SolidColorBrush(Color.FromRgb(64, 64, 64));

        var mainPanel = new StackPanel
        {
            Margin = new Thickness(12, 18, 12, 10),
            Spacing = 8
        };

        var labelSelectMemoryBank = new TextBlock
        {
            Text = $"Select memory bank slot for {presetTimbreName}:",
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(240, 240, 240))
        };
        mainPanel.Children.Add(labelSelectMemoryBank);

        var controlPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        comboBoxMemoryBank = new ComboBox { Width = 172 };
        PopulateComboBox();
        comboBoxMemoryBank.SelectionChanged += ComboBoxMemoryBank_SelectionChanged;
        controlPanel.Children.Add(comboBoxMemoryBank);

        var buttonPanel = new StackPanel { Spacing = 4 };
        buttonOK = new Button { Content = "OK", Width = 71, Height = 25 };
        buttonOK.Click += ButtonOK_Click;
        var buttonCancel = new Button { Content = "Cancel", Width = 71, Height = 26 };
        buttonCancel.Click += (_, _) => Close();
        buttonPanel.Children.Add(buttonOK);
        buttonPanel.Children.Add(buttonCancel);
        controlPanel.Children.Add(buttonPanel);

        mainPanel.Children.Add(controlPanel);
        Content = mainPanel;
    }

    private void PopulateComboBox()
    {
        string[] memoryTimbreNames = memoryState.GetTimbreNames().GetAll(MEMORY_GROUP);
        var enumeratedTimbreNames = new string[memoryTimbreNames.Length];
        for (int timbreNo = 0; timbreNo < memoryTimbreNames.Length; timbreNo++)
        {
            enumeratedTimbreNames[timbreNo] = $"{timbreNo + 1}:   {memoryTimbreNames[timbreNo]}";
        }
        comboBoxMemoryBank.ItemsSource = enumeratedTimbreNames;
        comboBoxMemoryBank.SelectedIndex = 0;
    }

    private void ComboBoxMemoryBank_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (comboBoxMemoryBank.SelectedIndex < 0) return;
        string timbreName = ParseTools.RightMost(memoryState.GetTimbreNames().Get(comboBoxMemoryBank.SelectedIndex, MEMORY_GROUP), MT32Strings.EMPTY.Length);
        buttonOK.Content = (timbreName == MT32Strings.EMPTY) ? "OK" : "Replace";
    }

    private void ButtonOK_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if ((string?)buttonOK.Content == "Replace")
        {
            if (!PlatformServices.Notification.AskUserToConfirm(
                $"This memory slot is already occupied. Overwrite {memoryState.GetTimbreNames().Get(comboBoxMemoryBank.SelectedIndex, MEMORY_GROUP)} with preset timbre {presetTimbreName}?",
                "Confirm timbre replacement"))
            {
                return;
            }
        }
        ReplaceMemoryTimbre();
        Close();
    }

    private void ReplaceMemoryTimbre()
    {
        int patchNo = memoryState.GetSelectedPatchNo();
        Patch selectedPatch = memoryState.GetPatch(patchNo);
        TimbreStructure timbre = PresetTimbres.Get(selectedPatch.GetTimbreNo(), selectedPatch.GetTimbreGroup());
        memoryState.SetMemoryTimbre(timbre, comboBoxMemoryBank.SelectedIndex);
        selectedPatch.SetTimbreGroup(MEMORY_GROUP);
        selectedPatch.SetTimbreNo(comboBoxMemoryBank.SelectedIndex);
    }
}

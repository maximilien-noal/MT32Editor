using Avalonia.Controls;
using Avalonia.Media;

namespace MT32Edit.Avalonia;

/// <summary>
/// Shared UI tools for Avalonia windows - equivalent of WinForms UITools.
/// Uses UISettings from Core for shared settings, provides Avalonia-specific theming.
/// </summary>
internal static class AvaloniaUITools
{
    /// <summary>
    /// Returns the appropriate background color based on current theme.
    /// </summary>
    public static IBrush GetBackgroundBrush(bool alternate = false)
    {
        if (UISettings.DarkMode)
        {
            return alternate
                ? new SolidColorBrush(Color.FromRgb(32, 32, 32))
                : new SolidColorBrush(Color.FromRgb(56, 56, 56));
        }
        return alternate
            ? new SolidColorBrush(Color.FromRgb(240, 240, 240))
            : new SolidColorBrush(Color.FromRgb(245, 245, 245));
    }

    public static IBrush GetForegroundBrush()
    {
        return UISettings.DarkMode
            ? new SolidColorBrush(Color.FromRgb(240, 240, 240))
            : Brushes.Black;
    }

    public static IBrush GetTitleBrush()
    {
        return UISettings.DarkMode
            ? new SolidColorBrush(Color.FromRgb(153, 180, 209))
            : new SolidColorBrush(Color.FromRgb(72, 61, 139));
    }

    public static IBrush GetWarningBrush()
    {
        return UISettings.DarkMode ? Brushes.Yellow : Brushes.OrangeRed;
    }

    public static IBrush GetListViewBackgroundBrush()
    {
        return UISettings.DarkMode
            ? new SolidColorBrush(Color.FromRgb(84, 84, 84))
            : new SolidColorBrush(Color.FromRgb(255, 250, 250));
    }

    /// <summary>
    /// Apply theme to a set of TextBlock labels.
    /// </summary>
    public static void ApplyTheme(TextBlock? titleLabel, TextBlock[]? labels, TextBlock[]? warningLabels)
    {
        if (titleLabel is not null)
            titleLabel.Foreground = GetTitleBrush();

        if (labels is not null)
        {
            foreach (var label in labels)
                label.Foreground = GetForegroundBrush();
        }

        if (warningLabels is not null)
        {
            foreach (var label in warningLabels)
                label.Foreground = GetWarningBrush();
        }
    }

    /// <summary>
    /// Apply theme to CheckBox and RadioButton controls.
    /// </summary>
    public static void ApplyThemeToControls(CheckBox[]? checkBoxes, RadioButton[]? radioButtons)
    {
        var brush = GetForegroundBrush();
        if (checkBoxes is not null)
        {
            foreach (var cb in checkBoxes)
                cb.Foreground = brush;
        }
        if (radioButtons is not null)
        {
            foreach (var rb in radioButtons)
                rb.Foreground = brush;
        }
    }

    /// <summary>
    /// Sets heading colors for Pitch, TVF, TVA labels.
    /// </summary>
    public static void SetGroupHeadingColours(TextBlock labelPitch, TextBlock labelTVF, TextBlock labelTVA)
    {
        if (UISettings.DarkMode)
        {
            labelPitch.Foreground = new SolidColorBrush(Color.FromRgb(221, 160, 221));
            labelTVA.Foreground = new SolidColorBrush(Color.FromRgb(102, 205, 170));
            labelTVF.Foreground = new SolidColorBrush(Color.FromRgb(240, 230, 140));
        }
        else
        {
            labelPitch.Foreground = new SolidColorBrush(Color.FromRgb(153, 50, 204));
            labelTVA.Foreground = new SolidColorBrush(Color.FromRgb(72, 61, 139));
            labelTVF.Foreground = new SolidColorBrush(Color.FromRgb(165, 42, 42));
        }
    }

    /// <summary>
    /// Returns title bar text - mirrors the logic from WinForms UITools.TitleBarText.
    /// </summary>
    public static string TitleBarText(string newFileName, string currentFileName, string textMessage = "", bool changesMade = false)
    {
        if (System.IO.Path.GetExtension(newFileName).ToLower() == FileTools.TIMBRE_FILE && FileTools.IsSysExOrMidi(currentFileName))
        {
            return $"{SysExFileDescription()}{System.IO.Path.GetFileName(currentFileName)}{ConditionalCloseBracket()}{ParseTools.UnsavedEdits(changesMade)} - MT32 Editor";
        }
        return $"{SysExFileDescription()}{System.IO.Path.GetFileName(newFileName)}{ConditionalCloseBracket()}{ParseTools.UnsavedEdits(changesMade)} - MT32 Editor";

        string SysExFileDescription()
        {
            if (string.IsNullOrEmpty(textMessage))
            {
                return string.Empty;
            }
            return $"{ParseTools.RemoveTrailingSpaces(ParseTools.RemoveLeadingSpaces(textMessage))} [";
        }

        string ConditionalCloseBracket()
        {
            if (string.IsNullOrEmpty(textMessage))
            {
                return string.Empty;
            }
            return "]";
        }
    }
}

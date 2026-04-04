using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace MT32Edit.Avalonia;

/// <summary>
/// About dialog - Avalonia equivalent of WinForms FormAbout.
/// </summary>
public class WindowAbout : Window
{
    private readonly TextBlock labelVersionNo;
    private readonly TextBlock labelFramework;
    private readonly TextBlock labelReleaseDate;

    public WindowAbout(string versionNo, string frameworkID, string date)
    {
        Title = "About MT-32 Editor";
        Width = 306;
        Height = 286;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = new SolidColorBrush(Color.FromRgb(160, 160, 150));

        var mainPanel = new StackPanel
        {
            Margin = new Thickness(17, 18, 17, 10),
            Spacing = 5
        };

        var titleText = new TextBlock
        {
            Text = "MT-32 Editor",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            Margin = new Thickness(0, 0, 0, 10)
        };
        mainPanel.Children.Add(titleText);

        labelVersionNo = new TextBlock
        {
            Text = versionNo,
            Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0))
        };
        mainPanel.Children.Add(labelVersionNo);

        string framework = string.IsNullOrWhiteSpace(frameworkID) ? "" : $"({frameworkID})";
        labelFramework = new TextBlock
        {
            Text = framework,
            Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0))
        };
        mainPanel.Children.Add(labelFramework);

        labelReleaseDate = new TextBlock
        {
            Text = date,
            Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0))
        };
        mainPanel.Children.Add(labelReleaseDate);

        mainPanel.Children.Add(new Panel { Height = 20 });

        mainPanel.Children.Add(new TextBlock
        {
            Text = " by S. Fryers",
            Foreground = Brushes.White
        });

        mainPanel.Children.Add(new TextBlock
        {
            Text = "A patch/timbre editor and SysEx librarian for"
        });

        mainPanel.Children.Add(new TextBlock
        {
            Text = "MT-32, CM-32L and compatible synthesizers."
        });

        mainPanel.Children.Add(new TextBlock
        {
            Text = "Licenced under GPL 3.0",
            Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
            Margin = new Thickness(0, 10, 0, 0)
        });

        var linkButton = new Button
        {
            Content = "https://github.com/sfryers/MT32Editor",
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Foreground = new SolidColorBrush(Color.FromRgb(0, 102, 204)),
            Cursor = new global::Avalonia.Input.Cursor(global::Avalonia.Input.StandardCursorType.Hand),
            Padding = new Thickness(0)
        };
        linkButton.Click += async (_, _) =>
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard is not null)
            {
                await clipboard.SetTextAsync("https://github.com/sfryers/MT32Editor");
            }
            PlatformServices.Notification.ShowMessage("URL copied to clipboard.", "MT-32 Editor");
        };
        mainPanel.Children.Add(linkButton);

        var closeButton = new Button
        {
            Content = "Close",
            Width = 54,
            Height = 23,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 10, 0, 0)
        };
        closeButton.Click += (_, _) => Close();
        mainPanel.Children.Add(closeButton);

        Content = mainPanel;
    }
}

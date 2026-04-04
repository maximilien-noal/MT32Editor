using Avalonia.Controls;
using Avalonia.Threading;

namespace MT32Edit.Avalonia;

/// <summary>
/// Avalonia implementation of IUserNotification using simple message box windows.
/// </summary>
internal class AvaloniaPlatformNotification : IUserNotification
{
    private readonly Window _owner;

    public AvaloniaPlatformNotification(Window owner)
    {
        _owner = owner;
    }

    public void ShowMessage(string message, string title = "MT-32 Editor")
    {
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                ShowInTaskbar = false
            };

            var panel = new StackPanel { Margin = new global::Avalonia.Thickness(20), Spacing = 15 };
            panel.Children.Add(new TextBlock
            {
                Text = message,
                TextWrapping = global::Avalonia.Media.TextWrapping.Wrap
            });

            var okButton = new Button { Content = "OK", HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center, Width = 80 };
            okButton.Click += (_, _) => dialog.Close();
            panel.Children.Add(okButton);

            dialog.Content = panel;
            await dialog.ShowDialog(_owner);
        }).Wait();
    }

    public bool AskUserToConfirm(string prompt, string title = "MT-32 Editor")
    {
        bool result = false;
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                ShowInTaskbar = false
            };

            var panel = new StackPanel { Margin = new global::Avalonia.Thickness(20), Spacing = 15 };
            panel.Children.Add(new TextBlock
            {
                Text = prompt,
                TextWrapping = global::Avalonia.Media.TextWrapping.Wrap
            });

            var buttonPanel = new StackPanel { Orientation = global::Avalonia.Layout.Orientation.Horizontal, Spacing = 10, HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center };
            var okButton = new Button { Content = "OK", Width = 80 };
            var cancelButton = new Button { Content = "Cancel", Width = 80 };
            okButton.Click += (_, _) => { result = true; dialog.Close(); };
            cancelButton.Click += (_, _) => { result = false; dialog.Close(); };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            panel.Children.Add(buttonPanel);

            dialog.Content = panel;
            await dialog.ShowDialog(_owner);
        }).Wait();
        return result;
    }
}

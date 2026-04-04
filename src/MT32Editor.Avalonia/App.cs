using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;

namespace MT32Edit.Avalonia;

public class App : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Initialize platform services before any Core code runs
            var mainWindow = new WindowMainMenu();
            PlatformServices.Initialize(
                new AvaloniaPlatformNotification(mainWindow),
                new AvaloniaPlatformFileDialog(mainWindow),
                new AvaloniaPlatformScreenInfo(mainWindow),
                new AvaloniaPlatformSysExLoadForm(mainWindow)
            );

            desktop.MainWindow = mainWindow;
            mainWindow.Initialize(desktop.Args ?? Array.Empty<string>());
        }

        base.OnFrameworkInitializationCompleted();
    }
}

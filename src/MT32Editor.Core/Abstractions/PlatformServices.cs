namespace MT32Edit;

/// <summary>
/// Central registry for platform-specific service implementations.
/// Must be initialized by the UI project before any service classes are used.
/// </summary>
public static class PlatformServices
{
    private static IUserNotification? _notification;
    private static IFileDialogProvider? _fileDialog;
    private static IScreenInfo? _screenInfo;
    private static ISysExLoadForm? _sysExLoadForm;

    /// <summary>
    /// Gets the user notification service.
    /// </summary>
    public static IUserNotification Notification
    {
        get => _notification ?? throw new InvalidOperationException("PlatformServices.Notification has not been initialized. Call PlatformServices.Initialize() from the UI project startup.");
        set => _notification = value;
    }

    /// <summary>
    /// Gets the file dialog provider.
    /// </summary>
    public static IFileDialogProvider FileDialog
    {
        get => _fileDialog ?? throw new InvalidOperationException("PlatformServices.FileDialog has not been initialized. Call PlatformServices.Initialize() from the UI project startup.");
        set => _fileDialog = value;
    }

    /// <summary>
    /// Gets the screen information provider.
    /// </summary>
    public static IScreenInfo ScreenInfo
    {
        get => _screenInfo ?? throw new InvalidOperationException("PlatformServices.ScreenInfo has not been initialized. Call PlatformServices.Initialize() from the UI project startup.");
        set => _screenInfo = value;
    }

    /// <summary>
    /// Gets the SysEx load form provider.
    /// </summary>
    public static ISysExLoadForm SysExLoadForm
    {
        get => _sysExLoadForm ?? throw new InvalidOperationException("PlatformServices.SysExLoadForm has not been initialized. Call PlatformServices.Initialize() from the UI project startup.");
        set => _sysExLoadForm = value;
    }

    /// <summary>
    /// Initializes all platform services at once.
    /// </summary>
    public static void Initialize(IUserNotification notification, IFileDialogProvider fileDialog, IScreenInfo screenInfo, ISysExLoadForm sysExLoadForm)
    {
        _notification = notification;
        _fileDialog = fileDialog;
        _screenInfo = screenInfo;
        _sysExLoadForm = sysExLoadForm;
    }
}

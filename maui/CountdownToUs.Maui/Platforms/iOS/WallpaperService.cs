namespace CountdownToUs.Maui;

/// <summary>
/// iOS does not allow apps to set the system wallpaper programmatically.
/// This stub keeps the DI graph valid; the UI hides wallpaper controls on iOS.
/// </summary>
public class WallpaperService : Services.IWallpaperService
{
    public bool IsSupported => false;

    public Task SetWallpaperAsync(byte[] pngBytes) => Task.CompletedTask;
}

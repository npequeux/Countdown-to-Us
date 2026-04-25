namespace CountdownToUs.Maui;

/// <summary>
/// Mac Catalyst does not expose a public API to set the system wallpaper.
/// This stub keeps the DI graph valid; the UI hides wallpaper controls on Mac.
/// </summary>
public class WallpaperService : Services.IWallpaperService
{
    public bool IsSupported => false;

    public Task SetWallpaperAsync(byte[] pngBytes) => Task.CompletedTask;
}

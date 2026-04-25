using Android.App;

namespace CountdownToUs.Maui;

/// <summary>Android implementation: uses WallpaperManager to set the system wallpaper.</summary>
public class WallpaperService : Services.IWallpaperService
{
    public bool IsSupported => true;

    public Task SetWallpaperAsync(byte[] pngBytes)
    {
        return Task.Run(() =>
        {
            var context = Android.App.Application.Context;
            var manager = WallpaperManager.GetInstance(context)
                ?? throw new InvalidOperationException("WallpaperManager is unavailable.");

            using var stream = new MemoryStream(pngBytes);
            manager.SetStream(stream);
        });
    }
}

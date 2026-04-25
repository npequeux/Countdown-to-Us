using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace CountdownToUs.Maui;

/// <summary>Windows implementation: uses SystemParametersInfo to set the desktop wallpaper.</summary>
public class WallpaperService : Services.IWallpaperService
{
    public bool IsSupported => true;

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool SystemParametersInfo(
        uint uiAction, uint uiParam, string pvParam, uint fWinIni);

    private const uint SpiSetDeskWallpaper = 0x0014;
    private const uint SpifUpdateIniFile   = 0x0001;
    private const uint SpifSendChange      = 0x0002;

    public async Task SetWallpaperAsync(byte[] pngBytes)
    {
        var path = Path.Combine(Path.GetTempPath(), "countdown_wallpaper.png");
        await File.WriteAllBytesAsync(path, pngBytes);

        // Set wallpaper display style to "Fill" (scale to cover, centered) so the image
        // is always correctly centered regardless of the user's previous wallpaper settings.
        using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", writable: true))
        {
            if (key != null)
            {
                key.SetValue("WallpaperStyle", "10", RegistryValueKind.String); // 10 = Fill (cover)
                key.SetValue("TileWallpaper",  "0",  RegistryValueKind.String); // 0  = No tiling
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    "[WallpaperService] Could not open 'Control Panel\\Desktop' registry key; " +
                    "wallpaper display style will not be updated.");
            }
        }

        bool ok = SystemParametersInfo(
            SpiSetDeskWallpaper, 0, path, SpifUpdateIniFile | SpifSendChange);

        if (!ok)
            throw new InvalidOperationException(
                $"SystemParametersInfo failed (error {Marshal.GetLastWin32Error()}).");
    }
}

using System.Runtime.InteropServices;

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

        bool ok = SystemParametersInfo(
            SpiSetDeskWallpaper, 0, path, SpifUpdateIniFile | SpifSendChange);

        if (!ok)
            throw new InvalidOperationException(
                $"SystemParametersInfo failed (error {Marshal.GetLastWin32Error()}).");
    }
}

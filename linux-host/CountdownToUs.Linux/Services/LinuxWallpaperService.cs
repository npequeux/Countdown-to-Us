namespace CountdownToUs.Linux.Services;

/// <summary>
/// Sets the Linux desktop wallpaper by writing a PNG to a temp file and
/// invoking the appropriate desktop-environment command.
/// Supported: GNOME (gsettings), KDE Plasma (plasma-apply-wallpaperimage),
/// XFCE (xfconf-query), and a generic fallback using feh/nitrogen/xwallpaper.
/// </summary>
public class LinuxWallpaperService
{
    private readonly ILogger<LinuxWallpaperService> _logger;

    public LinuxWallpaperService(ILogger<LinuxWallpaperService> logger)
    {
        _logger = logger;
    }

    public async Task SetWallpaperAsync(byte[] pngBytes)
    {
        var path = Path.Combine(Path.GetTempPath(), "countdown_wallpaper.png");
        await File.WriteAllBytesAsync(path, pngBytes);
        _logger.LogDebug("Wallpaper image written to {Path}", path);

        if (await TryGnomeAsync(path)) return;
        if (await TryKdePlasmaAsync(path)) return;
        if (await TryXfceAsync(path)) return;
        if (await TryFehAsync(path)) return;
        if (await TryNitrogenAsync(path)) return;
        if (await TryXwallpaperAsync(path)) return;

        _logger.LogWarning(
            "Could not detect a supported desktop environment to set the wallpaper. " +
            "The generated image is available at {Path}.", path);
    }

    // ─── Desktop-environment helpers ────────────────────────────────────────────

    private async Task<bool> TryGnomeAsync(string path)
    {
        var uri = $"file://{path}";
        bool ok =
            await RunAsync("gsettings", $"set org.gnome.desktop.background picture-uri \"{uri}\"") &&
            // Also update the dark-mode variant (GNOME ≥ 42)
            await RunAsync("gsettings", $"set org.gnome.desktop.background picture-uri-dark \"{uri}\"") &&
            // Ensure the image fills the screen (cover / zoom), centered
            await RunAsync("gsettings", "set org.gnome.desktop.background picture-options 'zoom'");
        if (ok) _logger.LogInformation("Wallpaper set via GNOME gsettings.");
        return ok;
    }

    private async Task<bool> TryKdePlasmaAsync(string path)
    {
        bool ok = await RunAsync("plasma-apply-wallpaperimage", $"\"{path}\"");
        if (ok) _logger.LogInformation("Wallpaper set via KDE plasma-apply-wallpaperimage.");
        return ok;
    }

    private async Task<bool> TryXfceAsync(string path)
    {
        // Try the most common XFCE property path; screen/monitor indices may vary.
        bool ok =
            await RunAsync("xfconf-query",
                $"-c xfce4-desktop -p /backdrop/screen0/monitor0/workspace0/last-image -s \"{path}\"") &&
            // Set image style to "Zoomed" (5 = cover/fill, centered) so the wallpaper fills the screen
            await RunAsync("xfconf-query",
                "-c xfce4-desktop -p /backdrop/screen0/monitor0/workspace0/image-style -s 5");
        if (ok) _logger.LogInformation("Wallpaper set via XFCE xfconf-query.");
        return ok;
    }

    private async Task<bool> TryFehAsync(string path)
    {
        // --bg-fill scales to cover the screen while preserving aspect ratio (centered crop)
        bool ok = await RunAsync("feh", $"--bg-fill \"{path}\"");
        if (ok) _logger.LogInformation("Wallpaper set via feh.");
        return ok;
    }

    private async Task<bool> TryNitrogenAsync(string path)
    {
        // --set-zoom scales to fill the screen (cover/centered crop)
        bool ok = await RunAsync("nitrogen", $"--set-zoom \"{path}\"");
        if (ok) _logger.LogInformation("Wallpaper set via nitrogen.");
        return ok;
    }

    private async Task<bool> TryXwallpaperAsync(string path)
    {
        bool ok = await RunAsync("xwallpaper", $"--zoom \"{path}\"");
        if (ok) _logger.LogInformation("Wallpaper set via xwallpaper.");
        return ok;
    }

    // ─── Process helper ─────────────────────────────────────────────────────────

    private async Task<bool> RunAsync(string command, string arguments)
    {
        try
        {
            using var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName               = command,
                    Arguments              = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true,
                }
            };

            process.Start();
            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Command '{Command}' not available or failed.", command);
            return false;
        }
    }
}

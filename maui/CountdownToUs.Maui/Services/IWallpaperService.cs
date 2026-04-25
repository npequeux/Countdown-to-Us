namespace CountdownToUs.Maui.Services;

/// <summary>Represents the data needed to render a countdown wallpaper image.</summary>
public record WallpaperData(
    DateTime TargetDate,
    int TotalDays,
    int Years,
    int Months,
    int Days,
    int Hours,
    int Minutes,
    int Seconds,
    DateTime CurrentTime,
    byte[]? BackgroundImageBytes,
    System.Globalization.CultureInfo Culture);

/// <summary>Platform service for setting the system desktop wallpaper.</summary>
public interface IWallpaperService
{
    /// <summary>Gets whether the current platform supports setting the system wallpaper.</summary>
    bool IsSupported { get; }

    /// <summary>Sets the system desktop wallpaper to the given PNG image bytes.</summary>
    Task SetWallpaperAsync(byte[] pngBytes);
}

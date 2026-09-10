using System.Globalization;

namespace CountdownToUs.Maui.Services;

/// <summary>
/// Persists widget-specific settings in app-private storage so both the MAUI app
/// and the Android widget provider can share them.
/// </summary>
internal static class WidgetSettingsStore
{
    internal static Task SaveTargetDateAsync(DateTime targetDate)
    {
#if ANDROID
        return File.WriteAllTextAsync(
            GetTargetDatePath(),
            targetDate.ToString("O", CultureInfo.InvariantCulture));
#else
        return Task.CompletedTask;
#endif
    }

    internal static DateTime GetTargetDate(DateTime fallback)
    {
#if ANDROID
        try
        {
            var path = GetTargetDatePath();
            if (File.Exists(path) &&
                DateTime.TryParse(
                    File.ReadAllText(path),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var storedTargetDate))
            {
                return storedTargetDate;
            }
        }
        catch
        {
        }
#endif

        return fallback;
    }

#if ANDROID
    private const string TargetDateFileName = "widget_target_date.txt";

    private static string GetTargetDatePath()
        => Path.Combine(
            Android.App.Application.Context.FilesDir!.AbsolutePath,
            TargetDateFileName);
#endif
}

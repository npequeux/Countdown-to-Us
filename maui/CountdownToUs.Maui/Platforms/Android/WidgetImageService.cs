namespace CountdownToUs.Maui;

/// <summary>
/// Android implementation: persists the current slideshow photo to the app's
/// internal files directory so the home-screen widget can read it.
/// </summary>
public class WidgetImageService : Services.IWidgetImageService
{
    internal const string FileName = "widget_photo.jpg";

    /// <summary>Absolute path to the file shared with the widget provider.</summary>
    internal static string GetFilePath()
        => Path.Combine(
            Android.App.Application.Context.FilesDir!.AbsolutePath,
            FileName);

    public Task SaveImageAsync(byte[] imageBytes)
        => Task.Run(() => File.WriteAllBytes(GetFilePath(), imageBytes));

    public Task ClearImageAsync()
        => Task.Run(() =>
        {
            var path = GetFilePath();
            if (File.Exists(path))
                File.Delete(path);
        });
}

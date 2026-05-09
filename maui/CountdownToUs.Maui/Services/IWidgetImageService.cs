namespace CountdownToUs.Maui.Services;

/// <summary>
/// Platform service for persisting the current slideshow photo so that the
/// Android home-screen widget can display it.  Non-Android platforms use a
/// no-op stub so call sites need no conditional compilation.
/// </summary>
public interface IWidgetImageService
{
    /// <summary>
    /// Saves <paramref name="imageBytes"/> as the widget photo.
    /// The widget will display this image on its next update.
    /// </summary>
    Task SaveImageAsync(byte[] imageBytes);

    /// <summary>
    /// Removes the previously saved widget photo.
    /// The widget will display no photo on its next update.
    /// </summary>
    Task ClearImageAsync();
}

namespace CountdownToUs.Maui;

/// <summary>MacCatalyst does not support Android-style home-screen widgets; this stub keeps the DI graph valid.</summary>
public class WidgetImageService : Services.IWidgetImageService
{
    public Task SaveImageAsync(byte[] imageBytes) => Task.CompletedTask;
    public Task ClearImageAsync() => Task.CompletedTask;
}

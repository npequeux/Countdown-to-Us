using SkiaSharp;

namespace CountdownToUs.Linux.Services;

/// <summary>
/// Countdown data required to render the wallpaper image.
/// </summary>
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
    byte[]? BackgroundImageBytes);

/// <summary>
/// Configuration for the wallpaper update service, bound from appsettings.json.
/// </summary>
public record WallpaperConfig
{
    public DateTime TargetDate            { get; init; } = new DateTime(2028, 10, 1);
    public int      UpdateIntervalMinutes { get; init; } = 1;
    public string?  BackgroundImagePath   { get; init; }
}

/// <summary>
/// Renders a 1920×1080 countdown wallpaper PNG using SkiaSharp.
/// </summary>
public static class WallpaperGenerator
{
    private const int Width  = 1920;
    private const int Height = 1080;

    public static byte[] GeneratePng(WallpaperData data)
    {
        var info = new SKImageInfo(Width, Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;

        DrawBackground(canvas, data.BackgroundImageBytes);
        DrawOverlay(canvas);
        DrawContent(canvas, data);

        using var image   = surface.Snapshot();
        using var encoded = image.Encode(SKEncodedImageFormat.Png, 95);
        return encoded.ToArray();
    }

    // ─── Background ─────────────────────────────────────────────────────────────

    private static void DrawBackground(SKCanvas canvas, byte[]? imageBytes)
    {
        if (imageBytes is { Length: > 0 })
        {
            try
            {
                using var bitmap = SKBitmap.Decode(imageBytes);
                if (bitmap != null)
                {
                    DrawCoverFitBitmap(canvas, bitmap);
                    return;
                }
            }
            catch { /* fall through */ }
        }

        using var shader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(Width, Height),
            new[] { new SKColor(10, 61, 92), new SKColor(4, 15, 35) },
            null,
            SKShaderTileMode.Clamp);
        using var paint = new SKPaint { Shader = shader };
        canvas.DrawRect(0, 0, Width, Height, paint);
    }

    private static void DrawCoverFitBitmap(SKCanvas canvas, SKBitmap bitmap)
    {
        float scaleX = (float)Width  / bitmap.Width;
        float scaleY = (float)Height / bitmap.Height;
        float scale  = Math.Max(scaleX, scaleY);

        float srcW = Width  / scale;
        float srcH = Height / scale;
        float srcX = (bitmap.Width  - srcW) / 2f;
        float srcY = (bitmap.Height - srcH) / 2f;

        canvas.DrawBitmap(bitmap,
            new SKRect(srcX, srcY, srcX + srcW, srcY + srcH),
            new SKRect(0, 0, Width, Height));
    }

    // ─── Overlay ────────────────────────────────────────────────────────────────

    private static void DrawOverlay(SKCanvas canvas)
    {
        using var paint = new SKPaint { Color = new SKColor(0, 0, 0, 115) };
        canvas.DrawRect(0, 0, Width, Height, paint);
    }

    // ─── Content ────────────────────────────────────────────────────────────────

    private static void DrawContent(SKCanvas canvas, WallpaperData data)
    {
        float cx = Width / 2f;

        // Title
        DrawCenteredText(canvas, "Countdown to Us", cx, 105f, 68f, SKColors.White, bold: false);

        // Separator line
        DrawHLine(canvas, cx - 380f, cx + 380f, 135f, new SKColor(255, 255, 255, 80), 1.5f);

        // Total days
        DrawCenteredText(canvas, data.TotalDays.ToString(), cx, 340f, 210f, SKColors.White, bold: true);
        DrawCenteredText(canvas, "days", cx, 400f, 50f, new SKColor(255, 255, 255, 200), bold: false);

        string breakdown =
            $"{data.Years} {Pluralise(data.Years, "year")}  •  " +
            $"{data.Months} {Pluralise(data.Months, "month")}  •  " +
            $"{data.Days} {Pluralise(data.Days, "day")}";
        DrawCenteredText(canvas, breakdown, cx, 460f, 34f, new SKColor(255, 255, 255, 180), bold: false);

        // Separator line
        DrawHLine(canvas, cx - 340f, cx + 340f, 493f, new SKColor(255, 255, 255, 60), 1f);

        // Hours / Minutes / Seconds columns
        float col1 = cx - 340f;
        float col2 = cx;
        float col3 = cx + 340f;

        DrawCenteredText(canvas, data.Hours.ToString("D2"),   col1, 605f, 110f, SKColors.White, bold: true);
        DrawCenteredText(canvas, data.Minutes.ToString("D2"), col2, 605f, 110f, SKColors.White, bold: true);
        DrawCenteredText(canvas, data.Seconds.ToString("D2"), col3, 605f, 110f, SKColors.White, bold: true);

        DrawCenteredText(canvas, "Hours",   col1, 650f, 30f, new SKColor(255, 255, 255, 170), bold: false);
        DrawCenteredText(canvas, "Minutes", col2, 650f, 30f, new SKColor(255, 255, 255, 170), bold: false);
        DrawCenteredText(canvas, "Seconds", col3, 650f, 30f, new SKColor(255, 255, 255, 170), bold: false);

        // Colon separators
        DrawCenteredText(canvas, ":", cx - 170f, 595f, 70f, new SKColor(255, 255, 255, 120), bold: false);
        DrawCenteredText(canvas, ":", cx + 170f, 595f, 70f, new SKColor(255, 255, 255, 120), bold: false);

        // Target date
        DrawCenteredText(canvas, $"Until {data.TargetDate:MMMM d, yyyy}", cx, 730f, 36f, new SKColor(255, 255, 255, 210), bold: false);

        // Current datetime (bottom)
        string nowDisplay = data.CurrentTime.ToString("dddd, MMMM d, yyyy  •  HH:mm");
        DrawCenteredText(canvas, nowDisplay, cx, 1038f, 28f, new SKColor(255, 255, 255, 140), bold: false);
    }

    // ─── Helpers ────────────────────────────────────────────────────────────────

    private static void DrawCenteredText(
        SKCanvas canvas, string text, float cx, float y,
        float textSize, SKColor color, bool bold)
    {
        using var font = new SKFont(SKTypeface.Default, textSize)
        {
            Embolden = bold,
        };
        using var paint = new SKPaint
        {
            Color       = color,
            IsAntialias = true,
        };
        canvas.DrawText(text, cx, y, SKTextAlign.Center, font, paint);
    }

    private static void DrawHLine(
        SKCanvas canvas, float x1, float x2, float y, SKColor color, float strokeWidth)
    {
        using var paint = new SKPaint
        {
            Color       = color,
            StrokeWidth = strokeWidth,
            Style       = SKPaintStyle.Stroke,
        };
        canvas.DrawLine(x1, y, x2, y, paint);
    }

    private static string Pluralise(int value, string singular)
        => value == 1 ? singular : singular + "s";
}

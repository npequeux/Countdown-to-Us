using SkiaSharp;

namespace CountdownToUs.Maui.Services;

/// <summary>
/// Renders a countdown wallpaper PNG using SkiaSharp.
/// Defaults to 1920×1080 (landscape desktop); pass portrait dimensions for mobile.
/// </summary>
public static class WallpaperGenerator
{
    private const int DefaultWidth  = 1920;
    private const int DefaultHeight = 1080;

    public static byte[] GeneratePng(WallpaperData data, int width = DefaultWidth, int height = DefaultHeight)
    {
        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;

        DrawBackground(canvas, data.BackgroundImageBytes, width, height);
        DrawOverlay(canvas, width, height);
        DrawContent(canvas, data, width, height);

        using var image   = surface.Snapshot();
        using var encoded = image.Encode(SKEncodedImageFormat.Png, 95);
        return encoded.ToArray();
    }

    // ─── Background ─────────────────────────────────────────────────────────────

    private static void DrawBackground(SKCanvas canvas, byte[]? imageBytes, int width, int height)
    {
        if (imageBytes is { Length: > 0 })
        {
            try
            {
                using var bitmap = SKBitmap.Decode(imageBytes);
                if (bitmap != null)
                {
                    DrawCoverFitBitmap(canvas, bitmap, width, height);
                    return;
                }
            }
            catch
            {
                // Image bytes may be corrupt or an unsupported format; fall through to the
                // gradient background so wallpaper generation always produces a valid image.
            }
        }

        using var shader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(width, height),
            new[] { new SKColor(10, 61, 92), new SKColor(4, 15, 35) },
            null,
            SKShaderTileMode.Clamp);
        using var gradientPaint = new SKPaint { Shader = shader };
        canvas.DrawRect(0, 0, width, height, gradientPaint);
    }

    private static void DrawCoverFitBitmap(SKCanvas canvas, SKBitmap bitmap, int width, int height)
    {
        float scaleX = (float)width  / bitmap.Width;
        float scaleY = (float)height / bitmap.Height;
        float scale  = Math.Max(scaleX, scaleY);

        float srcW = width  / scale;
        float srcH = height / scale;
        float srcX = (bitmap.Width  - srcW) / 2f;
        float srcY = (bitmap.Height - srcH) / 2f;

        canvas.DrawBitmap(bitmap,
            new SKRect(srcX, srcY, srcX + srcW, srcY + srcH),
            new SKRect(0, 0, width, height));
    }

    // ─── Semi-transparent overlay ───────────────────────────────────────────────

    private static void DrawOverlay(SKCanvas canvas, int width, int height)
    {
        using var paint = new SKPaint { Color = new SKColor(0, 0, 0, 115) };
        canvas.DrawRect(0, 0, width, height, paint);
    }

    // ─── Content ───────────────────────────────────────────────────────────────

    private static void DrawContent(SKCanvas canvas, WallpaperData data, int width, int height)
    {
        // Scale all coordinates relative to the 1920×1080 reference layout.
        float cx     = width / 2f;
        float scaleX = width  / (float)DefaultWidth;
        float scaleY = height / (float)DefaultHeight;
        // Font sizes are constrained by the narrower dimension so text always fits.
        float fs     = Math.Min(scaleX, scaleY);

        // Title
        DrawCenteredText(canvas, "Countdown to Us", cx, 105f * scaleY, 68f * fs, SKColors.White, bold: false);

        // Separator line
        DrawHLine(canvas, cx - 380f * scaleX, cx + 380f * scaleX, 135f * scaleY, new SKColor(255, 255, 255, 80), 1.5f);

        // Total days (giant)
        DrawCenteredText(canvas, data.TotalDays.ToString(), cx, 340f * scaleY, 210f * fs, SKColors.White, bold: true);
        DrawCenteredText(canvas, "days", cx, 400f * scaleY, 50f * fs, new SKColor(255, 255, 255, 200), bold: false);

        // Years/Months/Days breakdown
        string breakdown =
            $"{data.Years} {Pluralise(data.Years, "year")}  •  " +
            $"{data.Months} {Pluralise(data.Months, "month")}  •  " +
            $"{data.Days} {Pluralise(data.Days, "day")}";
        DrawCenteredText(canvas, breakdown, cx, 460f * scaleY, 34f * fs, new SKColor(255, 255, 255, 180), bold: false);

        // Separator line
        DrawHLine(canvas, cx - 340f * scaleX, cx + 340f * scaleX, 493f * scaleY, new SKColor(255, 255, 255, 60), 1f);

        // Hours / Minutes / Seconds columns
        float col1 = cx - 340f * scaleX;
        float col2 = cx;
        float col3 = cx + 340f * scaleX;

        DrawCenteredText(canvas, data.Hours.ToString("D2"),   col1, 605f * scaleY, 110f * fs, SKColors.White, bold: true);
        DrawCenteredText(canvas, data.Minutes.ToString("D2"), col2, 605f * scaleY, 110f * fs, SKColors.White, bold: true);
        DrawCenteredText(canvas, data.Seconds.ToString("D2"), col3, 605f * scaleY, 110f * fs, SKColors.White, bold: true);

        DrawCenteredText(canvas, "Hours",   col1, 650f * scaleY, 30f * fs, new SKColor(255, 255, 255, 170), bold: false);
        DrawCenteredText(canvas, "Minutes", col2, 650f * scaleY, 30f * fs, new SKColor(255, 255, 255, 170), bold: false);
        DrawCenteredText(canvas, "Seconds", col3, 650f * scaleY, 30f * fs, new SKColor(255, 255, 255, 170), bold: false);

        // Colon separators
        DrawCenteredText(canvas, ":", cx - 170f * scaleX, 595f * scaleY, 70f * fs, new SKColor(255, 255, 255, 120), bold: false);
        DrawCenteredText(canvas, ":", cx + 170f * scaleX, 595f * scaleY, 70f * fs, new SKColor(255, 255, 255, 120), bold: false);

        // Target date
        DrawCenteredText(canvas,
            $"Until {data.TargetDate.ToString("MMMM d, yyyy", data.Culture)}",
            cx, 730f * scaleY, 36f * fs, new SKColor(255, 255, 255, 210), bold: false);

        // Current date + time (bottom)
        string nowDisplay = data.CurrentTime.ToString("dddd, MMMM d, yyyy  •  HH:mm", data.Culture);
        DrawCenteredText(canvas, nowDisplay, cx, 1038f * scaleY, 28f * fs, new SKColor(255, 255, 255, 140), bold: false);
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

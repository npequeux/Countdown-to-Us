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
/// The visual design matches the app's glassmorphism UI: a centred frosted-glass
/// card over a blurred background, with the countdown units displayed in four
/// columns (Days / Hours / Minutes / Seconds).
/// </summary>
public static class WallpaperGenerator
{
    private const int Width  = 1920;
    private const int Height = 1080;

    // ─── Card geometry ───────────────────────────────────────────────────────────
    // Mirrors the app's .container CSS: border-radius 20px (scaled), backdrop-filter blur,
    // background rgba(255,255,255,0.10), border rgba(255,255,255,0.18).

    private const float CardWidth     = 1200f;
    private const float CardHeight    = 560f;
    private const float CardCornerRad = 40f;   // ≈ 20px at 2× screen density
    private const float CardPaddingX  = 60f;
    private const float CardPaddingY  = 60f;

    public static byte[] GeneratePng(WallpaperData data)
    {
        var info = new SKImageInfo(Width, Height, SKColorType.Rgba8888, SKAlphaType.Premul);

        // Render only the background (no overlay) to a separate surface so we can
        // use it for the card's backdrop-blur simulation.
        using var bgSurface = SKSurface.Create(info);
        DrawBackground(bgSurface.Canvas, data.BackgroundImageBytes);
        using var bgImage = bgSurface.Snapshot();

        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;

        // 1. Background image / gradient
        canvas.DrawImage(bgImage, 0, 0);

        // 2. Full-screen dark overlay  — rgba(0,0,0,0.45) → alpha 115
        DrawOverlay(canvas);

        // 3. Centred glassmorphism card
        float cardX = (Width  - CardWidth)  / 2f;
        float cardY = (Height - CardHeight) / 2f;
        var cardRect = new SKRect(cardX, cardY, cardX + CardWidth, cardY + CardHeight);
        var rrect    = new SKRoundRect(cardRect, CardCornerRad, CardCornerRad);

        DrawCardBackdropBlur(canvas, bgImage, rrect);
        DrawCardBackground(canvas, rrect);
        DrawCardContent(canvas, data, cardX, cardY);

        // 4. Current time at the very bottom of the wallpaper (outside the card)
        string nowDisplay = data.CurrentTime.ToString("dddd, MMMM d, yyyy  •  HH:mm");
        DrawCenteredText(canvas, nowDisplay, Width / 2f, Height - 22f, 26f,
            new SKColor(255, 255, 255, 140), bold: false);

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
            catch
            {
                // Image bytes may be corrupt or an unsupported format; fall through to the
                // gradient background so wallpaper generation always produces a valid image.
            }
        }

        // Fallback gradient — matches the app's body background colour (#0a3d5c)
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
        // Matches body::before { background: rgba(0,0,0,0.45) }  → alpha ≈ 115
        using var paint = new SKPaint { Color = new SKColor(0, 0, 0, 115) };
        canvas.DrawRect(0, 0, Width, Height, paint);
    }

    // ─── Card ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Simulates CSS backdrop-filter: blur(10px) by drawing a blurred copy of the
    /// background image clipped to the card shape.
    /// </summary>
    private static void DrawCardBackdropBlur(SKCanvas canvas, SKImage bgImage, SKRoundRect rrect)
    {
        canvas.Save();
        canvas.ClipRoundRect(rrect, antialias: true);

        // Blur the background image inside the card area
        using var blurFilter = SKImageFilter.CreateBlur(20f, 20f);
        using var blurPaint  = new SKPaint { ImageFilter = blurFilter };
        canvas.DrawImage(bgImage, 0, 0, blurPaint);

        // Re-apply the dark overlay so the brightness inside the card stays consistent
        using var overlayPaint = new SKPaint { Color = new SKColor(0, 0, 0, 115) };
        canvas.DrawRect(0, 0, Width, Height, overlayPaint);

        canvas.Restore();
    }

    private static void DrawCardBackground(SKCanvas canvas, SKRoundRect rrect)
    {
        // Soft drop shadow — mirrors box-shadow: 0 8px 32px 0 rgba(0,0,0,0.37)
        using var shadowPaint = new SKPaint
        {
            Color       = new SKColor(0, 0, 0, 94),  // 0.37 × 255
            MaskFilter  = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 32f),
            IsAntialias = true,
        };
        var shadowRect = new SKRoundRect(
            new SKRect(rrect.Rect.Left, rrect.Rect.Top + 8f, rrect.Rect.Right, rrect.Rect.Bottom + 8f),
            CardCornerRad, CardCornerRad);
        canvas.DrawRoundRect(shadowRect, shadowPaint);

        // Card fill — rgba(255,255,255,0.10) → alpha 26
        using var fillPaint = new SKPaint
        {
            Color       = new SKColor(255, 255, 255, 26),
            IsAntialias = true,
        };
        canvas.DrawRoundRect(rrect, fillPaint);

        // Card border — 1px solid rgba(255,255,255,0.18) → alpha 46  (2px at wallpaper res)
        using var borderPaint = new SKPaint
        {
            Color       = new SKColor(255, 255, 255, 46),
            Style       = SKPaintStyle.Stroke,
            StrokeWidth = 2f,
            IsAntialias = true,
        };
        canvas.DrawRoundRect(rrect, borderPaint);
    }

    // ─── Card content ────────────────────────────────────────────────────────────
    // Layout mirrors the app's HTML/CSS structure:
    //   h1  →  countdown row (4 columns)  →  days-breakdown  →  target-date

    private static void DrawCardContent(SKCanvas canvas, WallpaperData data, float cardX, float cardY)
    {
        float cx = cardX + CardWidth / 2f;   // horizontal centre of card (= Width/2)
        float y  = cardY + CardPaddingY;

        // ── Title ── (h1: font-size 2.5rem, text-shadow)
        string title = $"Countdown to {data.TargetDate.ToString("MMMM d, yyyy")}";
        const float TitleSize = 60f;
        DrawCenteredText(canvas, title, cx, y + TitleSize, TitleSize,
            SKColors.White, bold: false, textShadow: true);
        y += TitleSize + 40f;

        // Thin separator — mirrors the app's subtle divider styling
        DrawHLine(canvas, cx - 480f, cx + 480f, y, new SKColor(255, 255, 255, 80), 1.5f);
        y += 30f;

        // ── Countdown row ── (.countdown: 4 flex columns with gap)
        // Column centres spaced 275px apart (total spread = 3×275 = 825px, centred in 1200px card)
        const float ColSpacing = 275f;
        float col1 = cx - 1.5f * ColSpacing;   // Days
        float col2 = cx - 0.5f * ColSpacing;   // Hours
        float col3 = cx + 0.5f * ColSpacing;   // Minutes
        float col4 = cx + 1.5f * ColSpacing;   // Seconds

        // Large numbers — .number { font-size: 4rem; font-weight: bold; text-shadow }
        const float NumSize = 110f;
        DrawCenteredText(canvas, data.TotalDays.ToString(),        col1, y + NumSize, NumSize, SKColors.White, bold: true,  textShadow: true);
        DrawCenteredText(canvas, data.Hours.ToString("D2"),        col2, y + NumSize, NumSize, SKColors.White, bold: true,  textShadow: true);
        DrawCenteredText(canvas, data.Minutes.ToString("D2"),      col3, y + NumSize, NumSize, SKColors.White, bold: true,  textShadow: true);
        DrawCenteredText(canvas, data.Seconds.ToString("D2"),      col4, y + NumSize, NumSize, SKColors.White, bold: true,  textShadow: true);
        y += NumSize + 16f;

        // Labels — .label { text-transform: uppercase; opacity: 0.8 }  → alpha 204
        const float LabelSize = 32f;
        var labelColor = new SKColor(255, 255, 255, 204);
        DrawCenteredText(canvas, "DAYS",    col1, y + LabelSize, LabelSize, labelColor, bold: false);
        DrawCenteredText(canvas, "HOURS",   col2, y + LabelSize, LabelSize, labelColor, bold: false);
        DrawCenteredText(canvas, "MINUTES", col3, y + LabelSize, LabelSize, labelColor, bold: false);
        DrawCenteredText(canvas, "SECONDS", col4, y + LabelSize, LabelSize, labelColor, bold: false);
        y += LabelSize + 40f;

        // Thin separator
        DrawHLine(canvas, cx - 440f, cx + 440f, y, new SKColor(255, 255, 255, 60), 1f);
        y += 30f;

        // ── Days breakdown ── (.days-breakdown: font-size 0.9rem; opacity 0.75 → alpha 191)
        string breakdown =
            $"{data.Years} {Pluralise(data.Years, "year")}, " +
            $"{data.Months} {Pluralise(data.Months, "month")}, " +
            $"{data.Days} {Pluralise(data.Days, "day")}";
        const float BreakdownSize = 30f;
        DrawCenteredText(canvas, breakdown, cx, y + BreakdownSize, BreakdownSize,
            new SKColor(255, 255, 255, 191), bold: false);
        y += BreakdownSize + 20f;

        // ── Target date ── (.target-date: font-size 1.1rem; opacity 0.9 → alpha 230)
        string targetDisplay = $"Target Date: {data.TargetDate.ToString("MMMM d, yyyy")}";
        const float TargetSize = 34f;
        DrawCenteredText(canvas, targetDisplay, cx, y + TargetSize, TargetSize,
            new SKColor(255, 255, 255, 230), bold: false);
    }

    // ─── Helpers ────────────────────────────────────────────────────────────────

    private static void DrawCenteredText(
        SKCanvas canvas, string text, float cx, float y,
        float textSize, SKColor color, bool bold, bool textShadow = false)
    {
        using var font = new SKFont(SKTypeface.Default, textSize)
        {
            Embolden = bold,
        };

        if (textShadow)
        {
            // text-shadow: 2px 2px 4px rgba(0,0,0,0.3)
            using var shadowPaint = new SKPaint
            {
                Color       = new SKColor(0, 0, 0, 77),   // 0.3 × 255
                IsAntialias = true,
            };
            canvas.DrawText(text, cx + 2f, y + 2f, SKTextAlign.Center, font, shadowPaint);
        }

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

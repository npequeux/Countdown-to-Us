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

    /// <summary>
    /// Extra multiplier applied to font sizes when rendering in portrait orientation.
    /// The reference layout is landscape (1920×1080), so the raw font scale factor
    /// (width/1920) would be only ~0.56 on a 1080-wide phone – too small to read
    /// comfortably as a screensaver.  Multiplying by 1.5 brings it up to ~0.84,
    /// producing clearly legible text on portrait phone displays.
    /// </summary>
    private const float PortraitFontScale = 1.5f;

    // Card colours — mirror the glassmorphism theme of the web app.
    private static readonly SKColor CardFillColor   = new(10, 50, 80, 204);   // rgba(10,50,80,0.80)
    private static readonly SKColor CardBorderColor = new(255, 255, 255, 46); // rgba(255,255,255,0.18)

    /// <summary>Precomputed layout parameters derived from canvas dimensions.</summary>
    private readonly record struct WallpaperLayout(
        bool  IsPortrait,
        float Cx,
        float Fs,
        float ScaleX,
        float ScaleY,
        float Scale,
        float CardX,
        float CardY,
        float CardW,
        float CardH,
        float CardRadius,
        float ContentX,
        float ContentW,
        float InnerPadY);

    private static WallpaperLayout ComputeLayout(int width, int height)
    {
        bool  isPortrait = height > width;
        float cx         = width / 2f;

        float fsRaw = Math.Min((float)width / DefaultWidth, (float)height / DefaultHeight);
        float fs    = isPortrait ? fsRaw * PortraitFontScale : fsRaw;

        float scaleX = (float)width  / DefaultWidth;
        float scaleY = (float)height / DefaultHeight;
        float scale  = Math.Min(scaleX, scaleY);

        float cardMarginX = (isPortrait ? 54f : 200f) * scaleX;
        float cardMarginY = (isPortrait ? 152f : 54f) * scaleY;
        float cardX       = cardMarginX;
        float cardY       = cardMarginY;
        float cardW       = width  - 2f * cardMarginX;
        float cardH       = height - 2f * cardMarginY;
        float cardRadius  = 40f * scale;

        float innerPad = 32f * scale;
        float contentX = cardX + innerPad;
        float contentW = cardW - 2f * innerPad;
        float innerPadY = cardY + innerPad + 20f * scaleY;

        return new WallpaperLayout(
            IsPortrait: isPortrait,
            Cx:         cx,
            Fs:         fs,
            ScaleX:     scaleX,
            ScaleY:     scaleY,
            Scale:      scale,
            CardX:      cardX,
            CardY:      cardY,
            CardW:      cardW,
            CardH:      cardH,
            CardRadius: cardRadius,
            ContentX:   contentX,
            ContentW:   contentW,
            InnerPadY:  innerPadY);
    }

    public static byte[] GeneratePng(WallpaperData data, int width = DefaultWidth, int height = DefaultHeight)
    {
        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;

        DrawBackground(canvas, data.BackgroundImageBytes, width, height);
        DrawOverlay(canvas, width, height);
        var countdownY = DrawStaticContent(canvas, data, width, height);
        DrawDynamicContent(canvas, data, width, height, countdownY);

        using var image   = surface.Snapshot();
        using var encoded = image.Encode(SKEncodedImageFormat.Png, 95);
        return encoded.ToArray();
    }

    /// <summary>
    /// Renders the background, overlay, and card — everything except the countdown
    /// numbers that change every hour.  Store the returned bytes and pass them to
    /// <see cref="UpdateCountdownPng"/> on each tick so the expensive background
    /// decode only happens once per slideshow image change.
    /// </summary>
    public static byte[] GenerateBackground(WallpaperData data, int width = DefaultWidth, int height = DefaultHeight)
    {
        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;

        DrawBackground(canvas, data.BackgroundImageBytes, width, height);
        DrawOverlay(canvas, width, height);
        DrawStaticContent(canvas, data, width, height);

        using var image   = surface.Snapshot();
        using var encoded = image.Encode(SKEncodedImageFormat.Png, 95);
        return encoded.ToArray();
    }

    /// <summary>
    /// Composites fresh countdown numbers on top of a pre-rendered background PNG
    /// produced by <see cref="GenerateBackground"/>.  This avoids re-decoding the
    /// background image and re-drawing the static card content on every tick.
    /// </summary>
    public static byte[] UpdateCountdownPng(byte[] backgroundPng, WallpaperData data, int width = DefaultWidth, int height = DefaultHeight)
    {
        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;

        using var bgBitmap = SKBitmap.Decode(backgroundPng);
        if (bgBitmap != null)
            canvas.DrawBitmap(bgBitmap, 0, 0);

        var countdownY = ComputeCountdownY(data, width, height);
        DrawDynamicContent(canvas, data, width, height, countdownY);

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
        => DrawCoverFitBitmap(canvas, bitmap, 0, 0, width, height);

    private static void DrawCoverFitBitmap(
        SKCanvas canvas, SKBitmap bitmap,
        float destX, float destY, float destW, float destH)
    {
        float scaleX = destW / bitmap.Width;
        float scaleY = destH / bitmap.Height;
        float scale  = Math.Max(scaleX, scaleY);

        float srcW = destW / scale;
        float srcH = destH / scale;
        float srcX = (bitmap.Width  - srcW) / 2f;
        float srcY = (bitmap.Height - srcH) / 2f;

        canvas.DrawBitmap(bitmap,
            new SKRect(srcX, srcY, srcX + srcW, srcY + srcH),
            new SKRect(destX, destY, destX + destW, destY + destH));
    }

    // ─── Semi-transparent overlay ───────────────────────────────────────────────

    private static void DrawOverlay(SKCanvas canvas, int width, int height)
    {
        using var paint = new SKPaint { Color = new SKColor(0, 0, 0, 115) };
        canvas.DrawRect(0, 0, width, height, paint);
    }

    // ─── Content ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Draws the static portions of the wallpaper (card) and returns
    /// the Y coordinate at which the dynamic countdown content should begin.
    /// </summary>
    private static float DrawStaticContent(SKCanvas canvas, WallpaperData data, int width, int height)
    {
        var L = ComputeLayout(width, height);
        DrawCard(canvas, L.CardX, L.CardY, L.CardW, L.CardH, L.CardRadius);
        return L.InnerPadY;
    }

    /// <summary>
    /// Computes the Y coordinate where the dynamic countdown content begins.
    /// Uses the same layout logic as <see cref="DrawStaticContent"/> but without
    /// touching the canvas.  Use this when compositing onto a cached background.
    /// </summary>
    private static float ComputeCountdownY(WallpaperData data, int width, int height)
    {
        var L = ComputeLayout(width, height);
        return L.InnerPadY;
    }

    /// <summary>
    /// Draws the dynamic countdown content (years, months, days, hours) starting
    /// at <paramref name="y"/>.
    /// </summary>
    private static void DrawDynamicContent(SKCanvas canvas, WallpaperData data, int width, int height, float y)
    {
        var L = ComputeLayout(width, height);

        // Countdown row: YEARS | MONTHS | DAYS | HOURS
        float numSize = (L.IsPortrait ? 110f : 90f) * L.Fs;
        float lblSize = 28f * L.Fs;
        float colW    = L.ContentW / 4f;

        string[] nums = { data.Years.ToString(), data.Months.ToString(), data.Days.ToString(), data.Hours.ToString() };
        string[] lbls = { "YEARS", "MONTHS", "DAYS", "HOURS" };

        for (int i = 0; i < 4; i++)
        {
            float colCx = L.ContentX + colW * (i + 0.5f);
            DrawCenteredText(canvas, nums[i], colCx, y + numSize, numSize, SKColors.White, bold: true);
            DrawCenteredText(canvas, lbls[i], colCx, y + numSize + lblSize + 8f * L.ScaleY, lblSize,
                new SKColor(255, 255, 255, 204), bold: false);
        }
    }

    // ─── Card ───────────────────────────────────────────────────────────────────

    private static void DrawCard(SKCanvas canvas, float x, float y, float w, float h, float radius)
    {
        var rect  = new SKRect(x, y, x + w, y + h);
        var rrect = new SKRoundRect(rect, radius);

        // Semi-transparent dark-blue fill (simulates the glassmorphism card background
        // without requiring CSS backdrop-filter; keeps text readable over any photo).
        using var fillPaint = new SKPaint
        {
            Color       = CardFillColor,
            Style       = SKPaintStyle.Fill,
            IsAntialias = true,
        };
        canvas.DrawRoundRect(rrect, fillPaint);

        // Subtle white border matching rgba(255,255,255,0.18) of the web card.
        using var borderPaint = new SKPaint
        {
            Color       = CardBorderColor,
            Style       = SKPaintStyle.Stroke,
            StrokeWidth = 2f,
            IsAntialias = true,
        };
        canvas.DrawRoundRect(rrect, borderPaint);
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
}

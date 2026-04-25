namespace CountdownToUs.Linux.Services;

/// <summary>
/// Background service that periodically regenerates the countdown wallpaper
/// and sets it as the desktop background via <see cref="LinuxWallpaperService"/>.
/// </summary>
public class WallpaperUpdateHostedService : BackgroundService
{
    private readonly WallpaperConfig _config;
    private readonly LinuxWallpaperService _wallpaperService;
    private readonly ILogger<WallpaperUpdateHostedService> _logger;

    public WallpaperUpdateHostedService(
        WallpaperConfig config,
        LinuxWallpaperService wallpaperService,
        ILogger<WallpaperUpdateHostedService> logger)
    {
        _config          = config;
        _wallpaperService = wallpaperService;
        _logger          = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Wallpaper update service started. Interval: {Interval} min, target: {Target}.",
            _config.UpdateIntervalMinutes, _config.TargetDate);

        // Set wallpaper immediately on startup
        await UpdateWallpaperAsync();

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_config.UpdateIntervalMinutes));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await UpdateWallpaperAsync();
        }
    }

    private async Task UpdateWallpaperAsync()
    {
        try
        {
            var now      = DateTime.Now;
            var distance = _config.TargetDate - now;

            int totalDays = distance > TimeSpan.Zero ? (int)distance.TotalDays : 0;
            int years     = 0, months = 0, days = 0,
                hours = 0, minutes = 0, seconds = 0;

            if (distance > TimeSpan.Zero)
            {
                var startDate = now.Date;
                var endDate   = startDate.AddDays(distance.Days);

                years  = endDate.Year  - startDate.Year;
                months = endDate.Month - startDate.Month;
                days   = endDate.Day   - startDate.Day;

                if (days < 0)
                {
                    months--;
                    int borrowYear  = endDate.Month == 1 ? endDate.Year - 1 : endDate.Year;
                    int borrowMonth = endDate.Month == 1 ? 12 : endDate.Month - 1;
                    days += DateTime.DaysInMonth(borrowYear, borrowMonth);
                }
                if (months < 0) { years--; months += 12; }

                hours   = distance.Hours;
                minutes = distance.Minutes;
                seconds = distance.Seconds;
            }

            byte[]? bgBytes = null;
            if (!string.IsNullOrWhiteSpace(_config.BackgroundImagePath) &&
                File.Exists(_config.BackgroundImagePath))
            {
                bgBytes = await File.ReadAllBytesAsync(_config.BackgroundImagePath);
            }

            var data = new WallpaperData(
                TargetDate:          _config.TargetDate,
                TotalDays:           totalDays,
                Years:               years,
                Months:              months,
                Days:                days,
                Hours:               hours,
                Minutes:             minutes,
                Seconds:             seconds,
                CurrentTime:         now,
                BackgroundImageBytes: bgBytes);

            var png = WallpaperGenerator.GeneratePng(data);
            await _wallpaperService.SetWallpaperAsync(png);
            _logger.LogDebug("Wallpaper updated ({TotalDays} days remaining).", totalDays);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update wallpaper.");
        }
    }
}

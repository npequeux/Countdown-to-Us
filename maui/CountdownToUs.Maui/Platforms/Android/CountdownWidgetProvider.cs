using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Graphics;
using Android.Widget;
using System.Globalization;

namespace CountdownToUs.Maui;

[BroadcastReceiver(Label = "Countdown 4x2 Widget", Enabled = true, Exported = true)]
[IntentFilter(new[]
{
    AppWidgetManager.ActionAppwidgetUpdate,
    Intent.ActionTimeChanged,
    Intent.ActionTimezoneChanged,
    UpdateAction
})]
[MetaData("android.appwidget.provider", Resource = "@xml/countdown_widget_info")]
public class CountdownWidgetProvider : AppWidgetProvider
{
    private const string UpdateAction = "com.countdown.us.widget.UPDATE";
    private const string TargetDisplayFormat = "yyyy-MM-dd HH:mm";
    private static readonly DateTime TargetDate = DateTime.Parse(
        Constants.AppDefaults.TargetDateIso,
        CultureInfo.InvariantCulture);

    public override void OnEnabled(Context context)
    {
        base.OnEnabled(context);
        ScheduleUpdates(context);
    }

    public override void OnDisabled(Context context)
    {
        base.OnDisabled(context);
        CancelUpdates(context);
    }

    public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
    {
        base.OnUpdate(context, appWidgetManager, appWidgetIds);
        foreach (var appWidgetId in appWidgetIds)
        {
            UpdateWidget(context, appWidgetManager, appWidgetId);
        }

        ScheduleUpdates(context);
    }

    public override void OnReceive(Context context, Intent intent)
    {
        base.OnReceive(context, intent);

        if (intent.Action is not (UpdateAction or AppWidgetManager.ActionAppwidgetUpdate or Intent.ActionTimeChanged or Intent.ActionTimezoneChanged))
        {
            return;
        }

        var appWidgetManager = AppWidgetManager.GetInstance(context);
        var componentName = new ComponentName(context, Java.Lang.Class.FromType(typeof(CountdownWidgetProvider)));
        var appWidgetIds = appWidgetManager.GetAppWidgetIds(componentName);

        foreach (var appWidgetId in appWidgetIds)
        {
            UpdateWidget(context, appWidgetManager, appWidgetId);
        }

        if (appWidgetIds.Length > 0)
        {
            ScheduleUpdates(context);
        }
    }

    private static void UpdateWidget(Context context, AppWidgetManager appWidgetManager, int appWidgetId)
    {
        var now = DateTime.Now;
        var (years, months, days) = GetRemainingYearsMonthsDays(now, TargetDate);

        var views = new RemoteViews(context.PackageName, Resource.Layout.countdown_widget);
        views.SetTextViewText(Resource.Id.widget_years_value, years.ToString());
        views.SetTextViewText(Resource.Id.widget_months_value, months.ToString());
        views.SetTextViewText(Resource.Id.widget_days_value, days.ToString());
        views.SetTextViewText(Resource.Id.widget_target_date, $"Target: {TargetDate.ToString(TargetDisplayFormat, CultureInfo.InvariantCulture)}");

        // Show the saved photo thumbnail if available.
        var photoPath = WidgetImageService.GetFilePath();
        if (File.Exists(photoPath))
        {
            var bitmap = LoadScaledBitmap(photoPath);
            if (bitmap != null)
            {
                views.SetImageViewBitmap(Resource.Id.widget_image, bitmap);
                views.SetViewVisibility(Resource.Id.widget_image, Android.Views.ViewStates.Visible);
            }
            else
            {
                views.SetViewVisibility(Resource.Id.widget_image, Android.Views.ViewStates.Gone);
            }
        }
        else
        {
            views.SetViewVisibility(Resource.Id.widget_image, Android.Views.ViewStates.Gone);
        }

        var launchIntent = new Intent(context, typeof(MainActivity));
        launchIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);

        var launchPendingIntent = PendingIntent.GetActivity(
            context,
            0,
            launchIntent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        views.SetOnClickPendingIntent(Resource.Id.widget_root, launchPendingIntent);
        appWidgetManager.UpdateAppWidget(appWidgetId, views);
    }

    private static (int Years, int Months, int Days) GetRemainingYearsMonthsDays(DateTime now, DateTime targetDate)
    {
        var distance = targetDate - now;
        if (distance <= TimeSpan.Zero)
        {
            return (0, 0, 0);
        }

        var startDate = now.Date;
        var completeDays = (int)Math.Floor(distance.TotalDays);
        var endDate = startDate.AddDays(completeDays);

        int years = 0;
        var cursor = startDate;
        while (cursor.AddYears(1) <= endDate)
        {
            cursor = cursor.AddYears(1);
            years++;
        }

        int months = 0;
        while (cursor.AddMonths(1) <= endDate)
        {
            cursor = cursor.AddMonths(1);
            months++;
        }

        int days = (endDate - cursor).Days;

        return (years, months, days);
    }

    /// <summary>
    /// Decodes the image at <paramref name="filePath"/> scaled down so that its
    /// longest dimension does not exceed <paramref name="maxDimPx"/> pixels.
    /// Returns null if the file cannot be decoded.
    /// </summary>
    private static Bitmap? LoadScaledBitmap(string filePath, int maxDimPx = 300)
    {
        try
        {
            // First pass: read only the image dimensions.
            var sizeOpts = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeFile(filePath, sizeOpts);
            if (sizeOpts.OutWidth <= 0 || sizeOpts.OutHeight <= 0)
                return null;

            // Compute the largest power-of-two sample size that keeps the image
            // within maxDimPx on its longest side.
            int sampleSize = 1;
            int maxDim = Math.Max(sizeOpts.OutWidth, sizeOpts.OutHeight);
            while (maxDim / (sampleSize * 2) > maxDimPx)
                sampleSize *= 2;

            // Second pass: decode at the computed sample size.
            var opts = new BitmapFactory.Options { InSampleSize = sampleSize };
            return BitmapFactory.DecodeFile(filePath, opts);
        }
        catch
        {
            return null;
        }
    }

    private static void ScheduleUpdates(Context context)
    {
        var alarmManager = (AlarmManager?)context.GetSystemService(Context.AlarmService);
        if (alarmManager is null)
        {
            return;
        }

        var updateIntent = new Intent(context, typeof(CountdownWidgetProvider));
        updateIntent.SetAction(UpdateAction);

        var updatePendingIntent = PendingIntent.GetBroadcast(
            context,
            0,
            updateIntent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        alarmManager.SetInexactRepeating(
            AlarmType.Rtc,
            Java.Lang.JavaSystem.CurrentTimeMillis() + 60_000,
            60_000,
            updatePendingIntent);
    }

    private static void CancelUpdates(Context context)
    {
        var alarmManager = (AlarmManager?)context.GetSystemService(Context.AlarmService);
        if (alarmManager is null)
        {
            return;
        }

        var updateIntent = new Intent(context, typeof(CountdownWidgetProvider));
        updateIntent.SetAction(UpdateAction);

        var updatePendingIntent = PendingIntent.GetBroadcast(
            context,
            0,
            updateIntent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        alarmManager.Cancel(updatePendingIntent);
    }
}

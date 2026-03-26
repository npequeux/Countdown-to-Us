var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5000");

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("Countdown to Us is running at http://localhost:5000");
    Console.WriteLine("Press Ctrl+C to stop.");
    try
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "http://localhost:5000",
            UseShellExecute = true
        });
    }
    catch
    {
        // Browser could not be opened automatically; user can open it manually.
    }
});

app.Run();

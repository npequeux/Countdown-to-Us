# Countdown to Us — Blazor (.NET 10)

A .NET 10 Blazor WebAssembly port of the [Countdown to Us](../README.md) app.

## Features

- ⏱️ Real-time countdown (Days / Hours / Minutes / Seconds) updated every second
- �� Personal photo slideshow with auto-rotation every 5 seconds
- 🎨 Glassmorphism UI — matches the original design exactly
- ⚙️ Settings panel to change the target date/time and manage photos
- 💾 Persists settings and photos via browser `localStorage`
- 📱 Fully responsive (mobile & desktop)

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Run locally

```bash
cd blazor/CountdownToUs
dotnet run
```

Then open the URL shown in the terminal (e.g. `http://localhost:5000`).

## Build for production

```bash
cd blazor/CountdownToUs
dotnet publish -c Release -o publish
```

The static files are in `publish/wwwroot` and can be deployed to any static hosting provider (GitHub Pages, Azure Static Web Apps, Netlify, etc.).

# Countdown to Us — .NET MAUI Blazor Hybrid

A .NET MAUI Blazor Hybrid port of [Countdown to Us](../README.md), providing a native app for **Android**, **iOS**, **macOS** (Mac Catalyst), and **Windows** — entirely in C#.

This replaces the Capacitor/Java Android wrapper with a pure C# solution.

## Features

- ⏱️ Real-time countdown (Days / Hours / Minutes / Seconds) updated every second
- 🖼️ Personal photo slideshow with auto-rotation every 5 seconds
- 🎨 Glassmorphism UI — matches the original design exactly
- ⚙️ Settings panel to change the target date/time and manage photos
- 🌍 Multi-language support: English, Français, Español, 中文
- 💾 Persists settings and photos via browser `localStorage`
- 📱 Fully responsive (mobile & desktop)

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) with the MAUI workload:
  ```bash
  dotnet workload install maui
  ```
- For Android: Android SDK (via Android Studio or Visual Studio)
- For iOS/macOS: Xcode on macOS
- For Windows: Windows App SDK

## Run locally

```bash
# Android (requires connected device or emulator)
dotnet build -t:Run -f net10.0-android maui/CountdownToUs.Maui/CountdownToUs.Maui.csproj

# iOS (macOS only)
dotnet build -t:Run -f net10.0-ios maui/CountdownToUs.Maui/CountdownToUs.Maui.csproj

# Windows
dotnet build -t:Run -f net10.0-windows10.0.19041.0 maui/CountdownToUs.Maui/CountdownToUs.Maui.csproj
```

## Build for release

```bash
# Android APK
dotnet publish -f net10.0-android -c Release maui/CountdownToUs.Maui/CountdownToUs.Maui.csproj
```

## Project structure

| File | Description |
|------|-------------|
| `MauiProgram.cs` | App entry point; registers services |
| `App.xaml.cs` | Application class |
| `MainPage.xaml` | Single page hosting `<BlazorWebView>` |
| `Components/Home.razor` | Full countdown UI and logic |
| `Components/Routes.razor` | Blazor router |
| `wwwroot/index.html` | Host page for BlazorWebView |
| `wwwroot/css/app.css` | Glassmorphism styles |
| `Platforms/Android/MainActivity.cs` | Android entry point (**replaces `MainActivity.java`**) |
| `Platforms/Android/MainApplication.cs` | Android application class |
| `Platforms/iOS/AppDelegate.cs` | iOS entry point |
| `Platforms/MacCatalyst/AppDelegate.cs` | macOS entry point |
| `Platforms/Windows/App.xaml.cs` | Windows entry point |

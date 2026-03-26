# Countdown to Us

[![Blazor Build](https://github.com/npequeux/Countdown-to-Us/actions/workflows/blazor-build.yml/badge.svg)](https://github.com/npequeux/Countdown-to-Us/actions/workflows/blazor-build.yml)
[![MAUI Build](https://github.com/npequeux/Countdown-to-Us/actions/workflows/maui-build.yml/badge.svg)](https://github.com/npequeux/Countdown-to-Us/actions/workflows/maui-build.yml)

A beautiful countdown timer displaying the time remaining until October 1, 2028. Built entirely in C# with .NET 10, available as:

- A **web application** (Blazor WebAssembly)
- A native **Android app** (.NET MAUI)

## Features

- Real-time countdown display showing days, hours, minutes, and seconds
- Responsive design that works on desktop and mobile devices
- Modern glassmorphism UI with gradient background
- Updates every second for accurate time tracking
- Multi-language support (English, French, Spanish, Chinese)
- Customizable slideshow background with photo upload
- Configurable target date/time

## Project Structure

```
Countdown-to-Us/
├── blazor/
│   └── CountdownToUs/          # Blazor WebAssembly web app (.NET 10)
│       ├── Pages/Home.razor    # Main countdown component
│       ├── wwwroot/            # Static web assets (CSS, icons)
│       └── CountdownToUs.csproj
├── maui/
│   └── CountdownToUs.Maui/     # .NET MAUI cross-platform app
│       ├── Components/Home.razor  # Main countdown component
│       ├── wwwroot/            # Static assets
│       └── CountdownToUs.Maui.csproj
├── build/                      # App icons
├── docs/                       # Documentation
└── .github/workflows/          # CI/CD pipelines
```

## Prerequisites

### For Web Development (Blazor)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### For Android Development (.NET MAUI)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- .NET MAUI workload: `dotnet workload install maui-android`
- [Android Studio](https://developer.android.com/studio) (optional, for device emulator)

## How to Build

### Clone the Repository

```bash
git clone https://github.com/npequeux/Countdown-to-Us.git
cd Countdown-to-Us
```

### Build and Run the Web App (Blazor WebAssembly)

```bash
cd blazor/CountdownToUs
dotnet run
```

Then open `http://localhost:5000` in your browser.

To publish a production build:

```bash
dotnet publish -c Release -o publish
```

The published output in `publish/wwwroot` can be deployed to any static web host (GitHub Pages, Netlify, Azure Static Web Apps, etc.).

### Build the Android App (.NET MAUI)

Install the MAUI Android workload (first time only):

```bash
dotnet workload install maui-android
```

Build a debug APK:

```bash
cd maui/CountdownToUs.Maui
dotnet build -f net10.0-android -c Debug
```

Build a release APK:

```bash
dotnet publish -f net10.0-android -c Release \
  -p:AndroidPackageFormats=apk \
  -p:AndroidKeyStore=false
```

## Downloading Pre-built Packages

All platform packages are attached to every versioned release on the [Releases page](https://github.com/npequeux/Countdown-to-Us/releases):

- `countdown-to-us-blazor-*.zip` — Blazor WebAssembly web app (extract and serve statically)
- `countdown-to-us-android-*.apk` — Android APK (.NET MAUI)

## Creating a New Release

### Using Git Tags (Recommended)

```bash
git tag v1.2.0
git push origin v1.2.0
```

This triggers the release workflow that builds both the Blazor WASM zip and the MAUI Android APK, then creates a GitHub release.

### Manual Workflow Dispatch

1. Go to the **Actions** tab on GitHub
2. Select the **Release** workflow
3. Click **Run workflow** and enter the version number

## Versioning

This project follows [Semantic Versioning](https://semver.org/). When updating the version:

1. Update `ApplicationDisplayVersion` and `ApplicationVersion` in `maui/CountdownToUs.Maui/CountdownToUs.Maui.csproj`
2. Create and push a git tag (e.g., `git tag v1.2.0 && git push origin v1.2.0`)

## Target Date

The countdown is set to October 1, 2028 at 00:00:00 (midnight).

## License

ISC

## Contributing

Feel free to open issues or submit pull requests for improvements.

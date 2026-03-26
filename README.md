# Countdown to Us

[![Blazor Build](https://github.com/npequeux/Countdown-to-Us/actions/workflows/blazor-build.yml/badge.svg)](https://github.com/npequeux/Countdown-to-Us/actions/workflows/blazor-build.yml)
[![MAUI Build](https://github.com/npequeux/Countdown-to-Us/actions/workflows/maui-build.yml/badge.svg)](https://github.com/npequeux/Countdown-to-Us/actions/workflows/maui-build.yml)

A beautiful countdown timer displaying the time remaining until October 1, 2028. Built entirely in C# with .NET 10, available as:

- A **web application** (Blazor WebAssembly — runs in any browser, including on Linux)
- A native **Android app** (.NET MAUI)
- A native **Windows desktop app** (.NET MAUI) with installer
- A **Linux desktop app** (ASP.NET Core self-contained binary, opens in your browser)

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
│   └── CountdownToUs.Maui/     # .NET MAUI cross-platform app (Android + Windows)
│       ├── Components/Home.razor  # Main countdown component
│       ├── wwwroot/            # Static assets
│       └── CountdownToUs.Maui.csproj
├── linux-host/
│   └── CountdownToUs.Linux/    # ASP.NET Core self-hosted Linux app
│       ├── Program.cs          # Web server entry point
│       └── CountdownToUs.Linux.csproj
├── build/                      # App icons and installer scripts
│   ├── icon.ico
│   ├── icon.png
│   └── installer.iss           # Inno Setup script for Windows installer
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

### For Windows Development (.NET MAUI)
- Windows 10 version 1809 or higher
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- .NET MAUI workload: `dotnet workload install maui-windows`
- [Inno Setup 6](https://jrsoftware.org/isdl.php) (optional, for building the installer)

### For Linux Development
- Linux (x64)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

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

The published output in `publish/wwwroot` can be deployed to any static web host (GitHub Pages, Netlify, Azure Static Web Apps, etc.). The web app runs in any browser, including on Linux.

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

### Build the Windows Desktop App (.NET MAUI)

Must be run on a Windows machine. Install the MAUI Windows workload (first time only):

```powershell
dotnet workload install maui-windows
```

Build and publish:

```powershell
cd maui/CountdownToUs.Maui
dotnet publish -f net10.0-windows10.0.19041.0 -c Release `
  -p:WindowsPackageType=None `
  -p:SelfContained=true `
  -o windows-publish
```

Run `windows-publish\CountdownToUs.Maui.exe` directly — no installation required.

To build the installer (requires [Inno Setup 6](https://jrsoftware.org/isdl.php)):

```powershell
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" /DAppVersion=1.0.0 build\installer.iss
```

The installer will be created at `installer-output\countdown-to-us-setup-1.0.0.exe`.

### Build the Linux App

Build and publish a self-contained Linux binary:

```bash
# First, publish the Blazor WASM app
dotnet publish blazor/CountdownToUs/CountdownToUs.csproj -c Release -o blazor-publish

# Then publish the Linux host and add the Blazor files
dotnet publish linux-host/CountdownToUs.Linux/CountdownToUs.Linux.csproj \
  -c Release -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -o linux-publish
cp -r blazor-publish/wwwroot linux-publish/
```

Run it:

```bash
./linux-publish/CountdownToUs
```

The app starts a local web server and opens your browser at `http://localhost:5000`. Press Ctrl+C to stop.

## Downloading Pre-built Packages

All platform packages are attached to every versioned release on the [Releases page](https://github.com/npequeux/Countdown-to-Us/releases):

- `countdown-to-us-blazor-*.zip` — Blazor WebAssembly web app (extract and serve statically)
- `countdown-to-us-android-*.apk` — Android APK (.NET MAUI)
- `countdown-to-us-setup-*.exe` — Windows installer (recommended; creates Start Menu entry and uninstaller)
- `countdown-to-us-windows-*.zip` — Windows portable app (.NET MAUI, extract and run the `.exe`)
- `countdown-to-us-linux-*.tar.gz` — Linux self-contained app (x64, extract and run `./CountdownToUs`)
- `countdown-to-us-linux-*.deb` — Debian/Ubuntu package (x64, install with `sudo dpkg -i *.deb`)

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

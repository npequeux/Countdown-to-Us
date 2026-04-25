# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [v0.4] - Wallpaper Release

### Added
- **Real wallpaper app** — the countdown timer now sets the system desktop wallpaper across all environments
- **MAUI (Windows)**: "Set as Wallpaper" button renders a 1920×1080 PNG via SkiaSharp and applies it using the `SystemParametersInfo` Win32 API
- **MAUI (Android)**: Same button uses `WallpaperManager` to set the system home-screen/lock-screen wallpaper; requires `SET_WALLPAPER` permission (declared in `AndroidManifest.xml`)
- **Auto-update wallpaper** toggle (persisted in `localStorage`) — updates the wallpaper every minute so the countdown stays current
- **Linux host** (`linux-host/`): new `WallpaperUpdateHostedService` background service that renders the wallpaper with SkiaSharp and applies it automatically on startup and every minute; supports GNOME (`gsettings`), KDE (`plasma-apply-wallpaperimage`), XFCE (`xfconf-query`), and generic tools (`feh`, `nitrogen`, `xwallpaper`)
- `appsettings.json` for the Linux host — configure `TargetDate`, `UpdateIntervalMinutes`, and an optional `BackgroundImagePath`
- SkiaSharp 3.119.2 dependency added to both MAUI and Linux host projects
- iOS and Mac Catalyst stubs for `IWallpaperService` (wallpaper setting is not supported on those platforms; UI controls are hidden automatically)
- Full i18n for the new wallpaper settings group (EN / FR / ES / ZH)

### Wallpaper image layout (1920×1080)
- Cover-fit background image (current slideshow photo) or dark blue gradient fallback
- 45 % dark overlay for text contrast
- Title, giant total-days counter, years/months/days breakdown
- HH : MM : SS columns
- Target date and current date/time footer

## [v0.3] - Paradise Island Release

### Changed
- Replaced the default `us.jpg` picture with a rotating slideshow of 10 free-to-use paradise island photos (Unsplash license)
- Default photos are shuffled randomly on each page load for variety
- Background image now updates dynamically alongside the slideshow
- Navigation controls are always visible when using the default paradise island photos

### Removed
- Removed `us.jpg` as the default background/display image

## [v0.2] - Desktop Release

### Added
- Windows desktop application via Electron (NSIS installer + portable executable)
- Linux desktop application via Electron (AppImage + .deb package)
- `electron/main.js` — Electron main process entry point
- `npm run electron` script to run the app locally with Electron
- `npm run build:win` script to build Windows packages
- `npm run build:linux` script to build Linux packages
- `npm run build:desktop` script to build all desktop packages
- GitHub Actions workflow `desktop-build.yml` for CI desktop builds on every push
- Updated `release.yml` to build Android, Windows, and Linux packages in parallel and attach all artifacts to GitHub releases
- `electron-builder` configuration in `package.json` for NSIS installer, portable .exe, AppImage, and .deb

## [v0.1] - Initial Release

### Added
- Beautiful countdown timer to October 1, 2028
- Web application with responsive design
- Android APK support via Capacitor
- Glassmorphism UI with gradient background
- Real-time countdown updates every second
- GitHub Actions workflow for automated builds

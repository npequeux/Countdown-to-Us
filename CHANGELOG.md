# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

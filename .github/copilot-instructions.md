# Copilot Instructions for Countdown to Us

## Project Overview

**Countdown to Us** is a beautiful countdown timer displaying the time remaining until a configurable target date (default: October 1, 2028). It is available as:

- A **web application** (vanilla HTML/CSS/JavaScript)
- A native **Android app** (via [Capacitor](https://capacitorjs.com/))
- **Desktop apps** for Windows and Linux (via [Electron](https://www.electronjs.org/))

## Repository Structure

```
Countdown-to-Us/
├── .github/
│   └── workflows/          # CI/CD GitHub Actions workflows
├── android/                # Android native project (Capacitor-generated)
├── blazor/                 # Blazor web project
├── docs/                   # Documentation (APK signing, Play Store publishing)
├── electron/
│   └── main.js             # Electron main process (desktop entry point)
├── www/                    # Web source files synced to Android via Capacitor
│   ├── index.html
│   ├── countdown.js
│   └── style.css
├── index.html              # Root web app entry point
├── countdown.js            # Countdown logic and UI interactions (vanilla JS)
├── style.css               # Styles and responsive design (glassmorphism UI)
├── capacitor.config.json   # Capacitor configuration
└── package.json            # Node.js dependencies and npm scripts
```

## Technology Stack

- **Frontend**: Vanilla HTML5, CSS3, JavaScript (ES6+) — no framework
- **Android**: Capacitor (wraps the web app as a native Android app)
- **Desktop**: Electron (wraps the web app as a native desktop app)
- **CI/CD**: GitHub Actions

## Key Source Files

- `countdown.js` — All countdown logic, slideshow management, localStorage persistence, and settings panel event handlers
- `index.html` — Main HTML structure with the countdown display, settings panel, and slideshow
- `style.css` — Glassmorphism UI styling and responsive layout
- `electron/main.js` — Electron main process (desktop window configuration)

## Development Workflow

1. Edit `index.html`, `countdown.js`, and `style.css` in the root directory
2. Copy changes to the `www/` directory: `cp index.html countdown.js style.css www/`
3. Sync to Android: `npm run sync`
4. Test in browser by opening `index.html`

## npm Scripts

| Script | Description |
|---|---|
| `npm run sync` | Sync web assets to Android via Capacitor |
| `npm run build:android` | Build Android debug APK |
| `npm run build:android-release` | Build Android release APK |
| `npm run build:android-bundle` | Build Android App Bundle (AAB) for Google Play |
| `npm run open:android` | Open Android project in Android Studio |
| `npm run electron` | Run the app locally with Electron |
| `npm run build:win` | Build Windows desktop installer and portable |
| `npm run build:linux` | Build Linux AppImage and .deb package |
| `npm run build:desktop` | Build all desktop platforms |

## Testing

There is no automated test suite. To test:

- **Web app**: Open `index.html` in any modern browser, or serve with `python3 -m http.server 8000`
- **Android**: Use `npm run open:android` to open in Android Studio, or run `npm run build:android`
- **Desktop**: Use `npm run electron` to run locally

## Versioning

This project follows [Semantic Versioning](https://semver.org/). When updating the version:

1. Update `versionCode` and `versionName` in `android/app/build.gradle`
2. Update `version` in `package.json`
3. Create and push a git tag (e.g., `git tag v1.0.1 && git push origin v1.0.1`)

## Coding Conventions

- Vanilla JavaScript only — no frameworks or build tools for the web frontend
- Use `localStorage` for persisting user settings (target date, slideshow images)
- Keep changes to root-level files (`index.html`, `countdown.js`, `style.css`) in sync with the `www/` directory
- Maintain the glassmorphism UI aesthetic (backdrop-filter, semi-transparent backgrounds, gradient)
- Support both web and Android rendering — avoid browser-specific APIs not supported by Android WebView

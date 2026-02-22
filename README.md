# Countdown to Us

A beautiful countdown timer displaying the time remaining until October 1, 2028. Available as both a web application and a native Android app.

## Features

- Real-time countdown display showing days, hours, minutes, and seconds
- Responsive design that works on desktop and mobile devices
- Modern glassmorphism UI with gradient background
- Updates every second for accurate time tracking
- Available as an installable Android APK

## Prerequisites

### For Web Development
- A modern web browser (Chrome, Firefox, Safari, Edge)

### For Android Development
- [Node.js](https://nodejs.org/) (version 16 or higher)
- [npm](https://www.npmjs.com/) (comes with Node.js)
- [Java Development Kit (JDK)](https://www.oracle.com/java/technologies/downloads/) version 11 or higher
- [Android Studio](https://developer.android.com/studio) (for Android development)
- Android SDK with API level 22 or higher

## Installation

### Clone the Repository

```bash
git clone https://github.com/npequeux/Countdown-to-Us.git
cd Countdown-to-Us
```

### Install Dependencies

```bash
npm install
```

## How to Test

### Testing the Web App

The easiest way to test the web application:

1. **Direct browser access**: Simply open `index.html` in any modern web browser
2. **Using a local server** (recommended for more accurate testing):
   ```bash
   # Using Python 3
   python3 -m http.server 8000
   
   # Or using Node.js (if you have http-server installed)
   npx http-server . -p 8000
   ```
   Then open `http://localhost:8000` in your browser

### Testing the Android App

1. **Using Android Studio**:
   ```bash
   npm run open:android
   ```
   This opens the project in Android Studio where you can:
   - Run the app on an emulator
   - Run the app on a connected physical device
   - Debug the application

2. **Using the command line** (requires Android device connected or emulator running):
   ```bash
   cd android
   ./gradlew installDebug
   ```

## How to Compile

### Compiling for Web

The web application doesn't require compilation - it uses vanilla HTML, CSS, and JavaScript. However, if you make changes, you can sync them to the Android project:

```bash
npm run sync
```

### Compiling for Android

#### Debug Build (for testing)

```bash
npm run build:android
```

The APK will be generated at: `android/app/build/outputs/apk/debug/app-debug.apk`

#### Release Build (for distribution)

```bash
npm run build:android-release
```

The APK will be generated at: `android/app/build/outputs/apk/release/app-release-unsigned.apk`

**Note**: Release builds need to be signed before distribution. See [docs/APK_SIGNING.md](docs/APK_SIGNING.md) for a detailed guide on signing APKs for production.

#### Using Gradle Directly

You can also use Gradle commands directly:

```bash
cd android

# Debug build
./gradlew assembleDebug

# Release build
./gradlew assembleRelease

# Clean build artifacts
./gradlew clean
```

## Installing the APK on Android Devices

1. **Enable installation from unknown sources** on your Android device:
   - Go to Settings > Security
   - Enable "Unknown sources" or "Install unknown apps"

2. **Transfer the APK** to your device:
   - Via USB cable
   - Via cloud storage (Google Drive, Dropbox, etc.)
   - Via email
   - Download from GitHub Actions artifacts (see below)

3. **Install the APK**:
   - Open the APK file on your device
   - Follow the installation prompts

## Downloading Pre-built APKs

### From GitHub Releases (Recommended)

The easiest way to get the app is to download a pre-built APK from GitHub Releases:

1. Go to the [Releases page](https://github.com/npequeux/Countdown-to-Us/releases)
2. Download the latest `countdown-to-us-*.apk` file
3. Install it on your Android device

### From GitHub Actions Artifacts

Development builds are also available from GitHub Actions. The generated APK can be downloaded from the Actions tab in GitHub:

1. Go to the repository on GitHub
2. Click on the "Actions" tab
3. Select the latest workflow run
4. Download the APK from the "Artifacts" section

## Creating a New Release

To create a new release with an APK:

### Option 1: Using Git Tags (Recommended)

```bash
# Create and push a new version tag
git tag v1.0.1
git push origin v1.0.1
```

This will automatically trigger the release workflow that builds the APK and creates a GitHub release.

### Option 2: Manual Workflow Dispatch

1. Go to the "Actions" tab on GitHub
2. Select "Release APK" workflow
3. Click "Run workflow"
4. Enter the version number (e.g., v1.0.1)
5. Click "Run workflow"

The release workflow will:
- Build a release APK
- Create a GitHub release with the specified version
- Attach the APK to the release
- Make it available for download

## Versioning

This project follows [Semantic Versioning](https://semver.org/):
- **MAJOR version** (X.0.0): Incompatible changes
- **MINOR version** (1.X.0): New functionality in a backward compatible manner
- **PATCH version** (1.0.X): Backward compatible bug fixes

When updating the version:
1. Update `versionCode` and `versionName` in `android/app/build.gradle`
2. Update `version` in `package.json`
3. Create and push a git tag with the new version

## Project Structure

```
Countdown-to-Us/
├── www/                      # Web application source files
│   ├── index.html           # Main HTML structure
│   ├── countdown.js         # JavaScript countdown logic
│   └── style.css            # Styling and responsive design
├── android/                 # Android native project (generated)
├── capacitor.config.json    # Capacitor configuration
├── package.json            # Node.js dependencies and scripts
└── README.md               # This file
```

## Target Date

The countdown is set to October 1, 2028 at 00:00:00 (midnight).

## Development Workflow

1. Make changes to files in the root directory (`index.html`, `countdown.js`, `style.css`)
2. Copy changes to www directory: `cp index.html countdown.js style.css www/`
3. Test in browser by opening `index.html`
4. Sync changes to Android: `npm run sync`
5. Build Android app: `npm run build:android`
6. Test on Android device or emulator

## Troubleshooting

### Android Build Issues

- **Gradle build fails**: Ensure you have JDK 11 or higher installed and JAVA_HOME is set correctly
- **SDK not found**: Open the project in Android Studio at least once to download required SDKs
- **Out of memory**: Increase Gradle memory in `android/gradle.properties`

### Web App Issues

- **Countdown not updating**: Check browser console for JavaScript errors
- **Styling issues**: Clear browser cache and reload

## License

ISC

## Contributing

Feel free to open issues or submit pull requests for improvements.
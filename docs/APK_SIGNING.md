# APK Signing Guide

This guide explains how to sign your Android APK for production releases.

## Why Sign APKs?

- **Security**: Signing ensures the APK hasn't been tampered with
- **Google Play**: Required for publishing to Google Play Store
- **Updates**: Apps must be signed with the same key to be updated
- **Trust**: Users can verify the app's authenticity

## Generating a Keystore

### Option 1: Using Android Studio

1. Open the project in Android Studio
2. Go to **Build > Generate Signed Bundle / APK**
3. Select **APK** and click **Next**
4. Click **Create new...** under Key store path
5. Fill in the details:
   - Key store path: Choose a secure location
   - Password: Use a strong password
   - Alias: e.g., "countdown-to-us"
   - Validity: 25 years (recommended for app updates)
   - Certificate information: Fill in your details
6. Click **OK** and remember your passwords

### Option 2: Using Command Line

```bash
keytool -genkey -v -keystore countdown-to-us.keystore \
  -alias countdown-to-us -keyalg RSA -keysize 2048 -validity 10000
```

Follow the prompts to set passwords and certificate information.

**Important**: Store the keystore file and passwords securely! You'll need them for all future releases.

## Configuring Gradle for Signing

### Option 1: Local Signing (Development)

1. Create a `keystore.properties` file in the `android/` directory:

```properties
storeFile=path/to/countdown-to-us.keystore
storePassword=your_store_password
keyAlias=countdown-to-us
keyPassword=your_key_password
```

2. Add `keystore.properties` to `.gitignore`:

```bash
echo "android/keystore.properties" >> .gitignore
```

3. Update `android/app/build.gradle`:

```gradle
def keystorePropertiesFile = rootProject.file("keystore.properties")
def keystoreProperties = new Properties()
if (keystorePropertiesFile.exists()) {
    keystoreProperties.load(new FileInputStream(keystorePropertiesFile))
}

android {
    // ... existing config ...
    
    signingConfigs {
        release {
            if (keystorePropertiesFile.exists()) {
                keyAlias keystoreProperties['keyAlias']
                keyPassword keystoreProperties['keyPassword']
                storeFile file(keystoreProperties['storeFile'])
                storePassword keystoreProperties['storePassword']
            }
        }
    }
    
    buildTypes {
        release {
            signingConfig signingConfigs.release
            minifyEnabled false
            proguardFiles getDefaultProguardFile('proguard-android.txt'), 'proguard-rules.pro'
        }
    }
}
```

### Option 2: GitHub Actions Signing (CI/CD)

1. Encode your keystore in base64:

```bash
base64 -i countdown-to-us.keystore | tr -d '\n' > keystore.base64.txt
```

2. Add GitHub Secrets in your repository settings:
   - `KEYSTORE_BASE64`: Contents of `keystore.base64.txt`
   - `KEYSTORE_PASSWORD`: Your keystore password
   - `KEY_ALIAS`: Your key alias
   - `KEY_PASSWORD`: Your key password

3. Update `.github/workflows/release.yml` to decode and use the keystore:

```yaml
- name: Decode Keystore
  run: |
    echo "${{ secrets.KEYSTORE_BASE64 }}" | base64 --decode > android/app/countdown-to-us.keystore

- name: Create keystore properties
  run: |
    echo "storeFile=countdown-to-us.keystore" > android/keystore.properties
    echo "storePassword=${{ secrets.KEYSTORE_PASSWORD }}" >> android/keystore.properties
    echo "keyAlias=${{ secrets.KEY_ALIAS }}" >> android/keystore.properties
    echo "keyPassword=${{ secrets.KEY_PASSWORD }}" >> android/keystore.properties

- name: Build Signed Release APK
  run: |
    cd android
    ./gradlew assembleRelease --no-daemon
```

## Building a Signed APK

### Using Gradle

```bash
cd android
./gradlew assembleRelease
```

The signed APK will be at: `android/app/build/outputs/apk/release/app-release.apk`

### Using npm Script

Add to `package.json`:

```json
"scripts": {
  "build:android-release": "npx cap sync android && cd android && ./gradlew assembleRelease"
}
```

Then run:

```bash
npm run build:android-release
```

## Verifying the Signature

Check the signature of your APK:

```bash
jarsigner -verify -verbose -certs android/app/build/outputs/apk/release/app-release.apk
```

View certificate details:

```bash
keytool -printcert -jarfile android/app/build/outputs/apk/release/app-release.apk
```

## Security Best Practices

1. **Never commit keystore files**: Always add them to `.gitignore`
2. **Use strong passwords**: At least 8 characters with mixed case, numbers, and symbols
3. **Backup your keystore**: Store it in multiple secure locations
4. **Document key information**: Keep a record of:
   - Keystore location
   - Key alias
   - Certificate validity period
   - Creation date
5. **Restrict access**: Only trusted team members should have keystore access
6. **Use GitHub Secrets**: For CI/CD, always use encrypted secrets

## Troubleshooting

### "keystore file not found"
- Check the path in `keystore.properties`
- Ensure the file exists and is accessible

### "invalid keystore format"
- Ensure the keystore was created with `keytool`
- Check that the file isn't corrupted

### "incorrect keystore password"
- Verify passwords in `keystore.properties`
- Try generating a new keystore if forgotten

### "Failed to sign APK"
- Check Gradle build logs for specific errors
- Verify all signing config parameters are correct

## Publishing to Google Play

Once you have a signed APK:

1. Create a Google Play Developer account
2. Create a new application
3. Upload your signed APK
4. Fill in store listing details
5. Submit for review

For automated publishing, consider:
- [Google Play Publish Action](https://github.com/r0adkll/upload-google-play) - GitHub Action for publishing
- [Gradle Play Publisher](https://github.com/Triple-T/gradle-play-publisher) - Official Gradle plugin for Google Play publishing

## Additional Resources

- [Android App Signing Documentation](https://developer.android.com/studio/publish/app-signing)
- [Android Keystore System](https://developer.android.com/training/articles/keystore)
- [Google Play Publishing Guide](https://developer.android.com/distribute/console)

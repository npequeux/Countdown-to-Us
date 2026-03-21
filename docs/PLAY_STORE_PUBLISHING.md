# Publishing to Google Play Store

This guide walks you through everything needed to publish Countdown to Us on the Google Play Store, from account setup to automated CI/CD deployment.

## Prerequisites

- A [Google Play Developer account](https://play.google.com/console/signup) ($35 one-time registration fee)
- A production-signing keystore (see [APK_SIGNING.md](APK_SIGNING.md))
- [Android Studio](https://developer.android.com/studio) (optional, for local builds)
- [Node.js](https://nodejs.org/) 22+ and npm (for building locally)
- JDK 11 or higher

## Overview

Google Play now **requires Android App Bundles (AAB)** for new apps and updates. This project supports building both APKs (for direct installation) and AABs (for Play Store submission). The recommended upload format for the Play Store is `.aab`.

---

## Step 1 – Create a Production Keystore

Every app published to Google Play must be signed with a consistent production keystore.  
Follow the detailed steps in [APK_SIGNING.md](APK_SIGNING.md#generating-a-keystore) to generate your keystore.

> **Critical**: Back up your keystore and passwords securely. Losing them means you can never update your app on the Play Store.

---

## Step 2 – Configure Signing for Release Builds

### Local (Development Machine)

1. Create `android/keystore.properties` (this file is already in `.gitignore`):

   ```properties
   storeFile=/absolute/path/to/countdown-to-us.keystore
   storePassword=your_store_password
   keyAlias=countdown-to-us
   keyPassword=your_key_password
   ```

2. The `android/app/build.gradle` is already configured to read this file when it exists and use it for release builds.

### CI/CD (GitHub Actions)

Store credentials as **GitHub repository secrets**:

| Secret name        | Value                                                    |
|--------------------|----------------------------------------------------------|
| `KEYSTORE_BASE64`  | Base64-encoded keystore: `base64 -i your.keystore \| tr -d '\n'` |
| `KEYSTORE_PASSWORD`| Keystore password                                        |
| `KEY_ALIAS`        | Key alias (e.g. `countdown-to-us`)                       |
| `KEY_PASSWORD`     | Key password                                             |

To add secrets: **GitHub repo → Settings → Secrets and variables → Actions → New repository secret**.

---

## Step 3 – Build an Android App Bundle (AAB)

### Local Build

```bash
# Sync web assets and build a release AAB
npm run build:android-bundle
```

The AAB is generated at:  
`android/app/build/outputs/bundle/release/app-release.aab`

### CI/CD Build

Push a version tag (e.g. `v1.2.0`) or use the manual workflow dispatch in GitHub Actions to trigger the Play Store publishing workflow (see Step 6).

---

## Step 4 – Create Your Google Play App Listing

1. Open [Google Play Console](https://play.google.com/console)
2. Click **Create app**
3. Fill in:
   - **App name**: Countdown to Us
   - **Default language**: English (United States)
   - **App or Game**: App
   - **Free or Paid**: Free
4. Accept the declarations and click **Create app**

---

## Step 5 – Complete the Store Listing

Before you can publish, Google Play requires:

### App Content
- **Privacy policy URL** – required even for apps with no personal data collection
- **App access** – describe whether the app requires login
- **Ads** – declare whether the app contains ads (this app does not)
- **Content rating** – complete the rating questionnaire (this app is suitable for everyone)
- **Target audience** – specify the target age group

### Store Listing
- **Short description** (up to 80 characters):  
  `A beautiful countdown timer to your special date.`
- **Full description** (up to 4000 characters):  
  Describe the app, its features, and the countdown target date.
- **App icon** (512 × 512 px PNG)
- **Feature graphic** (1024 × 500 px PNG or JPG)
- **Screenshots** – at least 2 phone screenshots (minimum 320 px, maximum 3840 px per side)

### Release Track
Choose one of the following tracks for your first upload:
- **Internal testing** – up to 100 testers, instant availability (recommended for first upload)
- **Closed testing (alpha)** – limited testers
- **Open testing (beta)** – public beta
- **Production** – available to all users

---

## Step 6 – Publish Manually

1. Go to **Google Play Console → Your app → Release → Production** (or the desired track)
2. Click **Create new release**
3. Upload the signed `.aab` file from `android/app/build/outputs/bundle/release/app-release.aab`
4. Fill in the **Release name** (e.g. `1.1.0`) and **Release notes**
5. Click **Save**, then **Review release**, then **Start rollout to Production**

---

## Step 7 – Automate with GitHub Actions

This repository includes a workflow at `.github/workflows/play-store-publish.yml` that:

1. Builds a signed AAB using your production keystore stored as GitHub Secrets
2. Uploads the AAB to Google Play via the [upload-google-play](https://github.com/r0adkll/upload-google-play) action

### Additional Setup for the Automated Workflow

You need a **Google Play Service Account** with the *Release manager* role:

1. In Google Play Console, go to **Setup → API access**
2. Click **Link to a Google Cloud project** (or create a new one)
3. In Google Cloud Console, create a **Service Account**:
   - Go to **IAM & Admin → Service Accounts → Create Service Account**
   - Name it (e.g. `github-actions-play`)
   - Grant the role **Service Account User**
   - Click **Done**
4. Create a JSON key for the service account:
   - Click the service account → **Keys → Add Key → Create new key → JSON**
   - Download the `.json` key file
5. Back in Google Play Console, **Grant access** to the service account with the **Release manager** permission
6. Add the JSON key contents as a GitHub Secret named **`GOOGLE_PLAY_SERVICE_ACCOUNT_JSON`**

### Triggering the Workflow

Push a version tag to trigger an automatic Play Store release:

```bash
git tag v1.2.0
git push origin v1.2.0
```

Or trigger manually via **GitHub Actions → Play Store Publish → Run workflow**.

---

## Updating the App Version

Before releasing a new version, update the version in two places:

1. **`package.json`** – update the `version` field:
   ```json
   "version": "1.2.0"
   ```

2. **`android/app/build.gradle`** – increment `versionCode` and update `versionName`:
   ```gradle
   versionCode 2
   versionName "1.2.0"
   ```

> **Important**: `versionCode` must be a strictly increasing integer with every Play Store upload. `versionName` is the human-readable version shown to users.

---

## Troubleshooting

### "App not optimized" warning
If you upload an APK instead of an AAB, Google Play will show a warning. Use `npm run build:android-bundle` to build an AAB.

### "Upload failed – version code already used"
Increment `versionCode` in `android/app/build.gradle` before building.

### "Invalid signature"
Ensure you are signing with the same keystore used for the original upload. Google Play will reject updates signed with a different key.

### "Service account does not have permission"
In Google Play Console, go to **Users and permissions** and confirm the service account has at least the *Release manager* role for the app.

---

## Additional Resources

- [Google Play Console Help](https://support.google.com/googleplay/android-developer)
- [Android App Signing](https://developer.android.com/studio/publish/app-signing)
- [Android App Bundle format](https://developer.android.com/guide/app-bundle)
- [upload-google-play GitHub Action](https://github.com/r0adkll/upload-google-play)
- [APK Signing Guide](APK_SIGNING.md)

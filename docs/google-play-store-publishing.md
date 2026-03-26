# Publishing to the Google Play Store

This guide explains how to obtain every secret and token required to publish the **Countdown to Us** Android app to the Google Play Store, and how to store them safely as GitHub Actions secrets so the CI/CD pipeline can sign and upload the app automatically.

---

## Overview

Two independent sets of credentials are required:

| Credential | Purpose |
|---|---|
| **Android Keystore** | Signs the APK / AAB before upload |
| **Google Play Service Account JSON key** | Authenticates the CI/CD pipeline to the Google Play Developer API |

---

## Part 1 — Android Keystore

Every Android app published to Google Play must be signed with the same keystore for its entire lifetime. **If you lose the keystore you will not be able to release updates.**

### 1.1 Create a keystore

Run the following command on your local machine (requires the JDK — included with Android Studio):

```bash
keytool -genkey -v \
  -keystore countdown-to-us.jks \
  -alias countdown \
  -keyalg RSA \
  -keysize 2048 \
  -validity 10000
```

You will be prompted for:

- **Keystore password** — choose a strong password and save it somewhere secure
- **Key alias password** — can be the same as the keystore password
- **Distinguished name** (first/last name, organisation, city, country) — these values are embedded in the certificate

This creates a file called `countdown-to-us.jks`.

> **Never commit `countdown-to-us.jks` or either password to the repository.**

### 1.2 Base64-encode the keystore

GitHub Actions secrets must be plain text, so encode the binary keystore file:

```bash
base64 -w 0 countdown-to-us.jks > countdown-to-us.jks.b64
```

Copy the contents of `countdown-to-us.jks.b64` — you will paste it into a GitHub secret in the next step.

### 1.3 Add keystore secrets to GitHub Actions

Go to **GitHub → your repository → Settings → Secrets and variables → Actions → New repository secret** and create the following four secrets:

| Secret name | Value |
|---|---|
| `ANDROID_KEYSTORE_BASE64` | The base64 string from `countdown-to-us.jks.b64` |
| `ANDROID_KEYSTORE_PASSWORD` | The keystore password chosen during `keytool` |
| `ANDROID_KEY_ALIAS` | The alias used during `keytool` (e.g. `countdown`) |
| `ANDROID_KEY_PASSWORD` | The key alias password chosen during `keytool` |

---

## Part 2 — Google Play Service Account

The [Google Play Developer API](https://developers.google.com/android-publisher) allows CI/CD tools to upload APKs/AABs automatically. Access is granted through a **Google Cloud service account**.

### 2.1 Create (or open) a Google Play Developer account

If you do not already have one, register at [play.google.com/apps/publish](https://play.google.com/apps/publish). A one-time $25 registration fee applies.

### 2.2 Link a Google Cloud project

1. Open the [Google Play Console](https://play.google.com/console).
2. Go to **Setup → API access**.
3. Click **Link to a Google Cloud project** (or create a new Cloud project when prompted).
4. Follow the on-screen wizard to connect the Play Console to your Cloud project.

### 2.3 Create a service account

1. In the **API access** page of the Play Console, click **Create new service account**.
2. Follow the link to the **Google Cloud Console**.
3. In the Cloud Console, go to **IAM & Admin → Service Accounts → Create Service Account**:
   - **Name**: e.g. `countdown-github-actions`
   - **Description**: e.g. `Used by GitHub Actions to publish to Google Play`
4. Click **Create and continue**.
5. Skip the optional role assignment steps and click **Done**.

### 2.4 Create and download a JSON key

1. Still in the Cloud Console, open the service account you just created.
2. Go to the **Keys** tab → **Add Key → Create new key**.
3. Choose **JSON** and click **Create**.
4. A `.json` file is downloaded automatically — **keep it safe and never commit it**.

### 2.5 Grant the service account access in the Play Console

1. Return to the [Play Console → Setup → API access](https://play.google.com/console/developers/api-access).
2. Find the service account you created and click **Grant access**.
3. Under **App permissions**, add the app (`com.countdown.tous`) and grant at minimum:
   - **Release apps to testing tracks**
   - **Release apps to production** (when ready)
4. Click **Invite user** (or **Apply**) to save.

> Changes to service account permissions can take up to 24 hours to propagate.

### 2.6 Add the JSON key to GitHub Actions

1. Open the downloaded `.json` file and copy its entire contents.
2. In **GitHub → repository → Settings → Secrets and variables → Actions → New repository secret**, create:

| Secret name | Value |
|---|---|
| `GOOGLE_PLAY_SERVICE_ACCOUNT_JSON` | The full contents of the service account JSON file |

---

## Part 3 — Using the secrets in the CI/CD pipeline

The snippet below shows how to build a signed AAB and upload it to the **internal testing track** using the secrets defined above.
Add these steps to `.github/workflows/release.yml` (or a dedicated `play-store.yml` workflow) inside the `build-android` job, after the existing restore step:

```yaml
- name: Decode keystore
  run: |
    echo "${{ secrets.ANDROID_KEYSTORE_BASE64 }}" | base64 --decode \
      > ${{ github.workspace }}/countdown-to-us.jks

- name: Build signed Android App Bundle (AAB)
  run: |
    dotnet publish maui/CountdownToUs.Maui/CountdownToUs.Maui.csproj \
      -f net10.0-android -c Release --no-restore \
      -p:AndroidPackageFormats=aab \
      -p:AndroidKeyStore=true \
      -p:AndroidSigningKeyStore=${{ github.workspace }}/countdown-to-us.jks \
      -p:AndroidSigningKeyAlias=${{ secrets.ANDROID_KEY_ALIAS }} \
      -p:AndroidSigningKeyPass=${{ secrets.ANDROID_KEY_PASSWORD }} \
      -p:AndroidSigningStorePass=${{ secrets.ANDROID_KEYSTORE_PASSWORD }}

- name: Upload to Google Play (internal track)
  uses: r0adkll/upload-google-play@v1
  with:
    serviceAccountJsonPlainText: ${{ secrets.GOOGLE_PLAY_SERVICE_ACCOUNT_JSON }}
    packageName: com.countdown.tous
    releaseFiles: maui/CountdownToUs.Maui/bin/Release/net10.0-android/publish/*.aab
    track: internal
```

> Change `track: internal` to `alpha`, `beta`, or `production` when you are ready to promote the release.

---

## Part 4 — Secret checklist

Before running the pipeline, verify that all five secrets are present in **Settings → Secrets and variables → Actions**:

- [ ] `ANDROID_KEYSTORE_BASE64`
- [ ] `ANDROID_KEYSTORE_PASSWORD`
- [ ] `ANDROID_KEY_ALIAS`
- [ ] `ANDROID_KEY_PASSWORD`
- [ ] `GOOGLE_PLAY_SERVICE_ACCOUNT_JSON`

---

## References

- [Android developer — Sign your app](https://developer.android.com/studio/publish/app-signing)
- [Google Play Developer API — Getting started](https://developers.google.com/android-publisher/getting_started)
- [r0adkll/upload-google-play GitHub Action](https://github.com/r0adkll/upload-google-play)
- [.NET MAUI — Android signing](https://learn.microsoft.com/en-us/dotnet/maui/android/deployment/publish-cli)

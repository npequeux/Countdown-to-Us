# GitHub Secrets Setup Guide

This guide explains how to configure the GitHub repository secrets required to sign release builds and publish Countdown to Us to the Google Play Store via GitHub Actions.

Secrets are encrypted environment variables stored in your GitHub repository. They are never exposed in logs and are only available to authorized workflows.

---

## How to Add a Secret

1. Open your repository on GitHub
2. Go to **Settings → Secrets and variables → Actions**
3. Click **New repository secret**
4. Enter the secret **Name** and **Value** as described below
5. Click **Add secret**

---

## Android Signing Secrets

These four secrets are needed to sign release APKs and AABs with your production keystore. Without them, release builds fall back to the Android debug keystore (which cannot be published to the Play Store).

### 1. `KEYSTORE_BASE64`

A base64-encoded copy of your production keystore file.

**Step 1** – Generate a keystore (if you haven't already):

```bash
keytool -genkey -v -keystore countdown-to-us.keystore \
  -alias countdown-to-us -keyalg RSA -keysize 2048 -validity 10000
```

Follow the prompts to set passwords and certificate information.  
**Keep this file and its passwords safe — losing them means you can never update your app.**

**Step 2** – Base64-encode the keystore:

```bash
# macOS / Linux
base64 -i countdown-to-us.keystore | tr -d '\n'

# Windows (PowerShell)
[Convert]::ToBase64String([IO.File]::ReadAllBytes("countdown-to-us.keystore"))
```

Copy the entire output string and add it as the secret value.

---

### 2. `KEYSTORE_PASSWORD`

The password you chose for the keystore store when running `keytool` above.

---

### 3. `KEY_ALIAS`

The alias you chose for the key when running `keytool` (e.g. `countdown-to-us`).

---

### 4. `KEY_PASSWORD`

The password you chose for the individual key entry when running `keytool`.

---

## Google Play Secret

### 5. `GOOGLE_PLAY_SERVICE_ACCOUNT_JSON`

The JSON credentials of a Google Cloud service account that has been granted access to publish apps via the Google Play Developer API.

**Step 1** – Create a Google Cloud service account:

1. Open [Google Cloud Console](https://console.cloud.google.com/) and select (or create) a project
2. Enable the **Google Play Android Developer API** for the project
3. Go to **IAM & Admin → Service Accounts → Create Service Account**
4. Give it a name (e.g. `play-store-publish`) and click **Create and Continue**
5. Skip optional role assignment and click **Done**
6. Click on the new service account, then **Keys → Add Key → Create new key → JSON**
7. Download the generated `.json` file

**Step 2** – Grant the service account access to Play Console:

1. Open [Google Play Console](https://play.google.com/console)
2. Go to **Setup → API access**
3. Click **Link to a Google Cloud project** (if not already linked)
4. Under **Service accounts**, find the account you just created and click **Grant access**
5. In the permissions dialog, assign at minimum **Release manager** on the app you want to publish

**Step 3** – Add the secret:

Open the downloaded JSON file, copy its **entire contents** (the full JSON object), and add it as the secret value for `GOOGLE_PLAY_SERVICE_ACCOUNT_JSON`.

---

## Summary Table

| Secret name | Required for | Where the value comes from |
|---|---|---|
| `KEYSTORE_BASE64` | Release signing | `base64 -i your.keystore | tr -d '\n'` |
| `KEYSTORE_PASSWORD` | Release signing | Password set when creating the keystore |
| `KEY_ALIAS` | Release signing | Alias set when creating the keystore |
| `KEY_PASSWORD` | Release signing | Key password set when creating the keystore |
| `GOOGLE_PLAY_SERVICE_ACCOUNT_JSON` | Play Store publishing | Contents of the service account JSON file |

> `GITHUB_TOKEN` is provided automatically by GitHub Actions and does not need to be added manually.

---

## Verifying the Configuration

After adding the secrets, trigger a workflow run:

- **Release signing only**: Push a version tag (e.g. `git tag v1.0.0 && git push origin v1.0.0`) and check the **Auto Release on Merge to Master** or **Release** workflow logs for the message `Production keystore configured.`
- **Play Store publishing**: Run the **Play Store Publish** workflow manually from the **Actions** tab and check that the upload step completes successfully.

---

## Further Reading

- [APK_SIGNING.md](APK_SIGNING.md) — Detailed guide on keystore generation and local signing configuration
- [PLAY_STORE_PUBLISHING.md](PLAY_STORE_PUBLISHING.md) — Complete guide to setting up and publishing on the Google Play Store
- [GitHub encrypted secrets documentation](https://docs.github.com/en/actions/security-guides/encrypted-secrets)

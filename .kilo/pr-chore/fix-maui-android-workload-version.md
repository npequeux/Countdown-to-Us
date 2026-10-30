# Merge `chore/fix-maui-android-workload-version` into main

## Summary

Fixes an incorrect MAUI Android workload version specification in the CI build pipeline. The workflow was referencing version `10.0.20` instead of `v10.0.20`.

The fix changes `.github/workflows/maui-build.yml`:
- **Before:** `dotnet workload install maui-android --version 10.0.20`
- **After:** `dotnet workload install maui-android --version "v10.0.20"`

This ensures the workload installs correctly from the official NuGet feed (https://api.nuget.org/v3/index.json), which uses version identifiers prefixed with `v`.

## Changes

```diff
 .github/workflows/maui-build.yml | 2 +-
 1 file changed, 1 insertion(+), 1 deletion(-)
```

---

**Note:** Please review and merge this PR via the GitHub web UI. No CLI tool is available to create the PR programmatically. The changes have been pushed to `main` locally; once merged upstream, they will be reflected in the remote repository.
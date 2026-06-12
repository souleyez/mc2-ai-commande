Current Unity 6 mobile build smoke
==================================

This file is the repeatable checklist for the mobile-first gate. Android is the
first target. iOS stays behind Android proof because it needs macOS, Xcode,
Apple signing, and device provisioning.

Run these commands from the repository root:

```powershell
cd C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2
```

Prerequisite check
------------------

The Unity editor must have Android Build Support installed, including SDK, NDK,
and OpenJDK. Check the module folder:

```powershell
Test-Path "$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Data\PlaybackEngines\AndroidPlayer"
```

Expected:

```text
True
```

If this returns `False`, install Android Build Support for Unity `6000.4.7f1`
from Unity Hub before running the Android build command.
`MC2Demo.EditorTools.Mc2DemoBuilder.BuildAndroid` checks this folder before
rebuilding the scene, so a missing module should fail with a direct Android
Build Support message rather than scene churn or an ambiguous build failure.

Baseline validator
------------------

Before Android work, keep the Windows/BattleCore contract green:

```powershell
$Repo = (Get-Location).Path
$Unity = "$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe"
& $Unity `
  -batchmode -quit `
  -projectPath "$Repo\unity-mc2-demo" `
  -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract `
  -logFile "$Repo\analysis-output\unity-validate-mobile-baseline.log"
```

Expected success string:

```text
MC2 demo contract validation OK
```

Android build smoke
-------------------

Build the Android APK:

```powershell
$Repo = (Get-Location).Path
$Unity = "$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe"
& $Unity `
  -batchmode -quit `
  -projectPath "$Repo\unity-mc2-demo" `
  -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildAndroid `
  -logFile "$Repo\analysis-output\unity-build-android.log"
```

Expected success strings:

```text
Build Finished, Result: Success
MC2 Unity demo Android build OK
```

Expected output:

```text
unity-mc2-demo\Builds\Android\MC2UnityDemo.apk
```

The APK and all Unity build output stay under ignored folders. Do not stage
`unity-mc2-demo/Builds/`, logs, screenshots, JSON sidecars, or private reference
art.

Check that the ignored Android APK is newer than tracked Unity build inputs:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_freshness.ps1
```

Expected:

```text
Android APK freshness check OK
```

If this reports a stale APK, rebuild Android before running any G3 device smoke.

Check that the Android SDK tooling installed under Unity is still the expected
toolchain for this APK:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_sdk_tooling.ps1
```

Expected:

```text
Android SDK tooling check OK
```

This checks Unity's AndroidPlayer SDK, NDK, OpenJDK, `build-tools;36.0.0`,
`platforms;android-36`, adb, aapt and apksigner before install.

Check that the Android APK identity matches the expected package and launch
activity:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_identity.ps1
```

Expected:

```text
Android APK identity check OK
```

This requires package `com.DefaultCompany.unitymc2demo` and launch activity
`com.unity3d.player.UnityPlayerGameActivity`, matching the manual `adb shell am
start` command below.

Check that the Android APK compatibility metadata matches the expected device
target before install:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_compatibility.ps1
```

Expected:

```text
Android APK compatibility check OK
```

This requires `minSdkVersion` 25, `targetSdkVersion` 36, and native code ABI
`arm64-v8a`.

Check that the Android APK signing verifies before install:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_signing.ps1
```

Expected:

```text
Android APK signing check OK
```

This uses `apksigner verify --verbose --print-certs`, requires APK Signature
Scheme v2 verification, and confirms the current debug signer DN
`C=US, O=Android, CN=Android Debug`.

Check that the Android APK manifest keeps install targets broad enough for G3:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_manifest.ps1
```

Expected:

```text
Android APK manifest check OK
```

This checks the current permission allowlist, rejects required hardware
features, and confirms `small`, `normal`, `large`, and `xlarge` screen support.

Check that the Android APK contains the expected Unity/IL2CPP runtime payload:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_payload.ps1
```

Expected:

```text
Android APK payload check OK
```

This checks the required native libraries, Unity `assets/bin/Data` files,
single expected ABI folder, and enough data/native entries to catch truncated
or mispackaged APK output before install.

Check that the Android APK remains within the current early mobile demo size
budget:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_size_budget.ps1
```

Expected:

```text
Android APK size budget check OK
```

The current budget rejects implausibly small APKs and APKs over 100 MiB. This is
an install-readiness guard for accidental asset bloat, not the full G5 runtime
performance budget.

Check that Android smoke artifacts remain ignored and out of tracked/staged
source:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_artifact_hygiene.ps1
```

Expected:

```text
Android smoke artifact hygiene check OK
```

This guards ignored APK/AAB output and Android smoke logs, screenshots and
summary JSON before real G3 device runs.

Device smoke
------------

After the APK exists, first run the device-smoke preflight:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1
```

If the phone is not connected yet, use the waiting-state form:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
```

The strict form requires one authorized Android device. The waiting-state form
still checks the APK, Android SDK tooling, Android APK freshness, Android APK identity, Android APK
compatibility, Android APK signing, Android APK manifest, Android APK payload,
Android APK size budget, Android smoke artifact hygiene, Android smoke summary schema, adb, aapt, apksigner, package name and launchable
activity, then reports that G3 is waiting on a device.

Check that the device-smoke plan and preflight stay aligned before a phone is
connected:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_plan_consistency.ps1
```

Expected:

```text
Android smoke plan/preflight consistency check OK
```

Check the Android device connection state before real G3 smoke:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_connection.ps1
```

Expected without a phone:

```text
Android device connection check waiting on device
WpdOnlyAndroidProbe: True
AdbSetupHint: True
AdbWatchHint: True
```

If Windows sees a connected Android phone only as WPD/MTP, the same helper can
report `WpdOnlyAndroidDevice: True`. That is still a G3 waiting state until adb
shows exactly one authorized `device` row. The setup hint reports the current
Windows driver/provider/inf/service so the MTP-only state is distinguishable
from an ADB-ready phone.

To wait while changing phone USB debugging or driver state:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\watch_android_device_connection.ps1 -TimeoutSeconds 120
```

Check the Android smoke connection gate behavior before real G3 smoke:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_connection_gate.ps1
```

Expected without a phone:

```text
Android smoke connection gate check OK
Android smoke connection gate check waiting on device
```

Check the full G3 readiness bundle before installing:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_g3_readiness.ps1
```

Expected without a phone:

```text
Android G3 readiness check waiting on device
```

Check the strict G3 device requirement:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_g3_device_requirement.ps1
```

Expected without a phone:

```text
Android G3 device requirement check waiting on device
```

After preflight passes with a real device, install and collect logs:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1
```

Preview the same helper without a connected phone:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
```

Expected:

```text
Android device smoke plan OK
Screenshot: C:\...\analysis-output\android-device-smoke.png
ScreenshotCapture: True
Summary: C:\...\analysis-output\android-device-smoke-summary.json
SummaryWrite: True
CommandFileSmoke: True
UnityArguments: -mc2CommandFile
SmokeSuccessMarker: MC2 debrief summary assertion OK
SmokeSuccessMarker: MC2 loadout compact assertion OK
ConnectionCheck: check_android_device_connection.ps1 -RequireDevice
```

The helper verifies SDK tooling plus APK freshness, identity, compatibility,
signing, manifest, payload and size budget, discovers the APK package name
through `aapt`, checks that exactly one authorized Android device is connected,
installs the APK, pushes
`unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt`
to `/sdcard/Android/data/com.DefaultCompany.unitymc2demo/files/`, launches Unity
with `-mc2CommandFile`, waits briefly, captures logcat, an ignored
`analysis-output\android-device-smoke.png` screenshot and an ignored
`analysis-output\android-device-smoke-summary.json` summary, scans the log for
strong crash markers and the visible-flow success markers, validates the summary
schema, and fails if the package does not stay running before the command-file
smoke passes. If the strict connection gate is not satisfied, it fails before
install or launch with
`Android device smoke requires a single authorized Android device before install or launch`.
The standalone `check_android_smoke_connection_gate.ps1` gate also proves this
failure path does not rewrite the ignored log, screenshot or summary evidence
before a valid device is selected.

Self-test the log scanner without a device:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_log.ps1 -SelfTest
```

Self-test the summary schema checker without a device:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_summary.ps1 -SelfTest
```

Expected:

```text
Android smoke summary check self-test OK
```

Manual equivalent:

```powershell
$Adb = "$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe"
& $Adb devices
& $Adb install -r .\unity-mc2-demo\Builds\Android\MC2UnityDemo.apk
& $Adb shell mkdir -p /sdcard/Android/data/com.DefaultCompany.unitymc2demo/files
& $Adb push .\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt /sdcard/Android/data/com.DefaultCompany.unitymc2demo/files/mc2_01-visible-flow-audit.txt
& $Adb logcat -c
& $Adb shell am start -n com.DefaultCompany.unitymc2demo/com.unity3d.player.UnityPlayerGameActivity -e unity "-mc2CommandFile /sdcard/Android/data/com.DefaultCompany.unitymc2demo/files/mc2_01-visible-flow-audit.txt"
Start-Sleep -Seconds 12
& $Adb logcat -d > .\analysis-output\android-device-smoke.log
& $Adb exec-out screencap -p > .\analysis-output\android-device-smoke.png
```

The first manual pass must confirm:

- app launches without immediate crash;
- `check_android_smoke_log.ps1` reports no fatal exception, fatal signal, ANR,
  process death or forced activity finish for the package;
- battle scene is reachable;
- visible-flow path can reach battle, debrief, repair or MechLab, and relaunch;
- no `MINIMAX_API_KEY` is required;
- generated device logs and screenshots remain ignored.

Current local status
--------------------

On this machine, the Windows baseline passes and Android build smoke now passes.
The local Unity editor was present on disk but was not fully registered in Unity
Hub, so `install-modules` could not add Android modules directly. The local G2
recovery path used official component/tool downloads:

```text
Unity Android Build Support installer:
  UnitySetup-Android-Support-for-Editor-6000.4.7f1.exe
Android command-line tools:
  commandlinetools-win-14742923_latest.zip
OpenJDK:
  Microsoft.OpenJDK.17, linked at AndroidPlayer\OpenJDK
SDK packages:
  build-tools;36.0.0
  platforms;android-36
  platform-tools
  cmake;3.22.1
NDK:
  ndk;27.2.12479018, linked at AndroidPlayer\NDK
```

Current verified output:

```text
analysis-output\unity-validate-mobile-baseline.log -> MC2 demo contract validation OK
analysis-output\unity-build-android.log -> Build Finished, Result: Success.
analysis-output\unity-build-android-pc22-freshness.log -> MC2 Unity demo Android build OK
unity-mc2-demo\Builds\Android\MC2UnityDemo.apk -> exists, 20,667,008 bytes
scripts\unity\check_android_sdk_tooling.ps1 -> Android SDK tooling check OK
scripts\unity\check_android_apk_freshness.ps1 -> Android APK freshness check OK
scripts\unity\check_android_apk_identity.ps1 -> Android APK identity check OK
scripts\unity\check_android_apk_compatibility.ps1 -> Android APK compatibility check OK
scripts\unity\check_android_apk_signing.ps1 -> Android APK signing check OK
scripts\unity\check_android_apk_manifest.ps1 -> Android APK manifest check OK
scripts\unity\check_android_apk_payload.ps1 -> Android APK payload check OK
scripts\unity\check_android_apk_size_budget.ps1 -> Android APK size budget check OK
scripts\unity\check_android_smoke_artifact_hygiene.ps1 -> Android smoke artifact hygiene check OK
scripts\unity\check_android_smoke_summary.ps1 -SelfTest -> Android smoke summary check self-test OK
scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice -> smoke summary schema OK
scripts\unity\android_device_smoke.ps1 -PlanOnly -> ScreenshotCapture: True, Screenshot -> analysis-output\android-device-smoke.png
scripts\unity\android_device_smoke.ps1 -PlanOnly -> SummaryWrite: True, Summary -> analysis-output\android-device-smoke-summary.json
scripts\unity\android_device_smoke.ps1 -PlanOnly -> ConnectionCheck: check_android_device_connection.ps1 -RequireDevice
scripts\unity\check_android_smoke_connection_gate.ps1 -> Android smoke connection gate check waiting on device
scripts\unity\check_android_smoke_plan_consistency.ps1 -> Android smoke plan/preflight consistency check OK
scripts\unity\check_android_g3_readiness.ps1 -> Android G3 readiness check waiting on device
scripts\unity\check_android_g3_device_requirement.ps1 -> Android G3 device requirement check waiting on device
scripts\unity\check_android_device_connection.ps1 -> Android device connection check waiting on device, WpdOnlyAndroidProbe: True, AdbSetupHint: True
scripts\unity\watch_android_device_connection.ps1 -Once -AllowWaiting -> Android device connection watch waiting on device, AdbWatchHint: True
scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice -> Android device smoke preflight waiting on device
```

The APK, SDK logs, Unity build folder, Gradle cache, and device logs remain
ignored local outputs. The next gate is a real Android device smoke.
At the time this note was updated, `adb devices` returned no device rows, so G3
is waiting for a USB-debugging-enabled phone authorized on this PC.

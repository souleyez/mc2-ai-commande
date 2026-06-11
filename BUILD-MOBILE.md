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
still checks the APK, adb, aapt, package name and launchable activity, then
reports that G3 is waiting on a device.

After preflight passes with a real device, install and collect logs:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1
```

The helper discovers the APK package name through `aapt`, checks that exactly
one authorized Android device is connected, installs the APK, launches it, waits
briefly, captures logcat, scans the log for strong crash markers, and fails if
the package does not stay running.

Self-test the log scanner without a device:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_log.ps1 -SelfTest
```

Manual equivalent:

```powershell
$Adb = "$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe"
& $Adb devices
& $Adb install -r .\unity-mc2-demo\Builds\Android\MC2UnityDemo.apk
& $Adb logcat -c
& $Adb shell am start -n com.DefaultCompany.unitymc2demo/com.unity3d.player.UnityPlayerGameActivity
Start-Sleep -Seconds 12
& $Adb logcat -d > .\analysis-output\android-device-smoke.log
```

The first manual pass must confirm:

- app launches without immediate crash;
- `check_android_smoke_log.ps1` reports no fatal exception, fatal signal, ANR,
  process death or forced activity finish for the package;
- battle scene is reachable;
- visible-flow path can reach battle, debrief, repair or MechLab, and relaunch;
- no `MINIMAX_API_KEY` is required;
- generated device logs remain ignored.

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
analysis-output\unity-build-android.log -> MC2 Unity demo Android build OK
unity-mc2-demo\Builds\Android\MC2UnityDemo.apk -> exists, 20,666,724 bytes
scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice -> Android device smoke preflight waiting on device
```

The APK, SDK logs, Unity build folder, Gradle cache, and device logs remain
ignored local outputs. The next gate is a real Android device smoke.
At the time this note was updated, `adb devices` returned no device rows, so G3
is waiting for a USB-debugging-enabled phone authorized on this PC.

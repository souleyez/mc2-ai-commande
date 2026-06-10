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

After the APK exists, install and collect logs from a real Android device:

```powershell
adb devices
adb install -r .\unity-mc2-demo\Builds\Android\MC2UnityDemo.apk
adb logcat -c
adb logcat -d > .\analysis-output\android-device-smoke.log
```

The first manual pass must confirm:

- app launches without immediate crash;
- battle scene is reachable;
- visible-flow path can reach battle, debrief, repair or MechLab, and relaunch;
- no `MINIMAX_API_KEY` is required;
- generated device logs remain ignored.

Current local status
--------------------

On this machine, the Windows baseline passes. At the time this file was added,
Android Build Support was not installed:

```text
Test-Path ...\PlaybackEngines\AndroidPlayer -> False
```

That means `BuildAndroid` is now defined, but the actual Android artifact still
requires installing the Unity Android module.

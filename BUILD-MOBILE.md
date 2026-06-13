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
features, and confirms `small`,
ormal`, `large`, and `xlarge` screen support.

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
AdbDriverPackageProbe: True
AdbWatchHint: True
G3DeviceStatusReport: True
G3WhenReady: True
NoInstallOrLaunchUntilDeviceReady: True
```

Current wait-state checkpoint: `PC1-PC62`.
Completed mobile gates: `Pass Android G3 device smoke`, the landscape
`G4 Touch UI pass`, `G5 Mobile performance budget`, and
`G6 iOS feasibility gate`. `F2 map authoring contract` is also complete.
`F3 web ranking contract` is also complete.
`F4 creator economy boundary` is also complete.
`F5 server implementation boundary` is also complete.
`F6 local main-server prototype` is also complete.
`F7 document Unity main-server integration contract` is also complete.
`F8 implement optional Unity main-server client adapter` is also complete.
`F9 wire optional Unity main-server adapter into launch/debrief smoke` is also complete.
`F10 wire optional Unity inventory bootstrap smoke` is also complete.
`F11 plan inventory-to-MechBay binding boundary` is also complete.
Completed task: `F12 implement opt-in inventory-to-MechBay preview binding`.
Completed task: `F13 capture opt-in MechBay preview evidence`. Evidence gate: `scripts/unity/capture_inventory_mechbay_preview_evidence.ps1` -> `Inventory MechBay preview evidence capture OK`.
Completed task: `F14 capture landscape-phone MechLab source-line evidence`. Evidence gate: `scripts/unity/capture_landscape_phone_mechlab_source_line_evidence.ps1` -> `Landscape-phone MechLab source-line evidence capture OK`. `F15 plan server-backed receipt slice` is complete. Evidence gate: `scripts/unity/check_server_backed_receipt_slice_plan.ps1` -> `Server-backed receipt slice plan check OK`. `F16 implement server-backed receipt evidence gate` is complete. Evidence gate: `scripts/unity/capture_server_backed_receipt_evidence.ps1` -> `Server-backed receipt evidence capture OK`. `F17 plan post-receipt inventory refresh boundary` is complete. Evidence gate: `scripts/unity/check_post_receipt_inventory_refresh_boundary.ps1` -> `Post-receipt inventory refresh boundary check OK`. `F18 implement opt-in post-receipt inventory refresh binding` is complete. Evidence gate: `scripts/unity/check_post_receipt_inventory_refresh_binding.ps1` -> `Post-receipt inventory refresh binding check OK`. `F19 capture opt-in post-receipt refresh evidence` is complete. Evidence gate: `scripts/unity/capture_post_receipt_refresh_evidence.ps1` -> `Post-receipt refresh evidence capture OK`. `F20 refresh Android landscape build/smoke evidence` is complete. `F21 audit landscape touch UI ergonomics` is complete. Evidence gate: `scripts/unity/check_landscape_touch_ui_ergonomics.ps1` -> `Landscape touch UI ergonomics check OK`. `F22 audit landscape MechLab touch controls` is complete. Evidence gate: `scripts/unity/check_landscape_mechlab_touch_controls.ps1` -> `Landscape MechLab touch controls check OK`. `F23 capture landscape MechLab touch evidence` is complete. Evidence gate: `scripts/unity/capture_landscape_mechlab_touch_evidence.ps1` -> `Landscape MechLab touch evidence capture OK`. `F24 capture Android MechLab touch evidence` is complete. Evidence gate: `scripts/unity/capture_android_mechlab_touch_evidence.ps1` -> `Android MechLab touch evidence capture OK`. F25 capture Android battle command touch evidence is complete. Evidence gate: `scripts/unity/capture_android_battle_command_touch_evidence.ps1` -> `Android battle command touch evidence capture OK`. `F26 reduce Android combat effect log noise` is complete. `F27 audit Android entity placeholder collision path` is complete. Evidence gate: `scripts/unity/check_android_entity_placeholder_collision_path.ps1` -> `Android entity placeholder collision path check OK`. `F28 capture Android entity placeholder collision runtime evidence` is complete. Evidence gate: `scripts/unity/capture_android_entity_placeholder_collision_runtime_evidence.ps1` -> `Android entity placeholder collision runtime evidence capture OK`. `F29 audit PC controlled-demo visual readability` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_visual_readability.ps1` -> `PC controlled-demo visual readability audit OK`. `F30 implement PC controlled-demo visual readability fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_visual_readability_fixes.ps1` -> `PC controlled-demo visual readability fixes check OK`. `F31 refresh PC controlled-demo visual evidence after readability fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_visual_evidence.ps1` -> `PC controlled-demo visual evidence refresh OK`; `F32 audit PC controlled-demo command readability and formation feel` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_command_readability_formation.ps1` -> `PC controlled-demo command readability formation audit OK`; `F33 implement PC controlled-demo command readability and formation fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_command_readability_fixes.ps1` -> `PC controlled-demo command readability fixes check OK`; `F34 refresh PC controlled-demo command evidence after readability fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F35 audit post-F34 PC controlled-demo playable flow polish` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_playable_flow_polish.ps1` -> `PC controlled-demo playable flow polish audit OK`; `F36 implement post-F34 PC controlled-demo playable flow polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_playable_flow_polish_fixes.ps1` -> `PC controlled-demo playable flow polish fixes check OK`; `F37 refresh PC controlled-demo playable-flow evidence after polish fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F38 audit post-F37 PC controlled-demo investor readiness` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_readiness.ps1` -> `PC controlled-demo investor readiness audit OK`; `F39 implement post-F37 PC controlled-demo investor readiness fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_readiness_fixes.ps1` -> `PC controlled-demo investor readiness fixes check OK`; `F40 refresh PC controlled-demo investor-readiness evidence after fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F41 audit post-F40 PC controlled-demo investor evidence package` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_evidence_package.ps1` -> `PC controlled-demo investor evidence package audit OK`; `F42 implement post-F41 PC controlled-demo investor evidence package fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_package_fixes.ps1` -> `PC controlled-demo investor evidence package fixes check OK`; `F43 refresh PC controlled-demo investor evidence package after fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_refresh.ps1` -> `PC controlled-demo investor evidence refresh check OK`; `F44 audit post-F43 PC controlled-demo investor evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_evidence_refresh.ps1` -> `PC controlled-demo investor evidence refresh audit OK`; `F45 implement post-F44 PC controlled-demo investor evidence polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_polish_fixes.ps1` -> `PC controlled-demo investor evidence polish fixes check OK`; `F46 refresh PC controlled-demo investor route evidence after polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_refresh.ps1` -> `PC controlled-demo investor route evidence refresh check OK`; `F47 audit post-F46 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_refresh.ps1` -> `PC controlled-demo investor route evidence refresh audit OK`; `F48 implement post-F47 PC controlled-demo investor route evidence audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fixes check OK`; `F49 refresh PC controlled-demo investor route evidence after audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh check OK`; `F50 audit post-F49 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit OK`; `F51 implement post-F50 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fixes check OK`; `F52 refresh PC controlled-demo investor route evidence after F50 audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh check OK`; `F53 audit post-F52 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit OK`; `F54 implement post-F53 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fixes check OK`; next task was `F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F26 evidence gate: `scripts/unity/check_android_combat_effect_log_noise.ps1` -> `Android combat effect log noise check OK`.

Mobile orientation decision: the first phone version is landscape-only. In
product terms, it is the 手机端横版: a horizontal phone game version, not a
portrait UI that can rotate. Portrait layout is not a supported first-version target.
Future mobile UI work must keep the battle map, unit status rows, Jet/system
controls and MechLab usable on landscape phone aspect ratios before considering
any portrait pass.

Check the mobile landscape contract without launching Unity or the APK:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_landscape_contract.ps1 -RepoRoot .
```

Expected success string:

```text
Mobile landscape contract check OK.
```

Check installed Android ADB driver package candidates without installing or
launching:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_adb_driver_package.ps1
```

Expected markers:

```text
Android ADB driver package probe OK
AdbDriverPackageProbe: True
NoInstallOrLaunch: True
```

Checkpoint marker: `Add Android ADB driver package probe`.

If Windows sees a connected Android phone only as WPD/MTP, the same helper can
report `WpdOnlyAndroidDevice: True`. That is still a G3 waiting state until adb
shows exactly one authorized `device` row. The setup hint reports the current
Windows driver/provider/inf/service so the MTP-only state is distinguishable
from an ADB-ready phone.

To wait while changing phone USB debugging or driver state:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\watch_android_device_connection.ps1 -TimeoutSeconds 120
```

To write the current G3 device status report without installing or launching:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\write_android_g3_device_status.ps1
```

To preview the when-ready G3 runner without installing or launching:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\run_android_g3_when_ready.ps1 -PlanOnly
```

Checkpoint marker: `Add Android G3 when-ready runner`.

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
scripts\unity\check_android_smoke_connection_gate.ps1 -> Android smoke connection gate check ready for G3 device smoke
scripts\unity\check_android_smoke_plan_consistency.ps1 -> Android smoke plan/preflight consistency check OK
scripts\unity\check_android_g3_readiness.ps1 -RequireDevice -> Android G3 readiness check OK
scripts\unity\check_android_g3_device_requirement.ps1 -> Android G3 device requirement check OK
scripts\unity\check_android_device_connection.ps1 -> Android device connection check OK, b5212798:device, WpdOnlyAndroidProbe: True, AdbSetupHint: True
scripts\unity\check_android_adb_driver_package.ps1 -> Android ADB driver package probe OK, AdbDriverPackageProbe: True
scripts\unity\watch_android_device_connection.ps1 -Once -AllowWaiting -> Android device connection watch OK, AdbWatchHint: True
scripts\unity\write_android_g3_device_status.ps1 -> Android G3 device status report OK, G3DeviceStatusReport: True, G3DeviceReady: True
scripts\unity\run_android_g3_when_ready.ps1 -PlanOnly -> Android G3 when-ready plan OK, G3WhenReady: True
scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice -> Android device smoke preflight OK
scripts\unity\run_android_g3_when_ready.ps1 -TimeoutSeconds 30 -AllowWaiting -LaunchWaitSeconds 75 -> G3WhenReady: True, SmokeTestPassed: True, status=smokePassed
scripts\unity\check_mobile_landscape_contract.ps1 -> Mobile landscape contract check OK.
```

No-device fallback marker remains `Android device smoke preflight waiting on device`.

The APK, SDK logs, Unity build folder, Gradle cache, and device logs remain
ignored local outputs. The real Android device smoke gate, the landscape touch
UI pass, the first mobile performance budget, the iOS feasibility gate, the map
authoring contract, the Unity main-server integration contract, the optional
Unity main-server client adapter and the optional Unity main-server
launch/debrief smoke and optional Unity inventory bootstrap smoke have passed;
the inventory-to-MechBay binding boundary, opt-in MechBay preview binding,
preview evidence, and landscape-phone MechLab source-line evidence have
passed; `F15 plan server-backed receipt slice` is complete; `F16 implement server-backed receipt evidence gate` is complete; `F17 plan post-receipt inventory refresh boundary` is complete; `F18 implement opt-in post-receipt inventory refresh binding` is complete; `F19 capture opt-in post-receipt refresh evidence` is complete; `F20 refresh Android landscape build/smoke evidence` is complete; `F21 audit landscape touch UI ergonomics` is complete; `F22 audit landscape MechLab touch controls` is complete; `F23 capture landscape MechLab touch evidence` is complete; `F24 capture Android MechLab touch evidence` is complete; `F25 capture Android battle command touch evidence` is complete. Evidence gate: `scripts/unity/capture_android_battle_command_touch_evidence.ps1` -> `Android battle command touch evidence capture OK`; `F26 reduce Android combat effect log noise` is complete; `F27 audit Android entity placeholder collision path` is complete. Evidence gate: `scripts/unity/check_android_entity_placeholder_collision_path.ps1` -> `Android entity placeholder collision path check OK`; `F28 capture Android entity placeholder collision runtime evidence` is complete. Evidence gate: `scripts/unity/capture_android_entity_placeholder_collision_runtime_evidence.ps1` -> `Android entity placeholder collision runtime evidence capture OK`; F29 audit PC controlled-demo visual readability is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_visual_readability.ps1` -> `PC controlled-demo visual readability audit OK`; `F30 implement PC controlled-demo visual readability fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_visual_readability_fixes.ps1` -> `PC controlled-demo visual readability fixes check OK`; `F31 refresh PC controlled-demo visual evidence after readability fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_visual_evidence.ps1` -> `PC controlled-demo visual evidence refresh OK`; `F32 audit PC controlled-demo command readability and formation feel` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_command_readability_formation.ps1` -> `PC controlled-demo command readability formation audit OK`; `F33 implement PC controlled-demo command readability and formation fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_command_readability_fixes.ps1` -> `PC controlled-demo command readability fixes check OK`; `F34 refresh PC controlled-demo command evidence after readability fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F35 audit post-F34 PC controlled-demo playable flow polish` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_playable_flow_polish.ps1` -> `PC controlled-demo playable flow polish audit OK`; `F36 implement post-F34 PC controlled-demo playable flow polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_playable_flow_polish_fixes.ps1` -> `PC controlled-demo playable flow polish fixes check OK`; `F37 refresh PC controlled-demo playable-flow evidence after polish fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F38 audit post-F37 PC controlled-demo investor readiness` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_readiness.ps1` -> `PC controlled-demo investor readiness audit OK`; `F39 implement post-F37 PC controlled-demo investor readiness fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_readiness_fixes.ps1` -> `PC controlled-demo investor readiness fixes check OK`; `F40 refresh PC controlled-demo investor-readiness evidence after fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F41 audit post-F40 PC controlled-demo investor evidence package` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_evidence_package.ps1` -> `PC controlled-demo investor evidence package audit OK`; `F42 implement post-F41 PC controlled-demo investor evidence package fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_package_fixes.ps1` -> `PC controlled-demo investor evidence package fixes check OK`; `F43 refresh PC controlled-demo investor evidence package after fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_refresh.ps1` -> `PC controlled-demo investor evidence refresh check OK`; `F44 audit post-F43 PC controlled-demo investor evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_evidence_refresh.ps1` -> `PC controlled-demo investor evidence refresh audit OK`; `F45 implement post-F44 PC controlled-demo investor evidence polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_polish_fixes.ps1` -> `PC controlled-demo investor evidence polish fixes check OK`; `F46 refresh PC controlled-demo investor route evidence after polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_refresh.ps1` -> `PC controlled-demo investor route evidence refresh check OK`; `F47 audit post-F46 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_refresh.ps1` -> `PC controlled-demo investor route evidence refresh audit OK`; `F48 implement post-F47 PC controlled-demo investor route evidence audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fixes check OK`; `F49 refresh PC controlled-demo investor route evidence after audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh check OK`; `F50 audit post-F49 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit OK`; `F51 implement post-F50 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fixes check OK`; `F52 refresh PC controlled-demo investor route evidence after F50 audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh check OK`; `F53 audit post-F52 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit OK`; `F54 implement post-F53 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fixes check OK`; next task was `F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.
At the time this note was updated, `adb devices -l` returned `b5212798 device`
for Mi 11 Lite through `winusb.inf`; the package installs and launches through
the G3 when-ready runner/direct smoke path, the visible-flow command-file smoke
reaches the debrief and loadout compact success markers, and the device
screenshot summary reports `screenshot orientation OK landscape 2400x1080`.
The G5 performance baseline reports 30.48 FPS after warmup, 273,342 KB PSS,
19.80 MiB APK size, and `Thermal Status: 0`.

iOS feasibility
---------------

This Windows machine does not attempt an iOS build. The iOS lane requires a
Mac build host, Unity iOS Build Support, Xcode, Apple signing and a physical
iOS device.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_ios_feasibility_gate.ps1 -RepoRoot .
```

Expected success string:

```text
iOS feasibility gate check OK
```

Map authoring contract
----------------------

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_map_authoring_contract.ps1 -RepoRoot .
```

Expected success string:

```text
Map authoring contract check OK
```

Web ranking contract
--------------------

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_web_ranking_contract.ps1 -RepoRoot .
```

Expected success string:

```text
Web ranking contract check OK
```

Creator economy boundary
------------------------

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_creator_economy_boundary.ps1 -RepoRoot .
```

Expected success string:

```text
Creator economy boundary check OK
```

Server implementation boundary
------------------------------

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_server_implementation_boundary.ps1 -RepoRoot .
```

Expected success string:

```text
Server implementation boundary check OK
```

Local main-server prototype
---------------------------

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\server\check_local_main_server.ps1 -RepoRoot .
```

Expected success string:

```text
Local main-server prototype check OK
```

Unity main-server integration contract
--------------------------------------

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_unity_main_server_integration_contract.ps1 -RepoRoot .
```

This validates `docs-unity-main-server-integration-contract-2026-06-12.md`
against the local server prototype and the current offline-first plan markers.

Expected:

```text
Unity main-server integration contract check OK
```

Optional Unity main-server client adapter
-----------------------------------------

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_optional_unity_main_server_client_adapter.ps1 -RepoRoot .
```

Expected:

```text
Optional Unity main-server client adapter check OK
```

Optional Unity main-server launch/debrief smoke
-----------------------------------------------

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_optional_unity_main_server_launch_debrief_smoke.ps1 -RepoRoot .
```

Expected:

```text
Optional Unity main-server launch/debrief smoke check OK
```

Optional Unity inventory bootstrap smoke
----------------------------------------

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_optional_unity_inventory_bootstrap_smoke.ps1 -RepoRoot .
```

Expected:

```text
Optional Unity inventory bootstrap smoke check OK
```

Inventory-to-MechBay binding boundary
-------------------------------------

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_inventory_mechbay_binding_boundary.ps1 -RepoRoot .
```

Expected:

```text
Inventory-to-MechBay binding boundary check OK
```

## F12 Preview Binding Checkpoint

`F12 implement opt-in inventory-to-MechBay preview binding` is complete. `F13 capture opt-in MechBay preview evidence` is complete. `F14 capture landscape-phone MechLab source-line evidence` is complete. The opt-in gate is `scripts/unity/check_optional_inventory_mechbay_preview_binding.ps1`, with expected success string `Optional inventory-to-MechBay preview binding check OK`; the preview evidence gate is `scripts/unity/capture_inventory_mechbay_preview_evidence.ps1`, with expected success string `Inventory MechBay preview evidence capture OK`; the landscape-phone evidence gate is `scripts/unity/capture_landscape_phone_mechlab_source_line_evidence.ps1`, with expected success string `Landscape-phone MechLab source-line evidence capture OK`. `F15 plan server-backed receipt slice` is complete. Evidence gate: `scripts/unity/check_server_backed_receipt_slice_plan.ps1` -> `Server-backed receipt slice plan check OK`. `F16 implement server-backed receipt evidence gate` is complete. Evidence gate: `scripts/unity/capture_server_backed_receipt_evidence.ps1` -> `Server-backed receipt evidence capture OK`. `F17 plan post-receipt inventory refresh boundary` is complete. Evidence gate: `scripts/unity/check_post_receipt_inventory_refresh_boundary.ps1` -> `Post-receipt inventory refresh boundary check OK`. `F18 implement opt-in post-receipt inventory refresh binding` is complete. Evidence gate: `scripts/unity/check_post_receipt_inventory_refresh_binding.ps1` -> `Post-receipt inventory refresh binding check OK`. `F19 capture opt-in post-receipt refresh evidence` is complete. Evidence gate: `scripts/unity/capture_post_receipt_refresh_evidence.ps1` -> `Post-receipt refresh evidence capture OK`. `F20 refresh Android landscape build/smoke evidence` is complete. `F21 audit landscape touch UI ergonomics` is complete. Evidence gate: `scripts/unity/check_landscape_touch_ui_ergonomics.ps1` -> `Landscape touch UI ergonomics check OK`. `F22 audit landscape MechLab touch controls` is complete. Evidence gate: `scripts/unity/check_landscape_mechlab_touch_controls.ps1` -> `Landscape MechLab touch controls check OK`. `F23 capture landscape MechLab touch evidence` is complete. Evidence gate: `scripts/unity/capture_landscape_mechlab_touch_evidence.ps1` -> `Landscape MechLab touch evidence capture OK`. `F24 capture Android MechLab touch evidence` is complete. Evidence gate: `scripts/unity/capture_android_mechlab_touch_evidence.ps1` -> `Android MechLab touch evidence capture OK`. F25 capture Android battle command touch evidence is complete. Evidence gate: `scripts/unity/capture_android_battle_command_touch_evidence.ps1` -> `Android battle command touch evidence capture OK`. `F26 reduce Android combat effect log noise` is complete. `F27 audit Android entity placeholder collision path` is complete. Evidence gate: `scripts/unity/check_android_entity_placeholder_collision_path.ps1` -> `Android entity placeholder collision path check OK`. `F28 capture Android entity placeholder collision runtime evidence` is complete. Evidence gate: `scripts/unity/capture_android_entity_placeholder_collision_runtime_evidence.ps1` -> `Android entity placeholder collision runtime evidence capture OK`. `F29 audit PC controlled-demo visual readability` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_visual_readability.ps1` -> `PC controlled-demo visual readability audit OK`. `F30 implement PC controlled-demo visual readability fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_visual_readability_fixes.ps1` -> `PC controlled-demo visual readability fixes check OK`. `F31 refresh PC controlled-demo visual evidence after readability fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_visual_evidence.ps1` -> `PC controlled-demo visual evidence refresh OK`; `F32 audit PC controlled-demo command readability and formation feel` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_command_readability_formation.ps1` -> `PC controlled-demo command readability formation audit OK`; `F33 implement PC controlled-demo command readability and formation fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_command_readability_fixes.ps1` -> `PC controlled-demo command readability fixes check OK`; `F34 refresh PC controlled-demo command evidence after readability fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F35 audit post-F34 PC controlled-demo playable flow polish` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_playable_flow_polish.ps1` -> `PC controlled-demo playable flow polish audit OK`; `F36 implement post-F34 PC controlled-demo playable flow polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_playable_flow_polish_fixes.ps1` -> `PC controlled-demo playable flow polish fixes check OK`; `F37 refresh PC controlled-demo playable-flow evidence after polish fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F38 audit post-F37 PC controlled-demo investor readiness` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_readiness.ps1` -> `PC controlled-demo investor readiness audit OK`; `F39 implement post-F37 PC controlled-demo investor readiness fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_readiness_fixes.ps1` -> `PC controlled-demo investor readiness fixes check OK`; `F40 refresh PC controlled-demo investor-readiness evidence after fixes` is complete. Evidence gate: `scripts/unity/capture_pc_controlled_demo_command_evidence.ps1` -> `PC controlled-demo command evidence refresh OK`; `F41 audit post-F40 PC controlled-demo investor evidence package` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_evidence_package.ps1` -> `PC controlled-demo investor evidence package audit OK`; `F42 implement post-F41 PC controlled-demo investor evidence package fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_package_fixes.ps1` -> `PC controlled-demo investor evidence package fixes check OK`; `F43 refresh PC controlled-demo investor evidence package after fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_refresh.ps1` -> `PC controlled-demo investor evidence refresh check OK`; `F44 audit post-F43 PC controlled-demo investor evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_evidence_refresh.ps1` -> `PC controlled-demo investor evidence refresh audit OK`; `F45 implement post-F44 PC controlled-demo investor evidence polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_evidence_polish_fixes.ps1` -> `PC controlled-demo investor evidence polish fixes check OK`; `F46 refresh PC controlled-demo investor route evidence after polish fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_refresh.ps1` -> `PC controlled-demo investor route evidence refresh check OK`; `F47 audit post-F46 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_refresh.ps1` -> `PC controlled-demo investor route evidence refresh audit OK`; `F48 implement post-F47 PC controlled-demo investor route evidence audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fixes check OK`; `F49 refresh PC controlled-demo investor route evidence after audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh check OK`; `F50 audit post-F49 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit OK`; `F51 implement post-F50 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fixes check OK`; `F52 refresh PC controlled-demo investor route evidence after F50 audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh check OK`; `F53 audit post-F52 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit OK`; `F54 implement post-F53 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fixes check OK`; next task was `F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F54 implementation note: `F54 implement post-F53 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fixes check OK`; next task was `F55 refresh PC controlled-demo investor route evidence after F53 audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F55 implementation note: `F55 refresh PC controlled-demo investor route evidence after F53 audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh check OK`; next task was `F56 audit post-F55 PC controlled-demo investor route evidence refresh`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F56 implementation note: `F56 audit post-F55 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit OK`; next task was `F57 implement post-F56 PC controlled-demo investor route evidence refresh audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F57 implementation note: `F57 implement post-F56 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fixes check OK`; next task was `F58 refresh PC controlled-demo investor route evidence after F56 audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F58 implementation note: `F58 refresh PC controlled-demo investor route evidence after F56 audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh check OK`; next task was `F59 audit post-F58 PC controlled-demo investor route evidence refresh`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F59 implementation note: `F59 audit post-F58 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit OK`; next task was `F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F60 implementation note: `F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes check OK`; next task was `F61 refresh PC controlled-demo investor route evidence after F59 audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F61 implementation note: `F61 refresh PC controlled-demo investor route evidence after F59 audit fixes` is complete. Evidence gate: `scripts/unity/check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh check OK`; next task was `F62 audit post-F61 PC controlled-demo investor route evidence refresh`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

F62 implementation note: `F62 audit post-F61 PC controlled-demo investor route evidence refresh` is complete. Evidence gate: `scripts/unity/audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit.ps1` -> `PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit OK`; formal next task: `F63 implement post-F62 PC controlled-demo investor route evidence refresh audit fixes`. Mobile phones remain first-version landscape-only as the horizontal phone build; portrait is not a first-slice support target.

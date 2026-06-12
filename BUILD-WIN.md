Current Unity 6 Windows Demo handoff
====================================

This is the current repeatable path for the playable Windows demo. The older
native/C++ build notes are still kept below for historical engine work, but the
active first playable demo is the Unity 6 project in `unity-mc2-demo`.

Run these commands from the repository root:

```powershell
cd C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2
```

Validate the mission, loadout, AI boundary, occupancy, damage, and command-file
contracts:

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -batchmode -quit `
  -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" `
  -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract `
  -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-demo-package.log"
```

Build the Windows player:

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -batchmode -quit `
  -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" `
  -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 `
  -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-demo-package.log"
```

Preflight or launch the controlled Windows demo:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\run_windows_demo.ps1 -CheckOnly
```

To open the visible demo window with controlled 1280x720 windowed parameters:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\run_windows_demo.ps1
```

The launcher checks that `MC2UnityDemo.exe` and `MC2UnityDemo_Data` are both
present, writes the runtime log to `analysis-output/windows-demo-run.log`, and
passes Unity window arguments so the demo does not depend on stale player
window state from a previous run.

Check that the ignored Windows player output is newer than the tracked Unity
build inputs:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_windows_demo_build_freshness.ps1
```

The freshness check reads tracked Unity `Assets`, `ProjectSettings` and
`Packages` timestamps plus the ignored Windows player output. It does not start
Unity; rebuild the Windows player if it reports stale output.

Smoke the visible playable loop:

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" `
  -batchmode -nographics -mc2SmokeTest `
  -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" `
  -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-demo-package.log"
```

Capture reference screenshots and JSON sidecars when visual evidence is needed:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass `
  -File scripts\unity\capture_reference_visuals.ps1 `
  -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol
```

Check the current controlled-demo evidence package without rerunning Unity:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_evidence.ps1
```

The evidence checker validates the Windows build, visible-flow log, six PNG/JSON
captures, MechLab no-toggle fitting, terrain readability, sparse battle HUD,
contact separation, damage-demo story, and evidence freshness. The visible-flow
log must be newer than the current Windows build and command script; capture
PNG/JSON sidecars and per-preset capture logs must be newer than the current
Windows build and capture helper. It reads ignored local evidence and does not
create new artifacts.

Run the full controlled-demo readiness preflight:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_readiness.ps1
```

The readiness preflight wraps the launch preflight, build freshness, evidence
health check, and public boundary preflight. It does not start the Unity player
or regenerate captures.

Check the controlled-demo handoff consistency:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1
```

The handoff consistency check verifies that the key scripts, README, Windows
build handoff, plan docs, evidence page, and machine handoff plan still point to
the same controlled demo commands and current PC gate status.

Check demo source hygiene without launching Unity:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
```

This checks tracked and staged paths plus `.gitignore` markers so generated
evidence, Unity builds, APK/AAB outputs and private reference art stay out of
source commits.

Check Android smoke artifact hygiene without launching Unity:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_artifact_hygiene.ps1
```

This checks tracked and staged paths plus `.gitignore` markers so Android smoke
logs, screenshots, summary JSON files and APK/AAB build outputs stay ignored
until a real evidence drop is explicitly requested.

Check the AI deputy contract without launching Unity or calling the model:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_ai_deputy_contract.ps1
```

This checks source and command-script markers proving MiniMax remains optional,
slow, high-level and rule-fallback guarded, while normal visible-flow smoke does
not request MiniMax commander steps.

Check the PC core playable BattleCore contract:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_core_playable_contract.ps1
```

This runs the Unity editor validator and requires the explicit PC core marker
for command state, solo return, Jet legality, occupancy, damage/ejection, and
debrief/relaunch. It writes only an ignored validator log under
`analysis-output/`.

Check the mobile command model preflight without rerunning Unity:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
```

The mobile command model preflight reads the current ignored reference sidecars
and source/doc markers to verify that active battle remains translatable to the
planned phone command loop: status rows, Jet, map, bay/system, compact
objectives, hidden combat log/save/account/debug overlays, and MechLab fitting
without weapon enable/disable toggles.

Check the sparse battle HUD contract without launching Unity:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_battle_hud_sparse_contract.ps1
```

This checks the current Unity presentation source, capture gate, and mobile
command model preflight all require the same sparse battle HUD: status rows,
compact objective, closed mission map, hidden combat log, hidden overlays,
disabled save UI, and sidecar-only debug occupancy.

Check the PC visual capture sanity gate without launching Unity:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_visual_capture_sanity.ps1
```

This checks the six standard controlled-demo PNG captures for expected size,
sampled color variety, center visibility, contrast, low magenta fallback color,
and non-monochrome content.

Self-test the PC visual capture sanity thresholds without launching Unity:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_visual_capture_sanity.ps1 -SelfTest
```

Expected success string: `PC visual capture sanity self-test OK`.

Check the PC capture sidecar schema without launching Unity:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_capture_sidecar_schema.ps1
```

This checks the six standard controlled-demo JSON sidecars for screenshot path,
screen size, flow, camera, unit/contact counts, objective presence, summary
fields, and reference-asset metadata.

Expected success string: `PC capture sidecar schema check OK`.

Check the PC capture preset contract without launching Unity:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_capture_preset_contract.ps1
```

This checks that the standard controlled-demo preset list remains
`mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol` across the
capture helper, evidence checks, visual sanity checks, sidecar schema checks and
handoff docs.

Expected success string: `PC capture preset contract check OK`.

Check the PC capture artifact hygiene without launching Unity:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_capture_artifact_hygiene.ps1
```

This checks that local reference screenshots, JSON sidecars, capture logs and
visual sanity self-test images remain ignored generated evidence and are absent
from tracked or staged source paths.

Expected success string: `PC capture artifact hygiene check OK`.

Check the PC window contract without launching Unity:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_window_contract.ps1
```

This checks that the controlled Windows demo launcher and reference capture
helper keep `1280x720` windowed defaults and pass `-screen-fullscreen 0`.

Expected success string: `PC window contract check OK`.

Check the PC launch log hygiene without launching Unity:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_launch_log_hygiene.ps1
```

This checks that the controlled launcher writes runtime logs to
`analysis-output/windows-demo-run.log`, that Git ignores that path, and that
local launch logs are absent from tracked or staged source paths.

Expected success string: `PC launch log hygiene check OK`.

Check the PC build artifact hygiene without launching Unity:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_build_artifact_hygiene.ps1
```

This checks that the Windows player output stays under
`unity-mc2-demo/Builds/Windows/`, that Git ignores Unity build output paths,
and that local player builds are absent from tracked or staged source paths.

Expected success string: `PC build artifact hygiene check OK`.

Check the PC smoke artifact hygiene without launching Unity:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_smoke_artifact_hygiene.ps1
```

This checks that PC smoke, validator, build and saved-account evidence outputs
stay under ignored `analysis-output/` paths and are absent from tracked or staged
source paths.

Expected success string: `PC smoke artifact hygiene check OK`.

Check the current plan queue consistency without launching Unity:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_queue.ps1
```

This confirms the current PC/mobile package is sealed through `PC1-PC57`, that
`Pass Android G3 device smoke`, the landscape `G4 Touch UI pass`,
`G5 Mobile performance budget`, `G6 iOS feasibility gate`,
`F2 map authoring contract`, `F3 web ranking contract`, and
`F4 creator economy boundary`, `F5 server implementation boundary`,
`F6 local main-server prototype`,
`F7 document Unity main-server integration contract` and
`F8 implement optional Unity main-server client adapter` and
`F9 wire optional Unity main-server adapter into launch/debrief smoke` are
recorded, that `F10 wire optional Unity inventory bootstrap smoke` is recorded,
that `F11 plan inventory-to-MechBay binding boundary` is recorded, and that
`F12 implement opt-in inventory-to-MechBay preview binding`,
`F13 capture opt-in MechBay preview evidence`, and
`F14 capture landscape-phone MechLab source-line evidence` are recorded. The
`F16 implement server-backed receipt evidence gate` is complete; `F17 plan post-receipt inventory refresh boundary` is complete; `F18 implement opt-in post-receipt inventory refresh binding` is complete; `F19 capture opt-in post-receipt refresh evidence` is complete; `F20 refresh Android landscape build/smoke evidence` is complete; `F21 audit landscape touch UI ergonomics` is complete; `F22 audit landscape MechLab touch controls` is complete; `F23 capture landscape MechLab touch evidence` is complete; `F24 capture Android MechLab touch evidence` is complete; `F25 capture Android battle command touch evidence` is complete. Evidence gate: `scripts/unity/capture_android_battle_command_touch_evidence.ps1` -> `Android battle command touch evidence capture OK`; the formal next task is `F26 reduce Android combat effect log noise`.

Expected success string: `Current plan queue consistency check OK`.

Check the Android device connection state before real G3 smoke:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_connection.ps1
```

This reads `adb devices -l` and reports no-device, unauthorized, offline,
multiple-device or ready states without installing or launching the APK.

Current wait-state checkpoint: `PC1-PC57`.

Check the mobile landscape contract without launching Unity or the APK:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_landscape_contract.ps1 -RepoRoot .
```

Expected success string: `Mobile landscape contract check OK`.

Check the iOS feasibility gate without attempting a Windows iOS build:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_ios_feasibility_gate.ps1 -RepoRoot .
```

Expected success string: `iOS feasibility gate check OK`.

Check the map authoring contract:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_map_authoring_contract.ps1 -RepoRoot .
```

Expected success string: `Map authoring contract check OK`.

Check the Web ranking contract:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_web_ranking_contract.ps1 -RepoRoot .
```

Expected success string: `Web ranking contract check OK`.

Check the creator economy boundary:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_creator_economy_boundary.ps1 -RepoRoot .
```

Expected success string: `Creator economy boundary check OK`.

Check the server implementation boundary:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_server_implementation_boundary.ps1 -RepoRoot .
```

Expected success string: `Server implementation boundary check OK`.

Check the local main-server prototype:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\server\check_local_main_server.ps1 -RepoRoot .
```

Expected success string: `Local main-server prototype check OK`.

Check the Unity main-server integration contract:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_unity_main_server_integration_contract.ps1 -RepoRoot .
```

This validates `docs-unity-main-server-integration-contract-2026-06-12.md`
against the local server prototype and the current offline-first plan markers.

Expected success string: `Unity main-server integration contract check OK`.

Check the optional Unity main-server client adapter:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_optional_unity_main_server_client_adapter.ps1 -RepoRoot .
```

Expected success string: `Optional Unity main-server client adapter check OK`.

Check the optional Unity main-server launch/debrief smoke:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_optional_unity_main_server_launch_debrief_smoke.ps1 -RepoRoot .
```

Expected success string: `Optional Unity main-server launch/debrief smoke check OK`.

Check the optional Unity inventory bootstrap smoke:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_optional_unity_inventory_bootstrap_smoke.ps1 -RepoRoot .
```

Expected success string: `Optional Unity inventory bootstrap smoke check OK`.

Check the inventory-to-MechBay binding boundary:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_inventory_mechbay_binding_boundary.ps1 -RepoRoot .
```

This validates `docs-inventory-mechbay-binding-boundary-2026-06-12.md` before
any opt-in preview binding is implemented.

Expected success string: `Inventory-to-MechBay binding boundary check OK.`.

Expected waiting-state string without an authorized adb phone: `Android device connection check waiting on device`.

Diagnostic marker: `WpdOnlyAndroidProbe: True`.
Setup hint marker: `AdbSetupHint: True`.
Driver package marker: `AdbDriverPackageProbe: True`.
Watch marker: `AdbWatchHint: True`.
Status report marker: `G3DeviceStatusReport: True`.
When-ready marker: `G3WhenReady: True`.

When Windows only exposes a connected Android phone as WPD/MTP, the helper can
also report `WpdOnlyAndroidDevice: True`; that still means G3 is waiting for USB
debugging authorization or an ADB driver before install/launch. In the current
Mi 11 Lite state, Windows reports the phone through `winusb.inf`, adb exposes
one authorized `device` row, and the real G3 device smoke has passed.

Checkpoint marker: `Add Android ADB setup guidance`.

Check installed Android ADB driver package candidates:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_adb_driver_package.ps1
```

Expected markers: `AdbDriverPackageProbe: True`,
`CandidateDriverPackages:` and `CurrentPhoneDriver:`.

Checkpoint marker: `Add Android ADB driver package probe`.

Previous checkpoint marker: `Add Android ADB setup guidance`.

Watch the connection while changing phone-side USB debugging or driver state:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\watch_android_device_connection.ps1 -TimeoutSeconds 120
```

No-device-safe check:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\watch_android_device_connection.ps1 -Once -AllowWaiting
```

Checkpoint marker: `Add Android ADB readiness watch`.

Previous checkpoint marker: `Add Android device connection check`.

Write the current Android G3 device status report:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\write_android_g3_device_status.ps1
```

This writes ignored `analysis-output\android-g3-device-status.json` and reports
`G3DeviceStatusReport: True`, `G3DeviceReady: False` or `True`, and
`NoInstallOrLaunch: True`.

Checkpoint marker: `Add Android G3 device status report`.

Previous checkpoint marker: `Add Android ADB readiness watch`.

Preview the Android G3 when-ready runner:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\run_android_g3_when_ready.ps1 -PlanOnly
```

Expected markers: `G3WhenReady: True` and
`NoInstallOrLaunchUntilDeviceReady: True`.

Checkpoint marker: `Add Android G3 when-ready runner`.

Previous checkpoint marker: `Add Android G3 device status report`.

Real Android smoke runs now require the same connection gate before install or
launch. The plan output must include
`ConnectionCheck: check_android_device_connection.ps1 -RequireDevice`, and a
strict no-device run reports
`Android device smoke requires a single authorized Android device before install or launch`.

Checkpoint marker: `Wire Android smoke connection gate`.

Check the Android smoke connection gate behavior before real G3 smoke:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_connection_gate.ps1
```

This runs the real smoke helper only far enough to prove the strict connection
failure happens before install or launch, then checks that the ignored log,
screenshot and summary outputs remain unchanged while no valid device is
selected.

Expected success strings:

- `Android smoke connection gate check OK`
- `Android smoke connection gate check waiting on device`

Checkpoint marker: `Add Android smoke connection gate check`.

Preview the Android visible-flow command-file smoke before a real G3 run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
```

Expected plan markers:

```text
CommandFileSmoke: True
UnityArguments: -mc2CommandFile
SmokeSuccessMarker: MC2 debrief summary assertion OK
SmokeSuccessMarker: MC2 loadout compact assertion OK
```

Checkpoint marker: `Add Android visible-flow command-file smoke`.

Check the current plan gate without launching Unity:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
```

The current plan gate wraps handoff/readiness, Windows build freshness, demo
source hygiene, Android smoke artifact hygiene, AI deputy contract, mobile command model, battle HUD sparse
contract, PC visual capture sanity, PC visual capture sanity self-test, PC capture sidecar schema, PC capture preset contract, PC capture artifact hygiene, PC window contract, PC launch log hygiene, PC build artifact hygiene, PC smoke artifact hygiene, current plan queue consistency, Android device connection, Android smoke connection gate, Android SDK tooling, Android APK freshness, Android APK identity, Android APK
compatibility, Android APK signing, Android APK manifest, Android APK payload,
Android APK size budget, Android smoke summary schema, Android device-smoke preflight, Android smoke plan/preflight consistency, Android G3 readiness and Android G3 device requirement checks. The device preflight also runs the summary schema self-test. With no
authorized phone connected it should still pass while reporting Android as
waiting on device.

Check the Android SDK tooling directly:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_sdk_tooling.ps1
```

It validates the Unity AndroidPlayer SDK, NDK, OpenJDK, build tools, platform
and command-line tools before G3 install/launch.

Check the Android APK compatibility directly:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_compatibility.ps1
```

It validates the current APK `minSdkVersion`, `targetSdkVersion`, and native ABI
metadata before G3 install/launch.

Check the Android APK signing directly:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_signing.ps1
```

It validates `apksigner verify`, APK Signature Scheme v2, and the current debug
signer DN before G3 install/launch.

Check the Android APK manifest directly:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_manifest.ps1
```

It validates permissions, required hardware features and supported screen
classes before G3 install/launch.

Check the Android APK payload directly:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_payload.ps1
```

It validates the APK contains the expected Unity/IL2CPP native libraries,
`assets/bin/Data` runtime files and single `arm64-v8a` ABI folder before G3
install/launch.

Check the Android APK size budget directly:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_size_budget.ps1
```

It validates the APK is within the current early mobile demo package budget
before G3 install/launch.

Self-test the Android smoke log scanner:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_log.ps1 -SelfTest
```

The log scanner is used by the Android device smoke helper after logcat capture
to fail on strong crash markers such as fatal exceptions, fatal signals, ANRs
for the package, process death and forced activity finish.

Self-test the Android smoke summary schema checker:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_summary.ps1 -SelfTest
```

The summary checker validates the ignored JSON fields, package name, timestamp,
device/process values, evidence paths and execution flags. The real smoke helper
runs this check immediately after writing the summary.

Expected success string: `Android smoke summary check self-test OK`.

Preview the Android device smoke plan without installing or launching:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
```

The plan mode resolves the APK, adb, aapt, package name, launch activity, log
path, ignored `analysis-output\android-device-smoke.png` screenshot path,
ignored `analysis-output\android-device-smoke-summary.json` summary path, and
enabled install/launch/log-check/screenshot/summary steps without requiring a
connected phone.

Check that Android smoke plan mode and device-smoke preflight agree:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_plan_consistency.ps1
```

This compares `android_device_smoke.ps1 -PlanOnly` with
`check_android_device_preflight.ps1 -AllowNoDevice` for package, launch
activity, ignored log/screenshot/summary outputs, execution flags and summary
schema readiness.

Check Android G3 readiness directly:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_g3_readiness.ps1
```

This wraps Android device preflight, plan/preflight consistency, smoke plan,
log scanner self-test and summary schema self-test. Without an authorized
phone, it should report waiting on device.

Check that strict Android G3 readiness cannot pass without a phone:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_g3_device_requirement.ps1
```

Without an authorized phone, it should report waiting on device. With a phone,
it should report OK.

Check the public boundary for the controlled-demo metadata package:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\content-pack\check_controlled_demo_public_boundary.ps1
```

Optionally confirm the current local Windows development build is still blocked
from public packaging:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\content-pack\check_controlled_demo_public_boundary.ps1 -CheckDevBuild
```

The public boundary preflight validates only project-owned metadata examples by
default. With `-CheckDevBuild`, the current development build is expected to
return `Result: FAILED`; that confirms the dev build has not accidentally been
treated as a public-safe package.

Expected success strings:

- `MC2 demo contract validation OK`
- `MC2 PC core playable contract OK`
- `Build Finished, Result: Success`
- `MC2 Unity demo Windows build OK`
- `Windows demo launch preflight OK`
- `Windows demo build freshness check OK`
- `Controlled demo evidence check OK`
- `Controlled demo readiness preflight OK`
- `Controlled demo handoff consistency check OK`
- `Demo source hygiene check OK`
- `Android smoke artifact hygiene check OK`
- `AI deputy contract check OK`
- `PC core playable contract check OK`
- `Mobile command model preflight OK`
- `Battle HUD sparse contract check OK`
- `PC visual capture sanity check OK`
- `PC visual capture sanity self-test OK`
- `PC capture sidecar schema check OK`
- `PC capture preset contract check OK`
- `PC capture artifact hygiene check OK`
- `PC window contract check OK`
- `PC launch log hygiene check OK`
- `PC build artifact hygiene check OK`
- `PC smoke artifact hygiene check OK`
- `Current plan queue consistency check OK`
- `Optional Unity inventory bootstrap smoke check OK`
- `Android device connection check OK` or `Android device connection check waiting on device`
- `Current plan gate check OK`
- `Android SDK tooling check OK`
- `Android APK compatibility check OK`
- `Android APK signing check OK`
- `Android APK manifest check OK`
- `Android APK payload check OK`
- `Android APK size budget check OK`
- `Android smoke log check self-test OK`
- `Android smoke plan/preflight consistency check OK`
- `Android G3 readiness check OK` or `Android G3 readiness check waiting on device`
- `Android G3 device requirement check OK` or `Android G3 device requirement check waiting on device`
- `Android device smoke plan OK`
- `ScreenshotCapture: True`
- `SummaryWrite: True`
- `ConnectionCheck: check_android_device_connection.ps1 -RequireDevice`
- `Android smoke connection gate check OK`
- `Android smoke connection gate check ready for G3 device smoke` or `Android smoke connection gate check waiting on device`
- `Controlled demo public boundary preflight OK`
- `MC2 demo smoke test exiting with code 0`
- `MC2 reference visual captures passed`

Generated logs, screenshots, JSON sidecars, and Windows player builds stay under
ignored paths such as `analysis-output/` and `unity-mc2-demo/Builds/`. Do not
stage them unless a later task explicitly asks for a packaged evidence drop.

The private local reference content pack is optional for development evidence:
it can improve scale, pacing, and readability while validating the demo, but it
is not public release content. Public or commercial builds must use
project-owned or properly licensed replacement content packs.

Legacy native Windows build notes
=================================

Preparing 3rdparties:
====================

One can also use 3rdparty.zip package in the repo for simpler setup, it contains all needed 3rdparty libraries
If you select to do it then skip directly to **Compiling mc2**

zlib
----

1. Download zlib sources from here:
https://gnuwin32.sourceforge.net/packages/zlib.htm
direct link: https://gnuwin32.sourceforge.net/downlinks/zlib-src-zip.php
2. Download unistd.h for windows here:
https://gist.githubusercontent.com/mbikovitsky/39224cf521bfea7eabe9/raw/69e4852c06452a368a174ca1f0f33ce87bb52985/unistd.h
2b. Open it and comment out: `#include <getopt.h>` also comment integer types typedefs at the end
3. put it where zlib sourse files are located then open zconf.h and change `#include <unistd.h>` for `#include "unistd.h"` (or see 4.)
4. (alternative to 3) Put unistd.h to place where your compiler system headers are.
5. Open x86 Native Tools command prompt for VS2022 and cd to zlib
6.
make -f win32\Makefile.msc`
7. copy resulting .dll & .lib files to your 3rdparty folder: e.g. 3rdparty\lib\x86\
8. delete compilation files because we will now do same steps starting from step 5 but in x64 Native Tools (copy them to 3rdparty\lib\x64
9. Copy that unistd.h file to 3rdparty include folder

SDL
---

1. Download SLD x864 & x64 here: https://github.com/libsdl-org/SDL/releases/tag/release-2.30.11
direct link: https://github.com/libsdl-org/SDL/releases/download/release-2.30.11/SDL2-devel-2.30.11-VC.zip
2. copy libraries to corresponding x86 and x64 folders
3. copy headers to 3rdparty\include\SDL2 folder
4. copy files from cmake folder to 3rdparty\cmake

4. Download SLD_mixer here: https://github.com/libsdl-org/SDL_mixer/releases
direct link: https://github.com/libsdl-org/SDL_mixer/releases/download/release-2.8.0/SDL2_mixer-devel-2.8.0-VC.zip
5. do same with lib/dll and headers as with SDL (headers should also go to 3rdparty\include\SDL2\ folder)
6. copy files from cmake folder to 3rdparty\cmake

7 Download SDL2_ttf here: https://github.com/libsdl-org/SDL_ttf/releases/tag/release-2.24.0
direct link: https://github.com/libsdl-org/SDL_ttf/releases/download/release-2.24.0/SDL2_ttf-devel-2.24.0-VC.zip
8. do same with lib/dll and headers as with SDL (headers should also go to 3rdparty\include\SDL2\ folder)
9. copy files from cmake folder to 3rdparty\cmake


glew
----
From sources:
1. Download glew here: https://sourceforge.net/projects/glew/files/glew/snapshots/glew-20190928.tgz/download
2. unzip and open build\vs12\glew.sln file
3. build for x64 and Win32

Binaries:
2. Or download prebuilt lib/dlls from this page: https://glew.sourceforge.net/
3. Put lib/dll/headers accordingly to x86/x64 (headers go into 3rdparty\include\GL)


Compiling mc2
=============
```
git clone https://github.com/alariq/mc2.git
cd mc2
md build64
cd build64
cmake.exe -G "Visual Studio 17 2022" -DCMAKE_PREFIX_PATH=c:/path_to_3rdparty_folder/ -DCMAKE_LIBRARY_ARCHITECTURE=x64 ..
```
(use absolute path to 3rdparty folder)

Copy mc2.exe to your executable folder of preference (say mc2exe)
```
cd res
md build64
cd build64
cmake.exe -G "Visual Studio 17 2022" -DCMAKE_LIBRARY_ARCHITECTURE=x64 ..
```
Copy mc2res.dll/pdb to your executable folder of preference (say mc2exe)


Building data
=============
```
git clone https://github.com/alariq/mc2srcdata.git
cd mc2srcdata
```

1. Read `README.md` in `build_scripts` folder

If you did not here are the steps:
1. copy the following tools from the exe solution to `build_scripts` folder
(better copy Release version of these to make things faster):
    `aseconv`
    `makefst`
    `makersp`
    `pak`
    `text_tool`

    1a. Copy glew32.dll there as well (x86 or x64 depending on what version of tools you've built)

2. launch some console which has `make` in its path (needs GNUMake)
(you can install it from here: https://gnuwin32.sourceforge.net/packages/make.htm)
3. `cd build_scripts`
4. `make all` (or `>c:\path_to_gnumake\bin\make all`)
5. copy assets & data folder to your exe folder of preference
6. copy `*.cfg, *.fst, testtxm.tga` to your exe folder of preference


Final steps:
-----------
Copy all required dlls to your exe folder of preference

Run the game!

.. and, hopefully, enjoy

## F12 Preview Binding Checkpoint

`F12 implement opt-in inventory-to-MechBay preview binding` is complete. `F13 capture opt-in MechBay preview evidence` is complete. `F14 capture landscape-phone MechLab source-line evidence` is complete. The opt-in gate is `scripts/unity/check_optional_inventory_mechbay_preview_binding.ps1`, with expected success string `Optional inventory-to-MechBay preview binding check OK`; the preview evidence gate is `scripts/unity/capture_inventory_mechbay_preview_evidence.ps1`, with expected success string `Inventory MechBay preview evidence capture OK`; the landscape-phone evidence gate is `scripts/unity/capture_landscape_phone_mechlab_source_line_evidence.ps1`, with expected success string `Landscape-phone MechLab source-line evidence capture OK`. `F15 plan server-backed receipt slice` is complete. Evidence gate: `scripts/unity/check_server_backed_receipt_slice_plan.ps1` -> `Server-backed receipt slice plan check OK`. `F16 implement server-backed receipt evidence gate` is complete. Evidence gate: `scripts/unity/capture_server_backed_receipt_evidence.ps1` -> `Server-backed receipt evidence capture OK`. `F17 plan post-receipt inventory refresh boundary` is complete. Evidence gate: `scripts/unity/check_post_receipt_inventory_refresh_boundary.ps1` -> `Post-receipt inventory refresh boundary check OK.` `F18 implement opt-in post-receipt inventory refresh binding` is complete. Evidence gate: `scripts/unity/check_post_receipt_inventory_refresh_binding.ps1` -> `Post-receipt inventory refresh binding check OK.` `F19 capture opt-in post-receipt refresh evidence` is complete. Evidence gate: `scripts/unity/capture_post_receipt_refresh_evidence.ps1` -> `Post-receipt refresh evidence capture OK.` `F20 refresh Android landscape build/smoke evidence` is complete. `F21 audit landscape touch UI ergonomics` is complete. Evidence gate: `scripts/unity/check_landscape_touch_ui_ergonomics.ps1` -> `Landscape touch UI ergonomics check OK`. `F22 audit landscape MechLab touch controls` is complete. Evidence gate: `scripts/unity/check_landscape_mechlab_touch_controls.ps1` -> `Landscape MechLab touch controls check OK`. `F23 capture landscape MechLab touch evidence` is complete. Evidence gate: `scripts/unity/capture_landscape_mechlab_touch_evidence.ps1` -> `Landscape MechLab touch evidence capture OK`. `F24 capture Android MechLab touch evidence` is complete. Evidence gate: `scripts/unity/capture_android_mechlab_touch_evidence.ps1` -> `Android MechLab touch evidence capture OK`. F25 capture Android battle command touch evidence is complete. Evidence gate: `scripts/unity/capture_android_battle_command_touch_evidence.ps1` -> `Android battle command touch evidence capture OK`. Formal next task: `F26 reduce Android combat effect log noise`. Mobile phones remain first-version landscape-only; portrait is not a first-slice support target.

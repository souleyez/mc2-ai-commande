# Machine Handoff Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Move active development to another Windows machine without losing current source progress, Unity build reproducibility, local smoke validation, optional private reference visuals, or AI deputy configuration.

**Architecture:** Git remains the source of truth for code and public-safe metadata. Ignored build products, screenshots, logs, sidecars, Unity cache, and private reference exports are recreated or copied only as local development evidence. The new machine must first prove the clean fallback path works, then optionally restore private reference visuals for local-only readability checks.

**Tech Stack:** Windows, Git, PowerShell, Unity Hub, Unity 6000.4.7f1 with Windows and Android Build Support, Unity batchmode validator/build/smoke commands, controlled demo preflight scripts, optional MiniMax environment variables.

---

## Storage Note

The writing-plans skill normally saves plans under `docs/plans/`, but this
repository already has a tracked legacy file named `docs`. This plan therefore
uses the repository's existing root-level `docs-*.md` convention.

## Current Checkpoint

As of this handoff plan:

- Branch: `master`
- Primary project remote: `ai-origin git@github.com:souleyez/mc2-ai-commande.git`
- Previous project remote: `git@github.com:souleyez/mc2-ai-commander-demo.git` now redirects to the current repository.
- Upstream source remote kept for history: `origin https://github.com/alariq/mc2.git`
- Current branch state after the latest controlled demo checkpoint: `master...ai-origin/master`
- Latest sealed PC/mobile wait-state checkpoint: `PC1-PC57`
- Last completed PC/mobile checkpoint: landscape `G4 Touch UI pass`
- Previous mobile checkpoint retained in the gate chain: `Pass Android G3 device smoke`
- Previous mobile checkpoint retained in the gate chain: `G5 Mobile performance budget`
- Previous mobile checkpoint retained in the gate chain: `G6 iOS feasibility gate`
- Previous platform checkpoint retained in the gate chain: `F2 map authoring contract`
- Previous platform checkpoint retained in the gate chain: `F3 web ranking contract`
- Previous platform checkpoint retained in the gate chain: `F4 creator economy boundary`
- Previous platform checkpoint retained in the gate chain: `F5 server implementation boundary`
- Previous platform checkpoint retained in the gate chain: `F6 local main-server prototype`
- Previous platform checkpoint retained in the gate chain: `F7 document Unity main-server integration contract`
- Previous PC checkpoint retained in the gate chain: `Add Android ADB driver package probe`
- Previous PC checkpoint retained in the gate chain: `Add Android G3 when-ready runner`
- Previous PC checkpoint retained in the gate chain: `Add Android G3 device status report`
- Previous PC checkpoint retained in the gate chain: `Add Android ADB readiness watch`
- Previous PC checkpoint retained in the gate chain: `Add Android ADB setup guidance`
- Previous PC checkpoint retained in the gate chain: `Add Android WPD-only device diagnosis`
- Previous PC checkpoint retained in the gate chain: `Add Android visible-flow command-file smoke`
- Previous PC checkpoint retained in the gate chain: `Add Android smoke connection gate check`
- Previous PC checkpoint retained in the gate chain: `Wire Android smoke connection gate`
- Previous PC checkpoint retained in the gate chain: `Add Android device connection check`
- Previous PC checkpoint retained in the gate chain: `Add current plan queue consistency check`
- Previous PC checkpoint retained in the gate chain: `Add PC smoke artifact hygiene check`
- Previous PC checkpoint retained in the gate chain: `Add PC build artifact hygiene check`
- Previous PC checkpoint retained in the gate chain: `Add PC launch log hygiene check`
- Previous PC checkpoint retained in the gate chain: `Add PC window contract check`
- Previous PC checkpoint retained in the gate chain: `Add PC capture artifact hygiene check`
- Previous PC checkpoint retained in the gate chain: `Add PC capture preset contract check`
- Previous PC checkpoint retained in the gate chain: `Add PC capture sidecar schema check`
- Previous PC checkpoint retained in the gate chain: `Add PC visual capture sanity self-test`
- Previous PC checkpoint retained in the gate chain: `Add PC visual capture sanity check`
- Previous PC checkpoint retained in the gate chain: `Add Android G3 device requirement check`
- Previous PC checkpoint retained in the gate chain: `Add Android G3 readiness check`
- Previous PC checkpoint retained in the gate chain: `Add Android smoke plan/preflight consistency check`
- Previous PC checkpoint retained in the gate chain: `Add Android smoke summary preflight check`
- Previous PC checkpoint retained in the gate chain: `Add Android smoke summary schema check`
- Previous PC checkpoint retained in the gate chain: `Add Android smoke summary evidence output`
- Previous PC checkpoint retained in the gate chain: `Add Android smoke screenshot evidence capture`
- Previous PC checkpoint retained in the gate chain: `Add Android smoke artifact hygiene check`
- Previous PC checkpoint retained in the gate chain: `Add Android SDK tooling check`
- Previous PC checkpoint retained in the gate chain: `Add Android APK size budget check`
- Previous PC checkpoint retained in the gate chain: `Add Android APK payload check`
- Previous PC checkpoint retained in the gate chain: `Add Android APK manifest check`
- Previous PC checkpoint retained in the gate chain: `Add Android APK signing check`
- Previous PC checkpoint retained in the gate chain: `Add Android APK compatibility check`
- Previous PC checkpoint retained in the gate chain: `Add Android APK identity check`
- Previous PC checkpoint retained in the gate chain: `Add Android APK freshness check`
- Previous PC checkpoint retained in the gate chain: `Add controlled demo capture log freshness check`
- Current formal next development task after handoff: `F11 plan inventory-to-MechBay binding boundary`
- Mobile orientation decision retained for handoff: first phone version is landscape-only; portrait is not a first-version target.

Important: the new machine will not see local commits unless the old machine
first pushes to `ai-origin`, or the full repository is migrated by a trusted
local disk copy.

## Definition Of Done

The machine switch is safe only when all of these are true:

- Old machine has a clean worktree.
- Current branch is pushed to `ai-origin`, or the repository is copied whole by a trusted local transfer.
- New machine can clone or open the repository.
- Unity Editor version `6000.4.7f1` is installed, or a compatible Unity 6 editor is explicitly accepted.
- Unity validator prints `MC2 demo contract validation OK`.
- Unity Windows build prints `Build Finished, Result: Success` and `MC2 Unity demo Windows build OK`.
- Visible-flow smoke exits with `MC2 demo smoke test exiting with code 0`.
- `scripts/unity/check_controlled_demo_handoff.ps1` prints `Controlled demo handoff consistency check OK`.
- `scripts/unity/check_controlled_demo_readiness.ps1` prints `Controlled demo readiness preflight OK`.
- `scripts/unity/check_windows_demo_build_freshness.ps1` prints `Windows demo build freshness check OK`.
- `scripts/unity/check_controlled_demo_evidence.ps1` prints `Controlled demo evidence check OK` and rejects stale visible-flow/capture PNG/JSON/log evidence.
- `scripts/unity/check_demo_source_hygiene.ps1` prints `Demo source hygiene check OK`.
- `scripts/unity/check_ai_deputy_contract.ps1` prints `AI deputy contract check OK`.
- `scripts/unity/check_android_device_preflight.ps1 -AllowNoDevice` prints `Android device smoke preflight waiting on device` if no phone is connected.
- `scripts/unity/check_android_device_preflight.ps1 -AllowNoDevice` prints `smoke summary schema` and `Android smoke summary check self-test OK`.
- `scripts/unity/check_android_sdk_tooling.ps1` prints `Android SDK tooling check OK`.
- `scripts/unity/check_android_apk_freshness.ps1` prints `Android APK freshness check OK`.
- `scripts/unity/check_android_apk_identity.ps1` prints `Android APK identity check OK`.
- `scripts/unity/check_android_apk_compatibility.ps1` prints `Android APK compatibility check OK`.
- `scripts/unity/check_android_apk_signing.ps1` prints `Android APK signing check OK`.
- `scripts/unity/check_android_apk_manifest.ps1` prints `Android APK manifest check OK`.
- `scripts/unity/check_android_apk_payload.ps1` prints `Android APK payload check OK`.
- `scripts/unity/check_android_apk_size_budget.ps1` prints `Android APK size budget check OK`.
- `scripts/unity/check_android_smoke_artifact_hygiene.ps1` prints `Android smoke artifact hygiene check OK`.
- `scripts/unity/check_mobile_performance_budget.ps1` prints `Mobile performance budget check OK`.
- `scripts/unity/check_ios_feasibility_gate.ps1` prints `iOS feasibility gate check OK`.
- `scripts/unity/check_map_authoring_contract.ps1` prints `Map authoring contract check OK`.
- `scripts/unity/check_web_ranking_contract.ps1` prints `Web ranking contract check OK`.
- `scripts/unity/check_creator_economy_boundary.ps1` prints `Creator economy boundary check OK`.
- `scripts/unity/check_server_implementation_boundary.ps1` prints `Server implementation boundary check OK`.
- `scripts/server/check_local_main_server.ps1` prints `Local main-server prototype check OK`.
- `scripts/unity/check_unity_main_server_integration_contract.ps1` prints `Unity main-server integration contract check OK`.
- `scripts/unity/check_optional_unity_main_server_client_adapter.ps1` prints `Optional Unity main-server client adapter check OK`.
- `scripts/unity/check_optional_unity_main_server_launch_debrief_smoke.ps1` prints `Optional Unity main-server launch/debrief smoke check OK`.
- `scripts/unity/check_optional_unity_inventory_bootstrap_smoke.ps1` prints `Optional Unity inventory bootstrap smoke check OK`.
- `scripts/unity/check_pc_core_playable_contract.ps1` prints `PC core playable contract check OK`.
- `scripts/unity/check_mobile_command_model_preflight.ps1` prints `Mobile command model preflight OK`.
- `scripts/unity/check_mobile_landscape_contract.ps1` prints `Mobile landscape contract check OK`.
- `scripts/unity/check_battle_hud_sparse_contract.ps1` prints `Battle HUD sparse contract check OK`.
- `scripts/unity/check_pc_visual_capture_sanity.ps1` prints `PC visual capture sanity check OK`.
- `scripts/unity/check_pc_visual_capture_sanity.ps1 -SelfTest` prints `PC visual capture sanity self-test OK`.
- `scripts/unity/check_pc_capture_sidecar_schema.ps1` prints `PC capture sidecar schema check OK`.
- `scripts/unity/check_pc_capture_preset_contract.ps1` prints `PC capture preset contract check OK`.
- `scripts/unity/check_pc_capture_artifact_hygiene.ps1` prints `PC capture artifact hygiene check OK`.
- `scripts/unity/check_pc_window_contract.ps1` prints `PC window contract check OK`.
- `scripts/unity/check_pc_launch_log_hygiene.ps1` prints `PC launch log hygiene check OK`.
- `scripts/unity/check_pc_build_artifact_hygiene.ps1` prints `PC build artifact hygiene check OK`.
- `scripts/unity/check_pc_smoke_artifact_hygiene.ps1` prints `PC smoke artifact hygiene check OK`.
- `scripts/unity/check_current_plan_queue.ps1` prints `Current plan queue consistency check OK`.
- `scripts/unity/check_android_device_connection.ps1` prints `Android device connection check waiting on device` if no phone is connected.
- `scripts/unity/check_android_device_connection.ps1` prints `WpdOnlyAndroidProbe: True`.
- `scripts/unity/check_android_device_connection.ps1` prints `AdbSetupHint: True`.
- `scripts/unity/check_android_smoke_connection_gate.ps1` prints `Android smoke connection gate check OK`.
- `scripts/unity/check_android_smoke_connection_gate.ps1` prints `Android smoke connection gate check waiting on device` if no phone is connected.
- `scripts/unity/check_current_plan_gate.ps1` prints `Current plan gate check OK`.
- `scripts/unity/check_android_smoke_log.ps1 -SelfTest` prints `Android smoke log check self-test OK`.
- `scripts/unity/check_android_smoke_summary.ps1 -SelfTest` prints `Android smoke summary check self-test OK`.
- `scripts/unity/android_device_smoke.ps1 -PlanOnly` prints `Android device smoke plan OK`.
- `scripts/unity/android_device_smoke.ps1 -PlanOnly` prints `ConnectionCheck: check_android_device_connection.ps1 -RequireDevice`.
- `scripts/unity/android_device_smoke.ps1 -PlanOnly` prints `ScreenshotCapture: True` and `analysis-output\android-device-smoke.png`.
- `scripts/unity/android_device_smoke.ps1 -PlanOnly` prints `SummaryWrite: True` and `analysis-output\android-device-smoke-summary.json`.
- `scripts/unity/android_device_smoke.ps1 -PlanOnly` prints `CommandFileSmoke: True`.
- `scripts/unity/android_device_smoke.ps1 -PlanOnly` prints `UnityArguments: -mc2CommandFile`.
- `scripts/unity/android_device_smoke.ps1 -PlanOnly` prints `SmokeSuccessMarker: MC2 debrief summary assertion OK`.
- `scripts/unity/android_device_smoke.ps1 -PlanOnly` prints `SmokeSuccessMarker: MC2 loadout compact assertion OK`.
- A real Android smoke run without one authorized phone fails before install or launch with `Android device smoke requires a single authorized Android device before install or launch`.
- `scripts/unity/check_android_smoke_plan_consistency.ps1` prints `Android smoke plan/preflight consistency check OK`.
- `scripts/unity/check_android_g3_readiness.ps1` prints `Android G3 readiness check waiting on device` if no phone is connected.
- `scripts/unity/check_android_g3_device_requirement.ps1` prints `Android G3 device requirement check waiting on device` if no phone is connected.
- Any AI API key is configured through environment variables, not committed.
- Optional private reference visuals remain ignored and local-only.

## Task 1: Push Or Copy Source Checkpoint

**Files:**

- Modify: none
- Verify: Git repository state

**Step 1: Check local branch and uncommitted changes**

Run from the old machine repository root:

```powershell
git status --short --branch --untracked-files=all
git log --oneline -5
```

Expected:

```text
## master...ai-origin/master
```

There should be no modified or untracked source files unless they are part of
the handoff commit being made.

**Step 2: Push the current branch**

Preferred path:

```powershell
git push ai-origin master
```

Expected:

```text
master -> master
```

After push:

```powershell
git status --short --branch --untracked-files=all
```

Expected:

```text
## master...ai-origin/master
```

No `[ahead N]` should remain.

**Step 3: Use a local copy only if push is unavailable**

If GitHub push is blocked, copy the whole repository directory instead of only
selected files. Do not copy through a path that drops `.git`.

Do not add ignored local evidence to git just to transfer it.

**Step 4: Commit**

No code commit is required for the push itself. If this plan changes, commit
only the plan and directly related command-entry docs:

```powershell
git add README.md BUILD-WIN.md docs-ai-rts-commander-current-master-plan-2026-06-07.md docs-ai-rts-commander-current-detailed-plan-2026-06-07.md docs-machine-handoff-plan-2026-06-07.md
git commit -m "Refresh machine handoff checkpoint"
```

## Task 2: Install New Machine Tooling

**Files:**

- Modify: none
- Verify: local tools

**Step 1: Install Git**

```powershell
git --version
```

Expected: a Git version string.

**Step 2: Install Unity Hub and Unity Editor**

Install:

- Unity Hub
- Unity Editor `6000.4.7f1`
- Windows Build Support module

Verify the project version file after clone:

```powershell
Get-Content .\unity-mc2-demo\ProjectSettings\ProjectVersion.txt
```

Expected:

```text
m_EditorVersion: 6000.4.7f1
```

**Step 3: Record the Unity executable path**

Preferred default:

```powershell
$Unity = "$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe"
```

If Unity Hub installs elsewhere, set `$Unity` to that path for the local
session before running validation commands.

## Task 3: Clone And Open The Repository

**Files:**

- Modify: none
- Verify: Git clone and Unity project import

**Step 1: Clone the project-owned remote**

```powershell
git clone git@github.com:souleyez/mc2-ai-commander-demo.git
cd .\mc2-ai-commander-demo
```

If SSH is not configured on the new machine, use the GitHub HTTPS URL for the
same project-owned repository.

**Step 2: Confirm remotes**

```powershell
git remote -v
git status --short --branch --untracked-files=all
git log --oneline -5
```

Expected:

- active branch is `master`;
- worktree is clean;
- latest commit includes the current controlled demo checkpoint.

**Step 3: Create ignored output folders only when needed**

```powershell
New-Item -ItemType Directory -Force .\analysis-output | Out-Null
```

Unity will recreate `unity-mc2-demo\Library`, `Temp`, `Logs`, and build folders.
Do not copy those cache folders from the old machine unless diagnosing a Unity
import problem.

## Task 4: Prove Controlled Demo Handoff Works

**Files:**

- Read: `BUILD-WIN.md`
- Read: `unity-mc2-demo/README.md`
- Generate ignored output only: `analysis-output/`, `unity-mc2-demo/Builds/`

**Step 1: Run handoff consistency check**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1
```

Expected:

```text
Controlled demo handoff consistency check OK
```

This checks the current PC gate state, script inventory and handoff docs without
starting Unity.

**Step 2: Run full readiness preflight**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_readiness.ps1
```

Expected:

```text
Controlled demo readiness preflight OK
```

This wraps launch preflight, build freshness, evidence health and public
boundary gates. The evidence health step also rejects visible-flow logs and
capture PNG/JSON sidecars older than the current Windows build/evidence inputs.
It reads existing build/evidence outputs and does not regenerate screenshots.

**Step 3: Run Windows demo build freshness check**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_windows_demo_build_freshness.ps1
```

Expected:

```text
Windows demo build freshness check OK
```

This verifies the ignored Windows player output is newer than tracked Unity
build inputs. If it fails on a new machine, rebuild the Windows player before
using the controlled demo.

**Step 4: Run demo source hygiene check**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
```

Expected:

```text
Demo source hygiene check OK
```

This checks tracked and staged paths plus `.gitignore` markers so generated
evidence, Unity builds, APK/AAB outputs and private reference art stay out of
source commits.

**Step 5: Run AI deputy contract check**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_ai_deputy_contract.ps1
```

Expected:

```text
AI deputy contract check OK
```

This reads source, command files and docs without launching Unity or calling
MiniMax. It proves the AI deputy path is opt-in, slow, high-level, guarded by
local-rule fallback, absent from frame loops, and not used by normal visible-flow
smoke.

**Step 6: Run PC core playable contract check**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_core_playable_contract.ps1
```

Expected:

```text
PC core playable contract check OK
```

This runs the Unity/BattleCore validator and proves command-state, solo-return,
Jet legality, occupancy, damage/ejection and debrief/relaunch coverage without
launching the player or regenerating screenshots.

**Step 7: Run mobile command model preflight**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
```

Expected:

```text
Mobile command model preflight OK
```

This reads the current sidecars and source/doc markers to prove the PC demo
still maps to the mobile command model: sparse battle HUD, status rows, Jet,
map/bay/system, compact objective, hidden dense overlays and MechLab no-toggle
fitting.

**Step 7B: Run mobile landscape contract check**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_landscape_contract.ps1
```

Expected:

```text
Mobile landscape contract check OK
```

This verifies the first phone build remains landscape-only in Unity settings,
Android build settings, runtime rotation guards, APK/smoke checks and current
plan documents.

**Step 8: Run battle HUD sparse contract check**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_battle_hud_sparse_contract.ps1
```

Expected:

```text
Battle HUD sparse contract check OK
```

This reads current source and gate scripts without launching Unity, proving
normal battle still keeps status rows, compact objective, closed mission map,
hidden combat log, disabled save UI, hidden account UI, sidecar-only debug
occupancy and hidden overlays.

**Step 9: Run PC visual capture sanity check**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_visual_capture_sanity.ps1
```

Expected:

```text
PC visual capture sanity check OK
```

This verifies the six controlled-demo PNG captures are not blank, flat,
pink-box fallback images or low-information color blocks.

**Step 10: Run PC visual capture sanity self-test**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_visual_capture_sanity.ps1 -SelfTest
```

Expected:

```text
PC visual capture sanity self-test OK
```

This verifies the screenshot sanity gate catches flat and magenta fallback
sample images before accepting current evidence.

**Step 11: Run PC capture sidecar schema check**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_capture_sidecar_schema.ps1
```

Expected:

```text
PC capture sidecar schema check OK
```

This verifies the six controlled-demo JSON sidecars still point to the matching
screenshots and contain expected flow, camera, summary and reference-asset
metadata.

**Step 12: Run PC capture preset contract check**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_capture_preset_contract.ps1
```

Expected:

```text
PC capture preset contract check OK
```

This verifies the standard controlled-demo preset list stays
`mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol` across capture,
evidence, visual sanity, sidecar schema and handoff docs.

**Step 13: Run PC capture artifact hygiene check**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_capture_artifact_hygiene.ps1
```

Expected:

```text
PC capture artifact hygiene check OK
```

This verifies local reference screenshots, JSON sidecars, capture logs and PC
visual sanity self-test images remain ignored generated evidence and absent from
tracked/staged source paths.

**Step 14: Run PC window contract check**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_window_contract.ps1
```

Expected:

```text
PC window contract check OK
```

This verifies the controlled Windows demo launcher and reference capture helper
keep `1280x720` windowed defaults and pass `-screen-fullscreen 0`.

**Step 15: Run PC launch log hygiene check**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_launch_log_hygiene.ps1
```

Expected:

```text
PC launch log hygiene check OK
```

This verifies the controlled Windows demo launcher writes
`analysis-output/windows-demo-run.log`, Git ignores that path, and local launch
logs are absent from tracked/staged source paths.

**Step 16: Run PC build artifact hygiene check**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_build_artifact_hygiene.ps1
```

Expected:

```text
PC build artifact hygiene check OK
```

This verifies the Windows player output stays under
`unity-mc2-demo/Builds/Windows/`, Git ignores Unity player build outputs, and
local player builds are absent from tracked/staged source paths.

**Step 17: Run PC smoke artifact hygiene check**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_smoke_artifact_hygiene.ps1
```

Expected:

```text
PC smoke artifact hygiene check OK
```

This verifies PC smoke, validator, build and saved-account evidence outputs stay
under ignored `analysis-output/` paths and are absent from tracked/staged source
paths.

**Step 18: Run current plan queue consistency check**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_queue.ps1
```

Expected:

```text
Current plan queue consistency check OK
```

This verifies README, BUILD-WIN, master/detailed/PC/mobile/evidence/handoff docs
and helper scripts agree that the current PC/mobile package is sealed through
the PC1-PC57 checkpoint, that `Pass Android G3 device smoke`, the landscape
`G4 Touch UI pass`, `G5 Mobile performance budget`, `G6 iOS feasibility gate`
`F2 map authoring contract`, `F3 web ranking contract`,
`F4 creator economy boundary`, `F5 server implementation boundary`,
`F6 local main-server prototype`, `F7 document Unity main-server integration
contract`, `F8 implement optional Unity main-server client adapter` and
`F9 wire optional Unity main-server adapter into launch/debrief smoke` are
recorded, that `F10 wire optional Unity inventory bootstrap smoke` is recorded,
and that `F11 plan inventory-to-MechBay binding boundary` is the formal next
task.

**Step 19: Run Android device connection check**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_connection.ps1
```

Expected without a connected phone:

```text
Android device connection check waiting on device
WpdOnlyAndroidProbe: True
AdbSetupHint: True
```

This reads `adb devices -l` without installing or launching the APK. It reports
no-device, unauthorized, offline, multiple-device and ready states before G3
tries to run the real smoke. It also probes Windows PnP; when Windows only sees
the phone as WPD/MTP and adb has no rows, it can report `WpdOnlyAndroidDevice: True`.
That is still a waiting state until adb shows one authorized `device` row. The
setup hint reports current Windows driver/provider/inf/service, for example
`provider=Microsoft`, `inf=wpdmtp.inf` and `service=WUDFWpdMtp` on a Mi 11 Lite
MTP-only state. The current ADB-ready state reports `inf=winusb.inf` and
`service=WINUSB`.

**Step 19A: Run Android smoke connection gate check**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_connection_gate.ps1
```

Expected without a connected phone:

```text
Android smoke connection gate check OK
Android smoke connection gate check waiting on device
```

This proves the real Android smoke helper fails before install or launch when
no authorized phone is selected, and that existing ignored smoke log, screenshot
and summary evidence are not rewritten by that waiting-state check.

**Step 19B: Run Android ADB readiness watch**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\watch_android_device_connection.ps1 -Once -AllowWaiting
```

Expected while the phone is still WPD/MTP-only or absent from adb:

```text
Android device connection watch waiting on device
AdbWatchHint: True
```

This wraps the existing connection check in a no-install, no-launch watcher.
Use the normal timed mode when manually switching USB debugging, accepting the
RSA prompt, or changing Windows ADB drivers; it should only report ready after
adb shows one authorized `device` row.

**Step 19C: Write Android G3 device status report**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\write_android_g3_device_status.ps1
```

Expected after adb exposes one authorized device:

```text
Android G3 device status report OK
G3DeviceStatusReport: True
G3DeviceReady: True
NoInstallOrLaunch: True
```

This writes ignored `analysis-output\android-g3-device-status.json`, records the
current blocker or ready state, and remains useful diagnostic evidence after G3.

**Step 19D: Check Android ADB driver package candidates**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_adb_driver_package.ps1
```

Expected on the current ADB-ready Mi 11 Lite:

```text
Android ADB driver package probe OK
AdbDriverPackageProbe: True
CandidateDriverPackages: none
CurrentPhoneDriver: name=Mi 11 Lite; class=USBDevice; provider=Microsoft; desc=WinUsb Device; inf=winusb.inf; service=WINUSB
```

This is read-only. It does not install drivers, change PnP state, install APKs,
or launch the app.

Current G3 passed on Mi 11 Lite after phone-side USB installation was allowed.
`run_android_g3_when_ready.ps1 -TimeoutSeconds 30 -AllowWaiting -LaunchWaitSeconds 75`
installed the APK, launched the Unity activity, pushed the visible-flow command
file, captured log/screenshot/summary evidence and wrote
`status=smokePassed` to ignored `analysis-output\android-g3-when-ready-status.json`.

**Step 19E: Preview Android G3 when-ready runner**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\run_android_g3_when_ready.ps1 -PlanOnly
```

Expected:

```text
Android G3 when-ready plan OK
G3WhenReady: True
NoInstallOrLaunchUntilDeviceReady: True
```

This proves the real G3 entry will wait for adb readiness before calling
`android_device_smoke.ps1`. The preview path does not install or launch.

**Step 20: Run current plan gate check**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
```

Expected:

```text
Current plan gate check OK
```

This wraps handoff/readiness, Windows build freshness, demo source hygiene, AI
deputy contract, mobile command model, battle HUD sparse contract, PC visual capture sanity, PC visual capture sanity self-test, PC capture sidecar schema, PC capture preset contract, PC capture artifact hygiene, PC window contract, PC launch log hygiene, PC build artifact hygiene, PC smoke artifact hygiene, current plan queue consistency, Android device connection, Android WPD-only device diagnosis, Android ADB setup guidance, Android ADB driver package probe, Android ADB readiness watch, Android G3 device status report, Android G3 when-ready runner, Android smoke connection gate and Android
preflight checks. With no authorized phone connected, Android should be
reported as waiting on device; with one authorized phone, Android should report
OK. If the phone then rejects installation, the blocker is phone-side USB
install permission, not adb connectivity.

**Step 21: Self-test Android smoke log scanning**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_log.ps1 -SelfTest
```

Expected:

```text
Android smoke log check self-test OK
```

The real device smoke helper calls this scanner after logcat capture, so a
device launch with fatal exception, fatal signal, ANR, package process death or
forced activity finish is not accepted as a pass.

**Step 22: Self-test Android smoke summary schema**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_summary.ps1 -SelfTest
```

Expected:

```text
Android smoke summary check self-test OK
```

The real device smoke helper calls this checker after writing
`analysis-output\android-device-smoke-summary.json`, so incomplete or malformed
summary evidence is not accepted as a pass.

**Step 23: Preview Android device smoke plan**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
```

Expected:

```text
Android device smoke plan OK
ConnectionCheck: check_android_device_connection.ps1 -RequireDevice
```

This proves the real device-smoke helper can resolve the APK, adb, aapt,
package, activity, log path, planned install/launch/log-check actions and the
strict connection gate before a phone is connected.

**Step 24: Check Android smoke plan/preflight consistency**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_plan_consistency.ps1
```

Expected:

```text
Android smoke plan/preflight consistency check OK
```

This proves plan mode and direct G3 preflight agree on package, activity,
ignored evidence paths, execution flags and summary schema readiness before a
phone is connected.

**Step 25: Run Android G3 readiness directly**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_g3_readiness.ps1
```

Expected with no connected phone:

```text
Android G3 readiness check waiting on device
```

This wraps device preflight, plan/preflight consistency, plan mode, log scanner
self-test and summary schema self-test. It does not install or launch the app.

**Step 26: Confirm strict G3 device requirement**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_g3_device_requirement.ps1
```

Expected with no connected phone:

```text
Android G3 device requirement check waiting on device
```

This proves strict G3 readiness cannot be accepted without an authorized
Android phone.

**Step 27: Run Android device-smoke preflight directly if needed**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
```

Expected with no connected phone:

```text
Android device smoke preflight waiting on device
smoke summary schema
Android smoke summary check self-test OK
```

Expected with one authorized phone:

```text
Android device smoke preflight OK
```

This checks the APK, adb, aapt, package name and launchable activity without
installing or launching the app.

**Step 28: Set paths for rebuilding evidence if needed**

```powershell
$Repo = (Get-Location).Path
$Unity = "$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe"
```

If `$Unity` does not exist, point it to the installed Unity editor path.

**Step 29: Run validator when rebuilding or auditing from scratch**

```powershell
& $Unity `
  -batchmode -quit `
  -projectPath "$Repo\unity-mc2-demo" `
  -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract `
  -logFile "$Repo\analysis-output\unity-validate-machine-handoff.log"
```

Expected log string:

```text
MC2 demo contract validation OK
```

**Step 30: Build Windows player when rebuilding or auditing from scratch**

```powershell
& $Unity `
  -batchmode -quit `
  -projectPath "$Repo\unity-mc2-demo" `
  -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 `
  -logFile "$Repo\analysis-output\unity-build-machine-handoff.log"
```

Expected log strings:

```text
Build Finished, Result: Success
MC2 Unity demo Windows build OK
```

**Step 31: Run visible-flow smoke without AI key when rebuilding or auditing from scratch**

```powershell
$env:MINIMAX_API_KEY = ""
& "$Repo\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" `
  -batchmode -nographics -mc2SmokeTest `
  -mc2CommandFile "$Repo\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" `
  -logFile "$Repo\analysis-output\unity-player-machine-handoff.log"
```

Expected log string:

```text
MC2 demo smoke test exiting with code 0
```

## Task 5: Restore Optional Private Reference Visuals

**Files:**

- Optional local-only copy: `mc2-run64-dev/`
- Optional local-only copy: `runtime-shell-dev/`
- Optional local-only copy: `content-packs/project-owned-linked-dev/`
- Optional local-only copy: `analysis-output/unity-reference-art/`
- Optional local-only copy: `unity-mc2-demo/Assets/PrivateReferenceArt/`

**Step 1: Decide whether the new machine needs local reference visuals**

For normal source development, skip this task. The Unity loader has a fallback
path and should run without private reference art.

For investor-style local evidence or visual readability work, restore the
private reference pack from a trusted local transfer. Keep it out of Git.

**Step 2: Confirm ignored paths stay ignored**

```powershell
git status --short --branch --untracked-files=all
```

Expected: copying optional private reference assets should not create staged or
visible tracked changes.

**Step 3: Re-run capture only when visual evidence is needed**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass `
  -File .\scripts\unity\capture_reference_visuals.ps1 `
  -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol
```

Expected:

```text
MC2 reference visual captures passed
```

## Task 6: Restore Optional AI Deputy Environment

**Files:**

- Modify: none
- Local secret only: user environment variables

**Step 1: Keep normal validation no-key**

Validator and smoke should pass without any AI key. This protects local
development from token spend and API latency.

**Step 2: Set MiniMax only for explicit AI deputy tests**

```powershell
[Environment]::SetEnvironmentVariable("MINIMAX_API_KEY", "<your-api-key>", "User")
[Environment]::SetEnvironmentVariable("MINIMAX_BASE_URL", "https://api.minimaxi.com/v1", "User")
[Environment]::SetEnvironmentVariable("MINIMAX_MODEL", "MiniMax-M2.5", "User")
```

Never write these values into repo files, logs intended for commit, or public
documentation.

## Task 7: Resume Development After Handoff

**Files:**

- Read: `docs-ai-rts-commander-current-master-plan-2026-06-07.md`
- Read: `docs-ai-rts-commander-current-detailed-plan-2026-06-07.md`
- Read: `docs-mobile-first-plan-2026-06-10.md`
- Next planned work: `F11 plan inventory-to-MechBay binding boundary`

**Step 1: Confirm current next task**

Read the current commit queue. After this handoff, the product work should
resume at:

```text
F11 plan inventory-to-MechBay binding boundary
```

**Step 2: Do not start with a full remote platform**

Mobile proof, server implementation boundary, the local main-server prototype,
the Unity integration contract and the optional Unity client boundary are
already recorded. The next product work should wire that boundary into an
explicit opt-in launch/debrief smoke for the existing local server endpoints:

- account id;
- token ledger;
- inventory snapshot;
- signed squad loadout;
- reward claim endpoint;
- basic leaderboard.

Android Build Support, Android APK build smoke, real Android device smoke,
landscape touch UI and the first mobile performance budget have already been
handled on the old machine. If the new machine cannot produce or run the APK,
fix the local Unity Android module/toolchain before changing gameplay.

Map package/editor contracts, Web ranking, creator economy, server
implementation boundaries, the F6 local prototype and the F7 Unity integration
contract are now documented. F8 should keep Unity offline-first while adding an
optional request/response adapter with local fallback.

**Step 3: Commit next product work in one small commit**

The next commit should be the smallest change that advances the current queue,
for example a Unity/server integration contract checkpoint:

```powershell
git add unity-mc2-demo/Assets/Scripts README.md docs-ai-rts-commander-current-master-plan-2026-06-07.md docs-ai-rts-commander-current-detailed-plan-2026-06-07.md
git commit -m "Implement optional Unity main-server client adapter"
```

## Stop Conditions

Stop before continuing product development if:

- new machine cannot see the latest handoff commit;
- branch is still behind the old machine;
- Unity version mismatch creates import or build errors;
- validator fails;
- Windows player does not build;
- visible-flow smoke does not exit with code 0;
- private reference assets appear as tracked Git changes;
- AI key is needed for normal local validation;
- any generated screenshot, log, JSON sidecar, Unity build output, or private reference export appears in the staged diff.

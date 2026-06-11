# Mobile First Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Make Android and iOS viability the first product priority after machine handoff, proving the current Unity 6 tactical commander demo can become a mobile-first game before expanding map-server or creator-platform work.

**Architecture:** Keep `BattleCore` platform-neutral and deterministic. Unity remains the runtime presentation layer, but the next work shifts from Windows-only validation to mobile build, touch input, UI scaling, performance budgets, package size, and device smoke tests. Server, map editor, ranking, and creator-economy contracts remain important but are deferred until the mobile feasibility gate is proven.

**Tech Stack:** Unity 6, C#, BattleCore, Android Build Support, Android SDK/NDK/OpenJDK from Unity Hub, later iOS/Xcode on macOS, PowerShell build scripts, ignored device logs and capture evidence.

---

## Product Decision

Mobile support is the first priority.

Do not move the main game to Unreal for now. Unreal MCP can remain a later tool
research topic, but it is not part of the current execution queue.

After machine handoff validation, the next product work is:

```text
Mobile viability spike -> Android build smoke -> touch command UI -> mobile performance budget
```

Map authoring, Web ranking, creator economy, and server implementation move
behind this mobile gate.

## Current Waiting State

G2 Android build smoke is complete and the APK exists in ignored build output.
G3 Android device-smoke preflight now verifies the APK, Android SDK tooling,
adb, aapt, apksigner, package name, launchable activity, compatibility metadata,
signing and manifest install-target metadata, Unity/IL2CPP runtime payload,
APK size budget, Android smoke artifact hygiene, Android smoke screenshot
evidence capture, Android smoke summary evidence output, Android smoke
summary schema check, Android smoke summary preflight check and Android smoke
plan/preflight consistency check, Android G3 readiness check and Android G3 device requirement check. PC wait-state capture evidence is additionally guarded and self-tested by `check_pc_visual_capture_sanity.ps1`. The real G3 device smoke is still waiting on a physical Android
phone that is visible through `adb devices` and authorized for USB debugging.

While G3 is waiting, the active project work may continue on PC demo
optimization as defined in `docs-pc-optimization-plan-2026-06-11.md`. This does
not advance G4/G5 ahead of G3; it only keeps Windows demo quality moving while
the required phone is unavailable. The current PC/mobile waiting-state work is
sealed through PC39, including the PC core playable contract check, mobile
command model preflight, battle HUD sparse contract check, demo source hygiene
check, AI deputy contract check, Windows demo build freshness check, controlled
demo evidence freshness check, controlled demo capture log freshness check,
Android SDK tooling check, Android APK freshness check, Android APK identity check, Android APK
compatibility check, Android APK signing check, Android APK manifest check,
Android APK payload check, Android APK size budget check, Android smoke artifact hygiene check, Android smoke screenshot evidence capture, Android smoke summary evidence output, Android smoke summary schema check, Android smoke summary preflight check, Android smoke plan/preflight consistency check, Android G3 readiness check, Android G3 device requirement check, PC visual capture sanity check, PC visual capture sanity self-test, current plan gate
check, Android smoke log crash scan and Android smoke plan mode.

## Definition Of Done

The mobile-first gate is passed only when:

- Unity project builds an Android APK/AAB from CI-like command or documented Editor path.
- A physical Android device can launch the demo.
- The player can complete the visible-flow path with touch-oriented controls or command-file smoke.
- Battle UI remains sparse and readable on a phone aspect ratio.
- Fixed tactical camera, status rows, Jet, objective/map/system buttons, and MechLab fit grid are usable on touch.
- Average battle FPS, memory, load time, package size, and battery/thermal observations are recorded.
- BattleCore behavior remains the same as Windows validator/smoke.
- No generated device logs, APK/AAB, screenshots, JSON sidecars, private reference assets, or Unity build outputs are staged.

iOS is planned after Android proves the Unity mobile path. It requires a macOS
machine with Xcode and Apple signing. Do not block Android feasibility on iOS
signing work.

## Execution Gate Order

Run these gates in order. Do not start a later gate while an earlier gate is
failing, unless the later work is explicitly diagnostic.

| Gate | Status | Purpose | Required Before Next Gate |
| --- | --- | --- | --- |
| H2 | Done | New-machine baseline | Clone repo, run Windows validator/build/smoke on new machine |
| G2 | Done | Android build path | Produce Android artifact from Unity 6 without staging generated output |
| G3 | Waiting on Device | Android device smoke | Launch on real Android device and reach battle/debrief path |
| G4 | Later | Touch UI pass | Core command model usable on phone aspect ratios |
| G5 | Later | Mobile performance budget | Baseline FPS, memory, package size, load time and thermal notes recorded |
| G6 | Later | iOS feasibility | Document macOS/Xcode/signing requirements after Android proof |

## Executable Requirement Matrix

### How To Treat An Executable Target

每个目标必须能被下一位开发者直接执行，不依赖口头记忆。目标写法固定为：

- **Precondition:** 开始前必须满足的环境、分支、模块和干净工作区条件。
- **Action:** 要改的文件或要运行的命令。
- **Output:** 期望生成的文件、日志、截图、sidecar 或文档。
- **Verification:** 用哪条命令确认通过，日志里必须出现什么稳定字符串。
- **Failure Handling:** 失败时先看什么、停在哪里、哪些输出不能提交。
- **Commit Scope:** 允许进入提交的文件范围；生成物、日志和私有素材默认不提交。

当前移动执行目标只允许有一个 `In Progress` 或 `Waiting on Device`。如果前置条件失败，先把失败写成明确 blocker 或安装步骤，不跳到后续移动玩法任务。项目总执行线在 G3 等设备期间可转到 `docs-pc-optimization-plan-2026-06-11.md`。

### Current Mobile Target: G3 Android Device Smoke

**Precondition:**

- `git status --short --branch --untracked-files=all` 干净。
- Android APK exists at `unity-mc2-demo\Builds\Android\MC2UnityDemo.apk` and passes `check_android_sdk_tooling.ps1`, `check_android_apk_freshness.ps1`, `check_android_apk_identity.ps1`, `check_android_apk_compatibility.ps1`, `check_android_apk_signing.ps1`, `check_android_apk_manifest.ps1`, `check_android_apk_payload.ps1`, `check_android_apk_size_budget.ps1`, `check_android_smoke_artifact_hygiene.ps1`, the summary schema check inside `check_android_device_preflight.ps1`, `check_android_smoke_plan_consistency.ps1`, `check_android_g3_readiness.ps1`, and `check_android_g3_device_requirement.ps1`.
- `adb` exists at `$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe`.
- One physical Android device has USB debugging enabled and is trusted by this PC.

**Action:**

1. Run `scripts\unity\check_android_device_preflight.ps1`.
2. If no device row is shown, `check_android_device_preflight.ps1 -AllowNoDevice` should still prove the APK/tooling/package/summary-schema path and then stop at waiting-on-device; `check_android_smoke_plan_consistency.ps1` and `check_android_g3_readiness.ps1` should also prove the no-install G3 readiness bundle; `check_android_g3_device_requirement.ps1` should prove strict readiness still requires a phone; connect/authorize a phone before real G3.
3. If a device is present, run `scripts\unity\android_device_smoke.ps1`; it installs the APK, launches it, waits briefly and captures ignored `analysis-output/android-device-smoke.log`.
4. Record whether the app reaches battle/debrief manually or by command-file smoke.

**Output:**

- ignored device log under `analysis-output/`;
- optional short docs note with device model, Android version, install result and smoke result;
- no APK/log/screenshot sidecar staged.

**Verification:**

```powershell
git diff --check
git status --short --branch --untracked-files=all
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_plan_consistency.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_g3_readiness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_g3_device_requirement.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1
```

**Failure Handling:**

- no device row: stop and connect/authorize a physical phone;
- install failure: inspect adb output and Android package signing/SDK compatibility before changing gameplay;
- launch crash: inspect `android-device-smoke.log`, fix the smallest runtime blocker, rebuild APK, reinstall;
- generated logs/screenshots/APK appear in git: leave ignored or unstage, never commit them.

**Current G3 Evidence 2026-06-12:**

```text
unity-mc2-demo\Builds\Android\MC2UnityDemo.apk exists.
scripts\unity\android_device_smoke.ps1 exists and fails clearly when no device is connected.
scripts\unity\check_android_sdk_tooling.ps1 -> Android SDK tooling check OK.
scripts\unity\check_android_apk_freshness.ps1 -> Android APK freshness check OK.
scripts\unity\check_android_apk_identity.ps1 -> Android APK identity check OK.
scripts\unity\check_android_apk_compatibility.ps1 -> Android APK compatibility check OK.
scripts\unity\check_android_apk_signing.ps1 -> Android APK signing check OK.
scripts\unity\check_android_apk_manifest.ps1 -> Android APK manifest check OK.
scripts\unity\check_android_apk_payload.ps1 -> Android APK payload check OK.
scripts\unity\check_android_apk_size_budget.ps1 -> Android APK size budget check OK.
scripts\unity\check_android_smoke_artifact_hygiene.ps1 -> Android smoke artifact hygiene check OK.
scripts\unity\check_android_smoke_summary.ps1 -SelfTest -> Android smoke summary check self-test OK.
scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice -> smoke summary schema OK.
scripts\unity\android_device_smoke.ps1 -PlanOnly -> ScreenshotCapture: True, Screenshot -> analysis-output\android-device-smoke.png.
scripts\unity\android_device_smoke.ps1 -PlanOnly -> SummaryWrite: True, Summary -> analysis-output\android-device-smoke-summary.json.
scripts\unity\check_android_smoke_plan_consistency.ps1 -> Android smoke plan/preflight consistency check OK.
scripts\unity\check_android_g3_readiness.ps1 -> Android G3 readiness check waiting on device.
scripts\unity\check_android_g3_device_requirement.ps1 -> Android G3 device requirement check waiting on device.
scripts\unity\check_pc_visual_capture_sanity.ps1 -> PC visual capture sanity check OK.
scripts\unity\check_pc_visual_capture_sanity.ps1 -SelfTest -> PC visual capture sanity self-test OK.
scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice -> Android device smoke preflight waiting on device.
APK package -> com.DefaultCompany.unitymc2demo.
APK activity -> com.unity3d.player.UnityPlayerGameActivity.
APK compatibility -> minSdkVersion 25, targetSdkVersion 36, native-code arm64-v8a.
APK signing -> apksigner verify OK, v2 scheme true, signer DN C=US, O=Android, CN=Android Debug.
APK manifest -> allowed permissions, no required hardware features, touchscreen/vulkan not-required, small/normal/large/xlarge screens.
APK payload -> required Unity/IL2CPP native libraries, assets/bin/Data runtime files and arm64-v8a ABI are present.
APK size budget -> current APK is within the 1 MiB to 100 MiB early mobile demo budget.
Android SDK tooling -> AndroidPlayer SDK, NDK, OpenJDK, build-tools 36.0.0, android-36, adb, aapt and apksigner are present.
Android smoke artifact hygiene -> APK/AAB outputs, Android smoke logs/screenshots and Builds/Android outputs remain ignored and absent from tracked/staged paths.
Android smoke screenshot evidence -> PlanOnly reports ScreenshotCapture: True and the ignored target analysis-output\android-device-smoke.png.
Android smoke summary evidence -> PlanOnly reports SummaryWrite: True and the ignored target analysis-output\android-device-smoke-summary.json.
Android smoke summary schema -> SelfTest reports Android smoke summary check self-test OK.
Android smoke summary preflight -> device preflight reports smoke summary schema OK before waiting on device.
Android smoke plan/preflight consistency -> plan mode and preflight agree on package, activity, evidence paths, execution flags and summary schema readiness.
Android G3 readiness -> direct readiness gate reports waiting on device after passing preflight, plan consistency, plan mode, log scanner and summary schema checks.
Android G3 device requirement -> strict readiness is not accepted without an authorized Android phone.
adb devices -> no device rows.
G3 still requires a physical Android phone with USB debugging enabled and authorized.
```

**Commit Scope:**

- Allowed: docs, smoke helper source if needed, minimal Unity source if Android runtime exposes a real blocker.
- Not allowed: `analysis-output/`, `unity-mc2-demo/Builds/`, generated screenshots, JSON sidecars, private reference exports.

### H2: New-Machine Baseline

| ID | Requirement | Output | Verification |
| --- | --- | --- | --- |
| H2-R1 | New machine can clone the project-owned GitHub repository | Clean worktree on `master` | `git status --short --branch --untracked-files=all` shows `## master...origin/master` or equivalent clean upstream |
| H2-R2 | Latest mobile-first commit is visible | `3114d93 Reframe plan around mobile first` appears in log or newer commit includes it | `git log --oneline -5` |
| H2-R3 | Unity project opens with editor version `6000.4.7f1` or accepted compatible Unity 6 version | Unity import completes without source changes | `Get-Content .\unity-mc2-demo\ProjectSettings\ProjectVersion.txt` and `git status` |
| H2-R4 | Windows validator still passes before mobile work begins | `analysis-output/unity-validate-machine-handoff.log` | log contains `MC2 demo contract validation OK` |
| H2-R5 | Windows player still builds before mobile work begins | `unity-mc2-demo/Builds/Windows/MC2UnityDemo.exe` ignored output | log contains `Build Finished, Result: Success` and `MC2 Unity demo Windows build OK` |
| H2-R6 | Visible-flow smoke still passes no-key | ignored player log | log contains `MC2 demo smoke test exiting with code 0` |

H2 is complete only when all H2 requirements pass. If H2 fails, fix the
environment or Windows baseline first; do not start Android changes.

### G2: Android Build Smoke

| ID | Requirement | Output | Verification |
| --- | --- | --- | --- |
| G2-R0 | Worktree and Windows baseline are clean before Android work | clean status and validator log | `git status --short --branch --untracked-files=all`; log contains `MC2 demo contract validation OK` |
| G2-R1 | Unity Android Build Support is installed with SDK, NDK and OpenJDK | Unity module exists on the machine | Unity Hub module list or successful Android build |
| G2-R2 | Android build target can be selected without changing tracked project files unexpectedly | no unrelated project settings churn | `git status --short --branch --untracked-files=all` before staging |
| G2-R3 | Add or document repeatable Android build path | `BuildAndroid` editor method or exact manual Editor steps | `unity-mc2-demo/README.md` or build helper documents the path |
| G2-R4 | Android artifact is generated into ignored output | APK or AAB under ignored build folder | file exists, not staged |
| G2-R5 | Android build log has a stable success string | ignored log under `analysis-output/` | log contains the exact string chosen for future regression checks |
| G2-R6 | Windows validator still passes after Android build setup | validator log | `MC2 demo contract validation OK` |
| G2-R7 | Generated Android output remains outside git | no staged APK/AAB/build/log files | `git status --short --branch --untracked-files=all` shows only intentional source/doc changes or clean status |
| G2-R8 | G3 can start without rediscovering build path | `BUILD-MOBILE.md` has artifact path and adb install command | next task can run `adb install -r .\unity-mc2-demo\Builds\Android\MC2UnityDemo.apk` |

Initial preferred command, once `BuildAndroid` exists:

```powershell
$Repo = (Get-Location).Path
$Unity = "$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe"
& $Unity `
  -batchmode -quit `
  -projectPath "$Repo\unity-mc2-demo" `
  -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildAndroid `
  -logFile "$Repo\analysis-output\unity-build-android.log"
```

Expected stable success string to add when implemented:

```text
MC2 Unity demo Android build OK
```

G2 is complete only when an Android artifact is produced and no generated build
output is staged.

**Completed Evidence 2026-06-11:**

```text
analysis-output/unity-validate-mobile-baseline.log: MC2 demo contract validation OK
analysis-output/unity-build-android.log: Build Finished, Result: Success.
analysis-output/unity-build-android.log: MC2 Unity demo Android build OK: ...\unity-mc2-demo\Builds\Android\MC2UnityDemo.apk
unity-mc2-demo\Builds\Android\MC2UnityDemo.apk exists, 20,666,724 bytes, ignored output.
```

### G3: Android Device Smoke

Minimum device target for the first pass:

- one physical Android phone;
- Android 10 or newer preferred;
- 4 GB RAM minimum, 6 GB RAM preferred;
- USB debugging enabled;
- enough free storage for APK install and logs.

| ID | Requirement | Output | Verification |
| --- | --- | --- | --- |
| G3-R1 | Device is visible through adb | device id | `adb devices` shows one `device` row |
| G3-R1a | Device-smoke preflight can prove APK/tooling/package readiness before install | preflight rows | `check_android_device_preflight.ps1 -AllowNoDevice` reports waiting on device, with APK, Android SDK tooling, APK freshness, APK identity, APK compatibility, APK signing, APK manifest, APK payload, APK size budget, Android smoke artifact hygiene, Android smoke summary schema, adb, aapt, apksigner, package and activity OK |
| G3-R1j | Android SDK tooling is present before install | tooling output | `check_android_sdk_tooling.ps1` reports `Android SDK tooling check OK` for Unity AndroidPlayer SDK, NDK, OpenJDK, build-tools, platform, adb, aapt and apksigner |
| G3-R1c | Android APK is not stale before install | freshness output | `check_android_apk_freshness.ps1` reports `Android APK freshness check OK` |
| G3-R1d | Android APK identity matches the expected launch path | identity output | `check_android_apk_identity.ps1` reports `Android APK identity check OK` for package `com.DefaultCompany.unitymc2demo` and activity `com.unity3d.player.UnityPlayerGameActivity` |
| G3-R1e | Android APK compatibility matches the expected device target | compatibility output | `check_android_apk_compatibility.ps1` reports `Android APK compatibility check OK` for min SDK 25, target SDK 36 and native-code `arm64-v8a` |
| G3-R1f | Android APK signing verifies before install | signing output | `check_android_apk_signing.ps1` reports `Android APK signing check OK` with APK Signature Scheme v2 and debug signer DN |
| G3-R1g | Android APK manifest keeps install targets broad enough | manifest output | `check_android_apk_manifest.ps1` reports `Android APK manifest check OK` for permissions, no required features, not-required touchscreen/vulkan and screen support |
| G3-R1h | Android APK runtime payload is present before install | payload output | `check_android_apk_payload.ps1` reports `Android APK payload check OK` for Unity/IL2CPP native libraries, `assets/bin/Data` files and `arm64-v8a` ABI |
| G3-R1i | Android APK package size stays within the early mobile demo budget | size budget output | `check_android_apk_size_budget.ps1` reports `Android APK size budget check OK` for the 1 MiB to 100 MiB budget |
| G3-R1k | Android smoke outputs stay out of source control | artifact hygiene output | `check_android_smoke_artifact_hygiene.ps1` reports `Android smoke artifact hygiene check OK` for ignored APK/AAB outputs, Android smoke logs/screenshots and `Builds/Android` paths |
| G3-R1l | Android smoke can capture startup screenshot evidence | plan/screenshot output | `android_device_smoke.ps1 -PlanOnly` reports `ScreenshotCapture: True`; real device smoke writes ignored `analysis-output\android-device-smoke.png` |
| G3-R1m | Android smoke can write run summary evidence | plan/summary output | `android_device_smoke.ps1 -PlanOnly` reports `SummaryWrite: True`; real device smoke writes ignored `analysis-output\android-device-smoke-summary.json` |
| G3-R1n | Android smoke summary schema is checked | summary self-test/real smoke output | `check_android_smoke_summary.ps1 -SelfTest` reports `Android smoke summary check self-test OK`; real device smoke validates the summary after writing it |
| G3-R1o | Device-smoke preflight includes summary schema | preflight output | `check_android_device_preflight.ps1 -AllowNoDevice` reports `smoke summary schema` and `Android smoke summary check self-test OK` before waiting on device |
| G3-R1p | Device-smoke plan and preflight agree before install | consistency output | `check_android_smoke_plan_consistency.ps1` reports `Android smoke plan/preflight consistency check OK` for package, activity, evidence paths, execution flags and summary schema readiness |
| G3-R1q | Direct G3 readiness bundle passes before install | readiness output | `check_android_g3_readiness.ps1` reports `Android G3 readiness check waiting on device` when no authorized phone is connected, after preflight, plan consistency, plan mode, log scanner and summary schema checks pass |
| G3-R1r | Strict G3 readiness requires a real device | device requirement output | `check_android_g3_device_requirement.ps1` reports `Android G3 device requirement check waiting on device` when no authorized phone is connected and OK only after strict readiness sees a phone |
| G3-R1b | Device-smoke helper can preview planned actions without a device | plan output | `android_device_smoke.ps1 -PlanOnly` reports package, activity, log path and install/launch/log-check actions |
| G3-R2 | APK installs cleanly | installed package | `adb install -r <apk>` returns success |
| G3-R3 | App launches without immediate crash | app process/log | `adb logcat` has no fatal crash during launch |
| G3-R3a | Device smoke log is scanned for strong crash markers | scanner output | `check_android_smoke_log.ps1` reports OK after logcat capture |
| G3-R4 | Battle scene is reachable | manual note or smoke log | launch reaches battle mode |
| G3-R5 | Core visible-flow path is reachable | ignored smoke note/log | battle -> debrief -> repair/MechLab -> relaunch path observed or command-file smoke passes |
| G3-R6 | No AI key is required for mobile smoke | no key in environment or app config | no MiniMax API call required for pass |
| G3-R7 | Device logs remain ignored | logs under `analysis-output/` | `git status` does not show logs unless intentionally summarized in docs |

Recommended commands:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_sdk_tooling.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_freshness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_identity.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_compatibility.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_signing.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_manifest.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_payload.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_size_budget.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_artifact_hygiene.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_summary.ps1 -SelfTest
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_plan_consistency.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_g3_readiness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_g3_device_requirement.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1
git status --short --branch --untracked-files=all
```

G3 is complete when a real phone can launch and reach the core battle/debrief
path. If command-file automation is not available on Android yet, a manual smoke
note is acceptable for this gate, but the next iteration should automate it.

### G4: Touch Command UI

The mobile command model must stay simple. Do not introduce drag-box selection.
Do not add a dense battle log.

| ID | Requirement | Pass Standard |
| --- | --- | --- |
| G4-R1 | Default state controls the full squad | tapping ground issues squad move |
| G4-R2 | Tapping a hostile target issues squad attack/focus | squad target changes and target cue is visible |
| G4-R3 | Tapping a mech status row selects one mech for detached order | selected mech enters single-order state |
| G4-R4 | Detached mech ignores later squad order until its solo order completes | solo isolation still matches current design |
| G4-R5 | Detached mech auto-rejoins after order completion | status row returns to squad state |
| G4-R6 | Jet button is usable on phone | valid Jet moves; illegal landing keeps blocked mech still |
| G4-R7 | Mission map/objective control is usable and closable | map does not cover core command flow permanently |
| G4-R8 | System/pause panel is usable | restart/end/AI takeover shell remains accessible |
| G4-R9 | MechLab grid remains usable on touch | select mounted weapon, choose target cell, place/reset/apply, cycle armor/sink/clear |
| G4-R10 | Text does not overlap at common phone aspect ratios | check at 16:9, 19.5:9 and 20:9 layouts |
| G4-R11 | Battle HUD remains sparse | no large combat log, save-slot UI, account UI or debug overlay in active battle |

Initial UI target:

- status row touch target: at least 44 logical pixels high where practical;
- primary action buttons: at least 44 logical pixels high where practical;
- no text should be required to read below normal phone legibility at 1080p;
- if a text label cannot fit, shorten the label before adding another panel.

G4 is complete when the command loop can be performed by touch without relying
on keyboard, mouse hover, or drag selection.

### G5: Mobile Performance Budget

First budget is a decision gate, not a final optimization target. Record facts
before optimizing.

| Metric | Prototype Proceed Target | Investigate If Worse Than |
| --- | --- | --- |
| Battle FPS | playable and mostly stable at 30 FPS target | sustained under 25 FPS in first mission |
| App launch time | record baseline | over 45 seconds |
| Battle load time | record baseline | over 15 seconds after app is open |
| Memory | record baseline | over 1.5 GB on first mission |
| APK/AAB size | record baseline | over 500 MB before real final art/audio |
| Thermal | no obvious runaway heat in short smoke | device throttles or becomes uncomfortable in under 10 minutes |
| Battery | record manual note | severe drain visible in short smoke |

Recommended evidence:

```powershell
adb shell dumpsys meminfo <package-name> > .\analysis-output\android-meminfo.txt
adb shell dumpsys gfxinfo <package-name> > .\analysis-output\android-gfxinfo.txt
adb logcat -d > .\analysis-output\android-performance-logcat.txt
Get-Item .\unity-mc2-demo\Builds\Android\*.apk
```

G5 is complete when `docs-mobile-performance-budget-2026-06-10.md` records the
device model, Android version, artifact size, launch/load notes, FPS/gfxinfo,
memory, and thermal/battery observation.

### G6: iOS Feasibility Gate

Do not start iOS before Android artifact and device smoke pass.

| ID | Requirement | Output |
| --- | --- | --- |
| G6-R1 | Identify macOS build machine | note machine and macOS version |
| G6-R2 | Identify Xcode requirement | note installed/required Xcode version |
| G6-R3 | Identify Apple developer/signing state | note whether account and profiles exist |
| G6-R4 | Confirm Unity iOS module availability | note Unity Hub module status |
| G6-R5 | Define first iOS smoke | build Xcode project, install to device, launch battle |

G6 is complete when iOS blockers are explicit enough that we can schedule them
instead of discovering them during core gameplay work.

## Task 1: Reframe Current Plans Around Mobile Priority

**Files:**

- Modify: `README.md`
- Modify: `docs-ai-rts-commander-current-master-plan-2026-06-07.md`
- Modify: `docs-ai-rts-commander-current-detailed-plan-2026-06-07.md`
- Modify: `docs-machine-handoff-plan-2026-06-07.md`
- Create: `docs-mobile-first-plan-2026-06-10.md`

**Steps:**

1. Mark mobile support as first priority.
2. Move map authoring/web/creator contracts behind the mobile gate.
3. Remove the old contradiction that first scope excludes all mobile adaptation.
4. Keep realtime PVP, accounts, recharge, chain, and production server work out of the first mobile gate.
5. Run:

```powershell
git diff --check
```

6. Commit:

```powershell
git add README.md docs-ai-rts-commander-current-master-plan-2026-06-07.md docs-ai-rts-commander-current-detailed-plan-2026-06-07.md docs-machine-handoff-plan-2026-06-07.md docs-mobile-first-plan-2026-06-10.md
git commit -m "Reframe plan around mobile first"
```

## Task 2: Prove Android Build Tooling

**Files:**

- Modify if needed: `unity-mc2-demo/Assets/Editor/Mc2DemoBuilder.cs`
- Modify if needed: `unity-mc2-demo/README.md`
- Modify if needed: `BUILD-WIN.md` or create a mobile build note

**Steps:**

1. Install Unity Android Build Support with SDK, NDK, and OpenJDK.
2. Confirm Unity can switch the project to Android without importing private/generated artifacts.
3. Add or document a batch build entry such as `MC2Demo.EditorTools.Mc2DemoBuilder.BuildAndroid`.
4. Build an Android artifact into an ignored folder.
5. Record success strings and artifact path in ignored logs.

**Validation:**

```powershell
git diff --check
```

Expected:

```text
Android build succeeded
```

Exact Unity success string should be captured once the Android builder exists.

**Commit:** `Add Android build smoke path`

## Task 3: Run Android Device Smoke

**Files:**

- Modify if needed: `scripts/unity/` smoke helper
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`
- Generate ignored output only: `analysis-output/`

**Steps:**

1. Enable Android developer mode and USB debugging on a real device.
2. Install the APK through Unity or `adb install`.
3. Launch the app.
4. Run a minimal smoke path by command-file, debug menu, or deterministic startup mode.
5. Capture device log and record whether battle, MechLab, debrief, repair, and relaunch remain reachable.

**Validation:**

```powershell
adb devices
adb logcat -d | Select-String -Pattern "MC2"
git diff --check
```

**Commit:** `Document Android device smoke results`

## Task 4: Touch Command UI Pass

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify if needed: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/*.txt`

**Steps:**

1. Keep the battle UI sparse.
2. Scale status rows and buttons to mobile-safe touch targets.
3. Preserve the current command model:
   - default squad command;
   - status-row single mech selection;
   - tap ground or target to issue command;
   - Jet button;
   - mission map toggle;
   - system/pause.
4. Do not add drag-box selection.
5. Do not add dense combat logs.
6. Add a mobile UI sidecar/assertion if capture infrastructure supports it.

**Validation:**

```powershell
git diff --check
```

Run Android device smoke again after implementation.

**Commit:** `Adapt command UI for mobile touch`

## Task 5: Mobile Performance Budget

**Files:**

- Modify or create: `docs-mobile-performance-budget-2026-06-10.md`
- Modify if needed: mobile capture/smoke helper scripts

**Initial Budgets:**

- Phone target: playable on mid-range Android device first.
- Battle scale: 1-6 player mechs, usually 4.
- Camera: fixed tactical view, limited zoom.
- UI: sparse, no large logs in battle.
- FPS target: define after first real device run.
- Package-size target: define after first Android artifact.
- Thermal target: no obvious runaway heating in a short mission smoke.

**Steps:**

1. Record baseline FPS.
2. Record memory.
3. Record package size.
4. Record first-load time.
5. Record battle-load time.
6. Record thermal/battery notes manually.
7. Decide whether visuals need mobile LOD/material simplification before adding more content.

**Validation:**

```powershell
git diff --check
```

**Commit:** `Define mobile performance budget`

## Task 6: iOS Feasibility Note

**Files:**

- Modify or create: `docs-ios-feasibility-2026-06-10.md`

**Steps:**

1. Record macOS/Xcode/signing requirements.
2. Keep iOS behind Android build proof.
3. List expected blocker categories:
   - Apple developer account;
   - signing profiles;
   - macOS build machine;
   - device testing;
   - package size and Metal rendering checks.
4. Do not block Android work on iOS setup.

**Validation:**

```powershell
git diff --check
```

**Commit:** `Document iOS feasibility gate`

## Deferred Until Mobile Gate Passes

- F2 map authoring contract.
- F3 Web ranking contract.
- F4 creator economy boundary.
- Server implementation.
- Realtime PVP.
- Chain integration.
- Unreal MCP tooling spike.

These are not abandoned. They are ordered behind mobile proof because the game
must work on phones first.

## Stop Conditions

Stop and reassess before continuing if:

- Android build support cannot be installed cleanly.
- Unity Android build fails due project structure, package conflicts, or unsupported APIs.
- The demo launches but touch interaction cannot complete the core command loop.
- The fixed camera or MechLab grid is not readable on phone aspect ratios.
- BattleCore behavior diverges from Windows validator/smoke.
- Package size is already too large before real art/audio content.
- Device performance or thermal behavior is unacceptable for the target battle scale.
- Any generated APK/AAB, device logs, screenshots, JSON sidecars, Unity build output, or private reference art is staged.

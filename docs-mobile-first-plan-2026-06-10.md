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

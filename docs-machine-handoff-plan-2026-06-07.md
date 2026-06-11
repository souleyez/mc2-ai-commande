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
- Latest sealed PC/mobile wait-state checkpoint: `PC1-PC41`
- Last completed PC checkpoint: `Add PC capture preset contract check`
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
- Current formal next development task after handoff: `G3 Run Android device smoke`

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
- `scripts/unity/check_pc_core_playable_contract.ps1` prints `PC core playable contract check OK`.
- `scripts/unity/check_mobile_command_model_preflight.ps1` prints `Mobile command model preflight OK`.
- `scripts/unity/check_battle_hud_sparse_contract.ps1` prints `Battle HUD sparse contract check OK`.
- `scripts/unity/check_pc_visual_capture_sanity.ps1` prints `PC visual capture sanity check OK`.
- `scripts/unity/check_pc_visual_capture_sanity.ps1 -SelfTest` prints `PC visual capture sanity self-test OK`.
- `scripts/unity/check_pc_capture_sidecar_schema.ps1` prints `PC capture sidecar schema check OK`.
- `scripts/unity/check_pc_capture_preset_contract.ps1` prints `PC capture preset contract check OK`.
- `scripts/unity/check_current_plan_gate.ps1` prints `Current plan gate check OK`.
- `scripts/unity/check_android_smoke_log.ps1 -SelfTest` prints `Android smoke log check self-test OK`.
- `scripts/unity/check_android_smoke_summary.ps1 -SelfTest` prints `Android smoke summary check self-test OK`.
- `scripts/unity/android_device_smoke.ps1 -PlanOnly` prints `Android device smoke plan OK`.
- `scripts/unity/android_device_smoke.ps1 -PlanOnly` prints `ScreenshotCapture: True` and `analysis-output\android-device-smoke.png`.
- `scripts/unity/android_device_smoke.ps1 -PlanOnly` prints `SummaryWrite: True` and `analysis-output\android-device-smoke-summary.json`.
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

**Step 13: Run current plan gate check**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
```

Expected:

```text
Current plan gate check OK
```

This wraps handoff/readiness, Windows build freshness, demo source hygiene, AI
deputy contract, mobile command model, battle HUD sparse contract, PC visual capture sanity, PC visual capture sanity self-test, PC capture sidecar schema, PC capture preset contract and Android
preflight checks. With no authorized phone connected, Android should be
reported as waiting on device; with one authorized phone, Android should report
OK.

**Step 14: Self-test Android smoke log scanning**

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

**Step 15: Self-test Android smoke summary schema**

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

**Step 16: Preview Android device smoke plan**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
```

Expected:

```text
Android device smoke plan OK
```

This proves the real device-smoke helper can resolve the APK, adb, aapt,
package, activity, log path and planned install/launch/log-check actions before
a phone is connected.

**Step 17: Check Android smoke plan/preflight consistency**

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

**Step 18: Run Android G3 readiness directly**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_g3_readiness.ps1
```

Expected with no connected phone:

```text
Android G3 readiness check waiting on device
```

This wraps device preflight, plan/preflight consistency, plan mode, log scanner
self-test and summary schema self-test. It does not install or launch the app.

**Step 19: Confirm strict G3 device requirement**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_g3_device_requirement.ps1
```

Expected with no connected phone:

```text
Android G3 device requirement check waiting on device
```

This proves strict G3 readiness cannot be accepted without an authorized
Android phone.

**Step 20: Run Android device-smoke preflight directly if needed**

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

**Step 21: Set paths for rebuilding evidence if needed**

```powershell
$Repo = (Get-Location).Path
$Unity = "$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe"
```

If `$Unity` does not exist, point it to the installed Unity editor path.

**Step 22: Run validator when rebuilding or auditing from scratch**

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

**Step 23: Build Windows player when rebuilding or auditing from scratch**

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

**Step 24: Run visible-flow smoke without AI key when rebuilding or auditing from scratch**

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
- Next planned work: `G3 Run Android device smoke`

**Step 1: Confirm current next task**

Read the current commit queue. After this handoff, the product work should
resume at:

```text
G3 Run Android device smoke
```

**Step 2: Do not start with server or map-editor implementation**

Mobile support is the first priority. The next product work should prove:

- a real Android device can launch the demo;
- touch command UI is viable for squad command, single-unit command, Jet, mission map, system panel and MechLab;
- FPS, memory, package size, load time and thermal observations are recorded.

Android Build Support and Android APK build smoke have already been handled on
the old machine. If the new machine cannot produce the APK, fix the local Unity
Android module/toolchain before changing gameplay.

Map package/editor contracts, Web ranking and creator economy remain deferred
until the mobile gate passes.

**Step 3: Commit next product work in one small commit**

The next commit should be the smallest change that advances the current queue,
for example a real-device smoke evidence/documentation checkpoint:

```powershell
git add BUILD-MOBILE.md docs-mobile-first-plan-2026-06-10.md docs-ai-rts-commander-current-master-plan-2026-06-07.md docs-ai-rts-commander-current-detailed-plan-2026-06-07.md
git commit -m "Record Android device smoke"
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

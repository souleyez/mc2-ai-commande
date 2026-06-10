# Machine Handoff Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Move active development to another Windows machine without losing current source progress, Unity build reproducibility, local smoke validation, optional private reference visuals, or AI deputy configuration.

**Architecture:** Git remains the source of truth for code and public-safe metadata. Ignored build products, screenshots, logs, sidecars, Unity cache, and private reference exports are recreated or copied only as local development evidence. The new machine must first prove the clean fallback path works, then optionally restore private reference visuals for local-only readability checks.

**Tech Stack:** Windows, Git, PowerShell, Unity Hub, Unity 6000.4.7f1 with Windows Build Support, Unity batchmode validator/build/smoke commands, optional MiniMax environment variables.

---

## Storage Note

The writing-plans skill normally saves plans under `docs/plans/`, but this
repository already has a tracked legacy file named `docs`. This plan therefore
uses the repository's existing root-level `docs-*.md` convention.

## Current Checkpoint

As of this handoff plan:

- Branch: `master`
- Primary project remote: `ai-origin git@github.com:souleyez/mc2-ai-commander-demo.git`
- Upstream source remote kept for history: `origin https://github.com/alariq/mc2.git`
- Local branch state before this plan commit: `master...ai-origin/master [ahead 95]`
- Last completed product commit: `1044ef1 Document reward authority contract`
- Current formal next development task after handoff: `G2 Add Android build smoke path`

Important: the new machine will not see the last 95 local commits unless the old
machine first pushes to `ai-origin`, or the full repository is migrated by a
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
## master...ai-origin/master [ahead N]
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

No code commit is required for the push itself. The plan update commit is:

```powershell
git add README.md docs-ai-rts-commander-current-master-plan-2026-06-07.md docs-ai-rts-commander-current-detailed-plan-2026-06-07.md docs-machine-handoff-plan-2026-06-07.md
git commit -m "Prepare machine handoff plan"
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
- latest commit includes this handoff plan commit.

**Step 3: Create ignored output folders only when needed**

```powershell
New-Item -ItemType Directory -Force .\analysis-output | Out-Null
```

Unity will recreate `unity-mc2-demo\Library`, `Temp`, `Logs`, and build folders.
Do not copy those cache folders from the old machine unless diagnosing a Unity
import problem.

## Task 4: Prove Clean Fallback Demo Works

**Files:**

- Read: `BUILD-WIN.md`
- Read: `unity-mc2-demo/README.md`
- Generate ignored output only: `analysis-output/`, `unity-mc2-demo/Builds/`

**Step 1: Set paths**

```powershell
$Repo = (Get-Location).Path
$Unity = "$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe"
```

If `$Unity` does not exist, point it to the installed Unity editor path.

**Step 2: Run validator**

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

**Step 3: Build Windows player**

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

**Step 4: Run visible-flow smoke without AI key**

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
- Next planned work: Android build smoke path

**Step 1: Confirm current next task**

Read the current commit queue. After this handoff, the product work should
resume at:

```text
G2 Add Android build smoke path
```

**Step 2: Do not start with server or map-editor implementation**

Mobile support is the first priority. The next product work should prove:

- Android Build Support is installed and usable;
- the Unity project can produce an Android artifact;
- a real Android device can launch the demo;
- touch command UI is viable for squad command, single-unit command, Jet, mission map, system panel and MechLab;
- FPS, memory, package size, load time and thermal observations are recorded.

Map package/editor contracts, Web ranking and creator economy remain deferred
until the mobile gate passes.

**Step 3: Commit next product work in one small commit**

Expected future commit:

```powershell
git add unity-mc2-demo/Assets/Editor/Mc2DemoBuilder.cs unity-mc2-demo/README.md BUILD-WIN.md docs-ai-rts-commander-current-master-plan-2026-06-07.md docs-ai-rts-commander-current-detailed-plan-2026-06-07.md
git commit -m "Add Android build smoke path"
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

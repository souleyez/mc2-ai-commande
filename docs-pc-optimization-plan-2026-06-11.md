# PC Demo Optimization Implementation Plan

> **For Codex:** REQUIRED SUB-SKILL: Use executing-plans to implement this plan task-by-task.

**Goal:** 在 Android G3 真机验证等待设备期间，继续把 Windows/PC 可玩 Demo 打磨成能稳定演示、能截图、能解释战斗乐趣的版本。

**Architecture:** `BattleCore` 仍然是规则真相，PC 优化只改善可见性、输入舒适度、截图证据、构建和演示包装。任何 PC 便利功能都不能改变移动端的核心指挥模型：默认全队、状态栏单选、点地/点目标、Jet、任务地图、系统按钮。

**Tech Stack:** Unity 6, C#, deterministic BattleCore, Windows player build/smoke, PowerShell validator/build/capture scripts, ignored visual evidence under `analysis-output/`.

---

## Product Decision

Mobile support remains the product priority, but `G3 Android Device Smoke` is waiting on a physical Android phone with USB debugging authorized. While that device blocker exists, the active executable work is PC demo optimization.

This does not move G4/G5 mobile touch and performance ahead of G3. It only prevents the project from idling while the required phone is unavailable.

## Definition Of Done

The current PC optimization pass is complete when:

- Unity mission validator passes.
- Windows player build passes.
- Visible-flow smoke reaches debrief, repair and relaunch.
- `mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol` captures pass.
- First battle view clearly separates terrain, units, buildings, water, roads/runway, objectives and contact direction.
- Dense contact still reports no true overlap in sidecar evidence.
- Battle UI remains sparse: status rows, Jet, objective/map, system/pause; no large logs, save slots, account UI or debug overlays in normal battle.
- MechLab PC flow is easy to read: mounted weapon blocks are whole-grid objects, all installed weapons are active, heat/weight/slot legality is visible, no enable/disable toggle returns.
- No generated screenshot, JSON sidecar, log, Windows build output, APK/AAB, or private reference export is staged.

## Execution Gate Order

| Gate | Status | Purpose | Required Before Next Gate |
| --- | --- | --- | --- |
| PC0 | Done | Existing Windows baseline | Prior validator/build/smoke and visual captures have passed |
| PC1 | Done | Audit current PC baseline | Re-run validator, Windows build, visible-flow smoke and six captures; record exact current weakness |
| PC2 | Next | Polish battle readability | Fix only the highest-impact visible issue from PC1 |
| PC3 | Later | Polish MechLab PC flow | Improve grid/loadout readability without adding weapon toggles |
| PC4 | Later | Package controlled PC demo evidence | Refresh walkthrough/evidence and keep generated artifacts ignored |

Do not start PC2 until PC1 produces fresh evidence. Do not change gameplay rules from visual inspection alone; if the issue is collision, damage, command state or objective logic, first prove it in `BattleCore`.

## Completed Target: PC1 Audit Current PC Baseline

**Precondition:**

- `git status --short --branch --untracked-files=all` is clean or only contains this plan update.
- Unity exists at `$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe`.
- Windows build output remains ignored under `unity-mc2-demo\Builds\Windows`.
- Private reference visuals, if present, remain local-only and ignored.

**Action:**

1. Run the mission validator.
2. Run the Windows player build.
3. Run visible-flow smoke through the command file.
4. Run the six standard captures.
5. Inspect sidecars and screenshots only enough to choose one next PC polish target.
6. Update `docs-reference-visual-audit-2026-06-07.md` or `docs-playable-demo-investor-evidence-2026-06-07.md` only if the fresh evidence changes the written judgment.

**Commands:**

```powershell
$Repo = (Get-Location).Path
$Unity = "$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe"

& $Unity `
  -batchmode -quit `
  -projectPath "$Repo\unity-mc2-demo" `
  -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract `
  -logFile "$Repo\analysis-output\unity-validate-pc-baseline.log"

& $Unity `
  -batchmode -quit `
  -projectPath "$Repo\unity-mc2-demo" `
  -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 `
  -logFile "$Repo\analysis-output\unity-build-pc-baseline.log"

& "$Repo\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" `
  -batchmode -nographics `
  -mc2SmokeTest `
  -mc2CommandFile "$Repo\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" `
  -logFile "$Repo\analysis-output\unity-player-pc-visible-flow-baseline.log"

powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\capture_reference_visuals.ps1 -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol
```

**Output:**

- ignored validator/build/player logs under `analysis-output/`;
- ignored screenshots and JSON sidecars under `analysis-output/reference-visual-captures/`;
- optional docs note summarizing the next PC polish target;
- no generated artifact staged.

**Verification:**

```powershell
git diff --check
git status --short --branch --untracked-files=all
Select-String -Path .\analysis-output\unity-validate-pc-baseline.log -Pattern "MC2 demo contract validation OK"
Select-String -Path .\analysis-output\unity-build-pc-baseline.log -Pattern "Build Finished, Result: Success","MC2 Unity demo Windows build OK"
Select-String -Path .\analysis-output\unity-player-pc-visible-flow-baseline.log -Pattern "MC2 demo smoke test exiting with code 0"
```

Expected capture result:

```text
MC2 reference visual captures passed
```

**Failure Handling:**

- Validator failure: fix the smallest BattleCore/contract issue first; do not mask with Unity visuals.
- Windows build failure: fix compile or Unity build setting issue before visual work.
- Visible-flow smoke failure: inspect command state and startup flow; do not continue to screenshots until smoke passes.
- Capture failure: classify whether the issue is blank frame, UI regression, contact overlap, missing asset fallback, or unreadable framing.
- Generated artifacts in git: leave them ignored or unstage them; do not commit logs, screenshots, sidecars or builds.

**Commit Scope:**

- Allowed: plan docs, evidence docs, minimal script fix if PC smoke/capture command is broken.
- Not allowed: `analysis-output/`, `unity-mc2-demo/Builds/`, generated screenshots, JSON sidecars, logs, private reference exports.

**Completed Evidence 2026-06-11:**

```text
analysis-output/unity-validate-pc-baseline.log: MC2 demo contract validation OK.
analysis-output/unity-build-pc-baseline.log: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
analysis-output/unity-player-pc-visible-flow-baseline.log: MC2 demo smoke test exiting with code 0.
capture_reference_visuals.ps1 -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol: MC2 reference visual captures passed: 6 preset(s).
docs-reference-visual-audit-2026-06-07.md records the PC1 baseline and selects PC2 terrain/water/land readability as the next highest-impact polish target.
```

**Commit:** `Audit PC demo baseline`

## Current Executable Target: PC2 Polish Battle Readability

**Goal:** 根据 PC1 证据，只修一个最高影响的战场可读性问题：地形、水域、岸线、道路/跑道和可战斗陆地区域在默认 PC 镜头下必须更清楚。

**Likely Files:**

- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/DemoTerrainView.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/DemoStructureView.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `scripts/unity/capture_reference_visuals.ps1`
- Modify if needed: `docs-reference-visual-audit-2026-06-07.md`

**Executable Requirements:**

| ID | Requirement | Pass Standard |
| --- | --- | --- |
| PC2-R1 | Terrain and water are distinct | `spawn`, `airfield` and `north-patrol` do not read as one broad blue field with yellow-green noise |
| PC2-R2 | Units have readable silhouettes | player squad and first enemies are distinguishable at default camera |
| PC2-R3 | Contact is not a visual pile | `hangar-contact` sidecar still reports no true overlaps |
| PC2-R4 | Damage story remains visible | `damage-demo` still shows section damage/ejection/wreck cues |
| PC2-R5 | Sparse HUD is preserved | no large log, save, account or debug overlay appears in normal battle |

**Validation:**

```powershell
git diff --check
& "$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "$PWD\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "$PWD\analysis-output\unity-build-pc-readability.log"
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo
```

**Commit:** `Polish PC battle readability`

## PC3: Polish MechLab PC Flow

**Goal:** 让 PC 端装配界面更接近“整块装备放入格子”的直观感觉，不回退到武器启用/关闭。

**Likely Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutContract.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutValidator.cs`
- Modify if needed: `scripts/unity/capture_reference_visuals.ps1`

**Executable Requirements:**

| ID | Requirement | Pass Standard |
| --- | --- | --- |
| PC3-R1 | Weapon blocks are whole shapes | mounted weapons occupy their full shape visually |
| PC3-R2 | All mounted weapons are active | there is no enable/disable weapon toggle |
| PC3-R3 | Heat and weight pressure are visible | legal/illegal state is understandable without a long explanation |
| PC3-R4 | Armor/sink fillers remain simple | single-cell fillers are clear and do not become a second game system |
| PC3-R5 | Layout fits desktop demo | no text overlap at 1280x720 and normal Windows player size |

**Validation:**

```powershell
git diff --check
& "$HOME\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "$PWD\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "$PWD\analysis-output\unity-build-pc-mechlab.log"
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\capture_reference_visuals.ps1 -Presets mechlab
```

**Commit:** `Polish PC MechLab flow`

## PC4: Package Controlled PC Demo Evidence

**Goal:** 在 PC 可展示质量收稳后，刷新演示脚本和证据页，方便拿给外部人看。

**Files:**

- Modify: `docs-playable-demo-walkthrough-2026-06-07.md`
- Modify: `docs-playable-demo-investor-evidence-2026-06-07.md`
- Modify if needed: `README.md`
- Read: `BUILD-WIN.md`

**Validation:**

```powershell
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- Docs describe what the PC demo can actually show today.
- Docs do not imply private reference assets are public-safe final art.
- The demo can be run from the Windows build path without explaining internal scripts to the viewer.

**Commit:** `Package PC controlled demo evidence`

## Stop Conditions

Stop and reassess before committing if:

- Android phone becomes available; then run G3 before moving G4/G5 or any mobile-specific work.
- PC optimization would add controls that cannot translate to mobile's simple command model.
- A visual fix hides a BattleCore collision or damage bug instead of fixing the rule.
- Unity scene churn is only fileID noise.
- Any private original-derived file, generated screenshot, JSON sidecar, log or build output is about to be staged.
- Battle UI grows dense again.

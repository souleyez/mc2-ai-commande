# PC Demo Optimization Implementation Plan

> **For Codex:** REQUIRED SUB-SKILL: Use executing-plans to implement this plan task-by-task.

**Goal:** 在 Android G3 真机验证等待设备期间，继续把 Windows/PC 可玩 Demo 打磨成能稳定演示、能截图、能解释战斗乐趣的版本。

**Architecture:** `BattleCore` 仍然是规则真相，PC 优化只改善可见性、输入舒适度、截图证据、构建和演示包装。任何 PC 便利功能都不能改变移动端的核心指挥模型：默认全队、状态栏单选、点地/点目标、Jet、任务地图、系统按钮。

**Tech Stack:** Unity 6, C#, deterministic BattleCore, Windows player build/smoke, PowerShell validator/build/capture scripts, ignored visual evidence under `analysis-output/`.

---

## Product Decision

Mobile support remains the product priority, but `G3 Android Device Smoke` is waiting on a physical Android phone with USB debugging authorized. While that device blocker existed, this plan used PC demo optimization to keep the Windows demo moving.

The current PC/mobile wait-state optimization pass is now sealed through PC25. This does not move G4/G5 mobile touch and performance ahead of G3; the next mobile gate still requires the physical authorized phone.

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
- Controlled Windows demo launch has a preflight helper and defaults to a stable 1280x720 windowed launch.
- Controlled Windows demo build freshness can be checked without launching Unity, proving the ignored player output is newer than tracked Unity build inputs.
- Controlled Windows demo evidence can be checked by script without rerunning Unity.
- Controlled Windows demo evidence freshness can be checked by script, proving visible-flow logs, six capture PNG/JSON sidecars and six capture logs are newer than the current Windows build and evidence inputs.
- Controlled Windows demo public boundary metadata can be checked by script without packaging or creating artifacts.
- Controlled Windows demo readiness can be checked by one script that wraps launch, evidence and public boundary gates.
- Controlled Windows demo handoff consistency can be checked by one script that validates docs and helper scripts agree on the current gate set.
- Android device-smoke readiness can be checked without installing or launching the app, and can explicitly stop at waiting-on-device when no phone is connected.
- PC core playable contract can be checked by one script that runs the Unity/BattleCore validator and requires command-state, solo-return, Jet, occupancy, damage/ejection and debrief/relaunch coverage.
- Mobile command model preflight can be checked without launching Unity, proving the current PC command surface still maps to status rows, Jet, map/bay/system, compact objective, sparse HUD and MechLab no-toggle fitting.
- Current plan gate can be checked by one script that wraps handoff/readiness, Windows build freshness, demo source hygiene, AI deputy contract, mobile command model, battle HUD sparse contract and Android device-smoke preflight state.
- Android device smoke scans captured logcat for strong crash markers before accepting a real-device launch.
- Android device smoke can be previewed with `-PlanOnly` without a connected phone.
- Android APK freshness can be checked before G3, proving the ignored APK is newer than tracked Unity build inputs.
- Android APK identity can be checked before G3, proving the package name and launch activity match the install/launch commands.
- Android APK compatibility can be checked before G3, proving min SDK, target SDK and native ABI metadata match the intended Android smoke target.
- Android APK signing can be checked before G3, proving `apksigner verify` and APK Signature Scheme v2 pass before any device install.
- Sparse battle HUD can be checked without launching Unity through `check_battle_hud_sparse_contract.ps1`.
- Demo source hygiene can be checked without launching Unity through `check_demo_source_hygiene.ps1`.
- AI deputy contract can be checked without launching Unity or calling the model through `check_ai_deputy_contract.ps1`.
- Android APK freshness can be checked without launching Unity through `check_android_apk_freshness.ps1`.
- Android APK identity can be checked without launching Unity through `check_android_apk_identity.ps1`.
- Android APK compatibility can be checked without launching Unity through `check_android_apk_compatibility.ps1`.
- Android APK signing can be checked without launching Unity through `check_android_apk_signing.ps1`.
- No generated screenshot, JSON sidecar, log, Windows build output, APK/AAB, or private reference export is staged.

## Execution Gate Order

| Gate | Status | Purpose | Required Before Next Gate |
| --- | --- | --- | --- |
| PC0 | Done | Existing Windows baseline | Prior validator/build/smoke and visual captures have passed |
| PC1 | Done | Audit current PC baseline | Re-run validator, Windows build, visible-flow smoke and six captures; record exact current weakness |
| PC2 | Done | Polish battle readability | Fix only the highest-impact visible issue from PC1 |
| PC3 | Done | Polish MechLab PC flow | Improve grid/loadout readability without adding weapon toggles |
| PC4 | Done | Package controlled PC demo evidence | Refresh walkthrough/evidence and keep generated artifacts ignored |
| PC5 | Done | Add PC demo launch preflight | Check Windows build presence and launch with stable window args |
| PC6 | Done | Add controlled demo evidence check | Check build, visible-flow log and six capture sidecars |
| PC7 | Done | Add controlled demo public boundary preflight | Check clean project-owned metadata examples and optionally confirm the dev build remains blocked for public packaging |
| PC8 | Done | Add controlled demo readiness preflight | Wrap launch, evidence and public boundary gates into one command |
| PC9 | Done | Add controlled demo handoff consistency check | Check scripts and docs agree on the current controlled demo gate set |
| PC10 | Done | Add Android device smoke preflight | Check APK/tooling/package/device state before the real G3 install/launch smoke |
| PC11 | Done | Add PC core playable contract check | Run Unity/BattleCore validator through a script and require the PC core playable marker |
| PC12 | Done | Add mobile command model preflight | Check sidecar/source/doc markers for the mobile-low-complexity command model without launching Unity |
| PC13 | Done | Add current plan gate check | Run one current-state command for handoff/readiness, mobile command model and Android waiting/ready state |
| PC14 | Done | Add Android smoke log crash scan | Scan logcat for strong crash markers after real-device smoke launch |
| PC15 | Done | Add Android smoke plan mode | Preview the Android device smoke helper's resolved paths and actions without selecting a device |
| PC16 | Done | Add battle HUD sparse contract check | Check source, capture gate and mobile command preflight agree on sparse active-battle HUD without launching Unity |
| PC17 | Done | Add demo source hygiene check | Check tracked/staged paths and ignore markers keep generated evidence, Unity builds and private reference art out of source commits |
| PC18 | Done | Add AI deputy contract check | Check MiniMax stays optional, slow, high-level, rule-fallback guarded, absent from frame loops and not invoked by default smoke |
| PC19 | Done | Add Windows demo build freshness check | Check ignored Windows player output is newer than tracked Unity build inputs and wire it into readiness preflight |
| PC20 | Done | Add controlled demo evidence freshness check | Check visible-flow log and six capture PNG/JSON sidecars are newer than the current Windows build/evidence inputs |
| PC21 | Done | Add controlled demo capture log freshness check | Check six capture logs exist, are fresh, and prove preset, screenshot request and sidecar write markers |
| PC22 | Done | Add Android APK freshness check | Check ignored Android APK is newer than tracked Unity build inputs and wire it into G3 preflight/smoke helpers |
| PC23 | Done | Add Android APK identity check | Check package name and launch activity match expected G3 install/launch identity and wire it into G3 preflight/smoke helpers |
| PC24 | Done | Add Android APK compatibility check | Check min SDK, target SDK and native ABI match expected Android smoke compatibility metadata and wire it into G3 preflight/smoke helpers |
| PC25 | Done | Add Android APK signing check | Check apksigner verification, APK Signature Scheme v2 and signer DN before G3 install/launch |

Do not open another PC polish gate from visual inspection alone. If the issue is collision, damage, command state or objective logic, first prove it in `BattleCore`.

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

## Completed Target: PC2 Polish Battle Readability

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

**Completed Evidence 2026-06-11:**

```text
analysis-output/unity-build-pc-terrain-readability.log: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol: MC2 reference visual captures passed: 5 preset(s).
sidecar terrainReadability: texture=composite textureStrength=0.28 waterSurface=readable-overlay alpha=0.48 style=land-outline+runway-contrast+water-muted pathing=unchanged.
sidecar contactClearance: all five PC2 battle captures still report overlaps=0 status=separated.
```

**Commit:** `Polish PC battle readability`

## Completed Target: PC3 Polish MechLab PC Flow

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

**Completed Evidence 2026-06-11:**

```text
analysis-output/unity-build-pc-mechlab.log: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
capture_reference_visuals.ps1 -Presets mechlab: MC2 reference visual captures passed: 1 preset(s).
sidecar mechLab: layout=pressure-cards+whole-blocks+single-fillers, alwaysMounted=weapons 6/6 items 6/6 noToggle=yes.
```

**Commit:** `Polish PC MechLab flow`

## Completed Target: PC4 Package Controlled PC Demo Evidence

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

**Completed Evidence 2026-06-12:**

```text
analysis-output/unity-build-pc-evidence-package.log: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
analysis-output/unity-player-pc-evidence-visible-flow.log: MC2 demo smoke test exiting with code 0; debrief, repair/Mech Lab, relaunch identity and compact loadout checks passed.
capture_reference_visuals.ps1 -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol: MC2 reference visual captures passed: 6 preset(s).
sidecar terrainReadability: texture=composite textureStrength=0.28 waterSurface=readable-overlay alpha=0.48 style=land-outline+runway-contrast+water-muted pathing=unchanged.
sidecar mechLab: layout=pressure-cards+whole-blocks+single-fillers, pressure=H 12/22 W 16/16 G 12/16, alwaysMounted=weapons 6/6 items 6/6 noToggle=yes.
sidecar contactClearance: all five battle captures report overlaps=0 status=separated; evidence tolerance now treats sub-1-unit clearance jitter as touching, not gameplay overlap.
docs-playable-demo-walkthrough-2026-06-07.md and docs-playable-demo-investor-evidence-2026-06-07.md describe the current PC demo without claiming private reference visuals are public-safe final art.
```

**Commit:** `Package PC controlled demo evidence`

## Completed Target: PC5 Add PC Demo Launch Preflight

**Goal:** 在 G3 真机仍不可用时，只收稳当前 Windows 受控演示启动入口，不改玩法。

**Files:**

- Create: `scripts/unity/run_windows_demo.ps1`
- Modify: `BUILD-WIN.md`
- Modify: `README.md`
- Modify: current plan docs

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\run_windows_demo.ps1 -CheckOnly
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- `-CheckOnly` validates the Windows executable and Unity data folder without launching a player.
- Normal launch passes `-screen-width 1280 -screen-height 720 -screen-fullscreen 0`.
- Runtime log path stays ignored under `analysis-output/windows-demo-run.log`.
- No gameplay, BattleCore, HUD, MechLab, content-pack or generated evidence behavior changes.

**Commit:** `Add PC demo launch preflight`

## Completed Target: PC6 Add Controlled Demo Evidence Check

**Goal:** 在 G3 真机仍不可用时，只把当前 Windows 受控演示证据包变成可机器检查状态，不改玩法。

**Files:**

- Create: `scripts/unity/check_controlled_demo_evidence.ps1`
- Modify: `BUILD-WIN.md`
- Modify: `README.md`
- Modify: current plan docs

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_evidence.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- Checks Windows build and Unity data folder.
- Checks visible-flow log for smoke exit `0`, debrief summary and compact loadout assertions.
- Checks six capture PNG/JSON pairs, MechLab no-toggle fitting, terrain readability, sparse HUD, contact separation and damage-demo section story.
- Reads ignored local evidence only and does not create artifacts.
- Does not change gameplay, BattleCore, HUD, MechLab, content packs or generated evidence behavior.

**Commit:** `Add controlled demo evidence check`

## Completed Target: PC7 Add Controlled Demo Public Boundary Preflight

**Goal:** 在 G3 真机仍不可用时，只把当前受控演示的公开内容边界做成可机器检查状态，不改玩法、不生成素材、不把开发构建误标成 public-safe。

**Files:**

- Create: `scripts/content-pack/check_controlled_demo_public_boundary.ps1`
- Modify: `BUILD-WIN.md`
- Modify: `README.md`
- Modify: current plan docs and evidence page

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\content-pack\check_controlled_demo_public_boundary.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\content-pack\check_controlled_demo_public_boundary.ps1 -CheckDevBuild
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- Checks `project-owned-starter`, `project-owned-text-safe-slice`, `project-owned-visual-slice`, and `project-owned-art-safe-slice`.
- Requires each clean metadata target to return `Result: OK`.
- With `-CheckDevBuild`, confirms the current Windows development build returns expected `Result: FAILED`.
- Reads existing files only and does not create artifacts.
- Does not change gameplay, BattleCore, HUD, MechLab, content packs or generated evidence behavior.

**Commit:** `Add controlled demo public boundary preflight`

## Completed Target: PC8 Add Controlled Demo Readiness Preflight

**Goal:** 在 G3 真机仍不可用时，只把 PC 受控演示前的检查入口收成一条命令，不改玩法、不启动 Unity、不重跑截图。

**Files:**

- Create: `scripts/unity/check_controlled_demo_readiness.ps1`
- Modify: `BUILD-WIN.md`
- Modify: `README.md`
- Modify: current plan docs and evidence page

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_readiness.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- Runs Windows launch preflight in `-CheckOnly` mode.
- Runs controlled demo evidence health check.
- Runs controlled demo public boundary preflight with expected dev-build failure check.
- Reports one top-level readiness result and per-step OK rows.
- Does not start Unity, rebuild, regenerate screenshots, alter BattleCore, change HUD/MechLab behavior, or stage generated artifacts.

**Commit:** `Add controlled demo readiness preflight`

## Completed Target: PC9 Add Controlled Demo Handoff Consistency Check

**Goal:** 在 G3 真机仍不可用时，只把换机和演示前最容易漂移的文档/脚本入口做成可机器检查状态，不改玩法、不启动 Unity、不重跑截图。

**Files:**

- Create: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: `BUILD-WIN.md`
- Modify: `README.md`
- Modify: `docs-machine-handoff-plan-2026-06-07.md`
- Modify: current plan docs and evidence page

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- Checks key controlled demo scripts exist.
- Checks README, BUILD-WIN, current master plan, current detailed plan, PC optimization plan, investor evidence and machine handoff plan all mention the current controlled demo gate set.
- Rejects stale machine-handoff markers such as the old G2 next-task checkpoint, old ahead-count and old reward-contract commit.
- `-RunReadiness` proves the handoff check can also call the full readiness preflight.
- Does not start Unity, rebuild, regenerate screenshots, alter BattleCore, change HUD/MechLab behavior, or stage generated artifacts.

**Commit:** `Add controlled demo handoff consistency check`

## Completed Target: PC10 Add Android Device Smoke Preflight

**Goal:** 在 G3 真机仍不可用时，只把 Android 真机 smoke 的前置条件做成可机器检查状态，证明当前链路只缺授权设备，不提前做 G4/G5。

**Files:**

- Create: `scripts/unity/check_android_device_preflight.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: `BUILD-MOBILE.md`
- Modify: `README.md`
- Modify: current plan docs

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- Checks Android APK, adb and aapt.
- Extracts package name and launchable activity from the APK.
- Strict mode fails when no authorized Android device is connected.
- `-AllowNoDevice` passes the current waiting state with an explicit waiting-on-device message.
- Does not install, launch, rebuild, capture logs, alter BattleCore, change HUD/MechLab behavior, or stage generated artifacts.

**Commit:** `Add Android device smoke preflight`

## Completed Target: PC11 Add PC Core Playable Contract Check

**Goal:** 在 G3 真机仍不可用时，不继续扩大 PC 玩法，只把受控演示最核心的规则状态做成单独可运行的机器检查。

**Files:**

- Create: `scripts/unity/check_pc_core_playable_contract.ps1`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: `BUILD-WIN.md`
- Modify: `README.md`
- Modify: current plan docs, evidence page and handoff plan

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_core_playable_contract.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- Runs the Unity editor validator from a single command.
- Requires `MC2 PC core playable contract OK`.
- Requires `MC2 demo contract validation OK`.
- Keeps generated validator logs under ignored `analysis-output/`.
- Does not launch the player, rebuild, regenerate screenshots, alter HUD/MechLab behavior, install Android packages, or stage generated artifacts.

**Commit:** `Add PC core playable contract check`

## Completed Target: PC12 Add Mobile Command Model Preflight

**Goal:** 在 G3 真机仍不可用时，不提前做触控 UI；先把 PC 演示当前指挥面是否仍能迁移到移动端做成单独机器检查。

**Files:**

- Create: `scripts/unity/check_mobile_command_model_preflight.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: `BUILD-WIN.md`
- Modify: `README.md`
- Modify: current plan docs, evidence page and handoff plan

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- Reads existing ignored capture sidecars and tracked source/docs only.
- Requires battle sidecars to keep status rows, Jet, map, bay/system, compact objective, closed mission map, hidden combat log, hidden account UI, disabled save UI and hidden overlays.
- Requires MechLab sidecar to keep whole-block fitting, all mounted weapons active and no weapon enable/disable toggles.
- Keeps generated artifacts untouched and does not launch Unity, rebuild, install Android packages or alter gameplay.

**Commit:** `Add mobile command model preflight`

## Completed Target: PC13 Add Current Plan Gate Check

**Goal:** 在 G3 真机仍不可用时，不继续扩大 PC 玩法；把当前计划状态收成一条可重复命令，便于每次继续前确认可交接、可演示、移动指挥模型未回退，且 Android 只差授权设备。

**Files:**

- Create: `scripts/unity/check_current_plan_gate.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: `BUILD-WIN.md`
- Modify: `README.md`
- Modify: current plan docs, evidence page and handoff plan

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- Runs handoff/readiness, mobile command model and Android device-smoke preflight checks.
- Accepts either a ready Android device or the explicit waiting-on-device state.
- Does not launch Unity, rebuild, regenerate screenshots, install Android packages, alter gameplay or stage generated artifacts.

**Commit:** `Add current plan gate check`

## Completed Target: PC14 Add Android Smoke Log Crash Scan

**Goal:** 在 G3 真机仍不可用时，不越过 G3；先增强真机 smoke 的失败判定，让设备到位后能自动识别 logcat 里的强崩溃信号。

**Files:**

- Create: `scripts/unity/check_android_smoke_log.ps1`
- Modify: `scripts/unity/android_device_smoke.ps1`
- Modify: `scripts/unity/check_current_plan_gate.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: `BUILD-MOBILE.md`
- Modify: README/plans/evidence/handoff docs

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_log.ps1 -SelfTest
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- `android_device_smoke.ps1` scans logcat after capture unless `-SkipLogCheck` is used.
- Scanner catches fatal exception, fatal signal, `SIGSEGV`, `SIGABRT`, ANR for the package, package process death, forced activity finish and Unity crash marker.
- `-SelfTest` proves both clean and crash samples without requiring a device.
- Current plan gate includes the scanner self-test.

**Commit:** `Add Android smoke log crash scan`

## Completed Target: PC15 Add Android Smoke Plan Mode

**Goal:** 在 G3 真机仍不可用时，不越过 G3；让真机 smoke helper 可以无设备预演，提前证明 APK/tool/package/activity/log path 和动作开关解析正确。

**Files:**

- Modify: `scripts/unity/android_device_smoke.ps1`
- Modify: `scripts/unity/check_current_plan_gate.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: `BUILD-MOBILE.md`
- Modify: README/plans/evidence/handoff docs

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- `-PlanOnly` exits before adb device selection and performs no install, launch or logcat capture.
- It prints `Android device smoke plan OK`.
- It resolves APK, adb, aapt, package, activity or monkey fallback, log path and install/launch/log-check switch state.
- Current plan gate includes the plan mode.

**Commit:** `Add Android smoke plan mode`

## Completed Target: PC16 Add Battle HUD Sparse Contract Check

**Goal:** 在 G3 真机仍不可用时，不扩大玩法；把“战斗中不用显示太多信息”做成源码级和 capture gate 级合约，避免普通战斗 HUD 重新长出大日志、存档、账号或调试覆盖层。

**Files:**

- Create: `scripts/unity/check_battle_hud_sparse_contract.ps1`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `scripts/unity/capture_reference_visuals.ps1`
- Modify: `scripts/unity/check_current_plan_gate.ps1`
- Modify: README/BUILD-WIN/current plans/evidence/handoff docs

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_battle_hud_sparse_contract.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- The script reads current Unity presentation source, capture gate and mobile command model preflight without launching Unity.
- `SparseBattleUiRegressionSummaryOk` requires `missionMap=available-closed`.
- `capture_reference_visuals.ps1` fails battle HUD sidecars that do not report `missionMap=available-closed`.
- Current plan gate includes the sparse HUD contract check.

**Commit:** `Add battle HUD sparse contract check`

## Completed Target: PC17 Add Demo Source Hygiene Check

**Goal:** 在 G3 真机仍不可用时，不扩大玩法；把“生成截图、日志、Unity build、APK/AAB、私有参考导出不能进入源码提交”做成轻量检查，避免后续换机、演示或提交时污染仓库。

**Files:**

- Create: `scripts/unity/check_demo_source_hygiene.ps1`
- Modify: `scripts/unity/check_current_plan_gate.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: README/BUILD-WIN/current plans/evidence/handoff docs

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- The script checks tracked and staged paths without requiring a clean working tree.
- It fails tracked/staged generated evidence, Unity builds, APK/AAB outputs, logs, private reference art, non-example content packs and reference export paths.
- It checks `.gitignore` still contains the expected generated/private-output markers.
- Current plan gate includes the demo source hygiene check.

**Commit:** `Add demo source hygiene check`

## Completed Target: PC18 Add AI Deputy Contract Check

**Goal:** 在 G3 真机仍不可用时，不扩大玩法、不消耗模型 token；把 AI 副官只做慢频高层决策、本地规则继续负责具体战斗的边界做成轻量机器检查。

**Files:**

- Create: `scripts/unity/check_ai_deputy_contract.ps1`
- Modify: `scripts/unity/check_current_plan_gate.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: README/BUILD-WIN/current plans/evidence/handoff docs

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_ai_deputy_contract.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- The script reads source, command files and docs without launching Unity.
- The check performs no MiniMax/API request and requires no key.
- MiniMax prompt remains directive-only, slow and high-level; no coordinates, unit ids, JSON, markdown or tactical micro output.
- Startup MiniMax use remains opt-in through `-mc2MinimaxCommanderSteps`, clamped to a small count, and falls back to local rules when unavailable.
- Normal visible-flow/demo scripts do not request MiniMax steps by default.
- Unity frame loops do not instantiate or call MiniMax commander logic.
- Current plan gate includes the AI deputy contract check.

**Commit:** `Add AI deputy contract check`

## Completed Target: PC19 Add Windows Demo Build Freshness Check

**Goal:** 在 G3 真机仍不可用时，不扩大玩法；把“可展示 Windows player 是否对应当前 Unity 输入”做成轻量检查，避免拿旧构建做受控演示。

**Files:**

- Create: `scripts/unity/check_windows_demo_build_freshness.ps1`
- Modify: `scripts/unity/check_controlled_demo_readiness.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: README/BUILD-WIN/current plans/evidence/handoff docs

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_windows_demo_build_freshness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_readiness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- The script reads tracked Unity `Assets`, `ProjectSettings` and `Packages` inputs plus ignored Windows player outputs.
- It fails if the Windows player output is missing or older than tracked Unity build inputs.
- It excludes the generated scene file from timestamp freshness because the builder can rewrite scene fileIDs after build.
- It does not launch Unity or create artifacts.
- `check_controlled_demo_readiness.ps1` includes the build freshness gate.
- Generated Windows build output remains ignored and unstaged.

**Commit:** `Add Windows demo build freshness check`

## Completed Target: PC20 Add Controlled Demo Evidence Freshness Check

**Goal:** 在 G3 真机仍不可用时，不扩大玩法；把受控演示证据包从“文件存在且内容正确”加强为“文件存在、内容正确且晚于当前 Windows build/证据输入”，避免旧日志旧截图误判为当前可展示状态。

**Files:**

- Modify: `scripts/unity/check_controlled_demo_evidence.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: README/BUILD-WIN/current plans/evidence/handoff docs

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_evidence.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_readiness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- The evidence checker fails if `unity-player-pc-evidence-visible-flow.log` is older than the current Windows build or visible-flow command file.
- It fails if any standard capture PNG/JSON sidecar is older than the current Windows build or `capture_reference_visuals.ps1`.
- It still checks visible-flow success, debrief, loadout compact assertion, six capture sidecars, MechLab no-toggle, terrain readability, sparse HUD, contact separation and damage story.
- It does not launch Unity or create artifacts.
- Fresh ignored visible-flow and six capture outputs exist locally after validation.
- Generated evidence remains ignored and unstaged.

**Commit:** `Add controlled demo evidence freshness check`

## Completed Target: PC21 Add Controlled Demo Capture Log Freshness Check

**Goal:** 在 G3 真机仍不可用时，不扩大玩法；把六个标准截图的 `.log` 也纳入受控演示证据检查，确保截图失败或视觉异常时有对应的当前运行日志可追溯。

**Files:**

- Modify: `scripts/unity/check_controlled_demo_evidence.ps1`
- Modify: `scripts/unity/check_controlled_demo_handoff.ps1`
- Modify: README/BUILD-WIN/current plans/evidence/handoff docs

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_evidence.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_readiness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- The evidence checker fails if any standard capture log is missing.
- It fails if any standard capture log is older than the current Windows build or `capture_reference_visuals.ps1`.
- It requires each log to contain the matching capture preset, screenshot request and sidecar write markers.
- It does not launch Unity or create artifacts.
- Generated evidence remains ignored and unstaged.

**Commit:** `Add controlled demo capture log freshness check`

## Completed Target: PC22 Add Android APK Freshness Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 G3 依赖的 ignored Android APK 是否落后于当前 Unity 输入做成机器检查，避免设备到位后安装旧包。

**Files:**

- Create: `scripts/unity/check_android_apk_freshness.ps1`
- Modify: `scripts/unity/check_android_device_preflight.ps1`
- Modify: `scripts/unity/android_device_smoke.ps1`
- Modify: `scripts/unity/check_current_plan_gate.ps1`
- Modify: README/BUILD-MOBILE/BUILD-WIN/current plans/evidence/handoff docs
- Refresh ignored local output: `unity-mc2-demo/Builds/Android/MC2UnityDemo.apk`

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_freshness.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- The freshness checker fails if `MC2UnityDemo.apk` is missing, empty or older than tracked Unity `Assets`, `ProjectSettings` or `Packages` inputs.
- Generated scene fileID churn is excluded from the timestamp anchor, matching the Windows freshness policy.
- `check_android_device_preflight.ps1 -AllowNoDevice` reports APK freshness OK before stopping at waiting-on-device.
- `android_device_smoke.ps1 -PlanOnly` and real device smoke both reject stale APK before install/launch.
- `check_current_plan_gate.ps1` includes an explicit Android APK freshness gate.
- Refreshed APK remains ignored and unstaged.

**Commit:** `Add Android APK freshness check`

## Completed Target: PC23 Add Android APK Identity Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 G3 安装/启动依赖的 APK 包名和 launch activity 做成机器检查，避免设备到位后安装错包或启动错入口。

**Files:**

- Create: `scripts/unity/check_android_apk_identity.ps1`
- Modify: `scripts/unity/check_android_device_preflight.ps1`
- Modify: `scripts/unity/android_device_smoke.ps1`
- Modify: `scripts/unity/check_current_plan_gate.ps1`
- Modify: README/BUILD-MOBILE/BUILD-WIN/current plans/evidence/handoff/mobile docs

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_identity.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Acceptance:**

- The identity checker fails if `aapt` cannot read APK badging.
- It fails unless package name is `com.DefaultCompany.unitymc2demo`.
- It fails unless launch activity is `com.unity3d.player.UnityPlayerGameActivity`.
- `check_android_device_preflight.ps1 -AllowNoDevice` reports APK identity OK before stopping at waiting-on-device.
- `android_device_smoke.ps1 -PlanOnly` and real device smoke both reject wrong APK identity before install/launch.
- `check_current_plan_gate.ps1` includes an explicit Android APK identity gate.
- No APK, log or generated output is staged.

**Commit:** `Add Android APK identity check`

## Completed Target: PC24 Add Android APK Compatibility Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 G3 安装前的 Android 兼容性元数据做成机器检查，避免设备到位后才发现 SDK 或 ABI 配置漂移。

**Scope:**

- Add `scripts/unity/check_android_apk_compatibility.ps1`.
- Read APK badging through Unity Android SDK `aapt`.
- Require `minSdkVersion` 25.
- Require `targetSdkVersion` 36.
- Require native ABI `arm64-v8a`.
- Wire compatibility into `check_android_device_preflight.ps1`, `android_device_smoke.ps1`, and `check_current_plan_gate.ps1`.
- Update handoff, mobile and evidence docs to keep the current PC/mobile wait-state status sealed through PC25.

**Acceptance:**

- The Android APK compatibility checker fails if `aapt` cannot read APK badging.
- It fails unless `minSdkVersion` is 25.
- It fails unless `targetSdkVersion` is 36.
- It fails unless native-code ABI is exactly `arm64-v8a`.
- `check_android_device_preflight.ps1 -AllowNoDevice` checks APK compatibility before reporting the expected waiting-on-device state.
- `android_device_smoke.ps1 -PlanOnly` and real device smoke reject incompatible APK metadata before install/launch.
- `check_current_plan_gate.ps1` includes an explicit Android APK compatibility gate.
- No APK, log or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_compatibility.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android APK compatibility check`

## Completed Target: PC25 Add Android APK Signing Check

**Goal:** 在 G3 真机仍不可用时，不提前做 G4/G5；把 Android 安装前的 APK 签名有效性做成机器检查，避免设备到位后才发现签名包无法安装。

**Scope:**

- Add `scripts/unity/check_android_apk_signing.ps1`.
- Run Unity Android SDK `apksigner verify --verbose --print-certs`.
- Require the APK verifies successfully.
- Require APK Signature Scheme v2 verification.
- Require the current debug signer DN `C=US, O=Android, CN=Android Debug`.
- Record the signer SHA-256 digest for visibility without locking it as a cross-machine requirement.
- Wire signing into `check_android_device_preflight.ps1`, `android_device_smoke.ps1`, and `check_current_plan_gate.ps1`.
- Update handoff, mobile and evidence docs to keep the current PC/mobile wait-state status sealed through PC25.

**Acceptance:**

- The Android APK signing checker fails if `apksigner` cannot verify the APK.
- It fails unless the APK verifies.
- It fails unless APK Signature Scheme v2 is verified.
- It fails unless the signer DN is `C=US, O=Android, CN=Android Debug`.
- It prints the signer SHA-256 digest for diagnosis.
- `check_android_device_preflight.ps1 -AllowNoDevice` checks APK signing before reporting the expected waiting-on-device state.
- `android_device_smoke.ps1 -PlanOnly` and real device smoke reject unsigned or wrongly signed APK metadata before install/launch.
- `check_current_plan_gate.ps1` includes an explicit Android APK signing gate.
- No APK, log or generated output is staged.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_signing.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_device_preflight.ps1 -AllowNoDevice
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1 -RunReadiness
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
git diff --check
git status --short --branch --untracked-files=all
```

**Commit:** `Add Android APK signing check`

## Stop Conditions

Stop and reassess before committing if:

- Android phone becomes available; then run G3 before moving G4/G5 or any mobile-specific work.
- PC optimization would add controls that cannot translate to mobile's simple command model.
- A visual fix hides a BattleCore collision or damage bug instead of fixing the rule.
- Unity scene churn is only fileID noise.
- Any private original-derived file, generated screenshot, JSON sidecar, log or build output is about to be staged.
- Battle UI grows dense again.

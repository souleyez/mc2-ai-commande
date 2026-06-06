# Playable Demo Locked Execution Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use executing-plans to implement this plan task-by-task.

**Goal:** 把当前 Unity 6 Windows 本地原型收成一版可演示、可截图、可讲清楚价值的轻量机甲战术指挥 Demo：机库装配直观，第一张地图可读，玩家能用极少指令完成一场战斗，AI 副官只作为高层能力窗口出现。

**Architecture:** `BattleCore` 是权威规则层，负责命令、移动、喷射落点、占位、武器、热量、装甲硬度、部位损伤、结算和 AI observation/directive。Unity Presentation 负责输入、固定镜头、UI、模型、材质、特效、截图和本地启动；Unity 可以显示碰撞占位和调试辅助，但不能成为唯一 gameplay truth。开发期可以使用本地私有参考内容包验证画面、比例和节奏，公开构建必须能替换为项目自有或合规授权内容包。

**Tech Stack:** Unity 6, C#, Windows Standalone, deterministic BattleCore, PowerShell validation/capture scripts, `mc2-unity-demo-contract-v1`, private local reference content pack, Git/GitHub.

---

## 0. How To Use This Plan

日期：2026-06-07。

这是当前后续开发的锁定执行版计划。继续开发时优先读这份，再查证据文档。其他旧计划保留为背景，不再作为新的任务入口。

配套文档：

- `docs-reference-visual-audit-2026-06-07.md`: 截图、sidecar、validator、smoke 证据。
- `docs-playable-demo-completion-plan-2026-06-07.md`: 阶段摘要和历史执行记录。
- `docs-playable-demo-detailed-roadmap-2026-06-07.md`: 旧细化路线，后续以本文件为准。
- `docs-content-replacement-plan.md`: 私有参考内容包和公开替换包边界。
- `docs-ai-commander-directive-contract.md`: AI 副官 observation/directive 合同。
- `docs-platform-ecosystem-plan.md`: 地图服务器、认证奖励、排行、创作者生态长期方向。

当前执行点：

1. Stage 1 可见流程已经锁住。
2. Stage 2 战场空间、碰撞占位和 hangar encounter 构图已经收口到可继续推进。
3. 当前进入 Stage 3: Combat Feel Lock。
4. `Regress weapon family cues` 已完成，下一提交优先做 `Lock section damage and ejection cues`。
5. 工作树提交前必须保持干净，Unity scene fileID churn 不得误提交。

当前已知真实文件校准：

- 旧计划中的 `unity-mc2-demo/Assets/Scripts/Presentation/DemoEffectsView.cs` 当前不存在。
- 武器轨迹、命中、方向、impact cue 当前主要在 `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`。
- 单位部位、损伤、热量、喷射等世界表现主要在 `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`。
- 规则合同和验证主要在 `unity-mc2-demo/Assets/Scripts/BattleCore/*` 与 `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`。

## 1. Locked Product Scope

第一版只做 Windows 本地可玩 Demo。核心是两个画面：

1. 机库装配：机甲、武器、装甲板、散热器、载重、热量、维修和出战。
2. 地图战斗：固定视角、小队命令、独立命令、喷射、自动交火、部位损伤、战后结算。

第一版必须保留：

- 1-6 台机甲，常规 4 台。
- 默认全队控制。
- 状态栏点单机甲进入独立命令。
- 独立命令完成后自动归队。
- 喷射按每台机甲独立判断合法落点，非法单位不动，合法单位位移。
- 固定俯视镜头，默认跟随排序第一的指挥官机甲，允许有限缩放，不做自由旋转。
- 战斗中 UI 只保留状态栏、喷射、任务地图、暂停/系统和少量必要状态。
- 机库装配按整块武器占格，装甲板/散热器填剩余格子。
- 武器装上即启用，不做武器开关。
- 装甲板增加整体硬度，部位损伤仍然存在。
- 断臂、腿瘫、驾驶舱弹射要在世界画面和状态栏中可见。
- AI 只做高层建议、开场计划和托管意图，不逐帧控制战斗。

第一版明确不做：

- 实时 PVP。
- 移动端适配。
- 地图服务器。
- 账号经济、充值、提现、链上资产。
- 复杂保存系统。
- AI 导演。
- 大模型逐帧战斗控制。
- 公开发布私有参考素材、旧作剧情、旧作专有名称、旧作商标或旧作文案。

## 2. First Demo Definition

第一版 Demo 的完整可见流程：

1. 启动 Windows Demo。
2. 进入机库/准备界面。
3. 看见 1-6 台机甲，常规 4 台。
4. 点一台机甲，看见武器整块占格、装甲板、散热器、热量、重量和合法性。
5. 一键出战 `mc2_01` 小图。
6. 固定俯视镜头跟随指挥官机甲。
7. 点击地点，全队移动。
8. 点击敌方或目标，全队移动攻击/集火。
9. 点状态栏某台机甲，再点地点或目标，该机甲进入独立命令。
10. 独立命令完成后自动归队，并继续接受最新全队命令。
11. 点击喷射，每台机甲独立判断合法落点。
12. 战斗中能看清地形、建筑、树木/道具、敌我单位、开火、爆炸、残骸和部位损伤。
13. 战斗结束进入简洁战报。
14. 一键维修/补给，回机库，再次出战同图验证改装效果。

第一版成功标准：

- 本机启动后 1 分钟内能看到战斗场景。
- 1280x720 截图不用解释也能分辨地形、机甲、建筑、敌我和战斗状态。
- `spawn`、`airfield`、`hangar-contact`、`damage-demo`、`north-patrol` 五张截图都不退化。
- `damage-demo` 能明确看见武器方向、部位损伤或弹射事件。
- 机库格子能让人一眼理解“什么放在哪、为什么超重/过热、怎么改”。
- 没有网络、没有模型 API 时，本地 Demo 仍能完整运行。

## 3. Architecture Contract

### 3.1 BattleCore Boundary

BattleCore owns:

- mission contract loading and trigger state;
- squad command acceptance;
- detached single-unit command state;
- auto rejoin;
- jet landing legality;
- unit, structure, hard terrain object, water and map-bound occupancy;
- weapon range, damage, cooldown, heat and armor hardness;
- section damage and destruction state;
- debrief, repair and relaunch rules;
- compact AI observation and high-level directive adapter.

Unity Presentation owns:

- click/raycast input;
- fixed tactical camera and limited zoom;
- HUD layout;
- unit, building, terrain and prop rendering;
- visual collision placeholders;
- weapon trails, impact cues, damage fragments and ejection cues;
- reference screenshot capture and sidecar summaries.

Rule: if a behavior affects movement, damage, victory, loss, repair or AI decision, it must be represented in BattleCore or contract data first. Unity-only code can make it visible, but cannot be the only source of truth.

### 3.2 Content Boundary

Development can use local private reference content to validate:

- terrain silhouette;
- mech and vehicle scale;
- building/prop density;
- weapon effect readability;
- damage node placement;
- mission pacing.

Public repository and public builds must use:

- project-owned names;
- project-owned UI text;
- project-owned or licensed models/textures/audio/icons;
- replaceable content pack IDs;
- provenance manifest for asset source and license.

Do not market the project as a clone. The public pitch is: AI-assisted tactical RTS commander exploration, deterministic mech squad battle, optional AI deputy, replaceable content packs and future community map ecosystem.

### 3.3 AI Commander Boundary

AI can:

- read compact observation;
- draft a high-level tactical plan;
- choose a high-level intent such as attack, focus target, defend, regroup, hold, retreat or protect unit;
- provide a short advice line in UI.

AI cannot:

- mutate `BattleMission` directly;
- decide every shot or every frame;
- block local battle when model API is slow or unavailable;
- become required for smoke tests.

## 4. Validation Bus

Every commit:

```powershell
git diff --check
```

BattleCore or contract change:

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-current.log"
```

Unity C# or presentation change:

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-current.log"
```

Visible player flow change:

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-visible-flow-audit.log"
```

Visual change:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol
```

Known good strings:

- `MC2 demo contract validation OK`
- `Build Finished, Result: Success`
- `MC2 Unity demo Windows build OK`
- `MC2 demo smoke test exiting with code 0`
- `MC2 reference visual captures passed`

Do not stage generated PNG/JSON/log evidence unless explicitly requested.

## 5. Current Status By System

| System | Status | Current Gap | Next Action |
| --- | --- | --- | --- |
| Unity build/smoke/capture | Working | Unity scene fileID churn can appear after batch runs | Check diff before every commit |
| `mc2_01` map loading | Working | Only one small map is first-demo scope | Preserve trigger/map data, polish readability |
| Terrain/water/roads | Acceptable | Later edge polish only | Regression only |
| Model/reference asset loading | Working locally | Public replacement boundary still needs guard | Keep private pack local, document public pack |
| Occupancy/collision | Evidence present | Must remain BattleCore-backed | Validator and sidecar guards |
| Command flow | Smoke-covered | Keep UI simple | Regression only unless player flow breaks |
| Weapon effects | Basic | Family cues and direction need stronger visual language | Stage 3.1 |
| Section damage/ejection | Basic | Needs clear world-space event and state-row agreement | Stage 3.2 |
| Armor hardness | Basic | Need simple proof and documentation | Stage 3.3 |
| MechLab | Functional | Needs original-like block fitting polish | Stage 4 |
| Debrief/repair | Basic | Needs clean first-demo loop, no save UI | Stage 5 |
| AI deputy | Experimental | Needs compact optional capability window | Stage 6 |
| Public safety | Partially documented | Needs build/content guard | Stage 7 |
| Demo handoff | Incomplete | Needs walkthrough and repeatable package story | Stage 8 |

## 6. Stage 3: Combat Feel Lock

Goal: combat should read as mech tactics, not colored models exchanging hidden numbers.

### Task 3.1: Regress Weapon Family Cues

**Status:** Completed 2026-06-07.

**Result:** `Mc2DemoBootstrap` now adds family-specific direction cues on top of existing weapon traces: energy direction cores, missile approach pips, ballistic snap-lines and generic explosive shock pulses. The combat situation smoke summary now guards `direction-core`, `approach-pips`, `snap-line` and `approach-wedge` so the cue language stays covered.

**Goal:** Energy, missile, ballistic and explosive cues are visually distinct, and incoming direction is readable at tactical zoom.

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatEvent.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatDataContract.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/UnitState.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Inspect current helpers in `Mc2DemoBootstrap.cs`: `SpawnCombatEffect`, `CreateWeaponTrace`, `CreateEnergyTrace`, `CreateMissileTrace`, `CreateBallisticTrace`, `CreateHitDirectionCue`, `WeaponFxCueSummary`.
2. Confirm `CombatEvent` already carries `WeaponType`, `SpecialEffect`, `AttackerId`, `TargetId` and `SectionName`.
3. Add or tighten validator/smoke summary for weapon family cues before visual polish.
4. Add family-specific direction cues:
   - energy: beam core, bright pillar, muzzle flash, scorch and direction core;
   - missile: arc lanes, salvo spread, blast ring, smoke/crater and approach pips;
   - ballistic: tracer, muzzle snap, sparks, punch ring, debris and snap-line;
   - explosive: blast ring, shock pulse, smoke/debris and short afterglow.
5. Keep cues short and tactical; do not cover the left status rows.
6. Ensure effects begin from plausible muzzle/hardpoint positions and end at section-biased hit points.
7. Capture `damage-demo`.
8. Inspect `analysis-output/reference-visual-captures/damage-demo.png`.
9. Record result in `docs-reference-visual-audit-2026-06-07.md`.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-weapon-family-cues.log"
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-weapon-family-cues.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets damage-demo
```

**Acceptance:**

- Viewer can tell who fired and roughly what kind of weapon fired.
- Effects are visible in `damage-demo` without burying mech status rows.
- Direction of incoming fire is readable at tactical zoom.
- No weapon enable/disable UI is introduced.

**Commit:** `Regress weapon family cues`

### Task 3.2: Lock Section Damage And Ejection Cues

**Goal:** Arm loss, leg disable and cockpit ejection are visible world events and match status rows.

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/UnitState.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/DamageSection.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceObjMeshLibrary.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Confirm sections are cockpit, torso, left arm, right arm and legs.
2. Add validator evidence for at least one arm destruction, one leg disable and one cockpit critical path.
3. Make destroyed arm lower firepower or clearly mark lost weapon state.
4. Make destroyed legs reduce movement/jet capability or clearly mark mobility loss.
5. Use reference damage nodes when available.
6. Fall back to project-owned placeholder fragments when reference nodes are missing.
7. Add cockpit ejection as a short pod/chute/escape marker event.
8. Sync status-row section labels with world-space damage events.
9. Capture `damage-demo`.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-section-damage-lock.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets damage-demo
```

**Acceptance:**

- `damage-demo` shows at least one severe section damage event.
- Cockpit ejection is visual, not only log text.
- World cue and status row agree.

**Commit:** `Lock section damage and ejection cues`

### Task 3.3: Lock Armor Hardness Damage Rule

**Goal:** Armor plates increase one overall hardness value; section damage remains the fun part.

**Files:**

- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/UnitState.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutValidator.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `docs-mc2-detailed-development-plan.md`

**Steps:**

1. Confirm armor plate cells accumulate into one hardness number.
2. Apply hardness before section damage allocation.
3. Keep cockpit/torso/arm/leg section health and destruction events.
4. Add validator case comparing armored and unarmored damage outcome.
5. Document the rule in one sentence: armor plates add overall hardness, but they do not erase section damage.
6. Do not add per-location armor plate bookkeeping in first version.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-armor-hardness-lock.log"
```

**Acceptance:**

- Armor math is simple and deterministic.
- Section destruction still exists.
- Rule is cheap enough for later server validation.

**Commit:** `Lock armor hardness damage rule`

## 7. Stage 4: MechLab Feel Lock

Goal: MechLab should feel like fitting parts into a chassis, not editing a spreadsheet.

### Task 4.1: Audit Mounted Weapon Semantics

**Goal:** Remove or quarantine any player-facing weapon enable/disable concept.

**Files:**

- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Search for `enabledWeapons`, `weapon.*toggle`, `toggle.*weapon`, `Enable`, `Disable`.
2. Classify every hit as player-facing, internal implementation or unrelated Unity wording.
3. Rename player-facing concepts to mounted/unmounted or fitted/unfitted.
4. Keep installed weapon always active.
5. Add guard if a smoke/test path can still disable a mounted weapon from the UI.

**Validation:**

```powershell
rg -n "enabledWeapons|weapon.*toggle|toggle.*weapon|Enable|Disable" unity-mc2-demo/Assets/Scripts unity-mc2-demo/Assets/Editor
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-mounted-weapon-semantics.log"
```

**Acceptance:**

- Player cannot see or use weapon toggles.
- Mounted weapon means active weapon.

**Commit:** `Audit mounted weapon semantics`

### Task 4.2: Make MechLab Grid Blocks Explicit

**Goal:** Weapons are contiguous multi-cell blocks; armor plates and heat sinks are single-cell fillers.

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutContract.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutValidator.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify if needed: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-loadout-compact.txt`

**Steps:**

1. Confirm every chassis has grid width, height, blocked cells and equipment slots.
2. Confirm every weapon has a shape, including vertical multi-cell weapons.
3. Render each weapon as one visual block spanning all occupied cells.
4. Render armor plate and heat sink as one-cell pieces.
5. Show conflict, out-of-bounds, overweight and overheat immediately.
6. Keep controls future-mobile-friendly: select item, click target cell, optional nudge.
7. Smoke the compact loadout screen.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-mechlab-grid-blocks.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-loadout-compact.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-mechlab-grid-blocks.log"
```

**Acceptance:**

- Player sees why a weapon fits or does not fit.
- Armor plates and heat sinks clearly fill remaining cells.
- No nested or noisy UI blocks.

**Commit:** `Make MechLab grid blocks explicit`

### Task 4.3: Prove Loadout Battle Effects

**Goal:** MechLab changes enter BattleCore and alter combat behavior.

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/UnitState.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/MechBayInventoryContract.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify if needed: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-loadout-compact.txt`

**Steps:**

1. Confirm mounted weapons affect damage, range, cooldown and heat per shot.
2. Confirm heat sinks affect heat dissipation or heat-lock risk.
3. Confirm armor hardness affects damage outcome.
4. Confirm weight can affect movement or at least displayed readiness.
5. Add validator evidence for displayed values used by BattleCore.
6. Add smoke evidence if loadout change should alter combat summary.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-loadout-battle-effect.log"
```

**Acceptance:**

- Changing weapon, armor or heat sink has observable rule impact.
- First version remains deterministic.

**Commit:** `Prove loadout battle effects`

## 8. Stage 5: Debrief And Relaunch Loop

Goal: Do not build a heavy save system; do finish the natural play loop.

### Task 5.1: Simplify Debrief Player Flow

**Goal:** Debrief shows result, damage, reward, repair and next action without save-slot concepts.

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-debrief-summary.txt`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Debrief shows mission result.
2. Debrief shows completed objectives.
3. Debrief shows player mech damage.
4. Debrief shows salvage/parts and one-token payout.
5. Debrief shows repair action and back-to-MechLab/relaunch action.
6. Hide save-slot, continue-company and account-management copy from normal player flow.
7. Smoke the debrief summary.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-debrief-summary.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-debrief-summary.log"
```

**Acceptance:**

- Player knows what happened and what to do next.
- Normal flow does not ask player to manage saves.

**Commit:** `Simplify debrief player flow`

### Task 5.2: Guard Repair And Relaunch Loop

**Goal:** Destroyed or damaged player mechs are repaired with token cost, no waiting timer, no permanent loss.

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/MechBayInventoryContract.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Confirm repair cost calculation covers structure and section damage.
2. Confirm ordinary weapon loss is a repair/rebuy cost, not a blocked flow.
3. Confirm repair is immediate.
4. Confirm repaired units can relaunch the same mission.
5. Confirm no waiting timer appears in first-version UI.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-repair-relaunch.log"
```

**Acceptance:**

- Repair all is immediate.
- Relaunch does not require save management.

**Commit:** `Guard repair and relaunch loop`

## 9. Stage 6: AI Commander Capability Window

Goal: AI feels like an optional deputy, not the engine of the battle.

### Task 6.1: Freeze Compact Observation

**Goal:** Observation is small, stable and useful for high-level decisions.

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs`
- Modify: `docs-ai-commander-directive-contract.md`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Include mission phase.
2. Include objective summary.
3. Include player unit health, section damage and detached command state.
4. Include active hostile pressure summary.
5. Include nearby threats and available high-level intents.
6. Exclude per-frame projectile data and full path graphs.
7. Add validator or command export evidence.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-observation.log"
```

**Acceptance:**

- Observation is compact enough for slow model calls.
- Local battle runs without AI.

**Commit:** `Freeze AI observation contract`

### Task 6.2: Guard Directive Adapter

**Goal:** AI directive becomes ordinary BattleCore commands and has safe fallback.

**Files:**

- Create or Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/AiCommanderDirective.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderCommandPort.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/RuleCommander.cs`
- Modify: `docs-ai-commander-directive-contract.md`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Define directive types: attack, focusTarget, defend, regroup, hold, retreat, protectUnit.
2. Convert directive to existing commander command.
3. Add timeout and invalid-response fallback to local rule commander.
4. Ensure AI cannot mutate mission state directly.
5. Validate one directive conversion end to end.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-directive-adapter.log"
```

**Acceptance:**

- Model output is optional and bounded.
- Invalid or slow response does not block battle.

**Commit:** `Guard AI directive adapter`

### Task 6.3: Show Optional AI Advice Window

**Goal:** UI shows one compact tactical suggestion when AI is available.

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/MiniMaxCommander.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/README.md`

**Steps:**

1. Add a compact optional advice row or small panel.
2. Show one short tactical suggestion.
3. Keep player commands higher priority.
4. If API key is unavailable, show offline/disabled state without breaking Demo.
5. Do not spend tokens by default in normal smoke tests.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-advice-window.log"
```

**Acceptance:**

- AI reads as deputy capability, not chat console.
- No API key still gives a complete local Demo.

**Commit:** `Show optional AI advice window`

## 10. Stage 7: Content Boundary And Public Safety

Goal: local reference validation remains useful, public repo/build remains safe.

### Task 7.1: Document Private Reference Content Boundary

**Files:**

- Modify: `docs-content-replacement-plan.md`
- Modify: `docs-content-pack.md`
- Modify: `README.md`
- Modify: `unity-mc2-demo/README.md`

**Steps:**

1. State private reference assets are local development validation only.
2. State public release must use project-owned or properly licensed content.
3. Document replaceable layers: identity, UI text, mech/weapon/pilot/faction data, mission scripts, models, textures, effects, audio, icons and provenance.
4. Avoid copy that sells the project as a clone.

**Validation:**

```powershell
rg -n "MechCommander|MechWarrior|原版|复刻|reference|private|public" README.md unity-mc2-demo/README.md docs-content-replacement-plan.md docs-content-pack.md
git diff --check
```

**Acceptance:**

- Public-facing text emphasizes AI-assisted tactical RTS exploration.
- Private reference use is clearly development-only.

**Commit:** `Document private reference content boundary`

### Task 7.2: Add Public Build Content Safety Check

**Files:**

- Modify: `scripts/unity/*`
- Modify: `.gitignore`
- Modify: `unity-mc2-demo/README.md`

**Steps:**

1. Add or document public build mode that rejects private reference output in public packages.
2. Confirm generated screenshots/logs stay ignored.
3. Confirm ignored local reference paths are not accidentally included in release artifacts.
4. Keep local private development build available.

**Validation:**

```powershell
git status --short --ignored
git check-ignore -v analysis-output/reference-visual-captures/spawn.png
git diff --check
```

**Acceptance:**

- Public build path is safer.
- Private validation path remains useful.

**Commit:** `Add public build content safety check`

## 11. Stage 8: Demo Handoff

Goal: another person can run, understand and show the Demo.

### Task 8.1: Write Three-Minute Demo Walkthrough

**Files:**

- Create: `docs-demo-walkthrough-2026-06-07.md`
- Modify: `README.md`
- Modify: `unity-mc2-demo/README.md`

**Steps:**

1. Script flow: start, MechLab, inspect fit, launch, squad move, focus target, solo command, Jet, damage, debrief, repair, relaunch.
2. Link local capture presets for each beat.
3. Use project-owned wording: AI-assisted tactical RTS, mech squad command, deterministic BattleCore, optional AI deputy.
4. Avoid original IP references in outward-facing text.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- Another person can demo the build from the document.
- Value is clear without private reference content.

**Commit:** `Add playable demo walkthrough`

### Task 8.2: Prepare Repeatable Windows Demo Build

**Files:**

- Modify: `scripts/unity/*`
- Modify: `unity-mc2-demo/README.md`
- Modify: `README.md`
- Optional Modify: `docs-demo-walkthrough-2026-06-07.md`

**Steps:**

1. Provide one command to build Windows player.
2. Provide one command to validate.
3. Provide one command to smoke visible flow.
4. Provide one command to capture reference screenshots.
5. Document optional private reference pack requirement.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-demo-handoff.log"
```

**Acceptance:**

- README can guide another developer.
- Build, validator, smoke and capture commands are explicit.

**Commit:** `Prepare repeatable Windows demo build`

### Task 8.3: Package Demo Evidence

**Files:**

- Modify: `docs-demo-walkthrough-2026-06-07.md`
- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify: `README.md`

**Steps:**

1. Pick 3-5 screenshots: MechLab, spawn/airfield, contact, damage/ejection and debrief.
2. Write concise captions focused on simple squad command, AI-assisted tactical decisions, mech fitting, BattleCore simulation and replaceable content packs.
3. Avoid original IP references in outward-facing text.
4. Record exact build/capture commands that produced the evidence.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- Demo has a clean story for investors or collaborators.
- Evidence is local and reproducible.

**Commit:** `Package playable demo evidence`

## 12. Later Platform Plan

These are intentionally deferred until the Windows local Demo is stable.

| Later Phase | Start Condition | Scope |
| --- | --- | --- |
| P1 Main server prototype | Local battle and MechLab loop are stable | account id, inventory snapshot, token ledger, signed loadout, reward claim, leaderboard |
| P2 Map package/editor loop | BattleCore contract stable enough for external maps | map package schema, local editor export, uncertified map play |
| P3 Certified reward maps | Main server prototype exists | certification states, session tickets, reward caps, validation |
| P4 Partner/community map servers | reward validation works | map server protocol, reputation, replay/digest upload |
| P5 Creator economy and optional chain | off-chain economy and moderation proven | creator revenue accounting, optional Ethereum/L2 proof or settlement |

Long-term rules:

- Maps can be open.
- Cross-map rewards must be certified by the main server.
- Third-party map servers cannot directly mint portable assets.
- Web ranking reads certified results.
- Chain integration is optional later accounting/proof, not first-demo gameplay.

## 13. Immediate Commit Queue

Recently completed:

1. `Guard visible playable flow`
2. `Document visible flow capture baseline`
3. `Audit battle occupancy evidence`
4. `Show collision occupancy placeholders`
5. `Tune hangar encounter composition`
6. `Regress weapon family cues`

Next commits:

1. `Lock section damage and ejection cues`
2. `Lock armor hardness damage rule`
3. `Audit mounted weapon semantics`
4. `Make MechLab grid blocks explicit`
5. `Prove loadout battle effects`
6. `Simplify debrief player flow`
7. `Guard repair and relaunch loop`
8. `Freeze AI observation contract`
9. `Guard AI directive adapter`
10. `Show optional AI advice window`
11. `Document private reference content boundary`
12. `Add public build content safety check`
13. `Add playable demo walkthrough`
14. `Prepare repeatable Windows demo build`
15. `Package playable demo evidence`

Every commit must record:

- files changed;
- validation commands run;
- log or screenshot paths;
- remaining problem to tackle next.

## 14. Stop Conditions

Pause and fix before adding features when:

- `hangar-contact` or `damage-demo` becomes more cluttered, darker, blurrier or more UI-covered.
- A presentation-only change breaks smoke tests.
- BattleCore legal landing and Unity presentation disagree.
- Units, structures or terrain objects rely only on Unity collision without BattleCore evidence.
- Unity scene file changed only because of fileID churn.
- First-demo flow exposes complex save/load UI again.
- A task introduces server, PVP, mobile, complex economy or chain code into first-demo mainline.
- Public docs describe private reference content as product content.

The current line is simple: keep the first map readable, make weapons and damage feel like mech combat, make MechLab fitting satisfying, then package the local Windows Demo with clean public boundaries.

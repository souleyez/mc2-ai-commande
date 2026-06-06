# Playable Demo Locked Execution Implementation Plan

> **For Codex:** REQUIRED SUB-SKILL: Use executing-plans to implement this plan task-by-task.

**Goal:** 把当前 Unity 6 Windows 本地原型收成一版可演示、可截图、可讲清楚价值的轻量机甲战术指挥 Demo：机库装配直观，第一张地图可读，玩家能用极少指令完成一场战斗，AI 副官只作为高层能力窗口出现。

**Architecture:** `BattleCore` 是权威规则层，负责命令、移动、喷射落点、占位、武器、热量、装甲硬度、部位损伤、结算和 AI observation/directive。Unity Presentation 负责输入、固定镜头、UI、模型、材质、特效、截图和本地启动；Unity 可以显示碰撞占位和调试辅助，但不能成为唯一 gameplay truth。开发期可以使用本地私有参考内容包验证画面、比例和节奏，公开构建必须能替换为项目自有或合规授权内容包。

**Tech Stack:** Unity 6, C#, Windows Standalone, deterministic BattleCore, PowerShell validation/capture scripts, `mc2-unity-demo-contract-v1`, private local reference content pack, Git/GitHub.

**Revision:** 2026-06-07 fine-grained reset. This file is the single active execution plan. Older plan files are background and evidence only.

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

1. Stage 1 可见流程已经锁住：visible-flow smoke 能覆盖机库、出战、独立命令、喷射、集火、战报、回机库和再启动 identity。
2. Stage 2 战场空间、碰撞占位和 hangar encounter 构图已经收口：BattleCore occupancy、hard prop placeholder、hangar pressure spread 都有证据。
3. Stage 3 Combat Feel Lock 已完成当前锁定项：武器类型 cue、部位损伤/弹射 cue、装甲硬度规则。
4. `Regress weapon family cues` 已完成并提交为 `4ea5666`。
5. `Lock section damage and ejection cues` 已完成并提交为 `db1efa7`。
6. `Lock armor hardness damage rule` 已完成并提交为 `e7c4a07`。
7. Stage 4 MechLab 的 `Audit mounted weapon semantics` 已完成并提交为 `8010a56`。
8. `Make MechLab grid blocks explicit` 已完成验证。
9. `Prove loadout battle effects` 已完成验证：装配预览现在通过 BattleCore helper 进入 UnitState，武器、散热器、装甲硬度和重量都有 validator 证据。
10. 当前处在 Stage 5 / Debrief And Relaunch Loop，下一步优先做 C1：收简洁战报和正常玩家下一步，不暴露保存槽/账号管理概念。
11. 工作树提交前必须保持干净；Unity scene fileID churn 不得误提交，生成截图/日志/JSON 默认不进 Git。

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
| Unity build/smoke/capture | Working | Unity scene fileID churn can appear after batch runs | Check diff before every commit; never stage generated evidence by default |
| `mc2_01` map loading | Working | Only one small map is first-demo scope | Preserve trigger/map data; use this map for the vertical slice |
| Terrain/water/roads | Acceptable | Later edge polish only | Regression only unless screenshots get darker or unreadable |
| Model/reference asset loading | Working locally | Public replacement boundary still needs guard | Keep private pack local; document replaceable public content pack |
| Occupancy/collision | Evidence present | Must remain BattleCore-backed | Keep validator and sidecar guards; Unity placeholders stay review-only |
| Command flow | Smoke-covered | Keep UI simple | Regression only unless player flow breaks |
| Weapon effects | Completed for current pass | Needs only regression unless `damage-demo` becomes unreadable | Guard with combat situation smoke and damage capture |
| Section damage/ejection | Completed for current pass | Future polish can add closer motion/capture readability | Regression only unless `damage-demo` regresses |
| Armor hardness | Completed for current pass | Future tuning can adjust numbers, not the rule shape | Regression only |
| MechLab | Active | Mounted weapon semantics and grid block cues are cleaned; loadout values still need battle-effect proof | Start Stage 4.3 |
| Debrief/repair | Basic | Needs clean first-demo loop, no save UI in normal flow | Stage 5 |
| AI deputy | Experimental | Needs compact optional capability window and offline fallback | Stage 6 |
| Public safety | Partially documented | Needs build/content guard and README cleanup where needed | Stage 7 |
| Demo handoff | Incomplete | Needs walkthrough and repeatable package story | Stage 8 |

## 5.1 Milestone Gates

These gates decide whether to move forward. If a gate fails, fix the regression before adding features.

| Gate | Must Be True | Evidence |
| --- | --- | --- |
| G0: Worktree hygiene | No unintended Unity scene churn, no generated screenshots/logs staged, unrelated user edits preserved | `git status --short --branch`, targeted `git diff` |
| G1: Combat feel | `damage-demo` shows readable weapon direction plus at least one readable section damage/ejection event | validator, Windows build, `damage-demo` capture, visible-flow smoke |
| G2: Armor rule | Armor plates add one deterministic hardness value before section damage, without removing section destruction | validator case comparing armored/unarmored outcome |
| G3: MechLab feel | Mounted weapons are always active; grid blocks show weapon shape, armor plates and heat sinks clearly | loadout validator, `mc2_01-loadout-compact.txt` smoke |
| G4: Battle loop | Debrief, repair and relaunch are simple and do not expose save-slot management in normal flow | debrief smoke and one walkthrough pass |
| G5: AI capability | AI is optional, compact and high-level; no key or timeout still leaves the local demo playable | AI contract validator and offline UI state |
| G6: Public boundary | Public docs and build path describe project-owned AI RTS work, not private reference content as product content | README/content audit and public build safety check |

## 5.2 Definition Of Done For Every Small Commit

Each small commit must include:

1. One product-facing improvement or one guardrail, not a mixed bag.
2. Exact files changed in the audit or commit summary.
3. At least one validation command with expected success evidence.
4. Screenshot or sidecar evidence for visual work.
5. A note for the next remaining problem.

Do not commit:

- generated PNG, JSON or log evidence unless explicitly requested;
- private reference content;
- Unity scene fileID churn;
- unrelated cleanup;
- a plan-only update bundled with unvalidated gameplay code.

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

**Status:** Completed 2026-06-07.

**Result:** `DemoUnitView` now adds arm firepower-lost markers, leg danger ring/mobility beacon and cockpit escape-column/route cues. `Mc2DemoBootstrap` guards those cue fragments in the combat situation smoke summary, and `Mc2DemoValidator` now asserts that cockpit critical destruction kills the unit. Evidence is recorded in `docs-reference-visual-audit-2026-06-07.md` under `Stage 3.2 Section Damage And Ejection Cue Result`.

**Goal:** Arm loss, leg disable and cockpit ejection are visible world events and match status rows.

**Files:**

- Read: `unity-mc2-demo/Assets/Scripts/BattleCore/UnitState.cs`
- Read if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/DamageSection.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceObjMeshLibrary.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify: `docs-playable-demo-locked-execution-plan-2026-06-07.md`

**Current draft already present:**

- `DemoUnitView.SectionDamageCueSummary()` now includes arm firepower marker, leg danger ring/mobility beacon and cockpit escape-column/route cue language.
- `DemoUnitView` adds stronger world cues for destroyed arms, destroyed legs and cockpit ejection.
- `Mc2DemoBootstrap` combat situation assertion now checks the expanded cue summary.
- `Mc2DemoValidator.ValidateSectionDamageModifiers()` now adds a cockpit critical destruction assertion.

**Steps:**

1. Review the existing draft diff and keep scope limited to section damage/ejection.
2. Run `git diff --check`; fix only whitespace or doc issues if needed.
3. Run Unity validator and inspect the log for `MC2 demo contract validation OK`.
4. If validator fails because the direct cockpit test is too synthetic, convert it to a small `BattleMission` fixture instead of weakening the assertion.
5. Run Windows build because presentation C# changed.
6. If Unity dirties `Assets/Scenes/Mc2Demo.unity` with fileID churn, restore the scene manually before staging.
7. Capture `damage-demo`.
8. Inspect `analysis-output/reference-visual-captures/damage-demo.png` directly.
9. Run visible-flow smoke because `assert-combat-situation` now checks the expanded section cue summary.
10. Add an audit entry to `docs-reference-visual-audit-2026-06-07.md` with files, commands, evidence, observed effect and remaining issue.
11. Stage only the three source files plus the two docs.
12. Commit as `Lock section damage and ejection cues`.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-section-damage-lock.log"
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-section-damage-lock.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets damage-demo
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-visible-flow-after-section-damage-lock.log"
```

**Acceptance:**

- `damage-demo` shows at least one severe section damage event.
- Cockpit ejection is visual, not only log text.
- World cue and status row agree.
- Smoke summary includes the expanded section cue fragments.
- Existing weapon-family cue and occupancy evidence do not regress.

**Commit:** `Lock section damage and ejection cues`

### Task 3.3: Lock Armor Hardness Damage Rule

**Status:** Completed 2026-06-07.

**Result:** Armor plates remain one overall hardness value. Validator coverage now proves the same direct hit damages armored units less than unarmored units, combat events surface mitigation, and high enough damage still destroys a non-critical section on an armored unit. The design rule is documented in `docs-mc2-detailed-development-plan.md`.

**Goal:** Armor plates increase one overall hardness value; section damage remains the fun part.

**Files:**

- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/UnitState.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutValidator.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `docs-mc2-detailed-development-plan.md`

**Steps:**

1. Inspect current armor-related fields in `CombatLoadoutPreview`, `LoadoutValidator` and `UnitState`.
2. Confirm armor plate cells accumulate into one hardness number, not per-body-part armor.
3. Ensure hardness reduces incoming damage before section damage allocation.
4. Keep cockpit/torso/arm/leg section health and destruction events.
5. Add one validator setup with two otherwise identical units: unarmored and armored.
6. Apply the same direct or simulated hit to both units.
7. Assert the armored unit takes lower structure or section damage.
8. Assert section damage still occurs when damage is high enough.
9. Document the rule in one sentence: armor plates add overall hardness, but they do not erase section damage.
10. Do not add per-location armor plate bookkeeping in first version.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-armor-hardness-lock.log"
```

**Acceptance:**

- Armor math is simple and deterministic.
- Section destruction still exists.
- Rule is cheap enough for later server validation.
- The rule is understandable from MechLab UI copy and validator evidence.

**Commit:** `Lock armor hardness damage rule`

## 7. Stage 4: MechLab Feel Lock

Goal: MechLab should feel like fitting parts into a chassis, not editing a spreadsheet.

### Task 4.1: Audit Mounted Weapon Semantics

**Status:** Completed 2026-06-07.

**Result:** The only weapon-specific enable/disable naming was the internal loadout preview mask and its validator coverage. It now uses mounted/unmounted terminology, and the rg audit no longer finds weapon toggle language. Mounted weapon still means active weapon.

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

**Status:** Completed 2026-06-07.

**Result:** `Mc2DemoBootstrap` now draws stronger MechLab block frames for contiguous mounted weapon shapes, adds internal cell dividers, adds selected-block framing, and gives armor/heat-sink filler cells their own single-cell frame language. The loadout compact smoke now guards `GridBlock=outer-frame+contiguous-weapon+cell-dividers+single-cell-filler+shape-label` and reports `preview=weaponBlock:yes/filler:target/shape:yes` for the default Bushwacker fitting surface.

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

**Status:** Completed 2026-06-07.

**Result:** `UnitLoadoutCombatOverrideBuilder` now owns the preview-to-combat override path in BattleCore. Unity presentation delegates to it, and validator coverage proves full mounted loadouts preserve combat stats, reduced mounted weapons reduce battle-ready heat/weight, armor fillers apply hardness, and heat-sink fillers apply cooling.

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
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-loadout-battle-effects.log"
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-loadout-battle-effects.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-visible-flow-after-loadout-battle-effects.log"
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

## 13. Current Plan Snapshot

Use this section when the next user message is "按计划继续". Start at the first `Next` item unless the user changes priority.

Recently completed:

| Order | Commit | Result | Evidence |
| --- | --- | --- | --- |
| 1 | `Guard visible playable flow` | MechLab to battle to debrief to relaunch flow is smoke-covered | visible-flow smoke |
| 2 | `Document visible flow capture baseline` | Baseline screenshots and sidecars are recorded | visual audit |
| 3 | `Audit battle occupancy evidence` | BattleCore reports units, structures, hard props and landing predicate evidence | validator, sidecar |
| 4 | `Show collision occupancy placeholders` | Optional hard-object footprint review layer exists | capture sidecars |
| 5 | `Tune hangar encounter composition` | Enemy pressure spreads across player squad without reducing encounter pressure | validator, build, capture |
| 6 | `Regress weapon family cues` | Energy, missile, ballistic and explosive cue language is guarded | validator, build, `damage-demo`, smoke |
| 7 | `Lock section damage and ejection cues` | Arm loss, leg disable and cockpit ejection have guarded world cues | validator, build, `damage-demo`, smoke |
| 8 | `Lock armor hardness damage rule` | Armor is one deterministic hardness value before section damage | validator |
| 9 | `Audit mounted weapon semantics` | Mounted weapon means active weapon; no player-facing weapon toggle remains | `rg` audit, validator |
| 10 | `Make MechLab grid blocks explicit` | MechLab grid shows block frames, cell dividers and single-cell filler language | validator, build, loadout smoke |
| 11 | `Prove loadout battle effects` | Fitted weapons, armor fillers and heat-sink fillers now feed BattleCore UnitState combat stats | validator, build, visible-flow smoke |

Current state:

- Current stage: Stage 5 / Debrief And Relaunch Loop.
- Current next commit: `Simplify debrief player flow`.
- Current demo risk: combat and MechLab rules are now guarded, but the debrief still exposes too much system/save-management wording for a clean first-player flow.
- Current build risk: Unity batch build can dirty `Assets/Scenes/Mc2Demo.unity` with fileID churn; inspect and restore before commit.
- Current content risk: private reference content can be used locally for validation but must stay out of Git and public packages.

Next commits:

| Order | Status | Commit | Gate |
| --- | --- | --- | --- |
| B2 | Done | `Make MechLab grid blocks explicit` | G3 MechLab feel |
| B3 | Done | `Prove loadout battle effects` | G3 MechLab feel |
| C1 | Next | `Simplify debrief player flow` | G4 Battle loop |
| C2 | Pending | `Guard repair and relaunch loop` | G4 Battle loop |
| D1 | Pending | `Freeze AI observation contract` | G5 AI capability |
| D2 | Pending | `Guard AI directive adapter` | G5 AI capability |
| D3 | Pending | `Show optional AI advice window` | G5 AI capability |
| E1 | Pending | `Document private reference content boundary` | G6 Public boundary |
| E2 | Pending | `Add public build content safety check` | G6 Public boundary |
| E3 | Pending | `Add playable demo walkthrough` | Demo handoff |
| E4 | Pending | `Prepare repeatable Windows demo build` | Demo handoff |
| E5 | Pending | `Package playable demo evidence` | Demo handoff |

Every commit must record:

- files changed;
- validation commands run;
- log or screenshot paths;
- whether Unity scene churn appeared and how it was handled;
- remaining problem to tackle next.

## 14. Fine-Grained Execution Queue

These cards are intentionally small. Each card should be one commit unless the task uncovers a real blocking bug.

### B2: Make MechLab Grid Blocks Explicit

**Goal:** Make MechLab read like fitting physical parts: mounted weapons are contiguous shape blocks, armor plates and heat sinks are single-cell fillers, and selected/invalid states are obvious without weapon enable toggles.

**Files:**

- Read: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if rule evidence is missing: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify: `docs-playable-demo-locked-execution-plan-2026-06-07.md`

**Steps:**

1. Inspect `DrawProjectedLoadoutGrid`, `LoadoutBlockRect`, `DrawLoadoutBlockLabel`, `LoadoutBlockLabel`, `LoadoutBlockDetailText` and `BuildLoadoutCompactAssertion`.
2. Preserve existing cell projection rules; do not add drag-only interaction.
3. Add stronger block framing for contiguous weapon cells: outer frame, selected frame and subtle internal cell dividers.
4. Keep armor plate and heat sink fillers as single-cell blocks with different filler language.
5. Add or tighten a compact assertion summary such as `GridBlock=outer-frame+contiguous-weapon+cell-dividers+single-cell-filler+shape-label`.
6. Run validator and loadout compact smoke.
7. Update the visual audit and this plan with result, evidence and next gap.
8. Commit only code/docs for this task; do not stage generated logs or screenshots.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-mechlab-grid-blocks.log"
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-mechlab-grid-blocks.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-loadout-compact.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-mechlab-grid-blocks.log"
```

**Acceptance:**

- Mounted weapons look like placed blocks rather than independent colored cells.
- Armor and heat sink cells look like fillers, not weapons.
- Smoke output proves the grid cue summary.
- No player-facing weapon enable/disable control is introduced.

**Commit:** `Make MechLab grid blocks explicit`

### B3: Prove Loadout Battle Effects

**Goal:** Prove that what the player fits in MechLab changes BattleCore combat behavior, not only UI preview text.

**Status:** Completed 2026-06-07.

**Result:** BattleCore now has a shared `UnitLoadoutCombatOverrideBuilder` that converts `CombatLoadoutPreview` into battle-ready UnitState overrides. Presentation calls that helper instead of rebuilding loadout stats locally. Validator evidence covers full fitted weapons, reduced mounted weapons, armor filler hardness and heat-sink cooling.

**Files:**

- Read: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Read: `unity-mc2-demo/Assets/Scripts/BattleCore/UnitState.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/MechBayInventoryContract.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify: `docs-playable-demo-locked-execution-plan-2026-06-07.md`

**Steps:**

1. Locate existing validator coverage for preview heat, weight, slot count and armor hardness.
2. Add an assertion that a mounted weapon contributes range, damage, cooldown or heat to the combat-ready unit.
3. Add an assertion that removing or changing a mounted weapon changes the combat-ready weapon list.
4. Add an assertion that heat sinks and armor plates affect combat stats through the same loadout path.
5. Keep numbers tied to existing catalogs; do not rebalance weapons in this task.
6. Run validator and, if presentation was touched, visible-flow smoke.
7. Record evidence.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-loadout-battle-effects.log"
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-loadout-battle-effects.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-visible-flow-after-loadout-battle-effects.log"
```

**Acceptance:**

- Validator proves weapon fitting affects battle-ready weapon data.
- Validator proves armor and heat sink fitting remain on the BattleCore path.
- No economy, save system or broad MechLab redesign is introduced.

**Commit:** `Prove loadout battle effects`

### C1: Simplify Debrief Player Flow

**Goal:** After a mission, show only what the player needs: result, objective state, damage, salvage/payout, repair and relaunch path.

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Read/Modify: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-debrief-summary.txt`
- Modify: `docs-playable-demo-locked-execution-plan-2026-06-07.md`

**Steps:**

1. Inspect current debrief screen and smoke summary.
2. Remove or hide normal-flow save-slot/account-management wording from the debrief surface.
3. Keep result, objectives, damaged units, destroyed sections, salvage/payout and next action.
4. Add a clear repair/relaunch action if it is not already visible.
5. Keep diagnostics available only through explicit scripts or hidden developer paths.
6. Run debrief smoke.
7. Record evidence.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-debrief-summary.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-debrief-summary.log"
```

**Acceptance:**

- Player understands mission result and next click.
- Normal flow does not ask the player to manage save slots.

**Commit:** `Simplify debrief player flow`

### C2: Guard Repair And Relaunch Loop

**Goal:** Damaged mechs can be repaired immediately with token cost and relaunched without waiting or save management.

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/MechBayInventoryContract.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify if needed: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-visible-flow-audit.txt`
- Modify: `docs-playable-demo-locked-execution-plan-2026-06-07.md`

**Steps:**

1. Confirm repair cost calculation covers unit health and damaged sections.
2. Confirm ordinary weapon loss is modeled as repair/rebuy cost, not permanent first-demo loss.
3. Confirm repair is immediate and consumes only the one token/currency concept.
4. Add validator or smoke assertion that repair restores launch eligibility.
5. Confirm relaunch uses the repaired loadout.
6. Run validator and visible-flow smoke.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-repair-relaunch.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-repair-relaunch.log"
```

**Acceptance:**

- Repair all is immediate.
- Repaired units can relaunch the same mission.
- No waiting timer or complex save UI appears.

**Commit:** `Guard repair and relaunch loop`

### D1: Freeze Compact AI Observation

**Goal:** Keep model input small enough for high-latency AI calls and useful enough for high-level tactical advice.

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs`
- Modify: `docs-ai-commander-directive-contract.md`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `docs-playable-demo-locked-execution-plan-2026-06-07.md`

**Steps:**

1. Include mission phase, objective summary and commander identity.
2. Include player unit health, section damage and detached command state.
3. Include hostile pressure summary and nearby threat summary.
4. Include available high-level intents.
5. Exclude full projectile history, full path graphs and per-frame data.
6. Add validator/export evidence that observation remains compact and stable.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-observation.log"
```

**Acceptance:**

- Observation is compact enough to send to a slow model.
- Local battle runs without AI.

**Commit:** `Freeze AI observation contract`

### D2: Guard AI Directive Adapter

**Goal:** Convert model output into ordinary BattleCore commands with strict fallback and no direct mission mutation.

**Files:**

- Create or Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/AiCommanderDirective.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderCommandPort.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/RuleCommander.cs`
- Modify: `docs-ai-commander-directive-contract.md`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `docs-playable-demo-locked-execution-plan-2026-06-07.md`

**Steps:**

1. Define directive types: attack, focusTarget, defend, regroup, hold, retreat, protectUnit.
2. Parse invalid model output into no-op or local rule fallback.
3. Convert valid directive into existing commander command objects.
4. Add timeout and invalid-response expectations to docs.
5. Validate one directive conversion end to end.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-directive-adapter.log"
```

**Acceptance:**

- AI directive is high-level only.
- Invalid or slow model output cannot block battle.

**Commit:** `Guard AI directive adapter`

### D3: Show Optional AI Advice Window

**Goal:** Show one compact AI deputy suggestion when available, while keeping the demo complete offline.

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/MiniMaxCommander.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/README.md`
- Modify: `docs-playable-demo-locked-execution-plan-2026-06-07.md`

**Steps:**

1. Add a small advice row or compact panel, not a chat console.
2. Show one short suggestion based on the frozen observation/directive shape.
3. Keep player command priority above AI advice.
4. Show offline/disabled state if key is unavailable.
5. Ensure default smoke tests do not spend tokens.
6. Run validator and an offline-state check.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-advice-window.log"
```

**Acceptance:**

- AI reads as deputy capability, not required gameplay.
- No API key still gives a complete local demo.

**Commit:** `Show optional AI advice window`

### E1: Document Private Reference Content Boundary

**Goal:** Make the repo and README story safe: private reference content is development-only, public value is AI-assisted tactical RTS command with replaceable content packs.

**Files:**

- Modify: `docs-content-replacement-plan.md`
- Modify: `docs-content-pack.md`
- Modify: `README.md`
- Modify: `unity-mc2-demo/README.md`
- Modify: `docs-playable-demo-locked-execution-plan-2026-06-07.md`

**Steps:**

1. State private reference assets are local validation only.
2. State public release must use project-owned or properly licensed content.
3. Document replaceable layers: data, names, UI text, models, textures, effects, audio, icons and provenance.
4. Keep outward pitch centered on AI-assisted tactical RTS, deterministic BattleCore and optional AI deputy.
5. Search for public-facing wording that markets the project as a clone.

**Validation:**

```powershell
rg -n "MechCommander|MechWarrior|原版|复刻|clone|reference|private|public" README.md unity-mc2-demo/README.md docs-content-replacement-plan.md docs-content-pack.md
git diff --check
```

**Acceptance:**

- Public-facing docs do not sell private reference content as product content.
- Private reference use remains clearly local and replaceable.

**Commit:** `Document private reference content boundary`

### E2: Add Public Build Content Safety Check

**Goal:** Make it harder to accidentally package private reference content into a public build.

**Files:**

- Modify: `scripts/unity/*`
- Modify: `.gitignore`
- Modify: `unity-mc2-demo/README.md`
- Modify: `docs-content-replacement-plan.md`
- Modify: `docs-playable-demo-locked-execution-plan-2026-06-07.md`

**Steps:**

1. Identify ignored private reference and capture output paths.
2. Add or document a public build mode that rejects private reference pack inputs.
3. Confirm generated screenshots, sidecars and logs stay ignored.
4. Confirm local private validation build path still works.
5. Keep the check simple and explicit; no server or account work in this task.

**Validation:**

```powershell
git status --short --ignored
git check-ignore -v analysis-output/reference-visual-captures/spawn.png
git diff --check
```

**Acceptance:**

- Public build path fails closed if private reference paths leak into package inputs.
- Local private validation path remains available.

**Commit:** `Add public build content safety check`

### E3: Add Playable Demo Walkthrough

**Goal:** Give collaborators or investors a three-minute script that shows MechLab, battle command, damage, debrief and relaunch without needing project context.

**Files:**

- Create: `docs-demo-walkthrough-2026-06-07.md`
- Modify: `README.md`
- Modify: `unity-mc2-demo/README.md`
- Modify: `docs-playable-demo-locked-execution-plan-2026-06-07.md`

**Steps:**

1. Script the flow: start, MechLab, inspect fit, launch, move, focus target, solo command, jet, damage, debrief, repair, relaunch.
2. Link capture presets for the beats: `spawn`, `airfield`, `hangar-contact`, `damage-demo`, `north-patrol`.
3. Use project-owned language: AI-assisted tactical RTS, mech squad command, deterministic BattleCore, optional AI deputy.
4. Keep private reference content as local validation language only.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- Another person can demo the build from the document.
- Value is clear without public original-IP references.

**Commit:** `Add playable demo walkthrough`

### E4: Prepare Repeatable Windows Demo Build

**Goal:** Make build, validate, smoke and capture commands explicit enough to repeat on this machine.

**Files:**

- Modify: `scripts/unity/*`
- Modify: `README.md`
- Modify: `unity-mc2-demo/README.md`
- Modify if needed: `BUILD-WIN.md`
- Modify if needed: `docs-demo-walkthrough-2026-06-07.md`
- Modify: `docs-playable-demo-locked-execution-plan-2026-06-07.md`

**Steps:**

1. Provide one command to build Windows player.
2. Provide one command to run validator.
3. Provide one command to run visible-flow smoke.
4. Provide one command to capture reference screenshots.
5. Document optional private reference pack requirement.
6. Run Windows build.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-demo-handoff.log"
```

**Acceptance:**

- README can guide another developer.
- Build, validator, smoke and capture commands are explicit.

**Commit:** `Prepare repeatable Windows demo build`

### E5: Package Playable Demo Evidence

**Goal:** Turn local evidence into a clean demo story: fitting, launch, command, damage, debrief, repair and relaunch.

**Files:**

- Modify: `docs-demo-walkthrough-2026-06-07.md`
- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify: `README.md`
- Modify: `docs-playable-demo-locked-execution-plan-2026-06-07.md`

**Steps:**

1. Pick 3-5 local evidence beats: MechLab, spawn/airfield, hangar contact, damage/ejection and debrief.
2. Write concise captions focused on simple squad command, AI-assisted decisions, mech fitting, BattleCore simulation and replaceable content packs.
3. Record exact build/capture commands that produced the evidence.
4. Do not commit generated PNG/JSON/log files unless explicitly requested.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- Demo has a clean story for investors or collaborators.
- Evidence is reproducible and does not package private reference content.

**Commit:** `Package playable demo evidence`

## 15. Stop Conditions

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

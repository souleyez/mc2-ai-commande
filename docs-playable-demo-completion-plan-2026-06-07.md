# Playable Demo Completion Implementation Plan

> **For Codex:** REQUIRED SUB-SKILL: Use executing-plans to implement this plan task-by-task.

**Goal:** 把 Unity 6 Windows 本地原型收成一版可演示、可截图、可讲清楚价值的轻量机甲战术指挥 Demo。

**Architecture:** `BattleCore` 是权威规则层，负责命令、移动、喷射落点、碰撞占位、武器、热量、部位损伤、结算和 AI 可观察状态。Unity Presentation 负责输入、固定镜头、UI、模型、材质、特效、截图和玩家可见流程。开发期可以使用本地私有参考内容包验证画面和节奏，公开构建必须切到可替换内容包，不能携带原版资产、文案、商标或专有名称。

**Tech Stack:** Unity 6, C#, Windows Standalone, PowerShell validation/capture scripts, deterministic BattleCore, `mc2-unity-demo-contract-v1`, private local reference content pack, Git/GitHub.

---

## 0. How To Use This Plan

这是 2026-06-07 起的主执行计划。以后继续开发时，优先打开这份文件，再按任务顺序推进。

配套文档：

- `docs-reference-visual-audit-2026-06-07.md`: 截图、sidecar 和 smoke 证据。
- `docs-playable-demo-current-detailed-plan-2026-06-07.md`: 旧主计划和历史上下文。
- `docs-content-replacement-plan.md`: 私有参考包、公开替换包和版权边界。
- `docs-ai-commander-directive-contract.md`: AI 副官 observation/directive 约束。
- `docs-platform-ecosystem-plan.md`: 地图服务器、排行、奖励认证、创作者生态长期计划。

执行规则：

1. 每个任务只做一个小提交。
2. 每个提交必须留下日志、截图、sidecar 或 `git diff --check` 证据。
3. BattleCore 规则优先于 Unity 表现；Unity 碰撞体可以帮助看见，但不能成为唯一规则来源。
4. 第一版只做 Windows 本地可玩 Demo，不做实时 PVP、服务器经济、移动端、链上、复杂保存系统。
5. Save Game 相关能力保留为隐藏诊断，不进入第一版玩家流程。
6. AI 只做高层建议和大决策接口，不逐帧控制战斗。
7. 公共文档和公开构建不宣传或携带私有参考素材。

## 1. Current State

日期：2026-06-07。

已经完成：

- Unity 6 Windows build、validator、smoke test、reference capture 能跑。
- `mc2_01` 小图已加载地形、目标、触发、单位、结构、terrain objects、导航点和源相机。
- 地形已从粉色/黑块修到可读：绿地、蓝水、岸线、道路、跑道、建筑基底都能分辨。
- 私有参考 OBJ/TGA/地形/建筑/树木/损伤节点已有本地加载链路。
- BattleCore 已有单位、targetable structure、大型 terrain object、水域、地图边界的确定性占位和落点判定。
- 敌方停车展开、视觉比例审计、固定镜头构图、遮挡淡化、sidecar 证据已建立。
- 默认全队、状态栏单选、独立命令、完成后自动归队、部分合法喷射已有 smoke 覆盖。
- 战斗 HUD 已压成更小的右侧状态块，战斗中不再展示完整任务列表。
- visible-flow smoke 已覆盖状态栏独立命令、喷射、全队移动、接敌、集火、战报、回机库、装配 compact、再启动 identity。
- Walkthrough 截图集已刷新并审计：`north-patrol` 是当前最佳演示候选，`damage-demo` 是必须继续修的损伤卖点图。
- 武器类型、命中方向、部位损伤、断臂/腿部/驾驶舱弹射已有表现基础，但还要视觉强化。
- 机库/装配已有格子、整块武器、装甲板、散热器、热量、重量、维修和小队整备基础。
- AI observation、rule commander、MiniMax 接入探索已有基础；第一版只保留能力窗口。

当前最重要缺口：

- 缺一份对外演示 walkthrough 文案，把机库、战斗、损伤、战报、回机库串成 3 分钟说明。
- `hangar-contact` 和 `damage-demo` 仍是最拥挤的视觉压力点。
- 物理碰撞占位需要继续以 sidecar 和 validator 维护，避免“看着有碰撞，规则层其实没有”。
- 装配界面还要更接近整块占格、即时合法性、简单直观的手感。
- 战斗效果要从“能跑”提升到“能看出武器、命中、损伤、弹射事件”。
- AI 副官窗口还没有自然接入第一版可见流程。
- 内容包边界仍要继续收，方便未来整包替换和公开展示。

## 2. First Demo Definition

第一版 Demo 必须能演示这条线：

1. 启动 Windows Demo。
2. 进入机库/准备界面。
3. 看见 1-6 台机甲，常规 4 台。
4. 点一台机甲，看见武器整块占格、装甲板、散热器、热量、重量、合法性。
5. 一键出战 `mc2_01` 小图。
6. 固定俯视镜头默认跟随排序第一的指挥官机甲。
7. 点击地点，默认全队移动。
8. 点击敌方或目标，全队移动攻击/集火。
9. 点状态栏某台机甲，再点地点或目标，该机甲进入独立命令。
10. 独立命令完成后自动归队，重新接受最新全队命令。
11. 点击喷射，按每台机甲自身位置和合法落点单独判定；非法单位不动，合法单位喷射。
12. 战斗中看得清地形、建筑、树木/道具、敌我单位、开火、爆炸、残骸和部位损伤。
13. 断臂、腿瘫、驾驶舱弹射能在世界表现和状态栏里看见。
14. 战斗结束进入简洁战报。
15. 一键维修/补给，回机库，再次出战同图验证改装效果。

第一版成功标准：

- 本机启动后 1 分钟内能看到战斗场景。
- 1280x720 截图不靠解释也能分辨地形、机甲、建筑、敌我和战斗状态。
- 玩家只用“点地点/目标、喷射、状态栏选机甲、暂停/系统”就能打一场。
- 机库格子能让人一眼理解“什么放在哪、为什么超重/过热、怎么改”。
- 没有网络、没有模型 API 时，本地 Demo 仍能完整运行。

## 3. Validation Bus

标准命令：

```powershell
git diff --check
```

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-current.log"
```

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-current.log"
```

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-visible-flow-audit.log"
```

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol
```

证据字符串：

- `MC2 demo contract validation OK`
- `Build Finished, Result: Success`
- `MC2 Unity demo Windows build OK`
- `MC2 demo smoke test exiting with code 0`
- `MC2 reference visual captures passed`

验证矩阵：

| Change Type | Minimum Validation | Extra Validation |
| --- | --- | --- |
| BattleCore rule | Unity validator | targeted command-file smoke |
| Command flow | command-file smoke | reference capture sidecar |
| Unity visual | Windows build | reference capture |
| UI layout | 1280x720 capture | smoke |
| MechLab rule | validator | loadout smoke and battle comparison |
| AI contract | validator or command export | timeout fallback smoke |
| Content boundary | docs + path audit | public build dry-run |
| Docs only | `git diff --check` | `git status --short` |

## 4. Stage 1: Visible Flow Lock

目标：让“人能走完、能看懂、能截图”先成立。

### Task 1.1: Close Visible-Flow Smoke Record

**Status:** Completed 2026-06-07 in the current update.

**Files:**

- Modify: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-visible-flow-audit.txt`
- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify: `docs-playable-demo-current-detailed-plan-2026-06-07.md`

**Steps:**

1. Ensure the command script asserts initial squad state.
2. Select `unit-1` from the status row.
3. Send a single-unit battle click and assert `unit-1` enters solo command.
4. Advance and assert auto-rejoin.
5. Send squad Jet and assert only legal units jump.
6. Send squad move and assert `accepted=3`.
7. Advance into encounter pacing and assert tracking/contact pressure.
8. Send squad attack against `structure-1-0` and assert `accepted=3`.
9. Force visible objective completion, open debrief, assert debrief summary.
10. Return to Mech Lab, apply local candidate/squad swap, launch again, assert identity and compact loadout.
11. Record the smoke evidence in the visual audit.

**Validation:**

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-visible-flow-audit.log"
rg -n "assertion failed|failed:|Debug.LogError|MC2 demo smoke test exiting with code 0" analysis-output\unity-player-visible-flow-audit.log
```

**Acceptance:**

- Command file reports `actions=37`.
- Smoke exits with code `0`.
- Failure scan shows no `assertion failed`, `failed:` or `Debug.LogError`.
- The script proves one end-to-end player loop, not only an isolated combat feature.

**Commit:** `Guard visible playable flow`

### Task 1.2: Capture Walkthrough Image Set

**Status:** Completed 2026-06-07. Refreshed `spawn`, `airfield`, `hangar-contact`, `damage-demo` and `north-patrol` captures. `north-patrol` is the current investor screenshot candidate, while `damage-demo` is the must-fix screenshot because it should sell section damage but the world event is still too compressed and the left status surface dominates the read.

**Files:**

- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify if needed: `scripts/unity/capture_reference_visuals.ps1`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`

**Steps:**

1. Capture `spawn,airfield,hangar-contact,damage-demo,north-patrol`.
2. Inspect every PNG directly.
3. For each preset, score: terrain readability, player squad readability, hostile readability, objective readability, UI occlusion, model/prop pile-up and weapon/damage readability.
4. Name one investor screenshot candidate.
5. Name one must-fix screenshot.
6. Record sidecar facts: active hostiles, visible hostiles, occupancy summary, reference asset scale, occlusion fade, camera offset.
7. Pick the next engineering task from screenshot evidence.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol
git diff --check
```

**Acceptance:**

- Audit doc names best and weakest screenshots.
- The next task is based on a visible screenshot failure.
- No generated PNG/JSON is staged.

**Commit:** `Document visible flow capture baseline`

### Task 1.3: Manual Walkthrough Checklist

**Files:**

- Create: `docs-demo-walkthrough-2026-06-07.md`
- Modify: `README.md`
- Modify: `unity-mc2-demo/README.md`

**Steps:**

1. Write a three-minute walkthrough: start, Mech Lab, inspect fit, launch, move, attack, status-row solo command, Jet, damage, debrief, repair and return to Mech Lab.
2. Link the matching local capture presets.
3. Use project-owned wording: AI-assisted tactical RTS, mech squad command, emergent battlefield decisions.
4. Do not market private reference content as product content.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- Another person can follow the file and demo the build.
- No original product names or story copy are needed to explain the value.

**Commit:** `Add playable demo walkthrough`

## 5. Stage 2: Battle Space And Collision Occupancy

目标：解决“看图还是堆在一起”和“碰撞占位要可证明”的问题。

### Task 2.1: Audit Occupancy Evidence Against Screenshots

**Status:** Completed 2026-06-07. Validator and refreshed `hangar-contact` / `damage-demo` captures confirm the current pile-up is not missing baseline BattleCore occupancy evidence: unit radii, targetable structure blockers, hard terrain-object blockers and water/map-bound landing predicates are all present. Remaining density is primarily encounter pressure, fixed-camera compression, left HUD weight and damage/effect scale.

**Files:**

- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/DemoTerrainView.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`

**Steps:**

1. Compare `hangar-contact.json` and `damage-demo.json` with screenshots.
2. Confirm BattleCore occupancy reports active units, unit radii, targetable structures, hard terrain-object blockers, map-bound/water landing predicate and destination fallback source.
3. If a visible hard object lacks rules evidence, add sidecar evidence first.
4. Add BattleCore rule only when there is a clear movement or landing failure.
5. Do not use Unity-only colliders as gameplay truth.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-occupancy-evidence.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets hangar-contact,damage-demo
```

**Acceptance:**

- Every obvious hard object in the selected screenshots is either BattleCore occupancy or documented presentation-only.
- If units still look stacked, the audit explains whether it is rule stacking, camera compression, or model scale.

**Commit:** `Audit battle occupancy evidence`

### Task 2.2: Add Presentation Collision Placeholders

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoTerrainView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoStructureView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`

**Steps:**

1. Add optional visual placeholder rings/ghost volumes for hard blockers.
2. Keep placeholders subtle and debug-gated.
3. Tie placeholder positions to BattleCore/contract data, not renderer guesses.
4. Add sidecar field proving placeholder count and source.
5. Do not alter movement legality in this task.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets hangar-contact,damage-demo
git diff --check
```

**Acceptance:**

- We can see where physical occupancy exists without turning the demo into a debug map.
- Presentation placeholders explain pile-up rather than hiding it.

**Commit:** `Show collision occupancy placeholders`

### Task 2.3: Tune Hangar Composition Without Reducing Pressure

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Preserve enemy activation counts and source triggers.
2. Move only deterministic parking/attack slot spread if the fight reads as one knot.
3. Keep attack slots inside real weapon ranges.
4. Validate no two pressure units share an unreadable same-center cluster.
5. Capture before/after `hangar-contact` and `damage-demo`.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-hangar-composition.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets hangar-contact,damage-demo
```

**Acceptance:**

- `hangar-contact` still feels pressured, but player/enemy/objective positions are separable.
- No enemy group is silently deleted to make the scene cleaner.

**Commit:** `Tune hangar encounter composition`

## 6. Stage 3: Combat Feel Lock

目标：让战斗像机甲战术，而不是模型互相扣血。

### Task 3.1: Regress Weapon Family Cues

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoEffectsView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatDataContract.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/UnitState.cs`

**Steps:**

1. Group weapon effects into energy, missile, ballistic and explosive families.
2. Make hit direction readable with trails, beams, arcs, muzzle/impact cues.
3. Keep all mounted weapons active; do not add weapon enable/disable controls.
4. Add capture or smoke assertion that effect families are present.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets damage-demo
git diff --check
```

**Acceptance:**

- A viewer can tell who fired and roughly what kind of weapon fired.
- Effects do not cover the mech status rows.

**Commit:** `Regress weapon family cues`

### Task 3.2: Lock Section Damage And Ejection Cues

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/UnitState.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoEffectsView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceObjMeshLibrary.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Keep cockpit, torso, arms and legs as meaningful sections.
2. Use reference nodes for detached arms/parts when available.
3. Fall back to clear project-owned placeholder fragments.
4. Show cockpit ejection as a short readable event.
5. Make status row section damage match world events.
6. Make leg disable affect movement/jet behavior or at least state feedback.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-section-damage-lock.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets damage-demo
```

**Acceptance:**

- `damage-demo` can show at least one severe section damage event.
- Cockpit ejection is visual, not only log text.
- State bar and world event agree.

**Commit:** `Lock section damage and ejection cues`

### Task 3.3: Confirm Armor Hardness Simplicity

**Files:**

- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/UnitState.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Treat armor plates as one overall hardness bonus.
2. Apply hardness before section damage.
3. Keep individual section health and destruction events.
4. Prove armored and unarmored fits produce different damage results.
5. Avoid first-version per-location armor plate bookkeeping.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-armor-hardness-lock.log"
```

**Acceptance:**

- 装甲计算简单。
- 部位损伤仍是核心卖点。
- 计算成本低，规则容易解释。

**Commit:** `Lock armor hardness damage rule`

## 7. Stage 4: MechLab Feel Lock

目标：让装配界面成为核心乐趣点。

### Task 4.1: Remove Remaining Weapon Toggle Semantics

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Search for `enabledWeapons`, `weapon toggle`, `Enable`, `Disable`.
2. Replace player-facing toggle concepts with mounted/unmounted.
3. If an internal helper remains, rename or document it as loadout presence.
4. Keep rule: weapon installed means weapon active.

**Validation:**

```powershell
rg -n "enabledWeapons|weapon.*toggle|toggle.*weapon|Enable|Disable" unity-mc2-demo/Assets/Scripts unity-mc2-demo/Assets/Editor
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-no-weapon-toggle.log"
```

**Acceptance:**

- 玩家界面不再暗示武器可以开关。
- 装上就启用，拆下才不用。

**Commit:** `Remove weapon toggle semantics`

### Task 4.2: Make Grid Blocks Original-Like

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutContract.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutValidator.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Confirm each chassis has grid width, height and blocked cells.
2. Confirm each weapon has a multi-cell shape where appropriate.
3. Render a weapon as one contiguous visual block.
4. Render armor plate and heat sink as single-cell fillers.
5. Show conflict, out-of-bounds, overweight and overheat immediately.
6. Keep controls mobile-friendly for later: select item, click target cell, optional nudge.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-mechlab-grid-blocks.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-loadout-compact.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-mechlab-grid-blocks.log"
```

**Acceptance:**

- 玩家一眼看懂武器占了哪些格子。
- 装甲板/散热器像“填剩余格子”的工具。
- 不出现武器启用/关闭按钮。

**Commit:** `Make MechLab grid blocks explicit`

### Task 4.3: Prove Loadout Changes Battle Behavior

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/UnitState.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify if needed: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-loadout-compact.txt`

**Steps:**

1. Confirm loadout affects weapon range, damage, cooldown and heat.
2. Confirm heat sinks affect heat recovery or lock risk.
3. Confirm armor hardness affects damage outcome.
4. Add validator evidence if a displayed value is not used.
5. Add smoke evidence if a changed fit should alter combat summary.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-loadout-battle-effect.log"
```

**Acceptance:**

- 机库不只是外观，配置会进入 BattleCore。
- 过热、火力、射程、装甲至少有一个能在战斗中体现。

**Commit:** `Prove loadout battle effects`

## 8. Stage 5: Debrief And Relaunch Loop

目标：不做复杂保存，但要能自然完成“出战 -> 战报 -> 维修 -> 回机库 -> 再战”。

### Task 5.1: Simplify Debrief Player Flow

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-debrief-summary.txt`

**Steps:**

1. Debrief shows mission result, completed objectives, player damage, salvage/parts, payout/funds delta and repair action.
2. Hide save-slot concepts from normal debrief.
3. Provide clear buttons: repair all, back to Mech Lab, contracts/relaunch.
4. Smoke must open debrief and assert visible summary.

**Validation:**

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-debrief-summary.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-debrief-summary.log"
```

**Acceptance:**

- 战后不要求玩家理解保存系统。
- 修复和再战路径直接。

**Commit:** `Simplify debrief player flow`

### Task 5.2: Guard Repair And Relaunch

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/MechBayInventoryContract.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Confirm damaged mechs can be repaired immediately by spending funds.
2. Confirm ordinary weapon loss is repair/rebuy cost, not a blocked flow.
3. Confirm repair never creates waiting timers in first version.
4. Confirm repaired units can relaunch the same mission.
5. Confirm destroyed player mechs are repaired, not permanently deleted.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-repair-relaunch.log"
```

**Acceptance:**

- 维修是一键资金消耗。
- 第一版没有等待维修、复杂存档或永久毁机。

**Commit:** `Guard repair and relaunch loop`

## 9. Stage 6: AI Commander Capability Window

目标：AI 做大决策和建议，不拖死本地确定性战斗。

### Task 6.1: Freeze Compact Observation

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs`
- Modify: `docs-ai-commander-directive-contract.md`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`

**Steps:**

1. Observation contains mission phase, objective summary, player unit summary, detached command state, enemy pressure summary, nearby threats and available high-level intents.
2. Exclude per-frame projectile data and full path graphs.
3. Add command export or validator evidence.
4. Keep observation small enough for slow model calls.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-observation.log"
```

**Acceptance:**

- Observation 小、稳定、适合 AI 副官。
- 没有 AI 时本地战斗无影响。

**Commit:** `Freeze AI observation contract`

### Task 6.2: Add Directive Adapter Guard

**Files:**

- Create or Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/AiCommanderDirective.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderCommandPort.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/RuleCommander.cs`
- Modify: `docs-ai-commander-directive-contract.md`

**Steps:**

1. Directive types: attack, focusTarget, defend, regroup, hold, retreat, protectUnit.
2. Adapter converts directives to existing commander commands.
3. Timeout, empty response or invalid directive falls back to local rule commander.
4. AI never mutates `BattleMission` directly.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-directive-adapter.log"
```

**Acceptance:**

- 一条高层 AI 指令能变成普通游戏命令。
- 模型慢或失败不会阻塞战斗。

**Commit:** `Guard AI directive adapter`

### Task 6.3: Show AI Advice As Optional Capability

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/MiniMaxCommander.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/README.md`

**Steps:**

1. Add a compact optional advice window or panel row.
2. Show one short tactical suggestion, not a chat console.
3. Keep player commands higher priority.
4. If API key is unavailable, show disabled/offline state without breaking Demo.
5. Do not spend tokens by default in normal smoke tests.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-advice-window.log"
```

**Acceptance:**

- AI 像副官能力窗口，不像调试台。
- 默认本地 Demo 不依赖网络。

**Commit:** `Show optional AI advice window`

## 10. Stage 7: Content Boundary And Public Safety

目标：本地可用私有参考包验证，公开仓库和公开构建安全。

### Task 7.1: Document Current Private Reference Use

**Files:**

- Modify: `docs-content-replacement-plan.md`
- Modify: `docs-content-pack.md`
- Modify: `README.md`
- Modify: `unity-mc2-demo/README.md`

**Steps:**

1. State private reference assets are for local development validation only.
2. State public release must use project-owned or properly licensed content.
3. Document replaceable pack layers: product identity, UI text, mech/weapon/pilot/faction data, mission scripts, models/textures/effects/audio/icons and provenance records.
4. Avoid product copy that sells the project as a clone.

**Validation:**

```powershell
rg -n "MechCommander|MechWarrior|原版|复刻|reference|private|public" README.md unity-mc2-demo/README.md docs-content-replacement-plan.md docs-content-pack.md
git diff --check
```

**Acceptance:**

- Public-facing README emphasizes AI RTS commander exploration.
- Private reference use is documented as development-only.

**Commit:** `Document private reference content boundary`

### Task 7.2: Add Public Build Safety Check

**Files:**

- Modify: `scripts/unity/*`
- Modify: `.gitignore`
- Modify: `unity-mc2-demo/README.md`

**Steps:**

1. Add or document a build mode that rejects private reference output in public packages.
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

目标：整理成别人能启动、能看懂、能演示的 Windows 包。

### Task 8.1: Prepare Repeatable Windows Demo Build

**Files:**

- Modify: `scripts/unity/*`
- Modify: `unity-mc2-demo/README.md`
- Modify: `README.md`
- Optional Create: `docs-demo-walkthrough-2026-06-07.md`

**Steps:**

1. Provide one command to build Windows player.
2. Provide one command to validate.
3. Provide one command to smoke the visible flow.
4. Provide one command to capture reference screenshots.
5. Document optional private reference pack requirement.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-demo-handoff.log"
```

**Acceptance:**

- 不打开 Unity Editor 也能复现构建。
- README 能引导另一个开发者跑起来。

**Commit:** `Prepare repeatable Windows demo build`

### Task 8.2: Package Investor Demo Evidence

**Files:**

- Create or Modify: `docs-demo-walkthrough-2026-06-07.md`
- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify: `README.md`

**Steps:**

1. Pick 3-5 screenshots: Mech Lab, spawn/airfield, contact, damage/ejection and debrief.
2. Write concise captions focused on simple squad command, AI-assisted tactical decisions, mech fitting, BattleCore deterministic simulation and replaceable content pack.
3. Avoid original IP references in outward-facing text.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- Demo has a clean story for investors or collaborators.
- Evidence is local and reproducible.

**Commit:** `Package playable demo evidence`

## 12. Later Platform Plan

这些不进入第一版 Windows 本地 Demo 主线，但设计时要留接口：

| Later Phase | Start Condition | Scope |
| --- | --- | --- |
| P1 Main server prototype | 本地战斗和机库稳定 | account id, inventory snapshot, token ledger, signed loadout, reward claim, leaderboard |
| P2 Map package/editor loop | BattleCore contract 稳定 | map package schema, local editor export, uncertified map play |
| P3 Certified reward maps | 主服务器原型存在 | certification states, session tickets, reward caps, validation |
| P4 Partner map servers | reward validation works | map server protocol, reputation, replay/digest upload |
| P5 Creator economy and optional chain | 经济、退款、审核先在线下验证 | creator revenue accounting, optional Ethereum/L2 proof or settlement |

长期原则：

- 地图可以开放。
- 跨地图带出的奖励必须由主服务器认证。
- 地图服务器可以第三方搭建，但奖励、排行和跨图资产不能完全相信第三方。
- 链上分账只作为后期可选结算层，不作为第一版玩法依赖。

## 13. Immediate Commit Queue

Recently completed:

1. `Guard visible playable flow`
2. `Document visible flow capture baseline`
3. `Audit battle occupancy evidence`

From the current point, recommended next commits:

1. `Show collision occupancy placeholders`
2. `Tune hangar encounter composition`
3. `Regress weapon family cues`
4. `Lock section damage and ejection cues`
5. `Lock armor hardness damage rule`
6. `Remove weapon toggle semantics`
7. `Make MechLab grid blocks explicit`
8. `Prove loadout battle effects`
9. `Simplify debrief player flow`
10. `Guard repair and relaunch loop`
11. `Freeze AI observation contract`
12. `Guard AI directive adapter`
13. `Show optional AI advice window`
14. `Document private reference content boundary`
15. `Add public build content safety check`
16. `Prepare repeatable Windows demo build`
17. `Package playable demo evidence`

每个提交结束必须记录：

- 修改文件。
- 跑过的验证命令。
- 日志或截图路径。
- 下一步仍存在的问题。

## 14. Stop Conditions

遇到这些情况先停下来修，不继续堆功能：

- `hangar-contact` 或 `damage-demo` 截图比上一轮更糊、更挤或 UI 更挡。
- Smoke test 在表现层小改后失败。
- BattleCore 合法落点和 Unity 表现不一致。
- 单位、建筑、terrain object 只在 Unity 里有碰撞，BattleCore 没证据。
- Unity 场景文件只有 fileID churn。
- 第一版流程重新露出复杂保存系统。
- 任务引入服务器、经济、PVP、移动端或链上代码，导致本地 Demo 主线变慢。
- 公开文档把私有参考素材描述成产品内容。

当前主线一句话：先把完整可见流程和战场占位钉住，再把机库格子和战斗损伤磨顺，最后补 AI 副官能力窗口和公开内容边界。

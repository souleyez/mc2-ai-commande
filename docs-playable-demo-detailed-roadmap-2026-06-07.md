# Playable Demo Detailed Roadmap Implementation Plan

> **For Codex:** REQUIRED SUB-SKILL: Use executing-plans to implement this plan task-by-task.

**Goal:** 把当前 Unity 6 Windows 原型收成一版能稳定演示的机甲战术指挥 Demo：机库装配能看懂，第一张地图能打完，战斗画面能读清，AI 副官能作为高层能力窗口出现。

**Architecture:** `BattleCore` 是权威规则层，负责任务、命令、移动、喷射、占位、武器、热量、装甲、部位损伤和结算。Unity Presentation 只负责输入、固定镜头、UI、模型/材质/特效、截图和本地启动；可以显示碰撞占位，但不能把 Unity 物理当作唯一 gameplay truth。私有参考内容包只用于本地验证画面和节奏，公开构建必须换成项目自有或合规授权内容包。

**Tech Stack:** Unity 6, C#, Windows Standalone, deterministic BattleCore, PowerShell validation/capture scripts, `mc2-unity-demo-contract-v1`, private local reference content pack, Git/GitHub.

---

## 0. Plan Status

日期：2026-06-07。

这份文档是当前最细的施工蓝图。历史和背景继续保留在：

- `docs-playable-demo-completion-plan-2026-06-07.md`: 当前主执行计划和阶段清单。
- `docs-reference-visual-audit-2026-06-07.md`: 截图、sidecar、validator 和 smoke 证据。
- `docs-mc2-detailed-development-plan.md`: 产品和架构总览。
- `docs-content-replacement-plan.md`: 私有参考内容包到公开替换包的边界。
- `docs-platform-ecosystem-plan.md`: 地图服务器、认证奖励、排行和创作者生态的长期设想。

当前已经完成：

- Unity 6 Windows build、validator、smoke test 和 reference capture 链路可运行。
- `mc2_01` 小图已经能加载地形、水域、道路、目标、触发、建筑、树木/道具、敌我单位和源相机。
- 地形从粉色/黑块修到可读，绿色地面、蓝色水面、岸线、道路/跑道、建筑基底能分辨。
- 固定俯视镜头默认跟随排序第一的指挥官机甲，支持有限缩放，不做自由旋转。
- 小队指挥规则已经建立：默认全队、状态栏点单机、单机独立命令、完成后自动归队。
- 喷射已按每台机甲单独判断合法落点，非法机甲不动，合法机甲正常位移。
- BattleCore 已有单位、结构、大型 terrain object、水域和地图边界占位证据。
- Presentation 已能调试显示碰撞占位 placeholder，sidecar 能报告占位数量和来源。
- 战斗 HUD 已收成更少信息的 compact 状态块，避免战斗中显示过多内容。
- visible-flow smoke 已覆盖从机库、战斗、喷射、集火、战报、回机库、再出战的主流程。
- 机库装配已有整块武器、装甲板、散热器、热量、重量、槽位合法性、维修和小队准备基础。
- 武器命中、部位损伤、断肢、驾驶舱弹射已经有基础表现，但仍需要强化到截图可读。
- AI observation、rule commander、MiniMax 探索已有基础，第一版只保留高层能力窗口。

当前真实阶段：

1. **正在收 Stage 2：战场空间和碰撞可读性。**
2. 下一提交应优先做 `Tune hangar encounter composition`。
3. 然后进入 Stage 3：武器、损伤、弹射和装甲硬度手感锁定。

## 1. First Demo Definition

第一版 Demo 只追求一个可信闭环：

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
11. 点击喷射，每台机甲独立判断合法落点；非法单位不动。
12. 战斗中能看清地形、建筑、树木/道具、敌我单位、开火、爆炸、残骸和部位损伤。
13. 断臂、腿瘫、驾驶舱弹射能在世界表现和状态栏里看见。
14. 战斗结束进入简洁战报。
15. 一键维修/补给，回机库，再次出战同图验证改装效果。

成功标准：

- 本机启动后 1 分钟内能看到战斗场景。
- 1280x720 截图不用解释也能分辨地形、机甲、建筑、敌我和战斗状态。
- 玩家只用“点地点/目标、喷射、状态栏选机甲、暂停/系统”就能打一场。
- 机库格子能让人一眼理解“什么放在哪、为什么超重/过热、怎么改”。
- 没有网络、没有模型 API 时，本地 Demo 仍能完整运行。

第一版明确不做：

- 实时 PVP。
- 移动端适配。
- 地图服务器和认证奖励。
- 真实充值、链上结算、可提现资产。
- 完整经济和长期养成。
- 复杂存档系统。
- 大模型逐帧控制战斗。
- 公开发布私有参考素材、专有名称、原剧情或商标。

## 2. Quality Gates

每个任务结束至少跑：

```powershell
git diff --check
```

BattleCore 或合同改动必须跑：

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-current.log"
```

Unity C# 或 presentation 改动必须跑 Windows build：

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-current.log"
```

可见流程改动必须跑：

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-visible-flow-audit.log"
```

视觉改动必须刷新截图：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol
```

成功字符串：

- `MC2 demo contract validation OK`
- `Build Finished, Result: Success`
- `MC2 Unity demo Windows build OK`
- `MC2 demo smoke test exiting with code 0`
- `MC2 reference visual captures passed`

硬性停止条件：

- `hangar-contact` 或 `damage-demo` 比上一轮更糊、更挤或 UI 更挡。
- BattleCore 合法落点和 Unity 表现不一致。
- 单位、建筑、terrain object 只在 Unity 里有碰撞，BattleCore 没证据。
- 第一版流程重新暴露复杂保存系统。
- 引入服务器、PVP、移动端、链上或复杂经济代码，拖慢本地 Demo 主线。
- 公开文档把私有参考素材描述成产品内容。

## 3. Execution Rules

- 每个任务只做一个小提交。
- 每个提交必须记录修改文件、验证命令、日志或截图路径、下一步问题。
- 先守住 BattleCore deterministic rule，再做 Unity 表现。
- 先让截图清楚，再加新功能。
- 先完成 Windows 本地 Demo，再考虑跨端。
- AI 只做高层建议、开场计划、目标优先级和托管意图，不逐帧接管战斗。
- 机库和战斗是第一版核心，平台、经济、地图服务器全部后置。

## 4. Stage 2: Battle Space And Collision Occupancy

目标：解决“看图还是堆在一起”和“有物理碰撞占位但必须可证明”的问题。

### Task 2.3: Tune Hangar Encounter Composition

**Status:** Next.

**Goal:** 保留战斗压力和敌人数量，但把 `hangar-contact` 的敌我、目标、硬物读清，不再像所有东西挤在一个点。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify: `docs-playable-demo-completion-plan-2026-06-07.md`

**Steps:**

1. Read the current enemy activation, ambush parking and attack-slot logic in `BattleMission.cs`.
2. Write down current enemy group counts and trigger source names from sidecar/logs.
3. Add or tighten validator checks for enemy attack slot spread and infantry ambush parking spread.
4. Adjust deterministic attack/parking offsets only; do not delete enemy groups.
5. Keep attack offsets inside real weapon ranges.
6. Capture `hangar-contact` and `damage-demo`.
7. Inspect screenshots and sidecars together.
8. Record whether remaining density is rule stacking, camera compression, HUD weight or damage/effect scale.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-hangar-composition.log"
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-hangar-composition.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets hangar-contact,damage-demo
```

**Acceptance:**

- `hangar-contact` 仍有敌方压力，但玩家、敌人、目标建筑和硬物可分辨。
- 敌人数量和触发节奏没有被静默削弱。
- sidecar 仍能说明碰撞占位来源。

**Commit:** `Tune hangar encounter composition`

## 5. Stage 3: Combat Feel Lock

目标：让战斗像机甲战术，而不是模型互相扣血。

### Task 3.1: Regress Weapon Family Cues

**Goal:** 让能量、导弹、弹道、爆炸类武器在战术视角下有区别，且能看出攻击方向。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoEffectsView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatDataContract.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/UnitState.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Inventory current source weapon family metadata.
2. Map weapon families to presentation cues: beam, salvo arc, tracer, blast.
3. Keep every mounted weapon active; do not reintroduce enable/disable controls.
4. Ensure effect starts from plausible hardpoints.
5. Ensure hit flashes offset toward reported damage section.
6. Capture `damage-demo`.
7. Note whether a viewer can tell who fired and what kind of weapon fired.

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets damage-demo
```

**Acceptance:**

- Energy, missile and ballistic cues are visually distinct.
- Effects do not bury the mech status rows.
- Direction of incoming fire is readable at tactical zoom.

**Commit:** `Regress weapon family cues`

### Task 3.2: Lock Section Damage And Ejection Cues

**Goal:** 把断臂、腿瘫、驾驶舱弹射从“日志/小提示”拉到世界画面和状态栏都能看见。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/UnitState.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoEffectsView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceObjMeshLibrary.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Confirm cockpit, torso, arms and legs remain separate damage sections.
2. Prove destroyed arms lower firepower or at least create visible lost-weapon state.
3. Prove destroyed legs slow movement and disable Jet.
4. Use reference nodes for detached parts when available.
5. Fall back to project-owned fragments when reference nodes are missing.
6. Make cockpit ejection a short visual event: pod, chute or escape marker.
7. Make status row section icons agree with world events.
8. Capture `damage-demo`.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-section-damage-lock.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets damage-demo
```

**Acceptance:**

- `damage-demo` 至少能看到一个严重部位损伤事件。
- 驾驶舱弹射是可见事件，不只是 log。
- 世界画面和状态栏一致。

**Commit:** `Lock section damage and ejection cues`

### Task 3.3: Confirm Armor Hardness Simplicity

**Goal:** 装甲板只增加整体硬度，计算简单，但保留部位损伤和爆头/断肢乐趣。

**Files:**

- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/UnitState.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Confirm armor plates accumulate into one hardness number.
2. Apply hardness before section damage is assigned.
3. Keep section health and destruction events.
4. Add validator proof that armored and unarmored fits produce different damage outcomes.
5. Do not add per-location armor plate bookkeeping in first version.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-armor-hardness-lock.log"
```

**Acceptance:**

- 装甲规则一句话能解释。
- 部位损伤仍是核心卖点。
- 计算成本低，适合未来多人/服务器复核。

**Commit:** `Lock armor hardness damage rule`

## 6. Stage 4: MechLab Feel Lock

目标：装配界面成为核心乐趣，而不是一个数据表。

### Task 4.1: Audit Weapon Toggle Removal

**Goal:** 玩家界面不再出现武器启用/关闭概念，武器只要装配上就是启用。

**Files:**

- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Search for player-facing weapon toggle text and state.
2. Rename remaining internal concepts to mounted/unmounted or fitted/unfitted.
3. Add validator or smoke guard if a toggle path still mutates active weapons.
4. Keep mounted weapon list only as selection for slot editing.

**Validation:**

```powershell
rg -n "enabledWeapons|weapon.*toggle|toggle.*weapon|Enable|Disable" unity-mc2-demo/Assets/Scripts unity-mc2-demo/Assets/Editor
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-no-weapon-toggle.log"
```

**Acceptance:**

- 玩家看不到武器开关。
- 装上即启用，拆下才不用。

**Commit:** `Audit mounted weapon semantics`

### Task 4.2: Make Grid Blocks Original-Like

**Goal:** 武器作为整块占格物件出现，装甲板和散热器作为单格填充物，装配像拼图一样直观。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutContract.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutValidator.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Confirm each chassis has width, height and blocked cells.
2. Confirm each weapon has multi-cell shape where appropriate.
3. Render each weapon as one contiguous block.
4. Render armor plate and heat sink as single-cell fillers.
5. Show conflict, out-of-bounds, overweight and overheat immediately.
6. Keep interaction mobile-friendly for later: select item, click target cell, optional nudge.
7. Capture or smoke the compact loadout screen.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-mechlab-grid-blocks.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-loadout-compact.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-mechlab-grid-blocks.log"
```

**Acceptance:**

- 玩家一眼看懂武器占了哪些格子。
- 装甲板/散热器像填剩余格子的工具。
- 无武器启用/关闭按钮。

**Commit:** `Make MechLab grid blocks explicit`

### Task 4.3: Prove Loadout Changes Battle Behavior

**Goal:** 机库装配不是视觉 UI，配置必须进入 BattleCore 并改变战斗。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/UnitState.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify if needed: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-loadout-compact.txt`

**Steps:**

1. Confirm mounted weapons affect range, damage, cooldown and heat.
2. Confirm heat sinks affect cooling or heat lock risk.
3. Confirm armor hardness affects damage outcome.
4. Add validator proof for displayed values used by BattleCore.
5. Add smoke proof for a changed fit altering readiness or combat summary.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-loadout-battle-effect.log"
```

**Acceptance:**

- 火力、射程、热量、装甲至少各有一条验证链。
- 改装后再出战能看到不同战斗表现或数值结果。

**Commit:** `Prove loadout battle effects`

## 7. Stage 5: Debrief And Relaunch Loop

目标：不做复杂保存，但要能自然完成“出战 -> 战报 -> 维修 -> 回机库 -> 再战”。

### Task 5.1: Simplify Debrief Player Flow

**Goal:** 战报只显示玩家需要的结果、损伤、奖励、维修和下一步，不暴露保存系统。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-debrief-summary.txt`

**Steps:**

1. Debrief shows mission result, completed objectives, player damage, salvage/parts, payout/funds delta and repair action.
2. Hide save-slot concepts from normal debrief.
3. Keep buttons clear: repair all, back to Mech Lab, contracts/relaunch.
4. Smoke opens debrief and asserts summary.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-debrief-summary.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-debrief-summary.log"
```

**Acceptance:**

- 战后不要求玩家理解保存系统。
- 修复和再战路径直接。

**Commit:** `Simplify debrief player flow`

### Task 5.2: Guard Repair And Relaunch

**Goal:** 机甲损毁就是花资金修复，一键完成，没有等待和永久毁机。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/MechBayInventoryContract.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Confirm damaged mechs can be repaired immediately by spending funds.
2. Confirm ordinary weapon loss is repair/rebuy cost, not a blocked flow.
3. Confirm repair never creates first-version timers.
4. Confirm repaired units can relaunch the same mission.
5. Confirm destroyed player mechs are repaired, not permanently deleted.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-repair-relaunch.log"
```

**Acceptance:**

- 维修是一键资金消耗。
- 第一版没有等待维修、复杂存档或永久毁机。

**Commit:** `Guard repair and relaunch loop`

## 8. Stage 6: AI Commander Capability Window

目标：AI 做大决策和建议，不拖慢本地确定性战斗。

### Task 6.1: Freeze Compact Observation

**Goal:** 给模型的 observation 小而稳定，只包含大决策需要的信息。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs`
- Modify: `docs-ai-commander-directive-contract.md`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`

**Steps:**

1. Include mission phase, objective summary, player unit summary, detached command state, enemy pressure summary, nearby threats and available high-level intents.
2. Exclude per-frame projectile data, full path graphs and noisy raw logs.
3. Add command export or validator evidence.
4. Keep payload small enough for slow model calls.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-observation.log"
```

**Acceptance:**

- Observation 小、稳定、适合 AI 副官。
- 没有 AI 时本地战斗无影响。

**Commit:** `Freeze AI observation contract`

### Task 6.2: Guard Directive Adapter

**Goal:** 模型只输出高层指令，adapter 把指令转成已有玩家/指挥命令。

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
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-directive-adapter.log"
```

**Acceptance:**

- 一条高层 AI 指令能变成普通游戏命令。
- 模型慢或失败不会阻塞战斗。

**Commit:** `Guard AI directive adapter`

### Task 6.3: Show Optional AI Advice

**Goal:** AI 在第一版表现为“副官能力窗口”，不是聊天框，也不是战斗依赖。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/MiniMaxCommander.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/README.md`

**Steps:**

1. Add compact optional advice row or panel.
2. Show one short tactical suggestion.
3. Keep player command higher priority.
4. If API key is unavailable, show disabled/offline state without breaking Demo.
5. Do not spend tokens by default in smoke tests.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-advice-window.log"
```

**Acceptance:**

- AI 像一个可选副官，而不是调试台。
- 默认本地 Demo 不依赖网络或模型 API。

**Commit:** `Show optional AI advice window`

## 9. Stage 7: Content Boundary And Public Safety

目标：本地可继续用私有参考包验证，公开仓库和公开构建保持安全。

### Task 7.1: Document Private Reference Boundary

**Goal:** 明确“开发验证可参考，公开展示必须替换”的边界。

**Files:**

- Modify: `docs-content-replacement-plan.md`
- Modify: `docs-content-pack.md`
- Modify: `README.md`
- Modify: `unity-mc2-demo/README.md`

**Steps:**

1. State private reference assets are local development validation only.
2. State public release must use project-owned or properly licensed content.
3. Document replaceable layers: identity, UI text, mech/weapon/pilot/faction data, mission scripts, models/textures/effects/audio/icons and provenance.
4. Avoid copy that sells the project as a clone.

**Validation:**

```powershell
rg -n "MechCommander|MechWarrior|原版|复刻|reference|private|public" README.md unity-mc2-demo/README.md docs-content-replacement-plan.md docs-content-pack.md
git diff --check
```

**Acceptance:**

- Public-facing README emphasizes AI-assisted RTS commander exploration.
- Private reference use is documented as development-only.

**Commit:** `Document private reference content boundary`

### Task 7.2: Add Public Build Safety Check

**Goal:** 公开构建路径默认不携带私有参考内容。

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

## 10. Stage 8: Demo Handoff

目标：整理成别人能启动、能看懂、能演示的 Windows 包。

### Task 8.1: Write Three-Minute Walkthrough

**Goal:** 有一份对内/对外都能照着讲的 3 分钟 Demo 脚本。

**Files:**

- Create: `docs-demo-walkthrough-2026-06-07.md`
- Modify: `README.md`
- Modify: `unity-mc2-demo/README.md`

**Steps:**

1. Script the flow: Mech Lab, loadout grid, launch, squad move, solo order, Jet, contact, damage, debrief, repair, relaunch.
2. Link local capture presets for each beat.
3. Use project-owned wording: AI-assisted tactical RTS, mech squad command, deterministic BattleCore, optional AI deputy.
4. Do not mention private reference content as a selling point.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- Another person can demo the build from the document.
- The value is clear without original IP references.

**Commit:** `Add playable demo walkthrough`

### Task 8.2: Prepare Repeatable Windows Demo Build

**Goal:** 不打开 Unity Editor，也能复现构建、验证、smoke 和截图。

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

- README 能引导另一个开发者跑起来。
- Build、validator、smoke、capture 都有明确命令。

**Commit:** `Prepare repeatable Windows demo build`

### Task 8.3: Package Investor Demo Evidence

**Goal:** 有一组能支持融资/合作讨论的截图、说明和本地可复现证据。

**Files:**

- Modify: `docs-demo-walkthrough-2026-06-07.md`
- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify: `README.md`

**Steps:**

1. Pick 3-5 screenshots: Mech Lab, spawn/airfield, contact, damage/ejection, debrief.
2. Write concise captions focused on simple squad command, AI-assisted tactical decisions, mech fitting, BattleCore simulation and replaceable content pack.
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

## 11. Later Platform Plan

这些只保留接口，不进入第一版 Windows 本地 Demo 主线：

| Later Phase | Start Condition | Scope |
| --- | --- | --- |
| P1 Main server prototype | 本地战斗和机库稳定 | account id, inventory snapshot, token ledger, signed loadout, reward claim, leaderboard |
| P2 Map package/editor loop | BattleCore contract 稳定 | map package schema, local editor export, uncertified map play |
| P3 Certified reward maps | 主服务器原型存在 | certification states, session tickets, reward caps, validation |
| P4 Partner/community map servers | reward validation works | map server protocol, reputation, replay/digest upload |
| P5 Creator economy and optional chain | 经济、退款、审核先在线下验证 | creator revenue accounting, optional Ethereum/L2 proof or settlement |

长期原则：

- 地图可以开放，奖励必须由主服务器认证。
- 地图服务器可以第三方搭建，但不能决定跨图资产。
- 玩家资产和排行榜由主服务器控制。
- 链上只作为后期可选分账或证明层，不绑定第一版战斗。

## 12. Immediate Commit Queue

Recently completed:

1. `Guard visible playable flow`
2. `Document visible flow capture baseline`
3. `Audit battle occupancy evidence`
4. `Show collision occupancy placeholders`

Next commits:

1. `Tune hangar encounter composition`
2. `Regress weapon family cues`
3. `Lock section damage and ejection cues`
4. `Lock armor hardness damage rule`
5. `Audit mounted weapon semantics`
6. `Make MechLab grid blocks explicit`
7. `Prove loadout battle effects`
8. `Simplify debrief player flow`
9. `Guard repair and relaunch loop`
10. `Freeze AI observation contract`
11. `Guard AI directive adapter`
12. `Show optional AI advice window`
13. `Document private reference content boundary`
14. `Add public build content safety check`
15. `Add playable demo walkthrough`
16. `Prepare repeatable Windows demo build`
17. `Package playable demo evidence`

## 13. Definition Of Done

第一版可演示完成时，必须满足：

- Windows build 可重复生成。
- visible-flow smoke 通过。
- `spawn`、`airfield`、`hangar-contact`、`damage-demo`、`north-patrol` 截图全部可读。
- `damage-demo` 能看见至少一个明确部位损伤或弹射事件。
- Mech Lab 格子、热量、重量、装甲板、散热器和武器整块占格能直观看懂。
- Debrief、维修、回机库、再出战流程不依赖复杂保存系统。
- AI advice 窗口可选；无网络或无 key 时本地 Demo 不受影响。
- README、walkthrough、content boundary 文档不把私有参考内容当作公开产品内容。

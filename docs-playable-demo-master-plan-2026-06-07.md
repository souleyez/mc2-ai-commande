# Playable Demo Master Plan Implementation Plan

> **For Codex:** Execute this plan task-by-task. Each task should end with validation evidence and a small commit.

**Current detailed execution entry:** `docs-playable-demo-detailed-execution-plan-2026-06-07.md` is now the preferred day-to-day plan. This master plan keeps the broader phase background and historical context.

**Goal:** 把当前 Unity 6 Windows 原型推进到一版可演示、可截图、可继续融资的轻量机甲战术指挥 Demo：玩家能改装 1-6 台机甲，进入一张参考小地图，用极少 UI 完成移动、集火、喷射、独立命令、损伤结算和再次出战。

**Architecture:** `BattleCore` 是权威战斗核心，负责任务、命令、单位状态、命中、伤害、碰撞占位、结算和 AI 可观察状态；Unity Presentation 只负责输入、镜头、UI、材质、模型、特效和截图验证。开发期允许加载本地私有参考资源验证地形/模型/节奏，但所有公开构建必须能切到可替换内容包，不能携带原版资产、文案、商标和专有名称。

**Tech Stack:** Unity 6, C#, Windows Standalone, PowerShell validation/capture scripts, `mc2-unity-demo-contract-v1`, deterministic BattleCore, private local reference content pack, Git/GitHub.

---

## 0. 当前判断

日期：2026-06-07。

当前不是从零开发阶段，而是第一张可玩地图的可见体验收口阶段。已经完成的地基包括：

- Unity 6 Windows build、smoke test、截图 preset 可运行。
- `mc2_01` 任务合同可加载地形采样、目标、触发、单位、建筑、terrain objects、导航点。
- 小队默认全选、单机甲独立命令、喷射、自动归队、战后简报、装配预览已有基础。
- 参考 OBJ/TGA/地形/建筑/树木/损伤节点已经能在本地私有包中加载一部分。
- BattleCore 已有单位碰撞、targetable structure 占位、大型 terrain object 占位和喷射非法落点保护。
- 固定镜头遮挡淡化已提交，开火阶段右侧 UI 已压成 compact objective card。

当前最紧急的问题：

- Phase A 地形/水面/道路可读性已经完成并提交，当前不再是黑块问题，而是“单位、敌人、建筑和道具挤在一起，战术关系还不够清楚”的问题。
- Phase B / B2 比例审计、B3 碰撞占位证据和 B4 固定镜头构图已经完成，sidecar 现在能报告单位/prop 分类比例、BattleCore 占位、地形 landing predicate 和 camera composition offset；Phase C / C1 command state 和 C2 status-row solo flow 已经完成，当前下一步进入 C3 喷射落点规则。
- 装配界面已经有格子方向，但还需要更像原版的整块武器占格和即时合法性反馈。
- AI 指挥官只保留高层决策接口，不进入逐帧控制。
- 保存游戏、地图服务器、经济、PVP、移动端、链上分账都暂缓。

## 0.1 当前路线图快照

这份计划现在按“先可见、再可玩、再可展示、再可扩展”的顺序执行。任何时候如果画面或指挥闭环退化，先回到当前阶段修正，不跨阶段堆功能。

| 阶段 | 状态 | 本阶段产物 | 下一动作 |
| --- | --- | --- | --- |
| Phase A: 地形/水面/道路可读性 | Done | 地面、水域、岸线、跑道/道路、建筑基底在截图里可读；提交 `89a686f Improve terrain and water readability` | 只做回归检查，不再主动展开 |
| Phase B: 第一张地图战场可读性 | Done | 敌我单位不堆点，建筑/树木/炮塔/道具比例可信，固定镜头能看懂战术关系 | 后续只做回归 |
| Phase C: 指挥战斗闭环 | Active | 默认全队、状态栏单选、独立命令、自动归队、喷射和最小战斗 UI 可稳定演示 | 继续 C3 squad jet landing rules |
| Phase D: 损伤、武器和战斗手感 | Next | 激光/导弹/炮弹层次、部位损伤、断臂/瘫痪/驾驶舱弹射能在截图或观战中看见 | C 阶段命令稳定后进入 |
| Phase E: 原版式装配垂直切片 | Next | 整块武器占格、装甲板/散热器、热量/重量/槽位合法性、配置进战斗 | D 阶段战斗反馈可读后进入 |
| Phase F: 战后和再战闭环 | Next | 简洁 Debrief、一键维修、回装配、再进同图 | E 阶段装配能影响战斗后进入 |
| Phase G: AI 副官能力窗口 | Later | compact observation、高层 directive、本地 adapter、AI 建议窗口 | 本地完整 Demo 成形后补 |
| Phase H/I: 内容包边界和演示构建 | Later | 私有参考包不进公开包，一键 Windows 演示构建，演示脚本 | Demo 可看可玩后收尾 |
| Platform Work | Deferred | 地图服务器、Web 排名、奖励认证、链上分账研究 | 第一版本地 Demo 稳定后再设计 |

## 0.2 当前完成度和缺口盘点

这不是百分比承诺，只是开发优先级判断，用来防止“还没看清战场就去做服务器”这种跑偏。

| 模块 | 当前程度 | 已经有的东西 | 当前缺口 | 最近动作 |
| --- | --- | --- | --- | --- |
| 本地启动/构建 | 基本可用 | Unity 6 Windows build、batch validator、smoke player、截图脚本 | Unity 偶发序列化 churn 需要提交前检查；公开构建安全边界还要再收 | 每个代码提交跑 build/smoke/capture 之一 |
| 第一张地图加载 | Phase B 可读性已收口 | `mc2_01` 地形、目标、触发、单位、结构、terrain objects、比例审计、占位证据、固定镜头构图已加载并验证 | 后续发现截图退化时再回归 | Phase C |
| 3D 地形/环境 | 过了黑块阶段 | 地面、水面、岸线、跑道/道路、建筑基底已可读 | 水边、道路边缘、地图边缘三角面仍是后续美术 polish | 只做回归，不扩大战线 |
| 机甲/载具/炮塔显示 | 比例审计已建立 | 参考 OBJ 加载、父级缩放补偿、分类缩放、sidecar scale summary、损伤节点部分可用 | 仍需要跟 B3 的硬物占位证据对照 | B3 |
| 物理碰撞占位 | 证据已建立 | 单位间、targetable structure、大型 terrain object、水域非法喷射落点已有 BattleCore 逻辑，sidecar 已输出 occupancy summary | 后续只在发现具体穿模/落点错误时补规则 | Phase C 回归 |
| 指挥操作 | 有基础逻辑 | 默认全队、状态栏单选、独立命令、自动归队、喷射已有基础 | 需要完整 smoke 覆盖和更少 UI 噪音 | Phase C |
| 战斗反馈 | 有雏形 | 武器命中、爆炸、损伤、残骸、驾驶舱逃生提示已有部分事件 | 武器类型层次、断臂/断腿/驾驶舱弹射可见度还不够强 | Phase D |
| 装配界面 | 能跑但不是最终乐趣点 | 热量/重量/槽位/装备预览已有基础；武器装上即启用 | 要更像原作整块占格，装甲板/散热器填格，合法性即时反馈 | Phase E |
| 战后再战 | 有基础，不做复杂保存 | Debrief、奖励/维修方向已有 | 一键维修、回装配、再进同图要收成可演示闭环 | Phase F |
| AI 副官 | 保留接口 | observation/directive 方向已明确，Minimax 接入已有探索 | 第一版只做高层建议窗口，不做逐帧控制 | Phase G |
| 平台/经济/地图服务器 | 暂缓 | 有产品设想和平台计划文档 | 不进入第一版本地 Demo 主线 | Demo 稳后重开 |

## 0.3 当前执行颗粒度

接下来每个小提交都按“先截图可读，再操作闭环，再战斗反馈，再装配乐趣”的顺序走。

当前下一批 6 个可执行提交：

1. `Finalize squad jet landing rules`：喷射按单机合法性结算，非法落点单位不动，其他单位照常跳。
2. `Freeze minimal battle UI`：战斗中只保留状态栏、喷射、任务地图、暂停/系统，不堆信息。
3. `Differentiate weapon visual effects`：激光、导弹、弹道、爆炸至少有可分辨的命中方向和效果层次。
4. `Strengthen mech section damage cues`：断臂、瘫痪、驾驶舱逃生在世界和状态栏都看得见。
5. `Make mech lab grid item fitting explicit`：武器/装甲/散热器按整块格子占位，合法性即时反馈。

每个提交结束时至少记录：

- 修改文件。
- 跑过的验证命令。
- 对应日志或截图路径。
- 下一步仍然存在的问题。

## 1. 第一版定义

第一版 Demo 的完整可见流程：

1. 启动 Windows Demo。
2. 进入机甲/小队准备界面。
3. 查看 1-6 台机甲，常规 4 台。
4. 在装配格子里看见武器、装甲板、散热器、热量、重量和合法性。
5. 进入一张参考小地图任务。
6. 固定俯视镜头默认跟随指挥官机甲。
7. 点击地点，默认全队移动。
8. 点击敌人或目标，全队移动攻击/集火。
9. 点击状态栏某台机甲后再点击地点或目标，该机甲进入独立命令。
10. 独立命令完成后自动归队，重新接受最新全队命令。
11. 点击喷射，队伍按各自当前位置向目标方向做固定距离快速位移，非法落点单位保持不动。
12. 战斗里能看到地形、建筑、树木/道具、我方、敌方、开火、爆炸、部位损伤、断臂/瘫痪/驾驶舱逃生提示。
13. 任务胜利或失败后进入战后简报。
14. 一键维修/补给，不做等待和复杂存档。
15. 回到装配，再次进入同一任务验证改装效果。

第一版不做：

- 实时 PVP。
- 地图服务器和第三方地图奖励。
- 完整经济、充值、提现或链上资产。
- 完整账号存档。
- AI 导演。
- 大模型逐帧战斗控制。
- 公开发布原版素材、任务文案、专有名称或商标。

## 2. 验收总线

每个提交必须满足对应验证，不靠主观感觉合并。

| 改动类型 | 最低验证 | 推荐额外验证 |
| --- | --- | --- |
| BattleCore 规则 | Unity validator | smoke command + targeted assertion |
| Unity 表现/材质/模型 | Windows build | reference capture + sidecar |
| UI 流程 | smoke command | 1280x720 capture |
| 装配规则 | validator 或 loadout smoke | 战斗中观察数值差异 |
| 文档 | `git diff --check` | `git status --short` |

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
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-combat-situation.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-smoke-current.log"
```

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol
```

## 2.1 阶段门验收

阶段门用于决定能不能进入下一阶段。没有过门时，不要用新功能掩盖旧问题。

| 阶段门 | 必须看见的结果 | 必须跑的验证 | 失败时回退 |
| --- | --- | --- | --- |
| A -> B | 地形、水域、道路、岸线、建筑基底可读，没有粉色/黑块主画面 | build + smoke + `airfield,hangar-contact,damage-demo` capture | 回到 terrain shader/material/texture grading |
| B -> C | 我方、敌方、建筑、炮塔/道具在 `hangar-contact`、`damage-demo` 里不再堆点；碰撞占位不把单位推入硬物 | validator + smoke + `hangar-contact,damage-demo` capture | 回到 BattleCore spacing/collision/visual scale |
| C -> D | 默认全队、状态栏单选、独立命令、自动归队、喷射都能不用说明书操作 | command smoke + 1280x720 capture | 回到 input/state/UI feedback |
| D -> E | 武器类型、命中方向、部位损伤、断臂/瘫痪/驾驶舱逃生有可见事件 | damage capture + validator for damage state | 回到 combat effect/damage model |
| E -> F | 装配界面能清楚表达整块武器占格、热量、载重、装甲/散热器，并影响战斗 | loadout validator + battle smoke | 回到 MechLab rules/UI |
| F -> G | 改装、出战、战后、维修、再战能一口气演示 | walkthrough smoke or manual capture set | 回到 debrief/repair/session state |
| G -> H/I | AI 只做高层建议，断网/超时不影响本地 Demo | observation/directive validator | 回到 AI contract/adapter |

本阶段优先级顺序固定：截图可读性高于内容数量，BattleCore 可验证高于 Unity 表现花活，第一张图闭环高于长期平台想象。

## 3. Phase A: 地形可读性回归段

状态：已完成。目标曾是把地形/水域/道路可读性实验修到可提交状态，不能把暗块截图提交成阶段成果。后续只在相关表现改动后跑回归，不主动扩展。

### Task A1: 固定当前视觉基线

**Files:**

- Read: `analysis-output/reference-visual-captures/airfield.png`
- Read: `analysis-output/reference-visual-captures/hangar-contact.png`
- Read: `analysis-output/reference-visual-captures/damage-demo.png`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Run capture for `airfield,hangar-contact,damage-demo`.
2. Inspect the three PNGs.
3. Record whether roads, runway, water, shore, buildings, squad, hostile contacts are readable.
4. Do not change gameplay code in this task.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets airfield,hangar-contact,damage-demo
git diff --check
```

**Acceptance:**

- The audit document states the exact visual failure before fixes.
- The worktree still separates docs from code changes.

**Commit:**

```text
Document terrain readability baseline
```

### Task A2: Make terrain colors readable even without private textures

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceTerrainTextureLibrary.cs`
- Modify: `unity-mc2-demo/Assets/Shaders/SourceTerrainVertexColor.shader`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`

**Steps:**

1. Add semantic color grading for water, shore, runway/road, dirt, grass and building-adjacent ground.
2. Blend private source textures toward semantic colors so dark TGA tiles cannot turn the terrain into black polygons.
3. Reduce terrain texture strength until unit silhouettes stay readable.
4. Keep terrain mesh geometry, click raycasts and BattleCore water rules unchanged.
5. Use a dedicated readable water material instead of a full-map dark transparent plane.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-terrain-readability.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-combat-situation.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-terrain-readability-smoke.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets airfield,hangar-contact,damage-demo
```

**Acceptance:**

- `airfield` can distinguish runway/road, grass/dirt, water/shore and building bases.
- `hangar-contact` no longer has huge unreadable black terrain patches.
- Smoke exits with code `0`.

**Commit:**

```text
Improve terrain and water readability
```

### Task A3: Restore Unity scene file if only file IDs churn

**Files:**

- Inspect: `unity-mc2-demo/Assets/Scenes/Mc2Demo.unity`

**Steps:**

1. After Unity build, run `git status --short`.
2. If only scene file IDs changed, inspect diff.
3. Restore the exact file IDs manually with `apply_patch`, or leave the scene unstaged.
4. Never stage generated reference art or build outputs.

**Validation:**

```powershell
git status --short
git diff -- unity-mc2-demo/Assets/Scenes/Mc2Demo.unity
```

**Acceptance:**

- The terrain readability commit contains source/shader/docs only.
- No accidental generated asset or scene churn enters Git.

## 4. Phase B: 第一张地图战场可读性收口

目标：让 `hangar-contact` 和 `damage-demo` 从“堆在一起”变成可读战术场面。

### Task B1: Enemy density and parking spread

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Read capture sidecars for active/visible hostiles at `airfield`, `hangar-contact`, `damage-demo`.
2. Record the current pressure baseline: active hostiles, visible hostiles, player squad count, target objective, and whether the screen reads as “one knot”.
3. Strengthen `ValidateEnemyAttackFormationSpacing` so a synthetic enemy group must produce non-overlapping `MoveTarget` points around one player target.
4. Increase deterministic enemy attack/parking slot spread in `BattleMission.EnemyAttackFormationOffset`, but keep each slot inside practical weapon range.
5. Preserve source mission triggers, patrol anchors, objective graph and active enemy count; do not make the fight easier by silently deleting enemies.
6. Keep BattleCore as the collision authority: unit-to-unit, structure and terrain-object occupancy must remain deterministic and validator-covered.
7. Capture `hangar-contact` and `damage-demo` before/after, then update the visual audit with the result.

**Implementation notes:**

- Use ring/slot changes before mission rewrites. The original mission cadence is the reference; only the readable parking geometry should move first.
- Minimum spacing should be expressed against mission coordinates or desired attack slots, not Unity renderer bounds.
- If a specific enemy cannot find a legal attack slot because of water, structure, or hard terrain object occupancy, prefer a nearby fallback slot over clipping into the obstacle.
- Do not add Unity physics as authoritative gameplay logic. Unity colliders may help presentation, but BattleCore must still decide legal positions.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-enemy-spacing.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-combat-situation.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-enemy-spacing-smoke.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets hangar-contact,damage-demo
```

**Acceptance:**

- The hangar fight remains busy but units are not visually on one point.
- Objective logic and smoke path still pass.
- Enemy `MoveTarget` slots have a validator-backed minimum spread and remain inside useful attack range.
- Screenshots still show original-like pressure: this is readability tuning, not encounter nerfing.

**Commit:**

```text
Spread first mission enemy parking
```

### Task B2: Mech, vehicle, turret and prop scale audit

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceObjMeshLibrary.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/ReferencePropLibrary.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Read the latest `spawn.json`, `airfield.json`, `hangar-contact.json` and `damage-demo.json` sidecars.
2. List the `mc2_01` unit types and terrain-object classes actually appearing in the first-slice captures.
3. Group visible assets into `mech`, `vehicle`, `infantry`, `turret`, `aircraft`, `building`, `barricade`, `tree`, `other`.
4. Add a category summary to the runtime log or capture sidecar, for example `ReferenceScale=mech 4 vehicle 9 building 12 tree 80 fallback 6`.
5. Confirm imported reference meshes compensate their placeholder parent scale; no unit should be scaled once by the primitive parent and again by the reference child.
6. Define conservative per-category multipliers. Start with mechs as the actor baseline, vehicles lower, infantry much lower, buildings/aircraft/large props large but not screen-blocking.
7. Keep actual BattleCore radius and hit rules unchanged in this task. This task is visual scale only.
8. Capture `spawn`, `airfield`, `hangar-contact`, `damage-demo`.
9. Update the visual audit with before/after findings and exact remaining readability failure.

**Implementation notes:**

- If a model has bad source units, fix through one mapping table instead of hard-coding scattered scale exceptions.
- Prefer category-based scale first. Only add asset-specific overrides for obvious outliers such as aircraft, hangars, towers, or tiny infantry.
- Mechs must read as the main actors; buildings should sell the map scale but not bury the squad.
- Do not use Unity physics colliders as hidden gameplay truth. If a visual scale implies a bigger obstacle, the next task must decide whether BattleCore occupancy should also change.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-scale-audit.log"
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-scale-audit.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo
```

**Acceptance:**

- Mechs are readable as the main actors.
- Vehicles and turrets are smaller but recognizable.
- Buildings and large props feel large without hiding the whole battle.
- The audit states whether any visual scale now disagrees with BattleCore occupancy.
- No public-safe content boundary changes are made in this task.

**Commit:**

```text
Tune first slice visual scale
```

### Task B3: BattleCore occupancy evidence pass

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Inventory the occupancy sources currently used by BattleCore: live units, targetable structures, hard terrain objects, water/illegal landing cells and fallback destination search.
2. Add validator assertions that a destination inside each hard obstacle class is rejected or pushed to a legal nearby point.
3. Add capture-sidecar evidence for occupancy counts, for example `Occupancy=units 23 structures 4 hardProps 120 waterCells 340`.
4. Add an optional development overlay or log-only debug mode for collision footprints. Keep it off in normal play and normal screenshots.
5. Compare B2 visual-scale findings against actual occupancy radius/classes. If a huge visible object has no occupancy, either add it to the hard-prop class or document why it remains soft.
6. Re-run `mc2_01-jet-landing-block.txt` and `mc2_01-squad-jet-partial.txt` if changed code touches landing legality.
7. Capture `hangar-contact` and `damage-demo` and record whether units still appear to stand inside hard objects.

**Implementation notes:**

- BattleCore remains the authority. Unity colliders may exist for presentation, but they must not be the only thing preventing illegal movement.
- Trees and forest masses should usually be soft occlusion/fade unless a specific trunk/large object is clearly a hard obstacle.
- Water and hard structures are strict. If a unit starts near water and jet target falls into water, that single unit stays still while other legal jumps proceed.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-occupancy-evidence.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-squad-jet-partial.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-occupancy-jet-smoke.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets hangar-contact,damage-demo
```

**Acceptance:**

- The sidecar/log proves physical occupancy exists for the visible hard objects that matter in the first slice.
- Units do not visibly park inside target structures, hard terrain objects or water.
- Soft trees/forest props fade or visually yield instead of blocking the whole map.

**Commit:**

```text
Expose battle occupancy evidence
```

### Task B4: Commander camera composition pass

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`

**Steps:**

1. Keep fixed tactical yaw/pitch and limited zoom.
2. Keep default follow on commander unit, which is the first sorted player mech.
3. Add small composition offset only when the active objective and commander would be hidden by UI or large props.
4. Let active objective, commander squad and primary enemy direction share the frame in `hangar-contact`.
5. Do not add free rotation or manual camera dragging.
6. Capture `spawn`, `hangar-contact` and `damage-demo` at 1280x720.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,hangar-contact,damage-demo
```

**Acceptance:**

- Commander squad, active objective and enemy direction can be read together.
- UI does not cover the fight center.

**Commit:**

```text
Tune commander camera composition
```

## 5. Phase C: 指挥战斗闭环

目标：不用说明书也能用少量点击打完第一张地图。

### Task C1: Command state validator

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`
- Modify: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-combat-situation.txt`

**Steps:**

1. Add script assertions for full-squad move accepted count.
2. Add script assertions for solo command count.
3. Add script assertions for solo command completion and auto-rejoin.
4. Add script assertions for latest full-squad command being accepted after rejoin.

**Validation:**

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-combat-situation.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-command-state-smoke.log"
```

**Acceptance:**

- Full-squad command, solo command and auto-rejoin are covered by smoke.

**Commit:**

```text
Assert commander command states
```

### Task C2: Status-bar selection and click contract

**Status:** Completed 2026-06-07. The solo-order smoke now simulates status-row selection plus terrain click, then asserts visible selection returns to squad, the selected row shows solo state, and the row returns to ready after arrival. The solo-attack smoke uses the same status-row flow for target-structure clicks.

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`
- Modify: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-solo-order-state.txt`
- Modify: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-solo-attack-isolation.txt`

**Steps:**

1. Treat the battle state as full-squad selected by default.
2. When the player clicks a mech row in the left status bar, mark only that mech as the next-command target.
3. The next terrain click becomes a solo move; the next hostile/objective click becomes a solo attack/focus command.
4. After issuing the solo command, return the visible selection to full-squad mode while that mech remains in independent-command state internally.
5. Show the independent-command state in that mech status row, not through a new map selection box.
6. When the solo command completes, clear the independent state and let that mech obey the latest full-squad command.
7. Add script assertions for row select, solo command isolation, full-squad visible selection restore and auto-rejoin.

**Implementation notes:**

- Do not add drag selection, box selection or per-mech map tapping requirements.
- Avoid extra battle buttons. The player should mainly use status rows, map clicks, jet and pause/system.
- The UI should show enough state to prevent confusion, but battle should not become a debug dashboard.

**Validation:**

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-solo-order-state.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-status-row-solo-smoke.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-solo-attack-isolation.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-solo-attack-isolation.log"
```

**Acceptance:**

- The visible default returns to full-squad after a solo command is issued.
- The solo mech keeps its independent command until completion.
- No box-select or map-select interaction is required.

**Commit:**

```text
Tighten status row solo command flow
```

### Task C3: Jet movement final rules

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Keep jet movement as fixed-distance impulse from each mech toward target direction.
2. Reject water, occupied hard obstacle, occupied structure and out-of-map landing points.
3. If one unit has illegal landing, keep only that unit still.
4. Add UI feedback for partial jet success.
5. Add validator case for one unit at water edge while other units jump normally.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-jet-rules.log"
```

**Acceptance:**

- Jet command behaves independently per mech.
- Illegal landing does not cancel valid jumps for the rest of the squad.

**Commit:**

```text
Finalize squad jet landing rules
```

### Task C4: Battle UI freeze

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`

**Steps:**

1. Keep left mech status list as primary information surface.
2. Show health, damaged sections and independent command state.
3. Keep only jet, mission map and pause/system as explicit battle controls.
4. Hide verbose debug text during normal battle.
5. Test 1280x720 and screenshot presets.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,hangar-contact,damage-demo
```

**Acceptance:**

- Battle scene has little UI noise.
- Player can still read mech damage and command state.

**Commit:**

```text
Freeze minimal battle UI
```

## 6. Phase D: 损伤、武器和战斗手感

目标：让战斗有“机甲指挥”的味道，不只是模型互相扣血。

### Task D1: Weapon effect tiers

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoEffectsView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/WeaponCatalog.cs`

**Steps:**

1. Group weapons into laser, ballistic, missile and explosive tiers.
2. Give each group distinct line/trail/impact style.
3. Make hit direction readable without adding too much screen clutter.
4. Keep all equipped weapons enabled; do not reintroduce weapon on/off UI.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets damage-demo
```

**Acceptance:**

- A player can tell who is shooting and roughly what type of weapon fired.

**Commit:**

```text
Differentiate weapon visual effects
```

### Task D2: Section damage visual pass

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatDamageModel.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoEffectsView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceObjMeshLibrary.cs`

**Steps:**

1. Keep cockpit, torso, arms and legs as meaningful damage sections.
2. Use real reference nodes for detached parts when available.
3. Fall back to clear project-owned placeholder fragments when nodes are missing.
4. Show cockpit ejection as a short readable event.
5. Sync state bar section damage with world event.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets damage-demo
```

**Acceptance:**

- Damage demo visibly shows arm loss, leg disable or cockpit ejection.
- Status bar confirms the same damaged section.

**Commit:**

```text
Strengthen mech section damage cues
```

### Task D3: Armor hardness simplification

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatDamageModel.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/MechLoadoutRules.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`

**Steps:**

1. Treat armor plates as a single overall hardness bonus.
2. Apply hardness before section damage allocation.
3. Keep individual section health and destruction events.
4. Add one validator or smoke assertion proving armor changes survival.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-armor-hardness.log"
```

**Acceptance:**

- Armor calculation is simple.
- Section destruction still exists and remains visible.

**Commit:**

```text
Simplify armor hardness damage math
```

## 7. Phase E: 原版式装配垂直切片

目标：把装配界面做成第二个核心乐趣点。

### Task E1: Remove any weapon enable/disable leftovers

**Files:**

- Search: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Search: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Search: `unity-mc2-demo/Assets/Scripts/Presentation/MechLabView.cs`

**Steps:**

1. Use `rg "enabledWeapons|Enable|Disable|toggle|Toggle"` to find leftover weapon toggle logic.
2. Keep equipped weapons always active.
3. If old helper exists only for tests, rename it toward loadout presence instead of active toggle.
4. Update docs/comments only where needed.

**Validation:**

```powershell
rg "enabledWeapons|weapon.*toggle|toggle.*weapon" unity-mc2-demo/Assets/Scripts
git diff --check
```

**Acceptance:**

- No player-facing weapon enable/disable concept remains.

**Commit:**

```text
Remove weapon toggle leftovers
```

### Task E2: Original-like grid layout

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/MechChassisCatalog.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/WeaponCatalog.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/MechLabView.cs`

**Steps:**

1. Define chassis slot grid width, height and blocked cells.
2. Define each weapon shape as a set of grid cells, including vertical multi-cell weapons.
3. Render each item as one contiguous visual block, not a list row.
4. Show armor plates and heat sinks as single-cell fillers.
5. Show conflict, overweight and overheat states immediately.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-loadout-grid.log"
```

**Acceptance:**

- The mech lab visually reads as grid fitting.
- Players can see why an item fits or does not fit.

**Commit:**

```text
Make mech lab grid item fitting explicit
```

### Task E3: Loadout affects battle

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/MechLoadoutRules.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatProfile.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`

**Steps:**

1. Feed selected loadout into unit combat profiles before mission start.
2. Make weapon damage, range and cooldown use loadout weapon definitions.
3. Make heat/armor modifiers affect combat state in a simple deterministic way.
4. Add smoke script that changes a loadout and reports different combat summary.

**Validation:**

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-combat-situation.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-loadout-battle-smoke.log"
```

**Acceptance:**

- Changing a weapon or armor piece has observable battle impact.

**Commit:**

```text
Apply mech lab loadouts in battle
```

## 8. Phase F: 战后和再战闭环

目标：完成“打一局 -> 看结果 -> 修复 -> 再出战”。

### Task F1: Hide save-system surface from first demo

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/README.md`
- Modify: `docs-mc2-detailed-development-plan.md`

**Steps:**

1. Audit visible UI entries for Save Slot, Continue, New Company, export/import and saved-account developer panels.
2. Keep any useful internal account snapshot code only as hidden developer tooling.
3. Remove or hide save-related entries from the first-version player-facing battle, debrief and startup flow.
4. Update README wording so saved-account commands are clearly marked as developer diagnostics, not first-demo gameplay.
5. Keep first-demo flow as current run state: refit, launch, debrief, repair, relaunch.

**Validation:**

```powershell
rg -n "Save Slot|saved-account|Continue|New Company|save/load" unity-mc2-demo/README.md unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs
git diff --check
```

**Acceptance:**

- First-demo UI does not ask the player to manage save files.
- Hidden developer save diagnostics, if kept, do not appear in the normal demo walkthrough.

**Commit:**

```text
Hide save system from first demo flow
```

### Task F2: Debrief compact loop

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DebriefView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/PostBattleReport.cs`

**Steps:**

1. Show mission outcome.
2. Show mech damage summary.
3. Show reward and salvage summary using one token currency.
4. Show one repair/refit action.
5. Hide complex save/load concepts from first version flow.

**Validation:**

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-combat-situation.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-debrief-loop-smoke.log"
```

**Acceptance:**

- Task completion leads to debrief.
- Debrief leads back to refit or restart without a heavy save system.

**Commit:**

```text
Tighten debrief and repair loop
```

### Task F3: Instant repair and ordinary weapon replacement

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/PostBattleReport.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/MechLoadoutRules.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`

**Steps:**

1. Treat mech destruction as money repair, not permanent loss.
2. Treat ordinary weapon loss as repair/rebuy cost.
3. Keep NPC pilot death risk rules for later; do not block first demo on it.
4. Apply repair immediately.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- No waiting timer.
- No complex inventory persistence.

**Commit:**

```text
Add instant repair demo rules
```

## 9. Phase G: AI 副官能力窗口

目标：AI 做大决策，不抢本地确定性战斗。

### Task G1: Freeze commander observation

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs`
- Create or Modify: `docs-ai-commander-directive-contract.md`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`

**Steps:**

1. Keep observation compact: phase, squad health, solo command state, active hostiles, objectives, nearby threats.
2. Do not include every projectile or every frame detail.
3. Add command-line export option or smoke assertion for one observation JSON.
4. Document field meanings.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- Observation is small enough for a slow model call.
- It is stable enough to hand to Minimax/OpenAI/local models later.

**Commit:**

```text
Freeze AI commander observation contract
```

### Task G2: Freeze high-level directive

**Files:**

- Create or Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/AiCommanderDirective.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify: `docs-ai-commander-directive-contract.md`

**Steps:**

1. Define directive kinds: attack, defend, regroup, hold, retreat, focusTarget, protectUnit.
2. Convert each directive into existing BattleCore commands.
3. Add timeout/fallback behavior.
4. Do not let AI mutate combat state directly.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-directive.log"
```

**Acceptance:**

- A high-level AI decision can become a normal game command.
- Missing model response does not stop the battle.

**Commit:**

```text
Add AI commander directive adapter
```

### Task G3: AI ability window UI

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`

**Steps:**

1. Add a small optional AI advice panel outside the main fight center.
2. Show current AI suggestion in one short sentence.
3. Keep player control primary.
4. Do not add natural-language tactical chat to first version.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets hangar-contact
```

**Acceptance:**

- AI feels like a副官窗口, not a noisy debug console.

**Commit:**

```text
Show compact AI commander advice
```

## 10. Phase H: 内容包和公开边界

目标：本地可以用参考包验证，公开可以安全替换。

### Task H1: Content pack provenance manifest

**Files:**

- Modify: `docs-content-replacement-plan.md`
- Create: `content-packs/project-owned-visual-slice.example.json`
- Modify: `unity-mc2-demo/README.md`

**Steps:**

1. Document private reference pack, project-owned pack and community map pack.
2. Add provenance fields for source, license, generated status and public-safe status.
3. Ensure Unity loaders use asset IDs instead of public product names.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- The same game IDs can point to private reference assets or future original replacement assets.

**Commit:**

```text
Document replaceable visual content packs
```

### Task H2: Public build safety check

**Files:**

- Modify: `scripts/unity/*`
- Modify: `.gitignore`
- Modify: `README.md`
- Modify: `unity-mc2-demo/README.md`

**Steps:**

1. Add a script check for private reference asset paths in public build inputs.
2. Keep ignored generated output out of Git.
3. Keep README focused on AI-assisted tactical RTS exploration, not original-game replication.
4. Document local private-reference build separately.

**Validation:**

```powershell
git status --short
git diff --check
```

**Acceptance:**

- GitHub version is safe to show.
- Local private demo remains useful for visual validation.

**Commit:**

```text
Add public build content safety notes
```

## 11. Phase I: 演示构建

目标：把本地 Demo 收成能给人看的 Windows 包。

### Task I1: One-command demo build

**Files:**

- Modify: `scripts/unity/*`
- Modify: `unity-mc2-demo/README.md`

**Steps:**

1. Ensure one build command creates Windows player.
2. Ensure smoke test command is copied into README.
3. Ensure reference capture command is copied into README.
4. Mark private reference pack as optional local dependency.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-demo-handoff.log"
```

**Acceptance:**

- Fresh local build can run without manual Unity editor clicks.

**Commit:**

```text
Prepare repeatable Windows demo build
```

### Task I2: Demo walk-through script

**Files:**

- Create: `docs-demo-walkthrough-2026-06-07.md`
- Modify: `README.md`

**Steps:**

1. Write a 3-minute demo script: mech lab, launch, move, attack, jet, solo command, damage, debrief.
2. Link screenshots generated by capture presets as local evidence paths.
3. Avoid original product names and story descriptions.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- Another person can understand what to show in the demo.

**Commit:**

```text
Add playable demo walkthrough
```

## 12. Later Platform Work

这些路线保留，但不进入第一版 Demo 主线。

### Task P1: Main server prototype

**Scope:**

- Account ID.
- Inventory snapshot.
- Token ledger.
- Signed squad loadout.
- Reward claim endpoint.
- Basic leaderboard.

**Start condition:**

- Windows demo has stable battle and mech lab loop.

### Task P2: Map package and editor loop

**Scope:**

- Map package schema.
- Local map editor export.
- Draft/uncertified/certified map states.
- No portable rewards until main server validates.

**Start condition:**

- BattleCore contract is stable enough to replay or validate map results.

### Task P3: Partner/community map servers

**Scope:**

- Public map server protocol.
- Session tickets.
- Replay or summary digest upload.
- Main-server reward certification.

**Start condition:**

- Main server prototype and reward caps exist.

### Task P4: Optional chain layer

**Scope:**

- Creator revenue proof.
- Cosmetic ownership proof.
- Transparent event pools.

**Start condition:**

- Economy, refund, moderation and fraud rules are already proven off-chain.

## 13. Commit Order From Here

Recommended next commits from the current active point. Phase A terrain readability is already complete in `89a686f`, and B1 enemy parking spread is complete.

1. `Finalize squad jet landing rules`
2. `Freeze minimal battle UI`
3. `Differentiate weapon visual effects`
4. `Strengthen mech section damage cues`
5. `Make mech lab grid item fitting explicit`
6. `Apply mech lab loadouts in battle`
7. `Hide save system from first demo flow`
8. `Tighten debrief and repair loop`
9. `Freeze AI commander observation contract`
10. `Add AI commander directive adapter`
11. `Document replaceable visual content packs`
12. `Prepare repeatable Windows demo build`

Every commit should include:

- What changed.
- Which validation ran.
- Which screenshot or log proves it.
- What remains next.

## 14. Stop Conditions

Pause and reassess instead of adding features when any of these happens:

- `hangar-contact` or `damage-demo` screenshots become visually worse.
- Smoke test fails after a presentation-only change.
- A Unity scene file changes only because of editor serialization.
- A task tries to introduce server, economy, save-game, mobile or blockchain code before the first local Demo is stable.
- A public-facing doc starts describing private reference assets as product content.

The shape is simple: first make the battle understandable, then make the mech lab satisfying, then package the local demo. Everything else waits its turn.

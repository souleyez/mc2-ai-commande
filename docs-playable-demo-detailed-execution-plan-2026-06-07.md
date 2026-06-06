# Playable Demo Detailed Execution Plan

> **For Codex:** Execute this plan task-by-task. Each task should end with validation evidence and a small commit.

**Status Update 2026-06-07:** This file is now historical phase context. Continue day-to-day work from `docs-playable-demo-current-detailed-plan-2026-06-07.md`, which reflects the current implementation state and the next visible-flow/occupancy/HUD work.

**Goal:** 把当前 Unity 6 Windows 原型收成一版能演示、能截图、能继续融资推进的机甲战术指挥 Demo。

**Architecture:** `BattleCore` 负责确定性规则、任务、命令、伤害、碰撞占位、结算和 AI 可观察状态。Unity Presentation 负责输入、镜头、UI、模型、材质、特效和截图验证。本地开发可以用私有参考内容包验证战场比例和手感，公开构建必须切到可替换内容包，不能携带原版资产、原版文案、商标或专有名称。

**Tech Stack:** Unity 6, C#, Windows Standalone, PowerShell validation scripts, `mc2-unity-demo-contract-v1`, deterministic BattleCore, private local reference content pack, Git/GitHub.

---

## 1. Plan Role

这份文档是当前阶段的执行工作台。它不替代长期产品设想，也不展开服务器、经济、移动端或链上设计。每天继续开发时，优先看这里：

1. 当前阶段做到哪。
2. 下一次应该改哪些文件。
3. 每个小提交怎么验收。
4. 哪些事情暂缓，避免跑偏。

关联文档：

- `docs-playable-demo-master-plan-2026-06-07.md`: 老的主计划，保留完整阶段背景和历史任务。
- `docs-mc2-detailed-development-plan.md`: 产品和架构总览。
- `docs-reference-visual-audit-2026-06-07.md`: 截图和视觉审计记录。
- `docs-content-replacement-plan.md`: 私有参考内容和公开替换内容边界。
- `docs-platform-ecosystem-plan.md`: 地图服务器、排行、奖励认证和创作者生态的长期方向。

## 2. Current Snapshot

日期：2026-06-07。

当前阶段：**Phase C: 指挥战斗闭环**。

已经完成：

- Unity 6 Windows build、validator、player smoke 和截图 preset 都能跑。
- `mc2_01` 任务合同能加载地形、目标、触发、单位、结构、terrain objects、导航点和源相机信息。
- Phase A 地形/水面/道路可读性已完成，提交 `89a686f Improve terrain and water readability`。
- Phase B1 敌方停靠点展开已完成，提交 `ba18a31 Spread first mission enemy parking`。
- Phase B2 单位/道具比例审计已完成，提交 `8de970a Tune first slice visual scale`。
- Phase B3 BattleCore 占位证据已完成，提交 `a61574b Expose battle occupancy evidence`。
- Phase B4 指挥官固定镜头构图已完成，提交 `14183a4 Tune commander camera composition`。
- BattleCore 已有单位间、目标建筑、大型 terrain object、水域非法落点的轻量确定性占位逻辑。
- 小队默认全选、单机独立命令、喷射、自动归队、状态栏和战后流程已有基础。
- 装配界面已有热量、重量、槽位和装备预览方向，武器装上即启用。
- AI 副官方向已明确为高层决策，不做逐帧控制。
- Phase C / Task C1 command-state smoke 已完成：主战斗脚本和独立命令脚本现在都覆盖全队命令、单机独立命令、独立时全队接受数减少、完成后自动归队、归队后再次接受全队命令。

当前最重要缺口：

- 状态栏点选单机、下达独立命令、视觉选择回到全队的 UI contract 还需要收紧。
- 战斗中 UI 还需要进一步变干净，状态栏承担主要反馈。
- 武器效果、部位损伤、断臂/瘫痪/驾驶舱弹射需要更强可见事件。
- 装配界面还需要更像原版整块格子放置，装甲板和散热器应作为清楚的填格组件。
- 战后再战闭环需要压成一键维修、回装配、再出战，不做复杂保存。
- 私有参考素材还可以用于本地验证，但公开仓库和公开构建必须保持可替换内容边界。

## 3. Locked Product Decisions

这些决策当前不再反复摇摆，除非后续试玩证明方向错了。

| Decision | Current Choice |
| --- | --- |
| Engine | Unity 6 |
| First platform | Windows playable demo |
| Camera | 固定俯视战术视角，可有限缩放，不自由旋转 |
| Battle authority | `BattleCore` 是权威，Unity 不决定合法落点和伤害结果 |
| Squad size | 1-6 台，常规 4 台 |
| Default selection | 默认全队 |
| Solo command | 点状态栏选单机后下令，该机甲独立执行，完成后自动归队 |
| Evasion | 第一版等同独立命令，不加单独按钮 |
| Jet | 每台机甲独立判定合法落点，非法单位不动，合法单位正常喷射 |
| Battle UI | 只保留状态栏、喷射、任务地图、暂停/系统和必要目标提示 |
| Weapon activation | 武器装上即启用，不做启用/关闭 |
| Armor math | 装甲板提升整体硬度，部位损伤仍保留 |
| Save system | 第一版不做玩家可见复杂保存 |
| AI | 只做高层 observation/directive 和副官建议窗口 |
| PvP | 第一版不做实时 PvP |
| Platform work | 地图服务器、排行、链上、复杂经济都暂缓 |
| Public content | 不能公开原版资产、文案、商标和专有名称 |

## 4. First Playable Demo Definition

第一版 Demo 必须能走完这条可见流程：

1. 启动 Windows Demo。
2. 进入准备或装配界面。
3. 查看 1-6 台机甲，默认小队通常 4 台。
4. 看见机甲格子、武器、装甲板、散热器、热量、重量和合法性。
5. 进入 `mc2_01` 参考小地图任务。
6. 镜头默认跟随排序第一的指挥官机甲。
7. 点击地点，全队移动到合法位置。
8. 点击敌人或目标，全队移动攻击或集火。
9. 点击状态栏某台机甲，再点地点或目标，该机甲执行独立命令。
10. 独立命令完成后自动归队，重新接受最新全队命令。
11. 点击喷射，小队按各自合法性执行短距离快速位移。
12. 战斗中能看见地形、建筑、树木/道具、我方、敌方、开火、爆炸、部位损伤和驾驶舱逃生。
13. 胜利或失败后进入战后简报。
14. 一键维修/补给，不做等待。
15. 回装配，再次进入同图验证改装效果。

第一版明确不做：

- 实时 PvP。
- 地图服务器和第三方地图奖励。
- 完整账号、充值、提现、链上资产。
- AI 导演。
- 大模型逐帧控制。
- 公开发布私有参考素材或原版表达。

## 5. Validation Bus

所有提交都要有证据。最少验证按改动类型选择：

| Change Type | Minimum Validation | Extra Validation |
| --- | --- | --- |
| BattleCore rule | Unity validator | player smoke command |
| Command flow | targeted command-file smoke | screenshot sidecar |
| Unity visual | Windows build | capture preset |
| UI layout | capture at 1280x720 | smoke |
| MechLab rule | validator or loadout smoke | battle comparison |
| Docs only | `git diff --check` | `git status --short` |

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

阶段门：

| Gate | Must Prove |
| --- | --- |
| C -> D | 全队命令、单机独立命令、自动归队、喷射、最小 UI 都能稳定演示 |
| D -> E | 武器类型、命中方向、部位损伤、断臂/瘫痪/驾驶舱逃生有可见事件 |
| E -> F | 装配格子和 loadout 能影响 BattleCore 战斗表现 |
| F -> G | 改装、出战、战后、维修、再战能一口气演示 |
| G -> H/I | AI 只做高层建议，模型超时不影响本地 Demo |

## 6. Phase C: Commandable Battle Loop

目标：玩家不用框选、不看说明，也能用状态栏和少量按钮指挥一场战斗。

### Task C1: Assert Commander Command States

**Status:** Completed 2026-06-07. `mc2_01-combat-situation.txt` and `mc2_01-solo-order-state.txt` now assert full-squad accepted counts, solo accepted counts, detached-unit exclusion from squad commands, auto-rejoin, and full-squad acceptance after rejoin.

**Files:**

- Modify: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-combat-situation.txt`
- Optional Modify: `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`
- Optional Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`

**Steps:**

1. 在 command file 里先断言开局 `solo=0`。
2. 下达一次全队移动，断言 `accepted=3` 或当前实际可指挥单位数。
3. 对 `unit-1` 下达独立移动，断言 `accepted=1`。
4. 再下达全队移动，断言独立单位不吃这次全队命令，全队接受数减少。
5. `advance` 到独立命令完成，断言 `solo=0`。
6. 再下达全队命令，断言归队单位重新接受全队命令。
7. 保留原有 contact/fire 断言，证明这条 smoke 仍覆盖战斗进入状态。

**Validation:**

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-combat-situation.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-command-state-smoke.log"
```

**Acceptance:**

- 日志出现 `MC2 demo smoke test exiting with code 0`。
- command file 证明全队命令、独立命令、自动归队和归队后再接全队命令。

**Validation Evidence:**

- `analysis-output/unity-player-command-state-smoke.log`: main combat situation smoke exits with code `0`.
- `analysis-output/unity-player-solo-order-state.log`: solo-order smoke exits with code `0`.

**Commit:** `Assert commander command states`

### Task C2: Tighten Status Row Solo Command Flow

**Status:** Completed 2026-06-07. `mc2_01-solo-order-state.txt` now drives the player-like status-row flow: select `unit-1` from the left status bar, click terrain, restore visible selection to the full squad, keep the detached row marked as solo, then clear it after arrival. `mc2_01-solo-attack-isolation.txt` covers the same status-row flow for clicking a target structure.

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`
- Modify: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-solo-order-state.txt`
- Modify: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-solo-attack-isolation.txt`

**Steps:**

1. 状态栏点选单机时，不在地图里引入框选 UI。
2. 下达单机命令后，视觉选择回到全队。
3. 被下令机甲状态栏显示独立命令标记。
4. 单机命令完成后标记消失。
5. smoke 脚本覆盖状态变化。

**Validation:**

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-solo-order-state.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-solo-order-state.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-solo-attack-isolation.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-solo-attack-isolation.log"
```

**Acceptance:**

- 视觉上玩家仍像在指挥全队。
- 状态栏能看出哪台机甲正在执行独立命令。
- 完成后自动归队。

**Validation Evidence:**

- `analysis-output/unity-build-status-row-solo.log`: Windows build exits with `Build Finished, Result: Success` and `MC2 Unity demo Windows build OK`.
- `analysis-output/unity-player-solo-order-state.log`: status-row solo smoke exits with code `0`.
- `analysis-output/unity-player-solo-attack-isolation.log`: status-row target-click smoke exits with code `0`.
- `analysis-output/unity-player-command-state-smoke.log`: main command-state smoke exits with code `0`.

**Commit:** `Tighten status row solo command flow`

### Task C3: Finalize Squad Jet Landing Rules

**Status:** Completed 2026-06-07. Existing BattleCore jump logic already computes each mech's fixed-distance landing from its own position and rejects illegal landings through the BattleCore landing predicate. This pass added validator coverage for hard terrain-object jump rejection and strengthened the squad-jet smoke so it proves `unit-1` jumps while `unit-2` and `unit-3` stay ready.

**Files:**

- Audit: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Audit: `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderCommandPort.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-squad-jet-partial.txt`

**Steps:**

1. 确认喷射按每台机甲当前位置向目标方向计算固定距离。
2. 对水域、地图边界、结构和硬 terrain object 落点做 BattleCore 判定。
3. 非法落点单位保持不动。
4. 其他有合法落点的单位照常喷射。
5. smoke 断言部分接受和落地结束。

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-jet-rules.log"
```

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-squad-jet-partial.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-squad-jet-partial.log"
```

**Acceptance:**

- 喷射命令不会因为单台机甲非法落点而取消整队。
- BattleCore 仍是唯一权威判定。

**Validation Evidence:**

- `analysis-output/unity-validate-jet-rules.log`: mission validator exits with `MC2 demo contract validation OK` and includes hard terrain-object jump rejection coverage.
- `analysis-output/unity-player-squad-jet-partial.log`: squad-jet smoke exits with code `0`; assertions show `unit-1:jetting` while `unit-2:ready` and `unit-3:ready`, then all three return to `ready`.

**Commit:** `Finalize squad jet landing rules`

### Task C4: Freeze Minimal Battle UI

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Optional Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DebriefView.cs`

**Steps:**

1. 战斗中保留左侧机甲状态栏。
2. 状态栏显示生命、部位损伤、独立命令、可行动状态。
3. 主按钮只保留喷射、任务地图、暂停/系统。
4. 收起普通战斗里的 verbose debug 信息。
5. 任务目标保持 compact，不遮挡主战区。
6. 检查 1280x720 截图。

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,hangar-contact,damage-demo
```

**Acceptance:**

- 战斗画面不被 UI 抢戏。
- 玩家仍能读出每台机甲状态和独立命令。

**Commit:** `Freeze minimal battle UI`

### Task C5: Command Loop Walkthrough Capture

**Files:**

- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Optional Modify: `scripts/unity/capture_reference_visuals.ps1`

**Steps:**

1. 捕获 `spawn`、`hangar-contact`、`damage-demo`。
2. 记录 sidecar 的 active hostiles、visible hostiles、camera composition、occupancy summary。
3. 在审计文档中记录 C 阶段 UI 和命令可读性。
4. 如果截图退化，先修回 C1-C4，不进入 D。

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- 有一组能代表 C 阶段完成状态的本地截图和审计记录。

**Commit:** `Document command loop visual baseline`

## 7. Phase D: Combat Feedback And Damage Feel

目标：让战斗不只是扣血，而是看起来像机甲战术战斗。

### Task D1: Differentiate Weapon Visual Effects

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoEffectsView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/WeaponCatalog.cs`

**Steps:**

1. 按激光、弹道、导弹、爆炸分组。
2. 每组给不同线束、轨迹、命中和爆炸表现。
3. 命中方向可读，但不制造太多屏幕噪音。
4. 不增加武器启用/关闭 UI。

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets damage-demo
```

**Acceptance:**

- 玩家能看出谁在打谁，以及大概是什么类型武器。

**Commit:** `Differentiate weapon visual effects`

### Task D2: Strengthen Mech Section Damage Cues

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatDamageModel.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoEffectsView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceObjMeshLibrary.cs`

**Steps:**

1. 保留驾驶舱、躯干、双臂、双腿等部位。
2. 有 reference node 时用真实节点克隆断臂或部件脱落。
3. 没有节点时用清楚的自有占位碎片。
4. 驾驶舱逃生做短促但可见的事件。
5. 状态栏部位损伤和世界事件一致。

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets damage-demo
```

**Acceptance:**

- `damage-demo` 不用读日志也能看见严重部位损伤或驾驶舱逃生。

**Commit:** `Strengthen mech section damage cues`

### Task D3: Simplify Armor Hardness Damage Math

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatDamageModel.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/MechLoadoutRules.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`

**Steps:**

1. 装甲板提供整体硬度。
2. 伤害先经过硬度折减，再分配到部位。
3. 部位健康和摧毁事件继续存在。
4. 加一个 validator 或 smoke 断言证明装甲改变生存结果。

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-armor-hardness.log"
```

**Acceptance:**

- 装甲计算简单。
- 部位损伤仍是战斗卖点。

**Commit:** `Simplify armor hardness damage math`

## 8. Phase E: Original-Like MechLab Slice

目标：把装配界面做成第二个核心卖点。

### Task E1: Remove Weapon Toggle Leftovers

**Files:**

- Search: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Search: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Search: `unity-mc2-demo/Assets/Scripts/Presentation/MechLabView.cs`

**Steps:**

1. 搜索 `enabledWeapons`、`weapon.*toggle`、`toggle.*weapon`、`Enable`、`Disable`。
2. 删除或改名任何玩家可见的武器开关概念。
3. 保留装备存在与否作为唯一启用规则。

**Validation:**

```powershell
rg "enabledWeapons|weapon.*toggle|toggle.*weapon" unity-mc2-demo/Assets/Scripts
git diff --check
```

**Acceptance:**

- 装上武器就是启用，界面不再暗示可以开关武器。

**Commit:** `Remove weapon toggle leftovers`

### Task E2: Make Grid Item Fitting Explicit

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/MechChassisCatalog.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/WeaponCatalog.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/MechLabView.cs`

**Steps:**

1. 每台机甲定义 grid width、height 和 blocked cells。
2. 每个武器定义占用形状，例如竖三格、横两格、L 形。
3. UI 用整块组件显示武器，不用列表替代格子。
4. 装甲板和散热器作为单格填充物。
5. 冲突、超重、过热即时提示。

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-loadout-grid.log"
```

**Acceptance:**

- 玩家一眼看懂武器占了哪些格子。
- 不能放时能看懂原因。

**Commit:** `Make mech lab grid item fitting explicit`

### Task E3: Apply MechLab Loadouts In Battle

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/MechLoadoutRules.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatProfile.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`

**Steps:**

1. 任务开始前把当前 loadout 写入 unit combat profile。
2. 武器伤害、射程、冷却使用 loadout weapon definitions。
3. 装甲硬度进入伤害模型。
4. 散热器先做简单热压力折减或过热缓冲。
5. smoke 或 validator 证明换配置会改变战斗摘要。

**Validation:**

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-combat-situation.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-loadout-battle-smoke.log"
```

**Acceptance:**

- 改武器或装甲后，战斗表现或数值能观察到差异。

**Commit:** `Apply mech lab loadouts in battle`

## 9. Phase F: Debrief, Repair, Relaunch

目标：完成“改装 -> 出战 -> 战后 -> 修复 -> 再战”的本地闭环。

### Task F1: Hide Save-System Surface

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/README.md`
- Modify: `docs-mc2-detailed-development-plan.md`

**Steps:**

1. 审计 Save Slot、Continue、New Company、export/import 和 saved-account 开发面板。
2. 正常演示流程不展示复杂保存入口。
3. 有用的内部状态快照只保留为开发诊断。
4. README 标清楚 developer diagnostics 和 first demo gameplay 的区别。

**Validation:**

```powershell
rg -n "Save Slot|saved-account|Continue|New Company|save/load" unity-mc2-demo/README.md unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs
git diff --check
```

**Acceptance:**

- 第一版玩家流程不要求管理存档。

**Commit:** `Hide save system from first demo flow`

### Task F2: Tighten Debrief And Repair Loop

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DebriefView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/PostBattleReport.cs`

**Steps:**

1. 显示任务结果。
2. 显示机甲损伤摘要。
3. 显示统一代币奖励、碎片或缴获摘要。
4. 显示一键修复/回装配。
5. 机甲损毁只扣修复资金，不做等待。
6. 普通武器损毁按维修或重买处理。

**Validation:**

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-combat-situation.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-debrief-loop-smoke.log"
```

**Acceptance:**

- 战后可以自然回装配或再战。
- 不出现复杂保存和等待维修概念。

**Commit:** `Tighten debrief and repair loop`

## 10. Phase G: AI Commander Capability Window

目标：AI 做大决策，不影响本地确定性战斗。

### Task G1: Freeze Observation Contract

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs`
- Create or Modify: `docs-ai-commander-directive-contract.md`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`

**Steps:**

1. observation 只包含任务阶段、我方状态、独立命令、敌方摘要、目标状态、附近威胁和可用意图。
2. 不塞每帧 projectile、完整路径或所有内部数值。
3. 加命令行导出或 smoke 断言。
4. 文档解释字段含义。

**Acceptance:**

- observation 小到可以给延迟较高的模型使用。
- 没有模型时本地战斗照常运行。

**Commit:** `Freeze AI commander observation contract`

### Task G2: Add Directive Adapter

**Files:**

- Create or Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/AiCommanderDirective.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify: `docs-ai-commander-directive-contract.md`

**Steps:**

1. directive 类型包括 attack、defend、regroup、hold、retreat、focusTarget、protectUnit。
2. 本地 adapter 把 directive 转成已有 BattleCore 命令。
3. 超时或空回复走默认本地逻辑。
4. AI 不直接改 BattleCore 状态。

**Acceptance:**

- 一条高层 AI 指令可以变成普通游戏命令。
- 模型超时不阻塞战斗。

**Commit:** `Add AI commander directive adapter`

### Task G3: Show Compact AI Advice

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`

**Steps:**

1. 添加一个小型可选 AI 建议窗口。
2. 只显示一句短建议。
3. 玩家控制优先。
4. 不做自然语言战术聊天。

**Acceptance:**

- AI 像副官能力窗口，不像调试控制台。

**Commit:** `Show compact AI commander advice`

## 11. Phase H: Content Boundary And Public Safety

目标：本地能用私有参考包验证，公开仓库和公开构建安全。

### Task H1: Document Replaceable Content Packs

**Files:**

- Modify: `docs-content-replacement-plan.md`
- Create: `content-packs/project-owned-visual-slice.example.json`
- Modify: `unity-mc2-demo/README.md`

**Steps:**

1. 文档区分 `reference-private`、`project-original`、`community-map` 和 `server-certified`。
2. manifest 加来源、license、生成状态、public-safe 状态。
3. Unity loader 使用 asset id，不用公开产品名。

**Acceptance:**

- 同一套 game id 可以指向本地参考资产或未来自有资产。

**Commit:** `Document replaceable visual content packs`

### Task H2: Add Public Build Content Safety Notes

**Files:**

- Modify: `scripts/unity/*`
- Modify: `.gitignore`
- Modify: `README.md`
- Modify: `unity-mc2-demo/README.md`

**Steps:**

1. 增加私有参考路径检查。
2. README 强调 AI 副官 RTS 探索，不宣传原版复刻。
3. 公开构建不打包 ignored reference art。
4. 私有参考构建只作为本地开发验证。

**Acceptance:**

- GitHub 版本可展示。
- 本地私有 Demo 仍能用于验证画面和节奏。

**Commit:** `Add public build content safety notes`

## 12. Phase I: Demo Handoff

目标：整理成别人能启动、能看懂、能演示的 Windows 包。

### Task I1: Prepare Repeatable Windows Demo Build

**Files:**

- Modify: `scripts/unity/*`
- Modify: `unity-mc2-demo/README.md`
- Optional Create: `BUILD-WIN.md`

**Steps:**

1. 一条命令生成 Windows player。
2. README 放 validator、build、smoke、capture 命令。
3. 私有参考包标为可选本地依赖。
4. 明确公开包不包含私有参考资产。

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-demo-handoff.log"
```

**Acceptance:**

- 不打开 Unity Editor 也能复现本地构建。

**Commit:** `Prepare repeatable Windows demo build`

### Task I2: Add Playable Demo Walkthrough

**Files:**

- Create: `docs-demo-walkthrough-2026-06-07.md`
- Modify: `README.md`

**Steps:**

1. 写 3 分钟演示脚本：装配、进图、移动、攻击、喷射、独立命令、损伤、战后。
2. 链接本地截图证据路径。
3. 避免原版产品名和原版剧情描述。

**Acceptance:**

- 另一个人能按脚本演示 Demo。

**Commit:** `Add playable demo walkthrough`

## 13. Later Platform Work

这些是长期路线，当前只保留边界，不进入第一版本地 Demo。

| Later Phase | Start Condition | Scope |
| --- | --- | --- |
| P1 Main server prototype | Windows demo battle and MechLab stable | account id, inventory snapshot, token ledger, signed loadout, reward claim, leaderboard |
| P2 Map package/editor loop | BattleCore contract stable enough for replay or summary validation | map package schema, local editor export, uncertified map play |
| P3 Certified reward maps | main server prototype exists | certification states, session tickets, reward caps, validation |
| P4 Partner map servers | reward validation works | map server protocol, reputation, replay/digest upload |
| P5 Creator economy and optional chain | economy, refunds, moderation proven off-chain | creator revenue accounting, optional Ethereum/L2 proof or settlement |

长期原则：地图可以开放，跨地图可带出的奖励必须由主服务器认证。

## 14. Commit Order From Current Point

推荐后续提交顺序：

1. `Freeze minimal battle UI`
2. `Document command loop visual baseline`
3. `Differentiate weapon visual effects`
4. `Strengthen mech section damage cues`
5. `Simplify armor hardness damage math`
6. `Remove weapon toggle leftovers`
7. `Make mech lab grid item fitting explicit`
8. `Apply mech lab loadouts in battle`
9. `Hide save system from first demo flow`
10. `Tighten debrief and repair loop`
11. `Freeze AI commander observation contract`
12. `Add AI commander directive adapter`
13. `Document replaceable visual content packs`
14. `Prepare repeatable Windows demo build`
15. `Add playable demo walkthrough`

每个提交都记录：

- 改了什么。
- 跑了什么验证。
- 哪个日志或截图能证明。
- 下一步还剩什么问题。

## 15. Stop Conditions

遇到这些情况先停下来修，不继续堆功能：

- `hangar-contact` 或 `damage-demo` 截图比上一轮明显更糊、更挤或 UI 更挡。
- smoke test 在表现层小改后失败。
- BattleCore 合法落点和 Unity 表现不一致。
- Unity 场景文件只有 fileID churn。
- 任务引入服务器、经济、保存系统、PVP、移动端或链上代码，导致本地 Demo 主线变慢。
- 公开文档开始把私有参考素材描述成产品内容。

当前主线很简单：先把指挥闭环钉住，再把战斗反馈做出机甲味，再把装配格子做成乐趣点，最后整理演示包。平台化梦想先放在旁边，它值得做，但不能抢第一张可玩图的火候。

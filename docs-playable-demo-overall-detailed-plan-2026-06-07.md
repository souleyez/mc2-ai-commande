# Playable Demo Overall Detailed Implementation Plan

> **For Codex:** REQUIRED SUB-SKILL: Use executing-plans to implement this plan task-by-task. Use `docs-playable-demo-current-execution-plan-2026-06-07.md` as the day-to-day commit queue.

**Goal:** 做出一版 Windows 本地可玩的轻量机甲战术指挥 Demo：玩家先在机库里按格子改装机甲，再进入一张 3D 任务地图，用很少操作指挥 1-6 台机甲完成战斗，并能看到可读的地形、建筑、机甲、武器、部位损伤、战后维修和 AI 副官建议。

**Architecture:** `BattleCore` 是确定性规则层，负责任务、触发、命令、移动、喷射、碰撞占位、武器、热量、装甲硬度、部位损伤、维修、结算和 AI observation/directive。Unity 6 Presentation 只负责固定镜头、输入、UI、模型、材质、特效、截图、调试可视化和本地演示。开发期可以使用本地私有参考内容包验证比例、节奏和可读性；公开构建必须能整体切换到项目自有或合规授权内容包。

**Tech Stack:** Unity 6, C#, Windows Standalone, deterministic BattleCore, PowerShell validator/build/smoke/capture scripts, `mc2-unity-demo-contract-v1`, private local reference content pack, replaceable public content pack, Git/GitHub.

**Revision:** 2026-06-07 overall detailed plan v3. The deeper task-by-task plan is now `docs-playable-demo-v1-detailed-plan-2026-06-07.md`.

---

## 0. How To Use This Plan

这份文件是总计划。它回答“现在做到哪、为什么这么做、后面按什么阶段推进”。

实际继续开发时按这个顺序看：

1. `docs-playable-demo-v1-detailed-plan-2026-06-07.md`: 更细的产品边界、架构边界、提交级任务、验收门和后续路线。
2. `docs-playable-demo-current-execution-plan-2026-06-07.md`: 当前唯一日常执行入口，从 `Current Commit Queue` 的第一个 `Next` 或 `In Progress` 开始。
3. `docs-playable-demo-overall-detailed-plan-2026-06-07.md`: 总计划、阶段门、长期边界和历史上下文。
4. `docs-reference-visual-audit-2026-06-07.md`: 截图、sidecar、smoke、validator 证据。
5. `docs-ai-commander-directive-contract.md`: AI 副官 observation/directive 合同。
6. `docs-content-replacement-plan.md` 和 `docs-content-pack.md`: 私有参考包与公开替换包边界。
7. `docs-platform-ecosystem-plan.md`: 地图服务器、排行、奖励认证、创作者生态、链上分账等后续平台方向。

当用户说“按计划继续”：

1. 先看当前执行计划的队列。
2. 一次只做一个可验证小提交。
3. 代码、截图、验证、文档、提交一起收口。
4. 不因为长期平台想象打断第一版本地 Demo。

## 1. Product Thesis

这个项目不是要让玩家逐帧操控每台机甲，而是让玩家像战术指挥官一样下达意图。

核心乐趣有两块：

- **Mech Lab / 机库装配:** 收集机甲、武器、装甲板、散热器和推进器，在格子槽位里拼出小队战斗能力。
- **Map Combat / 地图战斗:** 带 1-6 台机甲进入任务地图，少量指令完成移动、集火、喷射、独立行动、损伤恢复和战利品结算。

AI 副官的定位：

- 做大方向判断、开局计划、阶段性建议和未来托管。
- 不代替本地 BattleCore 做逐帧移动、射击、碰撞和伤害。
- 断网、没 key、模型慢时，本地 Demo 仍然完整可玩。

长期产品想象：

- 合作商或社区可以搭建地图服务器。
- 玩家带自己的机甲小队进入不同地图作战。
- 地图编辑器和地图包可以开放。
- 可带出的奖励必须由主服务器认证。
- Web 站展示排名、地图热度、玩家小队和战绩。
- 后续可以开放皮肤、地图自定义和创作者分成。
- 链上只适合后期做创作者收益证明、皮肤收藏或活动奖池，不进入第一版本地 Demo。

### 1.1 Product Layer Map

后续不要把所有想法同时塞进第一版。产品按层推进：

| Layer | Purpose | First Visible Form | Deferred Form |
| --- | --- | --- | --- |
| 战斗核心 | 让机甲小队在地图上自己会打，玩家只下达关键意图 | 一张本地任务图，固定视角，小队命令，喷射，部位损伤 | 多任务、多地形、多敌军脚本、难度曲线 |
| 装配养成 | 让玩家在战前做有意义的机甲和武器组合 | MechLab 格子装配、热量、重量、硬度、维修 | 工匠改造、高级机体、高级武器、皮肤 |
| AI 副官 | 减少微操，把玩家从“点每一下”提升到“做战术判断” | 能力窗口、高层建议、离线 fallback | AI 托管、语音/文本指挥、纸面结算预测 |
| 内容包 | 开发期验证节奏，公开期可替换素材 | 私有参考包只做本地证据；公开包可替换 | 项目自有美术、合作方授权包、社区内容包 |
| 平台服务 | 让奖励、排行、地图生态可信 | 先只保留协议和文档边界 | 主服务器、地图服务器、认证奖励、Web 排行 |
| 创作者经济 | 让地图和皮肤贡献有收益闭环 | 只保留长期设计 | 分成、活动池、可选链上证明 |

当前只冲前四层的最小闭环：战斗核心、装配养成、AI 副官轻接入、内容包边界。平台服务和创作者经济暂时只写接口方向，不进入本地 Demo 开发节奏。

## 2. First Playable Demo Scope

第一版只做 Windows 本地可玩 Demo，目标是能演示、能截图、能讲清楚价值。

必须有：

- Unity 6 Windows Standalone 可以构建和启动。
- 一张参考任务小图 `mc2_01`。
- 3D 地形、水面、道路/跑道、建筑、树木/环境物、炮塔或静态目标可读。
- 1-6 台机甲，常规 4 台，默认第一台是指挥官机甲。
- 固定俯视战术视角，默认跟随指挥官，允许有限缩放，不做自由旋转。
- 默认全队控制：点地点移动，点目标移动攻击或集火。
- 状态栏点单台机甲，再点地点或目标，该机甲进入独立命令。
- 独立命令完成后自动归队，并接受最新全队命令。
- 喷射从每台机甲当前位置沿目标方向尝试固定距离位移，非法落点单位不动，其他合法单位位移。
- 战斗 UI 极简：机甲状态栏、喷射、任务地图、暂停/系统、必要目标状态。
- 武器装上即启用，不做武器启用/关闭开关。
- 装甲板增加整体硬度，不引入复杂逐部位装甲账本。
- 保留部位损伤：断臂、腿瘫、驾驶舱损毁/弹射、残骸。
- 战后简报、一键维修、回机库、再次出战。
- AI 副官窗口只显示高层建议和状态，不阻塞本地战斗。
- 物理占位有 BattleCore 证据；Unity 可以显示审计占位层，但不作为唯一规则。

第一版明确不做：

- 实时 PVP。
- 移动端适配。
- 地图服务器和社区编辑器。
- 账号经济、充值、提现、链上资产。
- 复杂保存槽和保存游戏 UI。
- AI 导演。
- 大模型逐帧战斗控制。
- 公开发布私有参考素材、旧作剧情、旧作专有名称、旧作商标或旧作文案。

### 2.1 First Demo Work Packages

第一版 Demo 的交付物拆成 8 个工作包。每个工作包都要有“玩家能看见的结果”和“开发能验证的证据”。

| Package | Player Result | Engineering Result | Current State | Exit Evidence |
| --- | --- | --- | --- | --- |
| W1 战场可读性 | 地形、水面、建筑、敌我单位不是色块和一团模型 | capture preset 和 sidecar 能说明镜头、单位、占位、hostile 数 | 基础完成，回归保持 | `spawn,airfield,hangar-contact,north-patrol` |
| W2 物理占位 | 单位、建筑、水域、硬障碍不会让人感觉穿模乱叠 | BattleCore occupancy 是规则真相，Unity 只显示审计层 | 已完成，回归保持 | `occupancyPlaceholders` sidecar |
| W3 指挥 UI | 默认全队、点状态栏单独命令、喷射、任务地图、系统按钮足够完成任务 | command smoke 覆盖全队、单机、喷射、归队、战报 | 已完成稀疏化 | `visible-flow-audit` smoke |
| W4 战斗表现 | 武器方向、命中、爆炸、残骸、断臂、腿瘫、弹射能讲故事 | BattleCore 部位损伤和 Unity world cue 对齐 | 基础完成，继续截图级打磨 | `damage-demo` capture + sidecar |
| W5 MechLab | 武器整块占格，装甲板/散热器补格，热量/重量/合法性清楚 | Loadout contract/validator/smoke/capture 一致 | 已完成首轮 | `mechlab` capture + loadout smoke |
| W6 战后循环 | 战报、维修、回机库、再出战顺畅，不做保存槽 | repair/relaunch 状态在 BattleCore 可测 | 基础完成，需最终演示审计 | visible flow smoke |
| W7 AI 副官 | 显示“它能帮你做大判断”，但不拖慢本地战斗 | observation/directive 合同稳定，离线 fallback | 基础完成，回归保持 | AI validator + advice window smoke |
| W8 交付包 | 协作者能构建、运行、演示、解释内容边界 | BUILD/README/证据页/边界检查齐全 | H1-H3 done, P1 next | walkthrough + build docs + evidence page |

这 8 个工作包都达到“能演示”后，才进入更多地图、公开替换包、服务器和经济系统。

## 3. Current State Snapshot

当前日期：2026-06-07。

当前分支状态：

- Branch: `master...ai-origin/master` with local demo commits ahead of remote.
- 当前日常执行入口：`docs-playable-demo-current-execution-plan-2026-06-07.md`.
- 当前下一步：`P1 Document reference content boundary`.

已收口的事实：

| Area | Current State | Evidence |
| --- | --- | --- |
| Unity Windows build | 可构建，可 batch smoke | `analysis-output/unity-build-*.log` |
| First map `mc2_01` | 可加载地形、目标、触发、单位、建筑、terrain objects、相机 | validator + capture sidecars |
| Visible flow | 机库、出战、独立命令、喷射、集火、战报、回机库、再启动 identity 已覆盖 | `mc2_01-visible-flow-audit.txt` |
| Battle occupancy | 单位、建筑、硬 terrain object、水域/边界占位已有 BattleCore evidence | `OccupancySummary()` + sidecars |
| Occupancy placeholders | 可生成审计占位证据，当前 sidecar 已报告 hard props 和 placeholders | `analysis-output/reference-visual-captures/*.json` |
| Terrain readability | 不再是黑块主画面，水、岸线、跑道/道路、建筑基底已可读 | reference captures |
| HUD | 战斗中信息已压缩，不追求大量数据面板 | reference captures |
| Combat cues | 武器方向、命中、爆炸、残骸、断臂、腿瘫、弹射已有基础 | validator + `damage-demo` |
| Armor hardness | 装甲板走整体硬度，部位损伤仍保留 | validator |
| MechLab | 整块武器 block summary、形状标签、装甲板/散热器单格填充、热量、重量、合法性已有 smoke 和截图证据 | loadout validator + `mechlab` capture |
| Debrief loop | 战报、维修、回机库、再出战基础完成，不暴露保存槽概念 | repair/relaunch validator + smoke |
| AI observation | Compact schema and prompt budget locked | `analysis-output/unity-validate-ai-observation.log` |
| AI directive | High-level directive adapter guarded, missing key falls back locally | `analysis-output/unity-validate-ai-directive.log` |
| AI window | System panel 已有 compact AI Deputy / AI副官窗口 | `analysis-output/unity-player-ai-advice-window.log` |
| Content boundary | README 已转向 AI RTS commander exploration 叙事，内容包替换方向已有文档 | README + content-pack docs |

当前最重要的问题：

| Gap | Why It Matters | Current Plan |
| --- | --- | --- |
| Demo 公开边界还需要收口 | H3 已补截图证据页；后续融资或协作需要更明确的私有参考内容/公开替换包边界 | P1/P2 content boundary |
| 碰撞占位后续只需回归 | V3 已提供单位、结构、hardProp 和 landing blocked 审计层；后续发现具体碰撞 bug 再加 close-up preset | V3 regression |
| 公开内容安全还要脚本 guard | 本地参考包可以开发验证，公开包不能混入旧素材 | P1/P2 content boundary |

### 3.1 Current Phase Judgment

现在不是“从零搭技术”的阶段，也还不是“扩内容/做平台”的阶段。当前阶段应定义为：

```text
Playable Demo Handoff: 把已能跑的本地 Demo 收成可重复构建、可展示、可解释、可继续开发的版本。
```

当前阶段的优先级：

1. 可重复构建：任何协作者能按 `BUILD-WIN.md` 构建 Windows Demo。
2. 可重复验证：validator、smoke、capture 命令能证明主流程没退化。
3. 可重复演示：walkthrough 能三分钟讲清 MechLab、战斗、损伤、维修和 AI 副官。
4. 可安全公开：README 和内容包文档不把私有参考素材当作公开产品资产。
5. 可继续扩展：后续地图、机甲、服务器、AI 托管都不需要推翻 BattleCore/Unity 边界。

当前阶段暂时不碰：

- 新经济系统；
- 新账号系统；
- 新 PVP；
- 新地图服务器；
- 大规模素材替换；
- 复杂 AI 托管。

这些都等 P1-P2 收口后再开新阶段计划。

## 4. Architecture Rules

### 4.1 BattleCore Owns Gameplay Truth

BattleCore owns:

- mission contract loading;
- objective and trigger state;
- squad command acceptance;
- detached single-unit command state;
- auto rejoin;
- jet landing legality;
- unit, structure, hard terrain object, water and map-bound occupancy;
- weapon range, cooldown, damage, heat and armor hardness;
- section damage, destruction and cockpit ejection state;
- debrief, repair and relaunch rules;
- compact AI observation and high-level directive adapter.

Core files:

- `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/RuleCommander.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/MiniMaxCommander.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutContract.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutValidator.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/MechBayInventoryContract.cs`

Rule: if a behavior affects movement, damage, victory, loss, repair, reward or AI decision, it must exist in BattleCore or contract data first.

### 4.2 Unity Presentation Owns Visibility

Unity Presentation owns:

- click/raycast input;
- fixed tactical camera and limited zoom;
- sparse battle HUD;
- MechLab layout;
- model/material/terrain rendering;
- visual collision placeholders;
- weapon trails, impact cues, damage fragments and ejection cues;
- command-file smoke handling;
- screenshot capture and sidecar summaries.

Core files:

- `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/DemoTerrainView.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`
- `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- `unity-mc2-demo/Assets/Editor/Mc2DemoBuilder.cs`
- `scripts/unity/capture_reference_visuals.ps1`

Rule: Unity can show collision rings, blocker boxes, fade, silhouettes and debug markers, but cannot become the only gameplay collision system.

### 4.3 Content Pack Boundary

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

Public pitch:

```text
AI-assisted tactical RTS commander exploration with deterministic mech squad battle,
optional AI deputy, replaceable content packs, and future community map ecosystem.
```

### 4.4 AI Boundary

AI can:

- read compact observation;
- draft an opening plan;
- choose one high-level directive for a phase;
- show one short advice line in UI;
- support future AI托管 and paper-resolution calculations.

AI cannot:

- mutate `BattleMission` directly;
- decide every shot or every frame;
- choose exact player-facing coordinates;
- block local battle when model API is slow or unavailable;
- become required for smoke tests.

## 5. Validation Bus

Every commit:

```powershell
git diff --check
git status --short --branch
```

BattleCore or contract change:

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-current.log"
```

Unity presentation change:

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

## 6. Milestone Roadmap

### Milestone 0: Plan Hygiene

**Status:** Active for this document.

**Goal:** 让所有后续开发都有一个清楚的总计划和一个唯一当前执行入口。

**Tasks:**

1. 新增本总计划。
2. 在当前执行计划里指向本总计划。
3. 保留旧 `docs-playable-demo-*.md` 作为历史，不再让它们竞争执行入口。
4. 之后每完成一个小提交，在当前执行计划和视觉审计文档中更新结果。

**Acceptance:**

- 用户问“整体计划”时能指向本文件。
- 用户说“按计划继续”时能从当前执行计划的队列直接开工。

### Milestone 1: Reference Visual Readability

**Status:** V2 and C1 completed 2026-06-07. Keep this milestone in regression while the next active task moves to C2.

**Goal:** 让第一张图看起来像 3D 战场，不像色块或模型团。优先解决远景可读性、损伤 spotlight、局部拥挤、材质对比和构图。

**Key files:**

- `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/DemoTerrainView.cs`
- `scripts/unity/capture_reference_visuals.ps1`
- `docs-reference-visual-audit-2026-06-07.md`

**Task V2-A: Strengthen damage spotlight**

1. Increase player-unit damage ground rings and vertical beacons for critical, lost-limb and pilot-ejection cases.
2. Keep enemy damage cues present but less dominant than player damage cues.
3. Update summary tokens and validator expectations so the cue is auditable.
4. Re-capture `damage-demo`.

**Task V2-B: Improve hangar combat composition**

1. Re-check `hangar-contact` sidecar: active hostiles, visible hostiles, camera ortho, composition offset, occupancy placeholders.
2. If units still read as one knot, adjust deterministic attack/parking slots before reducing enemy count.
3. Keep original trigger pressure unless a later pacing task explicitly retunes mission difficulty.
4. Re-capture `hangar-contact` and `north-patrol` to make sure open-terrain readability does not regress.

**Task V2-C: Reduce FX and material noise**

1. Keep weapon direction readable.
2. Avoid oversized impact sprites hiding mech silhouettes.
3. Preserve terrain contrast: water/shore/road/grass/building bases must not return to dark monotone.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-visual-readability.log"
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-visual-readability.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol
```

**Acceptance:**

- Five captures are nonblank and readable.
- `hangar-contact` does not look like every actor shares one coordinate.
- `damage-demo` shows at least one obvious world-space damage event.
- UI does not hide the core fight.

### Milestone 2: Occupancy And Physical Placeholder Evidence

**Status:** Completed 2026-06-07. Keep this milestone in regression unless collision doubt returns.

**Goal:** 用户和开发者都能看见“这里确实有物理占位”。BattleCore 负责真实规则；Unity 显示一个可开关的审计层。

**Key files:**

- `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/DemoTerrainView.cs`
- `scripts/unity/capture_reference_visuals.ps1`
- `docs-reference-visual-audit-2026-06-07.md`

**Task V3-A: Lock placeholder review layer**

1. Confirm placeholder source is `BattleMission` occupancy, not Unity-only collider state.
2. Show unit radius, targetable structure blockers, hard terrain object blockers, water/illegal landing zones and map boundary hints.
3. Make it capture-only or debug-toggle visible; normal player HUD stays clean.
4. Add sidecar summary: enabled/disabled, blocker counts, categories and source.

**Task V3-B: Audit jet landing legality visually**

1. Add capture preset or assertion for water-edge jet attempts.
2. Show that a unit whose landing point is water stays still while other legal units move.
3. Keep player-facing battle UI simple; this is evidence, not a permanent gameplay overlay.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-occupancy-placeholder.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets hangar-contact,damage-demo
```

**Acceptance:**

- Sidecar proves occupancy category counts.
- Screenshot can show blockers when debug review is enabled.
- Normal battle screenshot remains sparse and not cluttered.

### Milestone 3: Command And Battle UI

**Status:** Mostly built, needs final audit.

**Goal:** 战斗中不用显示太多信息。玩家靠状态栏、点地点/目标、喷射和系统按钮就能完成指挥。

**Key files:**

- `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`
- `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-visible-flow-audit.txt`
- `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Tasks:**

1. Audit battle HUD text density at 1280x720.
2. Keep mech status rows readable: damage part, structure/health, solo command status.
3. Keep only essential buttons: jet, map, pause/system.
4. Ensure status-row selection -> map click -> solo command -> auto rejoin is smoke-tested.
5. Keep commander camera following first roster unit.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-battle-ui.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,hangar-contact,damage-demo
```

**Acceptance:**

- No big tutorial wall in battle.
- No unnecessary weapon toggles.
- No saving-game language in first demo flow.
- User can infer command state from status row and unit behavior.

### Milestone 4: Combat Feel And Damage Story

**Status:** Built as foundation, needs stronger presentation.

**Goal:** 战斗要能讲故事：谁在打谁、什么武器命中、哪台机甲受损、断了什么部位、驾驶员是否弹射。

**Key files:**

- `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Tasks:**

1. Weapon families get distinct readable cues: beam, missile, ballistic, explosion.
2. Damage sections get world-space cues: arm lost, leg mobility loss, cockpit breach/ejection, wreck.
3. Status rows mirror key damage without becoming a spreadsheet.
4. Armor hardness remains simple: armor plates increase global hardness; section damage stays alive.
5. Destroyed mechs and cockpit damage feed debrief/repair/ejection logic.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-combat-damage.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-combat-situation.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-combat-damage.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets damage-demo
```

**Acceptance:**

- `damage-demo` can show at least one unmistakable limb/cockpit/wreck event.
- Effects help readability instead of covering models.
- Damage rules remain deterministic in BattleCore.

### Milestone 5: MechLab Block-Fitting Slice

**Status:** M1 and M2 completed; keep in regression while the next active task moves to C2.

**Goal:** 装配界面尽量参照原作“整块武器放格子”的直观乐趣，同时保持第一版轻量。

**Key files:**

- `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutContract.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutValidator.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/MechBayInventoryContract.cs`
- `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Tasks:**

1. Done: mounted weapon renders as one contiguous block with block summary evidence.
2. Done: multi-cell weapon shows occupied shape and internal cell divisions.
3. Done: armor plate and heat sink are single-cell fillers.
4. Done: heat, mass, slot conflict and legal/illegal state update immediately.
5. Done: weapon mounted means weapon enabled; player-facing toggle language is rejected by smoke.
6. Done: capture first-class MechLab screenshot evidence.
7. Ongoing regression: loadout affects BattleCore unit stats, weapons, heat capacity and armor hardness.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-mechlab.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-loadout-compact.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-mechlab.log"
```

**Acceptance:**

- 装配格子一眼能看懂什么占了哪里。
- 过热、超重、槽位冲突不需要读长说明。
- 出战单位使用装配结果。

### Milestone 6: Debrief, Repair And Relaunch

**Status:** Base done, needs final demo audit.

**Goal:** 不做复杂保存系统。第一版只要战后简报、一键维修、回机库、再出战。

**Key files:**

- `unity-mc2-demo/Assets/Scripts/BattleCore/MechBayInventoryContract.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`
- `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Tasks:**

1. Debrief shows outcome, repair need, key damage and simple reward placeholder.
2. Repair is one-click token cost, no waiting.
3. Destroyed/disabled mechs cannot relaunch until repaired.
4. Relaunch preserves loadout identity.
5. Remove save slot, restart run, account or live-service wording from first demo.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-debrief-repair.log"
```

**Acceptance:**

- 战后不显得像半成品菜单。
- 玩家能清楚回到装配并再打一局。

### Milestone 7: AI Deputy Light Integration

**Status:** Base done, keep in regression.

**Goal:** AI 做能力窗口和高层建议，不进入逐帧战斗。

**Key files:**

- `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/MiniMaxCommander.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/RuleCommander.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- `docs-ai-commander-directive-contract.md`

**Tasks:**

1. Keep compact observation stable and small.
2. Keep directive vocabulary high-level: assault objective, engage hostiles, regroup, hold.
3. UI window shows state, mode, intent and one short advice line.
4. Missing key or timeout always falls back locally.
5. Do not spend model tokens in smoke tests.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-current.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-ai-current.log"
```

**Acceptance:**

- No API key path works.
- AI window does not enlarge battle UI.
- AI reads as optional deputy, not required runtime.

### Milestone 8: Demo Handoff And Public Boundary

**Status:** Active. H1 walkthrough, H2 repeatable Windows build, and H3 evidence packaging completed 2026-06-07; P1 content-boundary documentation is next.

**Goal:** 把本地 Demo 收成可演示包：能构建、能截图、能讲、能明确区分私有参考内容和公开内容。

**Key files:**

- `README.md`
- `BUILD-WIN.md`
- `docs-content-replacement-plan.md`
- `docs-content-pack.md`
- `scripts/content-pack/*`
- `scripts/unity/capture_reference_visuals.ps1`

**Tasks:**

1. Done: write a three-minute playable walkthrough.
2. Done: prepare repeatable Windows demo build command.
3. Done: package evidence list: logs, scripts, screenshot preset names, not generated binaries unless requested.
4. Next: document private reference pack versus public replacement pack.
5. Add a public content boundary check that fails if forbidden reference paths/names are packaged.
6. Update GitHub README only with project-owned pitch and AI RTS commander exploration language.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-demo-handoff.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol
```

**Acceptance:**

- A collaborator can build and run the Windows Demo locally.
- README does not market the project as a clone.
- Public build path does not require private reference content.

## 7. Deferred Platform Plan

这些内容保留设计，不进入第一版本地 Demo 主线。

### 7.1 Map Server And Certified Rewards

Long-term model:

- Unity client asks main server for account, inventory and signed squad loadout.
- Map server hosts battle session and map runtime.
- Main server validates battle result and issues portable rewards.
- Map output is a claim, not a grant.

First future prototype after local demo:

1. Static account/inventory mock.
2. Signed local squad loadout hash.
3. Map package metadata.
4. Battle result summary.
5. Server-side reward claim stub.

### 7.2 Community Map Editor

Future map packages should contain:

- map id, version, title, author, license and provenance;
- terrain data and navigation metadata;
- objectives, trigger graph, enemy waves, structures, turrets and props;
- allowed squad size and expected difficulty;
- reward table references, not direct reward definitions;
- BattleCore compatibility version.

Certification states:

- Draft: local editor only.
- Uncertified Public: playable for fun, no portable rewards.
- Certified: eligible for main-server rewards under caps.
- Event: curated season or partner campaign.
- Retired: preserved, disabled for new rewards.

### 7.3 Economy And Social Pilots

Future economy direction:

- One game token.
- Earned from tasks, salvage sale, weapon sale and paid acquisition.
- Spent on NPC purchase, repairs and supplies.
- Mech fragments from first-clear, events and eligible multiplayer/team content.
- Ordinary weapons can be bought; advanced mechs and upgraded weapons are not simple NPC purchases.
- Friend-referred pilots can generate small daily in-game wage if active.
- NPC pilots can be hired and can die.
- Player commander pilot does not die.

Keep all this outside first local Demo until the combat and MechLab are convincing.

### 7.4 Blockchain Position

Do not put core combat, mech stats, normal inventory, repair costs or battle outcomes on-chain early.

Possible late uses:

- creator revenue proof;
- transparent event prize pools;
- optional cosmetic ownership;
- limited commemorative skins or badges.

Reason: balance, refund, fraud handling, bans, rollback and customer support need operational control while the economy is still changing.

## 8. Current Commit Queue Summary

The live detailed queue remains in `docs-playable-demo-current-execution-plan-2026-06-07.md`.

Current recommended order:

| Order | Status | Commit | Purpose |
| --- | --- | --- | --- |
| 1 | Done | `Improve reference visual readability` | Make `hangar-contact` and `damage-demo` read as battlefield, not model knot |
| 2 | Done | `Lock occupancy placeholder review layer` | Make physical blockers auditable without cluttering player HUD |
| 3 | Done | `Polish MechLab block fitting` | Make loadout grid closer to original-style block fitting |
| 4 | Done | `Capture MechLab fitting evidence` | Prove loadout UI and smoke flow |
| 5 | Done | `Strengthen damage demo readability` | Push limb/cockpit/ejection story into screenshot-grade clarity |
| 6 | Done | `Keep battle UI sparse` | Final pass against too much battle information |
| 7 | Done | `Write playable demo walkthrough` | Create three-minute demo narrative |
| 8 | Done | `Prepare repeatable Windows demo build` | Make local build/running repeatable |
| 9 | Done | `Package playable demo evidence` | Collect proof paths, not generated artifacts |
| 10 | Next | `Document reference content boundary` | Make private/public content split explicit |
| 11 | Pending | `Add public content boundary check` | Prevent private reference content from leaking into public build |

## 9. Stop Rules

Do not move to platform/server/economy work while any of these are still true:

- The main battle screenshot looks like unreadable blocks or one model pile.
- Unit collision/occupancy cannot be demonstrated.
- The player cannot finish one local mission loop without explanation.
- MechLab does not communicate weapon shape, heat, weight and legality.
- The demo cannot build on Windows through the documented command.
- Public content boundary is unclear.

Do not add new UI unless it improves one of:

- command clarity;
- damage readability;
- loadout legality;
- demo handoff.

Do not add new AI behavior unless it stays inside:

- observation;
- high-level directive;
- short advice window;
- future paper/托管 interface.

## 10. Definition Of Done For First Demo

The first Demo is considered ready to show externally when:

1. `git diff --check` passes.
2. Unity validator passes.
3. Windows build passes.
4. Visible flow smoke exits with code `0`.
5. Five reference captures are nonblank and visually readable.
6. `damage-demo` clearly shows a damage story.
7. `hangar-contact` reads as a tactical fight, not one coordinate pile.
8. MechLab grid shows block-fitting, heat, weight and legal/illegal state.
9. Battle UI stays sparse.
10. AI deputy window is optional and offline-safe.
11. README and content docs describe the project as AI-assisted tactical RTS exploration, not a clone.
12. Private reference content is documented as replaceable and excluded from public distribution.

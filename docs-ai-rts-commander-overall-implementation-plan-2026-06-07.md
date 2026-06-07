# AI RTS Commander V1 Overall Implementation Plan

> **For Codex:** REQUIRED SUB-SKILL: Use executing-plans to implement this plan task-by-task.

**Goal:** 把当前 Unity 6 Windows 本地原型收成一版能演示、能融资说明、能继续扩展的 AI 副官战术 RTS Demo：玩家在机库改装机甲小队，进入一张 3D 任务地图，用少量指挥完成战斗，并能看到可读地形、碰撞占位、武器效果、部位损伤、战后维修和高层 AI 建议。

**Architecture:** `BattleCore` 是确定性规则层，负责命令、移动、喷射落点、占位、武器、热量、装甲硬度、部位损伤、结算、维修、奖励草案和 AI observation/directive。Unity 6 只负责固定镜头、输入、UI、模型、材质、特效、截图、构建和本地演示。开发期可以用本地私有参考内容验证比例和节奏，公开演示或商业版本必须切到项目自有或合规授权内容包。

**Tech Stack:** Unity 6, C#, Windows Standalone first, deterministic BattleCore, PowerShell validator/build/smoke/capture scripts, replaceable content packs, optional high-level AI deputy adapter, future main server and map server contracts.

**Revision:** 2026-06-07 v4. This is the fixed overall plan. The current master execution plan is `docs-ai-rts-commander-current-master-plan-2026-06-07.md`; the previous detailed execution plan is `docs-ai-rts-commander-v1-detailed-execution-plan-2026-06-07.md`; the supporting roadmap is `docs-ai-rts-commander-detailed-roadmap-2026-06-07.md`; the archived day-to-day queue remains `docs-playable-demo-current-execution-plan-2026-06-07.md`.

---

## 0. How To Use This Plan

这份文件是当前“整体计划定格版”。它回答三个问题：

1. 当前项目做到什么程度。
2. 后续应该按什么阶段推进。
3. 用户说“按计划继续”时，下一步具体做什么。

当前计划入口分三层：

```text
docs-ai-rts-commander-overall-implementation-plan-2026-06-07.md
docs-ai-rts-commander-current-master-plan-2026-06-07.md
docs-ai-rts-commander-v1-detailed-execution-plan-2026-06-07.md
docs-ai-rts-commander-detailed-roadmap-2026-06-07.md
docs-playable-demo-current-execution-plan-2026-06-07.md
```

分工：

1. 本文件管产品方向、架构边界、里程碑和长期平台路线。
2. `docs-ai-rts-commander-current-master-plan-2026-06-07.md` 管当前提交级主队列、视觉/碰撞回归、私有参考视觉边界和近期执行顺序。
3. `docs-ai-rts-commander-v1-detailed-execution-plan-2026-06-07.md` 保留上一版提交级执行计划、文件入口、命令、验收标准和已完成任务细节。
4. `docs-ai-rts-commander-detailed-roadmap-2026-06-07.md` 管当前真实状态、阶段拆解、历史工作包和路线说明。
5. `docs-playable-demo-current-execution-plan-2026-06-07.md` 保留旧的日常执行上下文和已完成任务证据。

执行顺序：

1. 先看当前主计划 `docs-ai-rts-commander-current-master-plan-2026-06-07.md` 的第一个 `Next` 或 `In Progress`。
2. 每次只做一个可验证小提交。
3. BattleCore 规则先于 Unity 表现。
4. 截图和 smoke 只能作为证据，不能代替规则验证。
5. 不提交 generated PNG/JSON/log/build artifacts，除非用户明确要求。
6. Unity build 后检查 scene fileID churn，非必要不入库。
7. 任何公开文案只讲本项目自己的 AI RTS 指挥探索，不把本地参考内容当产品身份。
8. 如果工作区出现新的 source WIP，先完成 validator/capture/docs 验收，再开新功能。

## 1. Product North Star

玩家不是来微操每一次移动和开火的。玩家应该像战术指挥官一样决定：

- 带哪几台机甲。
- 怎么装武器、装甲、散热和推进器。
- 接什么任务。
- 什么时候推进、集火、喷射、撤退或托管给 AI。
- 战后如何维修、换装和再次出战。

AI 副官的价值不是替玩家逐帧操作，而是让机甲小队更像受训部队：

- 读懂任务目标和战场压力。
- 给出开局计划和阶段建议。
- 后续托管玩家队伍做低频大决策。
- 模型慢、断网、没 key 时，本地战斗仍然完整可玩。

第一版只证明这件事：

```text
MechLab 改装乐趣 + 固定视角 3D 任务战斗 + 部位损伤卖点 + 可选 AI 高层建议
```

## 2. Current Stage

当前阶段不是从零开发，也不是平台化扩张。现在处在：

```text
Playable Demo Handoff: 把已能跑的 Windows 本地 Demo 收成可重复构建、可展示、可解释、可继续开发的版本。
```

当前已完成的主要能力：

| Area | Current State | Evidence |
| --- | --- | --- |
| Unity Windows build | 可 batch build，可 smoke | `analysis-output/unity-build-*.log` |
| First mission map | `mc2_01` 可加载地形、单位、结构、目标、触发、相机 | validator + capture sidecars |
| Terrain readability | 水面、岸线、跑道/道路、建筑基底已可读 | reference capture presets |
| Physical occupancy | 单位、建筑、硬道具、水域、地图边界有 BattleCore 证据 | `OccupancySummary()` + sidecars |
| Command loop | 默认全队、状态栏单选、独立命令、自动归队、喷射、战报基础完成 | visible-flow smoke |
| Sparse battle UI | 战斗中不显示过量信息，只保留必要状态和按钮 | UI smoke + captures |
| Damage story | 武器命中、爆炸、断臂、腿瘫、驾驶舱弹射已有基础 | validator + `damage-demo` |
| MechLab | 整块武器占格、装甲板、散热器、热量、重量、合法性和战斗数值接入已有基础 | loadout validator + `mechlab` capture |
| Debrief loop | 战报、一键维修、回机库、再次出战已有基础 | repair/relaunch smoke |
| AI deputy | compact observation、directive adapter、AI advice window 已有基础 | AI validator + smoke |
| Content boundary | README 和内容包文档已开始转向项目自有叙事 | README + content docs |

当前真实缺口：

| Gap | Why It Matters | Next Move |
| --- | --- | --- |
| 视觉还需要稳定回归 | 当前已有样子，但机甲、道具、遮挡、占位仍要避免退化成堆叠 | 每次视觉改动跑 capture + sidecar |
| Demo handoff 已过 development-only 门检 | 构建、smoke、截图、walkthrough、内容边界已能一口气解释；当前 dev build 会被正确标记为 development-only | Guard sparse battle UI regression |
| 压力图已完成 V4 占位刷新 | `hangar-contact` 仍是最拥挤截图，但已有 `unitRadii 24/54/64` 和 `ContactSpread` 证据 | Refresh full evidence before next feature |
| 公开替换包还没进入生产 | 投资/公开演示需要至少 text-safe，最好 art-safe | 开 P3 content replacement slice |

## 3. First Version Scope

### 3.1 Must Have

第一版 Demo 必须有：

- Windows 本地可玩，不先做安卓、iOS、实时 PVP。
- 1 张小任务图，先用 `mc2_01` 作为验证图。
- 1-6 台机甲，常规 4 台，排序第一台是指挥官机甲。
- 固定俯视战术视角，默认跟随指挥官，允许有限缩放，不做自由旋转。
- 地形、水面、岸线、道路/跑道、建筑、树木/环境物、炮塔或静态目标可读。
- BattleCore 有单位、建筑、硬道具、水域、地图边界的占位规则。
- Unity 可显示碰撞占位审计层，但不能把 Unity collider 当唯一规则。
- 默认全队控制：点地点移动，点目标移动攻击或集火。
- 状态栏点单台机甲，再点地点或目标，该机甲进入独立命令。
- 独立命令完成后自动归队，并接受最新全队命令。
- 喷射按每台机甲当前位置朝目标方向尝试固定距离位移，非法落点单位保持不动。
- 战斗 UI 极简：机甲状态栏、喷射、任务地图/目标卡、暂停/系统。
- 武器装上即启用，不做武器启用/关闭开关。
- MechLab 采用整块占格：武器块、装甲板、散热器、推进器/雷达槽位。
- 装甲板增加整体硬度，不做复杂逐部位装甲账本。
- 保留部位损伤：断臂、腿瘫、驾驶舱损毁/弹射、残骸。
- 战后简报、一键维修、回机库、再次出战。
- AI 副官只做高层建议和能力窗口，不控制逐帧战斗。

### 3.2 Must Not Have

第一版明确不做：

- 实时 PVP。
- 移动端适配。
- 地图服务器和地图编辑器。
- 账号系统、充值、提现、链上资产。
- 复杂保存槽或保存游戏 UI。
- AI 导演。
- 大模型逐帧移动、开火、回避。
- 公开发布私有参考素材、旧剧情、第三方商标、专有名称或未清权资源。

## 4. Product Modules

| Module | First Demo Result | Later Result |
| --- | --- | --- |
| MechLab | 机甲、武器、装甲、散热、载重、热量、合法性形成可见改装乐趣 | 工匠改造、高级机甲、高级武器、皮肤、工坊 |
| Map Combat | 一张 3D 任务图可玩，少量指挥完成战斗 | 多地图、多任务链、多敌方脚本、多难度 |
| Damage Model | 部位损伤有卖点，断臂/瘫痪/弹射可见 | 更细的武器类型、装甲规则、驾驶员技能影响 |
| AI Deputy | 能读战况并给高层建议 | AI 托管、语言指挥、纸面计算预测、支援队伍行动 |
| Content Pack | 开发参考包与公开替换包边界清楚 | 项目自有包、合作方授权包、社区内容包 |
| Platform | 文档保留接口方向 | 主服务器、地图服务器、排行、认证奖励 |
| Creator Economy | 只写长期规则 | 地图分成、皮肤、活动池、可选链上证明 |

## 5. Architecture Contracts

### 5.1 BattleCore Owns Truth

BattleCore owns:

- mission contract loading;
- objective and trigger state;
- squad command state;
- detached single-unit command state;
- auto rejoin;
- movement and jet landing legality;
- unit, structure, hard prop, water and map-bound occupancy;
- weapon range, cooldown, heat and damage;
- armor hardness;
- section damage, cockpit breach, ejection and wreck state;
- debrief, repair and relaunch;
- compact AI observation and high-level directive interpretation.

Primary files:

- `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/RuleCommander.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/MiniMaxCommander.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutContract.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutValidator.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/MechBayInventoryContract.cs`

Rule:

```text
任何影响移动、命中、伤害、胜负、维修、奖励或 AI 决策的行为，先进入 BattleCore 或 contract 数据。
```

### 5.2 Unity Presentation Owns Visibility

Unity owns:

- click/raycast input;
- fixed tactical camera and limited zoom;
- sparse battle HUD;
- MechLab layout;
- model/material/terrain rendering;
- visual occupancy placeholder layer;
- weapon trail, impact, explosion, damage fragment and ejection cue;
- command-file smoke hooks;
- screenshot capture and sidecar summaries.

Primary files:

- `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/DemoTerrainView.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`
- `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- `unity-mc2-demo/Assets/Editor/Mc2DemoBuilder.cs`
- `scripts/unity/capture_reference_visuals.ps1`

Rule:

```text
Unity 可以把规则结果表现得更清楚，但不能独自决定规则结果。
```

### 5.3 AI Boundary

AI may:

- read compact observation;
- draft an opening plan;
- choose one high-level directive for a phase;
- show a short advice line;
- support future AI托管 and paper simulation.

AI must not:

- mutate `BattleMission` directly;
- decide every frame, shot or dodge;
- block local battle when model API is slow;
- spend tokens during smoke tests;
- become required for a playable local demo.

## 6. Content Boundary

开发期本地参考内容只能用于：

- 验证地图比例。
- 验证地形、建筑、机甲、武器效果的可读性。
- 验证任务节奏。
- 验证部位损伤和模型节点表现。

公开或投资安全版本必须使用：

- 项目自有或合规授权名称。
- 项目自有或合规授权 UI 文案。
- 项目自有或合规授权模型、贴图、音频、图标、特效。
- provenance manifest。
- public content boundary check。

内容替换状态：

| State | Use | Public |
| --- | --- | --- |
| Local reference pack | 私有开发验证 | Never |
| Reference-linked dev pack | 私有替换开发 | Never |
| Text-safe slice | 名称和文本安全，视觉可能仍是参考证据 | Controlled only |
| Art-safe slice | 文本和视觉都已自有或授权 | Yes |
| Clean public pack | 完整公开包 | Yes |

文档规则已经变成脚本 guard：

```text
scripts/content-pack/check_public_content_boundary.ps1
```

## 7. Milestone Roadmap

### Milestone 0: Plan And Handoff Hygiene

**Status:** Active.

**Goal:** 让计划、README、构建命令、演示脚本、证据路径和内容边界不打架。

**Tasks:**

1. Keep this plan as the overall detailed plan.
2. Keep `docs-playable-demo-current-execution-plan-2026-06-07.md` as the daily queue.
3. Keep old `docs-playable-demo-*.md` files as history/evidence unless explicitly consolidated later.
4. Keep P2 public content boundary check documented and in the validation bus.
5. Run a full handoff audit.

**Exit Gate:**

- README points to the current plan.
- Current execution queue has one clear `Next`.
- Public content boundary script exists and is documented.

### Milestone 1: Demo Handoff Gate

**Status:** Completed 2026-06-07.

**Result:** H4 confirmed the local Windows Demo is buildable, smoke-tested, capturable and explainable as a development Demo. Validator, Windows build and visible-flow smoke passed. Six capture presets refreshed. The clean starter manifest passes public boundary, while the current dev build is correctly marked development-only.

**Goal:** 能把本地 Demo 给协作者或投资人看，并解释它的产品价值和开发边界。

**Tasks:**

1. Run `git diff --check`.
2. Run Unity validator.
3. Run Windows build.
4. Run visible-flow smoke.
5. Capture `spawn,airfield,hangar-contact,damage-demo,north-patrol,mechlab`.
6. Run public content boundary check on build path.
7. Update evidence doc with pass/fail and any warnings.

**Exit Gate:**

- Build succeeds.
- Smoke exits code `0`.
- Captures are nonblank and readable.
- Any private reference findings are documented as development-only, not public-safe.

### Milestone 2: Visual And Occupancy Polish

**Status:** After handoff gate.

**Goal:** 进一步解决“模型堆在一起”和“碰撞占位感不强”的问题，让第一张图更像可玩的 3D 战术地图。

**Tasks:**

1. Add close-up capture preset for crowded contact area.
2. Compare unit positions, structure blockers and hard prop blockers in sidecar.
3. Add or tune BattleCore spacing rules only where evidence shows overlap.
4. Add Unity visual placeholder cues only as audit or optional debug.
5. Tune mech/vehicle/building scale if screenshot readability regresses.
6. Keep battle UI sparse.

**Files:**

- `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- `scripts/unity/capture_reference_visuals.ps1`
- `docs-reference-visual-audit-2026-06-07.md`

**Exit Gate:**

- No visible model knot in main combat preset.
- Occupancy sidecar reports blockers and units separately.
- Jet illegal landing behavior still works.

### Milestone 3: Combat Feel And Damage Story

**Status:** After visual/occupancy polish.

**Goal:** 战斗看起来像机甲战斗，而不是普通 RTS 血条互扣。

**Tasks:**

1. Differentiate laser, missile, ballistic and explosion effects.
2. Make impact direction visible at screenshot scale.
3. Strengthen section damage cues for arm loss, leg mobility loss and cockpit ejection.
4. Keep section damage state in BattleCore as truth.
5. Add one damage-focused smoke or validator assertion.
6. Update `damage-demo` evidence.

**Exit Gate:**

- `damage-demo` 一眼能看出部位损伤故事。
- 状态栏显示损伤，但不变成大仪表盘。
- Weapon effects do not hide terrain or unit silhouettes.

### Milestone 4: MechLab Core Fun

**Status:** Active foundation, later polish.

**Goal:** 装配界面成为第一版的主要乐趣点之一。

**Tasks:**

1. Keep weapon blocks as whole grid pieces.
2. Keep weapons always active when mounted.
3. Add clear legal/illegal state for heat, weight and occupied cells.
4. Keep armor plates and heat sinks as simple single-cell fillers.
5. Feed fitted loadout into battle stats.
6. Add a before/after battle smoke showing loadout effect.
7. Keep the UI clean and close to familiar equipment-grid mental model.

**Exit Gate:**

- 玩家不看说明也能理解为什么某个装备放得下或放不下。
- Heat/mass/legal state is visible.
- BattleCore sees the same loadout that UI displays.

### Milestone 5: Debrief, Repair And Relaunch

**Status:** Foundation done, polish later.

**Goal:** 不做复杂保存槽，只做战后简报、一键维修、回机库、再次出战。

**Tasks:**

1. Keep debrief compact.
2. Show damaged sections and repair cost.
3. One-click repair restores battle-ready state.
4. Relaunch same mission with the same squad identity.
5. Hide save-slot language from battle UI.

**Exit Gate:**

- Three-minute walkthrough can complete MechLab -> battle -> debrief -> repair -> relaunch.
- No manual file/save concept appears in normal player flow.

### Milestone 6: AI Deputy V1

**Status:** Foundation done, future expansion.

**Goal:** AI 只做大决策，让本地战斗规则保持稳定。

**Tasks:**

1. Keep compact observation schema stable.
2. Keep high-level directive adapter offline-safe.
3. Add a clear capability window: plan, risk, suggested priority, fallback.
4. Add AI托管 interface later, but still translate to BattleCore commands.
5. Add paper simulation only after local battle loop is convincing.

**Exit Gate:**

- No-key path works.
- Model timeout path works.
- AI advice does not enlarge battle UI or add token usage to smoke tests.

### Milestone 7: Public Replacement Content Slice

**Status:** Starts after handoff gate.

**Goal:** 开始把本地参考内容替换成可公开演示内容。

**Tasks:**

1. Build text-safe slice: project-owned names, UI text, pilot labels, weapon labels and mission-visible copy.
2. Build art-safe slice for one small mission: terrain materials, mech silhouettes, weapon effects, icons and audio cues.
3. Track provenance per asset.
4. Keep reference-linked development pack private.
5. Validate that clean pack runs through the same build/smoke path.

**Exit Gate:**

- Pack manifest is not `ReferenceLinks`.
- Pack notes do not say local-reference-only/private/development-only.
- Public boundary check passes.
- One mission can be captured without private reference art.

### Milestone 8: Mission And Map Authoring

**Status:** Deferred until first demo is stable.

**Goal:** 让官方、合作方或社区能做地图，但奖励必须由主服务器认证。

**Tasks:**

1. Define map package schema.
2. Define objective, trigger, enemy wave, prop, terrain and reward-table references.
3. Build local map validator.
4. Build lightweight editor/export path.
5. Add uncertified public maps with no portable rewards.
6. Add certified maps only after main-server validation exists.

**Exit Gate:**

- A new map package can be loaded locally.
- Main server, not map server, owns portable rewards.

### Milestone 9: Platform And Web

**Status:** Deferred.

**Goal:** 支持地图服务器、奖励认证、排行和创作者生态。

**Tasks:**

1. Main server prototype: account id, inventory snapshot, token ledger.
2. Signed squad loadout.
3. Reward claim endpoint.
4. Map registry and certification state.
5. Basic leaderboard and player profile.
6. Web ranking page for maps, squads and seasonal clears.
7. Anti-abuse: per-map cap, daily cap, replay digest, anomaly detection.

**Exit Gate:**

- Map output is a claim, not a grant.
- Main server can reject reward claims.
- Web can display verified results.

### Milestone 10: Creator Economy And Optional Chain Layer

**Status:** Deferred until real economy rules are stable.

**Goal:** 支持地图贡献、皮肤、自定义和收益分配，但不让链上系统绑死核心战斗。

**Tasks:**

1. Define creator revenue model in centralized ledger first.
2. Add map contribution stats.
3. Add skin or cosmetic ownership later.
4. Consider Ethereum/L2 only for proof, settlement transparency or commemorative cosmetics.
5. Keep mech stats, weapon stats, repair costs, battle outcomes and normal token ledger off-chain early.

**Exit Gate:**

- Fraud, refund, moderation and rollback rules are clear.
- Chain integration is optional, not required for core gameplay.

## 8. Current Commit Queue

The live queue is now `docs-ai-rts-commander-v1-detailed-execution-plan-2026-06-07.md`. The older `docs-playable-demo-current-execution-plan-2026-06-07.md` remains useful for completed-task context and evidence history.

Current recommended queue:

| Order | Status | Commit | Purpose |
| --- | --- | --- | --- |
| 1 | Done | `Polish MechLab grid feel` | Make equipment-grid fitting physical, short and readable |
| 2 | Done | `Prove loadout battle effects` | Prove mounted weapons, armor and cooling alter BattleCore |
| 3 | Done | `Polish weapon and damage readability` | Strengthen weapon families and section damage story |
| 4 | Next | `Guard sparse battle UI regression` | Keep battle UI clean while preserving command controls |
| 5 | Next | `Prepare public art-safe mission slice` | Move from text-safe metadata toward public visual replacement |
| 6 | Later | `Guard AI deputy regression` | Keep AI optional, high-level and offline-safe |
| 7 | Later | `Document platform reward contracts` | Define main-server reward authority and map-server limits |
| 8 | Later | `Plan map authoring prototype` | Prepare map package/editor validation rules |
| 9 | Later | `Plan web ranking prototype` | Prepare ranking, player profile and battle-record pages |
| 10 | Later | `Plan creator economy boundary` | Keep creator economy and chain layer late and optional |

## 9. Detailed Near-Term Tasks

The historical task notes below are kept for context and evidence. For the current executable breakdown, use `docs-ai-rts-commander-v1-detailed-execution-plan-2026-06-07.md`, starting with Task 1 `Polish MechLab Grid Feel`.

### Task P2: Add Public Content Boundary Check

**Status:** Completed 2026-06-07.

**Result:** Added `scripts/content-pack/check_public_content_boundary.ps1`. The check is read-only, scans names and text-like files, prints rule/path/line findings, returns `0` for clean input and returns `1` when private reference markers are found. `content-packs/project-owned-starter.example.json` returns OK. The current development build returns expected findings, including dev build identity, `mc2_01` command/mission ids, local absolute paths, extraction folders, reference-linked pack traces and legacy unit markers.

**Goal:** 发布或对外演示前能检查 build path 没混入私有参考素材、参考路径、旧名称或本地提取痕迹。

**Files:**

- Create: `scripts/content-pack/check_public_content_boundary.ps1`
- Modify: `README.md`
- Modify: `docs-content-replacement-plan.md`
- Modify: `docs-playable-demo-current-execution-plan-2026-06-07.md`

**Step 1: Define rules**

Add pattern groups for:

- private reference pack paths;
- local extraction folders;
- reference-linked manifest markers;
- legacy/proprietary names;
- development-only notes;
- known public-forbidden text markers.

**Step 2: Implement scan**

The script should:

- accept `-Path`;
- support `-DryRun`;
- scan file and directory names;
- scan text-like files only for content markers;
- print rule name, relative path and line number when possible;
- never delete, move or modify files;
- return `1` when forbidden content is found.

**Step 3: Document command**

Add the command to README and content replacement docs:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\content-pack\check_public_content_boundary.ps1 -Path "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows" -DryRun
```

**Step 4: Validate**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\content-pack\check_public_content_boundary.ps1 -Path "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows" -DryRun
```

Expected:

- Clean public build returns `0`.
- Development build with reference markers returns `1` and explains findings.
- No files are changed.

**Commit:** `Add public content boundary check`

### Task H4: Run Demo Handoff Gate Audit

**Status:** Next after P2.

**Goal:** 把“能跑、能看、能讲、能解释边界”一次性验证出来。

**Files:**

- Modify: `docs-playable-demo-investor-evidence-2026-06-07.md`
- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify if needed: `unity-mc2-demo/README.md`

**Steps:**

1. Run `git diff --check`.
2. Run Unity mission validator.
3. Build Windows demo.
4. Run visible-flow smoke.
5. Capture six presets.
6. Run public boundary check.
7. Update evidence page with exact status.
8. Do not stage generated screenshot/log/build files unless requested.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-handoff.log"
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-handoff.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-handoff-visible-flow.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol,mechlab
```

**Commit:** `Audit playable demo handoff gate`

### Task V4: Polish Crowded Contact Occupancy

**Status:** Next after handoff audit, if screenshots still show overlap.

**Goal:** 修正“堆在一起”的可见问题，同时保留 BattleCore 规则证据。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `scripts/unity/capture_reference_visuals.ps1`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Add or reuse a close-up preset around the crowded contact.
2. Confirm whether overlap is visual scale, spawn spacing, pathing, or missing blocker.
3. Add one BattleCore assertion or sidecar summary before changing presentation.
4. Tune spacing/blocker radius conservatively.
5. Capture before/after.

**Commit:** `Polish crowded contact occupancy`

### Task R1: Open Public Replacement Slice

**Status:** Later, after H4.

**Goal:** 开始做能公开展示的替换内容包，不再只依赖本地参考内容。

**Files:**

- Modify: `docs-content-replacement-plan.md`
- Modify or create: `content-packs/project-owned-dev/*`
- Modify if needed: `scripts/content-pack/new_content_pack.ps1`
- Modify if needed: `scripts/content-pack/validate_content_pack.ps1`

**Steps:**

1. Define one mission's project-owned visible names.
2. Replace visible UI text and labels.
3. Mark any remaining reference visuals as private-only.
4. Record provenance for new assets.
5. Validate pack and run boundary check.

**Commit:** `Open public replacement content slice`

## 10. Validation Bus

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

Visible-flow change:

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-visible-flow-audit.log"
```

Visual change:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol
```

MechLab change:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets mechlab
```

Public package boundary:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\content-pack\check_public_content_boundary.ps1 -Path "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows" -DryRun
```

Known good strings:

- `MC2 demo contract validation OK`
- `Build Finished, Result: Success`
- `MC2 Unity demo Windows build OK`
- `MC2 demo smoke test exiting with code 0`
- `MC2 reference visual captures passed`

## 11. Stop Conditions

Stop and reassess if:

- `git status --short` shows unrelated user/source changes in planned files.
- Unity scene fileID churn appears without intentional scene edits.
- Validator fails on movement, occupancy, damage, objective, repair or loadout behavior.
- `hangar-contact` or `damage-demo` becomes less readable than previous evidence.
- Unit/building/terrain collision exists only in Unity with no BattleCore evidence.
- AI code starts making per-frame, per-shot or per-dodge decisions.
- Battle UI starts showing save slots, account management, debug-only panels or excessive text.
- Public-facing docs pitch the project as a clone instead of AI-assisted tactical RTS exploration.
- Public build path contains private reference assets, paths, old names or development-only manifests.

## 12. Definition Of Done For First Demo

The first Demo is ready for controlled external showing when:

1. `git diff --check` passes.
2. Unity mission validator passes.
3. Windows build passes.
4. Visible-flow smoke exits with code `0`.
5. Reference captures are nonblank and visually readable.
6. `hangar-contact` reads as a tactical contact, not a model pile.
7. `damage-demo` clearly shows a damage story.
8. MechLab grid shows weapon blocks, armor plates, heat sinks, heat, weight and legal/illegal state.
9. Battle UI stays sparse.
10. AI deputy window is optional and offline-safe.
11. Debrief, one-click repair and relaunch work.
12. README and content docs describe project-owned AI RTS exploration.
13. Private reference content is documented as development-only.
14. Public content boundary check exists and is documented.

## 13. One-Line Direction

先把 Windows 本地 Demo 收成能跑、能看、能讲、能解释版权边界的版本；然后继续打磨战斗可读性、物理占位、部位损伤和 MechLab 乐趣；最后再开公开替换包、地图服务器、排行奖励和创作者生态。

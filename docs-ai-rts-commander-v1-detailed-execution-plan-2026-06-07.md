# AI RTS Commander V1 Detailed Execution Plan

> **For Codex:** REQUIRED SUB-SKILL: Use executing-plans to implement this plan task-by-task.

**Goal:** 把当前 Unity 6 Windows 本地原型推进成一版能稳定演示的 AI 副官机甲战术 RTS Demo：机库装配有乐趣，第一张 3D 任务图可读，机甲有物理占位和部位损伤，AI 只做高层建议，公开版本可以逐步替换为项目自有内容包。

**Architecture:** `BattleCore` 继续做唯一规则权威，负责装配、命令、移动、喷射、占位、交火、热量、装甲硬度、部位损伤、战报、维修、奖励草案和 AI observation/directive。Unity 6 Presentation 只做固定镜头、输入、HUD、MechLab、模型、材质、特效、截图、构建和本地 smoke。内容包是可替换边界：开发期可以使用本地参考包验证比例和节奏，公开演示必须切换到项目自有或合规授权内容。

**Tech Stack:** Unity 6, C#, Windows Standalone first, deterministic BattleCore, PowerShell validator/build/smoke/capture scripts, replaceable content packs, optional high-level AI deputy adapter, later main server/map server/Web ranking contracts.

**Revision:** 2026-06-07 v2. This file is now the previous detailed execution plan and remains useful for completed MechLab, loadout, damage and AI task details. The current master execution entrypoint is `docs-ai-rts-commander-current-master-plan-2026-06-07.md`.

---

## 0. How To Use This Plan

当用户说“按计划继续”时，执行顺序是：

1. 先看 `docs-ai-rts-commander-current-master-plan-2026-06-07.md` 的 `## 5. Current Commit Queue`。
2. 如果需要上一版任务细节，再回到本文件 `## 5. Current Commit Queue`。
3. 只取第一个 `Next` 或 `In Progress` 任务。
4. 每次做一个小提交，不跨阶段偷做大功能。
5. 先写或更新 validator/smoke/capture 证据，再做最小实现。
6. BattleCore 规则优先，Unity 只负责表现。
7. 不提交 `analysis-output/`、Unity player build、截图、sidecar JSON 或临时日志，除非用户明确要求打包。
8. Unity 运行后检查 scene fileID churn；没有真实 scene 变更就不要入库。
9. 公开文档只讲本项目自己的 AI RTS 指挥探索，不把本地参考内容当产品身份。

仓库里 `docs` 当前是一个历史说明文件，不是目录；因此本计划按现有项目习惯放在根目录 `docs-*.md`。

## 1. Current State Snapshot

当前阶段是：

```text
Playable Demo Handoff 后的 V1 打磨期
```

已经完成并可复用的能力：

| Area | Current State | Evidence |
| --- | --- | --- |
| Windows local demo | 可 batch validate、build、smoke、capture | `analysis-output/unity-validate-*.log`, `analysis-output/unity-build-*.log` |
| First mission | `mc2_01` 可加载地形、单位、结构、目标、触发、相机 | Unity validator + reference captures |
| Terrain readability | 水面、岸线、跑道、建筑基底、接触区已可读 | `spawn`, `airfield`, `hangar-contact`, `north-patrol` |
| Physical occupancy | 单位、建筑、硬道具、水域、边界有 BattleCore 证据 | `OccupancySummary()`, capture sidecars |
| Crowded contact | V4 已提升单位半径和接触 spread 证据 | `Polish crowded contact occupancy` |
| Sparse battle UI | 战斗 UI 已压缩，不显示大量日志 | `Keep battle UI sparse` |
| Damage story | 断臂、腿瘫、驾驶舱损毁/弹射已有基础 | `damage-demo` |
| MechLab foundation | 整块武器、装甲板、散热器、热量、重量、合法性已有基础 | `mechlab` capture |
| Debrief loop | 战报、一键维修、回机库、再次出战已有基础 | visible-flow smoke |
| AI deputy | compact observation、directive adapter、AI 能力窗口已有基础 | AI validator/smoke |
| Content boundary | README、边界脚本、text-safe slice 已建立 | `check_public_content_boundary.ps1` |

当前最重要缺口：

| Gap | Why It Matters | First Task |
| --- | --- | --- |
| 战斗 UI 稀疏性需要固化成回归门槛 | 已证明损伤故事后，后续改动不能把大日志/杂项 UI 带回战斗 | `Guard sparse battle UI regression` |
| 公开内容只有 text-safe metadata | 投资/公开演示需要 art-safe mission slice | `Prepare art-safe mission slice` |
| 平台路线还只是文档方向 | 后续地图服务器、奖励认证、排行要有契约 | `Document platform reward contracts` |

## 2. Product Scope Lock

### First Demo Must Have

- Windows 本地可玩，不先做安卓、iOS、实时 PVP。
- 一张小任务图，继续使用 `mc2_01` 验证固定视角战斗。
- 1-6 台机甲，常规 4 台；第一台是指挥官机甲。
- 固定俯视战术视角，默认跟随指挥官，允许有限缩放，不做自由旋转。
- 机库装配：机甲、武器、装甲板、散热器、热量、重量、槽位合法性、维修、出战。
- 武器装上即启用，不做启用/关闭开关。
- 战斗指挥：默认全队、状态栏单选、独立命令、自动归队、喷射、移动攻击。
- 喷射按每台机甲当前位置朝目标方向尝试固定距离；非法落点单位保持不动。
- 部位损伤：断臂、腿瘫、驾驶舱损毁/弹射、残骸。
- 装甲板增加整体硬度，不做复杂逐部位装甲账本。
- 战后简报、一键维修、回机库、再次出战。
- AI 副官只做高层建议和未来托管接口，不控制逐帧战斗。
- 物理占位必须有 BattleCore 规则证据，Unity collider 只做表现或审核。

### First Demo Must Not Have

- 实时 PVP。
- 移动端适配。
- 地图服务器、地图编辑器、Web 排行。
- 账号、充值、提现、链上资产。
- 复杂保存槽或保存游戏 UI。
- AI 导演。
- 大模型逐帧移动、开火、回避。
- 公开发布本地参考素材、旧剧情、旧商标、未清权模型/贴图/音频。

## 3. Architecture Boundary

### BattleCore Owns Truth

Primary files:

- `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/UnitState.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutContract.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutValidator.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/MechBayInventoryContract.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/RuleCommander.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/MiniMaxCommander.cs`

Rule:

```text
任何影响移动、命中、伤害、胜负、维修、奖励或 AI 决策的行为，
必须先进入 BattleCore 或 contract 数据。
```

### Unity Owns Visibility

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

### AI Owns High-Level Advice Only

AI may:

- read compact observation;
- draft opening plan;
- choose one high-level directive;
- provide one short advice line;
- support future AI 托管 or paper simulation.

AI must not:

- mutate `BattleMission` directly;
- decide every frame, shot, dodge or exact coordinate;
- block local battle when model API is slow or unavailable;
- spend tokens in validator/smoke tests.

## 4. Validation Bus

Every commit:

```powershell
git diff --check
git status --short --branch --untracked-files=all
```

BattleCore or contract changes:

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-current.log"
```

Unity presentation changes:

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-current.log"
```

Visible-flow smoke:

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-visible-flow-audit.log"
```

Reference captures:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol,mechlab
```

MechLab-only capture:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets mechlab
```

Public content boundary:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\content-pack\check_public_content_boundary.ps1 -Path "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\content-packs\project-owned-text-safe-slice.example.json" -DryRun
```

Known good strings:

- `MC2 demo contract validation OK`
- `Build Finished, Result: Success`
- `MC2 Unity demo Windows build OK`
- `MC2 demo smoke test exiting with code 0`
- `MC2 reference visual captures passed`
- `Result: OK`

## 5. Current Commit Queue

| Order | Status | Commit | Purpose | Primary Gate |
| --- | --- | --- | --- | --- |
| 1 | Done | `Polish MechLab grid feel` | 装配格子更像整块装备放入槽位 | validator + build + `mechlab` capture |
| 2 | Done | `Prove loadout battle effects` | 证明装配影响 BattleCore 战斗 | validator + build + visible-flow smoke |
| 3 | Done | `Polish weapon and damage readability` | 强化武器类型、断臂、腿瘫、弹射故事 | validator + build + `damage-demo`/`hangar-contact` capture |
| 4 | Done | `Guard sparse battle UI regression` | 确保战斗中不显示太多信息 | visible-flow smoke + captures |
| 5 | Next | `Prepare public art-safe mission slice` | 做第一张图的公开替换内容切片计划和入口 | boundary check |
| 6 | Later | `Guard AI deputy regression` | AI 保持高层、可离线、无 token smoke | validator/smoke |
| 7 | Later | `Document platform reward contracts` | 主服务器、地图服务器、奖励认证契约 | docs check |
| 8 | Later | `Plan map authoring prototype` | 地图包、触发、奖励引用和验证器规划 | docs check |
| 9 | Later | `Plan web ranking prototype` | 排行、战绩、地图页和公开资料规划 | docs check |
| 10 | Later | `Plan creator economy boundary` | 创作者分成、皮肤、自定义、链上边界 | docs check |

## 6. Detailed Tasks

### Task 1: Polish MechLab Grid Feel

**Status:** Completed 2026-06-07.

**Result:** Added structured MechLab cell-state evidence at the BattleCore preview boundary. `LoadoutValidationResult` now exposes short status codes such as `OK`, `HEAT!`, `WT!`, `OOB` and `OCC!`; `CombatLoadoutPreview` now exposes per-cell `OPEN`, `OCC`, `OCC!` and `OOB` states. The MechLab UI shows a compact state line, and the `mechlab` capture sidecar reports `CellState=OK OPEN4 OCC12 OCC!0 OOB0` while preserving whole weapon blocks, `A+`/`C+` fillers and `noToggle=yes`.

**Goal:** 让 MechLab 一眼像“把整块武器、装甲板、散热器放进机甲槽位”，并且不再暗示武器能启用/关闭。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutContract.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutValidator.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify: `docs-playable-demo-investor-evidence-2026-06-07.md`

**Step 1: Audit forbidden semantics**

Run:

```powershell
rg -n "enabledWeapons|weapon.*toggle|toggle.*weapon|Enable|Disable" unity-mc2-demo/Assets/Scripts unity-mc2-demo/Assets/Editor
```

Expected:

- No player-facing weapon toggle state.
- Unity lifecycle names such as `OnEnable` are allowed only if unrelated to weapon UI.

**Step 2: Add or strengthen validator expectations**

In `Mc2DemoValidator.cs`, assert:

- all mounted weapons count as active;
- multi-cell weapon summaries include shape or covered cells;
- armor plates and heat sinks remain single-cell fillers;
- legal/illegal messages are short.

Expected validator log:

```text
MC2 demo contract validation OK
```

**Step 3: Make grid cell state explicit**

In `CombatLoadoutPreview.cs` and `LoadoutValidator.cs`, ensure preview exposes:

- occupied cells;
- open cells;
- out-of-bounds placement;
- conflict placement;
- over-heat;
- over-weight.

Keep copy short, for example:

```text
OK
OPEN
OCC
OOB
HEAT!
WT!
```

**Step 4: Render multi-cell equipment as one block**

In `Mc2DemoBootstrap.cs`, adjust MechLab drawing so:

- each weapon block has one label and one continuous visual group;
- cells inside the same weapon do not look like independent toggles;
- armor and heat sink fillers remain simple single-cell elements;
- heat/mass/legal indicators stay visible without large help text.

**Step 5: Validate and capture**

Run:

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-mechlab-grid-feel.log"
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-mechlab-grid-feel.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets mechlab
```

**Acceptance:**

- 玩家一眼看懂装备占哪些格子。
- 没有武器启用/关闭 UI 或文字。
- 热量、重量、槽位是否合法都能短句看懂。
- `mechlab.json` 记录足够的 sidecar 证据。

**Commit:** `Polish MechLab grid feel`

### Task 2: Prove Loadout Changes Battle

**Status:** Completed 2026-06-07.

**Result:** Added a BattleCore-level guard and validator proof that MechLab previews are not static UI. `UnitLoadoutCombatOverride` now records source and mounted weapon counts, `UnitState` exposes those effective combat counts, and invalid preview fits no longer build or apply combat overrides. The validator now proves that the UI preview's mounted weapon blocks match the BattleCore-applied weapon count, that legal armor and heat-sink edits deterministically change armor hardness/cooling in `UnitState`, and that an overlap/invalid fit cannot silently enter battle.

**Goal:** 证明 MechLab 不是静态展示：装上的武器、装甲板、散热器会进入 BattleCore 并影响战斗。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/UnitState.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutValidator.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify if needed: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-visible-flow-audit.txt`

**Step 1: Write validator comparison**

Add a validator scenario that builds two loadouts:

- baseline fit;
- modified fit with at least one changed weapon, armor plate or heat sink.

Assert at least one BattleCore field changes:

- effective range;
- damage;
- cooldown;
- heat budget or cooling;
- armor hardness.

**Step 2: Apply fitted weapons**

Ensure mounted weapons become the unit's active weapons in BattleCore.

Rules:

- mounted weapon means active;
- no per-weapon enable flag;
- invalid fit cannot silently enter battle as valid.

**Step 3: Apply armor and heat sinks**

Keep first version simple:

- armor plates add overall hardness;
- heat sinks improve heat budget/cooling;
- no per-section armor ledger yet.

**Step 4: Validate**

Run:

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-loadout-battle-effect.log"
```

**Acceptance:**

- Validator proves UI displayed loadout equals BattleCore applied loadout.
- One equipment change produces one deterministic combat-stat change.
- Existing battle smoke still passes.

**Validated:** `git diff --check`; `analysis-output/unity-validate-loadout-battle-effect.log`; `analysis-output/unity-build-loadout-battle-effect.log`; `analysis-output/unity-player-loadout-battle-effect.log`.

**Commit:** `Prove loadout battle effects`

### Task 3: Polish Weapon And Damage Readability

**Status:** Completed 2026-06-07.

**Result:** Added a first-class `damageReadability` capture sidecar summary and made `damage-demo` capture fail if weapon-family cues, hit cues, section consequences, sparse HUD evidence and serious damage-story counts are missing. Strengthened BattleCore validator coverage so the section-damage fixture carries real weapon metadata, damaged arms expose reduced combat-event damage, destroyed legs still slow/disable jump, and cockpit destruction records a cockpit hit. Refreshed `damage-demo` and `hangar-contact`; `damage-demo` now proves weapon families, LA/LG/CP loss and sparse status-row readability in one sidecar.

**Goal:** 让机甲战斗比普通 RTS 血条互扣更有卖点：武器类型、命中方向、断臂、腿瘫、驾驶舱弹射在截图尺度上可见。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/UnitState.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify: `docs-playable-demo-investor-evidence-2026-06-07.md`

**Step 1: Define visible weapon families**

Use simple readable buckets:

- laser: thin sustained beam;
- ballistic: fast line/tracer plus impact spark;
- missile: arc/trail plus explosion;
- energy or heavy: short glow/impact pulse.

**Step 2: Guard section damage**

Validator should assert:

- arm destroyed disables or visibly reduces the corresponding weapon capacity;
- leg destroyed slows or immobilizes;
- cockpit destroyed triggers ejection or pilot-loss event according to current rules.

**Step 3: Keep the HUD sparse**

Status rows may show:

- HP;
- worst section;
- detached/solo state;
- repair-ready state.

Do not add a large combat log back into normal battle.

**Step 4: Validate and capture**

Run:

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-damage-readability.log"
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-damage-readability.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets damage-demo,hangar-contact
```

**Acceptance:**

- `damage-demo` 不看日志也能看出一次严重部位损伤故事。
- 特效不遮住单位、地形或状态栏。
- `hangar-contact` 不回退成视觉堆叠。

**Validated:** `git diff --check`; `analysis-output/unity-validate-damage-readability.log`; `analysis-output/unity-build-damage-readability.log`; `capture_reference_visuals.ps1 -Presets damage-demo,hangar-contact`.

**Commit:** `Polish weapon and damage readability`

### Task 4: Guard Sparse Battle UI Regression

**Status:** Completed 2026-06-07.

**Result:** The live current master plan now carries the stricter Task 4 result. Battle screenshots and visible-flow smoke both include `SparseBattleUi=statusRows+sections+solo`, assert Jet/Map/Bay/System controls stay available, and reject visible combat log, save UI, account UI, debug occupancy overlay and forced overlays in the default combat view. `spawn` and `damage-demo` passed the new sidecar gate.

**Goal:** 固化“战斗中不用显示太多信息”的产品决定。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`
- Modify: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-visible-flow-audit.txt`
- Modify: `scripts/unity/capture_reference_visuals.ps1`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Step 1: Add smoke assertions**

Guard that normal battle view does not show:

- save slot UI;
- account/economy UI;
- large combat log;
- debug-only occupancy copy unless review mode is enabled.

**Step 2: Keep essential controls**

Ensure normal battle still shows:

- mech status rows;
- selected/solo state;
- Jet button;
- compact objective/map;
- system/pause entry.

**Step 3: Validate and capture**

Run:

```powershell
git diff --check
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-battle-ui-regression.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,damage-demo
```

**Acceptance:**

- 战斗 UI 不遮挡战场。
- 玩家仍能下达移动、目标、喷射、单机独立命令。
- 状态栏承担损伤概览，不变成大仪表盘。

**Commit:** `Guard sparse battle UI regression`

### Task 5: Prepare Public Art-Safe Mission Slice

**Status:** After first gameplay polish pass.

**Goal:** 从 text-safe metadata 进入第一张图的 art-safe 替换计划，支撑公开展示和投资材料。

**Files:**

- Modify: `docs-content-replacement-plan.md`
- Modify: `docs-content-pack.md`
- Modify: `README.md`
- Modify or create: `content-packs/project-owned-art-safe-slice.example.json`
- Modify if needed: `scripts/content-pack/check_public_content_boundary.ps1`
- Modify if needed: `scripts/content-pack/validate_content_pack.ps1`

**Step 1: Define art-safe scope**

Only one small mission slice:

- product title and visible UI text;
- one terrain material set;
- 3-4 mech silhouettes;
- common weapon FX;
- 3-5 structures/props;
- icons needed by MechLab and battle status.

**Step 2: Record provenance**

Every replacement asset must have:

- source;
- author or generation method;
- license;
- allowed use;
- date;
- notes.

**Step 3: Keep development reference private**

The reference-linked dev pack may remain local, but it must not be described as public-safe.

**Step 4: Validate boundary**

Run:

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\content-pack\check_public_content_boundary.ps1 -Path "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\content-packs\project-owned-text-safe-slice.example.json" -DryRun
```

Expected:

```text
Result: OK
```

**Acceptance:**

- One mission has a clear path from local reference validation to public-safe assets.
- No public-facing doc suggests private reference content is final product content.

**Commit:** `Prepare public art-safe mission slice`

### Task 6: Guard AI Deputy Regression

**Status:** Later, after core battle loop remains stable.

**Goal:** AI 副官保持“小而稳”：做高层计划、风险判断和能力窗口，不拖慢本地战斗。

**Files:**

- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/RuleCommander.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/MiniMaxCommander.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `docs-ai-commander-directive-contract.md`

**Step 1: Keep observation compact**

Observation should include:

- mission phase;
- squad status;
- objective status;
- nearby threat summary;
- heat/damage pressure;
- allowed high-level directives.

Do not include per-frame combat spam.

**Step 2: Keep directives high-level**

Allowed directive shape stays close to:

```text
assault-objective
engage-hostiles
regroup
hold
```

**Step 3: Guard offline path**

No API key or timeout should produce local fallback, not a broken demo.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-deputy-regression.log"
```

**Acceptance:**

- AI is optional.
- No-key local demo works.
- Smoke tests do not spend model tokens.
- AI does not issue exact per-frame movement or shot commands.

**Commit:** `Guard AI deputy regression`

### Task 7: Document Platform Reward Contracts

**Status:** Deferred until local demo is convincing.

**Goal:** 把未来“地图服务器可开放，奖励必须主服务器认证”的架构写成可执行契约。

**Files:**

- Modify: `docs-platform-ecosystem-plan.md`
- Create if needed: `docs-platform-reward-contract-2026-06-07.md`

**Step 1: Define actors**

Actors:

- Unity client;
- main server;
- map server;
- map editor;
- Web ranking;
- optional chain layer.

**Step 2: Define reward claim**

Map output is a claim, not a grant:

```text
account id
signed squad loadout hash
map id + version
session id
deterministic seed
battle summary
timeline digest
```

**Step 3: Define validation ladder**

Ladder:

- local demo;
- trusted official server;
- certified partner server;
- high-value replay validation;
- competitive/prize events.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- Map server cannot mint portable rewards.
- Main server owns inventory, token ledger, reward tables and rankings.
- BattleCore remains reusable for validation.

**Commit:** `Document platform reward contracts`

### Task 8: Plan Map Authoring Prototype

**Status:** Deferred.

**Goal:** 为后续开源地图编辑器和社区地图包设计最小契约。

**Files:**

- Modify: `docs-platform-ecosystem-plan.md`
- Create if needed: `docs-map-authoring-contract-2026-06-07.md`

**Step 1: Define map package schema**

Fields:

- map id/version/title/author/license/provenance;
- terrain and navigation metadata;
- structures, props, turrets, cover, water;
- objectives and trigger graph;
- enemy waves and patrols;
- allowed squad size;
- difficulty estimate;
- reward table reference, not direct reward grant;
- BattleCore compatibility version.

**Step 2: Define certification states**

States:

- Draft;
- Uncertified Public;
- Certified;
- Event;
- Retired.

**Step 3: Define local validation**

Validator should catch:

- missing spawn;
- unreachable objective;
- invalid blockers;
- reward table id not found;
- forbidden content markers;
- incompatible BattleCore version.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- New maps can be open and editable.
- Portable rewards remain server-certified.

**Commit:** `Plan map authoring prototype`

### Task 9: Plan Web Ranking Prototype

**Status:** Deferred.

**Goal:** 给未来 Web 展示排行、地图成绩、队伍资料和战斗记录留契约。

**Files:**

- Create if needed: `docs-web-ranking-plan-2026-06-07.md`
- Modify: `docs-platform-ecosystem-plan.md`

**Step 1: Define public pages**

Pages:

- season leaderboard;
- map ranking;
- player public profile;
- squad loadout snapshot;
- battle record detail;
- creator/map author profile.

**Step 2: Define shown data**

Show only verified data:

- certified map id/version;
- clear result;
- time/damage/loss summary;
- squad hash and public unit names;
- reward summary;
- replay availability flag.

**Step 3: Define privacy boundary**

Do not expose:

- private account identifiers;
- raw API keys;
- unpublished inventory;
- exact anti-cheat internals.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- Web plan supports ranking and investment story.
- It does not force server work before local demo quality.

**Commit:** `Plan web ranking prototype`

### Task 10: Plan Creator Economy Boundary

**Status:** Deferred.

**Goal:** 将地图贡献、皮肤、自定义、收入分配和可选链上实验放在正确阶段，避免过早绑死核心游戏。

**Files:**

- Modify: `docs-platform-ecosystem-plan.md`
- Create if needed: `docs-creator-economy-boundary-2026-06-07.md`

**Step 1: Define centralized ledger first**

Keep early economy in main server:

- token ledger;
- inventory ledger;
- creator revenue accounting;
- refund/rollback/moderation controls.

**Step 2: Define creator contribution**

Creators can contribute:

- maps;
- skins;
- event campaigns;
- hosted map server capacity;
- curated challenge ladders.

**Step 3: Define optional chain layer**

Use Ethereum/L2 later only for:

- proof of revenue share;
- transparent creator pools;
- cosmetic ownership proof;
- commemorative items.

Do not put:

- core combat;
- mech stats;
- weapon stats;
- repair costs;
- normal inventory mutation;
- ordinary battle outcomes

on chain in first platform version.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- Chain is optional and late.
- Core gameplay remains deterministic and locally testable.

**Commit:** `Plan creator economy boundary`

## 7. First Controlled Demo Definition Of Done

Demo reaches controlled external-showing quality when:

1. `git diff --check` passes.
2. Unity mission validator passes.
3. Windows build passes.
4. Visible-flow smoke exits with code `0`.
5. `mechlab`, `spawn`, `airfield`, `hangar-contact`, `damage-demo`, `north-patrol` captures are nonblank.
6. `hangar-contact` reads as a tactical contact, not a model pile.
7. `damage-demo` clearly shows a damage story.
8. MechLab grid shows weapon blocks, armor plates, heat sinks, heat, weight and legal/illegal state.
9. Battle UI stays sparse.
10. AI deputy window is optional and offline-safe.
11. Debrief, one-click repair and relaunch work.
12. README describes AI-assisted tactical RTS exploration, not an old-game clone.
13. Current dev reference build is clearly marked development-only.
14. At least one clean public text-safe slice passes public boundary check.
15. Art-safe mission slice has a written path and provenance requirements.

## 8. Stop Conditions

Stop and reassess if:

- `git status --short` shows unrelated user/source changes in files planned for editing.
- Unity scene fileID churn appears without intentional scene changes.
- Validator fails on movement, occupancy, damage, objective, repair, loadout or AI behavior.
- `hangar-contact`, `damage-demo` or `mechlab` captures become less readable than current evidence.
- Unit, building, prop or water collision exists only in Unity with no BattleCore evidence.
- AI code starts making per-frame, per-shot or per-dodge decisions.
- Normal battle UI shows save slots, account management, debug panels or too much text.
- Public-facing docs pitch the project as a clone instead of AI-assisted tactical RTS exploration.
- Public build path contains private reference assets, local paths, old names or development-only manifests.

## 9. One-Line Direction

现在的主线是：先把 MechLab 装配乐趣和装配影响战斗打透，再强化部位损伤和武器表现，保持 AI 副官小而稳，随后用公开替换包、平台契约、地图服务器和 Web 排行为更大的世界铺路。

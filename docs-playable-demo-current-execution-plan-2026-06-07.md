# Playable Demo Current Execution Implementation Plan

> **For Codex:** REQUIRED SUB-SKILL: Use executing-plans to implement this plan task-by-task.

**Goal:** 把当前 Unity 6 Windows 本地原型收成一版能演示、能截图、能讲清楚价值的轻量机甲战术指挥 Demo：机库装配直观，战场可读，单位有物理占位证据，AI 副官只做高层建议，本地断网也能完整跑通。

**Architecture:** `BattleCore` 是权威规则层，负责任务、命令、移动、喷射落点、占位、武器、热量、装甲硬度、部位损伤、维修、结算和 AI observation/directive。Unity Presentation 只负责输入、固定镜头、HUD、模型、材质、特效、截图和调试可视化。开发期可以用本地私有参考内容包验证比例、节奏和可读性，公开构建必须能切到项目自有或合规授权内容包。

**Tech Stack:** Unity 6, C#, Windows Standalone, deterministic BattleCore, PowerShell validation/capture scripts, `mc2-unity-demo-contract-v1`, private local reference content pack, Git/GitHub.

**Revision:** 2026-06-07 current execution plan v8. This is the current canonical day-to-day execution queue. The fixed overall plan is `docs-ai-rts-commander-overall-implementation-plan-2026-06-07.md`; the fine-grained roadmap is `docs-ai-rts-commander-detailed-roadmap-2026-06-07.md`; `docs-playable-demo-v1-detailed-plan-2026-06-07.md` and older `docs-playable-demo-*.md` files remain history, evidence, or task archives.

---

## 0. Execution Rule

当用户说“按计划继续”时，从本文件 `## 5. Current Commit Queue` 的第一个 `Next` 或 `In Progress` 任务开始。

每个小提交只做一个可验证增量：

1. 写或更新最小断言。
2. 实现最小改动。
3. 跑对应 validator/build/smoke/capture。
4. 更新证据文档。
5. 检查 worktree，避免 Unity scene fileID churn 和生成物入库。
6. 提交。

当前真实起点：

- Branch: `master...ai-origin/master` with local demo commits ahead of remote.
- AI compact observation 已提交：`af7dbe9 Freeze AI observation contract`.
- AI directive adapter 已提交：`9bf26bd Guard AI directive adapter`.
- AI advice window 已提交：`b40372d Show optional AI advice window`.
- Battle occupancy readability re-audit 已提交：`87006c3 Re-audit battle occupancy readability`.
- Visual readability 已提交：`527a6be Improve reference visual readability`.
- Occupancy placeholder review layer 已提交：`1bd22e2 Lock occupancy placeholder review layer`.
- MechLab block fitting 已提交：`74e24bf Polish MechLab block fitting`.
- MechLab fitting evidence capture 已提交：`6ffa2ea Capture MechLab fitting evidence`.
- Damage readability 已提交：`295268f Strengthen damage demo readability`.
- Sparse battle UI 已提交：`a0d8750 Keep battle UI sparse`.
- Walkthrough 已提交：`85bb0ea Write playable demo walkthrough`.
- Repeatable Windows build 已提交：`3753857 Prepare repeatable Windows demo build`.
- Demo evidence package 已提交：`0bb822b Package playable demo evidence`.
- Content boundary documentation 已提交：`4819657 Document reference content boundary`.
- V4 crowded contact occupancy 已验证，下一步是 `Refresh playable demo evidence`.
- V4 evidence: `analysis-output/unity-validate-crowded-contact.log`, `analysis-output/unity-build-crowded-contact.log`, refreshed `hangar-contact` and `damage-demo` captures with `ContactSpread`.

## 1. First Demo Product Scope

第一版只做 Windows 本地可玩 Demo。核心是两个画面：

1. 机库装配：机甲、武器、装甲板、散热器、热量、载重、合法性、维修、出战。
2. 地图战斗：固定俯视视角、小队命令、独立命令、喷射、自动交火、部位损伤、战后结算。

必须保留：

- 1-6 台机甲，常规 4 台。
- 默认全队控制。
- 状态栏点单台机甲，再点地点或目标，该机甲进入独立命令。
- 独立命令完成后自动归队，并接受最新全队命令。
- 喷射从每台机甲当前位置沿目标方向尝试固定距离位移；非法落点单位不动，合法单位位移。
- 固定俯视镜头默认跟随排序第一位指挥官机甲，允许有限缩放，不做自由旋转。
- 战斗中 UI 尽量少：状态栏、喷射、任务地图、暂停/系统、必要目标状态。
- 武器装上即启用，不做启用/关闭。
- 装甲板增加整体硬度，不引入复杂逐部位装甲账本。
- 部位损伤、断臂、腿瘫、驾驶舱弹射必须可见。
- 物理占位必须有 BattleCore 规则证据；Unity 碰撞体和可视化占位只做表现和审核辅助。

第一版明确不做：

- 实时 PVP。
- 移动端适配。
- 地图服务器。
- 账号经济、充值、提现、链上资产。
- 复杂保存系统或保存槽 UI。
- AI 导演。
- 大模型逐帧控制战斗。
- 公开发布私有参考素材、旧作剧情、旧作专有名称、旧作商标或旧作文案。

## 2. Current Completion Snapshot

| Area | Current State | Evidence |
| --- | --- | --- |
| Unity Windows build | 可构建，可 smoke | `analysis-output/unity-build-*.log` |
| `mc2_01` first map | 可加载地形、单位、结构、目标、触发、相机信息 | validator + capture sidecars |
| Visible flow | 覆盖机库、出战、独立命令、喷射、集火、战报、回机库、再启动 identity | `mc2_01-visible-flow-audit.txt` |
| Battle occupancy | 单位、建筑、硬 terrain object、水域/边界占位已有 BattleCore evidence | `OccupancySummary()` + sidecars |
| HUD | 已压缩，战斗中信息不过量 | reference captures |
| Combat cues | 武器方向、命中、爆炸、残骸、断臂、腿瘫、弹射已有基础 | validator + `damage-demo` |
| Armor hardness | 装甲板走整体硬度，部位损伤仍保留 | validator |
| MechLab | 整块武器格子、装甲板、散热器、热量、重量、合法性已有基础 | loadout validator + smoke |
| Debrief loop | 战报、维修、回机库、再出战基础完成，不暴露保存槽概念 | repair/relaunch validator + smoke |
| AI observation | Compact schema and prompt budget locked | `analysis-output/unity-validate-ai-observation.log` |
| AI directive | High-level directive adapter guarded, missing key falls back locally | `analysis-output/unity-validate-ai-directive.log` |
| AI deputy window | System panel 已显示 compact AI Deputy / AI副官状态、模式、意图和短建议 | `analysis-output/unity-player-ai-advice-window.log` |
| Content boundary | 有内容包文档和替换包脚本方向 | README + content-pack docs |

当前主要缺口：

| Gap | Why It Matters | Next Task |
| --- | --- | --- |
| MechLab 后续只需回归 | 整块占格和截图证据已完成，后续 UI 改动需要保持这个体验不退化 | G3 regression |
| Demo handoff 已审计 | H4 已验证 build、smoke、capture 和 boundary；V4 已补充 contact-spread evidence | Refresh playable demo evidence |
| 公开内容安全还需要干净替换包 | 当前开发 build 会被边界检查正确标为不适合公开发布 | R1 replacement slice later |

## 3. Architecture Contracts

### 3.1 BattleCore Owns Gameplay Truth

BattleCore owns:

- mission contract loading;
- trigger and objective state;
- squad command acceptance;
- detached single-unit command state;
- auto rejoin;
- jet landing legality;
- unit, structure, hard terrain object, water and map-bound occupancy;
- weapon range, damage, cooldown, heat and armor hardness;
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

Rule: if a behavior affects movement, damage, victory, loss, repair or AI decision, it must exist in BattleCore or contract data first.

### 3.2 Unity Presentation Owns Visibility

Unity Presentation owns:

- click/raycast input;
- fixed tactical camera and limited zoom;
- HUD and MechLab layout;
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

### 3.3 AI Boundary

AI can:

- read compact observation;
- draft an opening plan;
- choose one high-level directive for a phase;
- show a short advice line in UI;
- support future AI托管 and paper-resolution calculations.

AI cannot:

- mutate `BattleMission` directly;
- decide every shot or every frame;
- choose exact player-facing coordinates;
- block local battle when model API is slow or unavailable;
- become required for smoke tests.

### 3.4 Content Boundary

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

## 4. Validation Bus

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

## 5. Current Commit Queue

| Order | Status | Commit | Gate |
| --- | --- | --- | --- |
| 0 | Done | `Freeze AI observation contract` | G5 AI capability |
| 1 | Done | `Guard AI directive adapter` | G5 AI capability |
| 2 | Done | `Show optional AI advice window` | G5 AI capability |
| 3 | Done | `Re-audit battle occupancy readability` | G2/G7 readability and occupancy |
| 4 | Done | `Improve reference visual readability` | G2 battle readability |
| 5 | Done | `Lock occupancy placeholder review layer` | G7 collision evidence |
| 6 | Done | `Polish MechLab block fitting` | G3 MechLab feel |
| 7 | Done | `Capture MechLab fitting evidence` | G3 MechLab evidence |
| 8 | Done | `Strengthen damage demo readability` | G1 combat feel |
| 9 | Done | `Keep battle UI sparse` | G1/G2 UI readability |
| 10 | Done | `Write playable demo walkthrough` | G8 handoff |
| 11 | Done | `Prepare repeatable Windows demo build` | G8 handoff |
| 12 | Done | `Package playable demo evidence` | G8 handoff |
| 13 | Done | `Document reference content boundary` | G6 public boundary |
| 14 | Done | `Add public content boundary check` | G6 public boundary |
| 15 | Done | `Run demo handoff gate audit` | G8 handoff |
| 16 | Done | `Polish crowded contact occupancy` | G2/G7 readability and occupancy |
| 17 | Next | `Refresh playable demo evidence` | G8 handoff after V4 |
| 18 | Next | `Open public replacement content slice` | G6 public boundary |

## 6. Detailed Tasks

### D3: Show Optional AI Advice Window

**Status:** Completed 2026-06-07.

**Result:** System panel now includes a compact AI Deputy / AI副官 window that reports state, mode, intent and one short advice sentence. The state builder reads model configuration and local BattleCore observation only; drawing the window does not call the model. `assert-ai-deputy-window` is now part of the command-file smoke path and proves the missing-key offline fallback without spending tokens. Evidence: `analysis-output/unity-validate-ai-advice-window.log`, `analysis-output/unity-build-ai-advice-window.log`, `analysis-output/unity-player-ai-advice-window.log`.

**Goal:** 在第一版 UI 中显示一个小型 AI 副官能力窗口，只给高层建议；没有 API key、模型超时或离线时，本地 Demo 完整可玩。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/MiniMaxCommander.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-visible-flow-audit.txt`
- Modify: `docs-ai-commander-directive-contract.md`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Step 1: Inspect current System panel and command smoke parser**

Read:

```powershell
rg -n "RunStartupMiniMaxCommander|DrawSystemPanel|SystemPanelRect|enum ActionType|TryParseLine" unity-mc2-demo/Assets/Scripts/Presentation
```

Expected:

- `DrawSystemPanel()` is the safest first place for the optional AI window.
- Active battle HUD should stay sparse; no always-open chat console.

**Step 2: Add a small AI deputy state builder**

In `Mc2DemoBootstrap.cs`, add a private state model similar to:

```csharp
private sealed class AiDeputyWindowState
{
    public string State = "Offline";
    public string Mode = "Local fallback";
    public string Directive = "assault-objective";
    public string Advice = "Advance objective";
}
```

Build it from:

- `MiniMaxCommander.ConfigFromEnvironment().IsConfigured`;
- current `BattleMission` observation through a temporary `CommanderObservationPort`;
- `RuleCommander` fallback command interpretation.

Do not make network calls while drawing UI.

**Step 3: Draw a compact subsection in System panel**

Show only:

- `AI Deputy / AI副官`;
- state: `Offline` or `Ready`;
- intent: one legal directive token;
- advice: one short sentence.

Move existing System buttons down if the panel needs more height. Do not add a large chat box.

**Step 4: Add smoke assertion**

In `StartupCommanderScript.cs`, add:

```text
assert-ai-deputy-window
```

Acceptance for this command:

- state is `Offline` or `Ready`;
- directive is one of `assault-objective`, `engage-hostiles`, `regroup`, `hold`;
- missing key path reports local fallback;
- assertion does not spend tokens.

**Step 5: Add validator parser coverage**

In `Mc2DemoValidator.cs`, assert:

- `assert-ai-deputy-window` parses with no payload;
- malformed payload is rejected;
- the default offline state is valid.

**Step 6: Validate**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-advice-window.log"
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-ai-advice-window.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-ai-advice-window.log"
```

**Acceptance:**

- AI reads as optional deputy, not required gameplay.
- No API key path is clear and playable.
- UI remains clean in battle.
- Smoke proves the offline window state without model calls.

**Commit:** `Show optional AI advice window`

### V1: Re-Audit Battle Occupancy Readability

**Status:** Completed 2026-06-07.

**Result:** Refreshed all five reference captures and classified the current readability failure. BattleCore occupancy and placeholder evidence are present (`hardProps=80`, `placeholders=81`), so the top problem is not missing collision. The single weakest screenshot is `damage-demo`, where 20 active / 20 visible hostiles and damage FX compress around the hangar objective. Evidence is recorded in `docs-reference-visual-audit-2026-06-07.md` under `V1 Battle Occupancy Readability Re-Audit Result`.

**Goal:** 先用截图和 sidecar 判断当前“堆在一起”到底是碰撞缺失、镜头压缩、模型比例、敌人密度、遮挡还是特效噪音。

**Files:**

- Read: `analysis-output/reference-visual-captures/hangar-contact.png`
- Read: `analysis-output/reference-visual-captures/hangar-contact.json`
- Read: `analysis-output/reference-visual-captures/damage-demo.png`
- Read: `analysis-output/reference-visual-captures/damage-demo.json`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Capture `spawn,airfield,hangar-contact,damage-demo,north-patrol`.
2. Compare screenshot with `BattleOccupancy`, `OccupancyPlaceholders`, `referenceAssets.scale`, `OcclusionFade`, active/visible hostile counts.
3. Classify the top failure:
   - physical overlap;
   - camera compression;
   - UI occlusion;
   - model scale;
   - dark/flat material;
   - too much FX;
   - enemy density.
4. Write one paragraph per key preset.
5. Do not change gameplay in this task.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol
git diff --check
```

**Acceptance:**

- The audit doc names the single weakest screenshot.
- The next visual/occupancy fix is evidence-driven.

**Commit:** `Re-audit battle occupancy readability`

### V2: Improve Reference Visual Readability

**Status:** Completed 2026-06-07.

**Result:** Strengthened the player damage spotlight in `DemoUnitView` and `Mc2DemoBootstrap`: damaged player mechs now get larger ground rings, taller beacons, and a compact world-space flag for screenshot readability. The pass did not change mission pressure, enemy counts, camera, BattleCore occupancy, movement, landing legality, or combat values. Evidence: `analysis-output/unity-validate-visual-readability.log`, `analysis-output/unity-build-visual-readability.log`, and refreshed `analysis-output/reference-visual-captures/*.png` sidecars.

**Goal:** 让第一张图更像 3D 战场，不像色块和模型团；优先改材质、比例、构图和表现密度，不随意改任务难度。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoTerrainView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `scripts/content-pack/export_reference_visual_pack.ps1`
- Modify if needed: `scripts/content-pack/export_tgl_to_obj.py`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Fix material contrast before adding new objects.
2. Keep palette varied and readable; avoid returning to dark monotone terrain.
3. Check mech/vehicle/infantry/prop scale against sidecar summaries.
4. If fight still reads as one knot, adjust deterministic parking/attack slots, not enemy count first.
5. Keep all movement/landing legality backed by BattleCore.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-visual-readability.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol
```

**Acceptance:**

- Five reference captures remain nonblank and readable.
- `hangar-contact` does not look like every actor shares one coordinate.
- `damage-demo` still shows at least one damage cue.

**Commit:** `Improve reference visual readability`

### V3: Lock Occupancy Placeholder Review Layer

**Status:** Completed 2026-06-07.

**Result:** The occupancy review layer now exposes active unit radii, blocking structures, hard terrain objects, and sampled blocked landing markers. Sidecars report `units`, `playerUnits`, `hostileUnits`, `structures`, `hardProps`, and `landingBlockedMarkers`, while `-NoOccupancyPlaceholders` proves the normal capture path can remain clean. Evidence: `analysis-output/unity-validate-occupancy-placeholder.log`, `analysis-output/unity-build-occupancy-placeholder.log`, `analysis-output/reference-visual-captures/hangar-contact.json`, `analysis-output/reference-visual-captures/damage-demo.json`, and `analysis-output/reference-visual-captures-no-placeholders/hangar-contact.json`.

**Goal:** 给开发审核一个可开关的物理占位层，能看见单位半径、建筑 blockers、hard props 和喷射非法区，但正常玩家 HUD 不显示调试层。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoTerrainView.cs`
- Modify: `scripts/unity/capture_reference_visuals.ps1`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Confirm current placeholder source is BattleCore, not Unity-only collider state.
2. Add missing blocker categories only when `BattleMission` reports them.
3. Add or refine capture preset flag for placeholders.
4. Sidecar must report enabled/disabled and blocker counts.

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets hangar-contact,damage-demo
```

**Acceptance:**

- Placeholder layer makes blockers auditable.
- Normal player HUD stays clean.
- Jet and movement legality still come from BattleCore.

**Commit:** `Lock occupancy placeholder review layer`

### M1: Polish MechLab Block Fitting

**Status:** Completed 2026-06-07.

**Result:** MechLab preview now carries explicit block summaries for mounted weapon blocks and single-cell armor/sink fillers. Multi-cell weapon blocks show shape labels such as `2x1`, smoke proves all source weapons remain mounted (`alwaysMounted=weapons 8/8 items 8/8 noToggle=yes`), and the player-facing fallback text now asks for a weapon block rather than implying a weapon enable/disable toggle. Evidence: `analysis-output/unity-validate-mechlab-blocks.log`, `analysis-output/unity-build-mechlab-blocks.log`, and `analysis-output/unity-player-mechlab-blocks.log`.

**Goal:** 装配界面继续靠近“整块武器放进格子”的乐趣，而不是表格编辑；武器装上即启用。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutValidator.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-loadout-compact.txt`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Search and reject player-facing weapon toggle language.
2. Mounted weapon renders as one contiguous block.
3. Multi-cell weapons show internal dividers and shape label.
4. Armor plates and heat sinks show as single-cell fillers.
5. Invalid, overweight and overheated states are short and immediate.
6. Smoke asserts no weapon toggle copy and grid block language exists.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-mechlab-blocks.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-loadout-compact.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-mechlab-blocks.log"
```

**Acceptance:**

- 机库截图能让人一眼看出“整块武器占格”。
- Armor/sink filler 不抢武器视觉。
- 装上武器即启用的设定不退化。

**Commit:** `Polish MechLab block fitting`

### M2: Capture MechLab Fitting Evidence

**Status:** Completed 2026-06-07.

**Result:** `scripts/unity/capture_reference_visuals.ps1 -Presets mechlab` now produces a first-class MechLab screenshot and sidecar evidence. The Unity capture preset opens the MechLab, selects the default mech, ensures the capture draft visibly contains weapon block shape, one armor filler and one heat-sink filler, and records `MechLabCapture=open ... fillers=A+/C+ ... pressure=H ... W ... noToggle=yes`. Evidence: `analysis-output/reference-visual-captures/mechlab.png`, `analysis-output/reference-visual-captures/mechlab.json`, and `analysis-output/reference-visual-captures/mechlab.log`.

**Goal:** 留一张能给人看的装配界面截图，而不是只靠 smoke 文本。

**Files:**

- Modify: `scripts/unity/capture_reference_visuals.ps1`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Add or reuse a MechLab capture preset.
2. Select the default mech.
3. Expose weapon block, armor filler, heat sink filler, weight and heat.
4. Record screenshot path and judgment in the audit doc.

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets mechlab
```

**Acceptance:**

- MechLab screenshot becomes first-class evidence.
- The screenshot does not imply weapon enable/disable toggles.

**Commit:** `Capture MechLab fitting evidence`

### C1: Strengthen Damage Demo Readability

**Status:** Completed 2026-06-07.

**Result:** `damage-demo` now frames the damaged squad closer, reports a concise top-bar story, and writes a `damageStory` sidecar summary proving `left-arm-lost`, `legs-lost`, and `cockpit-lost` in the player squad. The capture script now rejects a damage-demo preset that is too zoomed out or missing the forced section-damage story. Evidence: `analysis-output/unity-validate-damage-selling-moment.log`, `analysis-output/unity-build-damage-selling-moment.log`, `analysis-output/reference-visual-captures/damage-demo.png`, `analysis-output/reference-visual-captures/damage-demo.json`, and `analysis-output/reference-visual-captures/damage-demo.log`.

**Goal:** `damage-demo` 必须一眼看出这是机甲部位损伤战斗，而不是普通 RTS 小兵血条。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Keep BattleCore section state as truth.
2. Pick one unmistakable cue for the first pass:
   - arm loss marker;
   - leg mobility loss marker;
   - cockpit ejection trail;
   - wreck silhouette.
3. Keep cue short and readable at tactical zoom.
4. Status row must confirm the same section event.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-damage-selling-moment.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets damage-demo
```

**Acceptance:**

- `damage-demo` screenshot has one clear mech damage story.
- HUD remains light.
- Combat rules remain deterministic.

**Commit:** `Strengthen damage demo readability`

### C2: Keep Battle UI Sparse

**Status:** Completed 2026-06-07.

**Result:** The right-side battle panel is now a compact `Battle / 战斗` state card with squad/hostile counts and one tactical pulse line. The normal battle view no longer draws the rolling combat log, while the underlying log remains available for smoke/debug output. Capture sidecars now include `battleHud`, and the capture script rejects battle screenshots that re-enable visible combat logs, save UI, or an oversized combat panel. Evidence: `analysis-output/unity-validate-battle-ui-sparse.log`, `analysis-output/unity-build-battle-ui-sparse.log`, `analysis-output/unity-player-battle-ui-sparse.log`, `analysis-output/reference-visual-captures/spawn.png`, `analysis-output/reference-visual-captures/spawn.json`, `analysis-output/reference-visual-captures/damage-demo.png`, and `analysis-output/reference-visual-captures/damage-demo.json`.

**Goal:** 战斗中不用显示太多信息，但状态栏必须承担控制和损伤概览。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-visible-flow-audit.txt`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Audit active-battle text and remove nonessential surfaces.
2. Keep status rows, selected unit state, jet button, compact objective/map, pause/system.
3. Guard that save/account/debug-only copy is not in normal active battle.
4. Capture `spawn` and `damage-demo`.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-battle-ui-sparse.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,damage-demo
```

**Acceptance:**

- Battle UI does not cover core combat.
- Status rows still show health, damaged section, detached state and ready/repair state.

**Commit:** `Keep battle UI sparse`

### H1: Write Playable Demo Walkthrough

**Status:** Completed 2026-06-07.

**Result:** Added `docs-playable-demo-walkthrough-2026-06-07.md`, a three-minute talk track and operator checklist for showing MechLab fitting, sparse battle command, all-squad orders, status-row solo orders, Jet, section damage, debrief, repair and relaunch. README now lists the walkthrough as a key document. The wording stays on project-owned positioning: AI-assisted tactical RTS, deterministic BattleCore, optional AI deputy and replaceable content packs.

**Goal:** 给协作者或投资人一个三分钟脚本，照着走就能理解当前 Demo 价值。

**Files:**

- Create or modify: `docs-playable-demo-walkthrough-2026-06-07.md`
- Modify: `README.md`

**Steps:**

1. Script: start, MechLab, inspect fit, launch, squad move, focus target, solo command, jet, damage/ejection, debrief, repair, relaunch.
2. Use project-owned wording:
   - AI-assisted tactical RTS;
   - mech squad command;
   - deterministic BattleCore;
   - optional AI deputy;
   - replaceable content packs.
3. Do not market old franchise names or private reference content.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- A new person can follow the demo without knowing the repo.
- The script matches current UI labels.

**Commit:** `Write playable demo walkthrough`

### H2: Prepare Repeatable Windows Demo Build

**Status:** Completed 2026-06-07.

**Result:** `BUILD-WIN.md` now starts with the current Unity 6 Windows Demo handoff path: validator, Windows build, visible-flow smoke, reference capture command, expected success strings, ignored evidence paths, and private-reference-content boundary. `unity-mc2-demo/README.md` now points to that checklist and states where generated logs/player builds live. Verified with `analysis-output/unity-validate-demo-package.log`, `analysis-output/unity-build-demo-package.log`, and `analysis-output/unity-player-demo-package.log`.

**Goal:** 形成可重复构建、可 smoke、可发截图证据的本地 Windows Demo 包。

**Files:**

- Modify: `BUILD-WIN.md`
- Modify: `unity-mc2-demo/README.md`
- Modify if needed: `scripts/content-pack/start_runtime_shell.ps1`
- Modify if needed: `scripts/content-pack/install_dev_shortcut.ps1`

**Steps:**

1. Document the exact Unity batch build command.
2. Document validator command.
3. Document visible-flow smoke command.
4. Document reference capture command.
5. State private reference pack requirement as optional local validation dependency.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-demo-package.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-demo-package.log"
```

**Acceptance:**

- Build succeeds.
- Visible-flow smoke passes.
- README/BUILD docs have current commands.

**Commit:** `Prepare repeatable Windows demo build`

### H3: Package Demo Evidence

**Status:** Completed 2026-06-07.

**Result:** Added `docs-playable-demo-investor-evidence-2026-06-07.md`, listing the six refreshed local evidence beats, sidecar highlights, suggested three-minute use order and honest limits. Refreshed `mechlab`, `spawn`, `airfield`, `hangar-contact`, `damage-demo`, and `north-patrol` captures under ignored `analysis-output/reference-visual-captures/`. Updated `docs-reference-visual-audit-2026-06-07.md` and README navigation.

**Goal:** 收一组可以用来讲故事的截图和简短 caption。

**Files:**

- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Create or modify: `docs-playable-demo-investor-evidence-2026-06-07.md`

**Steps:**

1. Capture evidence beats:
   - MechLab fitting;
   - spawn/airfield;
   - hangar contact;
   - damage/ejection;
   - debrief.
2. Pick 3-5 screenshots.
3. Write one value caption per screenshot.
4. State that private reference content is development-only if any local reference visuals are used.

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol
```

**Acceptance:**

- Evidence page can support a short investment conversation.
- It does not imply private reference content is publishable final product content.

**Commit:** `Package playable demo evidence`

### P1: Document Reference Content Boundary

**Status:** Completed 2026-06-07.

**Result:** README now states the public product framing and release checklist: AI-assisted tactical RTS command, deterministic mech squad battle, optional AI deputy, replaceable content packs and future community maps. `docs-content-replacement-plan.md` now defines local reference, reference-linked dev, text-safe, art-safe and clean public pack states. `docs-content-pack.md` now defines public boundary states, clean replacement requirements and neutral reference manifest wording. `unity-mc2-demo/README.md` now explains that source-derived ids and encounter labels are development contract terms, not public product names.

**Goal:** 让仓库和 README 说清楚：开发可以用私有参考包验证，本项目价值在 AI RTS 指挥和可替换内容包，不在搬运旧素材。

**Files:**

- Modify: `README.md`
- Modify: `docs-content-replacement-plan.md`
- Modify: `docs-content-pack.md`
- Modify if needed: `unity-mc2-demo/README.md`

**Steps:**

1. Audit public text:

```powershell
rg -n "MechCommander|MechWarrior|原版|旧作|reference|private|clone|复刻" README.md docs-*.md unity-mc2-demo/README.md
```

2. Keep internal/reference language contained.
3. Public-facing README emphasizes:
   - AI RTS commander exploration;
   - deterministic mech squad battle;
   - optional AI deputy;
   - replaceable content packs;
   - future community maps.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- README does not pitch the product as an old-game clone.
- Private reference content is described as development-only.

**Commit:** `Document reference content boundary`

### P2: Add Public Content Boundary Check

**Status:** Completed 2026-06-07.

**Result:** Added `scripts/content-pack/check_public_content_boundary.ps1`, a read-only scanner for public package/build paths. It checks file and directory names plus text-like file contents for private reference pack paths, local extraction folders, reference-linked manifest markers, legacy/proprietary names, development-only notes and known public-forbidden markers. It returns `0` on clean input and `1` when forbidden markers are found. The current development build correctly fails because it still contains local absolute paths, reference-linked package traces, `mc2_01` task ids, `Starslayer` markers and `MC2UnityDemo` build identity.

**Goal:** 发布或对外演示前能检查 build path 没混入私有参考素材。

**Files:**

- Create or modify: `scripts/content-pack/check_public_content_boundary.ps1`
- Modify: `docs-content-replacement-plan.md`
- Modify: `README.md`

**Steps:**

1. Define forbidden path/name patterns:
   - private reference pack path;
   - legacy names;
   - local-only extraction folders;
   - original asset naming patterns if known.
2. Add dry-run check.
3. Do not delete or move anything.
4. Document how to run before public packaging.

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\content-pack\check_public_content_boundary.ps1 -Path "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows" -DryRun
```

**Acceptance:**

- The check is safe and non-destructive.
- Public build boundary can be explained to collaborators.

**Commit:** `Add public content boundary check`

### H4: Run Demo Handoff Gate Audit

**Status:** Completed 2026-06-07.

**Result:** H4 passed the local handoff gates for a development Demo. `git diff --check` passed. `analysis-output/unity-validate-handoff.log` reports contract validation OK. `analysis-output/unity-build-handoff.log` reports `Build Finished, Result: Success` and `MC2 Unity demo Windows build OK`. `analysis-output/unity-player-handoff-visible-flow.log` reports smoke exit code `0`. The six capture presets refreshed under `analysis-output/reference-visual-captures/`. Public boundary status is explicit: `project-owned-starter.example.json` returns `Result: OK`, while the current Windows dev build returns expected `Result: FAILED` with 172 findings, so it remains development-only.

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
5. Capture `spawn,airfield,hangar-contact,damage-demo,north-patrol,mechlab`.
6. Run the public content boundary check and record whether the current build is development-only or public-safe.
7. Update evidence docs with exact status.
8. Do not stage generated screenshot/log/build files unless requested.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-handoff.log"
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-handoff.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-handoff-visible-flow.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol,mechlab
```

**Acceptance:**

- Handoff evidence docs state which gates pass.
- Public boundary status is explicit: clean public build passes, current dev build may fail as development-only.
- Generated artifacts remain ignored unless the user asks to package them.

**Commit:** `Audit playable demo handoff gate`

### V4: Polish Crowded Contact Occupancy

**Status:** Completed 2026-06-07.

**Result:** V4 increased BattleCore unit radii to `infantry=24 vehicle=54 mech=64`, added unit-target footprint compensation to weapon range, and added capture sidecar `ContactSpread` evidence. `analysis-output/unity-validate-crowded-contact.log` reports contract validation OK. `analysis-output/unity-build-crowded-contact.log` reports `Build Finished, Result: Success` and `MC2 Unity demo Windows build OK`. Refreshed `hangar-contact` and `damage-demo` captures pass with `ContactSpread` and the damage story intact. `capture_reference_visuals.ps1` now waits longer for late capture presets and cleans leftover local player processes after timeout.

**Goal:** 继续解决 `hangar-contact` 仍然偏密的问题，让主战斗截图更像战术交战，而不是所有单位围着一个建筑挤在一起。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `scripts/unity/capture_reference_visuals.ps1`
- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify: `docs-playable-demo-investor-evidence-2026-06-07.md`

**Steps:**

1. Inspect current `hangar-contact.json` and `damage-demo.json` occupancy fields.
2. Decide whether the density is caused by unit attack-slot spacing, target parking, spawn activation, camera composition, or visual scale.
3. Prefer BattleCore spacing/occupancy evidence over presentation-only fixes.
4. If adding a new capture preset or sidecar field helps, keep it small and H4-compatible.
5. Re-run validator, build or smoke as appropriate.
6. Capture `hangar-contact` and `damage-demo`.
7. Update visual audit with before/after evidence.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-crowded-contact.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets hangar-contact,damage-demo
```

**Acceptance:**

- `hangar-contact` remains tactically readable with terrain, objective, squad, hostile direction and damage cues visible.
- Occupancy sidecar still proves unit, structure, hardProp and landing-blocked regions.
- No additional battle UI is added to explain the fight.

**Commit:** `Polish crowded contact occupancy`

## 7. Milestone Gates

| Gate | Definition | Evidence |
| --- | --- | --- |
| G1 Combat feel | Weapons, impact, section damage and ejection are visible enough in `damage-demo` | validator, build, `damage-demo` capture, visible-flow smoke |
| G2 Battle readability | First map reads as terrain/building/mech battle, not colored clutter | five reference captures + sidecars |
| G3 MechLab feel | Fitting reads as physical block placement; weapons are always active | loadout validator, MechLab smoke, MechLab screenshot |
| G4 Battle loop | Debrief, repair and relaunch are simple and do not expose save-slot management | debrief smoke + repair/relaunch validator |
| G5 AI capability | AI is optional, compact and high-level; no key or timeout still leaves local demo playable | AI validator + offline smoke |
| G6 Public boundary | README and build checks separate private reference content from product content | README audit + safety check |
| G7 Physical occupancy | Unit/building/prop/water blockers have BattleCore evidence and optional placeholder review | validator + sidecars |
| G8 Handoff | A collaborator can run, smoke and explain the demo in three minutes | walkthrough + build + evidence page |

## 8. Later Platform Plan

These are product roadmap items, not first-demo tasks.

| Phase | Start Condition | Scope |
| --- | --- | --- |
| L1 Main server prototype | Local battle and MechLab loop are convincing | account id, inventory snapshot, token ledger, signed loadout, reward claim, leaderboard |
| L2 Map package protocol | First demo package is stable | map manifest, mission script, reward table, license/provenance, validator |
| L3 Partner/community map server | Map package can be validated offline | hosted maps, signed result submit, reward cap, anti-cheat proof |
| L4 Web ranking | Server has signed battle summaries | leaderboard, player squad page, map ranking, season events |
| L5 AI托管/paper simulation | Local battle loop and commander adapter are stable | AI commander profile, paper battle loss model, daily support reward |
| L6 Ethereum/chain experiment | Economy and legal model are mature | optional proof or revenue-share accounting, not first-version gameplay dependency |

## 9. Stop Conditions

Stop and reassess if any of these happen:

- `git status --short` shows unrelated user/source changes in files planned for editing.
- Unity scene fileID churn appears without intentional scene changes.
- Validator fails on movement, damage, objective, repair or loadout behavior.
- `hangar-contact` or `damage-demo` screenshots become less readable than previous evidence.
- Unit, building or terrain collision exists only in Unity presentation with no BattleCore evidence.
- AI code starts making per-frame or per-shot decisions.
- Normal battle UI exposes save slots, account management, debug-only panels or too much text.
- Public-facing docs start pitching the project as a clone rather than AI-assisted tactical RTS exploration.

## 10. One-Line Direction

先把 AI 副官收成一个小而稳的能力窗口，然后把重点放回游戏本身：更清晰的 3D 战场、更可靠的物理占位证据、更直观的装配格子、更强的部位损伤卖点，最后打包一版能跑、能看、能讲的 Windows Demo。

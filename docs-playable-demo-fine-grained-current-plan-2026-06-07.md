# Playable Demo Fine-Grained Current Plan Implementation Plan

> **For Codex:** REQUIRED SUB-SKILL: Use executing-plans to implement this plan task-by-task.

**Goal:** 把 Unity 6 Windows 本地原型收成一版能演示、能截图、能讲清楚价值的轻量机甲战术指挥 Demo：机库装配直观，战场可读，单位有物理占位证据，AI 副官只做高层建议。

**Architecture:** `BattleCore` 是权威规则层，负责任务、移动、喷射、占位、武器、热量、装甲硬度、部位损伤、维修、结算和 AI observation/directive。Unity Presentation 只负责输入、固定镜头、HUD、模型、材质、特效、截图和调试可视化。开发期可以使用本地私有参考内容包验证画面、比例和节奏，公开构建必须能切到项目自有或合规授权内容包。

**Tech Stack:** Unity 6, C#, Windows Standalone, deterministic BattleCore, PowerShell validation/capture scripts, `mc2-unity-demo-contract-v1`, private local reference content pack, Git/GitHub.

**Revision:** 2026-06-07 v2. This file is the current execution entry. Older `docs-playable-demo-*.md` files are history, evidence, or task archives.

---

## 0. How To Use This Plan

日期：2026-06-07。

当下一句是“按计划继续”时，先看本节和 `## 6. Immediate Queue`，从第一个 `Next` 或 `In Progress` 任务开始。旧计划里已经完成的阶段不要重复做，除非新的截图或 validator 证明退化。

当前优先级：

1. 先收当前未提交的 AI compact observation 变更，避免脏工作树继续漂着。
2. 完成 AI 副官能力窗口的最小闭环，但不让 AI 进入逐帧战斗。
3. 回到游戏本身：战场碰撞占位证据、模型/地形可读性、装配手感、损伤卖点、投资演示包。
4. 保持第一版只做 Windows 本地可玩 Demo，不扩保存系统、不做实时 PVP、不做移动端、不做链上系统。

当前真实工作树提醒：

- Branch: `master...ai-origin/master [ahead 47]`。
- 当前 Stage 6 / D1 compact AI observation 已完成验证，下一步从 D2 directive adapter 开始。

主要配套文档：

- `docs-reference-visual-audit-2026-06-07.md`: 截图、sidecar、validator、smoke 证据。
- `docs-ai-commander-directive-contract.md`: AI 副官 high-level directive 合同。
- `docs-content-replacement-plan.md`: 私有参考包和公开替换包边界。
- `docs-platform-ecosystem-plan.md`: 地图服务器、排行、奖励认证、创作者生态长期方向。
- `README.md`: 对外叙事入口，必须强调 AI RTS commander exploration 和 replaceable content packs。

## 1. Current Product Line

第一版只做两块核心体验：

1. 机库装配：机甲、武器、装甲板、散热器、热量、重量、合法性、维修、出战。
2. 地图战斗：固定视角、小队命令、独立命令、喷射、自动交火、部位损伤、战后结算。

必须保留的操作规则：

- 默认全队控制。
- 状态栏点单台机甲，再点地点或目标，该机甲进入独立命令。
- 独立命令完成后自动归队，并接受最新全队命令。
- 喷射从每台机甲当前位置沿目标方向尝试固定距离位移，非法落点不动，合法落点位移。
- 固定俯视镜头默认跟随排序第一位指挥官机甲，允许有限缩放，不做自由旋转。
- 战斗中 UI 尽量少：状态栏、喷射、任务地图、暂停/系统和少量必要状态。
- 武器装上即启用，不做启用/关闭开关。
- 装甲板增加整体硬度，不引入复杂逐部位装甲账本。
- 部位损伤、断臂、腿部瘫痪、驾驶舱弹射不能丢。
- 物理占位必须有 BattleCore 规则证据；Unity 碰撞体和可视化占位只是审核辅助。

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

已经完成或已有证据：

| Area | Current State | Evidence |
| --- | --- | --- |
| Unity Windows build | 可构建，可 smoke | `analysis-output/unity-build-*.log` |
| `mc2_01` first map | 可加载地形、单位、结构、目标、触发、相机信息 | validator + capture sidecars |
| Visible flow | 已覆盖机库、出战、独立命令、喷射、集火、战报、回机库、再启动 identity | `mc2_01-visible-flow-audit.txt` |
| Battle occupancy | 单位、建筑、硬 terrain object、水域/边界占位已有 BattleCore evidence | `OccupancySummary()` + sidecars |
| HUD | 已压缩，战斗中信息不过量 | reference captures |
| Combat cues | 武器方向、命中、爆炸、残骸、断臂、腿瘫、弹射已有基础 | validator + `damage-demo` |
| Armor hardness | 装甲板走整体硬度，部位损伤仍保留 | validator |
| MechLab | 整块武器格子、装甲板、散热器、热量、重量、合法性已有基础 | loadout validator + smoke |
| Debrief loop | 战报、维修、回机库、再出战基础完成，不暴露保存槽概念 | repair/relaunch validator + smoke |
| Content boundary | 有内容包文档和替换包脚本 | README + content-pack docs |

当前风险和缺口：

| Risk | Why It Matters | Next Fix |
| --- | --- | --- |
| AI observation D1 脏工作树 | Resolved 2026-06-07 | D2 directive adapter |
| `hangar-contact`/`damage-demo` 仍可能显得挤 | 投资演示截图会被“堆一起”质疑 | 保留敌方压力，继续做构图、模型比例、占位可视化 |
| 原版参考模型/地形还不够清楚 | 用户要看到接近参考原型的 3D 地形、建筑、机甲模型 | 建立 material/model/readability pass |
| MechLab 手感仍是核心卖点 | 装配界面必须简单直观，接近原作整块占格乐趣 | 做交互和截图级 polish |
| AI 副官还没有自然窗口 | 需求是 AI 做大决策，不拖慢战斗 | 完成 optional advice window |
| 公共内容安全 | 开发期可参考私有资源，公开演示要可替换 | 做 public content safety check |

## 3. First Demo Acceptance

完整演示流程：

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
15. AI 副官窗口可显示一条高层建议；没有 API key 或模型超时时，流程完全不受阻。

截图级成功标准：

- 1280x720 不靠解释能分辨地形、机甲、建筑、敌我和战斗状态。
- `spawn`、`airfield`、`hangar-contact`、`damage-demo`、`north-patrol` 五张 preset 不退化。
- `damage-demo` 至少能看见一种明确损伤卖点：断臂、腿瘫、驾驶舱弹射或残骸。
- `hangar-contact` 保留敌方压力，但不明显像所有东西堆在一个点。
- 机库格子能让人一眼理解“什么放在哪、为什么超重/过热、怎么改”。

## 4. Architecture Boundaries

### 4.1 BattleCore Owns Gameplay Truth

BattleCore owns:

- mission contract loading;
- trigger state and objective completion;
- squad command acceptance;
- detached single-unit command state;
- auto rejoin;
- jet landing legality;
- unit, structure, hard terrain object, water and map-bound occupancy;
- weapon range, fire interval, damage, heat and armor hardness;
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

### 4.2 Unity Presentation Owns Visibility

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
- `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- `unity-mc2-demo/Assets/Editor/Mc2DemoBuilder.cs`
- `scripts/unity/capture_reference_visuals.ps1`

Rule: Unity can show collision rings, blocker boxes, fade, silhouettes and debug markers, but cannot become the only gameplay collision system.

### 4.3 Content Boundary

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

Do not market the project as a clone. Public pitch:

```text
AI-assisted tactical RTS commander exploration with deterministic mech squad battle,
optional AI deputy, replaceable content packs, and future community map ecosystem.
```

### 4.4 AI Boundary

AI can:

- read compact observation;
- draft an opening plan;
- choose one high-level directive for a 10-30 second phase;
- show a short advice line in UI;
- support future AI托管 and paper-resolution calculations.

AI cannot:

- mutate `BattleMission` directly;
- decide every shot or every frame;
- choose exact coordinates for the player-facing local command layer;
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

AI contract change:

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-observation.log"
```

Known good strings:

- `MC2 demo contract validation OK`
- `Build Finished, Result: Success`
- `MC2 Unity demo Windows build OK`
- `MC2 demo smoke test exiting with code 0`
- `MC2 reference visual captures passed`

Do not stage generated PNG/JSON/log evidence unless explicitly requested.

## 6. Immediate Queue

### D0: Stabilize Current Worktree

**Status:** Completed 2026-06-07.

**Goal:** 先收当前未提交的 `CommanderObservationPort.cs` 变化，防止后续计划和代码状态错位。

**Files:**

- Read: `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs`
- Read: `unity-mc2-demo/Assets/Scripts/BattleCore/MiniMaxCommander.cs`
- Read: `unity-mc2-demo/Assets/Scripts/BattleCore/RuleCommander.cs`
- Read: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Read: `docs-ai-commander-directive-contract.md`
- Modify if completing D1: same files above plus `docs-reference-visual-audit-2026-06-07.md`

**Step 1: Inspect dirty diff**

Run:

```powershell
git status --short --branch
git diff -- unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs
```

Expected: only compact observation work is dirty.

**Step 2: Decide whether D1 is completion work**

If the diff contains compact observation types, report index, commander identity, threat summary or available intents, continue D1. If the diff contains unrelated edits, stop and ask before changing those unrelated sections.

**Step 3: Do not start new visual or MechLab work yet**

Keep this task narrow. The acceptance is a clean or intentionally understood worktree.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- The uncommitted file is classified as D1 work.
- No unrelated code is overwritten.

**Commit:** none. This is a preflight.

### D1: Freeze Compact AI Observation

**Status:** Completed 2026-06-07.

**Result:** `CommanderObservationPort` now emits a bounded `mc2-ai-observation-compact-v1` summary beside the full local observation. `MiniMaxCommander` uses the compact summary for model prompts and keeps full observation only as a legacy fallback. `Mc2DemoValidator` proves compact schema, commander identity, objective summary, player damage/detached state, hostile pressure, available intents, size budget and forbidden detail exclusions. Evidence: `analysis-output/unity-validate-ai-observation.log`.

**Goal:** AI model input stays small, stable and useful for high-level decisions. Full local observation can remain for deterministic `RuleCommander` and debug reports.

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/MiniMaxCommander.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `docs-ai-commander-directive-contract.md`
- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify: `docs-playable-demo-fine-grained-current-plan-2026-06-07.md`

**Step 1: Add compact observation validator assertions**

In `Mc2DemoValidator.ValidateCommanderObservationPort()`, assert the compact observation includes:

- schema id;
- mission id and report index;
- mission phase;
- commander unit identity;
- current objective summary;
- compact player states;
- detached command state;
- section damage summary;
- hostile pressure summary;
- available intents.

Also assert it excludes:

- full `playerUnits` detail arrays in model prompt;
- `activeHostiles` full arrays in model prompt;
- projectile history;
- path graph data;
- per-frame data;
- exact move target and exact attack target IDs in model prompt.

**Step 2: Finish compact observation builder**

In `CommanderObservationPort.cs`, keep full `CommanderObservation` for local logic, but add `compact` as a bounded summary. Keep player count capped at 6 and threat list capped at a small number.

Do not remove fields that `RuleCommander` already uses.

**Step 3: Make MiniMax use compact summary**

In `MiniMaxCommander.cs`, build the user prompt from `observation.compact` when present. Keep a legacy fallback only for internal compatibility.

Add an internal validation accessor if the validator needs to inspect the generated prompt without making network calls.

**Step 4: Document the contract**

Update `docs-ai-commander-directive-contract.md` with:

- compact observation schema;
- allowed input fields;
- forbidden model input categories;
- max model prompt budget;
- fallback directive behavior.

**Step 5: Validate**

Run:

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-observation.log"
```

Expected log string:

```text
MC2 demo contract validation OK
```

**Step 6: Commit**

```powershell
git add unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs unity-mc2-demo/Assets/Scripts/BattleCore/MiniMaxCommander.cs unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs docs-ai-commander-directive-contract.md docs-reference-visual-audit-2026-06-07.md docs-playable-demo-fine-grained-current-plan-2026-06-07.md
git commit -m "Freeze AI observation contract"
```

**Acceptance:**

- Local battle runs without AI.
- Model prompt is compact and bounded.
- Full local `RuleCommander` path still works.
- Validator guards compact schema and forbidden detail leakage.

### D2: Guard AI Directive Adapter

**Status:** Pending.

**Goal:** AI output becomes ordinary BattleCore command intent, with safe fallback and no direct mission mutation.

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/RuleCommander.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/MiniMaxCommander.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify if needed: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-visible-flow-audit.txt`
- Modify: `docs-ai-commander-directive-contract.md`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Step 1: Write validator cases for all directive tokens**

Assert these tokens are accepted:

```text
assault-objective
engage-hostiles
regroup
hold
```

Assert invalid text falls back to `assault-objective`.

**Step 2: Validate no direct mutation**

The adapter should return a local command string or no-op decision. It should not mutate `BattleMission` by itself.

**Step 3: Validate fallback without API key**

Use environment-free test path. MiniMax failure should produce a local fallback directive and not break smoke.

**Step 4: Run validator and visible smoke**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-directive.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-ai-directive-fallback.log"
```

**Step 5: Commit**

```powershell
git add unity-mc2-demo/Assets/Scripts/BattleCore/RuleCommander.cs unity-mc2-demo/Assets/Scripts/BattleCore/MiniMaxCommander.cs unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs docs-ai-commander-directive-contract.md docs-reference-visual-audit-2026-06-07.md
git commit -m "Guard AI directive adapter"
```

**Acceptance:**

- AI directive is high-level only.
- Invalid or slow model output does not block battle.
- Player commands remain higher priority than AI suggestions.

### D3: Show Optional AI Advice Window

**Status:** Pending.

**Goal:** UI shows AI as a small deputy capability window, not a chat console and not a required battle system.

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/MiniMaxCommander.cs`
- Modify: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-visible-flow-audit.txt`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Step 1: Define window states**

Window states:

- `Offline`: no API key or disabled.
- `Planning`: request pending.
- `Advice`: directive plus short reason.
- `Fallback`: local commander active.

**Step 2: Keep it small**

Place it in the pause/system or compact mission side area. Do not add a large chat log. Do not cover status rows.

**Step 3: Add smoke assertion**

Command-file smoke should assert that local flow still passes when AI is offline.

**Step 4: Capture**

Run:

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-ai-advice-window.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-ai-advice-window.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,damage-demo
```

**Step 5: Commit**

```powershell
git add unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-visible-flow-audit.txt docs-reference-visual-audit-2026-06-07.md
git commit -m "Show optional AI advice window"
```

**Acceptance:**

- AI reads as optional deputy.
- No API key path is clear and playable.
- UI remains clean in battle.

## 7. Battle Space And Visual Readability Queue

### V1: Re-Audit Physical Occupancy Against Current Screenshots

**Status:** Pending after D1-D3, or earlier if screenshots are the priority.

**Goal:** 回答“怎么还是堆在一起”和“应该有物理碰撞占位”的问题，用 BattleCore evidence 和 screenshot sidecar 对齐。

**Files:**

- Read/modify: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Read/modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Read/modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoTerrainView.cs`
- Read/modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Step 1: Refresh captures**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets hangar-contact,damage-demo
```

**Step 2: Read sidecar occupancy facts**

Confirm sidecars contain:

- active unit count;
- visible unit count;
- unit collision radii;
- blocking structures;
- hard prop blockers;
- water/map landing blocks;
- occupancy placeholder totals.

**Step 3: Compare screenshot to rules evidence**

If units overlap visually but rules radii are valid, classify as model scale/camera compression/effect noise. If rules radii are too small, adjust BattleCore spacing and validate.

**Step 4: Validate**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-occupancy-readability.log"
```

**Acceptance:**

- `hangar-contact` and `damage-demo` have written diagnosis.
- Any collision change has BattleCore validator evidence.
- Unity-only placeholder changes are clearly labeled as review visualization.

**Commit:** `Re-audit battle occupancy readability`

### V2: Improve Original-Like 3D Terrain And Model Readability

**Status:** Pending.

**Goal:** 让本地 Demo 更接近参考原型的 3D 地形、建筑、树木/环境物和机甲可读性，减少“糊成一坨”的观感。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoTerrainView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `scripts/content-pack/export_reference_visual_pack.ps1`
- Modify if needed: `scripts/content-pack/export_tgl_to_obj.py`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Step 1: Split the failure type**

For each bad screenshot, mark the primary cause:

- material too flat;
- model scale too small/large;
- too many enemies in frame;
- camera compression;
- UI occlusion;
- tree/building occlusion;
- effects too noisy;
- missing reference asset.

**Step 2: Fix materials before adding complexity**

Improve terrain, water, road/runway, building base and mech material contrast. Avoid overusing one hue family.

**Step 3: Fix scale and silhouette**

Check mech height, vehicle height, building footprint and hard prop footprint. Preserve BattleCore collision evidence.

**Step 4: Refresh captures**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-visual-readability.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol
```

**Acceptance:**

- Five reference captures remain nonblank and readable.
- `hangar-contact` does not look like a single pile.
- `damage-demo` shows at least one readable mech damage cue.
- No content-pack/public-boundary rule is loosened.

**Commit:** `Improve reference visual readability`

### V3: Lock Collision Placeholder Review Layer

**Status:** Pending.

**Goal:** 给开发审核一个可开关的物理占位层，能看见单位半径、建筑 blockers、hard props 和喷射非法区，但不把调试层做成正式 UI。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoTerrainView.cs`
- Modify: `scripts/unity/capture_reference_visuals.ps1`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Step 1: Verify current placeholder toggle**

Run capture with current placeholder settings and confirm sidecar reports placeholder counts.

**Step 2: Add missing blocker categories only if needed**

Only add categories backed by BattleCore evidence. Do not create Unity-only gameplay blockers.

**Step 3: Capture with placeholders**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets hangar-contact,damage-demo
```

**Acceptance:**

- Sidecar states placeholder enabled/disabled.
- Placeholder layer makes blockers auditable.
- Normal player HUD does not show debug clutter.

**Commit:** `Lock occupancy placeholder review layer`

## 8. MechLab Queue

### M1: Make MechLab Interaction Feel Like Physical Blocks

**Status:** Pending.

**Goal:** 装配界面继续靠近“整块武器放进格子”的乐趣，而不是表格编辑。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutValidator.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-loadout-compact.txt`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Step 1: Preserve mounted weapon semantics**

Assert all mounted weapons are active. Do not add weapon enable/disable toggles.

**Step 2: Make blocks obvious**

Mounted weapons need:

- one outer frame per weapon;
- visible internal grid dividers;
- shape label;
- section label;
- selected/invalid state.

Armor plates and heat sinks need:

- single-cell filler language;
- count remaining;
- immediate heat/weight/hardness effect.

**Step 3: Add smoke assertion**

Smoke should assert:

- no weapon toggle copy;
- selected mech preview exists;
- grid block language exists;
- filler cells are visible when available.

**Step 4: Validate**

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

### M2: Add One MechLab Screenshot Evidence Pass

**Status:** Pending.

**Goal:** 不只靠 smoke 文本，留一张能给人看的装配界面截图。

**Files:**

- Modify: `scripts/unity/capture_reference_visuals.ps1`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Step 1: Add or use a MechLab capture preset**

Capture should open the compact loadout view, select the default mech and expose grid blocks.

**Step 2: Capture**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets mechlab
```

**Step 3: Record judgment**

Write the screenshot path and one-paragraph judgment in `docs-reference-visual-audit-2026-06-07.md`.

**Acceptance:**

- MechLab screenshot is a first-class evidence beat for the demo package.
- The screenshot does not require explaining weapon enable toggles.

**Commit:** `Capture MechLab fitting evidence`

## 9. Combat Feel Queue

### C1: Strengthen Damage-Demo Selling Moment

**Status:** Pending.

**Goal:** `damage-demo` 必须能一眼看出这不是普通 RTS 小兵血条，而是机甲部位损伤战斗。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Step 1: Preserve BattleCore damage truth**

Do not fake destroyed arms or ejection only in presentation. Use BattleCore section state.

**Step 2: Make one event unmistakable**

Prefer one strong cue over many tiny cues:

- arm loss marker;
- leg mobility loss marker;
- cockpit ejection trail;
- wreck silhouette;
- section status row highlight.

**Step 3: Validate and capture**

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

**Status:** Pending when UI starts creeping again.

**Goal:** 战斗中不用显示太多信息，但状态栏必须承担控制和损伤概览。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-visible-flow-audit.txt`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Step 1: Audit battle text**

Remove or hide nonessential active-battle text. Keep:

- status rows;
- selected unit state;
- jet button;
- compact objective/map;
- pause/system entry.

**Step 2: Guard with smoke**

Assert save/account/admin copy is not in normal active battle.

**Step 3: Capture**

```powershell
git diff --check
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-battle-ui-sparse.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,damage-demo
```

**Acceptance:**

- Battle UI does not cover core combat.
- Status rows still show health, damaged section, detached state and ready/repair state.

**Commit:** `Keep battle UI sparse`

## 10. Demo Handoff Queue

### H1: Write Three-Minute Walkthrough Script

**Status:** Pending.

**Goal:** 给协作者或投资人一个照着走就能看懂的演示脚本。

**Files:**

- Create or modify: `docs-playable-demo-walkthrough-2026-06-07.md`
- Modify: `README.md`

**Step 1: Script the flow**

Include:

1. start;
2. MechLab;
3. inspect fit;
4. launch;
5. squad move;
6. focus target;
7. solo command;
8. jet;
9. damage/ejection;
10. debrief;
11. repair;
12. relaunch.

**Step 2: Use safe wording**

Use project-owned language:

- AI-assisted tactical RTS;
- mech squad command;
- deterministic BattleCore;
- optional AI deputy;
- replaceable content packs.

Do not use old franchise names as product pitch.

**Step 3: Validate docs**

```powershell
git diff --check
```

**Acceptance:**

- A new person can follow the demo without knowing the repo.
- The script matches current UI labels.

**Commit:** `Write playable demo walkthrough`

### H2: Prepare Repeatable Windows Demo Build

**Status:** Pending.

**Goal:** 形成可重复构建、可 smoke、可发截图证据的本地 Windows Demo 包。

**Files:**

- Modify: `BUILD-WIN.md`
- Modify: `unity-mc2-demo/README.md`
- Modify if needed: `scripts/content-pack/start_runtime_shell.ps1`
- Modify if needed: `scripts/content-pack/install_dev_shortcut.ps1`

**Step 1: Build**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-demo-package.log"
```

**Step 2: Smoke**

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-demo-package.log"
```

**Step 3: Document exact run path**

State the local executable path and smoke command. Do not ask the user to copy files manually.

**Acceptance:**

- Build succeeds.
- Visible-flow smoke passes.
- README/BUILD docs have the current command.

**Commit:** `Prepare repeatable Windows demo build`

### H3: Package Demo Evidence

**Status:** Pending.

**Goal:** 收一组可以用来讲故事的截图和简短 caption。

**Files:**

- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Create or modify: `docs-playable-demo-investor-evidence-2026-06-07.md`

**Step 1: Capture all evidence beats**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol
```

**Step 2: Pick 3-5 screenshots**

Recommended:

- MechLab fitting;
- spawn/airfield;
- hangar contact;
- damage/ejection;
- debrief.

**Step 3: Write captions**

Each caption should explain one value:

- simple squad command;
- AI-assisted tactical decisions;
- mech fitting;
- BattleCore deterministic simulation;
- replaceable content packs.

**Acceptance:**

- Evidence page can support a short investment conversation.
- It does not imply private reference content is publishable final product content.

**Commit:** `Package playable demo evidence`

## 11. Content And Public Safety Queue

### P1: Document Private Reference Content Boundary

**Status:** Pending.

**Goal:** 让仓库和 README 说清楚：开发可以用私有参考包验证，本项目价值在 AI RTS 指挥和可替换内容包，不在搬运旧素材。

**Files:**

- Modify: `README.md`
- Modify: `docs-content-replacement-plan.md`
- Modify: `docs-content-pack.md`
- Modify if needed: `unity-mc2-demo/README.md`

**Step 1: Audit public text**

Search:

```powershell
rg -n "MechCommander|原版|旧作|reference|private|clone|复刻" README.md docs-*.md unity-mc2-demo/README.md
```

**Step 2: Keep internal/reference language contained**

Internal docs can mention private reference pack. Public-facing README should emphasize:

- AI RTS commander exploration;
- deterministic mech squad battle;
- optional AI deputy;
- replaceable content packs;
- future community maps.

**Step 3: Validate docs**

```powershell
git diff --check
```

**Acceptance:**

- README does not pitch the product as an old-game clone.
- Private reference content is described as development-only.

**Commit:** `Document reference content boundary`

### P2: Add Public Build Content Safety Check

**Status:** Pending.

**Goal:** 发布或对外演示前能检查 build path 没混入私有参考素材。

**Files:**

- Create or modify: `scripts/content-pack/check_public_content_boundary.ps1`
- Modify: `docs-content-replacement-plan.md`
- Modify: `README.md`

**Step 1: Define forbidden path/name patterns**

Include:

- private reference pack path;
- legacy names;
- local-only extraction folders;
- original asset naming patterns if known.

**Step 2: Add dry-run check**

The script should scan configured build/output path and report warnings without deleting or moving anything.

**Step 3: Validate**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\content-pack\check_public_content_boundary.ps1 -Path "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows" -DryRun
```

**Acceptance:**

- The check is safe, non-destructive and documented.
- Public build boundary can be explained to collaborators.

**Commit:** `Add public content boundary check`

## 12. Later Platform Plan

这些不是第一版 Demo 任务，只保留接口和文档方向。

| Phase | When To Start | Scope |
| --- | --- | --- |
| L1 Main server prototype | Local battle and MechLab loop are convincing | account id, inventory snapshot, token ledger, signed loadout, reward claim, leaderboard |
| L2 Map package protocol | First demo package is stable | map manifest, mission script, reward table, license/provenance, validator |
| L3 Partner/community map server | Map package can be validated offline | hosted maps, signed result submit, reward cap, anti-cheat proof |
| L4 Web ranking | Server has signed battle summaries | leaderboard, player squad page, map ranking, season events |
| L5 AI托管/paper simulation | Local battle loop and commander adapter are stable | AI commander profile, paper battle loss model, daily support reward |
| L6 Ethereum/chain experiment | Economy and legal model are mature | optional proof or revenue-share accounting, not first-version gameplay dependency |

## 13. Current Commit Order

Use this queue unless the user changes priority:

| Order | Status | Commit | Main Gate |
| --- | --- | --- | --- |
| 0 | Done | Preflight only | Dirty worktree understood |
| 1 | Done | `Freeze AI observation contract` | G5 AI capability |
| 2 | Next | `Guard AI directive adapter` | G5 AI capability |
| 3 | Pending | `Show optional AI advice window` | G5 AI capability |
| 4 | Pending | `Re-audit battle occupancy readability` | G2 battle readability |
| 5 | Pending | `Improve reference visual readability` | G2 battle readability |
| 6 | Pending | `Lock occupancy placeholder review layer` | G2 collision evidence |
| 7 | Pending | `Polish MechLab block fitting` | G3 MechLab feel |
| 8 | Pending | `Capture MechLab fitting evidence` | G3 MechLab evidence |
| 9 | Pending | `Strengthen damage demo readability` | G1 combat feel |
| 10 | Pending | `Keep battle UI sparse` | G1/G2 UI readability |
| 11 | Pending | `Write playable demo walkthrough` | G8 handoff |
| 12 | Pending | `Prepare repeatable Windows demo build` | G8 handoff |
| 13 | Pending | `Package playable demo evidence` | G8 handoff |
| 14 | Pending | `Document reference content boundary` | G6 public boundary |
| 15 | Pending | `Add public content boundary check` | G6 public boundary |

## 14. Milestone Gates

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

## 15. Stop Conditions

Stop and reassess if any of these happen:

- `git status --short` shows unrelated user/source changes in files planned for editing.
- Unity scene fileID churn appears without intentional scene changes.
- Validator fails on movement, damage, objective, repair or loadout behavior.
- `hangar-contact` or `damage-demo` screenshots become less readable than the previous evidence.
- Unit, building or terrain collision exists only in Unity presentation with no BattleCore evidence.
- AI code starts making per-frame or per-shot decisions.
- Normal battle UI exposes save slots, account management, debug-only panels or too much text.
- Public-facing docs start pitching the project as a clone rather than AI-assisted tactical RTS exploration.

## 16. One-Line Direction

先把 AI 副官收成一个小而稳的能力窗口，然后把重点放回游戏本身：更清晰的 3D 战场、更可靠的物理占位证据、更直观的装配格子、更强的部位损伤卖点，最后打包一版能跑、能看、能讲的 Windows Demo。

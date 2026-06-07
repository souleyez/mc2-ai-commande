# AI RTS Commander Current Master Implementation Plan

> **For Codex:** REQUIRED SUB-SKILL: Use executing-plans to implement this plan task-by-task.

**Goal:** 把当前 Unity 6 Windows 本地原型收成一版能稳定展示的 AI 副官机甲战术 RTS Demo：机库装配有乐趣，第一张固定视角 3D 任务图可读，机甲和建筑有 BattleCore 物理占位证据，部位损伤有卖点，AI 只做高层副官建议，后续能平滑进入公开替换包、地图服务器、Web 排行和创作者生态。

**Architecture:** `BattleCore` 是确定性规则层，负责移动、喷射、占位、命中、武器、热量、装甲硬度、部位损伤、任务触发、战报、维修、奖励草案和 AI observation/directive。Unity 6 只负责固定镜头、输入、稀疏 HUD、MechLab、模型、材质、特效、截图、构建和本地 smoke。开发期可以使用本机私有参考内容验证比例、节奏和视觉可读性，公开演示或商业版本必须切到项目自有或合规授权内容包。

**Tech Stack:** Unity 6, C#, Windows Standalone first, deterministic BattleCore, PowerShell validator/build/smoke/capture scripts, replaceable content packs, optional high-level AI deputy adapter, later main server/map server/Web ranking contracts.

**Revision:** 2026-06-07 v6. This is the current master plan after the private reference visual bridge, visible-flow seal, investor evidence package, art-safe metadata target, AI deputy offline guard, and platform reward authority contract were refreshed. Older plan files remain evidence/history; this file is the first place to read when the user says "按计划继续". The finer task breakdown now lives in `docs-ai-rts-commander-current-detailed-plan-2026-06-07.md`.

---

## 0. How To Use This Plan

当前仓库里 `docs` 是历史文件，不是目录，所以本计划继续按项目现有习惯放在根目录 `docs-*.md`。

当用户说“按计划继续”时：

1. 先看本文件 `## 5. Current Commit Queue`。
2. 只执行第一个 `Next` 或 `In Progress` 任务。
3. 每次只做一个能验证的小提交。
4. 规则先进入 BattleCore，Unity 只做表现和输入。
5. 视觉改动必须配截图或 sidecar 证据。
6. 不提交 `analysis-output/`、Unity player build、PNG、JSON sidecar、log 或本地参考导出，除非用户明确要求打包。
7. Unity 运行后检查 `unity-mc2-demo/Assets/Scenes/Mc2Demo.unity` 是否只有 fileID churn；非真实 scene 修改不要入库。
8. 公开文案只讲本项目自己的 AI RTS 指挥探索；本地参考内容只能作为私有开发验证材料。

配套文件分工：

| File | Purpose |
| --- | --- |
| `docs-ai-rts-commander-current-master-plan-2026-06-07.md` | 当前主计划和提交级队列 |
| `docs-ai-rts-commander-current-detailed-plan-2026-06-07.md` | 当前阶段的细颗粒执行计划、提交批次和验收门槛 |
| `docs-ai-rts-commander-overall-implementation-plan-2026-06-07.md` | 产品方向、架构边界和长期里程碑 |
| `docs-ai-rts-commander-v1-detailed-execution-plan-2026-06-07.md` | 上一版细计划，保留已完成任务细节 |
| `docs-ai-rts-commander-detailed-roadmap-2026-06-07.md` | 路线图和历史工作包 |
| `docs-reference-visual-restoration-plan.md` | 私有参考视觉还原专项计划 |
| `docs-reference-visual-audit-2026-06-07.md` | 截图、sidecar、视觉判断和证据 |
| `docs-playable-demo-investor-evidence-2026-06-07.md` | 演示证据页和投资沟通材料 |
| `docs-content-replacement-plan.md` | 私有参考内容到公开替换包的路径 |
| `docs-platform-ecosystem-plan.md` | 地图服务器、奖励认证、排行和创作者生态长期方向 |

## 1. Current Stage

当前阶段是：

```text
Public Art-Safe Mission Slice -> AI Deputy Offline Guard
```

不是从零开发，也不是马上做平台。现在要把本地 Demo 收成：

- 能打开；
- 能跑一局；
- 画面不是糊成色块；
- 机甲、建筑、地形和碰撞占位有可读证据；
- MechLab 装配能看出乐趣；
- 战斗 UI 不遮挡战场；
- 部位损伤有记忆点；
- AI 副官是可选能力，不拖慢本地战斗；
- 公开替换包路线讲得清楚。

已完成的关键基础：

| Area | Current State |
| --- | --- |
| Windows build | Unity batch build、player smoke、reference capture 已跑通过多轮 |
| Mission slice | `mc2_01` 可加载地形、单位、结构、目标、触发和相机 |
| Fixed camera | 使用固定俯视战术视角，默认围绕指挥官机甲 |
| Command loop | 默认全队、状态栏单选、独立命令、自动归队、喷射、集火已进入 smoke |
| Occupancy | 单位、建筑、硬道具、水域、地图边界有 BattleCore 证据 |
| Crowded contact | 已提高单位半径并增加 `ContactSpread` sidecar |
| MechLab | 武器整块占格、装甲板、散热器、热量、重量、合法性已有基础 |
| Loadout effect | validator 已证明装配会影响 BattleCore 战斗字段 |
| Damage story | 武器类型、断臂、腿瘫、驾驶舱损毁/弹射已有基础 |
| Sparse HUD | 战斗中不显示大日志和保存槽，主要保留状态行与少量按钮 |
| AI deputy | compact observation、directive adapter、能力窗口已有基础 |
| Content boundary | text-safe、visual-id、art-safe metadata slice 均已通过 public boundary check |
| Demo evidence | visible-flow、六截图、walkthrough、investor evidence 已刷新 |

当前最大缺口：

| Gap | Why It Matters | First Fix |
| --- | --- | --- |
| 清权资产还未真正接入运行包 | D1 已有 metadata 合同，但不是最终美术包 | D2 以后再做 mountable clean content pack |
| AI 副官仍要防止范围膨胀 | 模型只做大决策，不能拖慢本地战斗 | E1 offline/high-level guard |
| 平台化还是设想 | 地图服务器、奖励认证和 Web 排行要等 Demo 稳定后再做 | F1-F4 先写契约，不先写服务器 |

## 2. Product Scope Lock

### First Demo Must Have

- Windows 本地可玩。
- 一张固定视角小任务图，继续用 `mc2_01` 做验证图。
- 1-6 台机甲，常规 4 台，第一台是指挥官机甲。
- 固定俯视战术视角，允许有限缩放，不做自由旋转。
- 3D 地形：地表高度、水域、岸线、跑道/道路、建筑基底和主要遮挡物要可读。
- 物理占位：单位、建筑、硬道具、水域、地图边界必须由 BattleCore 记录，Unity 可画审计层。
- 默认全队操作：点地点移动，点目标移动攻击/集火。
- 状态栏单选：点某台机甲状态行，再点地点或目标，该机甲进入独立命令。
- 独立命令完成后自动归队，并接受最新整体命令。
- 喷射：每台机甲按当前位置朝目标方向尝试固定距离位移，非法落点不动。
- 稀疏战斗 UI：机甲状态行、喷射、任务地图/目标、系统/暂停。
- MechLab：整块武器占格，装甲板和散热器填空，显示热量、重量、合法性。
- 武器装上即启用，不做武器开关。
- 装甲板增加整体硬度，保留部位损伤，不做复杂逐部位装甲账本。
- 部位损伤：断臂、腿瘫、驾驶舱损毁/弹射、残骸。
- 战后简报、一键维修、回机库、再次出战。
- AI 副官只做高层建议和未来托管接口，不做逐帧战斗。

### First Demo Must Not Have

- 实时 PVP。
- 移动端适配。
- 地图服务器、地图编辑器、Web 排行。
- 账号、充值、提现、链上资产。
- 复杂保存槽或保存游戏 UI。
- AI 导演。
- 大模型逐帧移动、开火、回避。
- 公开发布私有参考素材、旧剧情、旧商标、旧专有名称或未清权模型/贴图/音频。

## 3. Architecture Boundary

### 3.1 BattleCore Owns Truth

Primary files:

- `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/UnitState.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/StructureState.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/MissionContract.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutContract.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutValidator.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/MechBayInventoryContract.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/RuleCommander.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/MiniMaxCommander.cs`

BattleCore owns:

- mission contract loading;
- objective and trigger state;
- squad command and detached command;
- auto rejoin;
- movement and jet landing legality;
- unit, structure, prop, water and map-bound occupancy;
- weapon range, cooldown, heat and damage;
- armor hardness;
- section damage, cockpit breach, ejection and wreck state;
- debrief, repair and relaunch;
- compact AI observation and high-level directive interpretation.

Rule:

```text
任何影响移动、命中、伤害、胜负、维修、奖励或 AI 决策的行为，
必须先进入 BattleCore 或 contract 数据。
```

### 3.2 Unity Owns Visibility

Primary files:

- `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/DemoTerrainView.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/DemoStructureView.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceObjMeshLibrary.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceTerrainTextureLibrary.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/ReferencePropLibrary.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`
- `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- `unity-mc2-demo/Assets/Editor/Mc2DemoBuilder.cs`
- `scripts/unity/capture_reference_visuals.ps1`

Unity owns:

- click/raycast input;
- fixed tactical camera and limited zoom;
- sparse battle HUD;
- MechLab layout;
- model/material/terrain rendering;
- optional occupancy placeholder review layer;
- weapon trails, impacts, explosions, fragments and ejection cues;
- command-file smoke hooks;
- screenshot capture and sidecar summaries.

Rule:

```text
Unity 可以把规则结果表现得更清楚，但不能独自决定规则结果。
```

### 3.3 Private Reference Visual Boundary

本地私有参考视觉可以用于：

- 验证地图比例；
- 验证地形、建筑、机甲、武器效果可读性；
- 验证任务节奏；
- 验证部位损伤和模型节点表现；
- 给后续项目自有替换包提供目标质量参考。

但必须满足：

- 不提交原始素材；
- 不提交原始素材导出的 OBJ/TGA/PNG/JSON；
- 不把本地参考包描述成可公开发布内容；
- 公开内容必须换成项目自有或合规授权包；
- loader/API 用 asset id 和 manifest 走替换边界，不写死旧名称作为产品身份。

Ignored evidence and local outputs should stay under:

- `analysis-output/`
- `analysis-output/unity-reference-art/`
- `analysis-output/reference-visual-captures/`
- `unity-mc2-demo/Assets/PrivateReferenceArt/`

### 3.4 AI Boundary

AI may:

- read compact observation;
- draft opening plan;
- choose one high-level directive for a phase;
- show one short advice line;
- support future AI 托管 and paper simulation.

AI must not:

- mutate `BattleMission` directly;
- decide every frame, shot or dodge;
- block local battle when API is slow or unavailable;
- spend tokens during normal validator/smoke;
- become required for local demo playability.

## 4. Validation Bus

Every commit:

```powershell
git diff --check
git status --short --branch --untracked-files=all
```

BattleCore or contract change:

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-current.log"
```

Unity presentation change:

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

Public boundary check:

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
| 0 | Done | `Refresh detailed master plan` | 把当前阶段、缺口、可展示 Demo 路线和后续平台契约拆成更细执行计划 | `git diff --check` |
| 1 | Done | `Polish MechLab grid feel` | 装配格子更像整块装备放入槽位 | validator + build + `mechlab` capture |
| 2 | Done | `Prove loadout battle effects` | 证明装配影响 BattleCore 战斗 | validator + build + visible-flow smoke |
| 3 | Done | `Polish weapon and damage readability` | 强化武器类型、断臂、腿瘫、弹射故事 | validator + build + `damage-demo` capture |
| 4 | Done | `Guard sparse battle UI regression` | 固化战斗 UI 稀疏性，不让大日志/存档/账号面板回流 | visible-flow smoke + `spawn,damage-demo` capture |
| 5 | Done | `Add close contact collision gate` | 把“堆在一起/碰撞不明显”变成可复现 sidecar 门槛 | `hangar-contact` capture + sidecar |
| 6 | Done | `Refresh first map visual slice` | 让第一张图继续朝真实 3D 地形、建筑、机甲模型靠拢；terrain/unit/structure-prop passes and `FirstMapVisual` gate done | build + five visual captures |
| 7 | Done | `Stabilize reference visual manifest export` | 私有参考模型/材质/道具/地形纹理导出走可审计 manifest，缺失时给清晰 warning | exporter validation + ignored manifest |
| 8 | Done | `Harden Unity reference visual loader` | Unity manifest-first，缺失私有包时回退到明显开发占位 | build + fallback capture |
| 9 | Done | `Document replaceable visual ids` | 固化换包 id，方便以后整包替换、换皮和公开安全包 | docs + boundary check |
| 10 | Done | `Seal visible playable walkthrough` | 启动、机库、战斗、损伤、结算、重开完整流程封口 | visible-flow smoke |
| 11 | Done | `Refresh investor evidence package` | 更新当前本地证据页，说明能演示什么、哪些仍是原型 | six captures + docs |
| 12 | Done | `Prepare public art-safe mission slice` | 从 text-safe metadata 进入第一张图的公开视觉替换计划 | boundary check + provenance docs |
| 13 | Done | `Guard AI deputy regression` | AI 保持高层、可离线、无 token smoke | validator/smoke |
| 14 | Done | `Document platform reward contracts` | 主服务器、地图服务器、奖励认证契约 | docs check |
| 15 | Next | `Plan map authoring prototype` | 地图包、触发、奖励引用和验证器规划 | docs check |
| 16 | Later | `Plan web ranking prototype` | 排行、战绩、地图页和公开资料规划 | docs check |
| 17 | Later | `Plan creator economy boundary` | 创作者分成、皮肤、自定义、链上边界 | docs check |

## 6. Detailed Tasks

本节保留主计划级任务说明。当前阶段的更细执行颗粒、每个小提交的文件清单、验收门槛和命令见 `docs-ai-rts-commander-current-detailed-plan-2026-06-07.md`。

### Task 4: Guard Sparse Battle UI Regression

**Status:** Completed 2026-06-07.

**Result:** Added a stricter `SparseBattleUi` regression summary to the battle HUD sidecar and visible-flow combat assertion. Normal active battle now reports status rows, section/solo state, Jet/Map/Bay/System controls, compact objective, closed-but-available mission map, funds-only economy, disabled save UI, hidden account UI, hidden overlays, hidden combat log and debug occupancy as sidecar-only. `capture_reference_visuals.ps1` now fails `spawn` or `damage-demo` if large logs, save/account/debug overlays or visible overlays return. The task also fixed the restarted MechLab focus path after a depot squad swap and generalized compact loadout assertions so legal non-AC10 replacement loadouts can pass.

**Goal:** 固化“战斗中不用显示太多信息”的产品决定，后续做视觉和特效时不能把战场盖住。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`
- Modify if needed: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-visible-flow-audit.txt`
- Modify: `scripts/unity/capture_reference_visuals.ps1`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Add or strengthen a battle HUD sidecar summary for normal active battle.
2. Assert normal battle view does not show:
   - save slot UI;
   - account/economy UI except tiny local funds if already present outside battle emphasis;
   - large combat log;
   - debug-only occupancy copy unless a review capture mode is active.
3. Assert normal battle still shows:
   - mech status rows;
   - selected/solo state;
   - Jet button;
   - compact objective/map affordance;
   - system/pause entry.
4. Keep battle map and system panel available, but not forced open in the default combat screenshot.
5. Rebuild if presentation code changed.
6. Run visible-flow smoke.
7. Capture `spawn,damage-demo`.
8. Update visual audit with the sparse HUD evidence.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-battle-ui-regression.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-battle-ui-regression.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,damage-demo
```

**Acceptance:**

- 战斗 UI 不遮挡战场。
- 玩家仍能移动、目标、喷射、单机独立命令。
- 状态栏承担损伤概览，不变成大仪表盘。
- Sidecar 能证明默认战斗没有大日志、存档、账号和调试面板。

**Commit:** `Guard sparse battle UI regression`

### Task 5: Add Close Contact Collision Gate

**Status:** Completed 2026-06-07.

**Result:** Added BattleCore `ContactClearanceSummary()` so close-contact captures can report nearest player-hostile, hostile-hostile and player-player pairs with center distance, collision radii, clearance, worst clearance, overlap count and separated/overlap status. Unity sidecars now write `contactClearance`, and `capture_reference_visuals.ps1` fails `hangar-contact`/`damage-demo` if real overlaps return. `hangar-contact` also now checks blocker counts, hard-prop categories, landing-blocked markers and contact spread. The observed dense hangar case is now measured as `overlaps=0 status=separated`; the closest contacts are touching or within the 0.5-unit collision audit tolerance, not real same-point stacking.

**Goal:** 把“看起来堆在一起”和“应该有物理碰撞占位”变成可重复检查的规则，而不是每次只靠肉眼判断。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `scripts/unity/capture_reference_visuals.ps1`
- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify if needed: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Inspect current `analysis-output/reference-visual-captures/hangar-contact.json` if present.
2. Keep or add sidecar fields:
   - unit radii by class;
   - nearest player-hostile distance;
   - nearest hostile-hostile distance;
   - player span and hostile span;
   - blocker counts by type;
   - landing-blocked count.
3. Add a close-contact capture preset only if current `hangar-contact` framing cannot judge spacing.
4. If overlap is real, tune BattleCore occupancy/attack-slot spacing conservatively.
5. Do not solve real overlap with presentation-only offsets.
6. Capture `hangar-contact,damage-demo`.
7. Update visual audit with a short judgment: real overlap, camera compression, visual scale, or fixed.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-close-contact-collision.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets hangar-contact,damage-demo
```

**Acceptance:**

- Capture sidecar explains spacing without opening Unity Editor.
- Unit/building/prop/water blockers are BattleCore evidence, not Unity-only visuals.
- `hangar-contact` reads as tactical contact, not one-point pile.
- Jet illegal landing behavior still works.

**Commit:** `Add close contact collision gate`

### Task 6: Refresh First Map Visual Slice

**Status:** Completed 2026-06-07.

**Goal:** 继续把第一张图从“有点样子”推到“可演示 3D 机甲战场”：地形、建筑、树木/道具、机甲和敌方单位在默认镜头下可分辨。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoTerrainView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoStructureView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `scripts/unity/capture_reference_visuals.ps1`
- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify if needed: `docs-playable-demo-investor-evidence-2026-06-07.md`

**Steps:**

1. Compare `spawn`, `airfield`, `hangar-contact`, `north-patrol`, `damage-demo`.
2. Classify each visual problem:
   - terrain too flat;
   - mech silhouette too small or too noisy;
   - props unreadable;
   - camera too tight;
   - effects too bright;
   - UI overlap;
   - missing/private reference asset fallback.
3. Prefer material/scale/contrast fixes before adding new visual complexity.
4. Keep palette readable and not dominated by one hue family.
5. Preserve sparse HUD and fixed camera.
6. Rebuild Windows player.
7. Capture five presets.
8. Update evidence docs with before/after judgment.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-first-map-visual-slice.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,north-patrol,damage-demo
```

**Acceptance:**

- 1280x720 默认截图能分辨地形、机甲、建筑、敌我和主要战斗状态。
- `hangar-contact` 不退化成模型堆叠。
- `damage-demo` 仍然能讲部位损伤故事。
- UI 不靠解释就显得克制。

**Commit:** `Refresh first map visual slice`

### Task 7: Stabilize Reference Visual Manifest

**Status:** Completed 2026-06-07. B1 exporter manifest, B2 Unity loader hardening, and B3 replaceable visual id documentation are done; public art-safe replacement remains Task 8/D1.

**Goal:** 开发期继续借助本地参考素材验证比例和节奏，但加载边界要可替换、可缺失回退、可审计。

**Files:**

- Modify: `scripts/content-pack/export_tgl_to_obj.py`
- Modify: `scripts/content-pack/export_reference_visual_pack.ps1`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceObjMeshLibrary.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/ReferencePropLibrary.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceTerrainTextureLibrary.cs`
- Modify: `docs-reference-visual-restoration-plan.md`
- Modify: `docs-content-replacement-plan.md`

**Steps:**

1. Finish exporter-side manifest work:
   - asset id;
   - source path;
   - generated OBJ/TGA paths;
   - asset class;
   - private-development-only provenance;
   - vertex/triangle/node counts;
   - section/helper node buckets when known;
   - material and texture ids.
2. Finish missing-source behavior so absent TGL/texture material yields clear warnings and ignored manifests, not broken docs.
3. Ensure Unity loader reads manifest first and logs chosen asset id per unit/prop/terrain texture.
4. Keep direct folder probing as fallback only.
5. Missing manifest or missing private files must fall back to obvious development visuals, not crash.
6. Keep all generated source derivatives ignored.
7. Run build and one visual capture after loader hardening.
8. Update docs to state this is private development-only.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-reference-visual-manifest.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,hangar-contact
```

**Acceptance:**

- Private reference visual loading is manifest-driven.
- Missing assets do not break the demo.
- No original-derived generated assets are staged.
- Public replacement pack can later reuse the same visual ids.

**Commit:** `Stabilize reference visual manifest`

### Task 8: Prepare Public Art-Safe Mission Slice

**Status:** Completed 2026-06-07 for metadata. The manifest and docs prove a clean replacement target; a mountable clean runtime pack remains later work.

**Goal:** 从 text-safe metadata 进入第一张图的 art-safe 替换路径，给公开展示和融资材料留干净出口。

**Files:**

- Modify: `docs-content-replacement-plan.md`
- Modify: `docs-content-pack.md`
- Modify: `README.md`
- Create or modify: `content-packs/project-owned-art-safe-slice.example.json`
- Modify if needed: `scripts/content-pack/check_public_content_boundary.ps1`
- Modify if needed: `scripts/content-pack/validate_content_pack.ps1`

**Steps:**

1. Define one mission slice only:
   - product title and visible UI text;
   - one terrain material set;
   - 3-4 mech silhouettes;
   - common weapon FX;
   - 3-5 structures/props;
   - MechLab and status icons.
2. Record provenance for every replacement asset:
   - source;
   - author/generation method;
   - license;
   - allowed use;
   - date;
   - notes.
3. Keep reference-linked development pack private.
4. Boundary check clean manifest.
5. Do not claim runtime art-safe until actual mountable assets exist.

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\content-pack\check_public_content_boundary.ps1 -Path "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\content-packs\project-owned-art-safe-slice.example.json" -DryRun
```

**Acceptance:**

- One mission has a clear path from local reference validation to public-safe assets.
- Public-facing docs do not imply private reference content is final product content.
- Public boundary check returns `Result: OK` for the clean candidate manifest.

**Commit:** `Prepare public art-safe mission slice`

### Task 9: Guard AI Deputy Regression

**Status:** Completed 2026-06-07.

**Goal:** AI 副官保持“小而稳”：大决策、慢频率、可离线，不进入帧级战斗。

**Files:**

- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/RuleCommander.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/MiniMaxCommander.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `docs-ai-commander-directive-contract.md`

**Steps:**

1. Keep compact observation bounded.
2. Keep directive tokens high-level:
   - `assault-objective`;
   - `engage-hostiles`;
   - `regroup`;
   - `withdraw-if-critical`;
   - `hold`.
3. Guard no-key and timeout fallback.
4. Ensure no model calls from draw/update loops.
5. Keep smoke tests token-free.

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

**Completed Evidence:**

```text
RuleCommander now supports withdraw-if-critical as a high-level directive mapped to deterministic local regroup/objective commands.
MiniMax prompt/extraction now accepts assault-objective, engage-hostiles, regroup, withdraw-if-critical and hold.
analysis-output/unity-validate-ai-deputy-offline.log: MC2 demo contract validation OK.
analysis-output/unity-build-ai-deputy-offline.log: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
analysis-output/unity-player-ai-deputy-offline.log: MC2 AI deputy window assertion OK: state=Offline mode=Local fallback intent=assault-objective advice=Advance objective.
analysis-output/unity-player-ai-deputy-offline.log: MC2 demo smoke test exiting with code 0.
```

**Commit:** `Guard AI deputy regression`

### Task 10: Document Platform Reward Contracts

**Status:** Completed 2026-06-07.

**Goal:** 把“合作方或自己可以搭地图服务器，但奖励必须由主服务器认证”的架构写成可执行契约。

**Files:**

- Modify: `docs-platform-ecosystem-plan.md`
- Create if needed: `docs-platform-reward-contract-2026-06-07.md`

**Steps:**

1. Define actors:
   - Unity client;
   - main server;
   - map server;
   - map editor;
   - Web ranking;
   - optional chain layer.
2. Define reward claim as a claim, not a grant:
   - account id;
   - signed squad loadout hash;
   - map id/version;
   - session id;
   - deterministic seed;
   - battle summary;
   - timeline digest.
3. Define validation ladder:
   - local demo;
   - trusted official server;
   - certified partner server;
   - high-value replay validation;
   - competitive/prize events.
4. Define what map server can and cannot do.
5. Keep economy off-chain and centralized first.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- Map server cannot mint portable rewards.
- Main server owns inventory, token ledger, reward tables and rankings.
- BattleCore remains reusable for validation.

**Completed Evidence:**

```text
docs-platform-reward-contract-2026-06-07.md defines actors, reward lifecycle, session ticket, reward claim payload, validation gates, claim states, grant calculation, ledger rules, rejection/capping examples, ranking publication and first implementation slice.
docs-platform-ecosystem-plan.md links to the detailed reward authority contract.
README.md lists the reward authority contract under key docs.
```

**Commit:** `Document platform reward contracts`

### Task 11: Plan Map Authoring Prototype

**Status:** Deferred.

**Goal:** 为后续开源地图编辑器和社区地图包设计最小契约。

**Files:**

- Modify: `docs-platform-ecosystem-plan.md`
- Create if needed: `docs-map-authoring-contract-2026-06-07.md`

**Steps:**

1. Define map package fields:
   - map id/version/title/author/license/provenance;
   - terrain and navigation metadata;
   - structures, props, turrets, cover and water;
   - objectives and trigger graph;
   - enemy waves and patrols;
   - allowed squad size;
   - difficulty estimate;
   - reward table reference, not direct reward grant;
   - BattleCore compatibility version.
2. Define certification states:
   - Draft;
   - Uncertified Public;
   - Certified;
   - Event;
   - Retired.
3. Define local validator checks:
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

### Task 12: Plan Web Ranking Prototype

**Status:** Deferred.

**Goal:** 给未来 Web 展示排行、地图成绩、队伍资料和战斗记录留契约。

**Files:**

- Create if needed: `docs-web-ranking-plan-2026-06-07.md`
- Modify: `docs-platform-ecosystem-plan.md`

**Steps:**

1. Define public pages:
   - season leaderboard;
   - map ranking;
   - player public profile;
   - squad loadout snapshot;
   - battle record detail;
   - creator/map author profile.
2. Define shown verified data:
   - certified map id/version;
   - clear result;
   - time/damage/loss summary;
   - squad hash and public unit names;
   - reward summary;
   - replay availability flag.
3. Define privacy boundary:
   - no private account identifiers;
   - no raw API keys;
   - no unpublished inventory;
   - no exact anti-cheat internals.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- Web plan supports ranking and investment story.
- It does not force server work before local demo quality.

**Commit:** `Plan web ranking prototype`

### Task 13: Plan Creator Economy Boundary

**Status:** Deferred.

**Goal:** 将地图贡献、皮肤、自定义、收入分配和可选链上实验放在正确阶段，避免过早绑死核心游戏。

**Files:**

- Modify: `docs-platform-ecosystem-plan.md`
- Create if needed: `docs-creator-economy-boundary-2026-06-07.md`

**Steps:**

1. Define centralized ledger first:
   - token ledger;
   - inventory ledger;
   - creator revenue accounting;
   - refund/rollback/moderation controls.
2. Define creator contribution:
   - maps;
   - skins;
   - event campaigns;
   - hosted map server capacity;
   - curated challenge ladders.
3. Define optional chain layer later only for:
   - proof of revenue share;
   - transparent creator pools;
   - cosmetic ownership proof;
   - commemorative items.
4. Do not put core combat, mech stats, weapon stats, repair costs, ordinary inventory mutation or normal battle outcomes on chain in the first platform version.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- Chain is optional and late.
- Core gameplay remains deterministic and locally testable.

**Commit:** `Plan creator economy boundary`

## 7. Milestones

| Milestone | Status | Exit Gate |
| --- | --- | --- |
| M0 Project framing | Done | README emphasizes AI-assisted tactical RTS, not old-game clone |
| M1 Windows local runnable | Done | build + smoke + shortcut path exists |
| M2 First mission gameplay | Done for V1 | command loop, jet, debrief, repair, relaunch work |
| M3 MechLab core fun | Done for V1, polish later | grid blocks, legal states, loadout battle effect proved |
| M4 Damage story | Done for V1, polish later | damage-demo shows weapon families and section consequences |
| M5 Sparse HUD | Done with guard | regression guard added and capture sidecar proves it |
| M6 Visual/collision readability | Done with gate | first map no longer reads as color blocks or one-point pile |
| M7 Private reference bridge | Done | manifest-driven, optional, ignored, replaceable |
| M8 Controlled demo evidence | Done | visible-flow, six captures, walkthrough and investor evidence refreshed |
| M9 Public-safe slice | Done for metadata | art-safe manifest/provenance and boundary check pass |
| M10 AI deputy V1 | Done | offline/no-key and high-level directive guarded |
| M11 Platform contracts | In Progress | main server reward authority done; map authoring, ranking and creator boundaries remain |

## 8. First Controlled Demo Definition Of Done

The Demo is ready for controlled external showing when:

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
12. README describes AI-assisted tactical RTS exploration.
13. Current dev reference build is clearly marked development-only.
14. At least one clean public text-safe slice passes public boundary check.
15. Art-safe mission slice has a written path and provenance requirements.

## 9. Stop Conditions

Stop and reassess if:

- `git status --short` shows unrelated user/source changes in files planned for editing.
- Unity scene fileID churn appears without intentional scene changes.
- Validator fails on movement, occupancy, damage, objective, repair, loadout or AI behavior.
- `hangar-contact`, `damage-demo`, `spawn` or `mechlab` captures become less readable than current evidence.
- Unit, building, prop or water collision exists only in Unity with no BattleCore evidence.
- AI code starts making per-frame, per-shot or per-dodge decisions.
- Normal battle UI shows save slots, account management, debug panels or too much text.
- Public-facing docs pitch the project as a clone instead of AI-assisted tactical RTS exploration.
- Public build path contains private reference assets, local paths, old names or development-only manifests.
- Any original-derived asset or generated derivative is about to be staged.

## 10. One-Line Direction

Windows 本地 Demo 的画面、碰撞、稀疏 UI、MechLab、损伤故事、演示证据、公开 art-safe 元数据合同、AI 副官离线边界和主服务器奖励权威契约已经收稳；下一步写地图包/编辑器契约；随后再做 Web 排行和创作者生态。

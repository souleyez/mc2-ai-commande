# AI RTS Commander Current Detailed Implementation Plan

> **For Codex:** REQUIRED SUB-SKILL: Use executing-plans to implement this plan task-by-task.

**Goal:** 把当前 Unity 6 Windows 本地原型推进到一版能稳定演示、能截图说明、能继续融资沟通的 AI 副官机甲战术 RTS Demo。

**Architecture:** `BattleCore` 继续做确定性规则核心，负责移动、喷射、占位、武器、热量、装甲、部位损伤、任务触发、结算和 AI observation/directive。Unity 6 只负责固定镜头、输入、稀疏 HUD、MechLab、模型材质、特效、截图和 smoke 证据。开发期可以使用本机私有参考素材验证比例和节奏，但公开版本必须切换到项目自有或合规授权内容包。

**Tech Stack:** Unity 6, C#, Windows Standalone first, deterministic BattleCore, PowerShell build/smoke/capture scripts, replaceable content packs, optional high-level AI deputy, later main server/map server/Web ranking contracts.

**Revision:** 2026-06-07 v2. This file is the fine-grained execution plan paired with `docs-ai-rts-commander-current-master-plan-2026-06-07.md`.

---

## 0. Current Snapshot

当前项目不是从零开始。已经完成的基础包括：

| Area | Status | Evidence |
| --- | --- | --- |
| Windows 本地构建 | 已可 batch build | `analysis-output/unity-build-*.log` 多轮成功 |
| 任务 Demo | `mc2_01` 可加载地形、单位、结构、目标、触发 | Unity validator 通过 |
| 指挥操作 | 默认全队、状态栏单选、独立命令、自动归队、喷射、集火 | visible-flow smoke 通过 |
| MechLab | 整块武器占格、装甲板、散热器、热量、重量、合法性 | `mechlab` capture sidecar |
| 装配影响战斗 | 合法装配改动能进入 BattleCore combat fields | loadout battle effect smoke |
| 部位损伤 | 断臂、腿瘫、驾驶舱损毁/弹射、残骸故事已有 | `damage-demo` sidecar |
| 稀疏战斗 UI | 大日志、存档、账号、调试覆盖层已被 sidecar 守护 | `SparseBattleUi` gate |
| 物理占位 | 单位、建筑、硬道具、水域、地图边界已有规则证据 | `ContactClearance` gate |
| AI 副官 | observation/directive 基础存在，保持高层慢频率 | no-key/offline-first direction |
| 内容边界 | README 已改成 AI RTS Commander Lab 叙事 | public boundary docs and checker |

当前最重要的问题：

1. 画面仍然有“糊成一坨”的风险，尤其是 `hangar-contact`。
2. 机甲、建筑、树木、地面材质在默认镜头下还不够像一个可信 3D 战场。
3. 私有参考素材的 manifest 和替换边界需要再收稳，避免以后换皮/换包成本过高。
4. 可展示 Demo 还需要一轮从启动、机库、战斗、损伤、结算、重开到截图证据的全链路封口。
5. 平台化方向很清楚，但现在只能写契约，不应该先写服务器。

## 1. Execution Rules

当用户说“按计划继续”时，按下面顺序执行：

1. 先看本文件 `## 4. Fine-Grained Commit Queue`。
2. 只执行第一个 `Next` 或 `In Progress` 任务。
3. 每次只做一个小提交，尽量 1 到 4 个源文件加必要文档。
4. 规则变化先进入 BattleCore，Unity 只消费规则结果。
5. 视觉变化必须配截图或 sidecar 证据。
6. 不提交 `analysis-output/`、PNG、JSON、log、Unity build output、私有参考导出。
7. Unity batch 运行后检查 `unity-mc2-demo/Assets/Scenes/Mc2Demo.unity`，仅 fileID churn 不入库。
8. 不扩大第一版范围，不做实时 PVP、移动端、账号、充值、链上、地图服务器实装。
9. 公共文案只讲本项目自己的 AI 副官战术 RTS 探索。
10. 原始或原始派生素材只能当本机开发验证材料，不进入公开发布路径。

## 2. Product Lock

### 2.1 First Demo Must Show

- Windows 本地启动一局。
- MechLab 装配前置流程：机甲、武器、装甲板、散热器、热量、重量、格子合法性。
- 一张固定视角 3D 任务图：地形、水域、岸线、跑道/道路、建筑、树木/道具、敌我单位。
- 默认全队指挥：点地点移动，点目标移动攻击。
- 状态栏单选：点机甲状态行后给独立命令，完成后自动归队。
- 喷射：朝目标方向尝试固定距离位移，非法落点不动。
- 稀疏战斗 UI：机甲状态行、喷射、任务地图/目标、系统/暂停。
- 物理占位：单位、建筑、硬道具、水域、地图边界都有 BattleCore 证据。
- 武器和部位损伤：能看出能量/导弹/弹道/爆炸等武器族，能看出断臂、腿瘫、驾驶舱损毁/弹射。
- 战后简报、一键维修、返回机库、再次出战。
- AI 副官能力窗口：只做高层建议，不拖慢本地战斗。

### 2.2 First Demo Must Not Show

- 实时 PVP。
- 移动端适配。
- 地图服务器、地图编辑器、Web 排行的实装。
- 账号、充值、提现、链上资产。
- 复杂保存槽或保存游戏 UI。
- AI 导演。
- 大模型逐帧控制移动、开火、回避。
- 未清权素材作为公开产品内容。

## 3. Milestone Map

| Milestone | Phase | Current State | Exit Gate |
| --- | --- | --- | --- |
| M0 | 项目叙事与边界 | Done | README 不再围绕旧作描述 |
| M1 | Windows 本地可运行 | Done | build + smoke 通过 |
| M2 | 第一任务玩法闭环 | Done for V1 | command, jet, objective, debrief, repair, relaunch |
| M3 | MechLab 核心乐趣 | Done for V1, polish later | 格子、装配合法性、战斗影响均有证据 |
| M4 | 部位损伤卖点 | Done for V1, polish later | `damage-demo` 有断臂、腿、驾驶舱故事 |
| M5 | 稀疏 UI | Done with guard | `SparseBattleUi` sidecar gate |
| M6 | 3D 地图视觉与占位 | Active | 五张战斗截图能读清战场，不像色块 |
| M7 | 私有参考视觉包稳定化 | Next | manifest-driven, missing-safe, replaceable |
| M8 | 可展示 Demo 封口 | Next | 六截图、visible-flow、walkthrough、一页证据 |
| M9 | Public art-safe slice | Later | 替换包 provenance 和 boundary check |
| M10 | AI 副官守护 | Later | no-token smoke, high-level directive only |
| M11 | 平台契约 | Later | 地图服务器、奖励认证、排行、创作者边界文档 |

## 4. Fine-Grained Commit Queue

| Order | Status | Commit | Purpose | Gate |
| --- | --- | --- | --- | --- |
| A0 | Done by this doc | `Refresh detailed master plan` | 收束当前阶段、缺口、执行批次 | `git diff --check` |
| A1 | Done | `Audit first map visual readability` | 固化五张截图的视觉问题分类和 sidecar 门槛 | screenshot review + docs |
| A2 | Done | `Improve terrain and water readability` | 地面、水域、岸线、跑道/道路不再像一整块平色 | build + `spawn,airfield,north-patrol` captures |
| A3 | Done | `Improve unit silhouette readability` | 敌我机甲/车辆在默认镜头下更容易分辨 | build + `spawn,hangar-contact,damage-demo` captures |
| A4 | Done | `Improve structure and prop readability` | 建筑、树木、机场道具不再是灰色糊团 | build + `airfield,hangar-contact,north-patrol` captures |
| A5 | Next | `Gate first map visual slice` | sidecar 检查第一图视觉、稀疏 UI、碰撞不回退 | capture script gates |
| A6 | Next | `Refresh demo evidence after visual pass` | 更新证据页和审计文档 | six captures + docs |
| B1 | Later | `Stabilize reference visual manifest export` | 私有参考单位/道具/地形资源导出 manifest | exporter dry run + build |
| B2 | Later | `Harden Unity reference visual loader` | Unity 优先读 manifest，缺失安全回退 | build + fallback capture |
| B3 | Later | `Document replaceable visual ids` | 固化换包 id，方便以后整包替换 | docs + boundary check |
| C1 | Later | `Seal visible playable walkthrough` | 启动、机库、战斗、损伤、结算、重开完整流程 | visible-flow smoke |
| C2 | Later | `Refresh investor evidence package` | 更新本地演示证据，不提交生成截图 | six captures + docs |
| D1 | Later | `Prepare public art-safe mission slice` | 从 text-safe 进入公开视觉替换包计划 | boundary check |
| E1 | Later | `Guard AI deputy offline behavior` | AI 高层、可离线、不逐帧、不花 smoke token | validator |
| F1 | Later | `Document reward authority contract` | 主服务器认证奖励，地图服务器只提交 claim | docs |
| F2 | Later | `Document map authoring contract` | 开源地图编辑器和地图包最小契约 | docs |
| F3 | Later | `Document web ranking contract` | Web 排行、战绩、地图页和隐私边界 | docs |
| F4 | Later | `Document creator economy boundary` | 皮肤、地图、分成、可选链上边界 | docs |

## 5. Detailed Execution Tasks

### A1: Audit First Map Visual Readability

**Goal:** 先用证据说清楚“糊成一坨”具体糊在哪里，避免盲目加效果。

**Files:**

- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify if needed: `docs-playable-demo-investor-evidence-2026-06-07.md`

**Steps:**

1. Open current captures:
   - `analysis-output/reference-visual-captures/spawn.png`
   - `analysis-output/reference-visual-captures/airfield.png`
   - `analysis-output/reference-visual-captures/hangar-contact.png`
   - `analysis-output/reference-visual-captures/north-patrol.png`
   - `analysis-output/reference-visual-captures/damage-demo.png`
2. For each screenshot classify:
   - terrain too flat;
   - water/shoreline weak;
   - mech silhouette too small;
   - enemy/friendly colors unclear;
   - structure/prop gray mush;
   - trees too same-color;
   - black occluders too hard;
   - effects stealing focus;
   - UI occlusion.
3. Read the matching `.json` sidecar and record:
   - `SparseBattleUi`;
   - `BattleOccupancy`;
   - `ContactSpread`;
   - `ContactClearance`;
   - `DamageStory` when present.
4. Decide which problem is visual-only and which would require BattleCore changes.
5. Do not touch gameplay code in this task.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- The audit names the top 3 visual causes.
- The next visual task has a precise target.
- No generated screenshot or sidecar is staged.

**Commit:** `Audit first map visual readability`

### A2: Improve Terrain And Water Readability

**Goal:** 让第一图的地表、水域、岸线、跑道/道路在默认镜头下能读成“地图”，不是一块暗色底板。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/DemoTerrainView.cs`
- Modify: `scripts/unity/capture_reference_visuals.ps1`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Implementation Notes:**

- 优先调材质、vertex color、texture blend、water alpha、shore color 和 runway/road contrast。
- 不改变任务坐标、不改变碰撞、不改变相机旋转。
- 不把地面做成单色高饱和，不用大片装饰渐变。
- 水域需要可识别，但不能亮到抢过机甲。
- 岸线和跑道是第一图最重要的地图阅读线索。

**Steps:**

1. Inspect terrain material creation in `Mc2DemoBootstrap`.
2. Add a compact terrain readability summary to capture sidecar if absent.
3. Tune terrain color/texture strength conservatively.
4. Tune water material color/alpha and shore contrast.
5. Keep click and pathfinding untouched.
6. Build Windows player.
7. Capture `spawn,airfield,north-patrol`.
8. Compare screenshots against A1 audit.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-terrain-readability.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,north-patrol
```

**Acceptance:**

- `spawn` 能区分陆地、水域、岸线和主要地表区域。
- `airfield` 能看出跑道/道路和机场结构关系。
- `north-patrol` 的水域不再像一大片平蓝色或黑蓝色。
- `SparseBattleUi` gate 不回退。

**Commit:** `Improve terrain and water readability`

### A3: Improve Unit Silhouette Readability

**Goal:** 让玩家一眼知道哪些是自己的机甲、哪些是敌方单位、哪些是残骸/损伤单位。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceObjMeshLibrary.cs`
- Modify: `scripts/unity/capture_reference_visuals.ps1`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Implementation Notes:**

- 优先加低调的 ground contact shadow、敌我识别环、team tint 微调。
- 不用大字标签盖在单位上。
- 不靠扩大 UI 解释单位状态。
- 不用 presentation-only offset 假装解决碰撞。
- 单位缩放如要调整，必须同步审计 sidecar 文案，避免和碰撞占位认知冲突。

**Steps:**

1. Inspect `DemoUnitView` reference visual attach and damage cue layers.
2. Add persistent contact shadow under unit.
3. Add low-alpha faction ring or equivalent cue under unit.
4. Ensure destroyed/damaged cues remain readable.
5. Keep section damage cue stronger than faction ring, weaker than active explosion.
6. Build Windows player.
7. Capture `spawn,hangar-contact,damage-demo`.
8. Verify `ContactClearance` still reports `overlaps=0`.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-unit-readability.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,hangar-contact,damage-demo
```

**Acceptance:**

- `spawn` 中玩家小队不用解释也能看出是三台机甲。
- `hangar-contact` 中敌我不会完全融入建筑和特效。
- `damage-demo` 仍能讲断臂、腿瘫、驾驶舱损毁故事。
- 不新增大 HUD 文本。

**Commit:** `Improve unit silhouette readability`

### A4: Improve Structure And Prop Readability

**Goal:** 让建筑、树木、机场道具和目标结构读成地图元素，而不是灰色噪声或黑块。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoStructureView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/ReferencePropLibrary.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Implementation Notes:**

- 优先做结构底座阴影、目标结构轻微高亮、树木/硬道具色阶差异。
- 大型硬道具应继续由 BattleCore occupancy 证明，不靠视觉膨胀。
- 不把所有建筑涂成同一种灰。
- 树木可以作为背景层，但不能把单位吞掉。

**Steps:**

1. Inspect structure and terrain-object creation paths.
2. Add subtle base pad/contact shadow for targetable structures.
3. Tune fallback building/tree/prop colors for contrast.
4. Keep targetable hangar visual distinct but not UI-like.
5. Build Windows player.
6. Capture `airfield,hangar-contact,north-patrol`.
7. Update audit with before/after visual judgment.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-structure-prop-readability.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets airfield,hangar-contact,north-patrol
```

**Completed Evidence:**

```text
analysis-output/unity-build-structure-prop-readability.log: Build Finished, Result: Success; MC2 Unity demo Windows build OK.
capture_reference_visuals.ps1 -Presets airfield,hangar-contact,north-patrol: MC2 reference visual captures passed: 3 preset(s).
StructureReadability: structures=1 structureViews=1 targetable=1 terrainProps=999 hardProps=80 building=32 aircraft=4 vehicle=12 barricade=108 treeObjects=956 visualOnly=yes collision=unchanged blockerGeometry=unchanged.
```

**Acceptance:**

- `airfield` 能看出机场、跑道/道路、建筑群和敌方方向。
- `hangar-contact` 能看出目标建筑的空间位置。
- 大面积树木不再把单位和建筑吞成一团。
- `BattleOccupancy` blocker counts 不回退。

**Commit:** `Improve structure and prop readability`

### A5: Gate First Map Visual Slice

**Goal:** 把“第一图可读”做成脚本门槛，后续 UI、模型、特效改动不能悄悄退回色块状态。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `scripts/unity/capture_reference_visuals.ps1`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Add or strengthen sidecar summary fields:
   - `FirstMapVisual`;
   - `TerrainReadability`;
   - `UnitReadability`;
   - `StructureReadability`.
2. In capture script, assert battle presets still include:
   - nonblank image;
   - sufficient unique colors;
   - sparse battle UI;
   - contact clearance;
   - battle occupancy;
   - first map visual summary.
3. Keep script checks textual and stable; do not overfit to exact PNG bytes.
4. Capture five battle presets.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-first-map-visual-gate.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,north-patrol,damage-demo
```

**Acceptance:**

- 五个 battle presets 通过 capture gate。
- `hangar-contact` 不退化成堆叠。
- `damage-demo` 不丢失损伤故事。
- 默认战斗 UI 仍然稀疏。

**Commit:** `Gate first map visual slice`

### A6: Refresh Demo Evidence After Visual Pass

**Goal:** 在视觉切片稳定后，更新对外沟通用的本地证据页。

**Files:**

- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify: `docs-playable-demo-investor-evidence-2026-06-07.md`
- Modify if needed: `README.md`

**Steps:**

1. Run full capture set:
   - `mechlab`;
   - `spawn`;
   - `airfield`;
   - `hangar-contact`;
   - `damage-demo`;
   - `north-patrol`.
2. Summarize what each screenshot proves.
3. State honestly which parts remain prototype-only.
4. Keep generated screenshots ignored.
5. Update demo talk track if visual quality improved enough.

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol
```

**Acceptance:**

- Evidence page matches current screenshots and sidecars.
- It does not imply private reference art is public-safe.
- It gives a clean three-minute demo order.

**Commit:** `Refresh demo evidence after visual pass`

### B1: Stabilize Reference Visual Manifest Export

**Goal:** 把本地参考模型、贴图、道具、地形纹理的导出结果整理成可审计 manifest，未来整包替换更容易。

**Files:**

- Modify: `scripts/content-pack/export_tgl_to_obj.py`
- Modify: `scripts/content-pack/export_reference_visual_pack.ps1`
- Modify if needed: `scripts/content-pack/export_terrain_texture_audit.ps1`
- Modify: `docs-reference-visual-restoration-plan.md`

**Manifest Should Record:**

- asset id;
- source path;
- generated ignored path;
- asset class: unit, prop, terrain, texture;
- vertex/triangle/node counts;
- material and texture ids;
- helper/node buckets if known;
- provenance note: private-development-only.

**Validation:**

```powershell
git diff --check
$env:PYTHONDONTWRITEBYTECODE='1'; python -m py_compile scripts/content-pack/export_tgl_to_obj.py
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\content-pack\export_reference_visual_pack.ps1 -Names werewolf,bushwacker,centipede,harasser,lrmc,urbanmech,starslayer
```

**Acceptance:**

- Ignored manifest is generated.
- No generated private derivatives are staged.
- Missing source material yields clear warnings, not broken docs.

**Commit:** `Stabilize reference visual manifest export`

### B2: Harden Unity Reference Visual Loader

**Goal:** Unity 读取 manifest 优先，缺失时能回退到开发占位，不因为本地私有素材缺失导致 Demo 无法启动。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceObjMeshLibrary.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/ReferencePropLibrary.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/ReferenceTerrainTextureLibrary.cs`
- Modify: `docs-reference-visual-restoration-plan.md`

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-reference-loader.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield
```

**Acceptance:**

- Loader log records selected manifest asset ids.
- Missing manifest still boots with clear fallback visuals.
- Public replacement pack can later reuse the same ids.

**Commit:** `Harden Unity reference visual loader`

### B3: Document Replaceable Visual IDs

**Goal:** 为以后“整包替换”和换皮项目留下稳定 id 体系，不把旧素材路径写死进产品逻辑。

**Files:**

- Modify: `docs-content-pack.md`
- Modify: `docs-content-replacement-plan.md`
- Modify: `docs-reference-visual-restoration-plan.md`
- Create if needed: `content-packs/project-owned-visual-slice.example.json`

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\content-pack\check_public_content_boundary.ps1 -Path "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\content-packs\project-owned-starter.example.json" -DryRun
```

**Acceptance:**

- Visual ids are project-facing, not legacy-name-facing.
- Docs clearly separate private reference pack and public replacement pack.
- Clean starter boundary still returns `Result: OK`.

**Commit:** `Document replaceable visual ids`

### C1: Seal Visible Playable Walkthrough

**Goal:** 确保真实演示时能从机库走到战斗，再到损伤、结算、维修、重开，不靠开发者口头补洞。

**Files:**

- Modify if needed: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-visible-flow-audit.txt`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`
- Modify: `docs-playable-demo-walkthrough-2026-06-07.md`
- Modify: `docs-playable-demo-investor-evidence-2026-06-07.md`

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-visible-flow-seal.log"
```

**Acceptance:**

- Smoke 到达胜利、战报、维修、回机库、再次出战。
- 不出现保存槽、账号面板、大日志。
- 文档能给非开发人员照着演示。

**Commit:** `Seal visible playable walkthrough`

### C2: Refresh Investor Evidence Package

**Goal:** 把当前本地 Demo 的可展示能力整理成一页证据，方便后续融资沟通或内部演示。

**Files:**

- Modify: `docs-playable-demo-investor-evidence-2026-06-07.md`
- Modify: `README.md`

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol
```

**Acceptance:**

- Evidence page states local screenshot paths and what each proves.
- It clearly says public/commercial build needs clean content pack.
- README points to current evidence and plan without old-game framing.

**Commit:** `Refresh investor evidence package`

### D1: Prepare Public Art-Safe Mission Slice

**Goal:** 把第一张图从私有参考验证路线转成公开可替换路线，但不要求这一阶段就做完所有美术。

**Files:**

- Modify: `docs-content-replacement-plan.md`
- Modify: `docs-content-pack.md`
- Create or modify: `content-packs/project-owned-art-safe-slice.example.json`
- Modify if needed: `scripts/content-pack/check_public_content_boundary.ps1`

**Slice Should Define:**

- project title and visible names;
- one mission id not tied to legacy marker;
- terrain material set;
- 3 to 4 mech silhouettes;
- common weapon FX;
- 3 to 5 structures/props;
- MechLab/status icons;
- provenance field for every public asset.

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\content-pack\check_public_content_boundary.ps1 -Path "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\content-packs\project-owned-art-safe-slice.example.json" -DryRun
```

**Acceptance:**

- Clean candidate manifest passes boundary check.
- Docs do not claim the private dev build is public-safe.
- Public replacement work can begin without rewriting loaders.

**Commit:** `Prepare public art-safe mission slice`

### E1: Guard AI Deputy Offline Behavior

**Goal:** 保持 AI 副官是大决策工具，不进入逐帧战斗，不让 API 延迟破坏 Demo。

**Files:**

- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/RuleCommander.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/MiniMaxCommander.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `docs-ai-commander-directive-contract.md`

**Directive Boundary:**

- Allowed:
  - `assault-objective`;
  - `engage-hostiles`;
  - `regroup`;
  - `hold`;
  - `withdraw-if-critical`.
- Not allowed:
  - per-frame steering;
  - per-shot weapon selection;
  - per-unit dodge loops;
  - blocking API calls in battle update;
  - token spending in normal smoke.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-deputy-offline.log"
```

**Acceptance:**

- No-key local demo works.
- AI failure falls back to rule commander or no advice.
- Compact observation remains bounded.

**Commit:** `Guard AI deputy offline behavior`

### F1: Document Reward Authority Contract

**Goal:** 把“地图服务器可以由合作方搭，但奖励必须由主服务器认证”写成契约。

**Files:**

- Modify: `docs-platform-ecosystem-plan.md`
- Create if needed: `docs-platform-reward-contract-2026-06-07.md`

**Contract Should Define:**

- client submits reward claim, not reward grant;
- main server owns inventory, token ledger, reward table and rankings;
- map server can host map/session and submit signed result;
- claim includes account id, squad hash, map id/version, seed, result summary, timeline digest;
- high-value claims can require replay or deterministic re-simulation.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- Map server cannot mint portable rewards.
- Main server can reject or cap suspicious claims.
- Contract does not require chain in first platform version.

**Commit:** `Document reward authority contract`

### F2: Document Map Authoring Contract

**Goal:** 为未来开源地图编辑器和社区地图包定义最小可验证格式。

**Files:**

- Modify: `docs-platform-ecosystem-plan.md`
- Create if needed: `docs-map-authoring-contract-2026-06-07.md`

**Map Package Should Include:**

- map id/version/title/author/license/provenance;
- terrain and navigation metadata;
- structures, props, turrets, cover, water;
- objectives and trigger graph;
- enemy waves and patrols;
- allowed squad size;
- difficulty estimate;
- reward table reference, not direct reward grant;
- BattleCore compatibility version.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- Community maps can be open and editable.
- Portable rewards remain certified by main server.
- Validator requirements are clear enough to implement later.

**Commit:** `Document map authoring contract`

### F3: Document Web Ranking Contract

**Goal:** 规划 Web 侧战绩、排行、地图页、队伍资料和公开展示边界。

**Files:**

- Modify: `docs-platform-ecosystem-plan.md`
- Create if needed: `docs-web-ranking-plan-2026-06-07.md`

**Public Pages:**

- season leaderboard;
- map ranking;
- player public profile;
- squad loadout snapshot;
- battle record detail;
- creator/map author profile.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- Web plan supports ranking and investment story.
- It does not expose private account ids, API keys, unpublished inventory or anti-cheat internals.

**Commit:** `Document web ranking contract`

### F4: Document Creator Economy Boundary

**Goal:** 把地图贡献、皮肤、自定义、收入分配和可选链上实验放在正确阶段，避免早期绑死核心玩法。

**Files:**

- Modify: `docs-platform-ecosystem-plan.md`
- Create if needed: `docs-creator-economy-boundary-2026-06-07.md`

**Rules:**

- Centralized ledger first.
- Creator revenue accounting can exist before chain.
- Chain later只适合 proof of revenue share、creator pools、cosmetic ownership proof、commemorative items。
- Core combat、机甲数值、武器数值、维修、普通库存变化和正常战斗结果不放链上。

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- Chain is optional and late.
- Core gameplay remains deterministic and locally testable.

**Commit:** `Document creator economy boundary`

## 6. Validation Matrix

| Change Type | Required Commands |
| --- | --- |
| Docs only | `git diff --check` |
| BattleCore rules | `git diff --check`; Unity validator |
| Unity presentation | `git diff --check`; Windows build; relevant captures |
| MechLab UI | `git diff --check`; Windows build; `mechlab` capture; visible-flow if launch path changed |
| Battle HUD | `git diff --check`; Windows build; visible-flow smoke; `spawn,damage-demo` captures |
| Collision/occupancy | `git diff --check`; Unity validator; `hangar-contact,damage-demo` captures |
| Public content boundary | `git diff --check`; `check_public_content_boundary.ps1 -DryRun` |
| AI deputy | `git diff --check`; Unity validator; no-token/offline path check |

Canonical commands:

```powershell
git diff --check
git status --short --branch --untracked-files=all
```

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-current.log"
```

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-current.log"
```

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-visible-flow-current.log"
```

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol
```

## 7. First Controlled Demo Definition Of Done

Demo 可以给少量外部人看时，必须满足：

1. Windows build passes.
2. Mission validator passes.
3. Visible-flow smoke reaches debrief, repair and relaunch.
4. `mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol` captures pass.
5. `mechlab` shows whole weapon blocks, armor/cooling fillers, heat/weight/grid pressure and no weapon toggle.
6. `spawn` shows sparse battle HUD and commander-follow camera.
7. `airfield` shows terrain, water, runway/road, buildings and first enemy direction.
8. `hangar-contact` shows dense tactical contact with `ContactClearance overlaps=0`.
9. `damage-demo` shows weapon families and section damage story.
10. Normal battle UI does not show save slots, account UI, large combat log or debug overlays.
11. AI deputy remains optional and offline-safe.
12. README and evidence docs do not pitch the project as a clone.
13. Development-only reference content is clearly marked.
14. Public-safe slice has a written replacement path and provenance requirements.

## 8. Stop Conditions

Stop and reassess before committing if:

- `git status --short` shows unrelated user changes in files being edited.
- Unity scene changes are only fileID churn.
- A visual fix creates BattleCore/Unity disagreement about collisions.
- `hangar-contact` or `damage-demo` loses `ContactClearance overlaps=0`.
- Battle UI regrows large log, save UI, account UI or debug overlay.
- A public-facing doc says or implies private reference art is final product art.
- Any private original-derived file is staged.
- AI code starts calling model APIs from update loops or smoke tests.
- A platform task tries to implement server code before local Demo is stable.

## 9. One-Line Direction

先把 Windows 本地 Demo 的第一张 3D 战场、碰撞占位、稀疏 UI、MechLab 和损伤故事收成能演示的闭环；然后把私有参考视觉沉淀为可替换 manifest；再做公开 art-safe 内容包；最后进入 AI 托管、地图服务器、奖励认证、Web 排行和创作者生态。

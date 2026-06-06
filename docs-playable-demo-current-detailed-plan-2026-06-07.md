# Playable Demo Current Detailed Plan Implementation Plan

> **For Codex:** REQUIRED SUB-SKILL: Use executing-plans to implement this plan task-by-task.

**Goal:** 把当前 Unity 6 Windows 原型收成一版能演示、能截图、能继续融资推进的轻量机甲战术指挥 Demo。

**Architecture:** `BattleCore` 是权威战斗层，负责命令、移动、碰撞占位、喷射落点、武器、热量、部位损伤、结算和 AI 可观察状态。Unity Presentation 只负责输入、镜头、UI、模型、材质、特效、截图和玩家可见流程。本地可以用私有参考内容包验证画面和节奏，公开构建必须能切到可替换内容包，不能携带原版资产、文案、商标或专有名称。

**Tech Stack:** Unity 6, C#, Windows Standalone, PowerShell validation/capture scripts, `mc2-unity-demo-contract-v1`, deterministic BattleCore, private local reference content pack, Git/GitHub.

---

## 1. Plan Role

这份文档是 2026-06-07 之后继续开发的主执行计划。它替代旧的线性阶段清单作为日常入口，但不删除旧文档，因为旧文档里还有历史审计、任务背景和验证命令。

优先参考顺序：

1. `docs-playable-demo-current-detailed-plan-2026-06-07.md`: 当前主执行计划。
2. `docs-reference-visual-audit-2026-06-07.md`: 截图证据和可读性回归。
3. `docs-content-replacement-plan.md`: 私有参考包和公开替换包边界。
4. `docs-ai-commander-directive-contract.md`: AI 副官 observation/directive 边界。
5. `docs-platform-ecosystem-plan.md`: 地图服务器、排行、奖励认证和创作者生态的长期方向。

旧计划保留为背景：

- `docs-playable-demo-detailed-execution-plan-2026-06-07.md`
- `docs-playable-demo-master-plan-2026-06-07.md`
- `docs-mc2-detailed-development-plan.md`

## 2. Current Snapshot

日期：2026-06-07。

当前真实状态：项目已经越过“能不能跑”的阶段，进入“完整可见流程审计和体验收口”阶段。很多旧计划里的 D/E/F 工作已经有实现和 validator 证据，不应重复从零做。

已经完成或已有基础：

- Unity 6 Windows build、validator、player smoke、reference capture preset 都可运行。
- `mc2_01` 任务合同可加载地形、目标、触发、单位、结构、terrain objects、导航点和源相机信息。
- 地形从黑块/粉色问题修到可读：绿色地面、蓝色水域、岸线、跑道/道路、建筑基底已经能在截图里看见。
- 私有参考 OBJ/TGA/地形/建筑/树木/损伤节点已经接入一部分，能用于本地验证，不作为公开内容。
- BattleCore 已有单位间、targetable structure、大型 terrain object、水域/地图边界的确定性占位和落点判定。
- 敌方停靠点展开、单位/道具比例审计、固定镜头构图、遮挡淡化、sidecar 证据已建立。
- 默认全队、状态栏单选、单机独立命令、完成后自动归队、喷射部分合法接受都已有 smoke 覆盖。
- 武器命中方向、能量/导弹/弹道表现、命中后果、残骸、部位损伤、断臂、腿部瘫痪、驾驶舱弹射等已有表现基础。
- 机库/装配界面已有格子、整块武器形状、装甲板、散热器、热量、重量、合法性、维修和小队整备基础。
- 战后简报、奖励摘要、维修/回机库/合约入口已有基础。
- Save Game UI 已从第一版玩家流程中隐藏，保留开发诊断能力。
- AI 副官已有 observation、rule commander、MiniMax 接入探索，但第一版只做高层建议窗口，不进入逐帧战斗。

当前主要缺口：

- 缺一条“从启动到改装、出战、指挥、损伤、战后、回机库”的完整可见流程审计。
- 战斗中信息仍可能偏多，需要把 HUD 锁成状态栏 + 喷射 + 任务地图 + 暂停/系统 + 必要目标提示。
- `hangar-contact`、`damage-demo` 等关键截图还需要继续防止模型/特效/道具在一个区域堆成一团。
- 物理碰撞占位需要持续作为 BattleCore 证据维护，不能只靠 Unity 视觉碰撞。
- 装配界面虽然已有格子，但需要进一步接近“原版那种整块占格、简单直观”的手感。
- AI 副官的能力窗口还没和第一版可见流程自然接上。
- 内容包边界需要继续保证：本地开发可用私有参考资源，公开构建可替换、可审计。

## 3. Locked Scope

第一版只做 Windows 本地 Demo。核心是两个画面：

1. 机库装配：机甲、武器、装甲、散热、载重、热量和维修。
2. 地图战斗：固定视角、小队命令、独立命令、喷射、自动交战、部位损伤、战后结算。

第一版不做：

- 实时 PVP。
- 地图服务器。
- 账号经济、充值、提现、链上资产。
- 复杂保存系统。
- AI 导演。
- 大模型逐帧控制战斗。
- 公开发布原版素材、文案、商标、专有名称或任务剧情。

长期方向先留接口：

- 社区/合作方地图服务器。
- 主服务器认证奖励。
- Web 排行和成绩展示。
- 皮肤/地图自定义。
- 创作者收入分配和可选链上证明。

## 4. First Demo Definition

第一版 Demo 的可见流程必须这样走：

1. 启动 Windows Demo。
2. 进入机库或准备界面。
3. 看见 1-6 台机甲，常规 4 台。
4. 选择一台机甲，看见整块武器占格、装甲板、散热器、热量、重量、合法性。
5. 一键出战 `mc2_01` 参考小图。
6. 固定俯视镜头默认跟随排序第一的指挥官机甲。
7. 点击地点，默认全队移动。
8. 点击敌方或目标，全队移动攻击或集火。
9. 点状态栏某台机甲，再点地点或目标，该机甲进入独立命令。
10. 独立命令完成后自动归队，重新接受最新全队命令。
11. 点击喷射，小队按每台机甲自身位置和合法落点单独判定；非法单位不动，合法单位喷射。
12. 战斗中能清楚看见地形、建筑、树木/道具、敌我单位、开火、爆炸、残骸和部位损伤。
13. 断臂、腿部瘫痪、驾驶舱弹射要能在世界和状态栏中被看见。
14. 胜利或失败后进入简洁战报。
15. 一键维修/补给，回机库，再次出战同图验证改装效果。

成功标准：

- 本机启动后 1 分钟内能看到战斗场景。
- 1280x720 截图不靠解释也能分辨地形、机甲、建筑、敌我和战斗状态。
- 机甲和敌人不会明显堆在一个点。
- 玩家只用“点地点/目标、喷射、状态栏选机甲、暂停/系统”就能打一场。
- 机库格子能让人一眼理解“什么放在哪、为什么超重/过热、怎么改”。
- 没有 AI 或模型超时时，本地 Demo 仍能完整运行。

## 5. Validation Bus

每个提交都必须留下证据。没有证据的“看起来可以”不算完成。

| Change Type | Minimum Validation | Extra Validation |
| --- | --- | --- |
| BattleCore rule | Unity validator | targeted command-file smoke |
| Command flow | command-file smoke | reference capture sidecar |
| Unity visual | Windows build | reference capture |
| UI layout | 1280x720 capture | smoke |
| MechLab rule | validator | loadout smoke and battle comparison |
| AI contract | validator or command export | timeout fallback smoke |
| Content boundary | docs + path audit | public build dry-run |
| Docs only | `git diff --check` | `git status --short` |

Standard commands:

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
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-visible-flow-audit.log"
```

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol
```

Known good evidence strings:

- `MC2 demo contract validation OK`
- `Build Finished, Result: Success`
- `MC2 Unity demo Windows build OK`
- `MC2 demo smoke test exiting with code 0`
- `MC2 reference visual captures passed`

## 6. Stage 0: Baseline Audit Before More Feature Work

目标：先知道现在到底可演示到什么程度，再继续做功能。这个阶段只收证据和修明显退化，不扩大战线。

### Task 0.1: Run Full Local Evidence Pass

**Status:** Completed 2026-06-07. Evidence recorded in `docs-reference-visual-audit-2026-06-07.md` under `Stage 0 Baseline Audit Result`.

**Files:**

- Read: `analysis-output/*.log`
- Read: `analysis-output/reference-visual-captures/*.png`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Run `git diff --check`.
2. Run Unity validator.
3. Run Windows build.
4. Run `mc2_01-visible-flow-audit.txt` smoke.
5. Capture `spawn,airfield,hangar-contact,damage-demo,north-patrol`.
6. Record log paths, screenshot paths and sidecar summary in the audit doc.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-baseline-audit.log"
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-baseline-audit.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-visible-flow-audit.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol
```

**Acceptance:**

- Validator, build, smoke and capture all complete.
- Audit doc states which screenshot is currently weakest.
- No code refactor happens in this task.

**Commit:** `Document current playable demo baseline`

### Task 0.2: Verify Scene Churn And Worktree Hygiene

**Status:** Completed 2026-06-07. Unity scene fileID churn was inspected and restored; generated logs/screenshots remain ignored.

**Files:**

- Read: `unity-mc2-demo/Assets/Scenes/Mc2Demo.unity`
- Read: `.gitignore`

**Steps:**

1. Run `git status --short --branch`.
2. If only Unity scene fileID churn appears, inspect the exact diff before keeping it.
3. Do not commit scene churn unless it corresponds to intentional GameObject or component changes.
4. Confirm private reference assets remain ignored.

**Validation:**

```powershell
git status --short --branch
git diff -- unity-mc2-demo/Assets/Scenes/Mc2Demo.unity
git check-ignore -v analysis-output/reference-visual-captures/spawn.png
```

**Acceptance:**

- Worktree is understandable before starting feature work.
- Generated screenshots/logs remain untracked.

**Commit:** docs-only or no commit.

## 7. Stage 1: Visible Flow Lock

目标：把“玩家真的能看见并走完一轮”钉住。这里优先级高于新玩法。

### Task 1.1: Freeze Minimal Battle HUD

**Status:** Completed 2026-06-07. The active battle mission brief now stays compact, the right combat panel is shorter, and validation evidence is recorded in `docs-reference-visual-audit-2026-06-07.md` under `Stage 1.1 Minimal Battle HUD Result`.

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`
- Modify if needed: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-combat-situation.txt`
- Modify if needed: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-visible-flow-audit.txt`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Identify battle-only GUI draw functions in `Mc2DemoBootstrap.cs`.
2. During active battle, keep only:
   - left mech status rows,
   - top compact mode/funds/status strip,
   - jet button,
   - mission map button,
   - pause/system button,
   - compact active objective cue.
3. Hide or collapse verbose combat log/debug panels from normal battle view.
4. Status rows must show:
   - unit name/role,
   - health/structure,
   - section damage short labels,
   - solo/detached command state,
   - ready/jetting/cooling/blocked state.
5. Add or update smoke assertion if current assertions cannot prove compact HUD state.
6. Capture `spawn,hangar-contact,damage-demo` at 1280x720.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-minimal-battle-hud.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-combat-situation.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-minimal-battle-hud.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,hangar-contact,damage-demo
```

**Acceptance:**

- `hangar-contact` and `damage-demo` 主战区没有被大片文字遮住。
- 玩家不用读 debug log 也能看懂队伍状态。
- 状态栏仍支持单机独立命令入口。

**Commit:** `Freeze minimal battle HUD`

### Task 1.2: Build One Complete Visible Flow Smoke

**Files:**

- Modify: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-visible-flow-audit.txt`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Start from current visible-flow script.
2. Ensure it covers:
   - open Mech Lab,
   - launch mission,
   - status row solo command,
   - whole squad command,
   - partial squad jet,
   - contact/fire,
   - complete visible objectives or force debrief,
   - open debrief,
   - return to Mech Lab or contracts.
3. Add assertions only where player-visible state matters.
4. Avoid overfitting to exact floating point positions unless validating landing legality.
5. Record the smoke result in the audit doc.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-visible-flow-audit.log"
```

**Acceptance:**

- Smoke exits with code `0`.
- Script proves the end-to-end player loop rather than a single isolated feature.
- Failure messages are readable enough to guide the next fix.

**Commit:** `Guard visible playable flow`

### Task 1.3: Capture Walkthrough Image Set

**Files:**

- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify if needed: `scripts/unity/capture_reference_visuals.ps1`

**Steps:**

1. Capture `spawn,airfield,hangar-contact,damage-demo,north-patrol`.
2. For each preset, inspect:
   - player squad readability,
   - hostile readability,
   - objective readability,
   - UI occlusion,
   - model/prop pile-up,
   - weapon/damage readability.
3. Update the audit matrix with current pass/fail notes.
4. Pick one “investor screenshot candidate” and one “must fix screenshot”.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol
git diff --check
```

**Acceptance:**

- Audit doc names the best and weakest screenshots.
- Next engineering task is based on screenshot evidence, not guesswork.

**Commit:** `Document visible flow capture baseline`

## 8. Stage 2: Battle Space And Collision Occupancy

目标：继续解决“看图像堆在一起”的问题，同时保持 BattleCore 权威。Unity 可以做碰撞体和辅助显示，但合法性不能只靠 Unity 物理。

### Task 2.1: Audit Occupancy Evidence Against Screenshots

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/DemoTerrainView.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Compare `hangar-contact.json` and `damage-demo.json` sidecar occupancy summary against screenshots.
2. Confirm BattleCore knows about:
   - unit radii,
   - targetable structures,
   - hard terrain objects,
   - water and map-bound landing predicate,
   - squad destination fallback.
3. Add sidecar fields only if current evidence cannot explain a visible pile-up.
4. Do not introduce Unity-only collision as source of truth.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-occupancy-evidence.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets hangar-contact,damage-demo
```

**Acceptance:**

- Every obvious hard object in the selected screenshot either has BattleCore occupancy or is explicitly documented as presentation-only.
- Spray fixes that only move Unity meshes without BattleCore evidence are not accepted.

**Commit:** `Audit battle occupancy evidence`

### Task 2.2: Add Presentation Collision Placeholders For Debug Visibility

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoStructureView.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/ReferencePropLibrary.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`

**Steps:**

1. Add non-authoritative colliders or debug footprint rings to units, target structures and hard props.
2. Keep them disabled or unobtrusive in normal play unless a debug flag is active.
3. Make their radius match BattleCore values closely enough for visual diagnosis.
4. Do not let Unity collision decide move acceptance, jump landing, damage or targeting.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-presentation-collision-placeholders.log"
```

**Acceptance:**

- 开发时能看出单位/建筑/硬物占位。
- 玩家正常画面不被调试圈污染。
- BattleCore 仍是唯一权威。

**Commit:** `Show debug occupancy placeholders`

### Task 2.3: Tune Hangar Encounter Composition Without Reducing Pressure

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/MissionScriptBridge.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Keep original trigger pressure: do not simply remove enemies.
2. If `hangar-contact` still piles up, tune:
   - attack ring slots,
   - infantry parking anchors,
   - structure-adjacent fallback points,
   - active objective focus spacing.
3. Add validator assertions for minimum separation around the dense encounter.
4. Re-capture `hangar-contact` and `damage-demo`.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-hangar-composition.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets hangar-contact,damage-demo
```

**Acceptance:**

- Hostile count can stay high, but unit silhouettes and firing direction are more legible.
- No unit destination sits inside a targetable structure or hard terrain object.

**Commit:** `Tune hangar encounter composition`

## 9. Stage 3: Combat Feel Lock

目标：战斗画面要有机甲味。不要靠 UI 解释“哪里损坏了”，而是世界里也能看出来。

### Task 3.1: Regress Weapon Family Cues

**Files:**

- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatEvent.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatProfile.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Confirm energy, missile and ballistic cues still differ in trace, muzzle, impact and aftermath.
2. Ensure effects start from hardpoints or believable unit centers.
3. Reduce effect noise if it hides unit silhouettes in `damage-demo`.
4. Preserve combat log and smoke assertions for weapon type cues.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-weapon-family-regression.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets damage-demo
```

**Acceptance:**

- 玩家能看出大概是谁在打谁、用的是什么类型火力。
- 特效不会重新把战场糊成一团。

**Commit:** `Regress weapon family cues`

### Task 3.2: Lock Section Damage And Ejection Cues

**Files:**

- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/UnitState.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Confirm sections include cockpit, torso, left arm, right arm and legs.
2. Confirm arm loss, leg collapse and cockpit ejection have separate visible events.
3. Ensure status row section labels match actual BattleCore damage state.
4. Add validator coverage if any cue lacks assertion.
5. Capture `damage-demo`.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-section-damage-lock.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets damage-demo
```

**Acceptance:**

- `damage-demo` 能看见至少一种严重部位损伤事件。
- 驾驶舱弹射不只是日志文字。
- 腿部损伤会影响移动/喷射能力或表现。

**Commit:** `Lock section damage and ejection cues`

### Task 3.3: Confirm Armor Hardness Simplicity

**Files:**

- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/UnitState.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Keep armor plate math as a single overall hardness bonus.
2. Damage should pass through hardness reduction before section damage.
3. Do not add per-location armor plate bookkeeping in first version.
4. Validator must prove armored and unarmored units take different section damage.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-armor-hardness-lock.log"
```

**Acceptance:**

- 装甲计算保持简单。
- 部位损伤仍是卖点。
- 计算成本低，规则容易解释。

**Commit:** `Lock armor hardness damage rule`

## 10. Stage 4: MechLab Feel Lock

目标：装配界面要成为核心乐趣，而不是一堆表格。第一版不需要复杂制造线，但必须让“整块武器占格”简单直观。

### Task 4.1: Remove Remaining Weapon Toggle Semantics

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Search for `enabledWeapons`, `weapon toggle`, `Enable`, `Disable`.
2. Replace player-facing toggle concepts with mounted/unmounted or fitted/unfitted concepts.
3. If internal `enabledWeapons` arrays still exist as implementation detail, either rename them or document why they are not exposed.
4. Keep rule: weapon installed means weapon active.

**Validation:**

```powershell
rg -n "enabledWeapons|weapon.*toggle|toggle.*weapon|Enable|Disable" unity-mc2-demo/Assets/Scripts unity-mc2-demo/Assets/Editor
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-no-weapon-toggle.log"
```

**Acceptance:**

- 玩家界面不再暗示武器可以开关。
- 装上就启用，拆下才不用。

**Commit:** `Remove weapon toggle semantics`

### Task 4.2: Make Grid Blocks More Original-Like

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutContract.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutValidator.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Confirm each weapon has a multi-cell shape where appropriate.
2. Render a weapon as one contiguous block, not independent unrelated cells.
3. Armor plate and heat sink remain single-cell fillers.
4. Conflict, out-of-bounds, overweight and overheat states must be immediate and short.
5. Keep controls simple enough for mobile later: select item, click target cell, small nudge buttons if needed.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-mechlab-grid-blocks.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-loadout-compact.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-mechlab-grid-blocks.log"
```

**Acceptance:**

- 玩家一眼看懂武器占了哪些格子。
- 装甲板/散热器作为“填剩余格子”的用途明确。
- 不出现武器启用/关闭按钮。

**Commit:** `Make MechLab grid blocks explicit`

### Task 4.3: Prove Loadout Changes Battle Behavior

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/UnitState.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify if needed: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-loadout-compact.txt`

**Steps:**

1. Confirm applied loadout changes:
   - weapon range,
   - weapon damage,
   - cooldown,
   - heat per shot,
   - heat dissipation,
   - armor hardness.
2. Add a validator scenario if any value is only displayed but not used.
3. Add smoke evidence that applying a fit changes readiness or combat summary.
4. Keep first version deterministic.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-loadout-battle-effect.log"
```

**Acceptance:**

- 机库不只是外观，配置会进入 BattleCore。
- 过热、火力、射程、装甲至少有一个能在战斗中体现。

**Commit:** `Prove loadout battle effects`

## 11. Stage 5: Debrief And Relaunch Loop

目标：不做复杂保存，但要能自然完成“出战 -> 战报 -> 维修 -> 回机库 -> 再战”。

### Task 5.1: Simplify Debrief Player Flow

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-debrief-summary.txt`

**Steps:**

1. Debrief shows:
   - mission result,
   - completed objectives,
   - player damage,
   - salvage/parts,
   - payout/funds delta,
   - repair affordance.
2. Hide save-slot concepts from normal debrief flow.
3. Provide clear buttons:
   - repair all,
   - back to Mech Lab,
   - contracts/relaunch.
4. Smoke must open debrief and assert visible summary.

**Validation:**

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-debrief-summary.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-debrief-summary.log"
```

**Acceptance:**

- 战后不要求玩家理解保存系统。
- 修复和再战路径直接。

**Commit:** `Simplify debrief player flow`

### Task 5.2: Guard Repair And Relaunch

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/MechBayInventoryContract.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Confirm damaged mechs can be repaired immediately by spending funds.
2. Confirm repair cost never creates waiting timers in first version.
3. Confirm repaired units can relaunch the same mission.
4. Confirm destroyed player mechs are repaired, not permanently deleted.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-repair-relaunch.log"
```

**Acceptance:**

- 维修是一键资金消耗。
- 第一版没有等待维修、复杂存档或永久毁机。

**Commit:** `Guard repair and relaunch loop`

## 12. Stage 6: AI Commander Capability Window

目标：AI 做大决策和初稿，不控制逐帧战斗。模型延迟高也不能拖死本地体验。

### Task 6.1: Freeze Compact Observation

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs`
- Modify: `docs-ai-commander-directive-contract.md`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`

**Steps:**

1. Observation contains only:
   - mission phase,
   - objective summary,
   - player unit summary,
   - detached command state,
   - enemy pressure summary,
   - nearby threats,
   - available high-level intents.
2. Exclude per-frame projectile data and full path graphs.
3. Add command export or validator evidence.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-observation.log"
```

**Acceptance:**

- Observation 小到适合高延迟模型。
- 没有 AI 时本地战斗无影响。

**Commit:** `Freeze AI observation contract`

### Task 6.2: Add Directive Adapter Guard

**Files:**

- Create or Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/AiCommanderDirective.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderCommandPort.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/RuleCommander.cs`
- Modify: `docs-ai-commander-directive-contract.md`

**Steps:**

1. Directive types:
   - attack,
   - focusTarget,
   - defend,
   - regroup,
   - hold,
   - retreat,
   - protectUnit.
2. Adapter converts directive to existing commander commands.
3. Timeout, empty response or invalid directive falls back to local rule commander.
4. AI never mutates `BattleMission` state directly.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-directive-adapter.log"
```

**Acceptance:**

- 一条高层 AI 指令能变成普通游戏命令。
- 模型慢或失败不会阻塞战斗。

**Commit:** `Guard AI directive adapter`

### Task 6.3: Show AI Advice As Optional Capability

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/MiniMaxCommander.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/README.md`

**Steps:**

1. Add a compact optional advice window or panel row.
2. Show one short tactical suggestion, not a chat console.
3. Keep player commands higher priority.
4. If API key is unavailable, show disabled/offline state without breaking Demo.
5. Do not spend tokens by default in normal smoke tests.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-advice-window.log"
```

**Acceptance:**

- AI 像副官能力窗口，不像调试台。
- 默认本地 Demo 不依赖网络。

**Commit:** `Show optional AI advice window`

## 13. Stage 7: Content Boundary And Public Safety

目标：本地能用私有参考包验证，公开仓库和公开构建安全。

### Task 7.1: Document Current Private Reference Use

**Files:**

- Modify: `docs-content-replacement-plan.md`
- Modify: `docs-content-pack.md`
- Modify: `README.md`
- Modify: `unity-mc2-demo/README.md`

**Steps:**

1. State clearly that private reference assets are for local development validation only.
2. State that public release must use project-owned or properly licensed content.
3. Document replaceable pack layers:
   - product identity,
   - UI text,
   - mech/weapon/pilot/faction data,
   - mission scripts,
   - models/textures/effects/audio/icons,
   - provenance records.
4. Avoid product copy that sells the project as a clone.

**Validation:**

```powershell
rg -n "MechCommander|MechWarrior|原版|复刻|reference|private|public" README.md unity-mc2-demo/README.md docs-content-replacement-plan.md docs-content-pack.md
git diff --check
```

**Acceptance:**

- Public-facing README emphasizes AI RTS commander exploration.
- Private reference use is documented as development-only.

**Commit:** `Document private reference content boundary`

### Task 7.2: Add Public Build Safety Check

**Files:**

- Modify: `scripts/unity/*`
- Modify: `.gitignore`
- Modify: `unity-mc2-demo/README.md`

**Steps:**

1. Add or document a build mode that rejects private reference output in public package.
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

## 14. Stage 8: Demo Handoff

目标：整理成别人能启动、能看懂、能演示的 Windows 包。

### Task 8.1: Prepare Repeatable Windows Demo Build

**Files:**

- Modify: `scripts/unity/*`
- Modify: `unity-mc2-demo/README.md`
- Modify: `README.md`
- Optional Create: `docs-demo-walkthrough-2026-06-07.md`

**Steps:**

1. Provide one command to build Windows player.
2. Provide one command to validate.
3. Provide one command to smoke the visible flow.
4. Provide one command to capture reference screenshots.
5. Document optional private reference pack requirement.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-demo-handoff.log"
```

**Acceptance:**

- 不打开 Unity Editor 也能复现构建。
- README 能引导另一个开发者跑起来。

**Commit:** `Prepare repeatable Windows demo build`

### Task 8.2: Write Three-Minute Demo Script

**Files:**

- Create: `docs-demo-walkthrough-2026-06-07.md`
- Modify: `README.md`

**Steps:**

1. Write a 3-minute flow:
   - start,
   - Mech Lab,
   - launch,
   - move,
   - focus target,
   - solo command,
   - jet,
   - damage,
   - debrief,
   - repair,
   - return to Mech Lab.
2. Link local screenshot evidence paths.
3. Keep wording project-owned and AI RTS focused.
4. Do not mention or market any original copyrighted product.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- 另一个人能照着演示。
- 演示话术聚焦 AI 副官和机甲战术，不依赖旧 IP。

**Commit:** `Add playable demo walkthrough`

## 15. Later Platform Plan

这些是后续产品线，不进入第一版 Windows 本地 Demo 主线。

| Later Phase | Start Condition | Scope |
| --- | --- | --- |
| P1 Main server prototype | 本地 Demo 的战斗和机库稳定 | account id, inventory snapshot, token ledger, signed loadout, reward claim, leaderboard |
| P2 Map package/editor loop | BattleCore contract 足够稳定 | map package schema, local editor export, uncertified map play |
| P3 Certified reward maps | 主服务器原型存在 | certification states, session tickets, reward caps, validation |
| P4 Partner map servers | reward validation works | map server protocol, reputation, replay/digest upload |
| P5 Creator economy and optional chain | 经济、退款、审核先在线下验证 | creator revenue accounting, optional Ethereum/L2 proof or settlement |

长期原则：

- 地图可以开放。
- 跨地图带出的奖励必须由主服务器认证。
- 地图服务器可以第三方搭建，但奖励、排行和跨图资产不能完全相信第三方。
- 链上分账只作为后期可选结算层，不作为第一版玩法依赖。

## 16. Recommended Commit Order

从当前点推荐按这个顺序继续：

1. `Document current playable demo baseline`
2. `Freeze minimal battle HUD`
3. `Guard visible playable flow`
4. `Document visible flow capture baseline`
5. `Audit battle occupancy evidence`
6. `Show debug occupancy placeholders`
7. `Tune hangar encounter composition`
8. `Regress weapon family cues`
9. `Lock section damage and ejection cues`
10. `Lock armor hardness damage rule`
11. `Remove weapon toggle semantics`
12. `Make MechLab grid blocks explicit`
13. `Prove loadout battle effects`
14. `Simplify debrief player flow`
15. `Guard repair and relaunch loop`
16. `Freeze AI observation contract`
17. `Guard AI directive adapter`
18. `Show optional AI advice window`
19. `Document private reference content boundary`
20. `Add public build content safety check`
21. `Prepare repeatable Windows demo build`
22. `Add playable demo walkthrough`

每个提交结束时记录：

- 修改文件。
- 跑过的验证命令。
- 日志或截图路径。
- 下一步仍存在的问题。

## 17. Stop Conditions

出现这些情况先停下来修，不继续堆功能：

- `hangar-contact` 或 `damage-demo` 截图比上一轮更糊、更挤或 UI 更挡。
- smoke test 在表现层小改后失败。
- BattleCore 合法落点和 Unity 表现不一致。
- 单位、建筑、terrain object 只在 Unity 里有碰撞，BattleCore 没证据。
- Unity 场景文件只有 fileID churn。
- 第一版流程重新露出复杂保存系统。
- 任务引入服务器、经济、PVP、移动端或链上代码，导致本地 Demo 主线变慢。
- 公开文档开始把私有参考素材描述成产品内容。

当前主线一句话：先把完整可见流程和战场占位钉住，再把机库格子和战斗损伤继续磨顺，最后补 AI 副官能力窗口和公开内容边界。先让第一张图能打、能看、能讲清楚。

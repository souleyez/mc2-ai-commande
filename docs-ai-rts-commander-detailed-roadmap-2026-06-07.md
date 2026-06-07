# AI RTS Commander Detailed Roadmap Implementation Plan

> **For Codex:** REQUIRED SUB-SKILL: Use executing-plans to implement this plan task-by-task.

**Goal:** 把当前 Unity 6 Windows 本地原型继续推进成一版可演示、可融资说明、可公开换包、可扩展到地图服务器和 AI 托管的轻量机甲战术 RTS Demo。

**Architecture:** `BattleCore` 是确定性规则层，负责任务、命令、占位、喷射、交火、热量、装甲硬度、部位损伤、战报、维修、奖励草案和 AI observation/directive。Unity Presentation 只负责固定镜头、输入、HUD、机库、模型、材质、特效、截图和本地构建。开发期可以用私有参考内容包校验比例和手感，公开版本必须切换到项目自有或合规授权内容包。

**Tech Stack:** Unity 6, C#, Windows Standalone first, deterministic BattleCore, PowerShell validator/build/smoke/capture scripts, replaceable content packs, optional high-level AI deputy adapter, future main server/map server/Web ranking.

**Revision:** 2026-06-07 v2. This is the current fine-grained roadmap. Day-to-day execution still starts from `docs-playable-demo-current-execution-plan-2026-06-07.md`.

---

## 0. How To Use This Plan

当用户说“按计划继续”时：

1. 先看 `docs-playable-demo-current-execution-plan-2026-06-07.md` 的 `Current Commit Queue`。
2. 如果队列不明确，就回到本文件的 `## 8. Current Execution Queue`。
3. 每次只做一个小提交，提交必须能用 validator、build、smoke、capture 或 docs check 说明。
4. 当前队列从 `Refresh playable demo evidence` 继续；如果之后又出现未提交 source WIP，先验收它再开新功能。
5. 不提交 `analysis-output/` 下的截图、sidecar JSON、构建日志或 player build，除非用户明确要求打包。
6. Unity 运行后如果只有 scene fileID churn，先手动审查，不随手提交。

## 1. Current State On 2026-06-07

当前项目不是从零开始。真实状态是：

| Area | State | Evidence Or Note |
| --- | --- | --- |
| Windows local demo | 已能 batch build、smoke、capture | `analysis-output/unity-build-*.log`, smoke logs |
| First mission | `mc2_01` 可加载地形、结构、目标、触发、单位、相机 | validator and capture sidecars |
| Terrain readability | 水域、岸线、跑道、地面分区、建筑基底已有可读版本 | reference captures |
| Physical occupancy | 单位、建筑、硬道具、水域、地图边界已有 BattleCore 证据 | `OccupancySummary()` |
| Battle controls | 默认全队、状态栏单选、独立命令、自动归队、喷射已有 smoke | visible-flow script |
| Sparse battle UI | 战斗中只保留状态栏、喷射、目标/地图、系统入口 | capture sidecars |
| Damage story | 断臂、腿瘫、驾驶舱损毁/弹射已有基础 | `damage-demo` |
| MechLab | 整块武器格子、装甲板、散热器、热量、重量、合法性已有基础 | `mechlab` capture |
| AI deputy | compact observation、directive adapter、能力窗口已有基础 | AI validator/smoke |
| Content boundary | README 和脚本已经把私有参考包与公开包分开 | `check_public_content_boundary.ps1` |

V4 crowded contact occupancy 已完成：

| Evidence | Result |
| --- | --- |
| `analysis-output/unity-validate-crowded-contact.log` | Validator OK |
| `analysis-output/unity-build-crowded-contact.log` | Windows build OK |
| `hangar-contact` sidecar | `unitRadii infantry=24 vehicle=54 mech=64`, `ContactSpread=players 3 hostiles 20 nearestPH=272.8 nearestHH=48` |
| `damage-demo` sidecar | `unitRadii infantry=24 vehicle=54 mech=64`, `ContactSpread=players 2 hostiles 20 nearestPH=118 nearestHH=78`, damage story intact |

下一步先刷新完整 Demo 证据，再开公开替换包或新功能。

## 2. Product Direction

第一版证明四件事：

1. **机库改装是乐趣核心。** 武器整块占格，装甲板和散热器填空，热量、重量、槽位合法性一眼看懂。
2. **战斗是指挥游戏。** 玩家不框选、不逐帧微操，只下地点、目标、喷射、单机独立命令。
3. **机甲战斗要有卖点。** 不是普通血条互扣，而是断臂、腿瘫、驾驶舱弹射、残骸和战后维修。
4. **AI 是副官，不是帧级脚本。** AI 做开局计划、风险判断、目标优先级和托管决策，本地规则负责具体战斗。

第一版不做：

- 实时 PVP。
- 移动端适配。
- 地图服务器和地图编辑器。
- 账号、充值、提现、链上资产。
- 复杂保存槽。
- AI 导演。
- 大模型逐帧移动、开火或回避。
- 公开发布私有参考素材、旧文案、旧商标、旧专有名称。

## 3. Architecture Contracts

### 3.1 BattleCore Owns Truth

BattleCore must own:

- mission contract loading;
- objective and trigger state;
- squad and detached command state;
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

Rule: any behavior affecting movement, hit, damage, victory, repair, reward or AI decision must exist in BattleCore or contract data first.

### 3.2 Unity Owns Visibility

Unity Presentation must own:

- click/raycast input;
- fixed tactical camera and limited zoom;
- sparse battle HUD;
- MechLab layout;
- model/material/terrain rendering;
- optional occupancy placeholder review layer;
- weapon trails, impacts, explosions, fragments and ejection cues;
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

Rule: Unity can clarify rule results, but it must not be the only gameplay collision or combat authority.

### 3.3 AI Boundary

AI may:

- read compact observation;
- draft an opening plan;
- choose one high-level directive for a phase;
- show one short advice line;
- support future AI托管 and paper simulation.

AI must not:

- mutate `BattleMission` directly;
- decide every frame, shot or dodge;
- block local battle when model API is slow;
- spend tokens during normal smoke tests;
- become required for the local demo.

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

Visible player flow:

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-visible-flow-audit.log"
```

Reference visual capture:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol,mechlab
```

Public content boundary:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\content-pack\check_public_content_boundary.ps1 -Path "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows" -DryRun
```

Known good strings:

- `MC2 demo contract validation OK`
- `Build Finished, Result: Success`
- `MC2 Unity demo Windows build OK`
- `MC2 demo smoke test exiting with code 0`
- `MC2 reference visual captures passed`

## 5. Milestone Plan

| Milestone | Status | Goal | Exit Gate |
| --- | --- | --- | --- |
| M0 Handoff hygiene | Done | 构建、smoke、walkthrough、证据、内容边界能解释 | H4 gate passed |
| M1 Visual occupancy | Done | 战场不再像模型堆叠，sidecar 有物理证据 | `hangar-contact` readable, V4 passed |
| M2 First controlled demo | Next | 本地 development Demo 可用于小范围展示 | build + smoke + evidence page updated |
| M3 MechLab fun | Next | 装配格子更接近整块占格乐趣 | `mechlab` screenshot and loadout validator |
| M4 Combat damage sell | Next | 武器类型、断臂、腿瘫、弹射更清楚 | `damage-demo` screenshot tells the story |
| M5 Public replacement slice | Next | 至少一张图的文本和可见包开始公开安全 | boundary check OK on clean pack |
| M6 AI deputy V1 | Foundation Done | AI 做高层建议，不拖慢本地战斗 | no-key/offline smoke passes |
| M7 Platform contracts | Deferred | 主服务器、地图服务器、排行、认证奖励 | only after local demo is convincing |

## 6. Detailed Work Packages

### A1: Finish V4 Contact Occupancy WIP

**Status:** Completed 2026-06-07.

**Result:** Unit collision radii increased to `24/54/64`, unit-target range checks now include target footprint, capture sidecars include `ContactSpread`, and the capture script cleans up leftover local player processes after timeout. Validator, Windows build, `hangar-contact`, and `damage-demo` evidence all passed.

**Goal:** 验证当前碰撞半径和 `ContactSpread` sidecar 改动是否真的改善 `hangar-contact`，并确保没有破坏任务规则。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify: `docs-playable-demo-investor-evidence-2026-06-07.md`

**Steps:**

1. Run `git diff --check`.
2. Run mission validator with `unity-validate-crowded-contact.log`.
3. If validator fails, fix the smallest rule or summary issue.
4. Capture `hangar-contact,damage-demo`.
5. Inspect `hangar-contact.json` and `damage-demo.json` for:
   - `unitRadii infantry=24 vehicle=54 mech=64`;
   - `ContactSpread=...`;
   - active and visible hostile counts.
6. Inspect screenshots at 1280x720.
7. Update visual audit with before/after.
8. Commit only source and docs.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-crowded-contact.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets hangar-contact,damage-demo
```

**Acceptance:**

- `hangar-contact` 不再显著同点堆叠。
- `damage-demo` 的部位损伤故事不退化。
- BattleCore summary and sidecar prove unit spread, not just visual rearrangement.

**Commit:** `Polish crowded contact occupancy`

### A2: Add A Close Contact Readability Gate

**Status:** Next after A1 if density is still hard to judge.

**Goal:** 给最拥挤区域增加一个可复现的近景或 sidecar gate，避免每次凭肉眼争论。

**Files:**

- Modify: `scripts/unity/capture_reference_visuals.ps1`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Add or tune a preset around hangar contact.
2. Keep fixed camera rules, only adjust capture preset framing.
3. Record nearest player-hostile, hostile-hostile and player span.
4. Reject blank or sidecar-missing captures.

**Validation:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets hangar-contact
git diff --check
```

**Acceptance:**

- The densest contact can be reviewed without opening Unity Editor.
- The capture explains whether the issue is real overlap, camera compression or visual scale.

**Commit:** `Add crowded contact readability gate`

### B1: Refresh Full Demo Handoff Evidence

**Status:** Next after visual occupancy is stable.

**Goal:** 重新跑一轮完整本地 Demo 证据，证明它能跑、能看、能讲。

**Files:**

- Modify: `docs-playable-demo-investor-evidence-2026-06-07.md`
- Modify: `docs-reference-visual-audit-2026-06-07.md`
- Modify if needed: `docs-playable-demo-walkthrough-2026-06-07.md`

**Steps:**

1. Run validator.
2. Build Windows player.
3. Run visible-flow smoke.
4. Capture `mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol`.
5. Run public boundary check and record that current dev build is development-only unless clean pack exists.
6. Update evidence page with current paths, pass strings and honest limitations.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-demo-refresh.log"
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-demo-refresh.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-demo-refresh.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol
```

**Acceptance:**

- A collaborator can reproduce the demo without knowing the codebase.
- Evidence page clearly separates local dev reference content from public-safe content.

**Commit:** `Refresh playable demo evidence`

### C1: Start Public Replacement Content Slice

**Status:** Next after B1.

**Goal:** 从“本地参考验证”进入“可公开替换包”的第一步，先保证文本和可见名字安全。

**Files:**

- Modify: `docs-content-replacement-plan.md`
- Modify: `docs-content-pack.md`
- Modify or create: `content-packs/project-owned-dev/*`
- Modify if needed: `scripts/content-pack/validate_content_pack.ps1`
- Modify if needed: `scripts/content-pack/check_public_content_boundary.ps1`

**Steps:**

1. Define project-owned visible names for one mission slice.
2. Replace visible mech, weapon, pilot, faction and mission labels in the content pack path.
3. Keep source-derived ids only as internal contract ids if still needed.
4. Add provenance notes for every project-owned replacement.
5. Run content pack validation.
6. Run public boundary check on the clean candidate path.

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\content-pack\check_public_content_boundary.ps1 -Path "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\content-packs\project-owned-starter.example.json" -DryRun
```

**Acceptance:**

- Text-safe slice does not expose old names, private paths or reference-linked manifest notes.
- Dev reference pack remains available locally but is clearly private.

**Commit:** `Open public replacement content slice`

### D1: MechLab Grid Feel Pass

**Status:** Next after content slice or earlier if UI becomes the main demo blocker.

**Goal:** 让机库装配更接近“整块装备放格子”的直观乐趣。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutContract.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutValidator.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`

**Steps:**

1. Search for remaining weapon enable/disable semantics.
2. Ensure mounted weapon means active weapon.
3. Render each multi-cell weapon as one contiguous block.
4. Keep armor plates and heat sinks as single-cell fillers.
5. Show overweight, overheat, occupied and out-of-bounds states in short copy.
6. Capture `mechlab`.

**Validation:**

```powershell
rg -n "enabledWeapons|weapon.*toggle|toggle.*weapon|Enable|Disable" unity-mc2-demo/Assets/Scripts unity-mc2-demo/Assets/Editor
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-mechlab-grid-feel.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets mechlab
```

**Acceptance:**

- 玩家一眼看懂装备占哪些格子。
- UI 不再暗示武器可以启用/关闭。
- Heat/mass/legal state is visible and short.

**Commit:** `Polish MechLab grid feel`

### D2: Prove Loadout Changes Battle

**Status:** After D1.

**Goal:** 机库不只是摆样子，装配必须进入 BattleCore 战斗结果。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/UnitState.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify if needed: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-loadout-compact.txt`

**Steps:**

1. Confirm fitted weapons alter range, damage, cooldown or heat.
2. Confirm armor plates alter overall hardness.
3. Confirm heat sinks alter heat budget or cooling.
4. Add validator assertions comparing two loadouts.
5. Keep first version deterministic.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-loadout-battle-effect.log"
```

**Acceptance:**

- 至少一个装备变化能改变战斗数值或状态。
- Validator 能证明 UI displayed loadout equals BattleCore applied loadout.

**Commit:** `Prove loadout battle effects`

### E1: Weapon And Damage Readability Pass

**Status:** After visual occupancy and MechLab regression.

**Goal:** 强化武器类型、命中方向、断臂、腿瘫和驾驶舱弹射，让机甲战斗有记忆点。

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- Modify: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Separate laser, missile, ballistic and explosion cues enough for screenshots.
2. Keep impact effects short and directional.
3. Ensure arm loss, leg loss and cockpit loss have world cues.
4. Ensure status rows show damage without becoming a large dashboard.
5. Capture `damage-demo`.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-damage-readability.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets damage-demo
```

**Acceptance:**

- `damage-demo` 不看日志也能看出至少一个严重部位损伤故事。
- FX does not hide units, terrain or the sparse HUD.

**Commit:** `Polish weapon and damage readability`

### F1: Keep AI Deputy Small

**Status:** Foundation done, regression guard later.

**Goal:** AI 副官只做大决策，不让模型延迟和 token 成本进入核心战斗循环。

**Files:**

- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/BattleCore/MiniMaxCommander.cs`
- Modify if needed: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `docs-ai-commander-directive-contract.md`

**Steps:**

1. Keep observation compact.
2. Keep directive tokens high level.
3. Keep no-key and timeout fallback local.
4. Do not call model from draw/update loops.
5. Add smoke or validator only when UI or adapter changes.

**Validation:**

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-ai-deputy-regression.log"
```

**Acceptance:**

- AI is optional.
- No-key local demo works.
- Smoke tests do not spend model tokens.

**Commit:** `Guard AI deputy regression`

### G1: Platform Contract Sketch

**Status:** Deferred.

**Goal:** 保留未来地图服务器、主服务器认证奖励、Web 排行和创作者生态方向，但不阻塞第一版 Demo。

**Files:**

- Modify: `docs-platform-ecosystem-plan.md`
- Create later: server contract docs only, no gameplay code yet.

**Steps:**

1. Define map package as content, not authority.
2. Define main server as inventory and reward authority.
3. Define signed battle summary and reward claim shape.
4. Define map ranking and player squad web profile shape.
5. Keep Ethereum optional and late.

**Acceptance:**

- Local BattleCore remains playable offline.
- Future map rewards are certified by main server, not trusted third-party map hosts.

**Commit:** `Document platform reward contracts`

## 7. First Demo Definition Of Done

The first controlled demo is ready when:

1. `git diff --check` passes.
2. Unity mission validator passes.
3. Windows build passes.
4. Visible-flow smoke exits with code `0`.
5. Reference captures are nonblank.
6. `hangar-contact` reads as a tactical contact, not a model pile.
7. `damage-demo` clearly shows a damage story.
8. MechLab grid shows weapon blocks, armor plates, heat sinks, heat, weight and legal/illegal state.
9. Battle UI stays sparse.
10. AI deputy window is optional and offline-safe.
11. Debrief, one-click repair and relaunch work.
12. README and content docs describe project-owned AI RTS exploration.
13. Current dev reference build is clearly marked development-only.
14. A clean public content slice can pass the public boundary check.

## 8. Current Execution Queue

| Order | Status | Commit | Purpose |
| --- | --- | --- | --- |
| 1 | Done | `Polish crowded contact occupancy` | Finish V4, validate BattleCore spacing and sidecar `ContactSpread` |
| 2 | Next | `Refresh playable demo evidence` | Re-run validator, build, smoke and capture after V4 |
| 3 | Next | `Open public replacement content slice` | Start text-safe and provenance-clean public content path |
| 4 | Next | `Polish MechLab grid feel` | Make equipment grid more physical and original-like without toggles |
| 5 | Next | `Prove loadout battle effects` | Prove fitted weapons, armor and cooling affect BattleCore |
| 6 | Next | `Polish weapon and damage readability` | Strengthen weapon families and section damage story |
| 7 | Later | `Guard AI deputy regression` | Keep AI compact, high-level and offline-safe |
| 8 | Later | `Document platform reward contracts` | Prepare map server and reward certification docs after demo |

## 9. Stop Conditions

Stop and reassess if:

- Worktree shows unrelated user/source changes in files planned for editing.
- Unity scene fileID churn appears without intentional scene edits.
- Validator fails on movement, occupancy, damage, objective, repair or loadout behavior.
- `hangar-contact` or `damage-demo` becomes less readable than previous evidence.
- Unit, building or terrain collision exists only in Unity with no BattleCore evidence.
- AI code starts making per-frame, per-shot or per-dodge decisions.
- Normal battle UI shows save slots, account management, debug panels or too much text.
- Public-facing docs sell the project as a clone instead of AI-assisted tactical RTS exploration.
- Public build path contains private reference assets, local paths, old names or development-only manifests.

## 10. One-Line Direction

先刷新完整 Demo 证据；随后开公开替换包，同时继续打磨 MechLab 装配乐趣、部位损伤卖点和可选 AI 副官，平台化和地图服务器等到本地战斗真正好看、好玩、好讲以后再动。

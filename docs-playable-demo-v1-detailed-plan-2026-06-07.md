# Playable Demo V1 Detailed Implementation Plan

> **For Codex:** REQUIRED SUB-SKILL: Use executing-plans to implement this plan task-by-task.

**Goal:** 把当前 Unity 6 Windows 本地原型收成一版能演示、能截图、能讲清楚价值的轻量机甲战术指挥 Demo：机库装配直观，战场可读，玩家用极少操作指挥 1-6 台机甲完成一张任务图，并能看到武器、喷射、物理占位、部位损伤、战后维修和可选 AI 副官建议。

**Architecture:** `BattleCore` 是权威规则层，负责命令、移动、喷射落点、碰撞占位、武器、热量、装甲硬度、部位损伤、战后维修、结算和 AI observation/directive。Unity Presentation 只负责固定镜头、输入、HUD、模型、材质、特效、截图、调试可视化和本地演示。开发期可以使用本地私有参考内容包验证比例、节奏和可读性；公开构建必须能切换到项目自有或合规授权内容包。

**Tech Stack:** Unity 6, C#, Windows Standalone, deterministic BattleCore, PowerShell validator/build/smoke/capture scripts, `mc2-unity-demo-contract-v1`, private local reference content pack, replaceable public content pack, Git/GitHub.

**Revision:** 2026-06-07 v1. This is the detailed product and execution map. The day-to-day queue remains `docs-playable-demo-current-execution-plan-2026-06-07.md`.

---

## 0. How To Use This Plan

When the user says `按计划继续`, use this order:

1. Read `docs-playable-demo-current-execution-plan-2026-06-07.md`.
2. Start with the first `Next` or `In Progress` item in `## 5. Current Commit Queue`.
3. Use this file for deeper acceptance criteria, module boundaries and later-stage order.
4. Make one small, verified commit at a time.
5. Do not stage generated `analysis-output` PNG/JSON/log files unless the user explicitly asks.

Current branch snapshot when this plan was written:

- Branch: `master...ai-origin/master [ahead 57]`.
- Latest completed commits include:
  - `527a6be Improve reference visual readability`
  - `1bd22e2 Lock occupancy placeholder review layer`
  - `74e24bf Polish MechLab block fitting`
  - `6ffa2ea Capture MechLab fitting evidence`
- Current next gameplay task: `P1 Document reference content boundary`.

## 1. Product Definition

First version target:

- Windows local playable Demo only.
- One reference mission map, currently `mc2_01`.
- Core loop: MechLab -> launch mission -> command battle -> damage/debrief -> one-click repair -> relaunch.
- Player controls intent, not every shot.
- AI deputy is optional and high-level; local battle must work offline.

First version must show:

- 1-6 mechs, most often 4, with the first mech as commander camera anchor.
- Fixed tactical camera, limited zoom, no rotation.
- Default all-squad command.
- Status-row single-mech command, treated as detached/solo command.
- Detached unit auto-rejoins after completing its order.
- Jet command: each mech attempts a fixed-distance boost toward the target direction; illegal landing units stay still.
- Sparse battle UI: mech status rows, jet, task map/compact objective, pause/system.
- MechLab block fitting: weapon blocks, armor plates, heat sinks, heat, mass and legal/illegal state.
- Weapons are active when mounted. No weapon enable/disable toggle.
- Simple armor hardness: armor plates increase global hardness, while section damage remains.
- Section damage: arm loss, leg mobility loss, cockpit breach/ejection, destroyed wreck.
- Physical occupancy evidence from BattleCore: units, structures, hard props, water and map bounds.
- Private reference assets can help development validation, but public builds must not depend on them.

First version does not include:

- Realtime PVP.
- Mobile/cross-platform build.
- Complex save slots or save-game UI.
- Map server, account economy, recharge/payment, withdrawal or chain assets.
- AI director.
- Model-driven per-frame or per-shot control.
- Public release of private reference assets, old story text, trademarks or proprietary names.

## 2. Current State

| Module | State | Evidence | Remaining Work |
| --- | --- | --- | --- |
| Unity Windows build | Build and smoke path exists | `analysis-output/unity-build-*.log` | Final handoff command cleanup |
| First map | `mc2_01` terrain, units, structures, objects, objectives and triggers load | validator + capture sidecars | Keep readability from regressing |
| Terrain | Ground, water, shore, roads/runway and building bases are readable | reference captures | Later art polish only |
| Occupancy | Unit, structure, hard terrain object and blocked landing evidence exists | sidecar `occupancyPlaceholders` | Regression only unless a concrete collision bug appears |
| Command loop | Default squad, solo order, auto rejoin, jet and debrief basics exist | visible-flow smoke | Final sparse UI audit |
| MechLab | Block fitting, filler cells and capture evidence exist | `mechlab` preset and smoke | Regression and battle-stat influence checks |
| Damage | Section and ejection foundations exist | `damage-demo` + validator | Needs screenshot-grade damage story |
| AI deputy | Observation, directive adapter and small advice window exist | AI validator/smoke | Keep optional and offline-safe |
| Public boundary | README pitch is no longer clone-oriented | README + content docs | Add guard/check before public package |

## 3. Architecture Boundaries

### 3.1 BattleCore Owns Rules

BattleCore owns:

- mission contract loading;
- objectives and triggers;
- squad command and detached-unit command state;
- auto rejoin;
- movement and jet legality;
- unit, structure, hard prop, water and map-bound occupancy;
- weapon range, cooldown, heat and damage;
- armor hardness;
- section damage, cockpit/ejection and destruction;
- repair and relaunch state;
- AI observation and high-level directive interpretation.

Primary files:

- `unity-mc2-demo/Assets/Scripts/BattleCore/BattleMission.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/CommanderObservationPort.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/RuleCommander.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/MiniMaxCommander.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutContract.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/LoadoutValidator.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/CombatLoadoutPreview.cs`
- `unity-mc2-demo/Assets/Scripts/BattleCore/MechBayInventoryContract.cs`

Rule: if behavior affects movement, damage, victory, loss, repair, reward or AI decisions, it must exist in BattleCore or contract data first.

### 3.2 Unity Presentation Owns Visibility

Unity Presentation owns:

- click/raycast input;
- fixed tactical camera and limited zoom;
- sparse battle HUD;
- MechLab layout;
- model/material/terrain rendering;
- visual blocker/occupancy placeholders;
- weapon trails, impact cues, section damage fragments and ejection cues;
- command-file smoke handling;
- screenshot capture and sidecar summaries.

Primary files:

- `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/DemoTerrainView.cs`
- `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`
- `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- `unity-mc2-demo/Assets/Editor/Mc2DemoBuilder.cs`
- `scripts/unity/capture_reference_visuals.ps1`

Rule: Unity can show collision rings, blocker boxes, silhouettes and sidecar proof, but Unity-only colliders cannot become the only gameplay collision source.

### 3.3 AI Boundary

AI may:

- read compact observation;
- produce one opening plan or high-level directive;
- show one short advice line;
- support future AI托管 and paper simulation.

AI must not:

- mutate `BattleMission` directly;
- drive every frame, shot or dodge;
- choose exact player-facing coordinates;
- block local battle when model API is slow or unavailable;
- spend tokens in smoke tests.

### 3.4 Content Boundary

Development may use local private reference content to validate:

- terrain silhouette;
- mech/vehicle/building/prop scale;
- weapon effect readability;
- damage node placement;
- mission pacing.

Public repo and public builds must use:

- project-owned names and UI text;
- project-owned or licensed models, textures, audio and icons;
- replaceable content pack IDs;
- provenance manifest;
- a public-boundary check before packaging.

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

Visible-flow change:

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-visible-flow-audit.log"
```

Reference visual change:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,airfield,hangar-contact,damage-demo,north-patrol
```

MechLab visual change:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets mechlab
```

Known good strings:

- `MC2 demo contract validation OK`
- `Build Finished, Result: Success`
- `MC2 Unity demo Windows build OK`
- `MC2 demo smoke test exiting with code 0`
- `MC2 reference visual captures passed`

## 5. Current Commit Queue

The authoritative queue is still in `docs-playable-demo-current-execution-plan-2026-06-07.md`. This copy gives the deeper intent.

| Order | Status | Commit | Purpose |
| --- | --- | --- | --- |
| C1 | Done | `Strengthen damage demo readability` | Make limb/cockpit/ejection damage readable at screenshot scale |
| C2 | Done | `Keep battle UI sparse` | Ensure battle UI stays clean while preserving command/damage state |
| H1 | Done | `Write playable demo walkthrough` | Create a three-minute demonstration script |
| H2 | Done | `Prepare repeatable Windows demo build` | Document and verify repeatable build/smoke/capture commands |
| H3 | Done | `Package playable demo evidence` | Build an evidence page with screenshot beats and captions |
| P1 | Next | `Document reference content boundary` | Make private reference vs public content boundary explicit |
| P2 | Pending | `Add public content boundary check` | Add a safe check for public packaging |

## 6. Detailed Execution Tasks

### Task C1: Strengthen Damage Demo Readability

**Status:** Completed 2026-06-07.

**Goal:** `damage-demo` must read as mech section-damage combat, not generic RTS health bars.

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/DemoUnitView.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify if needed: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Inspect `RunStartupDamageDemoCapturePrelude`, `ForceStartupDamageDemoSections`, `ApplyStartupDamageDemoSection`, `BuildCaptureSidecar` and section cue code in `DemoUnitView`.
2. Keep BattleCore section state as truth.
3. Make one damage story unmistakable: arm loss, leg mobility loss, cockpit/ejection or wreck.
4. If needed, tighten `damage-demo` camera composition around damaged units instead of adding more HUD.
5. Add sidecar or validator evidence that the forced damage story exists.
6. Update the visual audit with the observed result.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-damage-selling-moment.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets damage-demo
```

**Acceptance:**

- `analysis-output/reference-visual-captures/damage-demo.png` has one clear mech damage story.
- The same story is visible in status row or sidecar evidence.
- Combat rules remain deterministic.
- HUD remains sparse.

**Commit:** `Strengthen damage demo readability`

### Task C2: Keep Battle UI Sparse

**Status:** Completed 2026-06-07.

**Goal:** Battle screen should not show too much information. It should support command clarity, not become a dashboard.

**Files:**

- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/Mc2DemoBootstrap.cs`
- Modify: `unity-mc2-demo/Assets/Scripts/Presentation/StartupCommanderScript.cs`
- Modify: `unity-mc2-demo/Assets/StreamingAssets/CommanderScripts/mc2_01-visible-flow-audit.txt`
- Modify if needed: `unity-mc2-demo/Assets/Editor/Mc2DemoValidator.cs`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Audit visible active-battle copy for save slots, account text, debug counters, weapon toggle language and oversized AI/chat UI.
2. Keep only mech status rows, selected/detached command state, health, important damaged section, jet, compact objective/task map and pause/system.
3. Guard the intended state in command-file smoke where practical.
4. Capture `spawn`, `hangar-contact` and `damage-demo`.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-battle-ui-sparse.log"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets spawn,hangar-contact,damage-demo
```

**Acceptance:**

- A new viewer can read squad status without a tutorial wall.
- UI does not cover core combat.
- Status rows still show health, damaged section, detached/solo state and repair readiness.

**Commit:** `Keep battle UI sparse`

### Task H1: Write Playable Demo Walkthrough

**Status:** Completed 2026-06-07.

**Goal:** A collaborator or investor can follow a three-minute script and understand the Demo value.

**Files:**

- Create: `docs-playable-demo-walkthrough-2026-06-07.md`
- Modify: `README.md`

**Steps:**

1. Script the full flow: start, MechLab, inspect fit, launch, all-squad move, focus target, solo command, jet, damage/ejection, debrief, repair, relaunch.
2. Use project-owned wording: AI-assisted tactical RTS, mech squad command, deterministic BattleCore, optional AI deputy, replaceable content packs.
3. Avoid old franchise marketing names, private reference asset promises and clone language.

**Validation:**

```powershell
git diff --check
```

**Acceptance:**

- The walkthrough matches current UI labels and flow.
- It can be read aloud in roughly three minutes.
- It does not imply private reference content is final product content.

**Commit:** `Write playable demo walkthrough`

### Task H2: Prepare Repeatable Windows Demo Build

**Status:** Completed 2026-06-07.

**Result:** The root `BUILD-WIN.md` now has a current Unity 6 Windows Demo handoff checklist with validator, build, visible-flow smoke, reference capture command, expected success strings, ignored evidence paths and private-reference-content boundary. `unity-mc2-demo/README.md` points to that checklist. Verified with `analysis-output/unity-validate-demo-package.log`, `analysis-output/unity-build-demo-package.log`, and `analysis-output/unity-player-demo-package.log`.

**Goal:** Make local build, smoke and capture reproducible without relying on memory.

**Files:**

- Modify: `BUILD-WIN.md`
- Modify: `unity-mc2-demo/README.md`
- Modify if needed: `README.md`

**Steps:**

1. Document Unity batch validator.
2. Document Unity Windows build.
3. Document visible-flow smoke.
4. Document reference capture and MechLab capture.
5. State that generated evidence stays ignored under `analysis-output`.

**Validation:**

```powershell
git diff --check
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-demo-package.log"
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" -batchmode -nographics -mc2SmokeTest -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-demo-package.log"
```

**Acceptance:**

- Build succeeds.
- Visible-flow smoke exits with code `0`.
- Docs contain current command examples.

**Commit:** `Prepare repeatable Windows demo build`

### Task H3: Package Playable Demo Evidence

**Status:** Completed 2026-06-07.

**Result:** Added `docs-playable-demo-investor-evidence-2026-06-07.md`, refreshed the six local evidence captures under ignored `analysis-output/reference-visual-captures/`, and updated the visual audit plus README key-document navigation. The page points to local PNG/JSON sidecars, gives a short presentation order, and states that private reference visuals are development evidence only.

**Goal:** Create a small evidence page that tells the Demo story without checking in generated screenshots.

**Files:**

- Create: `docs-playable-demo-investor-evidence-2026-06-07.md`
- Modify: `docs-reference-visual-audit-2026-06-07.md`

**Steps:**

1. Capture `mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol`.
2. Write captions for MechLab fitting, initial squad, contact, damage/ejection and debrief/repair if available.
3. State whether private reference content is used as development evidence.

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\unity\capture_reference_visuals.ps1 -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol
```

**Acceptance:**

- Evidence page can support a short investment/cooperation conversation.
- It points to ignored local evidence paths, not committed binary assets.
- It does not claim private reference assets are public-shippable.

**Commit:** `Package playable demo evidence`

### Task P1: Document Reference Content Boundary

**Status:** Next.

**Goal:** Make the repo safe to show: the project is AI RTS commander exploration with replaceable content packs, not a public asset clone.

**Files:**

- Modify: `README.md`
- Modify: `docs-content-replacement-plan.md`
- Modify: `docs-content-pack.md`
- Modify if needed: `unity-mc2-demo/README.md`

**Steps:**

1. Audit public text for old franchise, clone and private reference language.
2. Keep internal/reference wording contained to development evidence docs.
3. Public-facing language emphasizes AI-assisted tactical RTS, deterministic mech squad battle, optional AI deputy, replaceable content packs and future community maps.

**Validation:**

```powershell
git diff --check
rg -n "MechCommander|MechWarrior|原版|旧作|clone|复刻|private|reference" README.md docs-*.md unity-mc2-demo/README.md
```

**Acceptance:**

- README does not pitch the product as a clone.
- Private reference content is described as local development evidence.
- Content pack docs explain the replacement path.

**Commit:** `Document reference content boundary`

### Task P2: Add Public Content Boundary Check

**Goal:** Before public packaging, have a safe non-destructive check that warns if private/reference content leaks into a build path.

**Files:**

- Create: `scripts/content-pack/check_public_content_boundary.ps1`
- Modify: `docs-content-replacement-plan.md`
- Modify: `README.md`

**Steps:**

1. Implement a dry-run style checker for private reference pack paths, local extraction folders, forbidden legacy/proprietary name patterns and documented private asset naming patterns.
2. The script must never delete or move files.
3. Return non-zero when forbidden content is found.
4. Print clear relative paths and pattern names.

**Validation:**

```powershell
git diff --check
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\content-pack\check_public_content_boundary.ps1 -Path "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows" -DryRun
```

**Acceptance:**

- The script is safe and non-destructive.
- Docs explain when to run it.
- Public build boundary can be explained to collaborators.

**Commit:** `Add public content boundary check`

## 7. Milestone Gates

| Gate | Definition | Evidence |
| --- | --- | --- |
| G1 Combat feel | Weapons, impact, section damage and ejection are readable | validator, `damage-demo` capture, visible-flow smoke |
| G2 Battle readability | Terrain/building/mech fight reads as a tactical map, not a colored pile | five reference captures + sidecars |
| G3 MechLab feel | Fitting reads as physical block placement; weapons are always active | loadout validator, MechLab smoke, MechLab screenshot |
| G4 Battle loop | Debrief, one-click repair and relaunch work without save-slot UI | visible-flow smoke |
| G5 AI capability | AI is optional, compact and high-level; offline fallback works | AI validator + offline smoke |
| G6 Public boundary | README and scripts separate private reference content from product content | README audit + boundary check |
| G7 Physical occupancy | Unit/building/prop/water blockers have BattleCore evidence | validator + sidecars |
| G8 Handoff | A collaborator can build, run and explain the Demo in three minutes | walkthrough + build + evidence page |

## 8. Post-Demo Roadmap

Do not start these until G1-G8 are good enough to show.

| Phase | Start Condition | Scope |
| --- | --- | --- |
| R1 Public replacement pack | First local Demo is convincing | project-owned mech/weapon/building placeholders, provenance manifest, no private assets |
| R2 Improved art and terrain | Replacement pack exists | better terrain edges, water, roads, readable mech silhouettes, damage meshes |
| R3 More missions | First map loop is stable | 2-3 new small maps, mission variants, trigger library |
| S1 Main server prototype | Local loop is fun | account id, inventory snapshot, token ledger, signed loadout, reward claim stub |
| S2 Map package protocol | Server stub exists | map manifest, objective graph, reward table references, license/provenance |
| S3 Community map server | Package validation works | hosted rooms, signed results, reward caps, anti-abuse telemetry |
| W1 Web rankings | Signed battle summaries exist | player profile, squad builds, map ranking, event leaderboard |
| A1 AI托管/paper simulation | Local commander adapter is stable | AI commander profile, offline paper loss model, support reward |
| B1 Blockchain experiment | Economy/legal model is mature | optional creator payout proof or cosmetics only |

## 9. Stop Conditions

Stop and reassess if:

- `git status --short` shows unrelated user/source changes in files planned for editing.
- Unity scene fileID churn appears without intentional scene changes.
- Validator fails on movement, damage, objective, repair or loadout behavior.
- `hangar-contact` or `damage-demo` screenshots become less readable than previous evidence.
- Occupancy exists only in Unity presentation with no BattleCore evidence.
- AI code starts making per-frame or per-shot decisions.
- Normal battle UI exposes save slots, account management, debug-only panels or too much text.
- Public-facing docs pitch the project as a clone instead of AI-assisted tactical RTS exploration.

## 10. Definition Of Done For First Demo

The first Demo is ready to show externally when:

1. `git diff --check` passes.
2. Unity validator passes.
3. Windows build passes.
4. Visible-flow smoke exits with code `0`.
5. Reference captures are nonblank and readable.
6. `damage-demo` clearly shows a section-damage story.
7. `hangar-contact` reads as a tactical fight, not a single coordinate pile.
8. MechLab grid shows block fitting, heat, weight and legal/illegal state.
9. Battle UI stays sparse.
10. AI deputy window is optional and offline-safe.
11. README and content docs describe AI-assisted tactical RTS exploration, not a clone.
12. Private reference content is documented as replaceable and excluded from public distribution.

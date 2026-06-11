# Playable Demo Walkthrough 2026-06-07

Purpose: give a collaborator, tester, or investor a three-minute way to understand the current Windows playable Demo without knowing the codebase.

This is a local development walkthrough for an AI-assisted tactical RTS commander prototype. The pitch is not that the player must micromanage every unit. The pitch is that the player sets squad intent, equipment, risk posture, and key orders while deterministic BattleCore and an optional AI deputy handle tactical execution.

## Before The Demo

Use the current Windows build or the local development shortcut. The normal Windows build entry is:

```text
unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe
```

The walkthrough assumes the Demo can reach these screens and labels:

- `Mech Lab`
- `Battle / 战斗`
- `Objective / 目标`
- `Squad Command`
- `All`
- `Jet`
- `Map`
- `Bay`
- `System`
- `Fit OK`
- `Review`

If the build uses local private reference content, describe it only as development evidence for scale, pacing, and readability. Public or commercial builds must use project-owned or licensed replacement content packs.

Current seal evidence:

```text
analysis-output/unity-player-pc-evidence-visible-flow.log
```

The current PC4 seal smoke exits with code `0` and proves the scripted live path reaches combat, Debrief, Debrief summary, repair/Mech Lab, squad relaunch identity and compact loadout review. The matching Windows build evidence is `analysis-output/unity-build-pc-evidence-package.log`.

## Three-Minute Talk Track

### 0:00 - 0:20: Open With The Product Idea

Say:

```text
This is an AI-assisted tactical RTS commander prototype. The player is not clicking every shot or babysitting every unit. The player builds a mech squad, gives battlefield intent, and makes the important calls. BattleCore keeps the fight deterministic and testable, while the AI deputy stays optional and high-level.
```

Show:

- The Windows Demo running.
- The sparse top status strip.
- The project-owned framing: mech squad command, deterministic battle, optional AI deputy, replaceable content packs.

### 0:20 - 0:55: Show The Mech Lab

Action:

1. Open `Bay` or start from the current `Mech Lab`.
2. Select the first mech.
3. Point to the grid-fitting area.
4. Point to whole weapon blocks, `A+` armor fillers, `C+` heat sink fillers, H/W/G pressure cards, and the cell-state summary.

Say:

```text
The first pillar is the Mech Lab. Weapons are mounted as physical blocks, not toggles. Armor and cooling fill spare cells. The player can read heat, weight, grid pressure, and legal fit at a glance. This is the collection and preparation loop.
```

Call out:

- `Fit OK` means the current fit can launch.
- `Review` means the fit is over a limit or needs attention.
- Mounted weapons are active by default.

### 0:55 - 1:15: Launch Into The Mission

Action:

1. Launch the current local mission.
2. Let the camera settle on the commander mech.
3. Point to `Battle / 战斗`, `Objective / 目标`, and `Squad Command`.

Say:

```text
The second pillar is tactical map combat. The camera is fixed for clarity and follows the commander mech. The combat UI is intentionally sparse: squad rows, Jet, Map, Bay, System, a compact battle card, and the current objective.
```

Call out:

- The first unit is the commander anchor.
- The status rows carry health, heat, target state, damaged sections, and solo order state.

### 1:15 - 1:40: Demonstrate All-Squad Command

Action:

1. Keep `All` selected.
2. Click a destination or objective direction.
3. Click a target or structure when enemies are active.

Say:

```text
By default, the whole squad is selected. A map click becomes a squad movement intent. A target click becomes a focus or attack intent. The player gives the order; the local battle system resolves movement, weapon range, heat, cooldowns, hit direction, and damage.
```

Call out:

- No box-select is required.
- This is meant to work cleanly on a future touch interface.

### 1:40 - 2:05: Demonstrate Solo Order And Auto-Rejoin

Action:

1. Click one mech status row.
2. Give that mech a separate destination or target.
3. Watch the status row enter the solo/detached state.
4. Let it finish and return to squad behavior.

Say:

```text
For mobile-friendly control, single-mech orders come from the status row. That mech temporarily detaches, completes the order, and then rejoins the squad. The rest of the squad remains easy to command.
```

Call out:

- This is the simplified replacement for dragging selections on a phone.
- The status row is both the selector and the state display.

### 2:05 - 2:25: Demonstrate Jet

Action:

1. Press `Jet`.
2. Click the boost direction.
3. Watch legal mechs jump and illegal landing units stay put.

Say:

```text
Jet is a squad intent too. Each mech tries a fixed-distance boost from its own current position. BattleCore checks water, map bounds, and occupied blockers. Legal units move; illegal landing units stay still.
```

Call out:

- This keeps the command simple while preserving tactical terrain rules.
- The current debug evidence can show occupancy and blocked landing markers when needed.

### 2:25 - 2:45: Show Damage Story

Action:

1. Use or capture the `damage-demo` moment.
2. Point to a damaged status row.
3. Point to the world-space damage cue.

Say:

```text
The combat model keeps section damage. Arms can be lost, legs can fail, and cockpit damage can trigger pilot risk or ejection. The player does not need a spreadsheet in the battle HUD; the status row and world cue tell the story.
```

Call out:

- The current `damage-demo` evidence proves left-arm loss, leg loss, and cockpit loss.
- Armor plates use a simple global hardness rule so the calculation stays light.

### 2:45 - 3:00: Close With Debrief, Repair, And Direction

Action:

1. Complete or open debrief.
2. Show the repair / return-to-lab path.
3. Mention relaunch.

Say:

```text
The first playable loop is simple: fit the squad, launch, command the fight, read the damage, repair, and return to the lab. The AI deputy is deliberately small right now: high-level advice and future handoff, not per-frame control. Once this local loop is strong, the roadmap expands toward replacement content packs, more maps, certified rewards, web rankings, and community map servers.
```

## Operator Checklist

Use this order if the presenter is driving live:

1. Start Windows Demo.
2. Open `Mech Lab`.
3. Show weapon blocks, `A+`, `C+`, H/W/G and `Fit OK` or `Review`.
4. Launch the current mission.
5. Show `Squad Command`, `Battle / 战斗`, and `Objective / 目标`.
6. Use `All` to move the squad.
7. Focus a target or structure.
8. Click one status row and issue a solo order.
9. Use `Jet`.
10. Show section damage in `damage-demo`.
11. Open debrief.
12. Repair or return to `Bay`.
13. Relaunch or explain relaunch.

## Evidence To Keep Handy

Local generated evidence remains ignored under `analysis-output`.

Useful current evidence paths:

```text
analysis-output/unity-build-pc-evidence-package.log
analysis-output/unity-player-pc-evidence-visible-flow.log
analysis-output/reference-visual-captures/mechlab.png
analysis-output/reference-visual-captures/mechlab.json
analysis-output/reference-visual-captures/spawn.png
analysis-output/reference-visual-captures/spawn.json
analysis-output/reference-visual-captures/airfield.png
analysis-output/reference-visual-captures/airfield.json
analysis-output/reference-visual-captures/hangar-contact.png
analysis-output/reference-visual-captures/hangar-contact.json
analysis-output/reference-visual-captures/damage-demo.png
analysis-output/reference-visual-captures/damage-demo.json
analysis-output/reference-visual-captures/north-patrol.png
analysis-output/reference-visual-captures/north-patrol.json
```

Use the evidence to support the conversation, not as final public content if it depends on private reference assets.

## What Not To Promise Yet

Do not present these as first-version features:

- realtime PVP;
- mobile release or validated Android true-device play;
- map server;
- account economy;
- recharge/payment;
- chain assets;
- AI director;
- model-driven per-frame combat;
- public release of private reference content.

The first-version promise is narrower and stronger: a local Windows playable Demo with readable MechLab fitting, sparse tactical command, deterministic battle, visible section damage, one-click recovery flow, and optional high-level AI deputy.

# PC Investor Demo Route

Date: 2026-06-13

Purpose: a short Windows-local route for showing the current playable command demo without promising licensed final art, live multiplayer, or AI autonomy that is not implemented yet.

## Boundary

- First visible target: Windows PC controlled demo.
- Product direction preserved: first phone version is landscape-only; portrait is not a first-slice target.
- Visual boundary: private/reference art can be used for local development validation only. Public/commercial builds need owned, commissioned, or licensed replacement content packs.
- Combat boundary: BattleCore owns deterministic combat, pathing, damage, section loss, ejection, repair cost, and reward consequence. The presentation layer only visualizes and captures evidence.

## Three-Minute Route

| Step | Preset | What to show | Evidence point |
| --- | --- | --- | --- |
| 1 | `spawn` | Fixed RTS camera, readable first map, squad status rail, sparse battle HUD | Map and UI are visible without player box-select controls. |
| 2 | `hangar-contact` | Enemy contact trigger, objective pressure, hostile labels, squad focus command | The pacing can create pressure without requiring fast micro. |
| 3 | `damage-demo` | Section damage, lost-part story, cockpit/ejection readiness, repair consequence | Debrief summary exposes repair and damage consequence from the sidecar. |
| 4 | `solo-order` | Tap status row or command one mech independently | Single mech enters an isolated order state while the squad remains the default command target. |
| 5 | `solo-return` | Ordered mech automatically rejoins squad command state | The mobile landscape command model avoids drag-select and keeps the UI simple. |

## Talk Track

- This is an AI-deputy RTS direction: the player gives intent-level orders, while local combat still resolves exact movement, fire, damage, and repair.
- The first version should feel like a compact tactical command game: four to six mechs, fixed camera, clear status rows, jet command, system/pause, mission map, and MechLab later in the flow.
- AI should enter first as a planning/deputy layer, not as a per-frame combat brain. Latency stays outside the moment-to-moment battle loop.
- Current proxy visuals are development-safe stand-ins. They are meant to prove framing, scale, command readability, and damage story before the final asset pack lands.

## Current Evidence Commands

```powershell
powershell -ExecutionPolicy Bypass -File scripts\unity\capture_pc_controlled_demo_command_evidence.ps1 -RepoRoot . -PlanOnly
powershell -ExecutionPolicy Bypass -File scripts\unity\check_pc_controlled_demo_investor_readiness_fixes.ps1 -RepoRoot .
```

Expected route gate: `PC controlled-demo investor readiness fixes check OK.`

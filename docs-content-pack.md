# Content Pack Boundary

This project can be developed as an engine shell plus a replaceable content pack.
The shell is the compiled game executable, DLLs, renderer, mission runtime, and
tools. The content pack is everything the shell loads as game data.

The short-term goal is to use local reference assets as a private reference pack
for playability checks. The long-term goal is to replace that pack with a clean
project-owned or properly licensed pack without changing game logic.

This document defines the pack contract. It does not grant rights to redistribute
third-party art, text, audio, missions, trademarks, private exports, or other
uncleared content. Public or commercial builds must use a replacement pack.

## Public Boundary

The pack system has three distinct states:

| State | Manifest Signals | Allowed Use | Distribution Rule |
| --- | --- | --- | --- |
| Local reference | `kind=reference`, `license=local-reference-only`, reference-only source paths | Private mechanics, scale, pacing and mission-structure validation | Do not publish |
| Linked development replacement | `kind=replacement`, `seedMode=ReferenceLinks`, notes mention scaffold or local validation | Private work while replacing names, text, art and effects | Do not publish |
| Clean replacement | `kind=replacement`, no reference links, provenance notes for every asset source | Public demo, investor-safe build, partner review, commercial build | Publish only after boundary check |

Reference-linked packs are useful because they keep gameplay validation moving.
They are not safe release artifacts. A public build must be able to run from its
own replacement pack without local reference links, private extraction folders,
or uncleared names, stories, art, audio, video, icons or marks.

## Pack Layout

A runnable pack should have the same shape as the current executable folder:

```text
content-packs/<pack-id>/
  pack.json
  system.cfg
  assets/
  data/
  shaders/
  mission.fst
  tgl.fst
  art.fst
  textures.fst
  misc.fst
  camera.fst
  effect.fst
  insignia.fst
  testtxm.tga
  prefs.cfg
  options.cfg
```

Only `pack.json`, `prefs.cfg`, and `options.cfg` are project conventions. The
engine contract comes from `system.cfg`, the `assets`, `data`, `shaders`
directories, the fast files, and `testtxm.tga`.

## Required Runtime Contract

The first validation pass requires these directories:

- `assets`
- `data`
- `shaders`

It also requires these files:

- `system.cfg`
- `mission.fst`
- `tgl.fst`
- `art.fst`
- `textures.fst`
- `misc.fst`
- `camera.fst`
- `effect.fst`
- `insignia.fst`
- `testtxm.tga`

The current engine reads `system.cfg` from the process working directory, then
opens the fast files listed in the `[FastFiles]` block. The existing Windows data
build instructions already copy the same set of files into the executable folder,
so this boundary matches the current project instead of inventing a new format.

## Suggested Manifest

`pack.json` is optional for the current engine, but useful for tooling:

```json
{
  "id": "local-reference-dev",
  "title": "Local Reference Dev Pack",
  "kind": "reference",
  "license": "local-reference-only",
  "version": "0.1.0",
  "engineContract": "mc2-content-pack-v1",
  "product": {
    "id": "local-reference-dev",
    "title": "Local Reference Dev Pack",
    "language": "en",
    "audience": "local-development"
  },
  "notes": [
    "Use only for local development validation.",
    "Do not redistribute reference assets in public builds.",
    "Replace this pack before investor-safe or public packaging."
  ]
}
```

Replacement packs should keep the same `engineContract` while changing content,
names, art, text, missions, and legal provenance.

The repository may still contain a legacy local-reference example manifest for
tool fallback. Treat it as a private development locator, not as a publishable
pack template. New public-facing scaffolds should start from
`content-packs/project-owned-starter.example.json`,
`content-packs/project-owned-text-safe-slice.example.json`, or a clean pack
generated with `new_content_pack.ps1 -SeedMode Empty`.

`project-owned-text-safe-slice.example.json` is metadata-only. It proves the
first project-owned naming, mission copy, pilot, unit, weapon, objective, UI text
and provenance shape can pass the public boundary check. It is not mountable
until the required runtime files are supplied from cleared assets.

`project-owned-visual-slice.example.json` is also metadata-only. It proves the
first project-owned visual id scheme can pass the public boundary check before
any cleared model, texture, effect, icon, or UI art files are available.

`project-owned-art-safe-slice.example.json` combines the text-safe and visual id
scaffolds into the first art-safe mission-slice target. It records one clean
mission id, unit ids, terrain material ids, prop ids, weapon and damage FX ids,
UI art ids, planned cleared asset paths, provenance placeholders, and mounting
prerequisites. It is still metadata-only, not a mountable runtime pack.

## Stable Visual ID Contract

Use stable project-facing visual ids as the bridge between BattleCore, Unity,
content packs, and future product variants. These ids are allowed to stay stable
while the actual mesh, texture, effect, icon, or audio files are replaced.

Recommended prefixes:

- `unit.<name>` for playable or hostile unit identities.
- `model.unit.<name>` and `model.vehicle.<name>` for 3D unit meshes.
- `texture.unit.<name>.<slot>` and `texture.vehicle.<name>.<slot>` for unit textures.
- `terrain.<map>.<region>` for terrain semantic regions.
- `material.terrain.<name>` and `texture.terrain.<name>` for terrain visuals.
- `prop.<map-or-biome>.<name>` for structures, trees, barricades, vehicles, turrets and map dressing.
- `model.prop.<map-or-biome>.<name>` and `texture.prop.<map-or-biome>.<name>.<slot>` for prop art.
- `fx.weapon.<name>` for weapon effects.
- `fx.damage.<name>` for section damage, ejection, wreck and explosion effects.
- `ui.icon.<name>` and `ui.panel.<name>` for interface art.

Rules:

- Game logic should request stable ids, not local file paths.
- A public pack may remap the same stable id to a different file as long as the
  role and gameplay readability remain compatible.
- Unit damage ids should keep common semantic node names such as
  `node.left-arm`, `node.right-arm`, `node.left-leg`, `node.right-leg`, and
  `node.cockpit`.
- Terrain ids should describe gameplay-readable regions such as grass, runway,
  water, rock, dirt, road, cliff, or shore instead of a tool-specific tile index.
- Every public asset behind a stable id needs provenance before packaging.
- Local private reference manifests can inform scale and readability, but public
  replacement packs should expose only project-facing ids and cleared asset paths.

## Metadata Slice Ladder

Use these metadata artifacts in order:

| Artifact | Purpose | Mountable? |
| --- | --- | --- |
| `project-owned-text-safe-slice.example.json` | Clean product, mission, unit, weapon, pilot, objective and UI copy | No |
| `project-owned-visual-slice.example.json` | Stable project-facing visual ids for units, terrain, props, FX and UI art | No |
| `project-owned-art-safe-slice.example.json` | Combined first-mission art-safe target with planned cleared paths and provenance placeholders | No |
| Clean runtime pack | Real runtime files, final provenance, no local-only links | Yes |

The first three files are contracts for replacement work. They should pass the
public boundary checker, but they do not prove the game can run from cleared
art yet. A clean runtime pack is only ready after the required engine files and
assets exist and the mounted build also passes the public boundary checker.

## Development Flow

1. Validate a pack directory.
2. Create a runtime shell from executable files and DLLs.
3. Mount or copy the content pack into the runtime shell.
4. Launch the game from that run folder.
5. Replace the whole pack when switching from reference content to project-owned
   content.

Use the validation script first:

```powershell
& .\scripts\content-pack\validate_content_pack.ps1 -PackPath .\mc2-run64-dev
```

Preview a pack mount:

```powershell
& .\scripts\content-pack\mount_content_pack.ps1 `
  -PackPath .\content-packs\project-owned-linked-dev `
  -RunPath .\mc2-run64-dev `
  -DryRun
```

`project-owned-linked-dev` is still a private linked-development pack because
its manifest uses `seedMode=ReferenceLinks`. Use this command to validate the
mounting mechanics, not to certify a public build. For public packaging, pass a
completed clean replacement pack instead.

Remove `-DryRun` to actually mount the pack. Actual mounts replace only the
content entries listed in this document. Existing runtime entries are first
archived through the local backup helper before the new entries are linked or
copied.

By default, preferences such as `options.cfg` and `*prefs*.cfg` are left in the
runtime folder. Pass `-IncludePreferences` when a pack intentionally owns those
files too.

To add only preference files to an existing runtime shell:

```powershell
& .\scripts\content-pack\mount_content_pack.ps1 `
  -PackPath .\content-packs\project-owned-linked-dev `
  -RunPath .\runtime-shell-dev `
  -OnlyPreferences
```

To record which content pack a runtime shell is currently using without changing
files:

```powershell
& .\scripts\content-pack\mount_content_pack.ps1 `
  -PackPath .\content-packs\project-owned-linked-dev `
  -RunPath .\runtime-shell-dev `
  -MarkerOnly
```

Full mounts write `.content-pack-mounted.json` automatically. The start script
uses that marker to detect when the requested pack differs from the mounted pack.

Preview a clean runtime shell with a mounted content pack:

```powershell
& .\scripts\content-pack\new_runtime_shell.ps1 `
  -ShellSourcePath .\mc2-run64-dev `
  -OutputPath .\runtime-shell-dev `
  -PackPath .\content-packs\project-owned-linked-dev `
  -DryRun
```

Remove `-DryRun` and add `-Force` when intentionally recreating an existing
runtime shell. Existing output folders are archived through the local backup
helper before they are replaced.

Preview the full start flow:

```powershell
& .\scripts\content-pack\start_runtime_shell.ps1 -DryRun -RebuildShell -Force
```

Run the local development shell:

```powershell
& .\scripts\content-pack\start_runtime_shell.ps1
```

When `content-packs\project-owned-linked-dev` exists, the start and shortcut
scripts use it as the default development pack. Otherwise they fall back to the
local reference manifest. That fallback is for private development only; public
packaging should pass an explicit clean replacement pack path.

Check the current runtime shell:

```powershell
& .\scripts\content-pack\status_runtime_shell.ps1
```

Generate a content index for the current development pack:

```powershell
& .\scripts\content-pack\index_content_pack.ps1
```

Add `-IncludeFileList` when a detailed per-file JSON index is needed. The
default index focuses on counts by root directory, extension, category, and FST
archive table summaries.

Extract and analyze the first reference mission:

```powershell
& .\scripts\content-pack\extract_mission_from_pack.ps1 -MissionId mc2_01
& .\scripts\content-pack\analyze_mission_extract.ps1
& .\scripts\content-pack\export_unity_demo_contract.ps1
```

Pass `-Mission <mission-name>` to use the existing `-mission` command-line path,
or `-ExtraArgs` for lower-level engine flags.

Install a desktop shortcut for the local development runtime:

```powershell
& .\scripts\content-pack\install_dev_shortcut.ps1
```

The shortcut calls `start_runtime_shell.ps1`, so it keeps using the current
runtime shell and mounted content pack.

Create a new content pack scaffold:

```powershell
& .\scripts\content-pack\new_content_pack.ps1 `
  -PackId project-owned-dev `
  -Title "Project Owned Dev" `
  -DryRun
```

Use `-SeedMode ReferenceLinks` only for private development packs that link to
the local reference pack while replacement work is underway. Use `-SeedMode
Empty` for public-facing pack scaffolds.

See `docs-content-replacement-plan.md` for the replacement milestones.

## Product Variants

A reskin or product variant should be expressed as a separate content pack with
its own manifest. That pack should replace:

- product title and UI strings
- mech, weapon, pilot, faction, and mission text
- textures, models, effects, audio, and video
- mission scripts and campaign structure
- legal provenance notes

The stable part is the `engineContract`. If two packs share
`mc2-content-pack-v1`, the runtime shell can switch between them through the
same scripts.

This makes variants convenient technically, but not automatically safe legally.
A public variant still needs project-owned or properly licensed assets and text.
It also needs a manifest/provenance review and the P2 public content boundary
check before packaging.

## Pack Replacement Rule

Game code should not hard-code reference asset names beyond what is necessary to
load the current private development mission and runtime data. New gameplay and
AI work should talk to stable concepts such as unit definitions, weapon
definitions, mission triggers, terrain, effects, and UI strings.

That keeps the engine work useful after the local reference pack is replaced.

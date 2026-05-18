# Content Pack Boundary

This project can be developed as an engine shell plus a replaceable content pack.
The shell is the compiled game executable, DLLs, renderer, mission runtime, and
tools. The content pack is everything the shell loads as game data.

The short-term goal is to use the original local assets as a reference pack for
playability checks. The long-term goal is to replace that pack with a clean
project-owned pack without changing game logic.

This document defines the pack contract. It does not grant rights to redistribute
original MechCommander 2 art, text, audio, missions, trademarks, or other content.
Public or commercial builds should use a replacement pack.

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
  "id": "mc2-original-local",
  "title": "MC2 Original Local Reference Pack",
  "kind": "reference",
  "license": "local-reference-only",
  "version": "0.1.0",
  "engineContract": "mc2-content-pack-v1",
  "product": {
    "id": "mc2-reference",
    "title": "MC2 Reference",
    "language": "en",
    "audience": "local-development"
  },
  "notes": [
    "Use only for local development validation.",
    "Do not redistribute original art assets in public builds."
  ]
}
```

Replacement packs should keep the same `engineContract` while changing content,
names, art, text, missions, and legal provenance.

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
  -PackPath .\content-packs\mc2-original.local.example.json `
  -RunPath .\mc2-run64-dev `
  -DryRun
```

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
  -PackPath .\content-packs\mc2-original.local.example.json `
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
  -PackPath .\content-packs\mc2-original.local.example.json `
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
local original reference manifest.

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
the local reference pack while replacement work is underway.

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

## Pack Replacement Rule

Game code should not hard-code original asset names beyond what is necessary to
load the current mission and runtime data. New gameplay and AI work should talk
to stable concepts such as unit definitions, weapon definitions, mission
triggers, terrain, effects, and UI strings.

That keeps the engine work useful after the original reference pack is replaced.

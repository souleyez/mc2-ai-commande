# Content Replacement Plan

The current local reference pack is useful for gameplay validation. A public
demo or reskin should replace the whole content pack with project-owned or
properly licensed content.

## Replacement Layers

1. Product identity
2. UI text and localization
3. Mech, weapon, pilot, and faction data
4. Mission scripts and campaign text
5. Models, textures, effects, audio, video, portraits, and icons
6. Runtime packaging and provenance records

## Practical Milestones

Milestone 1: reference-linked development variant

- Create a pack with `SeedMode ReferenceLinks`.
- Keep using original local assets only for private validation.
- Change manifest identity and begin tracking replacement work.

Milestone 2: text-safe vertical slice

- Replace product name, UI strings, mech names, weapon names, pilot names, and
  one mission's visible text. Use `mc2_01` as the first reference mission unless
  runtime testing shows a blocker.
- Keep original art local as temporary validation content.
- Validate that the game still boots and the mission flow still works.

Milestone 3: art-safe vertical slice

- Replace one small mission's map textures, mech visuals, UI icons, weapon
  effects, and audio cues.
- Keep the same runtime contract and mission load path.
- Use this as the first investor-safe capture target.

Milestone 4: clean public pack

- Remove all original reference links.
- Validate the pack from its own files only.
- Keep provenance notes for each asset source.

## Commands

Preview a new empty replacement pack:

```powershell
& .\scripts\content-pack\new_content_pack.ps1 `
  -PackId project-owned-dev `
  -Title "Project Owned Dev" `
  -DryRun
```

Create a local private variant that links to the reference pack:

```powershell
& .\scripts\content-pack\new_content_pack.ps1 `
  -PackId project-owned-dev `
  -Title "Project Owned Dev" `
  -SeedMode ReferenceLinks
```

Mount a completed pack into the runtime shell:

```powershell
& .\scripts\content-pack\mount_content_pack.ps1 `
  -PackPath .\content-packs\project-owned-dev `
  -RunPath .\runtime-shell-dev
```

Generate and inspect the content index:

```powershell
& .\scripts\content-pack\index_content_pack.ps1 -IncludeFileList
```

See `docs-content-index-notes.md` for the current index summary.

## Rule

The engine can use reference-linked packs during private development. Public
builds should use packs that pass review without original copyrighted assets,
trademarks, or story text.

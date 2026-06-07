# Content Replacement Plan

The current local reference pack is useful for private gameplay validation. A
public demo, investor-safe build, product variant, or reskin must replace the
whole content pack with project-owned or properly licensed content.

This project should be presented as AI-assisted tactical RTS commander
exploration: deterministic mech squad battle, optional AI deputy, replaceable
content packs, and future community maps. Reference-linked development is only a
temporary validation method, not a product identity or release path.

## Boundary States

| State | Allowed Use | Content Rule | Public Release |
| --- | --- | --- | --- |
| Local reference pack | Private validation of scale, pacing, map readability, combat feel and data extraction | May point to local-only reference assets and source-derived contracts | Never |
| Reference-linked dev pack | Private replacement work while mechanics are still being tested | May link to local reference files while replacing names, text and art | Never |
| Text-safe slice | Private or tightly controlled demo review | Product names, UI text, unit/weapon names and visible mission text are project-owned or cleared; art may still be local reference evidence | Not public unless art is also cleared |
| Art-safe slice | Investor-safe visual demo | Text and visuals are project-owned or licensed; provenance is recorded | Yes, with final review |
| Clean public pack | Public build, commercial build, partner demo | No local reference links; every asset has project-owned or licensed provenance | Yes |

The engine, BattleCore rules, Unity presentation, build scripts and AI contracts
should remain useful across every state. Only the content pack changes.

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
- Keep using local reference assets only for private validation.
- Change manifest identity and begin tracking replacement work.
- Mark the pack as `local-development-only` in notes and do not publish builds
  that depend on it.

Milestone 2: text-safe vertical slice

- Replace product name, UI strings, mech names, weapon names, pilot names, and
  one mission's visible text. Use `mc2_01` as the first reference mission unless
  runtime testing shows a blocker.
- Keep reference art local as temporary validation content.
- Validate that the game still boots and the mission flow still works.
- Do not call this public-ready while any visible art, audio, icon, portrait,
  video, mission text, or trademark is still reference-linked.

Milestone 3: art-safe vertical slice

- Replace one small mission's map textures, mech visuals, UI icons, weapon
  effects, and audio cues.
- Keep the same runtime contract and mission load path.
- Use this as the first investor-safe capture target.

Milestone 4: clean public pack

- Remove all local reference links.
- Validate the pack from its own files only.
- Keep provenance notes for each asset source.
- Run the public content boundary check before packaging or pushing the build.

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
builds must use packs that pass review without local reference assets, uncleared
copyrighted files, third-party trademarks, source story text, private extraction
paths, or reference-only manifests.

Before a build is treated as public-safe, verify:

1. The mounted pack manifest has `kind=replacement` or equivalent clean-pack
   status.
2. `seedMode` is not `ReferenceLinks`.
3. Pack notes do not say local-reference-only, private validation, or
   development-only.
4. Visible names and UI copy are project-owned or cleared.
5. Asset provenance exists for models, textures, audio, video, icons, portraits
   and effects.
6. The P2 public content boundary check passes on the build path.

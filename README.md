# MC2 AI Commander Demo

Private experimental fork for a Unity 6 command-demo, replaceable content-pack
workflow, and future AI commander experiments. This branch contains
AI-assisted prototype code and tooling. The upstream project history and
license notices remain intact below.

## Upstream project

# [Mech Commander 2](https://alariq.github.io/mc2-website/) open source engine + Linux port.
[website](https://alariq.github.io/mc2-website/)

This port is an open source implementation of a closed MC2 engine code using available interface (.h) files.
Currently game can be run on both Linux and Windows in 64bit mode.
Fixed a lot of bugs (including ones present in original game).
Sound system is not fully implemented (panning, doppler, etc. not supported yet)


## !NB: 
as russia conducts war in Ukraine I have limited time to support this project until we will get rid of the orcs.

## Disclaimer:
I consider this project finished for now, there is a lot more to do for someone who wants to improve the game, but all functionality (except networking) is implemented and I've passed the game on my Linux box. Also found original game bugs and crashes are fixed.

## Upstream AI policy:
This project has 0% LLM code and AIs (such as Claude, ChatGPT and other LLMs) are not welcome here. I have limited time for it and even less time and desire to dig through AI-generated stuff. This is a personal hobby project which started because I love retro games and coding - not using LLMs. It is ok if you use LLMs to help you figure out an issue or help with the code, but you need to understand what you are doing, not just throwing promts at it until it "works".

## TODO: 
* fix remaining memory leaks (finish implementation of memory heaps)
* ~~make nice data packs, so not only me can play the game :-)~~ (see [data repo](https://github.com/alariq/mc2srcdata) )
* ~~actually finish all missions in the game~~
* make sure no files are created outside of user directory
* reduce draw calls number
* reimplement/optimize priority queue
* finish moving lighting to shaders (move whole lighting there, not only shader-based drawing of CPU-prelit vertices like I do now)
* Update graphics to ~~2018~~ ~~2020~~ 2021
* Movies support
* Implement network support?
* Editor?
* I am sure there is more

### Licensing
* Original game was released under Shared Source Limited Permission License (please refer to EULA.txt)
* My code is licenced under GPL v.3 (see license.txt)
* All third party libraries use their own licenses

### Content packs
Local development can treat the executable folder as an engine shell plus a
replaceable content pack. See `docs-content-pack.md` for the pack boundary.

Validate the current local reference pack:

```powershell
& .\scripts\content-pack\validate_content_pack.ps1 -PackPath .\mc2-run64-dev
```

Preview mounting a pack into the local runtime shell:

```powershell
& .\scripts\content-pack\mount_content_pack.ps1 -PackPath .\content-packs\mc2-original.local.example.json -RunPath .\mc2-run64-dev -DryRun
```

Preview creating a clean runtime shell and mounting a pack:

```powershell
& .\scripts\content-pack\new_runtime_shell.ps1 -ShellSourcePath .\mc2-run64-dev -OutputPath .\runtime-shell-dev -PackPath .\content-packs\mc2-original.local.example.json -DryRun
```

Preview the full start flow:

```powershell
& .\scripts\content-pack\start_runtime_shell.ps1 -DryRun -RebuildShell -Force
```

Start the local development runtime:

```powershell
& .\scripts\content-pack\start_runtime_shell.ps1
```

When `content-packs\project-owned-linked-dev` exists, the start and shortcut
scripts use it as the default development pack. Otherwise they fall back to the
local original reference manifest.

Check the current mounted content pack:

```powershell
& .\scripts\content-pack\status_runtime_shell.ps1
```

Generate a content index:

```powershell
& .\scripts\content-pack\index_content_pack.ps1
```

Current index notes are in `docs-content-index-notes.md`.

Extract the first reference mission for analysis:

```powershell
& .\scripts\content-pack\extract_mission_from_pack.ps1 -MissionId mc2_01
```

Analyze the extracted mission into JSON and Markdown summaries:

```powershell
& .\scripts\content-pack\analyze_mission_extract.ps1
```

Export a Unity-facing demo contract from that analysis:

```powershell
& .\scripts\content-pack\export_unity_demo_contract.ps1
```

Build the Unity 6 command demo:

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -batchmode -quit `
  -projectPath ".\unity-mc2-demo" `
  -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64
```

Install a desktop shortcut:

```powershell
& .\scripts\content-pack\install_dev_shortcut.ps1
```

Preview a new replacement pack scaffold:

```powershell
& .\scripts\content-pack\new_content_pack.ps1 -PackId project-owned-dev -Title "Project Owned Dev" -DryRun
```

The original asset pack is useful for local playability validation, but public or
commercial builds should use a replacement pack with project-owned content.


Building on Windows
===================

Updated detailed build manual for Windows can be found in `BUILD-WIN.md`


Building on Linux
=================

You, probably already know how to do it. If not, please, see windows building section, the process is quite similar.


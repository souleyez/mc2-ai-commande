Current Unity 6 Windows Demo handoff
====================================

This is the current repeatable path for the playable Windows demo. The older
native/C++ build notes are still kept below for historical engine work, but the
active first playable demo is the Unity 6 project in `unity-mc2-demo`.

Run these commands from the repository root:

```powershell
cd C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2
```

Validate the mission, loadout, AI boundary, occupancy, damage, and command-file
contracts:

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -batchmode -quit `
  -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" `
  -executeMethod MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract `
  -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-validate-demo-package.log"
```

Build the Windows player:

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -batchmode -quit `
  -projectPath "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo" `
  -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64 `
  -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-build-demo-package.log"
```

Preflight or launch the controlled Windows demo:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\run_windows_demo.ps1 -CheckOnly
```

To open the visible demo window with controlled 1280x720 windowed parameters:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\run_windows_demo.ps1
```

The launcher checks that `MC2UnityDemo.exe` and `MC2UnityDemo_Data` are both
present, writes the runtime log to `analysis-output/windows-demo-run.log`, and
passes Unity window arguments so the demo does not depend on stale player
window state from a previous run.

Check that the ignored Windows player output is newer than the tracked Unity
build inputs:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_windows_demo_build_freshness.ps1
```

The freshness check reads tracked Unity `Assets`, `ProjectSettings` and
`Packages` timestamps plus the ignored Windows player output. It does not start
Unity; rebuild the Windows player if it reports stale output.

Smoke the visible playable loop:

```powershell
& "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe" `
  -batchmode -nographics -mc2SmokeTest `
  -mc2CommandFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt" `
  -logFile "C:\Users\soulzyn\Desktop\codex\mechcommander2-mc2\analysis-output\unity-player-demo-package.log"
```

Capture reference screenshots and JSON sidecars when visual evidence is needed:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass `
  -File scripts\unity\capture_reference_visuals.ps1 `
  -Presets mechlab,spawn,airfield,hangar-contact,damage-demo,north-patrol
```

Check the current controlled-demo evidence package without rerunning Unity:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_evidence.ps1
```

The evidence checker validates the Windows build, visible-flow log, six PNG/JSON
captures, MechLab no-toggle fitting, terrain readability, sparse battle HUD,
contact separation, damage-demo story, and evidence freshness. The visible-flow
log must be newer than the current Windows build and command script; capture
PNG/JSON sidecars and per-preset capture logs must be newer than the current
Windows build and capture helper. It reads ignored local evidence and does not
create new artifacts.

Run the full controlled-demo readiness preflight:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_readiness.ps1
```

The readiness preflight wraps the launch preflight, build freshness, evidence
health check, and public boundary preflight. It does not start the Unity player
or regenerate captures.

Check the controlled-demo handoff consistency:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_controlled_demo_handoff.ps1
```

The handoff consistency check verifies that the key scripts, README, Windows
build handoff, plan docs, evidence page, and machine handoff plan still point to
the same controlled demo commands and current PC gate status.

Check demo source hygiene without launching Unity:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_demo_source_hygiene.ps1
```

This checks tracked and staged paths plus `.gitignore` markers so generated
evidence, Unity builds, APK/AAB outputs and private reference art stay out of
source commits.

Check Android smoke artifact hygiene without launching Unity:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_artifact_hygiene.ps1
```

This checks tracked and staged paths plus `.gitignore` markers so Android smoke
logs, screenshots and APK/AAB build outputs stay ignored until a real evidence
drop is explicitly requested.

Check the AI deputy contract without launching Unity or calling the model:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_ai_deputy_contract.ps1
```

This checks source and command-script markers proving MiniMax remains optional,
slow, high-level and rule-fallback guarded, while normal visible-flow smoke does
not request MiniMax commander steps.

Check the PC core playable BattleCore contract:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_pc_core_playable_contract.ps1
```

This runs the Unity editor validator and requires the explicit PC core marker
for command state, solo return, Jet legality, occupancy, damage/ejection, and
debrief/relaunch. It writes only an ignored validator log under
`analysis-output/`.

Check the mobile command model preflight without rerunning Unity:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_mobile_command_model_preflight.ps1
```

The mobile command model preflight reads the current ignored reference sidecars
and source/doc markers to verify that active battle remains translatable to the
planned phone command loop: status rows, Jet, map, bay/system, compact
objectives, hidden combat log/save/account/debug overlays, and MechLab fitting
without weapon enable/disable toggles.

Check the sparse battle HUD contract without launching Unity:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_battle_hud_sparse_contract.ps1
```

This checks the current Unity presentation source, capture gate, and mobile
command model preflight all require the same sparse battle HUD: status rows,
compact objective, closed mission map, hidden combat log, hidden overlays,
disabled save UI, and sidecar-only debug occupancy.

Check the current plan gate without launching Unity:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_current_plan_gate.ps1
```

The current plan gate wraps handoff/readiness, Windows build freshness, demo
source hygiene, Android smoke artifact hygiene, AI deputy contract, mobile command model, battle HUD sparse
contract, Android SDK tooling, Android APK freshness, Android APK identity, Android APK
compatibility, Android APK signing, Android APK manifest, Android APK payload,
Android APK size budget and Android device-smoke preflight checks. With no
authorized phone connected it should still pass while reporting Android as
waiting on device.

Check the Android SDK tooling directly:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_sdk_tooling.ps1
```

It validates the Unity AndroidPlayer SDK, NDK, OpenJDK, build tools, platform
and command-line tools before G3 install/launch.

Check the Android APK compatibility directly:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_compatibility.ps1
```

It validates the current APK `minSdkVersion`, `targetSdkVersion`, and native ABI
metadata before G3 install/launch.

Check the Android APK signing directly:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_signing.ps1
```

It validates `apksigner verify`, APK Signature Scheme v2, and the current debug
signer DN before G3 install/launch.

Check the Android APK manifest directly:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_manifest.ps1
```

It validates permissions, required hardware features and supported screen
classes before G3 install/launch.

Check the Android APK payload directly:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_payload.ps1
```

It validates the APK contains the expected Unity/IL2CPP native libraries,
`assets/bin/Data` runtime files and single `arm64-v8a` ABI folder before G3
install/launch.

Check the Android APK size budget directly:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_apk_size_budget.ps1
```

It validates the APK is within the current early mobile demo package budget
before G3 install/launch.

Self-test the Android smoke log scanner:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_android_smoke_log.ps1 -SelfTest
```

The log scanner is used by the Android device smoke helper after logcat capture
to fail on strong crash markers such as fatal exceptions, fatal signals, ANRs
for the package, process death and forced activity finish.

Preview the Android device smoke plan without installing or launching:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\android_device_smoke.ps1 -PlanOnly
```

The plan mode resolves the APK, adb, aapt, package name, launch activity, log
path and enabled install/launch/log-check steps without requiring a connected
phone.

Check the public boundary for the controlled-demo metadata package:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\content-pack\check_controlled_demo_public_boundary.ps1
```

Optionally confirm the current local Windows development build is still blocked
from public packaging:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\content-pack\check_controlled_demo_public_boundary.ps1 -CheckDevBuild
```

The public boundary preflight validates only project-owned metadata examples by
default. With `-CheckDevBuild`, the current development build is expected to
return `Result: FAILED`; that confirms the dev build has not accidentally been
treated as a public-safe package.

Expected success strings:

- `MC2 demo contract validation OK`
- `MC2 PC core playable contract OK`
- `Build Finished, Result: Success`
- `MC2 Unity demo Windows build OK`
- `Windows demo launch preflight OK`
- `Windows demo build freshness check OK`
- `Controlled demo evidence check OK`
- `Controlled demo readiness preflight OK`
- `Controlled demo handoff consistency check OK`
- `Demo source hygiene check OK`
- `Android smoke artifact hygiene check OK`
- `AI deputy contract check OK`
- `PC core playable contract check OK`
- `Mobile command model preflight OK`
- `Battle HUD sparse contract check OK`
- `Current plan gate check OK`
- `Android SDK tooling check OK`
- `Android APK compatibility check OK`
- `Android APK signing check OK`
- `Android APK manifest check OK`
- `Android APK payload check OK`
- `Android APK size budget check OK`
- `Android smoke log check self-test OK`
- `Android device smoke plan OK`
- `Controlled demo public boundary preflight OK`
- `MC2 demo smoke test exiting with code 0`
- `MC2 reference visual captures passed`

Generated logs, screenshots, JSON sidecars, and Windows player builds stay under
ignored paths such as `analysis-output/` and `unity-mc2-demo/Builds/`. Do not
stage them unless a later task explicitly asks for a packaged evidence drop.

The private local reference content pack is optional for development evidence:
it can improve scale, pacing, and readability while validating the demo, but it
is not public release content. Public or commercial builds must use
project-owned or properly licensed replacement content packs.

Legacy native Windows build notes
=================================

Preparing 3rdparties:
====================

One can also use 3rdparty.zip package in the repo for simpler setup, it contains all needed 3rdparty libraries
If you select to do it then skip directly to **Compiling mc2**

zlib
----

1. Download zlib sources from here: 
https://gnuwin32.sourceforge.net/packages/zlib.htm
direct link: https://gnuwin32.sourceforge.net/downlinks/zlib-src-zip.php
2. Download unistd.h for windows here:
https://gist.githubusercontent.com/mbikovitsky/39224cf521bfea7eabe9/raw/69e4852c06452a368a174ca1f0f33ce87bb52985/unistd.h
2b. Open it and comment out: `#include <getopt.h>` also comment integer types typedefs at the end
3. put it where zlib sourse files are located then open zconf.h and change `#include <unistd.h>` for `#include "unistd.h"` (or see 4.)
4. (alternative to 3) Put unistd.h to place where your compiler system headers are.
5. Open x86 Native Tools command prompt for VS2022 and cd to zlib
6. `nmake -f win32\Makefile.msc`
7. copy resulting .dll & .lib files to your 3rdparty folder: e.g. 3rdparty\lib\x86\
8. delete compilation files because we will now do same steps starting from step 5 but in x64 Native Tools (copy them to 3rdparty\lib\x64
9. Copy that unistd.h file to 3rdparty include folder

SDL
---

1. Download SLD x864 & x64 here: https://github.com/libsdl-org/SDL/releases/tag/release-2.30.11
direct link: https://github.com/libsdl-org/SDL/releases/download/release-2.30.11/SDL2-devel-2.30.11-VC.zip
2. copy libraries to corresponding x86 and x64 folders 
3. copy headers to 3rdparty\include\SDL2 folder
4. copy files from cmake folder to 3rdparty\cmake

4. Download SLD_mixer here: https://github.com/libsdl-org/SDL_mixer/releases
direct link: https://github.com/libsdl-org/SDL_mixer/releases/download/release-2.8.0/SDL2_mixer-devel-2.8.0-VC.zip
5. do same with lib/dll and headers as with SDL (headers should also go to 3rdparty\include\SDL2\ folder)
6. copy files from cmake folder to 3rdparty\cmake

7 Download SDL2_ttf here: https://github.com/libsdl-org/SDL_ttf/releases/tag/release-2.24.0
direct link: https://github.com/libsdl-org/SDL_ttf/releases/download/release-2.24.0/SDL2_ttf-devel-2.24.0-VC.zip
8. do same with lib/dll and headers as with SDL (headers should also go to 3rdparty\include\SDL2\ folder)
9. copy files from cmake folder to 3rdparty\cmake


glew
----
From sources:
1. Download glew here: https://sourceforge.net/projects/glew/files/glew/snapshots/glew-20190928.tgz/download
2. unzip and open build\vs12\glew.sln file
3. build for x64 and Win32

Binaries:
2. Or download prebuilt lib/dlls from this page: https://glew.sourceforge.net/ 
3. Put lib/dll/headers accordingly to x86/x64 (headers go into 3rdparty\include\GL)


Compiling mc2
=============
```
git clone https://github.com/alariq/mc2.git
cd mc2
md build64
cd build64
cmake.exe -G "Visual Studio 17 2022" -DCMAKE_PREFIX_PATH=c:/path_to_3rdparty_folder/ -DCMAKE_LIBRARY_ARCHITECTURE=x64 ..
```
(use absolute path to 3rdparty folder)

Copy mc2.exe to your executable folder of preference (say mc2exe)
```
cd res
md build64
cd build64
cmake.exe -G "Visual Studio 17 2022" -DCMAKE_LIBRARY_ARCHITECTURE=x64 ..
```
Copy mc2res.dll/pdb to your executable folder of preference (say mc2exe)


Building data
=============
```
git clone https://github.com/alariq/mc2srcdata.git
cd mc2srcdata
```

1. Read `README.md` in `build_scripts` folder

If you did not here are the steps:
1. copy the following tools from the exe solution to `build_scripts` folder 
(better copy Release version of these to make things faster):
    `aseconv`
    `makefst`
    `makersp`
    `pak`
    `text_tool`
    
    1a. Copy glew32.dll there as well (x86 or x64 depending on what version of tools you've built)

2. launch some console which has `make` in its path (needs GNUMake)
(you can install it from here: https://gnuwin32.sourceforge.net/packages/make.htm)
3. `cd build_scripts`
4. `make all` (or `>c:\path_to_gnumake\bin\make all`)
5. copy assets & data folder to your exe folder of preference
6. copy `*.cfg, *.fst, testtxm.tga` to your exe folder of preference


Final steps:
-----------
Copy all required dlls to your exe folder of preference

Run the game!

.. and, hopefully, enjoy

# MC2 Unity Demo

This Unity 6 project is the first playable shell for the MC2-style command demo.
It reads `mc2-unity-demo-contract-v1` from `StreamingAssets` and builds a
placeholder battlefield at runtime.

Current demo behavior:

- loads `mc2_01` unit spawns and objective graph
- builds a source-driven 100 x 100 terrain mesh and water plane from packet data
- places original terrain object records as lightweight trees and buildings
- follows the first player mech as commander
- defaults to squad orders, with status-bar click for detached single-unit order
- lets map clicks on hostile units or targetable buildings issue focused attack orders
- renders command rings for selected units, order destinations, and focused targets
- shows a subtle weapon range ring for selected, detached, attacking, or firing units
- renders short weapon beams, hit flashes, and impact bursts for combat events
- shows placeholder limb breakoff, section sparks, smoke, and cockpit ejection cues
- applies simple section penalties: destroyed arms reduce firepower, destroyed legs slow movement and disable Jet
- tracks source-derived weapon heat, cooling, and heat lockouts in combat
- carries source weapon type and special-effect metadata into combat visuals
- shows weapon name, range, cooldown, heat lock, and range readiness in the unit panel
- supports one-shot Jet orders with terrain-aware landing rejection
- provides a toggleable mission map and a pause/restart/end system panel
- resolves mission victory or defeat through BattleCore result state
- auto-acquires hostile units in weapon range
- resolves temporary weapon cooldown, damage, cockpit/torso/arms/legs sections,
  and destruction
- shows player unit structure and section damage in the left status panel
- prints recent combat events in the top-right combat panel

Local setup:

```powershell
& ..\scripts\content-pack\extract_mission_from_pack.ps1 -MissionId mc2_01
& ..\scripts\content-pack\analyze_mission_extract.ps1
& ..\scripts\content-pack\export_unity_demo_contract.ps1
Copy-Item ..\analysis-output\unity-demo-contract\project-owned-linked-dev\mc2_01\mission-contract.json .\Assets\StreamingAssets\Missions\mc2_01\mission-contract.json -Force
```

The contract JSON is generated from the private local reference content pack and
is ignored by git.

Rebuild the demo scene and Windows player:

```powershell
& "C:\Users\soulzyn\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -batchmode -quit `
  -projectPath "$PWD" `
  -executeMethod MC2Demo.EditorTools.Mc2DemoBuilder.BuildWindows64
```

Run the player smoke test:

```powershell
& .\Builds\Windows\MC2UnityDemo.exe `
  -batchmode -nographics -mc2SmokeTest `
  -logFile "$PWD\..\analysis-output\unity-player-smoke.log"
```

Interactive build output:

```text
Builds/Windows/MC2UnityDemo.exe
```

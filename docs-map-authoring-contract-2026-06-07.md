# Map Authoring Contract

Status: Done for the first platform contract pass.

Date: 2026-06-12

## Stable Markers

```text
MapAuthoringContract: True
OpenMapsCertifiedRewards: True
PortableRewardsMainServerOnly: True
MapPackageGrantsNoPortableRewards: True
RewardTableReferenceOnly: True
MapValidatorRequired: True
FirstEditorTarget: Local package authoring before server upload
```

## Product Rule

Maps can be open and editable. Portable rewards must be certified.

Community, partner and official maps may define terrain, encounter pacing,
objectives, triggers, enemy waves and presentation metadata. They must not
directly grant tokens, mech fragments, weapons, skins, ranking points or any
other portable inventory. A map package references a reward table id; the main
server decides whether that reward table is valid for the certified map version
and calculates the final grant.

## Package Shape

A map package is a signed or hashable directory or archive. The first editor can
write this as JSON plus referenced assets; the final runtime can later convert
it to Unity-friendly bundles.

Required manifest fields:

| Field | Purpose |
| --- | --- |
| `mapId` | Stable lowercase id, unique in the registry |
| `version` | Semver or monotonically increasing map version |
| `title` | Player-visible map title |
| `author` | Account, studio, partner or system author id |
| `license` | License or commercial usage grant |
| `provenance` | Source/ownership note for included assets |
| `contentHash` | Hash of manifest plus referenced package files |
| `battleCoreVersion` | BattleCore contract version expected by this map |
| `terrain` | Height, water, bounds, roads, surfaces and nav metadata |
| `navigation` | Walkable zones, blocked zones, jump/boost limits and exits |
| `structures` | Buildings, turrets, hard props, cover and destructibles |
| `objectives` | Visible and hidden objectives with ids and completion rules |
| `triggerGraph` | Trigger nodes, conditions, flags and side effects |
| `enemyWaves` | Enemy groups, patrols, spawn anchors and activation triggers |
| `allowedSquadSize` | Min/max player mech count, normally 1-6 |
| `difficultyEstimate` | Author estimate used for certification review |
| `rewardTableId` | Reference to a main-server reward table, never direct grants |
| `certificationState` | Draft, UncertifiedPublic, Certified, Event or Retired |

Optional fields:

- `biome`
- `weather`
- `lightingPreset`
- `musicCue`
- `voiceCueIds`
- `recommendedPower`
- `rankingCategory`
- `creatorAttribution`
- `previewImage`
- `assetReferences`

## Trigger Graph

Trigger graph nodes should be data, not executable scripts. The first contract
allows these trigger kinds:

- `OnObjectiveComplete`
- `OnAreaEntered`
- `OnUnitDestroyed`
- `OnStructureDamaged`
- `OnTimerElapsed`
- `OnWaveCleared`
- `OnSquadState`
- `SetFlag`
- `ActivateWave`
- `RevealObjective`
- `CompleteObjective`
- `FailObjective`
- `PlayCue`
- `OpenExit`

Future editor scripting can be added later, but the first public map package
contract should stay deterministic and validator-friendly.

## Reward Boundary

Allowed:

- `rewardTableId`
- `eventId`
- `rankingCategory`
- `claimLimitId`
- local-only debrief flavor text

Rejected package fields:

- `directRewardGrant`
- `tokenGrant`
- `fragmentGrant`
- `weaponGrant`
- `skinGrant`
- `inventoryMutation`
- `ledgerMutation`
- `realMoneyPayout`
- `chainTransfer`

The main server must verify map id, version, content hash, certification state,
session ticket, squad loadout hash, battle summary, replay digest if required,
reward table id and claim limits before granting portable rewards.

## Validator Requirements

The first map validator should fail a package when:

- required manifest fields are missing;
- map id or version is malformed;
- content hash does not match the manifest and referenced files;
- `battleCoreVersion` is unsupported;
- terrain bounds, water or blocked zones are invalid;
- navigation has no legal deployment area or no legal exit when required;
- objectives reference unknown triggers or units;
- trigger graph has missing nodes, cycles that never terminate, or unsupported
  side effects;
- enemy wave definitions reference unknown unit profiles or invalid spawn
  anchors;
- allowed squad size is outside 1-6;
- reward table id is missing for a certified/event map;
- the package contains any direct reward or inventory mutation field;
- license or provenance is missing.

Validator output should be machine-readable and suitable for editor display:

```text
mapId
version
contentHash
status
errors[]
warnings[]
certificationHints[]
```

## Certification States

| State | Portable Rewards | Notes |
| --- | --- | --- |
| Draft | No | Local editor and private testing only |
| UncertifiedPublic | No | Shareable for fun and feedback |
| Certified | Yes, under main-server limits | Eligible for normal rewards |
| Event | Yes, curated | Locked version for event/ranking periods |
| Retired | No new claims | Historical records remain visible |

## First Editor Target

The first editor should create a local package that can be validated without a
server. It does not need to upload, host rooms, mint rewards or publish rankings
in the first pass.

Minimum editor loop:

1. Create or open a package.
2. Edit metadata, terrain references, objectives, triggers, enemy waves and
   reward table reference.
3. Run local validator.
4. Export a package manifest and package hash.
5. Run the map locally with placeholder rewards disabled.

## First Runtime Target

The Unity demo should eventually load one package using this contract and run it
as a local mission. The map package must remain separate from player inventory,
reward calculation and account mutation.

## Verification

Run from the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\unity\check_map_authoring_contract.ps1 -RepoRoot .
```

Expected:

```text
Map authoring contract check OK.
```

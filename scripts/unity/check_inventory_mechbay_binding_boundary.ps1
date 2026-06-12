param(
    [string]$RepoRoot = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
else {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
}

$failures = New-Object System.Collections.Generic.List[string]
$rows = New-Object System.Collections.Generic.List[object]

function Add-Failure {
    param([string]$Message)
    [void]$failures.Add($Message)
}

function Add-Row {
    param(
        [string]$Check,
        [string]$Detail
    )

    [void]$rows.Add([pscustomobject]@{
        Check = $Check
        Status = "OK"
        Detail = $Detail
    })
}

function Resolve-RepoPath {
    param([string]$RelativePath)
    return Join-Path $RepoRoot ($RelativePath -replace "/", "\")
}

function Read-RequiredText {
    param([string]$RelativePath)

    $path = Resolve-RepoPath -RelativePath $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Add-Failure "$RelativePath missing"
        return ""
    }

    Add-Row -Check $RelativePath -Detail "exists"
    return Get-Content -LiteralPath $path -Raw -Encoding UTF8
}

function Require-Text {
    param(
        [string]$Text,
        [string]$Needle,
        [string]$Label
    )

    if ([string]::IsNullOrWhiteSpace($Text) -or -not $Text.Contains($Needle)) {
        Add-Failure "$Label missing marker: $Needle"
        return
    }

    Add-Row -Check $Label -Detail $Needle
}

function Forbid-Text {
    param(
        [string]$Text,
        [string]$Needle,
        [string]$Label
    )

    if (-not [string]::IsNullOrWhiteSpace($Text) -and $Text.Contains($Needle)) {
        Add-Failure "$Label contains forbidden marker: $Needle"
        return
    }

    Add-Row -Check "$Label forbidden marker" -Detail $Needle
}

$contract = Read-RequiredText -RelativePath "docs-inventory-mechbay-binding-boundary-2026-06-12.md"
$unityContract = Read-RequiredText -RelativePath "docs-unity-main-server-integration-contract-2026-06-12.md"
$adapter = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\BattleCore\UnityMainServerClient.cs"
$mechBayContract = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\BattleCore\MechBayInventoryContract.cs"
$loadoutContract = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\BattleCore\LoadoutContract.cs"
$fixture = Read-RequiredText -RelativePath "server\main-server\fixtures\local-dev-fixture.json"
$currentGate = Read-RequiredText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
$queue = Read-RequiredText -RelativePath "scripts\unity\check_current_plan_queue.ps1"
$handoffScript = Read-RequiredText -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"
$masterPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md"
$detailedPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"
$mobilePlan = Read-RequiredText -RelativePath "docs-mobile-first-plan-2026-06-10.md"
$handoff = Read-RequiredText -RelativePath "docs-machine-handoff-plan-2026-06-07.md"
$readme = Read-RequiredText -RelativePath "README.md"
$buildWin = Read-RequiredText -RelativePath "BUILD-WIN.md"
$buildMobile = Read-RequiredText -RelativePath "BUILD-MOBILE.md"

$stableMarkers = @(
    "InventoryMechBayBindingBoundary: True",
    "InventoryMechBayBindingOptional: True",
    "InventoryToMechBayMapping: True",
    "MechBayOnlyProjection: True",
    "LocalFixtureDefault: True",
    "NoDefaultRemoteInventoryDependency: True",
    "OptInReversibleBinding: True",
    "NoBattleCoreFrameInventoryServerCalls: True",
    "ServerInventoryNotCombatAuthority: True",
    "MobileLandscapeOnly: True",
    "NoLoginPaymentMarketplaceRealtimePvpChain: True",
    "F12RecommendedNext: Implement opt-in inventory-to-MechBay preview binding"
)

foreach ($marker in $stableMarkers) {
    Require-Text -Text $contract -Needle $marker -Label "inventory-to-MechBay boundary"
}

$sourceFields = @(
    "UnityInventoryBootstrap",
    "UnityAccountRecord",
    "UnityPublicProfileRecord",
    "UnityInventorySnapshot",
    "UnityOwnedMechRecord",
    "UnityItemStackRecord",
    "OfflineFixtureFallback",
    "AccountRecord.accountId",
    "AccountRecord.publicPlayerId",
    "AccountRecord.displayName",
    "InventorySnapshot.tokenBalance",
    "InventorySnapshot.ownedMechs",
    "InventorySnapshot.itemStacks",
    "ownedMechId",
    "unitId",
    "unitType",
    "chassisId",
    "displayName",
    "activeLoadoutId",
    "availableForMission",
    "conditionPercent",
    "pilotId",
    "pilotDisplayName",
    "pilotType",
    "itemId",
    "category",
    "quantity",
    "equippedQuantity"
)

foreach ($field in $sourceFields) {
    Require-Text -Text $contract -Needle $field -Label "binding field mapping"
}

$targetFields = @(
    "MechBayInventoryContract",
    "MechBayOwnedMechDefinition",
    "MechBayItemStackDefinition",
    "MechBayInventoryValidator.Schema",
    "MechBayOwnedRosterService",
    "MechBayInventoryAvailabilityResult",
    "MechBayMissionHandoffPreview",
    "mc2-mech-bay-inventory-v1",
    "LoadoutItemCategory",
    "Weapon",
    "ArmorPlate",
    "HeatSink",
    "MechFragment"
)

foreach ($field in $targetFields) {
    Require-Text -Text $contract -Needle $field -Label "MechBay target mapping"
}

$fallbackMarkers = @(
    "FallbackServerUnavailable",
    "FallbackInvalidInventorySnapshot",
    "FallbackUnknownChassisOrLoadout",
    "FallbackUnknownItemCategory",
    "FallbackPilotIncomplete",
    "FallbackServerPreviewRejected",
    "MechBayInventoryBuilder.BuildDemoInventory",
    "MechBayInventoryValidator.Validate(projectedInventory).IsValid == true"
)

foreach ($marker in $fallbackMarkers) {
    Require-Text -Text $contract -Needle $marker -Label "binding fallback rules"
}

$runtimeBoundaryMarkers = @(
    "Update",
    "FixedUpdate",
    "LateUpdate",
    "BattleCore movement",
    "damage",
    "pathing",
    "AI deputy decision",
    "default visible-flow smoke",
    "Server inventory is not combat authority"
)

foreach ($marker in $runtimeBoundaryMarkers) {
    Require-Text -Text $contract -Needle $marker -Label "runtime boundary"
}

$nonGoals = @(
    "No login",
    "No payment",
    "No realtime PVP",
    "No chain",
    "No remote inventory dependency for default demo gates",
    "No per-frame server calls"
)

foreach ($marker in $nonGoals) {
    Require-Text -Text $contract -Needle $marker -Label "explicit non-goal"
}

foreach ($marker in @(
    "UnityInventoryBootstrap",
    "UnityInventorySnapshot",
    "UnityOwnedMechRecord",
    "UnityItemStackRecord",
    "TryBootstrapInventory",
    "NoPerFrameServerCalls = true",
    "DefaultEnabled = false"
)) {
    Require-Text -Text $adapter -Needle $marker -Label "Unity adapter source"
}

foreach ($marker in @(
    "MechBayInventoryContract",
    "MechBayOwnedMechDefinition",
    "MechBayItemStackDefinition",
    "MechBayInventoryValidator",
    'public const string Schema = "mc2-mech-bay-inventory-v1";',
    "BuildDemoInventory",
    "ValidateUsage"
)) {
    Require-Text -Text $mechBayContract -Needle $marker -Label "MechBay contract target"
}

foreach ($marker in @(
    'public const string Weapon = "Weapon";',
    'public const string ArmorPlate = "ArmorPlate";',
    'public const string HeatSink = "HeatSink";',
    'public const string MechFragment = "MechFragment";'
)) {
    Require-Text -Text $loadoutContract -Needle $marker -Label "Loadout category target"
}

foreach ($marker in @(
    '"unitType":',
    '"chassisId":',
    '"displayName":',
    '"pilotType":',
    '"equippedQuantity":',
    '"category": "weapon"',
    '"category": "armor"',
    '"category": "heat-sink"',
    '"category": "mech-fragment"'
)) {
    Require-Text -Text $fixture -Needle $marker -Label "server fixture projection input"
}

Require-Text -Text $unityContract -Needle "GET /accounts/{accountId}/inventory" -Label "Unity integration contract"
Require-Text -Text $unityContract -Needle "MechLab can use local fixtures." -Label "Unity integration offline-first"
Require-Text -Text $unityContract -Needle "Server calls are limited to preparation and debrief" -Label "Unity integration runtime boundary"

Require-Text -Text $readme -Needle "docs-inventory-mechbay-binding-boundary-2026-06-12.md" -Label "README boundary doc"
Require-Text -Text $readme -Needle "check_inventory_mechbay_binding_boundary.ps1" -Label "README gate script"
Require-Text -Text $buildWin -Needle "docs-inventory-mechbay-binding-boundary-2026-06-12.md" -Label "BUILD-WIN boundary doc"
Require-Text -Text $buildWin -Needle "Inventory-to-MechBay binding boundary check OK." -Label "BUILD-WIN gate marker"
Require-Text -Text $buildMobile -Needle "F12 implement opt-in inventory-to-MechBay preview binding" -Label "BUILD-MOBILE next task"
Require-Text -Text $buildMobile -Needle "first phone version is landscape-only" -Label "BUILD-MOBILE landscape invariant"

Require-Text -Text $currentGate -Needle "check_inventory_mechbay_binding_boundary.ps1" -Label "current gate script"
Require-Text -Text $currentGate -Needle "Inventory-to-MechBay binding boundary check OK." -Label "current gate marker"
Require-Text -Text $queue -Needle "F11 plan inventory-to-MechBay binding boundary" -Label "queue completed task"
Require-Text -Text $queue -Needle "F12 implement opt-in inventory-to-MechBay preview binding" -Label "queue next task"
Require-Text -Text $handoffScript -Needle "check_inventory_mechbay_binding_boundary.ps1" -Label "handoff script gate"
Require-Text -Text $handoffScript -Needle "Inventory-to-MechBay binding boundary check OK" -Label "handoff script marker"

Require-Text -Text $masterPlan -Needle '| 89 | Done | `Plan inventory-to-MechBay binding boundary` |' -Label "master F11 done"
Require-Text -Text $masterPlan -Needle '| 90 | Done | `Implement opt-in inventory-to-MechBay preview binding` |' -Label "master F12 next"
Require-Text -Text $detailedPlan -Needle '| F11 | Done | `Plan inventory-to-MechBay binding boundary` |' -Label "detailed F11 done"
Require-Text -Text $detailedPlan -Needle '| F12 | Done | `Implement opt-in inventory-to-MechBay preview binding` |' -Label "detailed F12 next"
Require-Text -Text $mobilePlan -Needle "F11 plan inventory-to-MechBay binding boundary" -Label "mobile completed task"
Require-Text -Text $mobilePlan -Needle "F12 implement opt-in inventory-to-MechBay preview binding" -Label "mobile next task"
Require-Text -Text $handoff -Needle 'Current formal next development task after handoff: `F26 reduce Android combat effect log noise`' -Label "handoff next task"
Require-Text -Text $handoff -Needle 'Next planned work: `F26 reduce Android combat effect log noise`' -Label "handoff next planned work"

foreach ($textAndLabel in @(
    @{ Text = $readme; Label = "README" },
    @{ Text = $buildWin; Label = "BUILD-WIN" },
    @{ Text = $buildMobile; Label = "BUILD-MOBILE" },
    @{ Text = $masterPlan; Label = "master plan" },
    @{ Text = $detailedPlan; Label = "detailed plan" },
    @{ Text = $mobilePlan; Label = "mobile plan" },
    @{ Text = $handoff; Label = "handoff" }
)) {
    Forbid-Text -Text $textAndLabel["Text"] -Needle 'F11 | Next | `Plan inventory-to-MechBay binding boundary`' -Label $textAndLabel["Label"]
}

if ($failures.Count -gt 0) {
    Write-Host "Inventory-to-MechBay binding boundary check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) inventory-to-MechBay binding boundary check(s) failed."
}

Write-Host "Inventory-to-MechBay binding boundary check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize

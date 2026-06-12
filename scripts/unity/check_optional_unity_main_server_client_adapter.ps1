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
    return Get-Content -LiteralPath $path -Raw
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

$adapter = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\BattleCore\UnityMainServerClient.cs"
$contract = Read-RequiredText -RelativePath "docs-unity-main-server-integration-contract-2026-06-12.md"
$server = Read-RequiredText -RelativePath "server\main-server\main-server.mjs"
$README = Read-RequiredText -RelativePath "README.md"
$buildWin = Read-RequiredText -RelativePath "BUILD-WIN.md"
$buildMobile = Read-RequiredText -RelativePath "BUILD-MOBILE.md"
$masterPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md"
$detailedPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"
$mobilePlan = Read-RequiredText -RelativePath "docs-mobile-first-plan-2026-06-10.md"
$handoff = Read-RequiredText -RelativePath "docs-machine-handoff-plan-2026-06-07.md"
$currentGate = Read-RequiredText -RelativePath "scripts\unity\check_current_plan_gate.ps1"

$adapterRecords = @(
    "UnityServerSettings",
    "UnityServerStatus",
    "UnityInventoryBootstrap",
    "UnitySquadSignRequest",
    "UnitySignedSquadResult",
    "UnityBattleResultClaim",
    "UnityRewardClaimResult",
    "OfflineFixtureFallback",
    "UnityMainServerClient"
)

foreach ($record in $adapterRecords) {
    Require-Text -Text $adapter -Needle $record -Label "adapter record"
    Require-Text -Text $contract -Needle ($record -replace "UnityMainServerClient", "UnityServerSettings") -Label "contract record"
}

$boundaryMarkers = @(
    "DefaultEnabled = false",
    "UnityOfflineFirst = true",
    "NoRuntimeServerDependency = true",
    "NoPerFrameServerCalls = true",
    "NoLoginPaymentMarketplaceRealtimePvpChainInUnityAdapter = true",
    "BattleCoreVersion = ""mc2-unity-demo-contract-v1""",
    "EndpointHealth = ""/healthz""",
    "EndpointVersion = ""/version""",
    "EndpointDevAccounts = ""/dev/accounts""",
    "EndpointSquadsSign = ""/squads/sign""",
    "EndpointRewardClaims = ""/reward-claims""",
    "TrySignSquad",
    "TrySubmitRewardClaim",
    "TryBootstrapInventory",
    "Probe"
)

foreach ($marker in $boundaryMarkers) {
    Require-Text -Text $adapter -Needle $marker -Label "adapter boundary"
}

$fallbackMarkers = @(
    "ServerUnavailable",
    "UnsignedSquad",
    "RejectedClaim",
    "DuplicateClaim",
    "offline-fixture",
    "rewardClaimEnabled = false",
    "Local fixture launch is allowed",
    "do not mutate server inventory",
    "do not double-apply local inventory deltas"
)

foreach ($marker in $fallbackMarkers) {
    Require-Text -Text $adapter -Needle $marker -Label "adapter fallback"
}

$contractFields = @(
    "accountId",
    "publicPlayerId",
    "mapId",
    "mapVersion",
    "battleCoreVersion",
    "ownedMechIds",
    "signedSquadId",
    "loadoutHash",
    "signature",
    "expiresAt",
    "unitCount",
    "idempotencyKey",
    "battleSummaryHash",
    "resultSummary",
    "completedRewardResourcePoints",
    "causedDamageScore",
    "debuffSeconds",
    "objectivesCompleted",
    "enemiesDestroyed",
    "squadLosses",
    "rewardGrant",
    "tokenLedgerEntry",
    "inventorySnapshot",
    "leaderboardRow"
)

foreach ($marker in $contractFields) {
    Require-Text -Text $adapter -Needle $marker -Label "adapter field"
}

$serverEndpointMarkers = @(
    "/healthz",
    "/version",
    "/dev/accounts",
    "/squads/sign",
    "/reward-claims"
)

foreach ($marker in $serverEndpointMarkers) {
    Require-Text -Text $server -Needle $marker -Label "server endpoint"
}

Forbid-Text -Text $adapter -Needle "Update(" -Label "adapter frame loop"
Forbid-Text -Text $adapter -Needle "FixedUpdate(" -Label "adapter frame loop"
Forbid-Text -Text $adapter -Needle "LateUpdate(" -Label "adapter frame loop"

$docMarkers = @(
    "check_optional_unity_main_server_client_adapter.ps1",
    "Optional Unity main-server client adapter check OK",
    "F8 implement optional Unity main-server client adapter",
    "F9 wire optional Unity main-server adapter into launch/debrief smoke",
    "F10 wire optional Unity inventory bootstrap smoke",
    "F11 plan inventory-to-MechBay binding boundary"
)

foreach ($marker in $docMarkers) {
    Require-Text -Text $README -Needle $marker -Label "README"
    Require-Text -Text $buildWin -Needle $marker -Label "BUILD-WIN"
    Require-Text -Text $buildMobile -Needle $marker -Label "BUILD-MOBILE"
}

Require-Text -Text $masterPlan -Needle '| 86 | Done | `Implement optional Unity main-server client adapter` |' -Label "master queue F8"
Require-Text -Text $masterPlan -Needle '| 87 | Done | `Wire optional Unity main-server adapter into launch/debrief smoke` |' -Label "master queue F9"
Require-Text -Text $masterPlan -Needle '| 88 | Done | `Wire optional Unity inventory bootstrap smoke` |' -Label "master queue F10"
Require-Text -Text $masterPlan -Needle '| 89 | Done | `Plan inventory-to-MechBay binding boundary` |' -Label "master queue F11"
Require-Text -Text $masterPlan -Needle '| 90 | Done | `Implement opt-in inventory-to-MechBay preview binding` |' -Label "master queue F12"
Require-Text -Text $detailedPlan -Needle '| F8 | Done | `Implement optional Unity main-server client adapter` |' -Label "detailed queue F8"
Require-Text -Text $detailedPlan -Needle '| F9 | Done | `Wire optional Unity main-server adapter into launch/debrief smoke` |' -Label "detailed queue F9"
Require-Text -Text $detailedPlan -Needle '| F10 | Done | `Wire optional Unity inventory bootstrap smoke` |' -Label "detailed queue F10"
Require-Text -Text $detailedPlan -Needle '| F11 | Done | `Plan inventory-to-MechBay binding boundary` |' -Label "detailed queue F11"
Require-Text -Text $detailedPlan -Needle '| F12 | Done | `Implement opt-in inventory-to-MechBay preview binding` |' -Label "detailed queue F12"
Require-Text -Text $mobilePlan -Needle "F9 wire optional Unity main-server adapter into launch/debrief smoke" -Label "mobile completed task"
Require-Text -Text $mobilePlan -Needle "F10 wire optional Unity inventory bootstrap smoke" -Label "mobile completed task"
Require-Text -Text $mobilePlan -Needle "F11 plan inventory-to-MechBay binding boundary" -Label "mobile completed task"
Require-Text -Text $mobilePlan -Needle "F12 implement opt-in inventory-to-MechBay preview binding" -Label "mobile next task"
Require-Text -Text $mobilePlan -Needle "first phone version is landscape-only" -Label "mobile landscape invariant"
Require-Text -Text $handoff -Needle 'Current formal next development task after handoff: `F13 capture opt-in MechBay preview evidence`' -Label "handoff next task"
Require-Text -Text $handoff -Needle 'Next planned work: `F13 capture opt-in MechBay preview evidence`' -Label "handoff next planned work"
Require-Text -Text $currentGate -Needle 'Optional Unity main-server client adapter check OK.' -Label "current gate marker"

if ($failures.Count -gt 0) {
    Write-Host "Optional Unity main-server client adapter check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) optional Unity main-server client adapter check(s) failed."
}

Write-Host "Optional Unity main-server client adapter check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize

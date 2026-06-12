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

$contract = Read-RequiredText -RelativePath "docs-unity-main-server-integration-contract-2026-06-12.md"
$server = Read-RequiredText -RelativePath "server\main-server\main-server.mjs"
$smoke = Read-RequiredText -RelativePath "server\main-server\smoke.mjs"
$README = Read-RequiredText -RelativePath "README.md"
$buildWin = Read-RequiredText -RelativePath "BUILD-WIN.md"
$buildMobile = Read-RequiredText -RelativePath "BUILD-MOBILE.md"
$platformPlan = Read-RequiredText -RelativePath "docs-platform-ecosystem-plan.md"
$masterPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md"
$detailedPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"
$mobilePlan = Read-RequiredText -RelativePath "docs-mobile-first-plan-2026-06-10.md"
$handoff = Read-RequiredText -RelativePath "docs-machine-handoff-plan-2026-06-07.md"
$currentGate = Read-RequiredText -RelativePath "scripts\unity\check_current_plan_gate.ps1"

$stableMarkers = @(
    "UnityMainServerIntegrationContract: True",
    "UnityServerIntegrationOptional: True",
    "UnityOfflineFirst: True",
    "NoRuntimeServerDependency: True",
    "NoLoginPaymentMarketplaceRealtimePvpChainInUnityAdapter: True",
    "NoPerFrameServerCalls: True",
    "SignedSquadBeforeLaunch: True",
    "RewardClaimAfterBattle: True"
)

foreach ($marker in $stableMarkers) {
    Require-Text -Text $contract -Needle $marker -Label "integration contract"
}

$endpoints = @(
    "GET /healthz",
    "GET /version",
    "POST /dev/accounts",
    "GET /accounts/{accountId}/inventory",
    "POST /squads/sign",
    "POST /reward-claims",
    "GET /leaderboards/basic",
    "POST /dev/reset"
)

foreach ($endpoint in $endpoints) {
    Require-Text -Text $contract -Needle $endpoint -Label "contract endpoint"
}

$serverEndpointMarkers = @(
    "/healthz",
    "/version",
    "/dev/reset",
    "/dev/accounts",
    "inventoryMatch",
    "/inventory",
    "/squads/sign",
    "/reward-claims",
    "/leaderboards/basic"
)

foreach ($marker in $serverEndpointMarkers) {
    Require-Text -Text $server -Needle $marker -Label "server endpoint"
}

$adapterRecords = @(
    "UnityServerSettings",
    "UnityServerStatus",
    "UnityInventoryBootstrap",
    "UnitySquadSignRequest",
    "UnitySignedSquadResult",
    "UnityBattleResultClaim",
    "UnityRewardClaimResult",
    "OfflineFixtureFallback"
)

foreach ($record in $adapterRecords) {
    Require-Text -Text $contract -Needle $record -Label "unity adapter record"
}

$fieldMarkers = @(
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

foreach ($marker in $fieldMarkers) {
    Require-Text -Text $contract -Needle $marker -Label "contract field"
}

$fallbackMarkers = @(
    "FallbackServerUnavailable: local fixture squad",
    "FallbackUnsignedSquad: local fixture launch with reward claim disabled",
    "FallbackRejectedClaim: local debrief remains and inventory is not mutated",
    "FallbackDuplicateClaim: idempotent approved response is success"
)

foreach ($marker in $fallbackMarkers) {
    Require-Text -Text $contract -Needle $marker -Label "fallback behavior"
}

$offlineMarkers = @(
    "Validator still runs without server.",
    "Windows and Android smoke still run without server.",
    "MechLab can use local fixtures.",
    "Server failure must not block local BattleCore development.",
    "Server calls are limited to preparation and debrief"
)

foreach ($marker in $offlineMarkers) {
    Require-Text -Text $contract -Needle $marker -Label "offline-first boundary"
}

$serverMarkers = @(
    "UNITY_OFFLINE_FIRST = true",
    "BATTLE_CORE_VERSION = ""mc2-unity-demo-contract-v1""",
    "REWARD_RULES_VERSION = ""local-reward-rules-v1""",
    "signedSquadId",
    "rewardGrant",
    "tokenLedgerEntry",
    "leaderboardRow",
    "idempotencyKey"
)

foreach ($marker in $serverMarkers) {
    Require-Text -Text $server -Needle $marker -Label "server compatibility"
}

$smokeMarkers = @(
    "POST /squads/sign",
    "POST /reward-claims",
    "UnityOfflineFirst: True",
    "NoRemoteUnityDependency: True",
    "NoPaymentMarketplaceRealtimePvpChain: True",
    "duplicate reward claim must not double-spend ledger"
)

foreach ($marker in $smokeMarkers) {
    Require-Text -Text $smoke -Needle $marker -Label "server smoke compatibility"
}

$docMarkers = @(
    "docs-unity-main-server-integration-contract-2026-06-12.md",
    "check_unity_main_server_integration_contract.ps1",
    "Unity main-server integration contract check OK",
    "F7 document Unity main-server integration contract",
    "F8 implement optional Unity main-server client adapter",
    "F9 wire optional Unity main-server adapter into launch/debrief smoke",
    "F10 wire optional Unity inventory bootstrap smoke"
)

foreach ($marker in $docMarkers) {
    Require-Text -Text $README -Needle $marker -Label "README"
    Require-Text -Text $buildWin -Needle $marker -Label "BUILD-WIN"
}

foreach ($marker in $docMarkers) {
    Require-Text -Text $buildMobile -Needle $marker -Label "BUILD-MOBILE"
}

Require-Text -Text $platformPlan -Needle "docs-unity-main-server-integration-contract-2026-06-12.md" -Label "platform plan link"
Require-Text -Text $platformPlan -Needle "Unity integration stays optional and offline-first" -Label "platform integration boundary"
Require-Text -Text $masterPlan -Needle '| 85 | Done | `Document Unity main-server integration contract` |' -Label "master queue F7"
Require-Text -Text $masterPlan -Needle '| 86 | Done | `Implement optional Unity main-server client adapter` |' -Label "master queue F8"
Require-Text -Text $masterPlan -Needle '| 87 | Done | `Wire optional Unity main-server adapter into launch/debrief smoke` |' -Label "master queue F9"
Require-Text -Text $masterPlan -Needle '| 88 | Next | `Wire optional Unity inventory bootstrap smoke` |' -Label "master queue F10"
Require-Text -Text $detailedPlan -Needle "| F7 | Done | ``Document Unity main-server integration contract`` |" -Label "detailed queue F7"
Require-Text -Text $detailedPlan -Needle "| F8 | Done | ``Implement optional Unity main-server client adapter`` |" -Label "detailed queue F8"
Require-Text -Text $detailedPlan -Needle "| F9 | Done | ``Wire optional Unity main-server adapter into launch/debrief smoke`` |" -Label "detailed queue F9"
Require-Text -Text $detailedPlan -Needle "| F10 | Next | ``Wire optional Unity inventory bootstrap smoke`` |" -Label "detailed queue F10"
Require-Text -Text $mobilePlan -Needle "F9 wire optional Unity main-server adapter into launch/debrief smoke" -Label "mobile completed task"
Require-Text -Text $mobilePlan -Needle "F10 wire optional Unity inventory bootstrap smoke" -Label "mobile next task"
Require-Text -Text $handoff -Needle 'Current formal next development task after handoff: `F10 wire optional Unity inventory bootstrap smoke`' -Label "handoff next task"
Require-Text -Text $handoff -Needle 'Next planned work: `F10 wire optional Unity inventory bootstrap smoke`' -Label "handoff next planned work"
Require-Text -Text $currentGate -Needle 'Unity main-server integration contract check OK.' -Label "current gate marker"

if ($failures.Count -gt 0) {
    Write-Host "Unity main-server integration contract check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) Unity main-server integration contract check(s) failed."
}

Write-Host "Unity main-server integration contract check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize

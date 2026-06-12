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

$contract = Read-RequiredText -RelativePath "docs-server-implementation-boundary-2026-06-07.md"
$platformPlan = Read-RequiredText -RelativePath "docs-platform-ecosystem-plan.md"
$detailedPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"

$stableMarkers = @(
    "ServerImplementationBoundary: True",
    "ModularMainServerFirst: True",
    "NoMicroservicesFirst: True",
    "UnityDemoOfflineFirst: True",
    "MapServerOutOfFirstSlice: True",
    "NoPaymentsMarketplaceChainRealtimePvpInFirstServer: True",
    "BattleCoreValidationContractOnly: True",
    "FirstServerTarget: Local main-server prototype before remote platform dependency"
)

foreach ($marker in $stableMarkers) {
    Require-Text -Text $contract -Needle $marker -Label "server implementation boundary"
}

$firstSlice = @(
    "account id",
    "token ledger",
    "inventory snapshot",
    "signed squad loadout",
    "reward claim endpoint",
    "basic leaderboard",
    "local admin reset for development",
    "health and version endpoint"
)

foreach ($marker in $firstSlice) {
    Require-Text -Text $contract -Needle $marker -Label "first server slice"
}

$explicitExclusions = @(
    "payment",
    "recharge",
    "cash-out",
    "marketplace",
    "realtime PVP",
    "chain integration",
    "NFT minting",
    "public map server registration",
    "full moderation dashboard",
    "creator payout execution",
    "anti-cheat model training",
    "remote server dependency for the Unity demo"
)

foreach ($marker in $explicitExclusions) {
    Require-Text -Text $contract -Needle $marker -Label "first server exclusion"
}

$modules = @(
    "Account",
    "Inventory",
    "TokenLedger",
    "SquadSigning",
    "RewardClaims",
    "Leaderboard",
    "AdminDev"
)

foreach ($module in $modules) {
    Require-Text -Text $contract -Needle $module -Label "local server module"
}

$records = @(
    "AccountRecord",
    "PublicProfileRecord",
    "InventorySnapshot",
    "TokenLedgerEntry",
    "SignedSquadLoadout",
    "RewardClaim",
    "RewardGrant",
    "LeaderboardRow",
    "AuditEvent"
)

foreach ($record in $records) {
    Require-Text -Text $contract -Needle $record -Label "data contract"
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
    Require-Text -Text $contract -Needle $endpoint -Label "api boundary"
}

$offlineMarkers = @(
    "Validator still runs without server.",
    "Windows and Android smoke still run without server.",
    "MechLab can use local fixtures.",
    "Server failure must not block local BattleCore development."
)

foreach ($marker in $offlineMarkers) {
    Require-Text -Text $contract -Needle $marker -Label "unity offline boundary"
}

$validationMarkers = @(
    "verify known BattleCore version",
    "verify signed squad loadout hash",
    "verify map id/version",
    "verify result summary shape",
    "apply reward table rules and caps",
    "publish approved leaderboard rows"
)

foreach ($marker in $validationMarkers) {
    Require-Text -Text $contract -Needle $marker -Label "battlecore validation boundary"
}

Require-Text -Text $platformPlan -Needle "docs-server-implementation-boundary-2026-06-07.md" -Label "platform plan link"
Require-Text -Text $platformPlan -Needle "small local main-server prototype" -Label "platform first server target"
Require-Text -Text $platformPlan -Needle "No payment, marketplace, realtime PVP, chain integration, public map server registration or remote Unity dependency belongs in the first server slice." -Label "platform first server exclusions"
Require-Text -Text $detailedPlan -Needle "| F5 | Done | ``Document server implementation boundary`` |" -Label "detailed plan F5 status"
Require-Text -Text $detailedPlan -Needle "| F6 | Done | ``Scaffold local main-server prototype`` |" -Label "detailed plan F6 status"
Require-Text -Text $detailedPlan -Needle "| F7 | Done | ``Document Unity main-server integration contract`` |" -Label "detailed plan F7 status"
Require-Text -Text $detailedPlan -Needle "| F8 | Done | ``Implement optional Unity main-server client adapter`` |" -Label "detailed plan F8"
Require-Text -Text $detailedPlan -Needle "| F9 | Done | ``Wire optional Unity main-server adapter into launch/debrief smoke`` |" -Label "detailed plan completed F9"
Require-Text -Text $detailedPlan -Needle "| F10 | Done | ``Wire optional Unity inventory bootstrap smoke`` |" -Label "detailed plan completed F10"
Require-Text -Text $detailedPlan -Needle "| F11 | Next | ``Plan inventory-to-MechBay binding boundary`` |" -Label "detailed plan next task"

if ($failures.Count -gt 0) {
    Write-Host "Server implementation boundary check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) server implementation boundary check(s) failed."
}

Write-Host "Server implementation boundary check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize

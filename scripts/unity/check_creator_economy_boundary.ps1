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

$contract = Read-RequiredText -RelativePath "docs-creator-economy-boundary-2026-06-07.md"
$platformPlan = Read-RequiredText -RelativePath "docs-platform-ecosystem-plan.md"
$detailedPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"

$stableMarkers = @(
    "CreatorEconomyBoundary: True",
    "CentralizedLedgerFirst: True",
    "CreatorRevenueAccountingBeforeChain: True",
    "OptionalChainLayerLater: True",
    "CoreGameplayOffChain: True",
    "CombatStatsOffChain: True",
    "NormalInventoryOffChain: True",
    "RepairCostsOffChain: True",
    "FirstCreatorTarget: Accounting contract before marketplace or chain implementation"
)

foreach ($marker in $stableMarkers) {
    Require-Text -Text $contract -Needle $marker -Label "creator economy boundary"
}

$centralLedgers = @(
    "token ledger",
    "inventory ledger",
    "reward grant ledger",
    "creator revenue accounting ledger",
    "creator payout status ledger",
    "refund, rollback and moderation adjustment ledger"
)

foreach ($ledger in $centralLedgers) {
    Require-Text -Text $contract -Needle $ledger -Label "centralized ledger"
}

$creatorContributions = @(
    "certified map packages",
    "visual mech skins",
    "weapon skins",
    "event campaigns",
    "hosted map server capacity",
    "curated challenge ladders",
    "cosmetic badge or commemorative item designs"
)

foreach ($item in $creatorContributions) {
    Require-Text -Text $contract -Needle $item -Label "creator contribution"
}

$revenueScopes = @(
    "map clear revenue share",
    "skin sale revenue share",
    "event prize pool allocation",
    "featured creator pool allocation",
    "partner-hosted server capacity bonus",
    "admin adjustment, refund and rollback entries"
)

foreach ($scope in $revenueScopes) {
    Require-Text -Text $contract -Needle $scope -Label "revenue share scope"
}

$chainAllowed = @(
    "proof of revenue share",
    "transparent creator pools",
    "cosmetic ownership proof",
    "commemorative items",
    "public audit trail for certified creator payouts"
)

foreach ($marker in $chainAllowed) {
    Require-Text -Text $contract -Needle $marker -Label "late chain use"
}

$chainForbidden = @(
    "core combat",
    "mech stats",
    "weapon stats",
    "pilot skills",
    "repair costs",
    "ordinary token ledger",
    "ordinary inventory mutation",
    "battle outcomes",
    "anti-cheat-sensitive state",
    "reward claim validation"
)

foreach ($marker in $chainForbidden) {
    Require-Text -Text $contract -Needle $marker -Label "off-chain boundary"
}

$moderationMarkers = @(
    "delist maps or skins",
    "freeze revenue sharing during fraud review",
    "refund purchases or event entries",
    "reverse mistaken ledger entries with audit records"
)

foreach ($marker in $moderationMarkers) {
    Require-Text -Text $contract -Needle $marker -Label "moderation boundary"
}

Require-Text -Text $platformPlan -Needle "docs-creator-economy-boundary-2026-06-07.md" -Label "platform plan link"
Require-Text -Text $platformPlan -Needle "creator economy boundary" -Label "platform creator boundary"
Require-Text -Text $platformPlan -Needle "Chain remains optional and late." -Label "platform chain boundary"
Require-Text -Text $platformPlan -Needle "Do not put core combat, mech stats, weapon stats, repair costs, ordinary inventory mutation, normal token ledger, battle outcomes or anti-cheat-sensitive state on chain." -Label "platform off-chain boundary"
Require-Text -Text $detailedPlan -Needle "| F4 | Done | ``Document creator economy boundary`` |" -Label "detailed plan F4 status"
Require-Text -Text $detailedPlan -Needle "| F5 | Done | ``Document server implementation boundary`` |" -Label "detailed plan F5 status"
Require-Text -Text $detailedPlan -Needle "| F6 | Done | ``Scaffold local main-server prototype`` |" -Label "detailed plan F6 status"
Require-Text -Text $detailedPlan -Needle "| F7 | Next | ``Document Unity main-server integration contract`` |" -Label "detailed plan next task"

if ($failures.Count -gt 0) {
    Write-Host "Creator economy boundary check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) creator economy boundary check(s) failed."
}

Write-Host "Creator economy boundary check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize

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

$contract = Read-RequiredText -RelativePath "docs-web-ranking-plan-2026-06-07.md"
$platformPlan = Read-RequiredText -RelativePath "docs-platform-ecosystem-plan.md"
$rewardContract = Read-RequiredText -RelativePath "docs-platform-reward-contract-2026-06-07.md"
$detailedPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"

$stableMarkers = @(
    "WebRankingContract: True",
    "PublishedResultsCertifiedOnly: True",
    "PublicProfilesPrivacySafe: True",
    "NoPrivateAccountIds: True",
    "NoApiKeys: True",
    "NoUnpublishedInventory: True",
    "NoAntiCheatInternals: True",
    "RankingReadsApprovedClaimsOnly: True",
    "FirstWebTarget: Static ranking and profile contract before server implementation"
)

foreach ($marker in $stableMarkers) {
    Require-Text -Text $contract -Needle $marker -Label "web ranking contract"
}

$publicPages = @(
    "Season Leaderboard",
    "Map Ranking",
    "Player Public Profile",
    "Squad Loadout Snapshot",
    "Battle Record Detail",
    "Creator Or Map Author Page"
)

foreach ($page in $publicPages) {
    Require-Text -Text $contract -Needle $page -Label "public page contract"
}

$publicFields = @(
    "publicPlayerId",
    "displayName",
    "rankingPoints",
    "mapId",
    "mapVersion",
    "squadSnapshotId",
    "battleId",
    "validatorSummary",
    "authorPublicId"
)

foreach ($field in $publicFields) {
    Require-Text -Text $contract -Needle $field -Label "public field"
}

$forbiddenBoundaries = @(
    "internal account id",
    "email, phone, OAuth",
    "API keys",
    "unpublished inventory",
    "anti-cheat thresholds",
    "raw result claim payloads"
)

foreach ($marker in $forbiddenBoundaries) {
    Require-Text -Text $contract -Needle $marker -Label "privacy boundary"
}

$resultStates = @(
    "approved",
    "approvedCapped",
    "practiceOnly",
    "unranked",
    "removed"
)

foreach ($state in $resultStates) {
    Require-Text -Text $contract -Needle $state -Label "public result state"
}

Require-Text -Text $platformPlan -Needle "docs-web-ranking-plan-2026-06-07.md" -Label "platform plan link"
Require-Text -Text $platformPlan -Needle "Web/ranking service" -Label "platform web boundary"
Require-Text -Text $rewardContract -Needle "Web/ranking service" -Label "reward contract web actor"
Require-Text -Text $rewardContract -Needle "expose private account ids, anti-cheat internals, or rejected-claim detail" -Label "reward contract privacy boundary"
Require-Text -Text $detailedPlan -Needle "| F3 | Done | ``Document web ranking contract`` |" -Label "detailed plan F3 status"
Require-Text -Text $detailedPlan -Needle "| F4 | Done | ``Document creator economy boundary`` |" -Label "detailed plan F4 status"
Require-Text -Text $detailedPlan -Needle "| F5 | Next | ``Document server implementation boundary`` |" -Label "detailed plan next task"

if ($failures.Count -gt 0) {
    Write-Host "Web ranking contract check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) web ranking contract check(s) failed."
}

Write-Host "Web ranking contract check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize

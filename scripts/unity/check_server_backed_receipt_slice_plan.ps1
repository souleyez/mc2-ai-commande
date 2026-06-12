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

$receiptPlan = Read-RequiredText -RelativePath "docs-server-backed-receipt-slice-2026-06-12.md"
$mainServer = Read-RequiredText -RelativePath "server\main-server\main-server.mjs"
$mainServerSmoke = Read-RequiredText -RelativePath "server\main-server\smoke.mjs"
$unityClient = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\BattleCore\UnityMainServerClient.cs"
$masterPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md"
$detailedPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"
$mobilePlan = Read-RequiredText -RelativePath "docs-mobile-first-plan-2026-06-10.md"
$handoff = Read-RequiredText -RelativePath "docs-machine-handoff-plan-2026-06-07.md"
$readme = Read-RequiredText -RelativePath "README.md"
$buildWin = Read-RequiredText -RelativePath "BUILD-WIN.md"
$buildMobile = Read-RequiredText -RelativePath "BUILD-MOBILE.md"
$currentGate = Read-RequiredText -RelativePath "scripts\unity\check_current_plan_gate.ps1"

$receiptPlanMarkers = @(
    "ServerBackedReceiptSlice: True",
    "ReceiptAuthority: MainServer",
    "BattleCoreFrameLoopServerCalls: False",
    "NoBattleCoreFrameServerCalls: True",
    "UnityOfflineFirst: True",
    "ReceiptSource: DebriefSummary",
    "RewardClaimIdempotent: True",
    "TokenLedgerAuthoritative: True",
    "InventorySnapshotAfterGrant: True",
    "LeaderboardProjectionFromAcceptedClaim: True",
    "SignedSquadValidationRequired: True",
    "MapServerCannotGrantRewards: True",
    "ClientCannotMutateInventoryDirectly: True",
    "DuplicateClaimReturnsSameLedgerEntry: True",
    "RejectedClaimDoesNotMutateInventory: True",
    "ServerReceiptsNotRealtimePvp: True",
    "NoPaymentNoCashoutNoChain: True",
    "MobileLandscapeUnaffected: True",
    "MobileFirstLandscapeOnly: True",
    "PortraitOutOfFirstSlice: True",
    "F16RecommendedNext: Implement server-backed receipt evidence gate",
    "first phone version is landscape-only",
    "Server-backed receipt slice plan check OK."
)

foreach ($marker in $receiptPlanMarkers) {
    Require-Text -Text $receiptPlan -Needle $marker -Label "receipt slice plan"
}

$serverMarkers = @(
    'url.pathname === "/reward-claims"',
    'url.pathname === "/leaderboards/basic"',
    "function acceptRewardClaim",
    "state.rewardClaims.has(idempotencyKey)",
    "signedSquadId",
    "battlecore_version_mismatch",
    "map_mismatch",
    "calculateTokenDelta",
    "state.tokenLedgers.get(accountId).push(ledgerEntry)",
    "syncCurrencyStack(inventory)",
    "inventorySnapshot: inventory",
    "leaderboardRow",
    "buildLeaderboardRow",
    "NoPaymentMarketplaceRealtimePvpChain: True"
)

foreach ($marker in $serverMarkers) {
    Require-Text -Text $mainServer -Needle $marker -Label "main-server receipt authority"
}

$smokeMarkers = @(
    'POST", "/squads/sign"',
    'POST", "/reward-claims"',
    "duplicate reward claim must return same ledger entry",
    "duplicate reward claim must not double-spend ledger",
    'GET", "/leaderboards/basic?limit=5"',
    "NoPaymentMarketplaceRealtimePvpChain: True"
)

foreach ($marker in $smokeMarkers) {
    Require-Text -Text $mainServerSmoke -Needle $marker -Label "main-server smoke receipt evidence"
}

$unityClientMarkers = @(
    "DefaultEnabled = false",
    "NoPerFrameServerCalls = true",
    'EndpointRewardClaims = "/reward-claims"',
    "TrySubmitRewardClaim",
    "CanSubmitRewardClaim",
    "ApprovedOrDuplicateClaim",
    "ForRejectedClaim",
    "Unity main-server adapter disabled by default."
)

foreach ($marker in $unityClientMarkers) {
    Require-Text -Text $unityClient -Needle $marker -Label "Unity optional receipt adapter"
}

$docs = @(
    @{ Name = "README.md"; Text = $readme },
    @{ Name = "BUILD-WIN.md"; Text = $buildWin },
    @{ Name = "BUILD-MOBILE.md"; Text = $buildMobile },
    @{ Name = "docs-ai-rts-commander-current-master-plan-2026-06-07.md"; Text = $masterPlan },
    @{ Name = "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"; Text = $detailedPlan },
    @{ Name = "docs-mobile-first-plan-2026-06-10.md"; Text = $mobilePlan },
    @{ Name = "docs-machine-handoff-plan-2026-06-07.md"; Text = $handoff }
)

foreach ($doc in $docs) {
    Require-Text -Text $doc.Text -Needle "F15 plan server-backed receipt slice" -Label "$($doc.Name) F15 done marker"
    Require-Text -Text $doc.Text -Needle "F16 implement server-backed receipt evidence gate" -Label "$($doc.Name) F16 marker"
    Require-Text -Text $doc.Text -Needle "F17 plan post-receipt inventory refresh boundary" -Label "$($doc.Name) F17 marker"
}

Require-Text -Text $masterPlan -Needle '| 93 | Done | `Plan server-backed receipt slice` |' -Label "master F15 done"
Require-Text -Text $masterPlan -Needle '| 94 | Done | `Implement server-backed receipt evidence gate` |' -Label "master F16 done"
Require-Text -Text $masterPlan -Needle '| 95 | Done | `Plan post-receipt inventory refresh boundary` |' -Label "master F17 done"
Require-Text -Text $detailedPlan -Needle '| F15 | Done | `Plan server-backed receipt slice` |' -Label "detailed F15 done"
Require-Text -Text $detailedPlan -Needle '| F16 | Done | `Implement server-backed receipt evidence gate` |' -Label "detailed F16 done"
Require-Text -Text $detailedPlan -Needle '| F17 | Done | `Plan post-receipt inventory refresh boundary` |' -Label "detailed F17 done"
Require-Text -Text $mobilePlan -Needle "first phone version is landscape-only" -Label "mobile landscape invariant"
Require-Text -Text $mobilePlan -Needle "portrait is not a first-slice support target" -Label "mobile portrait exclusion"
Require-Text -Text $handoff -Needle 'Current formal next development task after handoff: `F22 audit landscape MechLab touch controls`' -Label "handoff next task"
Require-Text -Text $handoff -Needle 'Next planned work: `F22 audit landscape MechLab touch controls`' -Label "handoff next planned work"
Require-Text -Text $currentGate -Needle 'check_server_backed_receipt_slice_plan.ps1' -Label "current gate receipt slice script"
Require-Text -Text $currentGate -Needle 'Server-backed receipt slice plan check OK.' -Label "current gate receipt slice marker"
Require-Text -Text $currentGate -Needle 'capture_server_backed_receipt_evidence.ps1' -Label "current gate receipt evidence script"
Require-Text -Text $currentGate -Needle 'Server-backed receipt evidence capture OK.' -Label "current gate receipt evidence marker"

if ($failures.Count -gt 0) {
    Write-Host "Server-backed receipt slice plan check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) server-backed receipt slice plan check(s) failed."
}

Write-Host "Server-backed receipt slice plan check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize

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

$unityClient = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\BattleCore\UnityMainServerClient.cs"
$bootstrap = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs"
$validator = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Editor\Mc2DemoValidator.cs"
$commanderScript = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\StartupCommanderScript.cs"
$mainServerSmoke = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-main-server-smoke.txt"
$visibleFlow = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt"
$boundary = Read-RequiredText -RelativePath "docs-post-receipt-inventory-refresh-boundary-2026-06-12.md"
$masterPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md"
$detailedPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"
$mobilePlan = Read-RequiredText -RelativePath "docs-mobile-first-plan-2026-06-10.md"
$handoff = Read-RequiredText -RelativePath "docs-machine-handoff-plan-2026-06-07.md"
$currentGate = Read-RequiredText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
$queue = Read-RequiredText -RelativePath "scripts\unity\check_current_plan_queue.ps1"
$handoffScript = Read-RequiredText -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"

foreach ($marker in @(
    "UnityPostReceiptInventoryRefreshResult",
    "UnityPostReceiptInventoryRefreshProjector",
    "SourceMainServerRewardClaim = ""MainServerRewardClaim""",
    "SourceLabelMainServerReward = ""MainServerReward""",
    "MessageServerRewardAlreadyApplied = ""Server Reward Already Applied""",
    "UnityRewardClaimResult.IsApproved == true",
    "rewardClaim?.status, ""approved""",
    "rewardGrant.claimId, rewardClaim.claimId",
    "tokenLedgerEntry.reason, ""reward-claim""",
    "tokenLedgerEntry.balanceAfter != inventorySnapshot.tokenBalance",
    "inventorySnapshot.accountId, rewardClaim.accountId",
    "UnityInventoryToMechBayProjector.Project(bootstrap)",
    "MechBayInventoryValidator.Validate(projected)",
    "DuplicateLedgerAlreadyApplied",
    "appliedLedgerEntryIds.Contains(tokenLedgerEntry.ledgerEntryId)",
    "CanApplyInventory",
    "FallbackInvalidTokenLedgerEntry",
    "FallbackProjectedInventoryRejected"
)) {
    Require-Text -Text $unityClient -Needle $marker -Label "Unity post-receipt projector"
}

foreach ($marker in @(
    "MechLabInventorySourceRewardText = ""Inventory Source: Main Server Reward""",
    "postReceiptAppliedLedgerEntryIds",
    "postReceiptInventoryRefreshSummary",
    "TryApplyPostReceiptInventoryRefresh(""post-debrief"", mainServerSmokeRewardClaim)",
    "BuildPostReceiptInventoryRefreshSummary",
    "PostReceiptInventoryRefresh: ",
    "MC2 post-receipt inventory refresh OK",
    "MC2 post-receipt duplicate refresh OK",
    "MC2 post-receipt rejected refresh OK",
    "DuplicateClaimNoDoubleApplyLocalRefresh: True",
    "InventorySource=MainServerReward",
    "Server Reward +",
    "MessageServerRewardAlreadyApplied",
    "LeaderboardCompactText",
    "Inventory Source: Main Server Reward"
)) {
    Require-Text -Text $bootstrap -Needle $marker -Label "Bootstrap post-receipt binding"
}

foreach ($marker in @(
    "ValidatePostReceiptInventoryRefreshProjector();",
    "BuildPostReceiptRewardClaimResult",
    "UnityPostReceiptInventoryRefreshProjector.Build",
    "DuplicateLedgerAlreadyApplied",
    "CanApplyInventory",
    "FallbackInvalidTokenLedgerEntry",
    "MessageServerRewardAlreadyApplied",
    "Post-receipt duplicate ledger did not stay accepted without reapply"
)) {
    Require-Text -Text $validator -Needle $marker -Label "Unity validator coverage"
}

foreach ($marker in @(
    "main-server-smoke",
    "assert-main-server-smoke",
    "RewardClaimAfterDebrief: True",
    "PostReceiptInventoryRefresh: "
)) {
    if ($marker -eq "PostReceiptInventoryRefresh: ") {
        Require-Text -Text $bootstrap -Needle $marker -Label "main-server assertion upgraded"
    }
    else {
        Require-Text -Text ($commanderScript + $mainServerSmoke + $bootstrap) -Needle $marker -Label "main-server command path"
    }
}

foreach ($marker in @(
    "PostReceiptInventoryRefreshBoundary: True",
    "AcceptedClaimRefreshesInventorySnapshot: True",
    "DuplicateClaimNoDoubleApplyLocalRefresh: True",
    "RejectedClaimKeepsLocalDebriefAndInventory: True",
    "RefreshIsPostDebriefOnly: True",
    "NoBattleCoreFrameReceiptServerCalls: True",
    "MobileLandscapeOnly: True",
    "F18RecommendedNext: Implement opt-in post-receipt inventory refresh binding"
)) {
    Require-Text -Text $boundary -Needle $marker -Label "F18 boundary source"
}

foreach ($marker in @(
    "F18 implement opt-in post-receipt inventory refresh binding",
    "F19 capture opt-in post-receipt refresh evidence"
)) {
    Require-Text -Text ($masterPlan + $detailedPlan + $mobilePlan + $handoff + $queue + $handoffScript) -Needle $marker -Label "plan queue F18/F19"
}

foreach ($marker in @(
    "check_post_receipt_inventory_refresh_binding.ps1",
    "Post-receipt inventory refresh binding check OK."
)) {
    Require-Text -Text ($currentGate + $queue + $handoffScript) -Needle $marker -Label "gate wiring"
}

Forbid-Text -Text $visibleFlow -Needle "main-server-smoke" -Label "default visible-flow command file"
Forbid-Text -Text $visibleFlow -Needle "assert-main-server-smoke" -Label "default visible-flow command file"
Forbid-Text -Text $unityClient -Needle "Update(" -Label "Unity main-server client frame loop"
Forbid-Text -Text $unityClient -Needle "FixedUpdate(" -Label "Unity main-server client frame loop"
Forbid-Text -Text $unityClient -Needle "LateUpdate(" -Label "Unity main-server client frame loop"

if ($failures.Count -gt 0) {
    Write-Host "Post-receipt inventory refresh binding check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) post-receipt inventory refresh binding check(s) failed."
}

Write-Host "Post-receipt inventory refresh binding check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize

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

$boundary = Read-RequiredText -RelativePath "docs-post-receipt-inventory-refresh-boundary-2026-06-12.md"
$receiptPlan = Read-RequiredText -RelativePath "docs-server-backed-receipt-slice-2026-06-12.md"
$mainServer = Read-RequiredText -RelativePath "server\main-server\main-server.mjs"
$receiptEvidence = Read-RequiredText -RelativePath "server\main-server\receipt-evidence.mjs"
$unityClient = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\BattleCore\UnityMainServerClient.cs"
$mechBayContract = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\BattleCore\MechBayInventoryContract.cs"
$bootstrap = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs"
$readme = Read-RequiredText -RelativePath "README.md"
$buildWin = Read-RequiredText -RelativePath "BUILD-WIN.md"
$buildMobile = Read-RequiredText -RelativePath "BUILD-MOBILE.md"
$masterPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md"
$detailedPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"
$mobilePlan = Read-RequiredText -RelativePath "docs-mobile-first-plan-2026-06-10.md"
$pcPlan = Read-RequiredText -RelativePath "docs-pc-optimization-plan-2026-06-11.md"
$evidenceDoc = Read-RequiredText -RelativePath "docs-playable-demo-investor-evidence-2026-06-07.md"
$handoff = Read-RequiredText -RelativePath "docs-machine-handoff-plan-2026-06-07.md"
$currentGate = Read-RequiredText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
$queue = Read-RequiredText -RelativePath "scripts\unity\check_current_plan_queue.ps1"
$handoffScript = Read-RequiredText -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"

$stableMarkers = @(
    "PostReceiptInventoryRefreshBoundary: True",
    "ServerReceiptInventorySnapshotAuthority: True",
    "AcceptedClaimRefreshesInventorySnapshot: True",
    "DebriefUsesServerGrantAsOverlay: True",
    "MechBayRefreshAfterAcceptedClaim: True",
    "DuplicateClaimNoDoubleApplyLocalRefresh: True",
    "RejectedClaimKeepsLocalDebriefAndInventory: True",
    "LocalFixtureFallbackDefault: True",
    "NoDefaultRemoteReceiptDependency: True",
    "RefreshIsPostDebriefOnly: True",
    "BattleCoreDebriefIsReceiptSourceOnly: True",
    "NoBattleCoreFrameReceiptServerCalls: True",
    "ServerReceiptNotCombatAuthority: True",
    "MobileLandscapeOnly: True",
    "MobileFirstLandscapeOnly: True",
    "PortraitOutOfFirstSlice: True",
    "NoLoginPaymentMarketplaceRealtimePvpChain: True",
    "F18RecommendedNext: Implement opt-in post-receipt inventory refresh binding",
    "Post-receipt inventory refresh boundary check OK."
)

foreach ($marker in $stableMarkers) {
    Require-Text -Text $boundary -Needle $marker -Label "post-receipt boundary"
}

$sourceContractMarkers = @(
    "POST /reward-claims",
    "RewardClaimResponse.rewardClaim",
    "RewardClaimResponse.rewardGrant",
    "RewardClaimResponse.tokenLedgerEntry",
    "RewardClaimResponse.inventorySnapshot",
    "RewardClaimResponse.leaderboardRow",
    "UnityRewardClaimResult",
    "UnityRewardClaim",
    "UnityRewardGrant",
    "UnityTokenLedgerEntry",
    "UnityInventorySnapshot",
    "UnityLeaderboardRow",
    "OfflineFixtureFallback"
)

foreach ($marker in $sourceContractMarkers) {
    Require-Text -Text $boundary -Needle $marker -Label "refresh source contract"
}

$refreshRules = @(
    "MainServerRewardClaim",
    "claimId",
    "ledgerEntryId",
    "tokenDelta",
    "tokenBalance",
    "inventorySnapshot",
    "leaderboardRow",
    "Server Reward +2310",
    "Inventory Source: Main Server Reward",
    "Server Reward Already Applied",
    "UnityRewardClaimResult.IsApproved == true",
    'rewardClaim.status == "approved"',
    'tokenLedgerEntry.reason == "reward-claim"',
    "tokenLedgerEntry.balanceAfter == inventorySnapshot.tokenBalance",
    "UnityInventoryToMechBayProjector.Project",
    "MechBayInventoryValidator.Validate(projectedInventory).IsValid == true"
)

foreach ($marker in $refreshRules) {
    Require-Text -Text $boundary -Needle $marker -Label "refresh rule"
}

$runtimeRules = @(
    "Update",
    "FixedUpdate",
    "LateUpdate",
    "BattleCore movement",
    "cockpit breach",
    "AI deputy directive evaluation",
    "Default validator",
    "Windows visible-flow smoke",
    "Android visible-flow smoke",
    "The first phone version is landscape-only",
    "No login",
    "No payment",
    "No realtime PVP",
    "No chain",
    "No required remote server dependency",
    "No per-frame server calls"
)

foreach ($marker in $runtimeRules) {
    Require-Text -Text $boundary -Needle $marker -Label "refresh runtime boundary"
}

foreach ($marker in @(
    "inventorySnapshot: inventory",
    "leaderboardRow",
    "tokenLedgerEntry: ledgerEntry",
    "rewardGrant",
    "state.rewardClaims.has(idempotencyKey)",
    "return deepClone(state.rewardClaims.get(idempotencyKey))",
    "syncCurrencyStack(inventory)"
)) {
    Require-Text -Text $mainServer -Needle $marker -Label "main-server response source"
}

foreach ($marker in @(
    "InventoryBalanceMutatedOnce",
    "DuplicateClaimReturnsSameLedgerEntry",
    "RejectedClaimDoesNotMutateInventory",
    "LeaderboardProjectionFromAcceptedClaim",
    "afterFirstClaim",
    "afterDuplicateClaim",
    "afterRejectedClaim"
)) {
    Require-Text -Text $receiptEvidence -Needle $marker -Label "receipt evidence source"
}

foreach ($marker in @(
    "UnityRewardClaimResult",
    "UnityRewardClaim",
    "UnityRewardGrant",
    "UnityTokenLedgerEntry",
    "UnityInventorySnapshot",
    "UnityLeaderboardRow",
    "DuplicateClaimSuccess",
    "TrySubmitRewardClaim",
    "CanSubmitRewardClaim",
    "ApprovedOrDuplicateClaim",
    "ForRejectedClaim",
    "UnityInventoryToMechBayProjector"
)) {
    Require-Text -Text $unityClient -Needle $marker -Label "Unity receipt DTO source"
}

foreach ($marker in @(
    "MechBayMissionReceipt",
    "TokenDelta",
    "TokenBalance",
    "ItemStacks",
    "MechBayInventoryValidator",
    "MechBayInventoryContract"
)) {
    Require-Text -Text $mechBayContract -Needle $marker -Label "MechBay refresh target"
}

foreach ($marker in @(
    "OpenPostMissionMechBay",
    "RefreshDemoInventoryValidation",
    "MissionReceiptLogText",
    "ReceiptAssemblyText",
    "Mech Lab"
)) {
    Require-Text -Text $bootstrap -Needle $marker -Label "Unity debrief/MechBay target"
}

Require-Text -Text $receiptPlan -Needle "InventorySnapshotAfterGrant: True" -Label "F15 receipt plan source"
Require-Text -Text $receiptPlan -Needle "DuplicateClaimReturnsSameLedgerEntry: True" -Label "F15 duplicate rule"
Require-Text -Text $receiptPlan -Needle "RejectedClaimDoesNotMutateInventory: True" -Label "F15 rejection rule"

$docs = @(
    @{ Name = "README.md"; Text = $readme },
    @{ Name = "BUILD-WIN.md"; Text = $buildWin },
    @{ Name = "BUILD-MOBILE.md"; Text = $buildMobile },
    @{ Name = "master"; Text = $masterPlan },
    @{ Name = "detailed"; Text = $detailedPlan },
    @{ Name = "mobile"; Text = $mobilePlan },
    @{ Name = "pc"; Text = $pcPlan },
    @{ Name = "evidence"; Text = $evidenceDoc },
    @{ Name = "handoff"; Text = $handoff }
)

foreach ($doc in $docs) {
    Require-Text -Text $doc.Text -Needle "F17 plan post-receipt inventory refresh boundary" -Label "$($doc.Name) F17 marker"
    Require-Text -Text $doc.Text -Needle "F18 implement opt-in post-receipt inventory refresh binding" -Label "$($doc.Name) F18 marker"
    Require-Text -Text $doc.Text -Needle "F19 capture opt-in post-receipt refresh evidence" -Label "$($doc.Name) F19 marker"
    Require-Text -Text $doc.Text -Needle "F20 refresh Android landscape build/smoke evidence" -Label "$($doc.Name) F20 completed marker"
    Require-Text -Text $doc.Text -Needle "F23 capture landscape MechLab touch evidence" -Label "$($doc.Name) F23 completed marker"
    Require-Text -Text $doc.Text -Needle "F24 capture Android MechLab touch evidence" -Label "$($doc.Name) F24 done marker"
}

Require-Text -Text $readme -Needle "docs-post-receipt-inventory-refresh-boundary-2026-06-12.md" -Label "README F17 boundary doc"
Require-Text -Text $readme -Needle "check_post_receipt_inventory_refresh_boundary.ps1" -Label "README F17 gate script"
Require-Text -Text $buildWin -Needle "Post-receipt inventory refresh boundary check OK." -Label "BUILD-WIN F17 gate marker"
Require-Text -Text $buildMobile -Needle "first phone version is landscape-only" -Label "BUILD-MOBILE landscape invariant"

Require-Text -Text $masterPlan -Needle '| 95 | Done | `Plan post-receipt inventory refresh boundary` |' -Label "master F17 done"
Require-Text -Text $masterPlan -Needle '| 96 | Done | `Implement opt-in post-receipt inventory refresh binding` |' -Label "master F18 done"
Require-Text -Text $masterPlan -Needle '| 97 | Done | `Capture opt-in post-receipt refresh evidence` |' -Label "master F19 done"
Require-Text -Text $masterPlan -Needle '| 98 | Done | `Refresh Android landscape build/smoke evidence` |' -Label "master F20 done"
Require-Text -Text $masterPlan -Needle '| 101 | Done | `Capture landscape MechLab touch evidence` |' -Label "master F23 done"
Require-Text -Text $masterPlan -Needle '| 102 | Done | `Capture Android MechLab touch evidence` |' -Label "master F24 done"
Require-Text -Text $detailedPlan -Needle '| F17 | Done | `Plan post-receipt inventory refresh boundary` |' -Label "detailed F17 done"
Require-Text -Text $detailedPlan -Needle '| F18 | Done | `Implement opt-in post-receipt inventory refresh binding` |' -Label "detailed F18 done"
Require-Text -Text $detailedPlan -Needle '| F19 | Done | `Capture opt-in post-receipt refresh evidence` |' -Label "detailed F19 done"
Require-Text -Text $detailedPlan -Needle '| F20 | Done | `Refresh Android landscape build/smoke evidence` |' -Label "detailed F20 done"
Require-Text -Text $detailedPlan -Needle '| F23 | Done | `Capture landscape MechLab touch evidence` |' -Label "detailed F23 done"
Require-Text -Text $detailedPlan -Needle '| F24 | Done | `Capture Android MechLab touch evidence` |' -Label "detailed F24 done"
Require-Text -Text $handoff -Needle 'Current formal next development task after handoff: `F44 audit post-F43 PC controlled-demo investor evidence refresh`' -Label "handoff next task"
Require-Text -Text $handoff -Needle 'Next planned work: `F44 audit post-F43 PC controlled-demo investor evidence refresh`' -Label "handoff next planned work"
Require-Text -Text $currentGate -Needle "check_post_receipt_inventory_refresh_boundary.ps1" -Label "current gate F17 script"
Require-Text -Text $currentGate -Needle "Post-receipt inventory refresh boundary check OK." -Label "current gate F17 marker"
Require-Text -Text $queue -Needle "F17 plan post-receipt inventory refresh boundary" -Label "queue completed F17"
Require-Text -Text $queue -Needle "F18 implement opt-in post-receipt inventory refresh binding" -Label "queue completed F18"
Require-Text -Text $queue -Needle "F19 capture opt-in post-receipt refresh evidence" -Label "queue completed F19"
Require-Text -Text $queue -Needle "F20 refresh Android landscape build/smoke evidence" -Label "queue F20 done"
Require-Text -Text $queue -Needle "F23 capture landscape MechLab touch evidence" -Label "queue completed F23"
Require-Text -Text $queue -Needle "F24 capture Android MechLab touch evidence" -Label "queue completed F24"
Require-Text -Text $handoffScript -Needle "check_post_receipt_inventory_refresh_boundary.ps1" -Label "handoff script F17 gate"
Require-Text -Text $handoffScript -Needle "Post-receipt inventory refresh boundary check OK" -Label "handoff script F17 marker"

foreach ($doc in $docs) {
    Forbid-Text -Text $doc.Text -Needle 'F17 | Next | `Plan post-receipt inventory refresh boundary`' -Label $doc.Name
    Forbid-Text -Text $doc.Text -Needle 'Formal next task: `F17 plan post-receipt inventory refresh boundary`' -Label $doc.Name
}

if ($failures.Count -gt 0) {
    Write-Host "Post-receipt inventory refresh boundary check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) post-receipt inventory refresh boundary check(s) failed."
}

Write-Host "Post-receipt inventory refresh boundary check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize

param(
    [string]$RepoRoot = "",
    [string]$OutputDir = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
else {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
}

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot "analysis-output\server-backed-receipt-evidence"
}
$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)

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

function Require-File {
    param([string]$RelativePath)

    $path = Resolve-RepoPath -RelativePath $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Add-Failure "$RelativePath missing"
        return ""
    }

    Add-Row -Check $RelativePath -Detail "exists"
    return $path
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

function Require-JsonBool {
    param(
        [object]$Object,
        [string]$Name,
        [string]$Label
    )

    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property -or [bool]$property.Value -ne $true) {
        Add-Failure "$Label expected true: $Name"
        return
    }

    Add-Row -Check $Label -Detail "$Name=True"
}

function Require-JsonValue {
    param(
        [object]$Object,
        [string]$Name,
        [object]$Expected,
        [string]$Label
    )

    $property = $Object.PSObject.Properties[$Name]
    $actual = if ($null -eq $property) { "<missing>" } else { $property.Value }
    if ($null -eq $property -or "$actual" -ne "$Expected") {
        Add-Failure "$Label expected ${Name}=${Expected}, got $actual"
        return
    }

    Add-Row -Check $Label -Detail "$Name=$Expected"
}

function Assert-IgnoredGeneratedPath {
    param(
        [string]$RelativePath,
        [string]$Label
    )

    $output = & git -C $RepoRoot check-ignore $RelativePath 2>&1
    if ($LASTEXITCODE -ne 0) {
        Add-Failure "$Label is not ignored by git: $RelativePath $($output -join ' ')"
        return
    }

    Add-Row -Check "$Label ignored" -Detail $RelativePath
}

$repoRootFull = [System.IO.Path]::GetFullPath($RepoRoot)
if (-not $OutputDir.StartsWith($repoRootFull, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDir must stay inside RepoRoot: $OutputDir"
}

$nodeCommand = Get-Command node -ErrorAction SilentlyContinue
if ($null -eq $nodeCommand) {
    Add-Failure "node is required for server-backed receipt evidence."
}
else {
    Add-Row -Check "node" -Detail $nodeCommand.Source
}

$runnerPath = Require-File -RelativePath "server\main-server\receipt-evidence.mjs"
$mainServerText = Read-RequiredText -RelativePath "server\main-server\main-server.mjs"
$packageText = Read-RequiredText -RelativePath "server\main-server\package.json"
$receiptPlanText = Read-RequiredText -RelativePath "docs-server-backed-receipt-slice-2026-06-12.md"
$gitignoreText = Read-RequiredText -RelativePath ".gitignore"
$masterPlanText = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md"
$detailedPlanText = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"
$mobilePlanText = Read-RequiredText -RelativePath "docs-mobile-first-plan-2026-06-10.md"
$handoffText = Read-RequiredText -RelativePath "docs-machine-handoff-plan-2026-06-07.md"
$currentGateText = Read-RequiredText -RelativePath "scripts\unity\check_current_plan_gate.ps1"

foreach ($marker in @(
    "ServerBackedReceiptEvidence",
    "SignedSquadBeforeLaunch",
    "RewardClaimAfterDebrief",
    "DuplicateClaimReturnsSameLedgerEntry",
    "InventoryBalanceMutatedOnce",
    "RejectedClaimDoesNotMutateInventory",
    "LeaderboardProjectionFromAcceptedClaim",
    "NoBattleCoreFrameServerCalls",
    "NoUnityLaunch",
    "MobileFirstLandscapeOnly"
)) {
    $runnerText = if ($runnerPath) { Get-Content -LiteralPath $runnerPath -Raw } else { "" }
    Require-Text -Text $runnerText -Needle $marker -Label "receipt evidence runner"
}

foreach ($marker in @(
    'url.pathname === "/reward-claims"',
    'url.pathname === "/leaderboards/basic"',
    "state.rewardClaims.has(idempotencyKey)",
    "state.tokenLedgers.get(accountId).push(ledgerEntry)",
    "inventorySnapshot: inventory",
    "leaderboardRow"
)) {
    Require-Text -Text $mainServerText -Needle $marker -Label "main-server receipt source"
}

Require-Text -Text $packageText -Needle '"receipt:evidence": "node receipt-evidence.mjs"' -Label "package evidence script"
Require-Text -Text $receiptPlanText -Needle "F16 should add a focused evidence gate" -Label "F15 plan F16 target"
Require-Text -Text $gitignoreText -Needle "analysis-output/server-backed-receipt-evidence/" -Label "receipt evidence ignore"
Assert-IgnoredGeneratedPath -RelativePath "analysis-output/server-backed-receipt-evidence/receipt-evidence.json" -Label "receipt evidence json"
Assert-IgnoredGeneratedPath -RelativePath "analysis-output/server-backed-receipt-evidence/receipt-evidence.log" -Label "receipt evidence log"
Assert-IgnoredGeneratedPath -RelativePath "analysis-output/server-backed-receipt-evidence/receipt-evidence-runner.log" -Label "receipt evidence runner log"

foreach ($doc in @(
    @{ Name = "master"; Text = $masterPlanText },
    @{ Name = "detailed"; Text = $detailedPlanText },
    @{ Name = "mobile"; Text = $mobilePlanText },
    @{ Name = "handoff"; Text = $handoffText }
)) {
    Require-Text -Text $doc.Text -Needle "F16 implement server-backed receipt evidence gate" -Label "$($doc.Name) F16 marker"
    Require-Text -Text $doc.Text -Needle "F17 plan post-receipt inventory refresh boundary" -Label "$($doc.Name) F17 marker"
}

Require-Text -Text $masterPlanText -Needle '| 94 | Done | `Implement server-backed receipt evidence gate` |' -Label "master F16 done"
Require-Text -Text $masterPlanText -Needle '| 95 | Done | `Plan post-receipt inventory refresh boundary` |' -Label "master F17 done"
Require-Text -Text $detailedPlanText -Needle '| F16 | Done | `Implement server-backed receipt evidence gate` |' -Label "detailed F16 done"
Require-Text -Text $detailedPlanText -Needle '| F17 | Done | `Plan post-receipt inventory refresh boundary` |' -Label "detailed F17 done"
Require-Text -Text $mobilePlanText -Needle "first phone version is landscape-only" -Label "mobile landscape invariant"
Require-Text -Text $handoffText -Needle 'Current formal next development task after handoff: `F44 audit post-F43 PC controlled-demo investor evidence refresh`' -Label "handoff next task"
Require-Text -Text $handoffText -Needle 'Next planned work: `F44 audit post-F43 PC controlled-demo investor evidence refresh`' -Label "handoff next planned work"
Require-Text -Text $currentGateText -Needle "capture_server_backed_receipt_evidence.ps1" -Label "current gate receipt evidence script"
Require-Text -Text $currentGateText -Needle "Server-backed receipt evidence capture OK." -Label "current gate receipt evidence marker"

if ($failures.Count -eq 0) {
    New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
    $runnerLogPath = Join-Path $OutputDir "receipt-evidence-runner.log"
    $evidenceJsonPath = Join-Path $OutputDir "receipt-evidence.json"

    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $output = & $nodeCommand.Source $runnerPath $OutputDir 2>&1
        $exitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    $outputLines = @($output | ForEach-Object { $_.ToString() })
    Set-Content -LiteralPath $runnerLogPath -Value $outputLines -Encoding UTF8
    $joinedOutput = $outputLines -join [Environment]::NewLine

    if ($exitCode -ne 0) {
        Add-Failure "receipt evidence runner exited with code $exitCode."
        foreach ($line in $outputLines) {
            Add-Failure "  $line"
        }
    }
    else {
        foreach ($marker in @(
            "Server-backed receipt evidence OK.",
            "ReceiptAuthority: MainServer",
            "SignedSquadBeforeLaunch: true",
            "RewardClaimAfterDebrief: true",
            "DuplicateClaimReturnsSameLedgerEntry: true",
            "InventoryBalanceMutatedOnce: true",
            "RejectedClaimDoesNotMutateInventory: true",
            "LeaderboardProjectionFromAcceptedClaim: true",
            "NoBattleCoreFrameServerCalls: true",
            "NoUnityLaunch: true"
        )) {
            Require-Text -Text $joinedOutput -Needle $marker -Label "receipt evidence runner output"
        }
    }

    if (-not (Test-Path -LiteralPath $evidenceJsonPath -PathType Leaf)) {
        Add-Failure "receipt evidence json missing: $evidenceJsonPath"
    }
    else {
        $evidence = Get-Content -LiteralPath $evidenceJsonPath -Raw | ConvertFrom-Json
        Require-JsonValue -Object $evidence -Name "schema" -Expected "ServerBackedReceiptEvidence" -Label "receipt evidence schema"
        foreach ($flag in @(
            "ServerBackedReceiptEvidence",
            "ReceiptAuthorityMainServer",
            "SignedSquadBeforeLaunch",
            "RewardClaimAfterDebrief",
            "DuplicateClaimReturnsSameLedgerEntry",
            "DuplicateClaimReturnsSameClaimId",
            "InventoryBalanceMutatedOnce",
            "LeaderboardProjectionFromAcceptedClaim",
            "RejectedClaimDoesNotMutateInventory",
            "NoBattleCoreFrameServerCalls",
            "UnityOfflineFirst",
            "NoPaymentMarketplaceRealtimePvpChain",
            "NoUnityLaunch",
            "MobileFirstLandscapeOnly",
            "PortraitOutOfFirstSlice"
        )) {
            Require-JsonBool -Object $evidence.flags -Name $flag -Label "receipt evidence flag"
        }

        Require-JsonValue -Object $evidence.balances -Name "initial" -Expected 12000 -Label "receipt evidence balance"
        Require-JsonValue -Object $evidence.balances -Name "tokenDelta" -Expected 2310 -Label "receipt evidence balance"
        Require-JsonValue -Object $evidence.balances -Name "afterFirstClaim" -Expected 14310 -Label "receipt evidence balance"
        Require-JsonValue -Object $evidence.balances -Name "afterDuplicateClaim" -Expected 14310 -Label "receipt evidence balance"
        Require-JsonValue -Object $evidence.balances -Name "afterRejectedClaim" -Expected 14310 -Label "receipt evidence balance"
        Require-JsonValue -Object $evidence.balances -Name "final" -Expected 14310 -Label "receipt evidence balance"
        Require-JsonValue -Object $evidence.leaderboard -Name "rowsAfterApproved" -Expected 1 -Label "receipt evidence leaderboard"
        Require-JsonValue -Object $evidence.leaderboard -Name "rowsAfterRejected" -Expected 1 -Label "receipt evidence leaderboard"
        Require-JsonValue -Object $evidence.rejectedClaim -Name "statusCode" -Expected 400 -Label "receipt evidence rejected claim"
        Require-JsonValue -Object $evidence.rejectedClaim -Name "status" -Expected "rejected" -Label "receipt evidence rejected claim"
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Server-backed receipt evidence capture failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) server-backed receipt evidence check(s) failed."
}

Write-Host "Server-backed receipt evidence capture OK."
Write-Host "Repo: $RepoRoot"
Write-Host "OutputDir: $OutputDir"
$rows | Format-Table -AutoSize

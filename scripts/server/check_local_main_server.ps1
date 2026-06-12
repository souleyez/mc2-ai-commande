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

$nodeCommand = Get-Command node -ErrorAction SilentlyContinue
if ($null -eq $nodeCommand) {
    Add-Failure "node is required for the local main-server smoke."
}
else {
    Add-Row -Check "node" -Detail $nodeCommand.Source
}

$serverPath = Require-File -RelativePath "server\main-server\main-server.mjs"
$smokePath = Require-File -RelativePath "server\main-server\smoke.mjs"
$fixturePath = Require-File -RelativePath "server\main-server\fixtures\local-dev-fixture.json"
$packagePath = Require-File -RelativePath "server\main-server\package.json"

if ($serverPath) {
    $serverText = Get-Content -LiteralPath $serverPath -Raw
    $serverMarkers = @(
        "GET",
        "/healthz",
        "/version",
        "/dev/accounts",
        "/squads/sign",
        "/reward-claims",
        "/leaderboards/basic",
        "/dev/reset",
        "UNITY_OFFLINE_FIRST",
        "EXCLUDED_FIRST_SLICE_FEATURES",
        "SignedSquadLoadout",
        "RewardClaim",
        "TokenLedgerEntry",
        "LeaderboardRow"
    )

    foreach ($marker in $serverMarkers) {
        Require-Text -Text $serverText -Needle $marker -Label "local main-server source"
    }
}

if ($fixturePath) {
    $fixtureText = Get-Content -LiteralPath $fixturePath -Raw
    $fixtureMarkers = @(
        "AccountRecord",
        "PublicProfileRecord",
        "InventorySnapshot",
        "TokenLedgerEntry",
        "local-dev-account",
        "tokenBalance",
        "ownedMechs",
        "itemStacks"
    )

    foreach ($marker in $fixtureMarkers) {
        Require-Text -Text $fixtureText -Needle $marker -Label "local main-server fixture"
    }
}

if ($failures.Count -eq 0) {
    $serverDir = Resolve-RepoPath -RelativePath "server\main-server"
    $previousLocation = Get-Location
    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        Set-Location -LiteralPath $serverDir
        $output = & $nodeCommand.Source $smokePath 2>&1
        $exitCode = $LASTEXITCODE
    }
    finally {
        Set-Location -LiteralPath $previousLocation
        $ErrorActionPreference = $previousErrorActionPreference
    }

    $lines = @($output | ForEach-Object { $_.ToString() })
    $joined = $lines -join [Environment]::NewLine
    if ($exitCode -ne 0) {
        Add-Failure "Local main-server smoke exited with code $exitCode."
        foreach ($line in $lines) {
            Add-Failure "  $line"
        }
    }
    else {
        $smokeMarkers = @(
            "Local main-server smoke OK.",
            "Service: mc2-main-server-local",
            "GET /healthz",
            "GET /version",
            "POST /dev/accounts",
            "GET /accounts/{accountId}/inventory",
            "POST /squads/sign",
            "POST /reward-claims",
            "GET /leaderboards/basic",
            "POST /dev/reset",
            "UnityOfflineFirst: True",
            "NoRemoteUnityDependency: True",
            "NoPaymentMarketplaceRealtimePvpChain: True",
            "RewardClaimId:",
            "LeaderboardRows: 1"
        )

        foreach ($marker in $smokeMarkers) {
            Require-Text -Text $joined -Needle $marker -Label "local main-server smoke"
        }
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Local main-server prototype check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) local main-server prototype check(s) failed."
}

Write-Host "Local main-server prototype check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize

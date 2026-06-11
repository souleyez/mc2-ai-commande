param(
    [string]$RepoRoot = "",
    [string]$UnityPath = "",
    [string]$ProjectPath = "",
    [string]$LogPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
else {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
}

if ([string]::IsNullOrWhiteSpace($UnityPath)) {
    $UnityPath = Join-Path $HOME "Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe"
}

if ([string]::IsNullOrWhiteSpace($ProjectPath)) {
    $ProjectPath = Join-Path $RepoRoot "unity-mc2-demo"
}
else {
    $ProjectPath = (Resolve-Path -LiteralPath $ProjectPath).Path
}

if ([string]::IsNullOrWhiteSpace($LogPath)) {
    $LogPath = Join-Path $RepoRoot "analysis-output\unity-pc-core-playable-contract.log"
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
        [string]$Status,
        [string]$Detail
    )

    [void]$rows.Add([pscustomobject]@{
        Check = $Check
        Status = $Status
        Detail = $Detail
    })
}

function Assert-PathExists {
    param(
        [string]$Label,
        [string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        Add-Failure "$Label missing: $Path"
        return $false
    }

    Add-Row -Check $Label -Status "OK" -Detail $Path
    return $true
}

function Assert-LogContains {
    param(
        [string]$Label,
        [string]$Text,
        [string]$Marker
    )

    if ($Text.IndexOf($Marker, [StringComparison]::Ordinal) -lt 0) {
        Add-Failure "$Label missing marker: $Marker"
        return
    }

    Add-Row -Check $Label -Status "OK" -Detail $Marker
}

$unityOk = Assert-PathExists -Label "Unity" -Path $UnityPath
$projectOk = Assert-PathExists -Label "Unity project" -Path $ProjectPath

if ($unityOk -and $projectOk) {
    $logDirectory = Split-Path -Parent $LogPath
    if (-not (Test-Path -LiteralPath $logDirectory)) {
        New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
    }

    $unityArguments = @(
        "-batchmode",
        "-quit",
        "-projectPath",
        $ProjectPath,
        "-executeMethod",
        "MC2Demo.EditorTools.Mc2DemoValidator.ValidateMissionContract",
        "-logFile",
        $LogPath
    )

    $unityProcess = Start-Process `
        -FilePath $UnityPath `
        -ArgumentList $unityArguments `
        -Wait `
        -PassThru `
        -WindowStyle Hidden

    $exitCode = $unityProcess.ExitCode

    if ($exitCode -ne 0) {
        Add-Failure "Unity validator failed with exit code $exitCode."
    }
    else {
        Add-Row -Check "Unity validator" -Status "OK" -Detail "exit code 0"
    }

    if (-not (Test-Path -LiteralPath $LogPath)) {
        Add-Failure "Unity validator log missing: $LogPath"
    }
    else {
        Add-Row -Check "Unity validator log" -Status "OK" -Detail $LogPath
        $logText = Get-Content -LiteralPath $LogPath -Raw
        Assert-LogContains -Label "PC core marker" -Text $logText -Marker "MC2 PC core playable contract OK"
        Assert-LogContains -Label "Demo contract marker" -Text $logText -Marker "MC2 demo contract validation OK"
    }
}

if ($failures.Count -gt 0) {
    Write-Host "PC core playable contract check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    if (Test-Path -LiteralPath $LogPath) {
        Write-Host ""
        Write-Host "Last validator log lines:"
        Get-Content -LiteralPath $LogPath -Tail 80
    }

    throw "$($failures.Count) PC core playable contract check(s) failed."
}

Write-Host "PC core playable contract check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize

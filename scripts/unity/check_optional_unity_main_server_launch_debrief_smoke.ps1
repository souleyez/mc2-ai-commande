param(
    [string]$RepoRoot = "",
    [string]$ExePath = "",
    [string]$NodePath = "node",
    [int]$ServerPort = 8787,
    [int]$RuntimeTimeoutSeconds = 90
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
else {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
}

if ([string]::IsNullOrWhiteSpace($ExePath)) {
    $ExePath = Join-Path $RepoRoot "unity-mc2-demo\Builds\Windows\MC2UnityDemo.exe"
}

if ($RuntimeTimeoutSeconds -lt 20) {
    throw "RuntimeTimeoutSeconds must be at least 20."
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

function Test-MainServerReady {
    param([int]$Port)

    try {
        $response = Invoke-WebRequest -UseBasicParsing -TimeoutSec 2 -Uri "http://127.0.0.1:$Port/healthz"
        return $response.StatusCode -eq 200 -and $response.Content -like '*mc2-main-server-local*'
    }
    catch {
        return $false
    }
}

function Wait-MainServerReady {
    param(
        [int]$Port,
        [int]$TimeoutSeconds
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        if (Test-MainServerReady -Port $Port) {
            return $true
        }

        Start-Sleep -Milliseconds 250
    }

    return $false
}

function Invoke-UnityCommandFileSmoke {
    param(
        [string]$Name,
        [string]$CommandFile,
        [string]$LogPath,
        [string[]]$RequiredMarkers,
        [string[]]$AnyMarkers = @()
    )

    if (-not (Test-Path -LiteralPath $ExePath -PathType Leaf)) {
        Add-Failure "$Name missing Windows demo build: $ExePath"
        return
    }

    if (-not (Test-Path -LiteralPath $CommandFile -PathType Leaf)) {
        Add-Failure "$Name command file missing: $CommandFile"
        return
    }

    $logDirectory = Split-Path -Parent $LogPath
    if (-not (Test-Path -LiteralPath $logDirectory)) {
        New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
    }

    $arguments = @(
        "-batchmode",
        "-nographics",
        "-mc2SmokeTest",
        "-mc2CommandFile",
        $CommandFile,
        "-logFile",
        $LogPath
    )

    $process = Start-Process `
        -FilePath $ExePath `
        -ArgumentList $arguments `
        -WorkingDirectory (Split-Path -Parent $ExePath) `
        -PassThru `
        -WindowStyle Hidden

    if ($null -eq $process) {
        Add-Failure "$Name failed to start Unity player."
        return
    }

    if (-not $process.WaitForExit($RuntimeTimeoutSeconds * 1000)) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
        Add-Failure "$Name timed out after $RuntimeTimeoutSeconds second(s)."
        return
    }

    if ($process.ExitCode -ne 0) {
        Add-Failure "$Name Unity player exited with code $($process.ExitCode)."
    }

    if (-not (Test-Path -LiteralPath $LogPath -PathType Leaf)) {
        Add-Failure "$Name log missing: $LogPath"
        return
    }

    $logText = Get-Content -LiteralPath $LogPath -Raw
    foreach ($marker in $RequiredMarkers) {
        if ($logText -notlike "*$marker*") {
            Add-Failure "$Name log missing marker: $marker"
        }
    }

    if ($AnyMarkers.Count -gt 0) {
        $matched = $false
        foreach ($marker in $AnyMarkers) {
            if ($logText -like "*$marker*") {
                $matched = $true
                break
            }
        }

        if (-not $matched) {
            Add-Failure "$Name log missing one of: $($AnyMarkers -join ' | ')"
        }
    }

    Add-Row -Check $Name -Detail $LogPath
}

$bootstrap = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs"
$commanderScript = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\StartupCommanderScript.cs"
$adapter = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\BattleCore\UnityMainServerClient.cs"
$serverCommandFile = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-main-server-smoke.txt"
$fallbackCommandFile = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-main-server-fallback-smoke.txt"
$visibleFlow = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt"
$currentGate = Read-RequiredText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
$queue = Read-RequiredText -RelativePath "scripts\unity\check_current_plan_queue.ps1"
$masterPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md"
$detailedPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"
$mobilePlan = Read-RequiredText -RelativePath "docs-mobile-first-plan-2026-06-10.md"

foreach ($marker in @(
    "main-server-smoke",
    "assert-main-server-smoke",
    "MainServerSmoke",
    "AssertMainServerSmoke"
)) {
    Require-Text -Text $commanderScript -Needle $marker -Label "command parser"
}

foreach ($marker in @(
    "mainServerSmokeEnabled",
    "RunStartupMainServerSmoke",
    "TrySignMainServerSmokeSquadBeforeLaunch",
    "TrySubmitMainServerSmokeRewardClaimAfterDebrief",
    "BuildMainServerSmokeRewardClaim",
    "MC2 main-server smoke signed squad OK",
    "MC2 main-server smoke reward claim OK",
    "MC2 main-server smoke assertion OK",
    "SignedSquadBeforeLaunch: True",
    "RewardClaimAfterDebrief: True",
    "NoPerFrameServerCalls: "
)) {
    Require-Text -Text $bootstrap -Needle $marker -Label "bootstrap opt-in wiring"
}

foreach ($marker in @(
    "TrySignSquad",
    "TrySubmitRewardClaim",
    "NoPerFrameServerCalls = true",
    "DefaultEnabled = false"
)) {
    Require-Text -Text $adapter -Needle $marker -Label "adapter boundary"
}

foreach ($marker in @(
    "main-server-smoke",
    "mech-bay-launch",
    "complete-visible-objectives",
    "open-debrief",
    "assert-main-server-smoke"
)) {
    Require-Text -Text $serverCommandFile -Needle $marker -Label "server command file"
}

foreach ($marker in @(
    "main-server-smoke",
    "mech-bay-launch",
    "complete-visible-objectives",
    "open-debrief",
    "assert-debrief-summary"
)) {
    Require-Text -Text $fallbackCommandFile -Needle $marker -Label "fallback command file"
}

Forbid-Text -Text $fallbackCommandFile -Needle "assert-main-server-smoke" -Label "fallback command file"
Forbid-Text -Text $visibleFlow -Needle "main-server-smoke" -Label "default visible-flow command file"
Forbid-Text -Text $visibleFlow -Needle "assert-main-server-smoke" -Label "default visible-flow command file"
Forbid-Text -Text $adapter -Needle "Update(" -Label "adapter frame loop"
Forbid-Text -Text $adapter -Needle "FixedUpdate(" -Label "adapter frame loop"
Forbid-Text -Text $adapter -Needle "LateUpdate(" -Label "adapter frame loop"

Require-Text -Text $currentGate -Needle "Optional Unity main-server launch/debrief smoke check OK." -Label "current gate marker"
Require-Text -Text $queue -Needle "F10 wire optional Unity inventory bootstrap smoke" -Label "queue next task"
Require-Text -Text $masterPlan -Needle '| 87 | Done | `Wire optional Unity main-server adapter into launch/debrief smoke` |' -Label "master F9 done"
Require-Text -Text $masterPlan -Needle '| 88 | Next | `Wire optional Unity inventory bootstrap smoke` |' -Label "master F10 next"
Require-Text -Text $detailedPlan -Needle '| F9 | Done | `Wire optional Unity main-server adapter into launch/debrief smoke` |' -Label "detailed F9 done"
Require-Text -Text $detailedPlan -Needle '| F10 | Next | `Wire optional Unity inventory bootstrap smoke` |' -Label "detailed F10 next"
Require-Text -Text $mobilePlan -Needle "first phone version is landscape-only" -Label "mobile landscape invariant"

if ($failures.Count -eq 0) {
    $serverStarted = $false
    $serverProcess = $null
    $serverOut = Join-Path $RepoRoot "analysis-output\main-server-f9-smoke.out.log"
    $serverErr = Join-Path $RepoRoot "analysis-output\main-server-f9-smoke.err.log"
    $serverDir = Resolve-RepoPath -RelativePath "server\main-server"

    try {
        if (Test-MainServerReady -Port $ServerPort) {
            Add-Row -Check "local main-server" -Detail "already running on $ServerPort"
        }
        else {
            $serverProcess = Start-Process `
                -FilePath $NodePath `
                -ArgumentList @("main-server.mjs") `
                -WorkingDirectory $serverDir `
                -RedirectStandardOutput $serverOut `
                -RedirectStandardError $serverErr `
                -PassThru `
                -WindowStyle Hidden
            $serverStarted = $true

            if (-not (Wait-MainServerReady -Port $ServerPort -TimeoutSeconds 15)) {
                Add-Failure "local main-server did not become ready on port $ServerPort."
            }
            else {
                Add-Row -Check "local main-server" -Detail "started on $ServerPort"
            }
        }

        if ($failures.Count -eq 0) {
            Invoke-UnityCommandFileSmoke `
                -Name "Unity main-server launch/debrief smoke" `
                -CommandFile (Resolve-RepoPath -RelativePath "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-main-server-smoke.txt") `
                -LogPath (Resolve-RepoPath -RelativePath "analysis-output\unity-player-main-server-smoke.log") `
                -RequiredMarkers @(
                    "MC2 main-server smoke signed squad OK",
                    "MC2 main-server smoke reward claim OK",
                    "MC2 main-server smoke assertion OK",
                    "SignedSquadBeforeLaunch: True",
                    "RewardClaimAfterDebrief: True",
                    "NoPerFrameServerCalls: True",
                    "MC2 debrief summary assertion OK"
                )
        }
    }
    finally {
        if ($serverStarted -and $null -ne $serverProcess -and -not $serverProcess.HasExited) {
            Stop-Process -Id $serverProcess.Id -Force -ErrorAction SilentlyContinue
            $serverProcess.WaitForExit(5000) | Out-Null
        }
    }

    if (-not (Test-MainServerReady -Port $ServerPort)) {
        Invoke-UnityCommandFileSmoke `
            -Name "Unity main-server no-server fallback smoke" `
            -CommandFile (Resolve-RepoPath -RelativePath "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-main-server-fallback-smoke.txt") `
            -LogPath (Resolve-RepoPath -RelativePath "analysis-output\unity-player-main-server-fallback-smoke.log") `
            -RequiredMarkers @(
                "MC2 main-server smoke fallback",
                "LocalFixtureLaunchPlayable: True",
                "MC2 debrief summary assertion OK"
            )
    }
    else {
        Add-Failure "No-server fallback smoke could not run because a main-server is still reachable on port $ServerPort."
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Optional Unity main-server launch/debrief smoke check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) optional Unity main-server launch/debrief smoke check(s) failed."
}

Write-Host "Optional Unity main-server launch/debrief smoke check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize

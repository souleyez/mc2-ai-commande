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

function Reset-MainServer {
    param([int]$Port)

    try {
        $response = Invoke-WebRequest -UseBasicParsing -TimeoutSec 4 -Method POST -Uri "http://127.0.0.1:$Port/dev/reset"
        if ($response.StatusCode -eq 200 -and $response.Content -like '*AdminDevResetResponse*') {
            Add-Row -Check "local main-server reset" -Detail "POST /dev/reset"
            return
        }

        Add-Failure "local main-server reset returned unexpected response: $($response.StatusCode)"
    }
    catch {
        Add-Failure "local main-server reset failed: $($_.Exception.Message)"
    }
}

function Invoke-UnityCommandFileSmoke {
    param(
        [string]$Name,
        [string]$CommandFile,
        [string]$LogPath,
        [string[]]$RequiredMarkers,
        [string[]]$ForbiddenMarkers = @()
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

    foreach ($marker in $ForbiddenMarkers) {
        if ($logText -like "*$marker*") {
            Add-Failure "$Name log contains forbidden marker: $marker"
        }
    }

    Add-Row -Check $Name -Detail $LogPath
}

$bootstrap = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs"
$commanderScript = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\StartupCommanderScript.cs"
$adapter = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\BattleCore\UnityMainServerClient.cs"
$inventoryContract = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\BattleCore\MechBayInventoryContract.cs"
$fixture = Read-RequiredText -RelativePath "server\main-server\fixtures\local-dev-fixture.json"
$previewCommandFile = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-inventory-mechbay-preview-smoke.txt"
$fallbackCommandFile = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-inventory-mechbay-preview-fallback-smoke.txt"
$visibleFlow = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt"
$currentGate = Read-RequiredText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
$queue = Read-RequiredText -RelativePath "scripts\unity\check_current_plan_queue.ps1"
$handoffScript = Read-RequiredText -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"
$masterPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md"
$detailedPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"
$mobilePlan = Read-RequiredText -RelativePath "docs-mobile-first-plan-2026-06-10.md"
$handoff = Read-RequiredText -RelativePath "docs-machine-handoff-plan-2026-06-07.md"

foreach ($marker in @(
    "inventory-mechbay-preview-smoke",
    "assert-inventory-mechbay-preview-smoke",
    "InventoryMechBayPreviewSmoke",
    "AssertInventoryMechBayPreviewSmoke"
)) {
    Require-Text -Text $commanderScript -Needle $marker -Label "command parser"
}

foreach ($marker in @(
    "inventoryMechBayPreviewSmokeEnabled",
    "RunStartupInventoryMechBayPreviewSmoke",
    "TryApplyMainServerInventoryMechBayPreview",
    "RunStartupInventoryMechBayPreviewSmokeAssertion",
    "MC2 inventory-to-MechBay preview OK",
    "MC2 inventory-to-MechBay preview assertion OK",
    "MainServerPreviewApplied: ",
    "ProjectedInventoryValid: ",
    "InventorySource=MainServerPreview",
    "LocalFixtureInventoryPlayable: True",
    "ServerInventoryNotCombatAuthority: True",
    "NoPerFrameServerCalls: ",
    "MobileLandscapeOnly: True",
    "Inventory Source: Main Server Preview",
    "Inventory Source: Local Fixture"
)) {
    Require-Text -Text $bootstrap -Needle $marker -Label "preview binding wiring"
}

foreach ($marker in @(
    "UnityInventoryToMechBayProjector",
    "MechBayInventoryProjectionResult",
    "unitId",
    "unitType",
    "chassisId",
    "displayName",
    "pilotType",
    "equippedQuantity",
    "weapon",
    "armor",
    "heat-sink",
    "mech-fragment",
    "currency",
    "MechBayInventoryValidator.Validate",
    "FallbackUnknownItemCategory",
    "FallbackUnknownChassisOrLoadout",
    "FallbackPilotIncomplete",
    "FallbackServerPreviewRejected"
)) {
    Require-Text -Text $adapter -Needle $marker -Label "projector contract"
}

foreach ($marker in @(
    "BuildDemoInventory",
    "LoadoutItemCategory.Weapon",
    "LoadoutItemCategory.ArmorPlate",
    "LoadoutItemCategory.HeatSink",
    "LoadoutItemCategory.MechFragment"
)) {
    Require-Text -Text $inventoryContract -Needle $marker -Label "local MechBay fallback contract"
}

foreach ($marker in @(
    '"category": "currency"',
    '"category": "weapon"',
    '"category": "armor"',
    '"category": "heat-sink"',
    '"category": "mech-fragment"',
    '"equippedQuantity": 4',
    '"tokenBalance": 12000'
)) {
    Require-Text -Text $fixture -Needle $marker -Label "server fixture projection source"
}

foreach ($marker in @(
    "inventory-mechbay-preview-smoke",
    "assert-inventory-mechbay-preview-smoke",
    "mech-bay-launch",
    "assert-restart-identity depot",
    "complete-visible-objectives",
    "open-debrief",
    "assert-debrief-summary"
)) {
    Require-Text -Text $previewCommandFile -Needle $marker -Label "preview command file"
}

foreach ($marker in @(
    "inventory-mechbay-preview-smoke",
    "mech-bay-launch",
    "assert-restart-identity depot",
    "complete-visible-objectives",
    "open-debrief",
    "assert-debrief-summary"
)) {
    Require-Text -Text $fallbackCommandFile -Needle $marker -Label "preview fallback command file"
}

Forbid-Text -Text $fallbackCommandFile -Needle "assert-inventory-mechbay-preview-smoke" -Label "preview fallback command file"
Forbid-Text -Text $visibleFlow -Needle "inventory-mechbay-preview-smoke" -Label "default visible-flow command file"
Forbid-Text -Text $visibleFlow -Needle "assert-inventory-mechbay-preview-smoke" -Label "default visible-flow command file"
Forbid-Text -Text $adapter -Needle "Update(" -Label "adapter frame loop"
Forbid-Text -Text $adapter -Needle "FixedUpdate(" -Label "adapter frame loop"
Forbid-Text -Text $adapter -Needle "LateUpdate(" -Label "adapter frame loop"

Require-Text -Text $currentGate -Needle "Optional inventory-to-MechBay preview binding check OK." -Label "current gate marker"
Require-Text -Text $queue -Needle "F12 implement opt-in inventory-to-MechBay preview binding" -Label "queue completed task"
Require-Text -Text $queue -Needle "F13 capture opt-in MechBay preview evidence" -Label "queue completed task"
Require-Text -Text $queue -Needle "F14 capture landscape-phone MechLab source-line evidence" -Label "queue completed task"
Require-Text -Text $queue -Needle "F15 plan server-backed receipt slice" -Label "queue completed task"
Require-Text -Text $queue -Needle "F16 implement server-backed receipt evidence gate" -Label "queue completed task"
Require-Text -Text $queue -Needle "F17 plan post-receipt inventory refresh boundary" -Label "queue completed task"
Require-Text -Text $queue -Needle "F18 implement opt-in post-receipt inventory refresh binding" -Label "queue completed task"
Require-Text -Text $queue -Needle "F19 capture opt-in post-receipt refresh evidence" -Label "queue completed task"
Require-Text -Text $queue -Needle "F20 refresh Android landscape build/smoke evidence" -Label "queue F20 completed task"
Require-Text -Text $queue -Needle "F23 capture landscape MechLab touch evidence" -Label "queue completed task"
Require-Text -Text $queue -Needle "F24 capture Android MechLab touch evidence" -Label "queue next task"
Require-Text -Text $handoffScript -Needle "Optional inventory-to-MechBay preview binding check OK" -Label "handoff script marker"
Require-Text -Text $masterPlan -Needle '| 90 | Done | `Implement opt-in inventory-to-MechBay preview binding` |' -Label "master F12 done"
Require-Text -Text $masterPlan -Needle '| 91 | Done | `Capture opt-in MechBay preview evidence` |' -Label "master F13 done"
Require-Text -Text $masterPlan -Needle '| 92 | Done | `Capture landscape-phone MechLab source-line evidence` |' -Label "master F14 done"
Require-Text -Text $masterPlan -Needle '| 93 | Done | `Plan server-backed receipt slice` |' -Label "master F15 done"
Require-Text -Text $masterPlan -Needle '| 94 | Done | `Implement server-backed receipt evidence gate` |' -Label "master F16 done"
Require-Text -Text $masterPlan -Needle '| 95 | Done | `Plan post-receipt inventory refresh boundary` |' -Label "master F17 done"
Require-Text -Text $masterPlan -Needle '| 96 | Done | `Implement opt-in post-receipt inventory refresh binding` |' -Label "master F18 done"
Require-Text -Text $masterPlan -Needle '| 97 | Done | `Capture opt-in post-receipt refresh evidence` |' -Label "master F19 done"
Require-Text -Text $masterPlan -Needle '| 98 | Done | `Refresh Android landscape build/smoke evidence` |' -Label "master F20 done"
Require-Text -Text $masterPlan -Needle '| 101 | Done | `Capture landscape MechLab touch evidence` |' -Label "master F23 done"
Require-Text -Text $masterPlan -Needle '| 102 | Done | `Capture Android MechLab touch evidence` |' -Label "master F24 done"
Require-Text -Text $detailedPlan -Needle '| F12 | Done | `Implement opt-in inventory-to-MechBay preview binding` |' -Label "detailed F12 done"
Require-Text -Text $detailedPlan -Needle '| F13 | Done | `Capture opt-in MechBay preview evidence` |' -Label "detailed F13 done"
Require-Text -Text $detailedPlan -Needle '| F14 | Done | `Capture landscape-phone MechLab source-line evidence` |' -Label "detailed F14 done"
Require-Text -Text $detailedPlan -Needle '| F15 | Done | `Plan server-backed receipt slice` |' -Label "detailed F15 done"
Require-Text -Text $detailedPlan -Needle '| F16 | Done | `Implement server-backed receipt evidence gate` |' -Label "detailed F16 done"
Require-Text -Text $detailedPlan -Needle '| F17 | Done | `Plan post-receipt inventory refresh boundary` |' -Label "detailed F17 done"
Require-Text -Text $detailedPlan -Needle '| F18 | Done | `Implement opt-in post-receipt inventory refresh binding` |' -Label "detailed F18 done"
Require-Text -Text $detailedPlan -Needle '| F19 | Done | `Capture opt-in post-receipt refresh evidence` |' -Label "detailed F19 done"
Require-Text -Text $detailedPlan -Needle '| F20 | Done | `Refresh Android landscape build/smoke evidence` |' -Label "detailed F20 done"
Require-Text -Text $detailedPlan -Needle '| F23 | Done | `Capture landscape MechLab touch evidence` |' -Label "detailed F23 done"
Require-Text -Text $detailedPlan -Needle '| F24 | Done | `Capture Android MechLab touch evidence` |' -Label "detailed F24 done"
Require-Text -Text $mobilePlan -Needle "F12 implement opt-in inventory-to-MechBay preview binding" -Label "mobile completed task"
Require-Text -Text $mobilePlan -Needle "F13 capture opt-in MechBay preview evidence" -Label "mobile completed task"
Require-Text -Text $mobilePlan -Needle "F14 capture landscape-phone MechLab source-line evidence" -Label "mobile completed task"
Require-Text -Text $mobilePlan -Needle "F15 plan server-backed receipt slice" -Label "mobile completed task"
Require-Text -Text $mobilePlan -Needle "F16 implement server-backed receipt evidence gate" -Label "mobile completed task"
Require-Text -Text $mobilePlan -Needle "F17 plan post-receipt inventory refresh boundary" -Label "mobile completed task"
Require-Text -Text $mobilePlan -Needle "F18 implement opt-in post-receipt inventory refresh binding" -Label "mobile completed task"
Require-Text -Text $mobilePlan -Needle "F19 capture opt-in post-receipt refresh evidence" -Label "mobile completed task"
Require-Text -Text $mobilePlan -Needle "F20 refresh Android landscape build/smoke evidence" -Label "mobile F20 completed task"
Require-Text -Text $mobilePlan -Needle "F23 capture landscape MechLab touch evidence" -Label "mobile completed task"
Require-Text -Text $mobilePlan -Needle "F24 capture Android MechLab touch evidence" -Label "mobile next task"
Require-Text -Text $mobilePlan -Needle "first phone version is landscape-only" -Label "mobile landscape invariant"
Require-Text -Text $handoff -Needle 'Current formal next development task after handoff: `F50 audit post-F49 PC controlled-demo investor route evidence refresh`' -Label "handoff next task"
Require-Text -Text $handoff -Needle 'Next planned work: `F50 audit post-F49 PC controlled-demo investor route evidence refresh`' -Label "handoff next planned work"

if ($failures.Count -eq 0) {
    $serverStarted = $false
    $serverProcess = $null
    $serverOut = Join-Path $RepoRoot "analysis-output\main-server-f12-preview.out.log"
    $serverErr = Join-Path $RepoRoot "analysis-output\main-server-f12-preview.err.log"
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
            Reset-MainServer -Port $ServerPort
            Invoke-UnityCommandFileSmoke `
                -Name "Unity inventory-to-MechBay preview smoke" `
                -CommandFile (Resolve-RepoPath -RelativePath "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-inventory-mechbay-preview-smoke.txt") `
                -LogPath (Resolve-RepoPath -RelativePath "analysis-output\unity-player-inventory-mechbay-preview-smoke.log") `
                -RequiredMarkers @(
                    "MC2 inventory-to-MechBay preview OK",
                    "MC2 inventory-to-MechBay preview assertion OK",
                    "MainServerPreviewApplied: True",
                    "ProjectedInventoryValid: True",
                    "InventorySource=MainServerPreview",
                    "tokenBalance=12000",
                    "ownedMechs=3",
                    "serverItemStacks=6",
                    "projectedItemStacks=5",
                    "skippedCurrencyStacks=1",
                    "ServerInventoryNotCombatAuthority: True",
                    "NoPerFrameServerCalls: True",
                    "MobileLandscapeOnly: True",
                    "MC2 debrief summary assertion OK"
                ) `
                -ForbiddenMarkers @(
                    "RewardClaimAfterDebrief: True"
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
            -Name "Unity inventory-to-MechBay preview no-server fallback smoke" `
            -CommandFile (Resolve-RepoPath -RelativePath "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-inventory-mechbay-preview-fallback-smoke.txt") `
            -LogPath (Resolve-RepoPath -RelativePath "analysis-output\unity-player-inventory-mechbay-preview-fallback-smoke.log") `
            -RequiredMarkers @(
                "MC2 inventory-to-MechBay preview fallback",
                "LocalFixtureInventoryPlayable: True",
                "NoPerFrameServerCalls: True",
                "MobileLandscapeOnly: True",
                "MC2 debrief summary assertion OK"
            )
    }
    else {
        Add-Failure "No-server fallback smoke could not run because a main-server is still reachable on port $ServerPort."
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Optional inventory-to-MechBay preview binding check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) optional inventory-to-MechBay preview binding check(s) failed."
}

Write-Host "Optional inventory-to-MechBay preview binding check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize

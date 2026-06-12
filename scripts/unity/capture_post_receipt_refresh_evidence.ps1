param(
    [string]$RepoRoot = "",
    [string]$ExePath = "",
    [string]$NodePath = "node",
    [int]$ServerPort = 8787,
    [string]$OutputDir = "",
    [int]$Width = 2400,
    [int]$Height = 1080,
    [int]$RuntimeTimeoutSeconds = 120,
    [int]$MinimumPngBytes = 160000
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

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot "analysis-output\post-receipt-refresh-evidence"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
}

if ($RuntimeTimeoutSeconds -lt 30) {
    throw "RuntimeTimeoutSeconds must be at least 30."
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

function Convert-ToRepoRelativePath {
    param([string]$Path)

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $repoRootFull = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd("\", "/")
    if (-not $fullPath.StartsWith($repoRootFull, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Path is outside RepoRoot: $Path"
    }

    return $fullPath.Substring($repoRootFull.Length).TrimStart("\", "/") -replace "\\", "/"
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

function Assert-IgnoredGeneratedPath {
    param(
        [string]$RelativePath,
        [string]$Label
    )

    & git -C $RepoRoot check-ignore -q -- $RelativePath
    if ($LASTEXITCODE -ne 0) {
        Add-Failure "$Label is not ignored by git: $RelativePath"
        return
    }

    Add-Row -Check "$Label gitignore" -Detail $RelativePath
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

function Remove-StaleEvidenceFile {
    param([string]$Path)

    $full = [System.IO.Path]::GetFullPath($Path)
    $outputRoot = [System.IO.Path]::GetFullPath($OutputDir)
    if (-not $full.StartsWith($outputRoot, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to clean evidence outside output dir: $Path"
    }

    if (Test-Path -LiteralPath $full -PathType Leaf) {
        Remove-Item -LiteralPath $full -Force
    }
}

function Join-ProcessArguments {
    param([string[]]$Arguments)

    $escaped = foreach ($argument in $Arguments) {
        if ($null -eq $argument) {
            '""'
            continue
        }

        '"' + ($argument -replace '\\(?=\\*")', '$0$0' -replace '"', '\"') + '"'
    }

    return ($escaped -join " ")
}

function Invoke-UnityPostReceiptCapture {
    param(
        [string]$Name,
        [string]$CommandFile,
        [string]$PngPath,
        [string]$JsonPath,
        [string]$LogPath,
        [string]$Preset
    )

    if (-not (Test-Path -LiteralPath $ExePath -PathType Leaf)) {
        Add-Failure "$Name missing Windows demo build: $ExePath"
        return
    }

    if (-not (Test-Path -LiteralPath $CommandFile -PathType Leaf)) {
        Add-Failure "$Name command file missing: $CommandFile"
        return
    }

    $dataDir = Join-Path (Split-Path -Parent $ExePath) "MC2UnityDemo_Data"
    if (-not (Test-Path -LiteralPath $dataDir)) {
        Add-Failure "$Name missing Unity data folder beside executable: $dataDir"
        return
    }

    foreach ($path in @($PngPath, $JsonPath, $LogPath)) {
        Remove-StaleEvidenceFile -Path $path
    }

    $arguments = @(
        "-screen-width", $Width.ToString([Globalization.CultureInfo]::InvariantCulture),
        "-screen-height", $Height.ToString([Globalization.CultureInfo]::InvariantCulture),
        "-screen-fullscreen", "0",
        "-mc2CommandFile", $CommandFile,
        "-mc2CapturePreset", $Preset,
        "-mc2CaptureScreenshot", $PngPath,
        "-mc2CaptureSidecar", $JsonPath,
        "-mc2CaptureQuit",
        "-logFile", $LogPath
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $ExePath
    $startInfo.WorkingDirectory = Split-Path -Parent $ExePath
    $startInfo.Arguments = Join-ProcessArguments -Arguments $arguments

    $process = [System.Diagnostics.Process]::Start($startInfo)
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

    Add-Row -Check $Name -Detail $LogPath
}

function Test-CaptureImage {
    param(
        [string]$Path,
        [string]$Label
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Add-Failure "$Label screenshot missing: $Path"
        return
    }

    $png = Get-Item -LiteralPath $Path
    if ($png.Length -lt $MinimumPngBytes) {
        Add-Failure "$Label screenshot too small: $($png.Length) bytes, expected at least $MinimumPngBytes"
        return
    }

    Add-Type -AssemblyName System.Drawing
    $bitmap = [System.Drawing.Bitmap]::new($Path)
    try {
        if ($bitmap.Width -ne $Width -or $bitmap.Height -ne $Height) {
            Add-Failure "$Label screenshot size mismatch: $($bitmap.Width)x$($bitmap.Height), expected ${Width}x${Height}"
            return
        }

        if ($bitmap.Width -le $bitmap.Height) {
            Add-Failure "$Label screenshot must be landscape: $($bitmap.Width)x$($bitmap.Height)"
            return
        }

        $aspect = [Math]::Round($bitmap.Width / [double]$bitmap.Height, 3)
        if ($aspect -lt 2.0 -or $aspect -gt 2.4) {
            Add-Failure "$Label screenshot aspect is not a landscape phone ratio: $aspect"
            return
        }

        $unique = New-Object 'System.Collections.Generic.HashSet[int]'
        for ($y = 0; $y -lt $bitmap.Height; $y += 12) {
            for ($x = 0; $x -lt $bitmap.Width; $x += 12) {
                [void]$unique.Add($bitmap.GetPixel($x, $y).ToArgb())
            }
        }

        if ($unique.Count -lt 80) {
            Add-Failure "$Label screenshot looks too flat: uniqueColors=$($unique.Count)"
            return
        }

        Add-Row -Check "$Label screenshot" -Detail "$($bitmap.Width)x$($bitmap.Height) aspect=$aspect bytes=$($png.Length) uniqueColors=$($unique.Count)"
    }
    finally {
        $bitmap.Dispose()
    }
}

function Read-Sidecar {
    param(
        [string]$Path,
        [string]$Label
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Add-Failure "$Label sidecar missing: $Path"
        return $null
    }

    try {
        $sidecar = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
        Add-Row -Check "$Label sidecar json" -Detail $Path
        return $sidecar
    }
    catch {
        Add-Failure "$Label sidecar is not valid JSON: $Path"
        return $null
    }
}

function Require-SidecarText {
    param(
        [object]$Sidecar,
        [string]$Property,
        [string]$Needle,
        [string]$Label
    )

    if ($null -eq $Sidecar) {
        return
    }

    $value = [string]$Sidecar.$Property
    if ($value -notlike "*$Needle*") {
        Add-Failure "$Label missing '$Needle': $value"
        return
    }

    Add-Row -Check $Label -Detail $Needle
}

function Test-CommonSidecar {
    param(
        [object]$Sidecar,
        [string]$Label,
        [string]$ExpectedFlow,
        [string]$ExpectedPreset
    )

    if ($null -eq $Sidecar) {
        return
    }

    if ($Sidecar.preset -ne $ExpectedPreset) {
        Add-Failure "$Label sidecar preset mismatch: $($Sidecar.preset)"
    }

    if ($Sidecar.flowScreen -ne $ExpectedFlow) {
        Add-Failure "$Label sidecar flow mismatch: $($Sidecar.flowScreen), expected $ExpectedFlow"
    }

    if ([int]$Sidecar.screenWidth -ne $Width -or [int]$Sidecar.screenHeight -ne $Height) {
        Add-Failure "$Label sidecar size mismatch: $($Sidecar.screenWidth)x$($Sidecar.screenHeight)"
    }

    Require-SidecarText -Sidecar $Sidecar -Property "mobileTouch" -Needle "orientation=landscape" -Label "$Label mobile touch"
    Require-SidecarText -Sidecar $Sidecar -Property "mobileTouch" -Needle "landscapeOnly=yes" -Label "$Label mobile touch"
    Require-SidecarText -Sidecar $Sidecar -Property "mobileTouch" -Needle "current=${Width}x${Height}" -Label "$Label mobile touch"
    Require-SidecarText -Sidecar $Sidecar -Property "postReceiptInventoryRefresh" -Needle "NoPerFrameServerCalls: True" -Label "$Label post-receipt"
    Require-SidecarText -Sidecar $Sidecar -Property "postReceiptInventoryRefresh" -Needle "MobileLandscapeOnly: True" -Label "$Label post-receipt"
}

function Test-Log {
    param(
        [string]$Path,
        [string[]]$Markers,
        [string]$Label
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Add-Failure "$Label log missing: $Path"
        return
    }

    $logText = Get-Content -LiteralPath $Path -Raw
    foreach ($marker in $Markers) {
        if ($logText -notlike "*$marker*") {
            Add-Failure "$Label log missing marker: $marker"
        }
        else {
            Add-Row -Check "$Label log" -Detail $marker
        }
    }
}

if ($Width -le $Height) {
    Add-Failure "Width/Height must be landscape for phone evidence: ${Width}x${Height}"
}

$aspect = [Math]::Round($Width / [double]$Height, 3)
if ($aspect -lt 2.0 -or $aspect -gt 2.4) {
    Add-Failure "Width/Height must be a landscape phone aspect ratio, got $aspect from ${Width}x${Height}"
}

$repoRootFull = [System.IO.Path]::GetFullPath($RepoRoot)
if (-not $OutputDir.StartsWith($repoRootFull, [StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDir must stay inside RepoRoot: $OutputDir"
}

New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

$bootstrap = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs"
$commanderScript = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\StartupCommanderScript.cs"
$mainServerClient = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\BattleCore\UnityMainServerClient.cs"
$mainServerCommandFileText = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-main-server-smoke.txt"
$postReceiptCommandFileText = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-post-receipt-refresh-evidence.txt"
$visibleFlow = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt"
$gitignore = Read-RequiredText -RelativePath ".gitignore"
$currentGate = Read-RequiredText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
$queue = Read-RequiredText -RelativePath "scripts\unity\check_current_plan_queue.ps1"
$masterPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md"
$detailedPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"
$mobilePlan = Read-RequiredText -RelativePath "docs-mobile-first-plan-2026-06-10.md"
$handoff = Read-RequiredText -RelativePath "docs-machine-handoff-plan-2026-06-07.md"

foreach ($marker in @(
    "post-receipt-duplicate-smoke",
    "assert-post-receipt-refresh-smoke",
    "open-mech-lab",
    "PostReceiptDuplicateSmoke",
    "AssertPostReceiptRefreshSmoke",
    "OpenMechLab"
)) {
    Require-Text -Text $commanderScript -Needle $marker -Label "command parser F19"
}

foreach ($marker in @(
    "debriefReward",
    "postReceiptInventoryRefresh",
    "BuildCaptureDebriefRewardSummary",
    "BuildCapturePostReceiptInventoryRefreshSummary",
    "RunStartupPostReceiptDuplicateSmoke",
    "RunStartupPostReceiptRefreshSmokeAssertion",
    "MC2 post-receipt duplicate no-double-apply OK",
    "MC2 post-receipt refresh evidence assertion OK",
    "Inventory Source: Main Server Reward"
)) {
    Require-Text -Text $bootstrap -Needle $marker -Label "bootstrap F19 evidence"
}

foreach ($marker in @(
    "UnityPostReceiptInventoryRefreshProjector",
    "MessageServerRewardAlreadyApplied",
    "DuplicateLedgerAlreadyApplied",
    "CanApplyInventory"
)) {
    Require-Text -Text $mainServerClient -Needle $marker -Label "post-receipt projector"
}

foreach ($marker in @(
    "main-server-smoke",
    "complete-visible-objectives",
    "open-debrief",
    "assert-main-server-smoke"
)) {
    Require-Text -Text $mainServerCommandFileText -Needle $marker -Label "normal reward command file"
}

foreach ($marker in @(
    "main-server-smoke",
    "complete-visible-objectives",
    "open-debrief",
    "assert-main-server-smoke",
    "post-receipt-duplicate-smoke",
    "assert-post-receipt-refresh-smoke",
    "open-mech-lab",
    "assert-loadout-compact"
)) {
    Require-Text -Text $postReceiptCommandFileText -Needle $marker -Label "duplicate reward command file"
}

Forbid-Text -Text $visibleFlow -Needle "post-receipt-duplicate-smoke" -Label "default visible-flow command file"
Forbid-Text -Text $visibleFlow -Needle "assert-post-receipt-refresh-smoke" -Label "default visible-flow command file"
Require-Text -Text $gitignore -Needle "analysis-output/post-receipt-refresh-evidence/" -Label "generated post-receipt evidence ignore"
Require-Text -Text $currentGate -Needle "capture_post_receipt_refresh_evidence.ps1" -Label "current gate F19 script"
Require-Text -Text $currentGate -Needle "Post-receipt refresh evidence capture OK." -Label "current gate F19 marker"

foreach ($marker in @(
    "F18 implement opt-in post-receipt inventory refresh binding",
    "F19 capture opt-in post-receipt refresh evidence"
)) {
    Require-Text -Text ($queue + $masterPlan + $detailedPlan + $mobilePlan + $handoff) -Needle $marker -Label "plan queue F19"
}

Assert-IgnoredGeneratedPath -RelativePath (Convert-ToRepoRelativePath -Path (Join-Path $OutputDir "debrief.png")) -Label "post-receipt debrief screenshot"
Assert-IgnoredGeneratedPath -RelativePath (Convert-ToRepoRelativePath -Path (Join-Path $OutputDir "debrief.json")) -Label "post-receipt debrief sidecar"
Assert-IgnoredGeneratedPath -RelativePath (Convert-ToRepoRelativePath -Path (Join-Path $OutputDir "debrief.log")) -Label "post-receipt debrief log"
Assert-IgnoredGeneratedPath -RelativePath (Convert-ToRepoRelativePath -Path (Join-Path $OutputDir "mechlab-duplicate.png")) -Label "post-receipt MechLab screenshot"
Assert-IgnoredGeneratedPath -RelativePath (Convert-ToRepoRelativePath -Path (Join-Path $OutputDir "mechlab-duplicate.json")) -Label "post-receipt MechLab sidecar"
Assert-IgnoredGeneratedPath -RelativePath (Convert-ToRepoRelativePath -Path (Join-Path $OutputDir "mechlab-duplicate.log")) -Label "post-receipt MechLab log"

if ($failures.Count -eq 0) {
    $serverStarted = $false
    $serverProcess = $null
    $serverOut = Join-Path $OutputDir "main-server.out.log"
    $serverErr = Join-Path $OutputDir "main-server.err.log"
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
            Invoke-UnityPostReceiptCapture `
                -Name "Unity post-receipt debrief reward capture" `
                -CommandFile (Resolve-RepoPath -RelativePath "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-main-server-smoke.txt") `
                -PngPath (Join-Path $OutputDir "debrief.png") `
                -JsonPath (Join-Path $OutputDir "debrief.json") `
                -LogPath (Join-Path $OutputDir "debrief.log") `
                -Preset "debrief"

            $debriefSidecar = Read-Sidecar -Path (Join-Path $OutputDir "debrief.json") -Label "debrief reward"
            Test-CaptureImage -Path (Join-Path $OutputDir "debrief.png") -Label "debrief reward"
            Test-CommonSidecar -Sidecar $debriefSidecar -Label "debrief reward" -ExpectedFlow "Debrief" -ExpectedPreset "debrief"
            Require-SidecarText -Sidecar $debriefSidecar -Property "debriefReward" -Needle "DebriefReward=ready" -Label "debrief reward sidecar"
            Require-SidecarText -Sidecar $debriefSidecar -Property "debriefReward" -Needle "serverRewardLine=Server Reward +" -Label "debrief reward sidecar"
            Require-SidecarText -Sidecar $debriefSidecar -Property "debriefReward" -Needle "MainServerRewardText: True" -Label "debrief reward sidecar"
            Require-SidecarText -Sidecar $debriefSidecar -Property "postReceiptInventoryRefresh" -Needle "PostReceiptInventoryRefresh=ready" -Label "debrief post-receipt sidecar"
            Require-SidecarText -Sidecar $debriefSidecar -Property "postReceiptInventoryRefresh" -Needle "MainServerRewardApplied: True" -Label "debrief post-receipt sidecar"
            Require-SidecarText -Sidecar $debriefSidecar -Property "postReceiptInventoryRefresh" -Needle "InventorySourceLine=Inventory Source: Main Server Reward" -Label "debrief post-receipt sidecar"
            Test-Log -Path (Join-Path $OutputDir "debrief.log") -Label "debrief reward" -Markers @(
                "MC2 main-server smoke reward claim OK",
                "MC2 post-receipt inventory refresh OK",
                "MC2 main-server smoke assertion OK",
                "PostReceiptInventoryRefresh: True",
                "MC2 capture sidecar written:"
            )

            Reset-MainServer -Port $ServerPort
            Invoke-UnityPostReceiptCapture `
                -Name "Unity post-receipt duplicate MechLab capture" `
                -CommandFile (Resolve-RepoPath -RelativePath "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-post-receipt-refresh-evidence.txt") `
                -PngPath (Join-Path $OutputDir "mechlab-duplicate.png") `
                -JsonPath (Join-Path $OutputDir "mechlab-duplicate.json") `
                -LogPath (Join-Path $OutputDir "mechlab-duplicate.log") `
                -Preset "mechlab"

            $mechLabSidecar = Read-Sidecar -Path (Join-Path $OutputDir "mechlab-duplicate.json") -Label "duplicate MechLab"
            Test-CaptureImage -Path (Join-Path $OutputDir "mechlab-duplicate.png") -Label "duplicate MechLab"
            Test-CommonSidecar -Sidecar $mechLabSidecar -Label "duplicate MechLab" -ExpectedFlow "Mech Lab" -ExpectedPreset "mechlab"
            if ($null -ne $mechLabSidecar -and [string]$mechLabSidecar.mechLabInventorySource -ne "Inventory Source: Main Server Reward") {
                Add-Failure "duplicate MechLab source line mismatch: $($mechLabSidecar.mechLabInventorySource)"
            }
            else {
                Add-Row -Check "duplicate MechLab source line" -Detail "Inventory Source: Main Server Reward"
            }

            Require-SidecarText -Sidecar $mechLabSidecar -Property "postReceiptInventoryRefresh" -Needle "PostReceiptInventoryRefresh=duplicate" -Label "duplicate post-receipt sidecar"
            Require-SidecarText -Sidecar $mechLabSidecar -Property "postReceiptInventoryRefresh" -Needle "DuplicateClaimNoDoubleApplyLocalRefresh: True" -Label "duplicate post-receipt sidecar"
            Require-SidecarText -Sidecar $mechLabSidecar -Property "postReceiptInventoryRefresh" -Needle "InventorySourceLine=Inventory Source: Main Server Reward" -Label "duplicate post-receipt sidecar"
            Require-SidecarText -Sidecar $mechLabSidecar -Property "debriefReward" -Needle "Server Reward Already Applied" -Label "duplicate debrief reward sidecar"
            Require-SidecarText -Sidecar $mechLabSidecar -Property "debriefReward" -Needle "DuplicateRewardText: True" -Label "duplicate debrief reward sidecar"
            Require-SidecarText -Sidecar $mechLabSidecar -Property "mechLab" -Needle "MechLabCapture=open" -Label "duplicate MechLab sidecar"
            Test-Log -Path (Join-Path $OutputDir "mechlab-duplicate.log") -Label "duplicate MechLab" -Markers @(
                "MC2 post-receipt duplicate refresh OK",
                "MC2 post-receipt duplicate no-double-apply OK",
                "MC2 post-receipt refresh evidence assertion OK",
                "MC2 Mech Lab open OK",
                "MC2 loadout compact assertion OK",
                "MC2 capture sidecar written:"
            )
        }
    }
    finally {
        if ($serverStarted -and $null -ne $serverProcess -and -not $serverProcess.HasExited) {
            Stop-Process -Id $serverProcess.Id -Force -ErrorAction SilentlyContinue
            $serverProcess.WaitForExit(5000) | Out-Null
        }
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Post-receipt refresh evidence capture failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) post-receipt refresh evidence check(s) failed."
}

Write-Host "Post-receipt refresh evidence capture OK."
Write-Host "Repo: $RepoRoot"
Write-Host "OutputDir: $OutputDir"
$rows | Format-Table -AutoSize

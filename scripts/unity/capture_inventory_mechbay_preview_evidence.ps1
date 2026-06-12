param(
    [string]$RepoRoot = "",
    [string]$ExePath = "",
    [string]$NodePath = "node",
    [int]$ServerPort = 8787,
    [string]$OutputDir = "",
    [int]$Width = 1280,
    [int]$Height = 720,
    [int]$RuntimeTimeoutSeconds = 90,
    [int]$MinimumPngBytes = 100000
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
    $OutputDir = Join-Path $RepoRoot "analysis-output\inventory-mechbay-preview-evidence"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
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

function Invoke-UnityMechLabPreviewCapture {
    param(
        [string]$CommandFile,
        [string]$PngPath,
        [string]$JsonPath,
        [string]$LogPath
    )

    if (-not (Test-Path -LiteralPath $ExePath -PathType Leaf)) {
        Add-Failure "Missing Windows demo build: $ExePath"
        return
    }

    $dataDir = Join-Path (Split-Path -Parent $ExePath) "MC2UnityDemo_Data"
    if (-not (Test-Path -LiteralPath $dataDir)) {
        Add-Failure "Missing Unity data folder beside executable: $dataDir"
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
        "-mc2CapturePreset", "mechlab",
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
        Add-Failure "Failed to start Unity player for MechBay preview capture."
        return
    }

    if (-not $process.WaitForExit($RuntimeTimeoutSeconds * 1000)) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
        Add-Failure "Unity MechBay preview capture timed out after $RuntimeTimeoutSeconds second(s)."
        return
    }

    if ($process.ExitCode -ne 0) {
        Add-Failure "Unity MechBay preview capture exited with code $($process.ExitCode)."
    }

    Add-Row -Check "Unity MechBay preview capture" -Detail $LogPath
}

function Test-CaptureImage {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Add-Failure "Evidence screenshot missing: $Path"
        return
    }

    $png = Get-Item -LiteralPath $Path
    if ($png.Length -lt $MinimumPngBytes) {
        Add-Failure "Evidence screenshot too small: $($png.Length) bytes, expected at least $MinimumPngBytes"
        return
    }

    Add-Type -AssemblyName System.Drawing
    $bitmap = [System.Drawing.Bitmap]::new($Path)
    try {
        if ($bitmap.Width -ne $Width -or $bitmap.Height -ne $Height) {
            Add-Failure "Evidence screenshot size mismatch: $($bitmap.Width)x$($bitmap.Height), expected ${Width}x${Height}"
            return
        }

        if ($bitmap.Width -le $bitmap.Height) {
            Add-Failure "Evidence screenshot must be landscape: $($bitmap.Width)x$($bitmap.Height)"
            return
        }

        $unique = New-Object 'System.Collections.Generic.HashSet[int]'
        for ($y = 0; $y -lt $bitmap.Height; $y += 8) {
            for ($x = 0; $x -lt $bitmap.Width; $x += 8) {
                [void]$unique.Add($bitmap.GetPixel($x, $y).ToArgb())
            }
        }

        if ($unique.Count -lt 64) {
            Add-Failure "Evidence screenshot looks too flat: uniqueColors=$($unique.Count)"
            return
        }

        Add-Row -Check "evidence screenshot" -Detail "$($bitmap.Width)x$($bitmap.Height) bytes=$($png.Length) uniqueColors=$($unique.Count)"
    }
    finally {
        $bitmap.Dispose()
    }
}

function Test-CaptureSidecar {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Add-Failure "Evidence sidecar missing: $Path"
        return
    }

    try {
        $sidecar = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
    }
    catch {
        Add-Failure "Evidence sidecar is not valid JSON: $Path"
        return
    }

    if ($sidecar.preset -ne "mechlab") {
        Add-Failure "Evidence sidecar preset mismatch: $($sidecar.preset)"
    }

    if ($sidecar.flowScreen -ne "Mech Lab") {
        Add-Failure "Evidence sidecar did not capture Mech Lab: $($sidecar.flowScreen)"
    }

    if ([int]$sidecar.screenWidth -ne $Width -or [int]$sidecar.screenHeight -ne $Height) {
        Add-Failure "Evidence sidecar size mismatch: $($sidecar.screenWidth)x$($sidecar.screenHeight)"
    }

    $mechLab = [string]$sidecar.mechLab
    foreach ($fragment in @(
        "MechLabCapture=open",
        "layout=pressure-cards+whole-blocks+single-fillers",
        "alwaysMounted=weapons",
        "noToggle=yes"
    )) {
        if ($mechLab -notlike "*$fragment*") {
            Add-Failure "Evidence sidecar mechLab missing '$fragment': $mechLab"
        }
    }

    $sourceLine = [string]$sidecar.mechLabInventorySource
    if ($sourceLine -ne "Inventory Source: Main Server Preview") {
        Add-Failure "Evidence sidecar inventory source mismatch: $sourceLine"
    }

    $preview = [string]$sidecar.inventoryMechBayPreview
    foreach ($fragment in @(
        "InventoryMechBayPreview=ready",
        "MainServerPreviewApplied: True",
        "ProjectedInventoryValid: True",
        "accountId=local-dev-account",
        "tokenBalance=12000",
        "ownedMechs=3",
        "serverItemStacks=6",
        "projectedItemStacks=5",
        "skippedCurrencyStacks=1",
        "ServerInventoryNotCombatAuthority: True",
        "NoPerFrameServerCalls: True",
        "MobileLandscapeOnly: True",
        "InventorySource=MainServerPreview"
    )) {
        if ($preview -notlike "*$fragment*") {
            Add-Failure "Evidence sidecar preview missing '$fragment': $preview"
        }
    }

    $mobileTouch = [string]$sidecar.mobileTouch
    foreach ($fragment in @(
        "orientation=landscape",
        "landscapeOnly=yes",
        "status=ready"
    )) {
        if ($mobileTouch -notlike "*$fragment*") {
            Add-Failure "Evidence sidecar mobile touch missing '$fragment': $mobileTouch"
        }
    }

    Add-Row -Check "evidence sidecar" -Detail $Path
}

function Test-CaptureLog {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Add-Failure "Evidence runtime log missing: $Path"
        return
    }

    $logText = Get-Content -LiteralPath $Path -Raw
    foreach ($marker in @(
        "MC2 inventory-to-MechBay preview OK",
        "MC2 inventory-to-MechBay preview assertion OK",
        "MainServerPreviewApplied: True",
        "ProjectedInventoryValid: True",
        "InventorySource=MainServerPreview",
        "ServerInventoryNotCombatAuthority: True",
        "NoPerFrameServerCalls: True",
        "MobileLandscapeOnly: True",
        "MC2 capture preset: mechlab",
        "MC2 screenshot capture requested:",
        "MC2 capture sidecar written:"
    )) {
        if ($logText -notlike "*$marker*") {
            Add-Failure "Evidence runtime log missing marker: $marker"
        }
    }

    if ($logText -like "*RewardClaimAfterDebrief: True*") {
        Add-Failure "Evidence runtime log claimed server combat reward in preview-only path."
    }

    Add-Row -Check "evidence runtime log" -Detail $Path
}

New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
$outputFullPath = [System.IO.Path]::GetFullPath($OutputDir)
$repoFullPath = [System.IO.Path]::GetFullPath($RepoRoot)
if (-not $outputFullPath.StartsWith($repoFullPath, [StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDir must stay inside RepoRoot: $OutputDir"
}

$bootstrap = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs"
$previewCommandFileText = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-inventory-mechbay-preview-smoke.txt"
$visibleFlowText = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt"
$gitignore = Read-RequiredText -RelativePath ".gitignore"

foreach ($marker in @(
    "mechLabInventorySource",
    "inventoryMechBayPreview",
    "BuildCaptureInventoryMechBayPreviewSummary",
    "InventorySourceLine()",
    "Inventory Source: Main Server Preview",
    "InventoryMechBayPreview=not-attempted"
)) {
    Require-Text -Text $bootstrap -Needle $marker -Label "capture sidecar wiring"
}

foreach ($marker in @(
    "inventory-mechbay-preview-smoke",
    "assert-inventory-mechbay-preview-smoke"
)) {
    Require-Text -Text $previewCommandFileText -Needle $marker -Label "preview command file"
}

Forbid-Text -Text $visibleFlowText -Needle "inventory-mechbay-preview-smoke" -Label "default visible-flow command file"
Require-Text -Text $gitignore -Needle "analysis-output/inventory-mechbay-preview-evidence/" -Label "generated evidence ignore"
Assert-IgnoredGeneratedPath -RelativePath "analysis-output/inventory-mechbay-preview-evidence/mechlab.png" -Label "evidence screenshot"
Assert-IgnoredGeneratedPath -RelativePath "analysis-output/inventory-mechbay-preview-evidence/mechlab.json" -Label "evidence sidecar"
Assert-IgnoredGeneratedPath -RelativePath "analysis-output/inventory-mechbay-preview-evidence/mechlab.log" -Label "evidence log"

if ($failures.Count -eq 0) {
    $serverStarted = $false
    $serverProcess = $null
    $serverDir = Resolve-RepoPath -RelativePath "server\main-server"
    $serverOut = Join-Path $RepoRoot "analysis-output\main-server-f13-preview-evidence.out.log"
    $serverErr = Join-Path $RepoRoot "analysis-output\main-server-f13-preview-evidence.err.log"
    $commandFile = Resolve-RepoPath -RelativePath "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-inventory-mechbay-preview-smoke.txt"
    $pngPath = Join-Path $OutputDir "mechlab.png"
    $jsonPath = Join-Path $OutputDir "mechlab.json"
    $logPath = Join-Path $OutputDir "mechlab.log"

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
            Invoke-UnityMechLabPreviewCapture `
                -CommandFile $commandFile `
                -PngPath $pngPath `
                -JsonPath $jsonPath `
                -LogPath $logPath
            Test-CaptureImage -Path $pngPath
            Test-CaptureSidecar -Path $jsonPath
            Test-CaptureLog -Path $logPath
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
    Write-Host "Inventory MechBay preview evidence capture failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) inventory MechBay preview evidence capture check(s) failed."
}

Write-Host "Inventory MechBay preview evidence capture OK."
Write-Host "Repo: $RepoRoot"
Write-Host "OutputDir: $OutputDir"
$rows | Format-Table -AutoSize

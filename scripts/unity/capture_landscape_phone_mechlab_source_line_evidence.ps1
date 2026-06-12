param(
    [string]$RepoRoot = "",
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

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot "analysis-output\landscape-phone-mechlab-source-line-evidence"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
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

function Test-PhoneScreenshot {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Add-Failure "Landscape-phone screenshot missing: $Path"
        return
    }

    $png = Get-Item -LiteralPath $Path
    if ($png.Length -lt $MinimumPngBytes) {
        Add-Failure "Landscape-phone screenshot too small: $($png.Length) bytes, expected at least $MinimumPngBytes"
        return
    }

    Add-Type -AssemblyName System.Drawing
    $bitmap = [System.Drawing.Bitmap]::new($Path)
    try {
        if ($bitmap.Width -ne $Width -or $bitmap.Height -ne $Height) {
            Add-Failure "Landscape-phone screenshot size mismatch: $($bitmap.Width)x$($bitmap.Height), expected ${Width}x${Height}"
            return
        }

        if ($bitmap.Width -le $bitmap.Height) {
            Add-Failure "Landscape-phone screenshot must be landscape: $($bitmap.Width)x$($bitmap.Height)"
            return
        }

        $aspect = [Math]::Round($bitmap.Width / [double]$bitmap.Height, 3)
        if ($aspect -lt 2.0 -or $aspect -gt 2.4) {
            Add-Failure "Landscape-phone screenshot aspect is not a phone landscape ratio: $aspect"
            return
        }

        $unique = New-Object 'System.Collections.Generic.HashSet[int]'
        for ($y = 0; $y -lt $bitmap.Height; $y += 12) {
            for ($x = 0; $x -lt $bitmap.Width; $x += 12) {
                [void]$unique.Add($bitmap.GetPixel($x, $y).ToArgb())
            }
        }

        if ($unique.Count -lt 80) {
            Add-Failure "Landscape-phone screenshot looks too flat: uniqueColors=$($unique.Count)"
            return
        }

        Add-Row -Check "landscape-phone screenshot" -Detail "$($bitmap.Width)x$($bitmap.Height) aspect=$aspect bytes=$($png.Length) uniqueColors=$($unique.Count)"
    }
    finally {
        $bitmap.Dispose()
    }
}

function Test-PhoneSidecar {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Add-Failure "Landscape-phone sidecar missing: $Path"
        return
    }

    try {
        $sidecar = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
    }
    catch {
        Add-Failure "Landscape-phone sidecar is not valid JSON: $Path"
        return
    }

    if ($sidecar.preset -ne "mechlab") {
        Add-Failure "Landscape-phone sidecar preset mismatch: $($sidecar.preset)"
    }

    if ($sidecar.flowScreen -ne "Mech Lab") {
        Add-Failure "Landscape-phone sidecar did not capture Mech Lab: $($sidecar.flowScreen)"
    }

    if ([int]$sidecar.screenWidth -ne $Width -or [int]$sidecar.screenHeight -ne $Height) {
        Add-Failure "Landscape-phone sidecar size mismatch: $($sidecar.screenWidth)x$($sidecar.screenHeight)"
    }

    $sourceLine = [string]$sidecar.mechLabInventorySource
    if ($sourceLine -ne "Inventory Source: Main Server Preview") {
        Add-Failure "Landscape-phone MechLab source line mismatch: $sourceLine"
    }

    $mechLab = [string]$sidecar.mechLab
    foreach ($fragment in @(
        "MechLabCapture=open",
        "weaponBlock=",
        "fillers=A+/C+",
        "layout=pressure-cards+whole-blocks+single-fillers",
        "alwaysMounted=weapons",
        "noToggle=yes"
    )) {
        if ($mechLab -notlike "*$fragment*") {
            Add-Failure "Landscape-phone sidecar MechLab missing '$fragment': $mechLab"
        }
    }

    $mobileTouch = [string]$sidecar.mobileTouch
    foreach ($fragment in @(
        "MobileTouchUi=ready",
        "orientation=landscape",
        "landscapeOnly=yes",
        "mechLabGridCell>=36",
        "touchRatios=16:9+19.5:9+20:9",
        "current=${Width}x${Height}",
        "status=ready"
    )) {
        if ($mobileTouch -notlike "*$fragment*") {
            Add-Failure "Landscape-phone sidecar mobile touch missing '$fragment': $mobileTouch"
        }
    }

    if ($mobileTouch -match "(?i)portrait") {
        Add-Failure "Landscape-phone mobile touch summary must not introduce portrait support: $mobileTouch"
    }

    $preview = [string]$sidecar.inventoryMechBayPreview
    foreach ($fragment in @(
        "InventoryMechBayPreview=ready",
        "MainServerPreviewApplied: True",
        "ProjectedInventoryValid: True",
        "ServerInventoryNotCombatAuthority: True",
        "NoPerFrameServerCalls: True",
        "MobileLandscapeOnly: True",
        "InventorySource=MainServerPreview"
    )) {
        if ($preview -notlike "*$fragment*") {
            Add-Failure "Landscape-phone sidecar preview missing '$fragment': $preview"
        }
    }

    Add-Row -Check "landscape-phone sidecar" -Detail "$Path sourceLine=$sourceLine"
}

function Test-PhoneLog {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Add-Failure "Landscape-phone runtime log missing: $Path"
        return
    }

    $logText = Get-Content -LiteralPath $Path -Raw
    foreach ($marker in @(
        "MC2 inventory-to-MechBay preview OK",
        "InventorySource=MainServerPreview",
        "MobileLandscapeOnly: True",
        "MC2 capture preset: mechlab",
        "MC2 screenshot capture requested:",
        "MC2 capture sidecar written:"
    )) {
        if ($logText -notlike "*$marker*") {
            Add-Failure "Landscape-phone runtime log missing marker: $marker"
        }
    }

    Add-Row -Check "landscape-phone runtime log" -Detail $Path
}

if ($Width -le $Height) {
    Add-Failure "Width/Height must be landscape for phone evidence: ${Width}x${Height}"
}

$aspect = [Math]::Round($Width / [double]$Height, 3)
if ($aspect -lt 2.0 -or $aspect -gt 2.4) {
    Add-Failure "Width/Height must be a landscape phone aspect ratio, got $aspect from ${Width}x${Height}"
}

New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
$outputFullPath = [System.IO.Path]::GetFullPath($OutputDir)
$repoFullPath = [System.IO.Path]::GetFullPath($RepoRoot)
if (-not $outputFullPath.StartsWith($repoFullPath, [StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDir must stay inside RepoRoot: $OutputDir"
}

$bootstrap = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs"
$mobilePlan = Read-RequiredText -RelativePath "docs-mobile-first-plan-2026-06-10.md"
$gitignore = Read-RequiredText -RelativePath ".gitignore"

foreach ($marker in @(
    "mechLabInventorySource",
    "InventorySourceLine()",
    "Inventory Source: Main Server Preview",
    "MobileTouchLayoutSummary",
    "orientation=landscape",
    "landscapeOnly=yes",
    "mechLabGridCell>=36"
)) {
    Require-Text -Text $bootstrap -Needle $marker -Label "landscape-phone sidecar wiring"
}

Require-Text -Text $mobilePlan -Needle "first phone version is landscape-only" -Label "mobile landscape plan"
Require-Text -Text $mobilePlan -Needle "F14 capture landscape-phone MechLab source-line evidence" -Label "mobile F14 plan"
Require-Text -Text $gitignore -Needle "analysis-output/landscape-phone-mechlab-source-line-evidence/" -Label "generated landscape-phone evidence ignore"

$pngPath = Join-Path $OutputDir "mechlab.png"
$jsonPath = Join-Path $OutputDir "mechlab.json"
$logPath = Join-Path $OutputDir "mechlab.log"
Assert-IgnoredGeneratedPath -RelativePath (Convert-ToRepoRelativePath -Path $pngPath) -Label "landscape-phone screenshot"
Assert-IgnoredGeneratedPath -RelativePath (Convert-ToRepoRelativePath -Path $jsonPath) -Label "landscape-phone sidecar"
Assert-IgnoredGeneratedPath -RelativePath (Convert-ToRepoRelativePath -Path $logPath) -Label "landscape-phone log"

if ($failures.Count -eq 0) {
    $previewScript = Resolve-RepoPath -RelativePath "scripts\unity\capture_inventory_mechbay_preview_evidence.ps1"
    if (-not (Test-Path -LiteralPath $previewScript -PathType Leaf)) {
        Add-Failure "Missing dependency script: $previewScript"
    }
    else {
        & $previewScript `
            -RepoRoot $RepoRoot `
            -OutputDir $OutputDir `
            -Width $Width `
            -Height $Height `
            -RuntimeTimeoutSeconds $RuntimeTimeoutSeconds `
            -MinimumPngBytes $MinimumPngBytes

        Test-PhoneScreenshot -Path $pngPath
        Test-PhoneSidecar -Path $jsonPath
        Test-PhoneLog -Path $logPath
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Landscape-phone MechLab source-line evidence capture failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) landscape-phone MechLab source-line evidence check(s) failed."
}

Write-Host "Landscape-phone MechLab source-line evidence capture OK."
Write-Host "Repo: $RepoRoot"
Write-Host "OutputDir: $OutputDir"
$rows | Format-Table -AutoSize

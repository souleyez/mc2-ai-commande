param(
    [string]$RepoRoot = "",
    [string]$OutputDir = "",
    [int]$PhoneWidth = 2400,
    [int]$PhoneHeight = 1080,
    [int]$PcWidth = 1280,
    [int]$PcHeight = 720,
    [int]$RuntimeTimeoutSeconds = 120,
    [int]$MinimumPhonePngBytes = 160000,
    [int]$MinimumPcPngBytes = 100000
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
    $OutputDir = Join-Path $RepoRoot "analysis-output\landscape-mechlab-touch-evidence"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
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

function Test-CaptureImage {
    param(
        [string]$Path,
        [int]$ExpectedWidth,
        [int]$ExpectedHeight,
        [int]$MinimumBytes,
        [string]$Label
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Add-Failure "$Label screenshot missing: $Path"
        return
    }

    $png = Get-Item -LiteralPath $Path
    if ($png.Length -lt $MinimumBytes) {
        Add-Failure "$Label screenshot too small: $($png.Length) bytes, expected at least $MinimumBytes"
        return
    }

    Add-Type -AssemblyName System.Drawing
    $bitmap = [System.Drawing.Bitmap]::new($Path)
    try {
        if ($bitmap.Width -ne $ExpectedWidth -or $bitmap.Height -ne $ExpectedHeight) {
            Add-Failure "$Label screenshot size mismatch: $($bitmap.Width)x$($bitmap.Height), expected ${ExpectedWidth}x${ExpectedHeight}"
            return
        }

        if ($bitmap.Width -le $bitmap.Height) {
            Add-Failure "$Label screenshot must be landscape: $($bitmap.Width)x$($bitmap.Height)"
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

        Add-Row -Check "$Label screenshot" -Detail "$($bitmap.Width)x$($bitmap.Height) bytes=$($png.Length) uniqueColors=$($unique.Count)"
    }
    finally {
        $bitmap.Dispose()
    }
}

function Read-CaptureSidecar {
    param(
        [string]$Path,
        [string]$Label
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Add-Failure "$Label sidecar missing: $Path"
        return $null
    }

    try {
        return Get-Content -LiteralPath $Path -Raw -Encoding UTF8 | ConvertFrom-Json
    }
    catch {
        Add-Failure "$Label sidecar is not valid JSON: $Path"
        return $null
    }
}

function Require-SummaryFragments {
    param(
        [string]$Summary,
        [string[]]$Fragments,
        [string]$Label
    )

    foreach ($fragment in $Fragments) {
        if ([string]::IsNullOrWhiteSpace($Summary) -or $Summary -notlike "*$fragment*") {
            Add-Failure "$Label missing '$fragment': $Summary"
        }
    }
}

function Test-TouchEvidenceSidecar {
    param(
        [object]$Sidecar,
        [string]$Label,
        [int]$ExpectedWidth,
        [int]$ExpectedHeight
    )

    if ($null -eq $Sidecar) {
        return
    }

    if ($Sidecar.preset -ne "mechlab") {
        Add-Failure "$Label sidecar preset mismatch: $($Sidecar.preset)"
    }

    if ($Sidecar.flowScreen -ne "Mech Lab") {
        Add-Failure "$Label sidecar did not capture Mech Lab: $($Sidecar.flowScreen)"
    }

    if ([int]$Sidecar.screenWidth -ne $ExpectedWidth -or [int]$Sidecar.screenHeight -ne $ExpectedHeight) {
        Add-Failure "$Label sidecar size mismatch: $($Sidecar.screenWidth)x$($Sidecar.screenHeight), expected ${ExpectedWidth}x${ExpectedHeight}"
    }

    Require-SummaryFragments -Summary ([string]$Sidecar.mechLab) -Label "$Label mechLab" -Fragments @(
        "MechLabCapture=open",
        "flow=Mech Lab",
        "weaponBlock=",
        "fillers=A+/C+",
        "fit=Fit OK",
        "CellState=OK",
        "OPEN",
        "OCC",
        "layout=pressure-cards+whole-blocks+single-fillers",
        "alwaysMounted=weapons",
        "noToggle=yes"
    )

    Require-SummaryFragments -Summary ([string]$Sidecar.mobileTouch) -Label "$Label mobileTouch" -Fragments @(
        "MobileTouchUi=ready",
        "orientation=landscape",
        "mechLabBack=44",
        "mechLabActions=44",
        "mechLabShop=44",
        "mechLabHire=44",
        "mechLabRoster=44",
        "mechLabSquad=44",
        "mechLabWeaponButtons=44",
        "mechLabRepair=44",
        "mechLabGridCell>=36",
        "landscapeOnly=yes",
        "touchRatios=16:9+19.5:9+20:9",
        "status=ready",
        "mechLabControl=44",
        "mechLabWeapon=44"
    )

    if ([string]$Sidecar.mobileTouch -match "(?i)portrait") {
        Add-Failure "$Label mobile touch summary must not introduce portrait support: $($Sidecar.mobileTouch)"
    }

    Add-Row -Check "$Label sidecar" -Detail "Mech Lab touch evidence ${ExpectedWidth}x${ExpectedHeight}"
}

function Test-CaptureLog {
    param(
        [string]$Path,
        [string]$Label
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Add-Failure "$Label runtime log missing: $Path"
        return
    }

    $logText = Get-Content -LiteralPath $Path -Raw -Encoding UTF8
    foreach ($marker in @(
        "MC2 capture preset: mechlab",
        "MC2 screenshot capture requested:",
        "MC2 capture sidecar written:"
    )) {
        if ($logText -notlike "*$marker*") {
            Add-Failure "$Label runtime log missing marker: $marker"
        }
    }

    Add-Row -Check "$Label runtime log" -Detail $Path
}

if ($PhoneWidth -le $PhoneHeight) {
    Add-Failure "Phone evidence must be landscape: ${PhoneWidth}x${PhoneHeight}"
}

$phoneAspect = [Math]::Round($PhoneWidth / [double]$PhoneHeight, 3)
if ($phoneAspect -lt 2.0 -or $phoneAspect -gt 2.4) {
    Add-Failure "Phone evidence must use a landscape phone aspect ratio, got $phoneAspect from ${PhoneWidth}x${PhoneHeight}"
}

if ($PcWidth -le $PcHeight) {
    Add-Failure "PC reference evidence must be landscape: ${PcWidth}x${PcHeight}"
}

New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
$outputFullPath = [System.IO.Path]::GetFullPath($OutputDir)
$repoFullPath = [System.IO.Path]::GetFullPath($RepoRoot)
if (-not $outputFullPath.StartsWith($repoFullPath, [StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDir must stay inside RepoRoot: $OutputDir"
}

$bootstrap = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs"
$masterPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md"
$detailedPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"
$mobilePlan = Read-RequiredText -RelativePath "docs-mobile-first-plan-2026-06-10.md"
$readme = Read-RequiredText -RelativePath "README.md"
$gitignore = Read-RequiredText -RelativePath ".gitignore"

foreach ($marker in @(
    "mechLabActions=44",
    "mechLabShop=44",
    "mechLabHire=44",
    "mechLabRoster=44",
    "mechLabSquad=44",
    "mechLabWeaponButtons=44",
    "mechLabRepair=44",
    "mechLabControl=",
    "mechLabWeapon=",
    "BuildCaptureMechLabSummary()",
    "BuildCaptureMobileTouchSummary()"
)) {
    Require-Text -Text $bootstrap -Needle $marker -Label "F23 runtime sidecar wiring"
}

foreach ($marker in @(
    "F23 capture landscape MechLab touch evidence",
    "F24 capture Android MechLab touch evidence"
)) {
    Require-Text -Text $masterPlan -Needle $marker -Label "master plan"
    Require-Text -Text $detailedPlan -Needle $marker -Label "detailed plan"
    Require-Text -Text $mobilePlan -Needle $marker -Label "mobile plan"
    Require-Text -Text $readme -Needle $marker -Label "README"
}

Require-Text -Text $gitignore -Needle "analysis-output/landscape-mechlab-touch-evidence/" -Label "generated F23 evidence ignore"

$phoneOutputDir = Join-Path $OutputDir ("phone-{0}x{1}" -f $PhoneWidth, $PhoneHeight)
$pcOutputDir = Join-Path $OutputDir "pc-reference"
$phonePngPath = Join-Path $phoneOutputDir "mechlab.png"
$phoneJsonPath = Join-Path $phoneOutputDir "mechlab.json"
$phoneLogPath = Join-Path $phoneOutputDir "mechlab.log"
$pcPngPath = Join-Path $pcOutputDir "mechlab.png"
$pcJsonPath = Join-Path $pcOutputDir "mechlab.json"
$pcLogPath = Join-Path $pcOutputDir "mechlab.log"

foreach ($entry in @(
    @($phonePngPath, "phone screenshot"),
    @($phoneJsonPath, "phone sidecar"),
    @($phoneLogPath, "phone log"),
    @($pcPngPath, "PC screenshot"),
    @($pcJsonPath, "PC sidecar"),
    @($pcLogPath, "PC log")
)) {
    Assert-IgnoredGeneratedPath -RelativePath (Convert-ToRepoRelativePath -Path $entry[0]) -Label $entry[1]
}

if ($failures.Count -eq 0) {
    $phoneScript = Resolve-RepoPath -RelativePath "scripts\unity\capture_landscape_phone_mechlab_source_line_evidence.ps1"
    $referenceScript = Resolve-RepoPath -RelativePath "scripts\unity\capture_reference_visuals.ps1"
    if (-not (Test-Path -LiteralPath $phoneScript -PathType Leaf)) {
        Add-Failure "Missing dependency script: $phoneScript"
    }
    if (-not (Test-Path -LiteralPath $referenceScript -PathType Leaf)) {
        Add-Failure "Missing dependency script: $referenceScript"
    }

    if ($failures.Count -eq 0) {
        & $phoneScript `
            -RepoRoot $RepoRoot `
            -OutputDir $phoneOutputDir `
            -Width $PhoneWidth `
            -Height $PhoneHeight `
            -RuntimeTimeoutSeconds $RuntimeTimeoutSeconds `
            -MinimumPngBytes $MinimumPhonePngBytes

        & $referenceScript `
            -RepoRoot $RepoRoot `
            -OutputDir $pcOutputDir `
            -Presets mechlab `
            -Width $PcWidth `
            -Height $PcHeight `
            -CaptureTimeoutSeconds $RuntimeTimeoutSeconds

        Test-CaptureImage -Path $phonePngPath -ExpectedWidth $PhoneWidth -ExpectedHeight $PhoneHeight -MinimumBytes $MinimumPhonePngBytes -Label "phone landscape MechLab touch"
        Test-TouchEvidenceSidecar -Sidecar (Read-CaptureSidecar -Path $phoneJsonPath -Label "phone landscape MechLab touch") -Label "phone landscape MechLab touch" -ExpectedWidth $PhoneWidth -ExpectedHeight $PhoneHeight
        Test-CaptureLog -Path $phoneLogPath -Label "phone landscape MechLab touch"

        Test-CaptureImage -Path $pcPngPath -ExpectedWidth $PcWidth -ExpectedHeight $PcHeight -MinimumBytes $MinimumPcPngBytes -Label "PC MechLab touch reference"
        Test-TouchEvidenceSidecar -Sidecar (Read-CaptureSidecar -Path $pcJsonPath -Label "PC MechLab touch reference") -Label "PC MechLab touch reference" -ExpectedWidth $PcWidth -ExpectedHeight $PcHeight
        Test-CaptureLog -Path $pcLogPath -Label "PC MechLab touch reference"
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Landscape MechLab touch evidence capture failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) landscape MechLab touch evidence check(s) failed."
}

Write-Host "Landscape MechLab touch evidence capture OK."
Write-Host "Repo: $RepoRoot"
Write-Host "OutputDir: $OutputDir"
$rows | Format-Table -AutoSize

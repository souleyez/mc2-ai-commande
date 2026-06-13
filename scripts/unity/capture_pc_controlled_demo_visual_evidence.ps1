param(
    [string]$RepoRoot = "",
    [string]$OutputDir = "",
    [string[]]$Presets = @("spawn", "hangar-contact", "damage-demo", "solo-order"),
    [int]$Width = 1280,
    [int]$Height = 720,
    [int]$CaptureTimeoutSeconds = 75,
    [switch]$SkipRun,
    [switch]$PlanOnly
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
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-visual-evidence"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
}

$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
$repoFullPath = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd("\", "/")
if (-not $OutputDir.StartsWith($repoFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDir must stay inside RepoRoot: $OutputDir"
}

$captureDir = Join-Path $OutputDir "captures"
$fixesReportJsonPath = Join-Path $OutputDir "pc-controlled-demo-visual-readability-fixes.json"
$fixesReportMarkdownPath = Join-Path $OutputDir "pc-controlled-demo-visual-readability-fixes.md"
$reportJsonPath = Join-Path $OutputDir "pc-controlled-demo-visual-evidence.json"
$reportMarkdownPath = Join-Path $OutputDir "pc-controlled-demo-visual-evidence.md"
$fixesScript = Join-Path $RepoRoot "scripts\unity\check_pc_controlled_demo_visual_readability_fixes.ps1"
$requiredPresets = @("spawn", "hangar-contact", "damage-demo", "solo-order")

function Normalize-CapturePreset {
    param([string]$Preset)
    if ([string]::IsNullOrWhiteSpace($Preset)) {
        return ""
    }

    return $Preset.Trim().ToLowerInvariant().Replace("_", "-")
}

function Expand-CapturePresets {
    param([string[]]$RawPresets)

    $expanded = New-Object System.Collections.Generic.List[string]
    foreach ($rawPreset in $RawPresets) {
        foreach ($part in ($rawPreset -split ",")) {
            $normalized = Normalize-CapturePreset -Preset $part
            if (-not [string]::IsNullOrWhiteSpace($normalized) -and -not $expanded.Contains($normalized)) {
                [void]$expanded.Add($normalized)
            }
        }
    }

    return $expanded.ToArray()
}

function Require-File {
    param(
        [string]$Path,
        [string]$Label
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Missing $Label`: $Path"
    }
}

function Convert-ToRepoRelativePath {
    param([string]$Path)

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    if ($fullPath.StartsWith($repoFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
        return ($fullPath.Substring($repoFullPath.Length).TrimStart("\", "/") -replace "\\", "/")
    }

    return $fullPath
}

function Require-Text {
    param(
        [string]$Text,
        [string]$Needle,
        [string]$Label
    )

    if ([string]::IsNullOrWhiteSpace($Text) -or -not $Text.Contains($Needle)) {
        throw "$Label missing '$Needle': $Text"
    }
}

function Read-SummaryNumber {
    param(
        [string]$Summary,
        [string]$Pattern,
        [string]$Label
    )

    $match = [regex]::Match($Summary, $Pattern)
    if (-not $match.Success) {
        throw "$Label missing number pattern '$Pattern': $Summary"
    }

    return [double]::Parse($match.Groups[1].Value, [Globalization.CultureInfo]::InvariantCulture)
}

function Test-SidecarEvidence {
    param(
        [string]$Preset,
        [object]$Sidecar
    )

    if ([string]$Sidecar.flowScreen -ne "Battle") {
        throw "$Preset expected refreshed Battle evidence, got $($Sidecar.flowScreen)"
    }

    if ([int]$Sidecar.screenWidth -ne $Width -or [int]$Sidecar.screenHeight -ne $Height) {
        throw "$Preset expected ${Width}x${Height}, got $($Sidecar.screenWidth)x$($Sidecar.screenHeight)"
    }

    $terrain = [string]$Sidecar.terrainReadability
    Require-Text -Text $terrain -Needle "textureStrength=0.36" -Label "$Preset terrainReadability"
    Require-Text -Text $terrain -Needle "shoreContrast=raised" -Label "$Preset terrainReadability"
    Require-Text -Text $terrain -Needle "pathing=unchanged" -Label "$Preset terrainReadability"

    $unit = [string]$Sidecar.unitReadability
    Require-Text -Text $unit -Needle "alpha=high-contact" -Label "$Preset unitReadability"
    Require-Text -Text $unit -Needle "damagePalette=amber+pilot-cyan" -Label "$Preset unitReadability"
    Require-Text -Text $unit -Needle "collision=unchanged" -Label "$Preset unitReadability"

    $structure = [string]$Sidecar.structureReadability
    Require-Text -Text $structure -Needle "ObjectiveNearOcclusion=fade+height-clamp+tone-down" -Label "$Preset structureReadability"
    Require-Text -Text $structure -Needle "visualOnly=yes" -Label "$Preset structureReadability"
    Require-Text -Text $structure -Needle "collision=unchanged" -Label "$Preset structureReadability"
    $objectiveNearProps = Read-SummaryNumber -Summary $structure -Pattern "ObjectiveNearOcclusion=fade\+height-clamp\+tone-down props=([0-9]+)" -Label "$Preset objective-near props"
    $treeClamps = Read-SummaryNumber -Summary $structure -Pattern "treeClamp=([0-9]+)" -Label "$Preset objective-near treeClamp"
    if ($objectiveNearProps -lt 1 -or $treeClamps -lt 1) {
        throw "$Preset refreshed evidence missing objective-near readability counts: props=$objectiveNearProps treeClamp=$treeClamps"
    }

    $hud = [string]$Sidecar.battleHud
    Require-Text -Text $hud -Needle "statusRailDensity=compact-pc" -Label "$Preset battleHud"
    Require-Text -Text $hud -Needle "combatLogVisible=no" -Label "$Preset battleHud"
    $statusRailWidth = Read-SummaryNumber -Summary $hud -Pattern "statusRailW=([0-9.]+)" -Label "$Preset battleHud"
    $statusRailShare = Read-SummaryNumber -Summary $hud -Pattern "statusRailShare1280=([0-9.]+)" -Label "$Preset battleHud"
    if ($statusRailWidth -gt 300 -or $statusRailShare -gt 0.24) {
        throw "$Preset refreshed evidence has wide status rail: width=$statusRailWidth share=$statusRailShare"
    }

    if ($Preset -eq "damage-demo") {
        $damage = [string]$Sidecar.damageReadability
        Require-Text -Text $damage -Needle "cuePalette=command-blue target-red damage-amber hostile-magenta pilot-cyan" -Label "$Preset damageReadability"
        Require-Text -Text $damage -Needle "Cockpit=breach+ejection-pod+chute+landing+arc+distress+escape-column+route" -Label "$Preset damageReadability"
    }

    if ($Preset -eq "solo-order") {
        $command = [string]$Sidecar.commandReadability
        Require-Text -Text $command -Needle "CommandReadability=all+single+jet+focus+commander-follow+formation" -Label "$Preset commandReadability"
        Require-Text -Text $command -Needle "solo=1" -Label "$Preset commandReadability"
        Require-Text -Text $command -Needle "SoloOrder=ring+beacon" -Label "$Preset commandReadability"
        Require-Text -Text $command -Needle "SoloReturn=ring+beacon" -Label "$Preset commandReadability"
        Require-Text -Text $command -Needle "CommandCuePalette=command-blue+target-red+damage-amber+hostile-magenta" -Label "$Preset commandReadability"
        $commander = [string]$Sidecar.commanderFollow
        Require-Text -Text $commander -Needle "CommanderFollow=unit-1+first-sort+fixed-view" -Label "$Preset commanderFollow"
        Require-Text -Text $commander -Needle "unit=unit-1" -Label "$Preset commanderFollow"
        Require-Text -Text $commander -Needle "sortedIndex=1" -Label "$Preset commanderFollow"
    }

    return [pscustomobject]@{
        preset = $Preset
        screenshot = Convert-ToRepoRelativePath -Path (Join-Path $captureDir "$Preset.png")
        sidecar = Convert-ToRepoRelativePath -Path (Join-Path $captureDir "$Preset.json")
        log = Convert-ToRepoRelativePath -Path (Join-Path $captureDir "$Preset.log")
        textureStrength = Read-SummaryNumber -Summary $terrain -Pattern "textureStrength=([0-9.]+)" -Label "$Preset terrainReadability"
        objectiveNearProps = $objectiveNearProps
        treeClamps = $treeClamps
        statusRailWidth = $statusRailWidth
        statusRailShare1280 = $statusRailShare
        activeHostiles = [int]$Sidecar.activeHostileCount
        visibleHostiles = [int]$Sidecar.visibleHostileCount
    }
}

$expandedPresets = @(Expand-CapturePresets -RawPresets $Presets)
foreach ($requiredPreset in $requiredPresets) {
    if ($expandedPresets -notcontains $requiredPreset) {
        throw "F31 visual evidence refresh requires preset '$requiredPreset'. Presets: $($expandedPresets -join ',')"
    }
}

Require-File -Path $fixesScript -Label "F30 visual readability fixes script"

if ($PlanOnly) {
    Write-Host "PC controlled-demo visual evidence refresh plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "CaptureDir: $captureDir"
    Write-Host "ReportJson: $reportJsonPath"
    Write-Host "ReportMarkdown: $reportMarkdownPath"
    Write-Host "Presets: $($expandedPresets -join ',')"
    Write-Host "WidthHeight: ${Width}x${Height}"
    return
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

if ($SkipRun) {
    & $fixesScript `
        -RepoRoot $RepoRoot `
        -OutputDir $OutputDir `
        -Presets $expandedPresets `
        -Width $Width `
        -Height $Height `
        -CaptureTimeoutSeconds $CaptureTimeoutSeconds `
        -SkipRun
}
else {
    & $fixesScript `
        -RepoRoot $RepoRoot `
        -OutputDir $OutputDir `
        -Presets $expandedPresets `
        -Width $Width `
        -Height $Height `
        -CaptureTimeoutSeconds $CaptureTimeoutSeconds
}

Require-File -Path $fixesReportJsonPath -Label "F30 readability fixes JSON report"
Require-File -Path $fixesReportMarkdownPath -Label "F30 readability fixes Markdown report"
$fixesReport = Get-Content -LiteralPath $fixesReportJsonPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ([string]$fixesReport.result -ne "pass") {
    throw "F30 fixes report did not pass: $fixesReportJsonPath"
}

if ([string]$fixesReport.completedTask -ne "F30 implement PC controlled-demo visual readability fixes") {
    throw "F30 fixes report completedTask mismatch: $($fixesReport.completedTask)"
}

if ([string]$fixesReport.nextFormalTask -ne "F31 refresh PC controlled-demo visual evidence after readability fixes") {
    throw "F30 fixes report nextFormalTask mismatch: $($fixesReport.nextFormalTask)"
}

$evidence = New-Object System.Collections.Generic.List[object]
foreach ($preset in $expandedPresets) {
    $pngPath = Join-Path $captureDir "$preset.png"
    $jsonPath = Join-Path $captureDir "$preset.json"
    $logPath = Join-Path $captureDir "$preset.log"
    Require-File -Path $pngPath -Label "$preset refreshed screenshot"
    Require-File -Path $jsonPath -Label "$preset refreshed sidecar"
    Require-File -Path $logPath -Label "$preset refreshed log"
    $sidecar = Get-Content -LiteralPath $jsonPath -Raw -Encoding UTF8 | ConvertFrom-Json
    [void]$evidence.Add((Test-SidecarEvidence -Preset $preset -Sidecar $sidecar))
}

$report = [pscustomobject]@{
    schema = "PCControlledDemoVisualEvidenceRefresh"
    result = "pass"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    width = $Width
    height = $Height
    completedTask = "F31 refresh PC controlled-demo visual evidence after readability fixes"
    nextFormalTask = "F32 audit PC controlled-demo command readability and formation feel"
    sourceFixesReport = Convert-ToRepoRelativePath -Path $fixesReportJsonPath
    sourceFixesMarkdown = Convert-ToRepoRelativePath -Path $fixesReportMarkdownPath
    captureDir = Convert-ToRepoRelativePath -Path $captureDir
    evidence = $evidence
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
[void]$markdownLines.Add("# PC Controlled Demo Visual Evidence Refresh")
[void]$markdownLines.Add("")
[void]$markdownLines.Add("Result: pass")
[void]$markdownLines.Add("")
[void]$markdownLines.Add('Completed task: `F31 refresh PC controlled-demo visual evidence after readability fixes`')
[void]$markdownLines.Add('Next formal task: `F32 audit PC controlled-demo command readability and formation feel`')
[void]$markdownLines.Add("")
[void]$markdownLines.Add("Source F30 report: `"$(Convert-ToRepoRelativePath -Path $fixesReportJsonPath)`"")
[void]$markdownLines.Add("Capture directory: `"$(Convert-ToRepoRelativePath -Path $captureDir)`"")
[void]$markdownLines.Add("")
[void]$markdownLines.Add("| Preset | Texture | Objective props | Tree clamps | Status rail | Hostiles visible |")
[void]$markdownLines.Add("| --- | ---: | ---: | ---: | ---: | ---: |")
foreach ($item in $evidence) {
    [void]$markdownLines.Add(("| {0} | {1} | {2} | {3} | {4} / {5} | {6}/{7} |" -f `
        $item.preset,
        $item.textureStrength,
        $item.objectiveNearProps,
        $item.treeClamps,
        $item.statusRailWidth,
        $item.statusRailShare1280,
        $item.visibleHostiles,
        $item.activeHostiles))
}

$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "PC controlled-demo visual evidence refresh OK."
Write-Host "Report: $reportJsonPath"

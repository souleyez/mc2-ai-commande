param(
    [string]$RepoRoot = "",
    [string]$EvidenceDir = "",
    [string]$CommandEvidenceDir = "",
    [string]$OutputDir = "",
    [int]$Width = 1280,
    [int]$Height = 720,
    [switch]$RefreshEvidence,
    [switch]$SkipEvidenceRun,
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

if ([string]::IsNullOrWhiteSpace($EvidenceDir)) {
    $EvidenceDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-visual-evidence"
}
elseif (-not [System.IO.Path]::IsPathRooted($EvidenceDir)) {
    $EvidenceDir = Join-Path $RepoRoot $EvidenceDir
}

if ([string]::IsNullOrWhiteSpace($CommandEvidenceDir)) {
    $CommandEvidenceDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-command-evidence"
}
elseif (-not [System.IO.Path]::IsPathRooted($CommandEvidenceDir)) {
    $CommandEvidenceDir = Join-Path $RepoRoot $CommandEvidenceDir
}

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-investor-evidence-package-audit"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
}

$repoFullPath = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([char[]]@("\", "/"))
$EvidenceDir = [System.IO.Path]::GetFullPath($EvidenceDir)
$CommandEvidenceDir = [System.IO.Path]::GetFullPath($CommandEvidenceDir)
$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
foreach ($candidate in @($EvidenceDir, $CommandEvidenceDir, $OutputDir)) {
    if (-not $candidate.StartsWith($repoFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Path must stay inside RepoRoot: $candidate"
    }
}

$captureDir = Join-Path $EvidenceDir "captures"
$visualEvidenceReportPath = Join-Path $EvidenceDir "pc-controlled-demo-visual-evidence.json"
$commandEvidenceReportPath = Join-Path $CommandEvidenceDir "pc-controlled-demo-command-evidence.json"
$commandEvidenceMarkdownPath = Join-Path $CommandEvidenceDir "pc-controlled-demo-command-evidence.md"
$reportJsonPath = Join-Path $OutputDir "pc-controlled-demo-investor-evidence-package-audit.json"
$reportMarkdownPath = Join-Path $OutputDir "pc-controlled-demo-investor-evidence-package-audit.md"
$commandEvidenceScript = Join-Path $RepoRoot "scripts\unity\capture_pc_controlled_demo_command_evidence.ps1"
$requiredPresets = @("spawn", "hangar-contact", "damage-demo", "solo-order", "solo-return")
$checks = New-Object System.Collections.Generic.List[object]
$followUps = New-Object System.Collections.Generic.List[object]

function Convert-ToRepoRelativePath {
    param([string]$Path)

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    if ($fullPath.StartsWith($repoFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
        return ($fullPath.Substring($repoFullPath.Length).TrimStart([char[]]@("\", "/")) -replace "\\", "/")
    }

    return $fullPath
}

function Resolve-RepoPath {
    param([string]$RelativePath)
    return Join-Path $RepoRoot ($RelativePath -replace "/", "\")
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

function Read-RequiredText {
    param([string]$RelativePath)

    $path = Resolve-RepoPath -RelativePath $RelativePath
    Require-File -Path $path -Label $RelativePath
    return Get-Content -LiteralPath $path -Raw -Encoding UTF8
}

function Require-Text {
    param(
        [string]$Text,
        [string]$Needle,
        [string]$Label
    )

    if ([string]::IsNullOrWhiteSpace($Text) -or -not $Text.Contains($Needle)) {
        throw "$Label missing marker '$Needle'"
    }
}

function Read-PropertyValue {
    param(
        [object]$Object,
        [string]$Name,
        [string]$Label
    )

    if ($null -eq $Object) {
        throw "$Label missing object for property: $Name"
    }

    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property -or $null -eq $property.Value) {
        throw "$Label missing property: $Name"
    }

    return $property.Value
}

function Read-StringProperty {
    param(
        [object]$Object,
        [string]$Name,
        [string]$Label
    )

    return [string](Read-PropertyValue -Object $Object -Name $Name -Label $Label)
}

function Read-IntProperty {
    param(
        [object]$Object,
        [string]$Name,
        [string]$Label
    )

    return [int](Read-PropertyValue -Object $Object -Name $Name -Label $Label)
}

function Add-Check {
    param(
        [string]$Area,
        [string]$Preset,
        [string]$Status,
        [string]$Detail
    )

    [void]$checks.Add([pscustomobject]@{
        area = $Area
        preset = $Preset
        status = $Status
        detail = $Detail
    })
}

function Add-FollowUp {
    param(
        [string]$Priority,
        [string]$Area,
        [string]$Preset,
        [string]$Issue,
        [string]$NextFix
    )

    [void]$followUps.Add([pscustomobject]@{
        priority = $Priority
        area = $Area
        preset = $Preset
        issue = $Issue
        nextFix = $NextFix
    })
}

function Test-RequiredPresetSet {
    param(
        [object[]]$Evidence,
        [string]$Label
    )

    $actualPresets = @($Evidence | ForEach-Object { [string]$_.preset })
    foreach ($preset in $requiredPresets) {
        if ($actualPresets -notcontains $preset) {
            throw "$Label missing required preset: $preset"
        }
    }

    if ($actualPresets.Count -ne $requiredPresets.Count) {
        throw "$Label expected $($requiredPresets.Count) preset rows, got $($actualPresets.Count): $($actualPresets -join ',')"
    }
}

function Read-EvidenceRow {
    param(
        [object[]]$Evidence,
        [string]$Preset,
        [string]$Label
    )

    $rows = @($Evidence | Where-Object { [string]$_.preset -eq $Preset })
    if ($rows.Count -ne 1) {
        throw "$Label expected one row for $Preset, got $($rows.Count)"
    }

    return $rows[0]
}

function Read-SidecarForPreset {
    param([string]$Preset)

    $path = Join-Path $captureDir "$Preset.json"
    Require-File -Path $path -Label "$Preset sidecar"
    $sidecar = Get-Content -LiteralPath $path -Raw -Encoding UTF8 | ConvertFrom-Json
    if ([string](Read-PropertyValue -Object $sidecar -Name "preset" -Label "$Preset sidecar") -ne $Preset) {
        throw "$Preset sidecar preset mismatch"
    }

    return $sidecar
}

function Test-CommandReportEnvelope {
    param(
        [object]$CommandReport,
        [string]$CommandMarkdown
    )

    if ([string]$CommandReport.result -ne "pass") {
        throw "Command evidence report result must be pass, got $($CommandReport.result)"
    }

    if ([string]$CommandReport.completedTask -ne "F40 refresh PC controlled-demo investor-readiness evidence after fixes") {
        throw "Command evidence completedTask mismatch: $($CommandReport.completedTask)"
    }

    if ([string]$CommandReport.nextFormalTask -ne "F41 audit post-F40 PC controlled-demo investor evidence package") {
        throw "Command evidence nextFormalTask mismatch: $($CommandReport.nextFormalTask)"
    }

    if ([int]$CommandReport.width -ne $Width -or [int]$CommandReport.height -ne $Height) {
        throw "Command evidence dimensions expected ${Width}x${Height}, got $($CommandReport.width)x$($CommandReport.height)"
    }

    Test-RequiredPresetSet -Evidence @($CommandReport.evidence) -Label "command evidence"

    foreach ($marker in @(
        "Investor proxy",
        "DamageDebrief=section-status+repair-line",
        "InvestorProxyVisuals=active",
        "publicSafe=proxy-only",
        "mobileLandscapeOnly=True"
    )) {
        Require-Text -Text $CommandMarkdown -Needle $marker -Label "command evidence markdown"
    }

    Add-Check -Area "evidence-envelope" -Preset "all" -Status "pass" -Detail "F40 command report is pass, 1280x720, five presets, investor proxy column, damage/debrief, and landscape markers"
}

function Test-MobileLandscapeBoundary {
    $projectSettings = Read-RequiredText -RelativePath "unity-mc2-demo\ProjectSettings\ProjectSettings.asset"
    foreach ($marker in @(
        "defaultScreenOrientation: 3",
        "allowedAutorotateToPortrait: 0",
        "allowedAutorotateToPortraitUpsideDown: 0",
        "allowedAutorotateToLandscapeRight: 1",
        "allowedAutorotateToLandscapeLeft: 1",
        "androidResizeableActivity: 0"
    )) {
        Require-Text -Text $projectSettings -Needle $marker -Label "ProjectSettings mobile landscape"
    }

    $bootstrap = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs"
    foreach ($marker in @(
        "ConfigureMobileLandscapeRuntime();",
        "Screen.autorotateToPortrait = false;",
        "Screen.autorotateToPortraitUpsideDown = false;",
        "Screen.autorotateToLandscapeLeft = true;",
        "Screen.autorotateToLandscapeRight = true;",
        "Screen.orientation = ScreenOrientation.AutoRotation;",
        "MobileTouchUi=ready orientation=landscape",
        "landscapeOnly=yes"
    )) {
        Require-Text -Text $bootstrap -Needle $marker -Label "runtime mobile landscape"
    }

    $mobilePlan = Read-RequiredText -RelativePath "docs-mobile-first-plan-2026-06-10.md"
    foreach ($marker in @(
        "The first phone version is landscape-only",
        "horizontal phone game build",
        "layout is not a supported first-version target"
    )) {
        Require-Text -Text $mobilePlan -Needle $marker -Label "mobile landscape plan"
    }

    Add-Check -Area "mobile-landscape-boundary" -Preset "all" -Status "pass" -Detail "phone target is landscape-only; portrait is excluded from first-version support"
}

function Test-CommonPresetRow {
    param(
        [object]$Row,
        [object]$Sidecar,
        [string]$Preset
    )

    foreach ($pathValue in @(
        [string]$Row.screenshot,
        [string]$Row.sidecar,
        [string]$Row.log
    )) {
        Require-File -Path (Resolve-RepoPath -RelativePath $pathValue) -Label "$Preset evidence file"
    }

    $pngPath = Resolve-RepoPath -RelativePath ([string]$Row.screenshot)
    $pngSize = (Get-Item -LiteralPath $pngPath).Length
    if ($pngSize -lt 200000) {
        throw "$Preset screenshot is unexpectedly small for investor evidence: $pngSize bytes"
    }

    if ([int](Read-PropertyValue -Object $Sidecar -Name "screenWidth" -Label "$Preset sidecar") -ne $Width -or [int](Read-PropertyValue -Object $Sidecar -Name "screenHeight" -Label "$Preset sidecar") -ne $Height) {
        throw "$Preset sidecar dimensions expected ${Width}x${Height}, got $($Sidecar.screenWidth)x$($Sidecar.screenHeight)"
    }

    $playableFlow = Read-StringProperty -Object $Row -Name "playableFlowPolish" -Label "$Preset command row"
    foreach ($marker in @(
        "battleHud=sparse",
        "statusRailW=280",
        "statusRailShare1280=0.22",
        "mobileLandscapeOnly=True",
        "orientation=landscape"
    )) {
        Require-Text -Text $playableFlow -Needle $marker -Label "$Preset playableFlowPolish"
    }

    $debrief = Read-StringProperty -Object $Row -Name "debriefRewardSummary" -Label "$Preset command row"
    foreach ($marker in @(
        "damageConsequenceLine=",
        "cockpitEjection=ready",
        "MobileLandscapeOnly: True"
    )) {
        Require-Text -Text $debrief -Needle $marker -Label "$Preset debriefRewardSummary"
    }

    $investorProxy = Read-StringProperty -Object $Row -Name "investorProxyVisuals" -Label "$Preset command row"
    foreach ($marker in @(
        "InvestorProxyVisuals=active",
        "unitFallbackProxy=mech-silhouette+vehicle-hull+infantry-fireteam",
        "propFallbackProxy=tree-canopy+building-roof+hardprop-stripe",
        "collision=unchanged",
        "pathing=unchanged",
        "BattleCore=unchanged",
        "publicSafe=proxy-only",
        "mobileLandscapeOnly=True"
    )) {
        Require-Text -Text $investorProxy -Needle $marker -Label "$Preset investorProxyVisuals"
    }

    $sidecarMobile = Read-StringProperty -Object $Sidecar -Name "mobileTouch" -Label "$Preset sidecar"
    foreach ($marker in @(
        "MobileTouchUi=ready orientation=landscape",
        "landscapeOnly=yes",
        "noDragBox=yes",
        "combatLog=hidden"
    )) {
        Require-Text -Text $sidecarMobile -Needle $marker -Label "$Preset mobileTouch"
    }

    $sidecarProxy = Read-StringProperty -Object $Sidecar -Name "investorProxyVisuals" -Label "$Preset sidecar"
    Require-Text -Text $sidecarProxy -Needle "publicSafe=proxy-only" -Label "$Preset sidecar investorProxyVisuals"

    Add-Check -Area "preset-package" -Preset $Preset -Status "pass" -Detail "sidecar, screenshot, sparse HUD, proxy-art boundary, debrief, and landscape evidence are present"
}

function Test-PresetSpecificStory {
    param(
        [object]$Row,
        [object]$Sidecar,
        [string]$Preset
    )

    $playableFlow = Read-StringProperty -Object $Row -Name "playableFlowPolish" -Label "$Preset command row"
    $debrief = Read-StringProperty -Object $Row -Name "debriefRewardSummary" -Label "$Preset command row"

    switch ($Preset) {
        "damage-demo" {
            foreach ($marker in @(
                "damageStory=section-loss+ejection+wreck",
                "debrief=repair-line"
            )) {
                Require-Text -Text $playableFlow -Needle $marker -Label "damage-demo playableFlowPolish"
            }

            foreach ($marker in @(
                "Repair -",
                "DamageDebrief=section-status+repair-line",
                "repairCost=",
                "cockpitEjection=ready"
            )) {
                Require-Text -Text $debrief -Needle $marker -Label "damage-demo debriefRewardSummary"
            }

            $damageReadability = Read-StringProperty -Object $Sidecar -Name "damageReadability" -Label "damage-demo sidecar"
            foreach ($marker in @(
                "Arms=missing-socket+flag+flight+landing-debris+firepower-marker",
                "Legs=collapse+red-cross+skid+dust+danger-ring+mobility-beacon",
                "Cockpit=breach+ejection-pod+chute+landing+arc+distress+escape-column+route",
                "Wreck=blast+smoke+marker+debris+salvage"
            )) {
                Require-Text -Text $damageReadability -Needle $marker -Label "damage-demo damageReadability"
            }

            Add-Check -Area "damage-debrief-story" -Preset $Preset -Status "pass" -Detail "section loss, cockpit ejection, wreck salvage, and repair-line debrief are visible in sidecar/report"
        }
        "hangar-contact" {
            if ((Read-IntProperty -Object $Row -Name "activeHostiles" -Label "$Preset command row") -le 0 -or (Read-IntProperty -Object $Row -Name "visibleHostiles" -Label "$Preset command row") -le 0) {
                throw "hangar-contact must show active and visible hostiles"
            }
            Require-Text -Text $playableFlow -Needle "contactPressure=ContactPressureCue=objective-panel+in-world" -Label "hangar-contact playableFlowPolish"
            Add-Check -Area "contact-pressure-story" -Preset $Preset -Status "pass" -Detail "hostile pressure is represented in report and objective/in-world cue"
        }
        "solo-order" {
            Require-Text -Text $playableFlow -Needle "soloReturn=order-active" -Label "solo-order playableFlowPolish"
            Require-Text -Text $playableFlow -Needle "detached=1" -Label "solo-order playableFlowPolish"
            Add-Check -Area "solo-command-story" -Preset $Preset -Status "pass" -Detail "single-unit order is detached and visible without drag-box selection"
        }
        "solo-return" {
            Require-Text -Text $playableFlow -Needle "soloReturn=returned" -Label "solo-return playableFlowPolish"
            Require-Text -Text $playableFlow -Needle "detached=0" -Label "solo-return playableFlowPolish"
            Add-Check -Area "solo-command-story" -Preset $Preset -Status "pass" -Detail "single-unit order settles back into squad control"
        }
        default {
            Add-Check -Area "baseline-story" -Preset $Preset -Status "pass" -Detail "spawn baseline keeps sparse HUD and landscape evidence"
        }
    }
}

function Add-NextFixFollowUps {
    Add-FollowUp `
        -Priority "P1" `
        -Area "investor-report-density" `
        -Preset "all" `
        -Issue "F40 markdown contains every raw field, but the report is still too dense for a short investor read." `
        -NextFix "Add a compact executive summary, preset highlights, and top-line damage/proxy/mobile claims before the raw table."

    Add-FollowUp `
        -Priority "P1" `
        -Area "proxy-visual-fidelity" `
        -Preset "all" `
        -Issue "The package proves public-safe proxy visuals and unchanged collision/pathing, but unit identity still depends on procedural silhouette placeholders." `
        -NextFix "Improve controlled-demo proxy materials and silhouette language without relying on private reference assets."

    Add-FollowUp `
        -Priority "P2" `
        -Area "damage-demo-composition" `
        -Preset "damage-demo" `
        -Issue "Damage, cockpit ejection, wreck, and repair consequence are present, but the screenshot/report pairing needs a clearer investor-facing callout." `
        -NextFix "Add a hero damage/ejection callout or paired debrief crop in the next evidence package."

    Add-FollowUp `
        -Priority "P2" `
        -Area "evidence-gate-runtime" `
        -Preset "all" `
        -Issue "The aggregate current-plan gate is broader than this slice and has been slow for PC evidence work." `
        -NextFix "Keep a fast investor-evidence gate separate from the heavy aggregate validation path."
}

if ($PlanOnly) {
    Write-Host "PC controlled-demo investor evidence package audit plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "EvidenceDir: $EvidenceDir"
    Write-Host "CommandEvidenceDir: $CommandEvidenceDir"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "Presets: $($requiredPresets -join ',')"
    Write-Host "WidthHeight: ${Width}x${Height}"
    Write-Host "RefreshEvidence: $RefreshEvidence"
    Write-Host "SkipEvidenceRun: $SkipEvidenceRun"
    return
}

Require-File -Path $commandEvidenceScript -Label "command evidence script"

if ($RefreshEvidence) {
    $refreshArgs = @(
        "-RepoRoot", $RepoRoot,
        "-EvidenceDir", $EvidenceDir,
        "-OutputDir", $CommandEvidenceDir,
        "-Width", $Width,
        "-Height", $Height
    )

    if ($SkipEvidenceRun) {
        $refreshArgs += "-SkipRun"
    }

    & $commandEvidenceScript @refreshArgs
}

Require-File -Path $visualEvidenceReportPath -Label "visual evidence report"
Require-File -Path $commandEvidenceReportPath -Label "command evidence report"
Require-File -Path $commandEvidenceMarkdownPath -Label "command evidence markdown"

$visualReport = Get-Content -LiteralPath $visualEvidenceReportPath -Raw -Encoding UTF8 | ConvertFrom-Json
$commandReport = Get-Content -LiteralPath $commandEvidenceReportPath -Raw -Encoding UTF8 | ConvertFrom-Json
$commandMarkdown = Get-Content -LiteralPath $commandEvidenceMarkdownPath -Raw -Encoding UTF8

if ([string]$visualReport.result -ne "pass") {
    throw "Visual evidence report result must be pass, got $($visualReport.result)"
}
if ([int]$visualReport.width -ne $Width -or [int]$visualReport.height -ne $Height) {
    throw "Visual evidence dimensions expected ${Width}x${Height}, got $($visualReport.width)x$($visualReport.height)"
}

Test-RequiredPresetSet -Evidence @($visualReport.evidence) -Label "visual evidence"
Test-CommandReportEnvelope -CommandReport $commandReport -CommandMarkdown $commandMarkdown
Test-MobileLandscapeBoundary

foreach ($preset in $requiredPresets) {
    $row = Read-EvidenceRow -Evidence @($commandReport.evidence) -Preset $preset -Label "command evidence"
    $visualRow = Read-EvidenceRow -Evidence @($visualReport.evidence) -Preset $preset -Label "visual evidence"
    $sidecar = Read-SidecarForPreset -Preset $preset

    if ([string]$row.screenshot -ne [string]$visualRow.screenshot -or [string]$row.sidecar -ne [string]$visualRow.sidecar) {
        throw "$preset command and visual evidence file paths differ"
    }

    Test-CommonPresetRow -Row $row -Sidecar $sidecar -Preset $preset
    Test-PresetSpecificStory -Row $row -Sidecar $sidecar -Preset $preset
}

Add-NextFixFollowUps
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

$report = [pscustomobject]@{
    schema = "PCControlledDemoInvestorEvidencePackageAudit"
    result = "pass-with-followups"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F41 audit post-F40 PC controlled-demo investor evidence package"
    nextFormalTask = "F42 implement post-F41 PC controlled-demo investor evidence package fixes"
    width = $Width
    height = $Height
    sourceCommandEvidenceReport = (Convert-ToRepoRelativePath -Path $commandEvidenceReportPath)
    sourceCommandEvidenceMarkdown = (Convert-ToRepoRelativePath -Path $commandEvidenceMarkdownPath)
    sourceVisualEvidenceReport = (Convert-ToRepoRelativePath -Path $visualEvidenceReportPath)
    outputMarkdown = (Convert-ToRepoRelativePath -Path $reportMarkdownPath)
    checks = $checks.ToArray()
    followUps = $followUps.ToArray()
}

$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdown = New-Object System.Collections.Generic.List[string]
[void]$markdown.Add("# PC Controlled Demo Investor Evidence Package Audit")
[void]$markdown.Add("")
[void]$markdown.Add("Result: pass-with-followups")
[void]$markdown.Add("")
[void]$markdown.Add('Completed task: `F41 audit post-F40 PC controlled-demo investor evidence package`')
[void]$markdown.Add('Next formal task: `F42 implement post-F41 PC controlled-demo investor evidence package fixes`')
[void]$markdown.Add("")
[void]$markdown.Add("Source command report: `"$(Convert-ToRepoRelativePath -Path $commandEvidenceReportPath)`"")
[void]$markdown.Add("Source visual report: `"$(Convert-ToRepoRelativePath -Path $visualEvidenceReportPath)`"")
[void]$markdown.Add("")
[void]$markdown.Add("## Checks")
[void]$markdown.Add("")
[void]$markdown.Add("| Area | Preset | Status | Detail |")
[void]$markdown.Add("| --- | --- | --- | --- |")
foreach ($check in $checks) {
    [void]$markdown.Add("| $($check.area) | $($check.preset) | $($check.status) | $($check.detail) |")
}
[void]$markdown.Add("")
[void]$markdown.Add("## Follow-Ups")
[void]$markdown.Add("")
[void]$markdown.Add("| Priority | Area | Preset | Issue | Next fix |")
[void]$markdown.Add("| --- | --- | --- | --- | --- |")
foreach ($followUp in $followUps) {
    [void]$markdown.Add("| $($followUp.priority) | $($followUp.area) | $($followUp.preset) | $($followUp.issue) | $($followUp.nextFix) |")
}

$markdown | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "PC controlled-demo investor evidence package audit OK."
Write-Host "Result: pass-with-followups"
Write-Host "Report: $(Convert-ToRepoRelativePath -Path $reportJsonPath)"
Write-Host "Markdown: $(Convert-ToRepoRelativePath -Path $reportMarkdownPath)"

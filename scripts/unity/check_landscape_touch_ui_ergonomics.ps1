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

function Resolve-RepoPath {
    param([string]$RelativePath)
    return Join-Path $RepoRoot ($RelativePath -replace "/", "\")
}

function Read-RepoText {
    param([string]$RelativePath)

    $path = Resolve-RepoPath -RelativePath $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Add-Failure "$RelativePath missing"
        return ""
    }

    return Get-Content -LiteralPath $path -Raw -Encoding UTF8
}

function Assert-Contains {
    param(
        [string]$Text,
        [string]$Needle,
        [string]$Label
    )

    if ([string]::IsNullOrWhiteSpace($Text) -or -not $Text.Contains($Needle)) {
        Add-Failure "$Label missing marker: $Needle"
    }
}

function Assert-NotContains {
    param(
        [string]$Text,
        [string]$Needle,
        [string]$Label
    )

    if (-not [string]::IsNullOrEmpty($Text) -and $Text.Contains($Needle)) {
        Add-Failure "$Label still contains stale marker: $Needle"
    }
}

function Add-OkRow {
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

$bootstrap = Read-RepoText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs"
$mobilePlan = Read-RepoText -RelativePath "docs-mobile-first-plan-2026-06-10.md"
$detailedPlan = Read-RepoText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"
$masterPlan = Read-RepoText -RelativePath "docs-ai-rts-commander-current-master-plan-2026-06-07.md"
$readme = Read-RepoText -RelativePath "README.md"

$sourceMarkers = @(
    "MobileTouchUi=ready orientation=landscape",
    "commandTargets=44",
    "statusRows=44",
    "primaryButtons=44",
    "mapBack=44",
    "systemButtons=44",
    "missionButtons=44",
    "debriefButtons=44",
    "landscapeOnly=yes",
    "noDragBox=yes",
    "combatLog=hidden",
    "MissionListButtonHeightFor(2400f, 1080f) >= MobileTouchMinTargetHeight",
    "MissionResultActionButtonHeightFor(2400f, 1080f) >= MobileTouchMinTargetHeight",
    "MissionListPanelRectFor(1560f, 720f).yMax <= 720f",
    "MissionResultPanelRectFor(1560f, 720f).yMax <= 720f",
    "float buttonHeight = MissionListButtonHeight();",
    "float actionHeight = MissionResultActionButtonHeight();",
    "MissionListPanelRectFor(float screenWidth, float screenHeight)",
    "MissionResultPanelRectFor(float screenWidth, float screenHeight)"
)

foreach ($marker in $sourceMarkers) {
    Assert-Contains -Text $bootstrap -Needle $marker -Label "Mc2DemoBootstrap landscape touch ergonomics"
}

$staleTouchButtonMarkers = @(
    "GUI.Button(new Rect(x, y, width, 30f), ContractsLaunchButtonLabel)",
    "GUI.Button(new Rect(x, y, halfWidth, 30f), `"Mech Lab`")",
    "GUI.Button(new Rect(x + halfWidth + 8f, y, halfWidth, 30f), `"System`")",
    "GUI.Button(new Rect(x, y, width, 30f), returnText)",
    "GUI.Button(new Rect(panel.x + 18f, panel.y + 296f, actionWidth, 30f), DebriefRepairMechLabButtonLabel)",
    "GUI.Button(new Rect(panel.x + 26f + actionWidth, panel.y + 296f, actionWidth, 30f), DebriefNextContractButtonLabel)",
    "GUI.Button(new Rect(panel.x + 18f, panel.y + 334f, actionWidth, 30f), DebriefRetryButtonLabel)",
    "GUI.Button(new Rect(panel.x + 26f + actionWidth, panel.y + 334f, actionWidth, 30f), DebriefCloseButtonLabel)"
)

foreach ($marker in $staleTouchButtonMarkers) {
    Assert-NotContains -Text $bootstrap -Needle $marker -Label "Mc2DemoBootstrap stale 30px visible-flow controls"
}

Add-OkRow -Check "runtime source" -Detail "$($sourceMarkers.Count) marker(s), stale visible-flow 30px controls rejected"

$docMarkers = @(
    "F21 audit landscape touch UI ergonomics",
    "F22 audit landscape MechLab touch controls",
    "F23 capture landscape MechLab touch evidence",
    "F24 capture Android MechLab touch evidence",
    "check_landscape_touch_ui_ergonomics.ps1",
    "Landscape touch UI ergonomics check OK"
)

foreach ($marker in $docMarkers) {
    Assert-Contains -Text $mobilePlan -Needle $marker -Label "mobile plan"
    Assert-Contains -Text $detailedPlan -Needle $marker -Label "detailed plan"
}

Assert-Contains -Text $masterPlan -Needle '| 99 | Done | `Audit landscape touch UI ergonomics` |' -Label "master plan F21"
Assert-Contains -Text $masterPlan -Needle '| 100 | Done | `Audit landscape MechLab touch controls` |' -Label "master plan F22"
Assert-Contains -Text $masterPlan -Needle '| 101 | Done | `Capture landscape MechLab touch evidence` |' -Label "master plan F23"
Assert-Contains -Text $masterPlan -Needle '| 102 | Done | `Capture Android MechLab touch evidence` |' -Label "master plan F24"
Assert-Contains -Text $detailedPlan -Needle '| F21 | Done | `Audit landscape touch UI ergonomics` |' -Label "detailed plan F21"
Assert-Contains -Text $detailedPlan -Needle '| F22 | Done | `Audit landscape MechLab touch controls` |' -Label "detailed plan F22"
Assert-Contains -Text $detailedPlan -Needle '| F23 | Done | `Capture landscape MechLab touch evidence` |' -Label "detailed plan F23"
Assert-Contains -Text $detailedPlan -Needle '| F24 | Done | `Capture Android MechLab touch evidence` |' -Label "detailed plan F24"
Assert-Contains -Text $readme -Needle "F21 audit landscape touch UI ergonomics" -Label "README F21"
Assert-Contains -Text $readme -Needle "F22 audit landscape MechLab touch controls" -Label "README F22"
Assert-Contains -Text $readme -Needle "F23 capture landscape MechLab touch evidence" -Label "README F23"
Assert-Contains -Text $readme -Needle "F24 capture Android MechLab touch evidence" -Label "README F24"
Add-OkRow -Check "plan docs" -Detail "F21/F22/F23/F24 done, F25 next"

if ($failures.Count -gt 0) {
    Write-Host "Landscape touch UI ergonomics check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) landscape touch UI ergonomics check(s) failed."
}

Write-Host "Landscape touch UI ergonomics check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize

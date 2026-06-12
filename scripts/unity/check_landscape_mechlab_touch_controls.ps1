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
    "mechLabActions=44",
    "mechLabShop=44",
    "mechLabHire=44",
    "mechLabRoster=44",
    "mechLabSquad=44",
    "mechLabWeaponButtons=44",
    "mechLabRepair=44",
    "MechLabTouchControlHeightFor(2400f, 1080f) >= MobileTouchMinTargetHeight",
    "LoadoutWeaponButtonHeightFor(2400f, 1080f) >= MobileTouchMinTargetHeight",
    "LoadoutRepairButtonHeightFor(2400f, 1080f) >= MobileTouchMinTargetHeight",
    "private static float MechLabTouchControlHeight()",
    "private static float LoadoutWeaponButtonHeight()",
    "private static float LoadoutRepairButtonHeight()",
    "private const float MobileTouchLoadoutCardHeight = 654f;",
    "private const float MobileTouchLoadoutCardStride = 666f;",
    "LoadoutCardStrideForCurrentLayout()",
    "GUI.Button(new Rect(x, y - 2f, buttonWidth, buttonHeight), `"Buy`")",
    "GUI.Button(new Rect(x, y - 2f, buttonWidth, buttonHeight), `"Hire`")",
    "GUI.Button(new Rect(x, y - 2f, buttonWidth, buttonHeight), `"Review`")",
    "GUI.Button(new Rect(x, y - 2f, buttonWidth, buttonHeight), `"Next Squad`")",
    "GUI.Box(new Rect(x, y, width, touchLayout ? 410f : 238f), `"Next Contract Squad`")",
    "float buttonHeight = LoadoutWeaponButtonHeight();",
    "LoadoutRepairButtonHeight()"
)

foreach ($marker in $sourceMarkers) {
    Assert-Contains -Text $bootstrap -Needle $marker -Label "Mc2DemoBootstrap landscape MechLab touch controls"
}

$staleButtonMarkers = @(
    "GUI.Button(new Rect(x, y - 2f, 46f, 22f), `"Buy`")",
    "GUI.Button(new Rect(x, y - 2f, 46f, 22f), `"Hire`")",
    "GUI.Button(new Rect(x, y - 2f, 72f, 22f), `"Review`")",
    "GUI.Button(new Rect(x, y - 2f, 92f, 22f), `"Next Squad`")",
    "GUI.Button(new Rect(x + width - 58f, y + 4f, 48f, 22f), SquadSelectionBackButtonLabel)",
    "GUI.Button(new Rect(x, y - 2f, 28f, 22f), `"<`")",
    "GUI.Button(new Rect(x + 34f, y - 2f, 28f, 22f), `">`")",
    "DrawActionButton(new Rect(x, y - 2f, 72f, 22f), `"Set`", canConfirm)",
    "DrawActionButton(new Rect(x, y - 2f, 72f, 22f), `"Launch`", ready)",
    "GUI.Button(new Rect(right, y - 2f, LoadoutRepairButtonWidth, 22f), LoadoutRepairButtonLabel(repairCost))",
    "float buttonHeight = touchLayout ? 36f : 20f;"
)

foreach ($marker in $staleButtonMarkers) {
    Assert-NotContains -Text $bootstrap -Needle $marker -Label "Mc2DemoBootstrap stale MechLab touch controls"
}

Add-OkRow -Check "runtime source" -Detail "$($sourceMarkers.Count) marker(s), stale MechLab 22/36px controls rejected"

$docMarkers = @(
    "F22 audit landscape MechLab touch controls",
    "F23 capture landscape MechLab touch evidence",
    "F24 capture Android MechLab touch evidence",
    "check_landscape_mechlab_touch_controls.ps1",
    "Landscape MechLab touch controls check OK"
)

foreach ($marker in $docMarkers) {
    Assert-Contains -Text $mobilePlan -Needle $marker -Label "mobile plan"
    Assert-Contains -Text $detailedPlan -Needle $marker -Label "detailed plan"
}

Assert-Contains -Text $masterPlan -Needle '| 100 | Done | `Audit landscape MechLab touch controls` |' -Label "master plan F22"
Assert-Contains -Text $masterPlan -Needle '| 101 | Done | `Capture landscape MechLab touch evidence` |' -Label "master plan F23"
Assert-Contains -Text $masterPlan -Needle '| 102 | Next | `Capture Android MechLab touch evidence` |' -Label "master plan F24"
Assert-Contains -Text $detailedPlan -Needle '| F22 | Done | `Audit landscape MechLab touch controls` |' -Label "detailed plan F22"
Assert-Contains -Text $detailedPlan -Needle '| F23 | Done | `Capture landscape MechLab touch evidence` |' -Label "detailed plan F23"
Assert-Contains -Text $detailedPlan -Needle '| F24 | Next | `Capture Android MechLab touch evidence` |' -Label "detailed plan F24"
Assert-Contains -Text $readme -Needle "F22 audit landscape MechLab touch controls" -Label "README F22"
Assert-Contains -Text $readme -Needle "F23 capture landscape MechLab touch evidence" -Label "README F23"
Assert-Contains -Text $readme -Needle "F24 capture Android MechLab touch evidence" -Label "README F24"
Add-OkRow -Check "plan docs" -Detail "F22/F23 done, F24 next"

if ($failures.Count -gt 0) {
    Write-Host "Landscape MechLab touch controls check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) landscape MechLab touch control check(s) failed."
}

Write-Host "Landscape MechLab touch controls check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize

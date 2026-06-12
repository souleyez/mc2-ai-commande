param(
    [string]$RepoRoot = "",
    [string]$LogPath = ""
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

    return Get-Content -LiteralPath $path -Raw
}

function Assert-Contains {
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

function Assert-NotContains {
    param(
        [string]$Text,
        [string]$Needle,
        [string]$Label
    )

    if (-not [string]::IsNullOrWhiteSpace($Text) -and $Text.Contains($Needle)) {
        Add-Failure "$Label still contains forbidden marker: $Needle"
        return
    }

    Add-Row -Check "$Label forbidden marker" -Detail $Needle
}

function Assert-RemainingPrimitiveLines {
    param([string]$RelativePath)

    $path = Resolve-RepoPath -RelativePath $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Add-Failure "$RelativePath missing"
        return
    }

    $allowed = @()

    $lines = Get-Content -LiteralPath $path
    $bad = New-Object System.Collections.Generic.List[string]
    for ($index = 0; $index -lt $lines.Count; $index++) {
        $line = $lines[$index]
        if ($line.Contains("GameObject.CreatePrimitive") -and -not ($allowed -contains $line.Trim())) {
            [void]$bad.Add(("{0}:{1}" -f ($index + 1), $line.Trim()))
        }
    }

    if ($bad.Count -gt 0) {
        foreach ($entry in $bad) {
            Add-Failure "$RelativePath unexpected runtime primitive path: $entry"
        }
        return
    }

    Add-Row -Check "$RelativePath remaining primitive paths" -Detail "No runtime GameObject.CreatePrimitive calls remain"
}

$factory = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\DemoPrimitiveVisualFactory.cs"
Assert-Contains -Text $factory -Needle "visual.AddComponent<MeshFilter>();" -Label "visual primitive factory"
Assert-Contains -Text $factory -Needle "visual.AddComponent<MeshRenderer>();" -Label "visual primitive factory"
Assert-NotContains -Text $factory -Needle "AddComponent<Collider>" -Label "visual primitive factory"
Assert-NotContains -Text $factory -Needle "GameObject.CreatePrimitive" -Label "visual primitive factory"

$unitView = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\DemoUnitView.cs"
Assert-Contains -Text $unitView -Needle "DemoPrimitiveVisualFactory.Create(primitive, Unit.Id + `" `" + sectionName)" -Label "DemoUnitView visual factory"
Assert-Contains -Text $unitView -Needle "DemoPrimitiveVisualFactory.Create(PrimitiveType.Cylinder, effectName)" -Label "DemoUnitView beam factory"
Assert-NotContains -Text $unitView -Needle "GameObject.CreatePrimitive" -Label "DemoUnitView"
Assert-NotContains -Text $unitView -Needle "GetComponent<Collider>" -Label "DemoUnitView"

$structureView = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\DemoStructureView.cs"
Assert-Contains -Text $structureView -Needle "DemoPrimitiveVisualFactory.Create(primitive, effectName)" -Label "DemoStructureView effect factory"
Assert-Contains -Text $structureView -Needle "DemoPrimitiveVisualFactory.Create(PrimitiveType.Cylinder, Structure.Id + `" `" + cueName)" -Label "DemoStructureView cue factory"
Assert-NotContains -Text $structureView -Needle "GameObject.CreatePrimitive" -Label "DemoStructureView"
Assert-NotContains -Text $structureView -Needle "GetComponent<Collider>" -Label "DemoStructureView"

$bootstrap = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs"
foreach ($marker in @(
    'DemoPrimitiveVisualFactory.Create(PrimitiveType.Cylinder, "Weapon Beam")',
    'DemoPrimitiveVisualFactory.Create(PrimitiveType.Sphere, destroyedTarget ? "Kill Impact" : "Hit Impact")',
    'DemoPrimitiveVisualFactory.Create(PrimitiveType.Cylinder, effectName)',
    'DemoPrimitiveVisualFactory.Create(PrimitiveType.Plane, "Mission Ground")',
    'DemoPrimitiveVisualFactory.Create(primitive, unit.Id + " " + unit.UnitType)',
    'DemoPrimitiveVisualFactory.Create(PrimitiveType.Cube, structure.Id + " " + structure.ObjectType)',
    'DemoPrimitiveVisualFactory.Create(PrimitiveType.Cylinder, "Occupancy " + region.Kind + " " + region.Id)',
    'DemoPrimitiveVisualFactory.Create(PrimitiveType.Cylinder, "Nav Marker " + marker.index)',
    'DemoPrimitiveVisualFactory.Create(PrimitiveType.Cylinder, "Forest Canopy Footprint")',
    'DemoPrimitiveVisualFactory.Create(PrimitiveType.Cylinder, objectName)',
    'DemoPrimitiveVisualFactory.Create(PrimitiveType.Cube, objectName)'
)) {
    Assert-Contains -Text $bootstrap -Needle $marker -Label "Mc2DemoBootstrap visual factory"
}

Assert-RemainingPrimitiveLines -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs"

$androidEvidence = Read-RequiredText -RelativePath "scripts\unity\capture_android_battle_command_touch_evidence.ps1"
foreach ($marker in @(
    "class 'CapsuleCollider' doesn't exist",
    "class 'SphereCollider' doesn't exist",
    "class 'BoxCollider' doesn't exist"
)) {
    Assert-Contains -Text $androidEvidence -Needle $marker -Label "Android battle command touch forbidden log marker"
}

if (-not [string]::IsNullOrWhiteSpace($LogPath)) {
    $resolvedLogPath = (Resolve-Path -LiteralPath $LogPath).Path
    $logText = Get-Content -LiteralPath $resolvedLogPath -Raw -Encoding UTF8
    foreach ($marker in @(
        "class 'CapsuleCollider' doesn't exist",
        "class 'SphereCollider' doesn't exist",
        "class 'BoxCollider' doesn't exist"
    )) {
        Assert-NotContains -Text $logText -Needle $marker -Label "Android runtime log"
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Android combat effect log noise check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) Android combat effect log noise check(s) failed."
}

Write-Host "Android combat effect log noise check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize

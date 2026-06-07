param(
    [string[]]$Names = @(
        "werewolf",
        "bushwacker",
        "centipede",
        "harasser",
        "lrmc",
        "urbanmech",
        "starslayer"
    ),
    [string]$InputRoot = "",
    [string]$OutputRoot = "",
    [string]$ManifestPath = "",
    [string]$MissionContractPath = "",
    [switch]$NoCopyTextures,
    [switch]$IncludeMissionProps
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptRoot "..\..")

if ([string]::IsNullOrWhiteSpace($InputRoot)) {
    $InputRoot = Join-Path $repoRoot "analysis-output\fst-unpack\tgl.fst\data\tgl"
}

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot "analysis-output\unity-reference-art\assets"
}

if ([string]::IsNullOrWhiteSpace($ManifestPath)) {
    $ManifestPath = Join-Path $repoRoot "analysis-output\unity-reference-art\manifest.json"
}

if ([string]::IsNullOrWhiteSpace($MissionContractPath)) {
    $MissionContractPath = Join-Path $repoRoot "unity-mc2-demo\Assets\StreamingAssets\Missions\mc2_01\mission-contract.json"
}

$exporter = Join-Path $scriptRoot "export_tgl_to_obj.py"
if (!(Test-Path $exporter)) {
    throw "Missing exporter: $exporter"
}

if (!(Test-Path $InputRoot)) {
    throw "Missing unpacked TGL input root: $InputRoot"
}

function Get-ReferenceShapeName {
    param([string]$Name)

    if ([string]::IsNullOrWhiteSpace($Name)) {
        return ""
    }

    $shapeName = [System.IO.Path]::GetFileNameWithoutExtension($Name.Trim())
    if ($shapeName -ieq "SlayerP") {
        return "slayerparked"
    }

    return $shapeName
}

function Expand-ReferenceShapeNames {
    param([string[]]$RawNames)

    $expanded = New-Object System.Collections.Generic.List[string]
    foreach ($rawName in $RawNames) {
        foreach ($part in ($rawName -split ",")) {
            $shapeName = Get-ReferenceShapeName -Name $part
            if (-not [string]::IsNullOrWhiteSpace($shapeName)) {
                $expanded.Add($shapeName)
            }
        }
    }

    return $expanded.ToArray()
}

function Test-FirstSliceTerrainObject {
    param($TerrainObject)

    if ($null -eq $TerrainObject) {
        return $false
    }

    if ($TerrainObject.sourceIndex -le 260 -or $TerrainObject.objectClass -eq "BUILDING") {
        return $true
    }

    if ($null -eq $TerrainObject.position) {
        return $false
    }

    return $TerrainObject.position.x -ge 2200 -and
        $TerrainObject.position.x -le 3950 -and
        $TerrainObject.position.y -ge -1300 -and
        $TerrainObject.position.y -le 350
}

$Names = Expand-ReferenceShapeNames -RawNames $Names

if ($IncludeMissionProps) {
    if (!(Test-Path -LiteralPath $MissionContractPath -PathType Leaf)) {
        throw "Missing mission contract for prop visual export: $MissionContractPath"
    }

    $contract = Get-Content -LiteralPath $MissionContractPath -Raw | ConvertFrom-Json
    $propNames = @()
    $propNames += @($contract.staticObjects | ForEach-Object { Get-ReferenceShapeName -Name $_.assetId })
    $propNames += @($contract.staticObjects | ForEach-Object { Get-ReferenceShapeName -Name $_.objectProfile })
    $propNames += @($contract.terrainObjects | Where-Object { Test-FirstSliceTerrainObject -TerrainObject $_ } | ForEach-Object { Get-ReferenceShapeName -Name $_.assetId })
    $propNames += @($contract.terrainObjects | Where-Object { Test-FirstSliceTerrainObject -TerrainObject $_ } | ForEach-Object { Get-ReferenceShapeName -Name $_.fileName })

    $Names = @(
        $Names + $propNames |
            Where-Object {
                -not [string]::IsNullOrWhiteSpace($_) -and
                (Test-Path -LiteralPath (Join-Path $InputRoot ($_.Trim() + ".tgl")) -PathType Leaf)
            } |
            Sort-Object -Unique
    )
}

$nameArg = ($Names | Where-Object { ![string]::IsNullOrWhiteSpace($_) }) -join ","
if ([string]::IsNullOrWhiteSpace($nameArg)) {
    throw "At least one visual asset name is required."
}

$args = @(
    $exporter,
    "--input-root", $InputRoot,
    "--output-root", $OutputRoot,
    "--manifest-path", $ManifestPath,
    "--name", $nameArg
)

if (!$NoCopyTextures) {
    $args += "--copy-textures"
}

Write-Host "Exporting private reference visuals: $nameArg"
Write-Host "OutputRoot: $OutputRoot"
Write-Host "Manifest: $ManifestPath"

& python @args
if ($LASTEXITCODE -ne 0) {
    throw "Reference visual export failed with exit code $LASTEXITCODE"
}

if (!(Test-Path $ManifestPath)) {
    throw "Reference visual export did not create manifest: $ManifestPath"
}

$manifest = Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json
$exportCount = if ($null -ne $manifest.exports) { @($manifest.exports).Count } else { 0 }
$missingSourceCount = if ($null -ne $manifest.missingSources) { @($manifest.missingSources).Count } else { 0 }
Write-Host ("Reference visual exports: {0}; missing sources: {1}" -f $exportCount, $missingSourceCount)
if ($missingSourceCount -gt 0) {
    foreach ($missing in @($manifest.missingSources)) {
        Write-Warning ("Missing private reference source for {0}: {1}" -f $missing.assetId, $missing.sourcePath)
    }
}

if ($null -ne $manifest.warnings) {
    foreach ($warning in @($manifest.warnings)) {
        if (-not [string]::IsNullOrWhiteSpace([string]$warning)) {
            Write-Warning ([string]$warning)
        }
    }
}

Write-Host "Reference visual manifest ready: $ManifestPath"

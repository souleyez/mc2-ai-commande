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
    [switch]$NoCopyTextures
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

$exporter = Join-Path $scriptRoot "export_tgl_to_obj.py"
if (!(Test-Path $exporter)) {
    throw "Missing exporter: $exporter"
}

if (!(Test-Path $InputRoot)) {
    throw "Missing unpacked TGL input root: $InputRoot"
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

Write-Host "Reference visual manifest ready: $ManifestPath"

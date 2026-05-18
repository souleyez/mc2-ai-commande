param(
    [Parameter(Mandatory = $false)]
    [string]$RunPath = ""
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$validateScript = Join-Path $PSScriptRoot "validate_content_pack.ps1"

if ([string]::IsNullOrWhiteSpace($RunPath)) {
    $RunPath = Join-Path $repoRoot "runtime-shell-dev"
}

if (-not (Test-Path -LiteralPath $RunPath -PathType Container)) {
    throw "Runtime shell does not exist: $RunPath"
}

$resolvedRun = (Resolve-Path -LiteralPath $RunPath).Path
$markerPath = Join-Path $resolvedRun ".content-pack-mounted.json"

Write-Output "Runtime shell: $resolvedRun"

if (Test-Path -LiteralPath $markerPath -PathType Leaf) {
    $marker = Get-Content -LiteralPath $markerPath -Raw | ConvertFrom-Json
    Write-Output "Mounted pack id: $($marker.id)"
    Write-Output "Mounted pack title: $($marker.title)"
    Write-Output "Mounted pack root: $($marker.packRoot)"
    Write-Output "Mounted at: $($marker.mountedAt)"
    Write-Output "Manifest: $($marker.manifestPath)"
}
else {
    Write-Output "Mounted pack marker: missing"
}

$shellFiles = @(
    "mc2.exe",
    "mc2res_64.dll",
    "SDL2.dll",
    "SDL2_mixer.dll",
    "SDL2_ttf.dll",
    "glew32.dll",
    "zlib1.dll"
)

$missingShellFiles = foreach ($relativePath in $shellFiles) {
    if (-not (Test-Path -LiteralPath (Join-Path $resolvedRun $relativePath) -PathType Leaf)) {
        $relativePath
    }
}

if ($missingShellFiles.Count -gt 0) {
    Write-Output "Shell files: missing"
    $missingShellFiles | ForEach-Object { Write-Output "  - $_" }
}
else {
    Write-Output "Shell files: OK"
}

& $validateScript -PackPath $resolvedRun | Out-Host

$running = Get-Process mc2 -ErrorAction SilentlyContinue |
    Where-Object { $_.Path -eq (Join-Path $resolvedRun "mc2.exe") }

if ($running) {
    $running | ForEach-Object {
        Write-Output "Runtime process: running pid=$($_.Id)"
    }
}
else {
    Write-Output "Runtime process: not running from this shell"
}

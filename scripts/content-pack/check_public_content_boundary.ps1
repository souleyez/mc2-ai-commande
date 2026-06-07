param(
    [Parameter(Mandatory = $true)]
    [string]$Path,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun,

    [Parameter(Mandatory = $false)]
    [int]$MaxFindings = 200
)

$ErrorActionPreference = "Stop"

if ($MaxFindings -lt 1) {
    throw "MaxFindings must be at least 1."
}

if (-not (Test-Path -LiteralPath $Path)) {
    throw "Path does not exist: $Path"
}

$target = Get-Item -LiteralPath $Path -Force
if ($target.PSIsContainer) {
    $scanRoot = $target.FullName
    $scanItems = @($target) + @(Get-ChildItem -LiteralPath $target.FullName -Recurse -Force)
}
else {
    $scanRoot = Split-Path -Parent $target.FullName
    $scanItems = @($target)
}

$pathRules = @(
    [ordered]@{
        name = "PrivateReferencePackPath"
        pattern = "(?i)(^|[\\/])(mc2-run64-dev|project-owned-linked-dev|runtime-shell-dev|release-run)$"
    },
    [ordered]@{
        name = "LocalExtractionOrAnalysisPath"
        pattern = "(?i)(^|[\\/])(analysis-output|mission-extract|mission-analysis|fst-unpack|pak-unpack|tgl-obj|reference-visual-captures|unity-reference-art|private-reference-art)$"
    },
    [ordered]@{
        name = "ReferenceManifestPath"
        pattern = "(?i)(^|[\\/])(mc2-original\.local\.example\.json|mc2-original|local-reference|reference-linked)(\.[^\\/]*)?$"
    },
    [ordered]@{
        name = "LegacyTechnicalBuildName"
        pattern = "(?i)(^|[\\/])(MC2UnityDemo(_Data)?|MC2UnityDemo\.exe|mc2_[^\\/]+)$"
    }
)

$contentRules = @(
    [ordered]@{
        name = "LocalAbsoluteProjectPath"
        pattern = "(?i)[A-Z]:\\Users\\[^\\]+\\Desktop\\codex\\mechcommander2-mc2\\"
    },
    [ordered]@{
        name = "PrivateReferencePackPath"
        pattern = "(?i)\b(mc2-run64-dev|project-owned-linked-dev|runtime-shell-dev|release-run)\b"
    },
    [ordered]@{
        name = "LocalExtractionOrAnalysisPath"
        pattern = "(?i)\b(analysis-output|mission-extract|mission-analysis|fst-unpack|pak-unpack|tgl-obj|reference-visual-captures|unity-reference-art|private-reference-art)\b"
    },
    [ordered]@{
        name = "ReferenceLinkedManifestMarker"
        pattern = "(?i)(ReferenceLinks|local-reference-only|reference-linked|development-only|private validation|private-reference|local reference)"
    },
    [ordered]@{
        name = "LegacyProductOrTrademark"
        pattern = "(?i)\b(MechCommander|MechWarrior|BattleTech|FASA|Microsoft Game Studios)\b"
    },
    [ordered]@{
        name = "LegacyMissionOrUnitName"
        pattern = "(?i)(\bmc2_[A-Za-z0-9_-]*|Starslayer|Urbies|sourceKind""\s*:\s*""mc2-|source-paced|source mc2|source-derived)"
    },
    [ordered]@{
        name = "CloneOrOriginalCopyMarker"
        pattern = "(?i)(\bclone\b|原版|旧作|复刻)"
    }
)

$textExtensions = New-Object "System.Collections.Generic.HashSet[string]" ([System.StringComparer]::OrdinalIgnoreCase)
@(
    ".abl",
    ".cfg",
    ".config",
    ".csv",
    ".fit",
    ".html",
    ".info",
    ".ini",
    ".json",
    ".log",
    ".manifest",
    ".md",
    ".meta",
    ".ps1",
    ".shader",
    ".txt",
    ".xml",
    ".yaml",
    ".yml"
) | ForEach-Object { [void]$textExtensions.Add($_) }

$script:totalFindings = 0
$script:findings = New-Object System.Collections.Generic.List[object]

function ConvertTo-RelativePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Root,

        [Parameter(Mandatory = $true)]
        [string]$ItemPath
    )

    $rootFull = [System.IO.Path]::GetFullPath($Root).TrimEnd("\", "/")
    $itemFull = [System.IO.Path]::GetFullPath($ItemPath)
    if ($itemFull.Equals($rootFull, [System.StringComparison]::OrdinalIgnoreCase)) {
        return "."
    }

    $rootWithSlash = $rootFull + [System.IO.Path]::DirectorySeparatorChar
    if ($itemFull.StartsWith($rootWithSlash, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $itemFull.Substring($rootWithSlash.Length).Replace("\", "/")
    }

    return $itemFull.Replace("\", "/")
}

function Get-Snippet {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Value
    )

    $snippet = ($Value -replace "\s+", " ").Trim()
    if ($snippet.Length -gt 180) {
        return $snippet.Substring(0, 177) + "..."
    }

    return $snippet
}

function Add-Finding {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Rule,

        [Parameter(Mandatory = $true)]
        [string]$Scope,

        [Parameter(Mandatory = $true)]
        [string]$RelativePath,

        [Parameter(Mandatory = $false)]
        [int]$Line = 0,

        [Parameter(Mandatory = $false)]
        [string]$Snippet = ""
    )

    $script:totalFindings++
    if ($script:findings.Count -ge $MaxFindings) {
        return
    }

    $script:findings.Add([ordered]@{
        rule = $Rule
        scope = $Scope
        path = $RelativePath
        line = $Line
        snippet = $Snippet
    })
}

function Test-TextFile {
    param(
        [Parameter(Mandatory = $true)]
        [System.IO.FileInfo]$File
    )

    $extension = [System.IO.Path]::GetExtension($File.FullName)
    return $textExtensions.Contains($extension)
}

foreach ($item in $scanItems) {
    $relativePath = ConvertTo-RelativePath -Root $scanRoot -ItemPath $item.FullName
    foreach ($rule in $pathRules) {
        if ($relativePath -match $rule.pattern) {
            Add-Finding -Rule $rule.name -Scope "path" -RelativePath $relativePath -Snippet (Get-Snippet -Value $relativePath)
        }
    }
}

if ($target.PSIsContainer) {
    $targetPathForRules = $target.FullName.Replace("\", "/")
    foreach ($rule in $pathRules) {
        if ($targetPathForRules -match $rule.pattern) {
            Add-Finding -Rule $rule.name -Scope "path" -RelativePath "." -Snippet (Get-Snippet -Value $target.FullName)
        }
    }
}

foreach ($file in ($scanItems | Where-Object { -not $_.PSIsContainer })) {
    $fileInfo = [System.IO.FileInfo]$file
    if (-not (Test-TextFile -File $fileInfo)) {
        continue
    }

    $relativePath = ConvertTo-RelativePath -Root $scanRoot -ItemPath $fileInfo.FullName
    $lineNumber = 0
    try {
        foreach ($line in [System.IO.File]::ReadLines($fileInfo.FullName)) {
            $lineNumber++
            foreach ($rule in $contentRules) {
                if ($line -match $rule.pattern) {
                    Add-Finding -Rule $rule.name -Scope "content" -RelativePath $relativePath -Line $lineNumber -Snippet (Get-Snippet -Value $line)
                }
            }
        }
    }
    catch {
        Add-Finding -Rule "UnreadableTextFile" -Scope "content" -RelativePath $relativePath -Line $lineNumber -Snippet $_.Exception.Message
    }
}

$mode = if ($DryRun) { "dry-run/read-only" } else { "read-only" }
Write-Output "Public content boundary check"
Write-Output "Path: $($target.FullName)"
Write-Output "Mode: $mode"
Write-Output "Rules: $($pathRules.Count) path, $($contentRules.Count) content"

if ($script:totalFindings -gt 0) {
    Write-Output "Result: FAILED"
    Write-Output "Findings: $script:totalFindings"
    if ($script:totalFindings -gt $script:findings.Count) {
        Write-Output "Showing first $($script:findings.Count) findings; use -MaxFindings to change the display limit."
    }

    foreach ($finding in $script:findings) {
        if ($finding.line -gt 0) {
            Write-Output ("[{0}] {1}:{2} ({3}) {4}" -f $finding.rule, $finding.path, $finding.line, $finding.scope, $finding.snippet)
        }
        else {
            Write-Output ("[{0}] {1} ({2}) {3}" -f $finding.rule, $finding.path, $finding.scope, $finding.snippet)
        }
    }

    exit 1
}

Write-Output "Result: OK"
Write-Output "No forbidden private/reference markers were found."
exit 0

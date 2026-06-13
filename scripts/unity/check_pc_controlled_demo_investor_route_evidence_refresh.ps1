param(
    [string]$RepoRoot = "",
    [string]$OutputDir = "",
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
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-investor-route-evidence-refresh"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
}

$repoFullPath = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([char[]]@("\", "/"))
$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
if (-not $OutputDir.StartsWith($repoFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDir must stay inside RepoRoot: $OutputDir"
}

$reportJsonPath = Join-Path $OutputDir "pc-controlled-demo-investor-route-evidence-refresh.json"
$reportMarkdownPath = Join-Path $OutputDir "pc-controlled-demo-investor-route-evidence-refresh.md"
$failures = New-Object System.Collections.Generic.List[string]
$rows = New-Object System.Collections.Generic.List[object]

function Resolve-RepoPath {
    param([string]$RelativePath)
    return Join-Path $RepoRoot ($RelativePath -replace "/", "\")
}

function Read-RepoText {
    param([string]$RelativePath)

    $path = Resolve-RepoPath -RelativePath $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        [void]$failures.Add("$RelativePath missing")
        return ""
    }

    return Get-Content -LiteralPath $path -Raw -Encoding UTF8
}

function Read-RepoJson {
    param([string]$RelativePath)

    $text = Read-RepoText -RelativePath $RelativePath
    if ([string]::IsNullOrWhiteSpace($text)) {
        return $null
    }

    try {
        return $text | ConvertFrom-Json
    }
    catch {
        [void]$failures.Add("$RelativePath is not valid JSON: $($_.Exception.Message)")
        return $null
    }
}

function Assert-Contains {
    param(
        [string]$Text,
        [string]$Needle,
        [string]$Label
    )

    if ([string]::IsNullOrWhiteSpace($Text) -or -not $Text.Contains($Needle)) {
        [void]$failures.Add("$Label missing marker: $Needle")
        return
    }

    [void]$rows.Add([pscustomobject]@{
        check = $Label
        status = "OK"
        detail = $Needle
    })
}

function Assert-All {
    param(
        [string]$Text,
        [string[]]$Needles,
        [string]$Label
    )

    foreach ($needle in $Needles) {
        Assert-Contains -Text $Text -Needle $needle -Label $Label
    }
}

function Assert-Equals {
    param(
        [object]$Actual,
        [object]$Expected,
        [string]$Label
    )

    if ($Actual -ne $Expected) {
        [void]$failures.Add("$Label expected '$Expected', got '$Actual'")
        return
    }

    [void]$rows.Add([pscustomobject]@{
        check = $Label
        status = "OK"
        detail = [string]$Expected
    })
}

function Assert-FileExists {
    param(
        [string]$RelativePath,
        [string]$Label
    )

    $path = Resolve-RepoPath -RelativePath $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        [void]$failures.Add("$Label missing: $RelativePath")
        return
    }

    $file = Get-Item -LiteralPath $path
    if ($file.Length -le 0) {
        [void]$failures.Add("$Label is empty: $RelativePath")
    }
    else {
        [void]$rows.Add([pscustomobject]@{
            check = $Label
            status = "OK"
            detail = $RelativePath
        })
    }
}

if ($PlanOnly) {
    Write-Host "PC controlled-demo investor route evidence refresh plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "NoUnityLaunch: True"
    return
}

$commandCapture = Read-RepoText -RelativePath "scripts\unity\capture_pc_controlled_demo_command_evidence.ps1"
$commandMarkdown = Read-RepoText -RelativePath "analysis-output\pc-controlled-demo-command-evidence\pc-controlled-demo-command-evidence.md"
$commandReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-command-evidence\pc-controlled-demo-command-evidence.json"
$f45PolishReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-investor-evidence-polish-fixes\pc-controlled-demo-investor-evidence-polish-fixes.json"
$currentGate = Read-RepoText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
$queueScript = Read-RepoText -RelativePath "scripts\unity\check_current_plan_queue.ps1"
$handoffScript = Read-RepoText -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"
$gitignore = Read-RepoText -RelativePath ".gitignore"

Assert-All -Text $commandCapture -Label "command evidence F46 source metadata" -Needles @(
    'F46 refresh PC controlled-demo investor route evidence after polish fixes',
    'F47 audit post-F46 PC controlled-demo investor route evidence refresh',
    '## Investor Route Summary',
    'DamageProof=damage-demo screenshot=$damageScreenshot sidecar=$damageSidecar log=$damageLog',
    'LandscapePhoneProof=mobileLandscapeOnly=True orientation=landscape firstPhoneVersion=horizontal-only portraitSupport=False',
    'ProxyParsing=source=proxyIdentity+materialLanguage+propIdentity sidecarFallback=investorProxyVisuals splitSidecarRecapturePending=True publicSafe=proxy-only'
)

Assert-All -Text $commandMarkdown -Label "command evidence F46 refreshed markdown" -Needles @(
    'Completed task: `F46 refresh PC controlled-demo investor route evidence after polish fixes`',
    'Next formal task: `F47 audit post-F46 PC controlled-demo investor route evidence refresh`',
    '## Investor Route Summary',
    'InvestorRoute=ready platform=Windows route=spawn>hangar-contact>damage-demo>solo-order>solo-return launch=scripts/unity/run_windows_demo.ps1 evidence=command-report+screenshots+sidecars',
    'DamageProof=damage-demo screenshot=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.png sidecar=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.json log=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.log callout=section-loss+cockpit-ejection+wreck-salvage+repair-line repairCost=9288',
    'LandscapePhoneProof=mobileLandscapeOnly=True orientation=landscape firstPhoneVersion=horizontal-only portraitSupport=False',
    'ProxyParsing=source=proxyIdentity+materialLanguage+propIdentity sidecarFallback=investorProxyVisuals splitSidecarRecapturePending=True publicSafe=proxy-only'
)

if ($null -ne $commandReport) {
    Assert-Equals -Actual ([string]$commandReport.result) -Expected "pass" -Label "command report result"
    Assert-Equals -Actual ([string]$commandReport.completedTask) -Expected "F46 refresh PC controlled-demo investor route evidence after polish fixes" -Label "command report completed task"
    Assert-Equals -Actual ([string]$commandReport.nextFormalTask) -Expected "F47 audit post-F46 PC controlled-demo investor route evidence refresh" -Label "command report next task"
    Assert-Equals -Actual ([int]$commandReport.width) -Expected 1280 -Label "command report width"
    Assert-Equals -Actual ([int]$commandReport.height) -Expected 720 -Label "command report height"
    Assert-Equals -Actual (@($commandReport.evidence).Count) -Expected 5 -Label "command report preset count"

    $damageRows = @($commandReport.evidence | Where-Object { [string]$_.preset -eq "damage-demo" })
    Assert-Equals -Actual $damageRows.Count -Expected 1 -Label "command report damage-demo row"
    if ($damageRows.Count -eq 1) {
        $damageRow = $damageRows[0]
        Assert-Equals -Actual ([string]$damageRow.screenshot) -Expected "analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.png" -Label "damage-demo screenshot link"
        Assert-Equals -Actual ([string]$damageRow.sidecar) -Expected "analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.json" -Label "damage-demo sidecar link"
        Assert-Equals -Actual ([string]$damageRow.log) -Expected "analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.log" -Label "damage-demo log link"
        Assert-Contains -Text ([string]$damageRow.playableFlowPolish) -Needle "mobileLandscapeOnly=True orientation=landscape" -Label "damage-demo landscape proof"
        Assert-Contains -Text ([string]$damageRow.debriefRewardSummary) -Needle "repairCost=9288" -Label "damage-demo repair cost"
    }
}

if ($null -ne $f45PolishReport) {
    Assert-Equals -Actual ([string]$f45PolishReport.result) -Expected "pass" -Label "F45 polish report result"
    Assert-Equals -Actual ([string]$f45PolishReport.nextFormalTask) -Expected "F46 refresh PC controlled-demo investor route evidence after polish fixes" -Label "F45 polish report next task"
}

foreach ($relativePath in @(
    "analysis-output\pc-controlled-demo-visual-evidence\captures\damage-demo.png",
    "analysis-output\pc-controlled-demo-visual-evidence\captures\damage-demo.json",
    "analysis-output\pc-controlled-demo-visual-evidence\captures\damage-demo.log"
)) {
    Assert-FileExists -RelativePath $relativePath -Label "damage-demo artifact"
}

Assert-All -Text $currentGate -Label "current gate F46 plan marker" -Needles @(
    "check_pc_controlled_demo_investor_route_evidence_refresh.ps1",
    "PC controlled-demo investor route evidence refresh plan OK."
)

Assert-All -Text $queueScript -Label "queue F46/F47 marker" -Needles @(
    "F46 refresh PC controlled-demo investor route evidence after polish fixes",
    "F47 audit post-F46 PC controlled-demo investor route evidence refresh"
)

Assert-All -Text $handoffScript -Label "handoff F46/F47 marker" -Needles @(
    "check_pc_controlled_demo_investor_route_evidence_refresh.ps1",
    "PC controlled-demo investor route evidence refresh check OK",
    "F47 audit post-F46 PC controlled-demo investor route evidence refresh"
)

Assert-Contains -Text $gitignore -Needle "analysis-output/pc-controlled-demo-investor-route-evidence-refresh/" -Label ".gitignore F46 output"

if ($failures.Count -gt 0) {
    Write-Host "PC controlled-demo investor route evidence refresh check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC controlled-demo investor route evidence refresh check(s) failed."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "PCControlledDemoInvestorRouteEvidenceRefresh"
    result = "pass"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F46 refresh PC controlled-demo investor route evidence after polish fixes"
    nextFormalTask = "F47 audit post-F46 PC controlled-demo investor route evidence refresh"
    noUnityLaunch = $true
    sourceCommandEvidenceReport = "analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json"
    refreshedAreas = @(
        "command evidence metadata advanced to F46/F47",
        "investor route summary present in refreshed markdown",
        "damage-demo screenshot, sidecar and log links present in refreshed markdown",
        "horizontal-only phone proof line present in refreshed markdown",
        "proxy parsing source/fallback marker present in refreshed markdown"
    )
    checks = $rows.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# PC Controlled Demo Investor Route Evidence Refresh")
$markdownLines.Add("")
$markdownLines.Add("Result: pass")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F46 refresh PC controlled-demo investor route evidence after polish fixes`')
$markdownLines.Add('Next formal task: `F47 audit post-F46 PC controlled-demo investor route evidence refresh`')
$markdownLines.Add("")
$markdownLines.Add("NoUnityLaunch: True")
$markdownLines.Add('Source command evidence: `analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json`')
$markdownLines.Add("")
$markdownLines.Add("## Refreshed Areas")
foreach ($area in $report.refreshedAreas) {
    $markdownLines.Add("- $area")
}
$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "PC controlled-demo investor route evidence refresh check OK."
Write-Host "Report: $reportJsonPath"

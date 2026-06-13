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
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fix-refresh"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
}

$repoFullPath = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([char[]]@("\", "/"))
$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
if (-not $OutputDir.StartsWith($repoFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDir must stay inside RepoRoot: $OutputDir"
}

$reportJsonPath = Join-Path $OutputDir "pc-controlled-demo-investor-route-evidence-audit-fix-refresh.json"
$reportMarkdownPath = Join-Path $OutputDir "pc-controlled-demo-investor-route-evidence-audit-fix-refresh.md"
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

if ($PlanOnly) {
    Write-Host "PC controlled-demo investor route evidence audit fix refresh plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "NoUnityLaunch: True"
    return
}

$commandCapture = Read-RepoText -RelativePath "scripts\unity\capture_pc_controlled_demo_command_evidence.ps1"
$commandMarkdown = Read-RepoText -RelativePath "analysis-output\pc-controlled-demo-command-evidence\pc-controlled-demo-command-evidence.md"
$commandReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-command-evidence\pc-controlled-demo-command-evidence.json"
$auditFixesReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fixes\pc-controlled-demo-investor-route-evidence-audit-fixes.json"
$currentGate = Read-RepoText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
$queueScript = Read-RepoText -RelativePath "scripts\unity\check_current_plan_queue.ps1"
$handoffScript = Read-RepoText -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"
$gitignore = Read-RepoText -RelativePath ".gitignore"

$closureMarkers = @(
    "RouteAuditFixRefresh=ready source=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fixes/pc-controlled-demo-investor-route-evidence-audit-fixes.json completed=F49 next=F50 noUnityLaunch=True mobile=landscape-only",
    "RouteAuditFixClosure=F48-doc-visibility status=closed surfaces=investor-route+playable-evidence+handoff",
    "RouteAuditFixClosure=gate-runtime status=route-gates-focused aggregateGate=not-required"
)

Assert-All -Text $commandCapture -Label "command evidence F49 source metadata" -Needles @(
    'F49 refresh PC controlled-demo investor route evidence after audit fixes',
    'F50 audit post-F49 PC controlled-demo investor route evidence refresh',
    'routeAuditFixClosure = $routeAuditFixClosure',
    'sourceRouteAuditFixesReport = Convert-ToRepoRelativePath -Path $routeAuditFixesReportPath',
    '## Route Audit Fix Closure'
)
Assert-All -Text $commandCapture -Label "command evidence F49 closure source" -Needles $closureMarkers

Assert-All -Text $commandMarkdown -Label "command evidence F49 refreshed markdown" -Needles @(
    'Completed task: `F49 refresh PC controlled-demo investor route evidence after audit fixes`',
    'Next formal task: `F50 audit post-F49 PC controlled-demo investor route evidence refresh`',
    'Source route audit fixes report: "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fixes/pc-controlled-demo-investor-route-evidence-audit-fixes.json"',
    '## Investor Route Summary',
    '## Route Audit Fix Closure',
    'InvestorRoute=ready platform=Windows route=spawn>hangar-contact>damage-demo>solo-order>solo-return launch=scripts/unity/run_windows_demo.ps1 evidence=command-report+screenshots+sidecars',
    'DamageProof=damage-demo screenshot=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.png sidecar=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.json log=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.log callout=section-loss+cockpit-ejection+wreck-salvage+repair-line repairCost=9288',
    'LandscapePhoneProof=mobileLandscapeOnly=True orientation=landscape firstPhoneVersion=horizontal-only portraitSupport=False',
    'ProxyParsing=source=proxyIdentity+materialLanguage+propIdentity sidecarFallback=investorProxyVisuals splitSidecarRecapturePending=True publicSafe=proxy-only'
)
Assert-All -Text $commandMarkdown -Label "command evidence F49 closure markdown" -Needles $closureMarkers

if ($null -ne $commandReport) {
    Assert-Equals -Actual ([string]$commandReport.result) -Expected "pass" -Label "command report result"
    Assert-Equals -Actual ([string]$commandReport.completedTask) -Expected "F49 refresh PC controlled-demo investor route evidence after audit fixes" -Label "command report completed task"
    Assert-Equals -Actual ([string]$commandReport.nextFormalTask) -Expected "F50 audit post-F49 PC controlled-demo investor route evidence refresh" -Label "command report next task"
    Assert-Equals -Actual ([string]$commandReport.sourceRouteAuditFixesReport) -Expected "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fixes/pc-controlled-demo-investor-route-evidence-audit-fixes.json" -Label "command report source route audit fixes"
    Assert-Equals -Actual ([int]$commandReport.width) -Expected 1280 -Label "command report width"
    Assert-Equals -Actual ([int]$commandReport.height) -Expected 720 -Label "command report height"
    Assert-Equals -Actual (@($commandReport.evidence).Count) -Expected 5 -Label "command report preset count"
    foreach ($marker in $closureMarkers) {
        $matches = @($commandReport.routeAuditFixClosure | Where-Object { [string]$_ -eq $marker })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "command report closure $marker"
    }
}

if ($null -ne $auditFixesReport) {
    Assert-Equals -Actual ([string]$auditFixesReport.result) -Expected "pass" -Label "F48 audit fixes report result"
    Assert-Equals -Actual ([string]$auditFixesReport.completedTask) -Expected "F48 implement post-F47 PC controlled-demo investor route evidence audit fixes" -Label "F48 audit fixes completed task"
    Assert-Equals -Actual ([string]$auditFixesReport.nextFormalTask) -Expected "F49 refresh PC controlled-demo investor route evidence after audit fixes" -Label "F48 audit fixes next task"
}

Assert-All -Text $currentGate -Label "current gate F49 plan marker" -Needles @(
    "check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh.ps1",
    "PC controlled-demo investor route evidence audit fix refresh plan OK."
)

Assert-All -Text $queueScript -Label "queue F49/F50 marker" -Needles @(
    "F49 refresh PC controlled-demo investor route evidence after audit fixes",
    "F50 audit post-F49 PC controlled-demo investor route evidence refresh",
    '| F49 | Done | `Refresh PC controlled-demo investor route evidence after audit fixes` |',
    '| F50 | Next | `Audit post-F49 PC controlled-demo investor route evidence refresh` |'
)

Assert-All -Text $handoffScript -Label "handoff script F49/F50 marker" -Needles @(
    "check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh.ps1",
    "PC controlled-demo investor route evidence audit fix refresh check OK",
    "F50 audit post-F49 PC controlled-demo investor route evidence refresh"
)

Assert-Contains -Text $gitignore -Needle "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh/" -Label ".gitignore F49 output"

if ($failures.Count -gt 0) {
    Write-Host "PC controlled-demo investor route evidence audit fix refresh check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC controlled-demo investor route evidence audit fix refresh check(s) failed."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "PCControlledDemoInvestorRouteEvidenceAuditFixRefresh"
    result = "pass"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F49 refresh PC controlled-demo investor route evidence after audit fixes"
    nextFormalTask = "F50 audit post-F49 PC controlled-demo investor route evidence refresh"
    noUnityLaunch = $true
    sourceCommandEvidenceReport = "analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json"
    sourceRouteAuditFixesReport = "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fixes/pc-controlled-demo-investor-route-evidence-audit-fixes.json"
    refreshedAreas = @(
        "command evidence metadata advanced to F49/F50",
        "route audit fix closure is present in command markdown",
        "route audit fix closure is present in command JSON",
        "investor route summary remains linked to five required presets",
        "mobile landscape-only proof remains visible",
        "route refresh stayed source/report-only without Unity launch"
    )
    checks = $rows.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# PC Controlled Demo Investor Route Evidence Audit Fix Refresh")
$markdownLines.Add("")
$markdownLines.Add("Result: pass")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F49 refresh PC controlled-demo investor route evidence after audit fixes`')
$markdownLines.Add('Next formal task: `F50 audit post-F49 PC controlled-demo investor route evidence refresh`')
$markdownLines.Add("")
$markdownLines.Add("NoUnityLaunch: True")
$markdownLines.Add('Source command evidence: `analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json`')
$markdownLines.Add('Source route audit fixes: `analysis-output/pc-controlled-demo-investor-route-evidence-audit-fixes/pc-controlled-demo-investor-route-evidence-audit-fixes.json`')
$markdownLines.Add("")
$markdownLines.Add("## Refreshed Areas")
foreach ($area in $report.refreshedAreas) {
    $markdownLines.Add("- $area")
}
$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "PC controlled-demo investor route evidence audit fix refresh check OK."
Write-Host "Report: $reportJsonPath"

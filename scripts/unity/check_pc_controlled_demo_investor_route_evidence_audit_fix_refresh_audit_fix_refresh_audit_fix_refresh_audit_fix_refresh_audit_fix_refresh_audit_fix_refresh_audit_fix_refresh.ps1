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
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
}

$repoFullPath = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([char[]]@("\", "/"))
$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
if (-not $OutputDir.StartsWith($repoFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDir must stay inside RepoRoot: $OutputDir"
}

$reportJsonPath = Join-Path $OutputDir "report.json"
$reportMarkdownPath = Join-Path $OutputDir "report.md"
$failures = New-Object System.Collections.Generic.List[string]
$rows = New-Object System.Collections.Generic.List[object]
$requiredPresets = @("spawn", "hangar-contact", "damage-demo", "solo-order", "solo-return")

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
        return
    }

    [void]$rows.Add([pscustomobject]@{
        check = $Label
        status = "OK"
        detail = $RelativePath
    })
}

if ($PlanOnly) {
    Write-Host "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "NoUnityLaunch: True"
    return
}

$commandCapture = Read-RepoText -RelativePath "scripts\unity\capture_pc_controlled_demo_command_evidence.ps1"
$commandMarkdown = Read-RepoText -RelativePath "analysis-output\pc-controlled-demo-command-evidence\pc-controlled-demo-command-evidence.md"
$commandReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-command-evidence\pc-controlled-demo-command-evidence.json"
$f62AuditReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit\report.json"
$f63FixReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes\report.json"
$currentGate = Read-RepoText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
$queueScript = Read-RepoText -RelativePath "scripts\unity\check_current_plan_queue.ps1"
$handoffScript = Read-RepoText -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"
$gitignore = Read-RepoText -RelativePath ".gitignore"

$closureMarkers = @(
    "RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditRefresh=ready source=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json completed=F67 next=F68 noUnityLaunch=True mobile=landscape-only",
    "RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure=F66-doc-gate-visibility status=closed surfaces=plan+evidence+handoff+gate",
    "RouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure=next-refresh-contract status=closed sourceAudit=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json sourceFixes=analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json"
)

Assert-All -Text $commandCapture -Label "command capture F67/F68 metadata" -Needles @(
    'F67 refresh PC controlled-demo investor route evidence after F65 audit fixes',
    'F68 audit post-F67 PC controlled-demo investor route evidence refresh',
    'sourceRouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditReport = Convert-ToRepoRelativePath -Path $routeAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditReportPath',
    'sourceRouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixesReport = Convert-ToRepoRelativePath -Path $routeAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixesReportPath',
    'routeAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure = $routeAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure',
    '## Route Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Closure'
)
Assert-All -Text $commandCapture -Label "command capture F67 closure source" -Needles $closureMarkers

Assert-All -Text $commandMarkdown -Label "command markdown F67 route evidence" -Needles @(
    'Completed task: `F67 refresh PC controlled-demo investor route evidence after F65 audit fixes`',
    'Next formal task: `F68 audit post-F67 PC controlled-demo investor route evidence refresh`',
    'Source route audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit report: "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json"',
    'Source route audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes report: "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json"',
    '## Investor Route Summary',
    '## Route Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Closure',
    'InvestorRoute=ready platform=Windows route=spawn>hangar-contact>damage-demo>solo-order>solo-return launch=scripts/unity/run_windows_demo.ps1 evidence=command-report+screenshots+sidecars',
    'DamageProof=damage-demo screenshot=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.png sidecar=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.json log=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.log callout=section-loss+cockpit-ejection+wreck-salvage+repair-line repairCost=9288',
    'LandscapePhoneProof=mobileLandscapeOnly=True orientation=landscape firstPhoneVersion=horizontal-only portraitSupport=False',
    'ProxyParsing=source=proxyIdentity+materialLanguage+propIdentity sidecarFallback=investorProxyVisuals splitSidecarRecapturePending=True publicSafe=proxy-only'
)
Assert-All -Text $commandMarkdown -Label "command markdown F67 closure markers" -Needles $closureMarkers

if ($null -ne $commandReport) {
    Assert-Equals -Actual ([string]$commandReport.result) -Expected "pass" -Label "command report result"
    Assert-Equals -Actual ([string]$commandReport.completedTask) -Expected "F67 refresh PC controlled-demo investor route evidence after F65 audit fixes" -Label "command report completed task"
    Assert-Equals -Actual ([string]$commandReport.nextFormalTask) -Expected "F68 audit post-F67 PC controlled-demo investor route evidence refresh" -Label "command report next task"
    Assert-Equals -Actual ([string]$commandReport.sourceRouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditReport) -Expected "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json" -Label "command report source F65 audit"
    Assert-Equals -Actual ([string]$commandReport.sourceRouteAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixesReport) -Expected "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json" -Label "command report source F66 fixes"
    Assert-Equals -Actual ([int]$commandReport.width) -Expected 1280 -Label "command report width"
    Assert-Equals -Actual ([int]$commandReport.height) -Expected 720 -Label "command report height"
    Assert-Equals -Actual (@($commandReport.evidence).Count) -Expected 5 -Label "command report preset count"

    foreach ($preset in $requiredPresets) {
        $matches = @($commandReport.evidence | Where-Object { [string]$_.preset -eq $preset })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "command report preset $preset"
        if ($matches.Count -eq 1) {
            Assert-FileExists -RelativePath ([string]$matches[0].screenshot) -Label "$preset screenshot"
            Assert-FileExists -RelativePath ([string]$matches[0].sidecar) -Label "$preset sidecar"
            Assert-FileExists -RelativePath ([string]$matches[0].log) -Label "$preset log"
        }
    }

    foreach ($marker in $closureMarkers) {
        $matches = @($commandReport.routeAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditClosure | Where-Object { [string]$_ -eq $marker })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "command report F67 closure $marker"
    }
}

if ($null -ne $f62AuditReport) {
    Assert-Equals -Actual ([string]$f62AuditReport.result) -Expected "pass-with-followups" -Label "F65 audit result"
    Assert-Equals -Actual ([string]$f62AuditReport.completedTask) -Expected "F65 audit post-F64 PC controlled-demo investor route evidence refresh" -Label "F65 audit completed task"
    Assert-Equals -Actual ([string]$f62AuditReport.nextFormalTask) -Expected "F66 implement post-F65 PC controlled-demo investor route evidence refresh audit fixes" -Label "F65 audit next task"
}

if ($null -ne $f63FixReport) {
    Assert-Equals -Actual ([string]$f63FixReport.result) -Expected "pass" -Label "F66 fixes result"
    Assert-Equals -Actual ([string]$f63FixReport.completedTask) -Expected "F66 implement post-F65 PC controlled-demo investor route evidence refresh audit fixes" -Label "F66 fixes completed task"
    Assert-Equals -Actual ([string]$f63FixReport.nextFormalTask) -Expected "F67 refresh PC controlled-demo investor route evidence after F65 audit fixes" -Label "F66 fixes next task"
}

Assert-All -Text $currentGate -Label "current gate F67 plan marker" -Needles @(
    "check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1",
    "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh plan OK."
)

Assert-All -Text $queueScript -Label "queue F67/F68 marker" -Needles @(
    "F67 refresh PC controlled-demo investor route evidence after F65 audit fixes",
    "F68 audit post-F67 PC controlled-demo investor route evidence refresh",
    '| F67 | Done | `Refresh PC controlled-demo investor route evidence after F65 audit fixes` |',
    '| F68 | Next | `Audit post-F67 PC controlled-demo investor route evidence refresh` |'
)

Assert-All -Text $handoffScript -Label "handoff script F67/F68 marker" -Needles @(
    "check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1",
    "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh check OK",
    "F68 audit post-F67 PC controlled-demo investor route evidence refresh"
)

Assert-Contains -Text $gitignore -Needle "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh/" -Label ".gitignore F67 output"

if ($failures.Count -gt 0) {
    Write-Host "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh check(s) failed."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "PCControlledDemoInvestorRouteEvidenceAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefreshAuditFixRefresh"
    result = "pass"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F67 refresh PC controlled-demo investor route evidence after F65 audit fixes"
    nextFormalTask = "F68 audit post-F67 PC controlled-demo investor route evidence refresh"
    noUnityLaunch = $true
    sourceCommandEvidenceReport = "analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json"
    sourceAuditReport = "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json"
    sourceFixesReport = "analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json"
    refreshedAreas = @(
        "command evidence metadata advanced to F67/F68",
        "F65 audit report is an explicit command evidence source",
        "F66 fix report is an explicit command evidence source",
        "F66 closure is present in command markdown",
        "F66 closure is present in command JSON",
        "mobile landscape-only proof remains visible",
        "route refresh stayed source/report-only without Unity launch"
    )
    checks = $rows.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# PC Controlled Demo Investor Route Evidence Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh Audit Fix Refresh")
$markdownLines.Add("")
$markdownLines.Add("Result: pass")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F67 refresh PC controlled-demo investor route evidence after F65 audit fixes`')
$markdownLines.Add('Next formal task: `F68 audit post-F67 PC controlled-demo investor route evidence refresh`')
$markdownLines.Add("")
$markdownLines.Add("NoUnityLaunch: True")
$markdownLines.Add("")
$markdownLines.Add('Source command evidence: `analysis-output/pc-controlled-demo-command-evidence/pc-controlled-demo-command-evidence.json`')
$markdownLines.Add('Source F65 audit: `analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit/report.json`')
$markdownLines.Add('Source F66 fixes: `analysis-output/pc-controlled-demo-investor-route-evidence-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fix-refresh-audit-fixes/report.json`')
$markdownLines.Add("")
$markdownLines.Add("## Refreshed Areas")
foreach ($area in $report.refreshedAreas) {
    $markdownLines.Add("- $area")
}
$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh check OK."
Write-Host "Report: $reportJsonPath"

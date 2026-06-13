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
    $OutputDir = Join-Path $RepoRoot "analysis-output\pc-controlled-demo-investor-evidence-polish-fixes"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot $OutputDir
}

$repoFullPath = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([char[]]@("\", "/"))
$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
if (-not $OutputDir.StartsWith($repoFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDir must stay inside RepoRoot: $OutputDir"
}

$reportJsonPath = Join-Path $OutputDir "pc-controlled-demo-investor-evidence-polish-fixes.json"
$reportMarkdownPath = Join-Path $OutputDir "pc-controlled-demo-investor-evidence-polish-fixes.md"
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
    Write-Host "PC controlled-demo investor evidence polish fixes plan OK."
    Write-Host "Repo: $RepoRoot"
    Write-Host "OutputDir: $OutputDir"
    Write-Host "NoUnityLaunch: True"
    return
}

$commandCapture = Read-RepoText -RelativePath "scripts\unity\capture_pc_controlled_demo_command_evidence.ps1"
$playableEvidence = Read-RepoText -RelativePath "docs-playable-demo-investor-evidence-2026-06-07.md"
$routeDoc = Read-RepoText -RelativePath "docs-pc-investor-demo-route-2026-06-13.md"
$bootstrap = Read-RepoText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs"
$mobileLandscape = Read-RepoText -RelativePath "scripts\unity\check_mobile_landscape_contract.ps1"
$f44AuditReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-investor-evidence-refresh-audit\pc-controlled-demo-investor-evidence-refresh-audit.json"
$commandReport = Read-RepoJson -RelativePath "analysis-output\pc-controlled-demo-command-evidence\pc-controlled-demo-command-evidence.json"
$currentGate = Read-RepoText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
$queueScript = Read-RepoText -RelativePath "scripts\unity\check_current_plan_queue.ps1"
$handoffScript = Read-RepoText -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"
$gitignore = Read-RepoText -RelativePath ".gitignore"

$routeSummarySourceMarkers = @(
    "## Investor Route Summary",
    "InvestorRoute=ready platform=Windows route=spawn>hangar-contact>damage-demo>solo-order>solo-return",
    'DamageProof=damage-demo screenshot=$damageScreenshot sidecar=$damageSidecar log=$damageLog',
    "LandscapePhoneProof=mobileLandscapeOnly=True orientation=landscape firstPhoneVersion=horizontal-only portraitSupport=False",
    "ProxyParsing=source=proxyIdentity+materialLanguage+propIdentity sidecarFallback=investorProxyVisuals splitSidecarRecapturePending=True publicSafe=proxy-only"
)

$routeSummaryDocMarkers = @(
    "## Investor Route Summary",
    "InvestorRoute=ready platform=Windows route=spawn>hangar-contact>damage-demo>solo-order>solo-return",
    "DamageProof=damage-demo screenshot=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.png sidecar=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.json log=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.log",
    "LandscapePhoneProof=mobileLandscapeOnly=True orientation=landscape firstPhoneVersion=horizontal-only portraitSupport=False",
    "ProxyParsing=source=proxyIdentity+materialLanguage+propIdentity sidecarFallback=investorProxyVisuals splitSidecarRecapturePending=True publicSafe=proxy-only"
)

Assert-All -Text $commandCapture -Label "command evidence F45 route summary source" -Needles $routeSummarySourceMarkers
Assert-All -Text $playableEvidence -Label "playable investor evidence F45 route summary" -Needles $routeSummaryDocMarkers
Assert-All -Text $routeDoc -Label "PC investor route F45 compact route summary" -Needles @(
    "InvestorRoute=ready platform=Windows route=spawn>hangar-contact>damage-demo>solo-order>solo-return",
    "DamageProof=damage-demo screenshot=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.png sidecar=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.json log=analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.log",
    "LandscapePhoneProof=mobileLandscapeOnly=True orientation=landscape firstPhoneVersion=horizontal-only portraitSupport=False",
    "ProxyParsing=source=proxyIdentity+materialLanguage+propIdentity sidecarFallback=investorProxyVisuals splitSidecarRecapturePending=True publicSafe=proxy-only"
)

Assert-All -Text $bootstrap -Label "bootstrap proxy language source" -Needles @(
    "proxyIdentity=role-silhouette+faction-color+scale-language",
    "materialLanguage=painted-panels+emissive-cockpit+weapon-hardpoints",
    "propIdentity=canopy+roof+hazard-stripe",
    "investorProxyVisuals = BuildCaptureInvestorProxyVisualSummary()"
)

Assert-All -Text $mobileLandscape -Label "mobile landscape-only proof contract" -Needles @(
    "defaultScreenOrientation: 3",
    "MobileTouchUi=ready orientation=landscape",
    "landscapeOnly=yes",
    "Portrait layout is not a supported first-version target",
    "Mobile landscape contract check OK."
)

if ($null -ne $f44AuditReport) {
    Assert-Equals -Actual ([string]$f44AuditReport.result) -Expected "pass-with-followups" -Label "F44 audit report result"
    Assert-Equals -Actual ([string]$f44AuditReport.nextFormalTask) -Expected "F45 implement post-F44 PC controlled-demo investor evidence polish fixes" -Label "F44 audit next task"
    foreach ($area in @("raw-sidecar-proxy-language", "route-summary-compactness", "damage-proof-link", "mobile-landscape-proof-line")) {
        $matches = @($f44AuditReport.followUps | Where-Object { [string]$_.area -eq $area })
        Assert-Equals -Actual $matches.Count -Expected 1 -Label "F44 follow-up $area"
    }
}

if ($null -ne $commandReport) {
    $damageRows = @($commandReport.evidence | Where-Object { [string]$_.preset -eq "damage-demo" })
    Assert-Equals -Actual $damageRows.Count -Expected 1 -Label "command report damage-demo row"
    if ($damageRows.Count -eq 1) {
        $damageRow = $damageRows[0]
        Assert-Equals -Actual ([string]$damageRow.screenshot) -Expected "analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.png" -Label "damage-demo screenshot link"
        Assert-Equals -Actual ([string]$damageRow.sidecar) -Expected "analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.json" -Label "damage-demo sidecar link"
        Assert-Equals -Actual ([string]$damageRow.log) -Expected "analysis-output/pc-controlled-demo-visual-evidence/captures/damage-demo.log" -Label "damage-demo log link"
        Assert-Contains -Text ([string]$damageRow.playableFlowPolish) -Needle "mobileLandscapeOnly=True orientation=landscape" -Label "damage-demo landscape proof"
        Assert-Contains -Text ([string]$damageRow.investorProxyVisuals) -Needle "publicSafe=proxy-only" -Label "damage-demo public-safe proxy"
    }
}

foreach ($relativePath in @(
    "analysis-output\pc-controlled-demo-visual-evidence\captures\damage-demo.png",
    "analysis-output\pc-controlled-demo-visual-evidence\captures\damage-demo.json",
    "analysis-output\pc-controlled-demo-visual-evidence\captures\damage-demo.log"
)) {
    Assert-FileExists -RelativePath $relativePath -Label "damage-demo artifact"
}

Assert-All -Text $currentGate -Label "current gate F45 plan marker" -Needles @(
    "check_pc_controlled_demo_investor_evidence_polish_fixes.ps1",
    "PC controlled-demo investor evidence polish fixes plan OK."
)

Assert-All -Text $queueScript -Label "queue F45/F46 marker" -Needles @(
    "F45 implement post-F44 PC controlled-demo investor evidence polish fixes",
    "F46 refresh PC controlled-demo investor route evidence after polish fixes"
)

Assert-All -Text $handoffScript -Label "handoff F45/F46 marker" -Needles @(
    "check_pc_controlled_demo_investor_evidence_polish_fixes.ps1",
    "PC controlled-demo investor evidence polish fixes check OK",
    "F46 refresh PC controlled-demo investor route evidence after polish fixes"
)

Assert-Contains -Text $gitignore -Needle "analysis-output/pc-controlled-demo-investor-evidence-polish-fixes/" -Label ".gitignore F45 output"

if ($failures.Count -gt 0) {
    Write-Host "PC controlled-demo investor evidence polish fixes check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) PC controlled-demo investor evidence polish fixes check(s) failed."
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$report = [pscustomobject]@{
    schema = "PCControlledDemoInvestorEvidencePolishFixes"
    result = "pass"
    timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
    completedTask = "F45 implement post-F44 PC controlled-demo investor evidence polish fixes"
    nextFormalTask = "F46 refresh PC controlled-demo investor route evidence after polish fixes"
    noUnityLaunch = $true
    fixedAreas = @(
        "compact investor route summary added to evidence docs and command-evidence template",
        "damage-demo screenshot, sidecar and log references made explicit",
        "mobile landscape-only proof line is visible in investor-facing route summary",
        "proxy identity/material/prop parsing source markers are explicit while split sidecar recapture remains deferred",
        "first phone version remains horizontal-only; portrait is excluded from the first slice"
    )
    checks = $rows.ToArray()
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.Generic.List[string]
$markdownLines.Add("# PC Controlled Demo Investor Evidence Polish Fixes")
$markdownLines.Add("")
$markdownLines.Add("Result: pass")
$markdownLines.Add("")
$markdownLines.Add('Completed task: `F45 implement post-F44 PC controlled-demo investor evidence polish fixes`')
$markdownLines.Add('Next formal task: `F46 refresh PC controlled-demo investor route evidence after polish fixes`')
$markdownLines.Add("")
$markdownLines.Add("NoUnityLaunch: True")
$markdownLines.Add("")
$markdownLines.Add("## Fixed Areas")
foreach ($area in $report.fixedAreas) {
    $markdownLines.Add("- $area")
}
$markdownLines | Set-Content -LiteralPath $reportMarkdownPath -Encoding UTF8

Write-Host "PC controlled-demo investor evidence polish fixes check OK."
Write-Host "Report: $reportJsonPath"

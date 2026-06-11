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
    return Join-Path $RepoRoot $RelativePath
}

function Read-RequiredText {
    param([string]$RelativePath)

    $path = Resolve-RepoPath -RelativePath $RelativePath
    if (-not (Test-Path -LiteralPath $path)) {
        Add-Failure "Missing file: $RelativePath"
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

    if ([string]::IsNullOrWhiteSpace($Text) -or $Text.IndexOf($Needle, [StringComparison]::Ordinal) -lt 0) {
        Add-Failure "$Label missing marker: $Needle"
    }
}

function Assert-NotContains {
    param(
        [string]$Text,
        [string]$Needle,
        [string]$Label
    )

    if (-not [string]::IsNullOrWhiteSpace($Text) -and $Text.IndexOf($Needle, [StringComparison]::Ordinal) -ge 0) {
        Add-Failure "$Label contains forbidden marker: $Needle"
    }
}

function Get-TextSlice {
    param(
        [string]$Text,
        [string]$StartMarker,
        [string]$EndMarker,
        [string]$Label
    )

    $start = $Text.IndexOf($StartMarker, [StringComparison]::Ordinal)
    if ($start -lt 0) {
        Add-Failure "$Label missing start marker: $StartMarker"
        return ""
    }

    $end = $Text.IndexOf($EndMarker, $start + $StartMarker.Length, [StringComparison]::Ordinal)
    if ($end -lt 0) {
        Add-Failure "$Label missing end marker: $EndMarker"
        return $Text.Substring($start)
    }

    return $Text.Substring($start, $end - $start)
}

function Assert-MethodAvoidsModelCall {
    param(
        [string]$Text,
        [string]$MethodMarker,
        [string]$Label
    )

    if ($Text.IndexOf($MethodMarker, [StringComparison]::Ordinal) -lt 0) {
        Add-Row -Check $Label -Detail "method absent"
        return
    }

    $slice = Get-TextSlice -Text $Text -StartMarker $MethodMarker -EndMarker "`n        private " -Label $Label
    foreach ($forbidden in @(
        "new MiniMaxCommander",
        "ChooseDirective(",
        "ConfigFromEnvironment()",
        "RunStartupMiniMaxCommander"
    )) {
        Assert-NotContains -Text $slice -Needle $forbidden -Label $Label
    }
}

$miniMax = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\BattleCore\MiniMaxCommander.cs"
$bootstrap = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\Presentation\Mc2DemoBootstrap.cs"
$ruleCommander = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\Scripts\BattleCore\RuleCommander.cs"
$visibleFlowScript = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt"
$demoScript = Read-RequiredText -RelativePath "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-demo.txt"

foreach ($marker in @(
    "You make slow, high-level decisions only; local battle code handles movement, target choice, firing, heat, and avoidance.",
    "Return exactly one directive token and nothing else.",
    "Valid directive tokens: assault-objective, engage-hostiles, regroup, withdraw-if-critical, hold.",
    "Never return coordinates, unit ids, markdown, JSON, commentary, or examples.",
    "Choose one directive token for the next 10-30 seconds.",
    "max_completion_tokens = 32",
    "stream = false",
    "MINIMAX_API_KEY",
    "NormalizeApiKey",
    "DescribeWithoutSecrets"
)) {
    Assert-Contains -Text $miniMax -Needle $marker -Label "MiniMax commander high-level contract"
}
Add-Row -Check "MiniMax high-level request contract" -Detail "10 marker(s)"

foreach ($marker in @(
    "DirectiveAssaultObjective",
    "DirectiveEngageHostiles",
    "DirectiveRegroup",
    "DirectiveWithdrawIfCritical",
    "DirectiveHold",
    "ChooseCommandForDirective"
)) {
    Assert-Contains -Text $ruleCommander -Needle $marker -Label "RuleCommander directive bridge"
}
Add-Row -Check "RuleCommander directive bridge" -Detail "6 marker(s)"

foreach ($marker in @(
    "private const float MiniMaxCommanderAdvanceSeconds = 8f;",
    'case "-mc2MinimaxCommanderSteps":',
    "Mathf.Clamp(requestedSteps, 0, 8)",
    "MC2 MiniMax commander skipped: steps=0.",
    "MiniMax commander unavailable: set MINIMAX_API_KEY to use the model; using rule fallback.",
    "MiniMaxCommander commander = config.IsConfigured ? new MiniMaxCommander(config) : null;",
    "MiniMax unavailable; rule fallback active.",
    "RuleCommander.DirectiveAssaultObjective",
    "fallbackCommander.ChooseCommandForDirective(observation, directive)",
    "AdvanceStartupSimulation(MiniMaxCommanderAdvanceSeconds)",
    "ReportStartupCommanderState();"
)) {
    Assert-Contains -Text $bootstrap -Needle $marker -Label "startup MiniMax commander boundary"
}
Add-Row -Check "startup MiniMax boundary" -Detail "11 marker(s)"

Assert-MethodAvoidsModelCall -Text $bootstrap -MethodMarker "private void Update()" -Label "Update loop"
Assert-MethodAvoidsModelCall -Text $bootstrap -MethodMarker "private void FixedUpdate()" -Label "FixedUpdate loop"
Assert-MethodAvoidsModelCall -Text $bootstrap -MethodMarker "private void LateUpdate()" -Label "LateUpdate loop"
Add-Row -Check "frame loop model-call guard" -Detail "Update/FixedUpdate/LateUpdate"

Assert-Contains -Text $visibleFlowScript -Needle "assert-ai-deputy-window" -Label "visible-flow AI deputy smoke"
Assert-NotContains -Text $visibleFlowScript -Needle "-mc2MinimaxCommanderSteps" -Label "visible-flow AI deputy smoke"
Assert-NotContains -Text $demoScript -Needle "-mc2MinimaxCommanderSteps" -Label "demo command script"
Add-Row -Check "smoke token guard" -Detail "assert window without MiniMax steps"

if ($failures.Count -gt 0) {
    Write-Host "AI deputy contract check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) AI deputy contract check(s) failed."
}

Write-Host "AI deputy contract check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize

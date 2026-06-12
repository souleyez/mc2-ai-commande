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
    return Join-Path $RepoRoot ($RelativePath -replace "/", "\")
}

function Read-RequiredText {
    param([string]$RelativePath)

    $path = Resolve-RepoPath -RelativePath $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Add-Failure "$RelativePath missing"
        return ""
    }

    Add-Row -Check $RelativePath -Detail "exists"
    return Get-Content -LiteralPath $path -Raw
}

function Require-Text {
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

$contract = Read-RequiredText -RelativePath "docs-map-authoring-contract-2026-06-07.md"
$platformPlan = Read-RequiredText -RelativePath "docs-platform-ecosystem-plan.md"
$rewardContract = Read-RequiredText -RelativePath "docs-platform-reward-contract-2026-06-07.md"
$detailedPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"

$stableMarkers = @(
    "MapAuthoringContract: True",
    "OpenMapsCertifiedRewards: True",
    "PortableRewardsMainServerOnly: True",
    "MapPackageGrantsNoPortableRewards: True",
    "RewardTableReferenceOnly: True",
    "MapValidatorRequired: True",
    "FirstEditorTarget: Local package authoring before server upload"
)

foreach ($marker in $stableMarkers) {
    Require-Text -Text $contract -Needle $marker -Label "map authoring contract"
}

$requiredFields = @(
    "mapId",
    "version",
    "title",
    "author",
    "license",
    "provenance",
    "contentHash",
    "battleCoreVersion",
    "terrain",
    "navigation",
    "structures",
    "objectives",
    "triggerGraph",
    "enemyWaves",
    "allowedSquadSize",
    "difficultyEstimate",
    "rewardTableId",
    "certificationState"
)

foreach ($field in $requiredFields) {
    Require-Text -Text $contract -Needle $field -Label "required package field"
}

$rejectedRewardFields = @(
    "directRewardGrant",
    "tokenGrant",
    "fragmentGrant",
    "weaponGrant",
    "skinGrant",
    "inventoryMutation",
    "ledgerMutation",
    "realMoneyPayout",
    "chainTransfer"
)

foreach ($field in $rejectedRewardFields) {
    Require-Text -Text $contract -Needle $field -Label "rejected reward field"
}

$triggerMarkers = @(
    "OnObjectiveComplete",
    "OnAreaEntered",
    "OnUnitDestroyed",
    "OnStructureDamaged",
    "OnTimerElapsed",
    "ActivateWave",
    "RevealObjective",
    "CompleteObjective"
)

foreach ($marker in $triggerMarkers) {
    Require-Text -Text $contract -Needle $marker -Label "trigger graph contract"
}

$certificationMarkers = @(
    "Draft",
    "UncertifiedPublic",
    "Certified",
    "Event",
    "Retired"
)

foreach ($marker in $certificationMarkers) {
    Require-Text -Text $contract -Needle $marker -Label "certification state"
}

Require-Text -Text $platformPlan -Needle "docs-map-authoring-contract-2026-06-07.md" -Label "platform plan link"
Require-Text -Text $platformPlan -Needle "Maps can be open; rewards must be certified." -Label "platform reward boundary"
Require-Text -Text $rewardContract -Needle "Map packages reference reward table ids; they do not define direct token" -Label "reward authority boundary"
Require-Text -Text $detailedPlan -Needle "| F2 | Done | ``Document map authoring contract`` |" -Label "detailed plan F2 status"
Require-Text -Text $detailedPlan -Needle "| F3 | Done | ``Document web ranking contract`` |" -Label "detailed plan F3 status"
Require-Text -Text $detailedPlan -Needle "| F4 | Done | ``Document creator economy boundary`` |" -Label "detailed plan F4 status"
Require-Text -Text $detailedPlan -Needle "| F5 | Done | ``Document server implementation boundary`` |" -Label "detailed plan F5 status"
Require-Text -Text $detailedPlan -Needle "| F6 | Done | ``Scaffold local main-server prototype`` |" -Label "detailed plan F6 status"
Require-Text -Text $detailedPlan -Needle "| F7 | Done | ``Document Unity main-server integration contract`` |" -Label "detailed plan F7 status"
Require-Text -Text $detailedPlan -Needle "| F8 | Done | ``Implement optional Unity main-server client adapter`` |" -Label "detailed plan F8"
Require-Text -Text $detailedPlan -Needle "| F9 | Next | ``Wire optional Unity main-server adapter into launch/debrief smoke`` |" -Label "detailed plan next task"

if ($failures.Count -gt 0) {
    Write-Host "Map authoring contract check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) map authoring contract check(s) failed."
}

Write-Host "Map authoring contract check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize

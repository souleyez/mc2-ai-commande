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

    return Get-Content -LiteralPath $path -Raw
}

function Assert-Contains {
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

function Assert-DoesNotContain {
    param(
        [string]$Text,
        [string]$Needle,
        [string]$Label
    )

    if (-not [string]::IsNullOrWhiteSpace($Text) -and $Text.Contains($Needle)) {
        Add-Failure "$Label still contains stale marker: $Needle"
        return
    }

    Add-Row -Check "$Label stale marker" -Detail $Needle
}

$requiredPlanMarkers = @(
    "PC1-PC62",
    "Add Android ADB driver package probe",
    "WpdOnlyAndroidProbe: True",
    "AdbSetupHint: True",
    "AdbDriverPackageProbe: True",
    "AdbWatchHint: True",
    "G3DeviceStatusReport: True",
    "G3WhenReady: True",
    "CommandFileSmoke: True",
    "SmokeSuccessMarker: MC2 debrief summary assertion OK",
    "SmokeSuccessMarker: MC2 loadout compact assertion OK",
    "Pass Android G3 device smoke",
    "G4 Touch UI pass",
    "F6 local main-server prototype",
    "F7 document Unity main-server integration contract",
    "F8 implement optional Unity main-server client adapter",
    "F9 wire optional Unity main-server adapter into launch/debrief smoke",
    "F10 wire optional Unity inventory bootstrap smoke",
    "F11 plan inventory-to-MechBay binding boundary",
    "F12 implement opt-in inventory-to-MechBay preview binding",
    "F13 capture opt-in MechBay preview evidence",
    "F14 capture landscape-phone MechLab source-line evidence",
    "F15 plan server-backed receipt slice",
    "F16 implement server-backed receipt evidence gate",
    "F17 plan post-receipt inventory refresh boundary",
    "F18 implement opt-in post-receipt inventory refresh binding",
    "F19 capture opt-in post-receipt refresh evidence",
    "F20 refresh Android landscape build/smoke evidence",
    "F21 audit landscape touch UI ergonomics",
    "F22 audit landscape MechLab touch controls",
    "F23 capture landscape MechLab touch evidence",
    "F24 capture Android MechLab touch evidence",
    "F25 capture Android battle command touch evidence",
    "F26 reduce Android combat effect log noise",
    "F27 audit Android entity placeholder collision path",
    "F28 capture Android entity placeholder collision runtime evidence",
    "F29 audit PC controlled-demo visual readability",
    "F30 implement PC controlled-demo visual readability fixes",
    "F31 refresh PC controlled-demo visual evidence after readability fixes",
    "F32 audit PC controlled-demo command readability and formation feel",
    "F33 implement PC controlled-demo command readability and formation fixes",
    "F34 refresh PC controlled-demo command evidence after readability fixes",
    "F35 audit post-F34 PC controlled-demo playable flow polish",
    "F36 implement post-F34 PC controlled-demo playable flow polish fixes",
    "F37 refresh PC controlled-demo playable-flow evidence after polish fixes",
    "F38 audit post-F37 PC controlled-demo investor readiness",
    "F39 implement post-F37 PC controlled-demo investor readiness fixes",
    "F40 refresh PC controlled-demo investor-readiness evidence after fixes",
    "F41 audit post-F40 PC controlled-demo investor evidence package",
    "F42 implement post-F41 PC controlled-demo investor evidence package fixes",
    "F43 refresh PC controlled-demo investor evidence package after fixes",
    "F44 audit post-F43 PC controlled-demo investor evidence refresh",
    "F45 implement post-F44 PC controlled-demo investor evidence polish fixes",
    "F46 refresh PC controlled-demo investor route evidence after polish fixes",
    "F47 audit post-F46 PC controlled-demo investor route evidence refresh",
    "F48 implement post-F47 PC controlled-demo investor route evidence audit fixes",
    "F49 refresh PC controlled-demo investor route evidence after audit fixes",
    "F50 audit post-F49 PC controlled-demo investor route evidence refresh",
    "F51 implement post-F50 PC controlled-demo investor route evidence refresh audit fixes",
    "F52 refresh PC controlled-demo investor route evidence after F50 audit fixes",
    "F53 audit post-F52 PC controlled-demo investor route evidence refresh",
    "F54 implement post-F53 PC controlled-demo investor route evidence refresh audit fixes",
    "F55 refresh PC controlled-demo investor route evidence after F53 audit fixes",
    "F56 audit post-F55 PC controlled-demo investor route evidence refresh",
    "F57 implement post-F56 PC controlled-demo investor route evidence refresh audit fixes",
    "F58 refresh PC controlled-demo investor route evidence after F56 audit fixes",
    "F59 audit post-F58 PC controlled-demo investor route evidence refresh",
    "F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes",
    "F61 refresh PC controlled-demo investor route evidence after F59 audit fixes",
    "F62 audit post-F61 PC controlled-demo investor route evidence refresh"
)

$docsToCheck = @(
    "README.md",
    "BUILD-WIN.md",
    "docs-ai-rts-commander-current-master-plan-2026-06-07.md",
    "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md",
    "docs-pc-optimization-plan-2026-06-11.md",
    "docs-mobile-first-plan-2026-06-10.md",
    "docs-machine-handoff-plan-2026-06-07.md",
    "docs-playable-demo-investor-evidence-2026-06-07.md"
)

foreach ($relativePath in $docsToCheck) {
    $text = Read-RequiredText -RelativePath $relativePath
    foreach ($marker in $requiredPlanMarkers) {
        Assert-Contains -Text $text -Needle $marker -Label "$relativePath plan queue marker"
    }

    Assert-DoesNotContain -Text $text -Needle "define PC55 before implementation" -Label $relativePath
    Assert-DoesNotContain -Text $text -Needle "必须先定义 PC55" -Label $relativePath
    Assert-DoesNotContain -Text $text -Needle "必须先写清 PC55" -Label $relativePath
    Assert-DoesNotContain -Text $text -Needle 'the next formal task is `F31 refresh PC controlled-demo visual evidence after readability fixes`' -Label $relativePath
    Assert-DoesNotContain -Text $text -Needle 'Formal next task: `F31 refresh PC controlled-demo visual evidence after readability fixes`' -Label $relativePath
    Assert-DoesNotContain -Text $text -Needle 'Formal next task: `F32 audit PC controlled-demo command readability and formation feel`' -Label $relativePath
    Assert-DoesNotContain -Text $text -Needle 'Formal next work is `F31 refresh PC controlled-demo visual evidence after readability fixes`' -Label $relativePath
    Assert-DoesNotContain -Text $text -Needle 'Formal next work is `F32 audit PC controlled-demo command readability and formation feel`' -Label $relativePath
    Assert-DoesNotContain -Text $text -Needle '正式下一步是 `F29 audit PC controlled-demo visual readability`' -Label $relativePath
    Assert-DoesNotContain -Text $text -Needle '正式下一步是 `F32 audit PC controlled-demo command readability and formation feel`' -Label $relativePath
    Assert-DoesNotContain -Text $text -Needle '正式下一步推进 F29 audit PC controlled-demo visual readability' -Label $relativePath
    Assert-DoesNotContain -Text $text -Needle '正式下一步推进 F32 audit PC controlled-demo command readability and formation feel' -Label $relativePath
    Assert-DoesNotContain -Text $text -Needle '下一步进入 `F29 audit PC controlled-demo visual readability`' -Label $relativePath
    Assert-DoesNotContain -Text $text -Needle '下一步进入 `F32 audit PC controlled-demo command readability and formation feel`' -Label $relativePath
}

$mobilePlan = Read-RequiredText -RelativePath "docs-mobile-first-plan-2026-06-10.md"
Assert-Contains -Text $mobilePlan -Needle "| G3 | Done | Android device smoke |" -Label "mobile gate order"
Assert-Contains -Text $mobilePlan -Needle "| G4 | Done | Touch UI pass |" -Label "mobile gate order"
Assert-Contains -Text $mobilePlan -Needle "| G5 | Done | Mobile performance budget |" -Label "mobile gate order"
Assert-Contains -Text $mobilePlan -Needle "| G6 | Done | iOS feasibility gate |" -Label "mobile gate order"
Assert-Contains -Text $mobilePlan -Needle "F2 map authoring contract" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F3 web ranking contract" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F4 creator economy boundary" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F5 server implementation boundary" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F6 local main-server prototype" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F7 document Unity main-server integration contract" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F8 implement optional Unity main-server client adapter" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F9 wire optional Unity main-server adapter into launch/debrief smoke" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F10 wire optional Unity inventory bootstrap smoke" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F11 plan inventory-to-MechBay binding boundary" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F12 implement opt-in inventory-to-MechBay preview binding" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F13 capture opt-in MechBay preview evidence" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F14 capture landscape-phone MechLab source-line evidence" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F15 plan server-backed receipt slice" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F16 implement server-backed receipt evidence gate" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F17 plan post-receipt inventory refresh boundary" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F18 implement opt-in post-receipt inventory refresh binding" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F19 capture opt-in post-receipt refresh evidence" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F20 refresh Android landscape build/smoke evidence" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F21 audit landscape touch UI ergonomics" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F22 audit landscape MechLab touch controls" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F23 capture landscape MechLab touch evidence" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F24 capture Android MechLab touch evidence" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F25 capture Android battle command touch evidence" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F26 reduce Android combat effect log noise" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F27 audit Android entity placeholder collision path" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F28 capture Android entity placeholder collision runtime evidence" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F29 audit PC controlled-demo visual readability" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F30 implement PC controlled-demo visual readability fixes" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F31 refresh PC controlled-demo visual evidence after readability fixes" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F32 audit PC controlled-demo command readability and formation feel" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F33 implement PC controlled-demo command readability and formation fixes" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F34 refresh PC controlled-demo command evidence after readability fixes" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F35 audit post-F34 PC controlled-demo playable flow polish" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F36 implement post-F34 PC controlled-demo playable flow polish fixes" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F37 refresh PC controlled-demo playable-flow evidence after polish fixes" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F38 audit post-F37 PC controlled-demo investor readiness" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F39 implement post-F37 PC controlled-demo investor readiness fixes" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F40 refresh PC controlled-demo investor-readiness evidence after fixes" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F41 audit post-F40 PC controlled-demo investor evidence package" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F42 implement post-F41 PC controlled-demo investor evidence package fixes" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F43 refresh PC controlled-demo investor evidence package after fixes" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F44 audit post-F43 PC controlled-demo investor evidence refresh" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F45 implement post-F44 PC controlled-demo investor evidence polish fixes" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F46 refresh PC controlled-demo investor route evidence after polish fixes" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F47 audit post-F46 PC controlled-demo investor route evidence refresh" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F48 implement post-F47 PC controlled-demo investor route evidence audit fixes" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F49 refresh PC controlled-demo investor route evidence after audit fixes" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F50 audit post-F49 PC controlled-demo investor route evidence refresh" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F51 implement post-F50 PC controlled-demo investor route evidence refresh audit fixes" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F52 refresh PC controlled-demo investor route evidence after F50 audit fixes" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F53 audit post-F52 PC controlled-demo investor route evidence refresh" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F54 implement post-F53 PC controlled-demo investor route evidence refresh audit fixes" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F55 refresh PC controlled-demo investor route evidence after F53 audit fixes" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F56 audit post-F55 PC controlled-demo investor route evidence refresh" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F57 implement post-F56 PC controlled-demo investor route evidence refresh audit fixes" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F58 refresh PC controlled-demo investor route evidence after F56 audit fixes" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F59 audit post-F58 PC controlled-demo investor route evidence refresh" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F61 refresh PC controlled-demo investor route evidence after F59 audit fixes" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F62 audit post-F61 PC controlled-demo investor route evidence refresh" -Label "mobile completed platform task"
Assert-Contains -Text $mobilePlan -Needle "F63 implement post-F62 PC controlled-demo investor route evidence refresh audit fixes" -Label "mobile next task"
Assert-Contains -Text $mobilePlan -Needle "first phone version is landscape-only" -Label "mobile orientation decision"
Assert-Contains -Text $mobilePlan -Needle "horizontal phone game" -Label "mobile horizontal phone version decision"

$detailedPlan = Read-RequiredText -RelativePath "docs-ai-rts-commander-current-detailed-plan-2026-06-07.md"
Assert-Contains -Text $detailedPlan -Needle '| F2 | Done | `Document map authoring contract` |' -Label "detailed queue F2"
Assert-Contains -Text $detailedPlan -Needle '| F3 | Done | `Document web ranking contract` |' -Label "detailed queue F3"
Assert-Contains -Text $detailedPlan -Needle '| F4 | Done | `Document creator economy boundary` |' -Label "detailed queue F4"
Assert-Contains -Text $detailedPlan -Needle '| F5 | Done | `Document server implementation boundary` |' -Label "detailed queue F5"
Assert-Contains -Text $detailedPlan -Needle '| F6 | Done | `Scaffold local main-server prototype` |' -Label "detailed queue F6"
Assert-Contains -Text $detailedPlan -Needle '| F7 | Done | `Document Unity main-server integration contract` |' -Label "detailed queue F7"
Assert-Contains -Text $detailedPlan -Needle '| F8 | Done | `Implement optional Unity main-server client adapter` |' -Label "detailed queue F8"
Assert-Contains -Text $detailedPlan -Needle '| F9 | Done | `Wire optional Unity main-server adapter into launch/debrief smoke` |' -Label "detailed queue F9"
Assert-Contains -Text $detailedPlan -Needle '| F10 | Done | `Wire optional Unity inventory bootstrap smoke` |' -Label "detailed queue F10"
Assert-Contains -Text $detailedPlan -Needle '| F11 | Done | `Plan inventory-to-MechBay binding boundary` |' -Label "detailed queue F11"
Assert-Contains -Text $detailedPlan -Needle '| F12 | Done | `Implement opt-in inventory-to-MechBay preview binding` |' -Label "detailed queue F12"
Assert-Contains -Text $detailedPlan -Needle '| F13 | Done | `Capture opt-in MechBay preview evidence` |' -Label "detailed queue F13"
Assert-Contains -Text $detailedPlan -Needle '| F14 | Done | `Capture landscape-phone MechLab source-line evidence` |' -Label "detailed queue F14"
Assert-Contains -Text $detailedPlan -Needle '| F15 | Done | `Plan server-backed receipt slice` |' -Label "detailed queue F15"
Assert-Contains -Text $detailedPlan -Needle '| F16 | Done | `Implement server-backed receipt evidence gate` |' -Label "detailed queue F16"
Assert-Contains -Text $detailedPlan -Needle '| F17 | Done | `Plan post-receipt inventory refresh boundary` |' -Label "detailed queue F17"
Assert-Contains -Text $detailedPlan -Needle '| F18 | Done | `Implement opt-in post-receipt inventory refresh binding` |' -Label "detailed queue F18"
Assert-Contains -Text $detailedPlan -Needle '| F19 | Done | `Capture opt-in post-receipt refresh evidence` |' -Label "detailed queue F19"
Assert-Contains -Text $detailedPlan -Needle '| F20 | Done | `Refresh Android landscape build/smoke evidence` |' -Label "detailed queue F20"
Assert-Contains -Text $detailedPlan -Needle '| F21 | Done | `Audit landscape touch UI ergonomics` |' -Label "detailed queue F21"
Assert-Contains -Text $detailedPlan -Needle '| F22 | Done | `Audit landscape MechLab touch controls` |' -Label "detailed queue F22"
Assert-Contains -Text $detailedPlan -Needle '| F23 | Done | `Capture landscape MechLab touch evidence` |' -Label "detailed queue F23"
Assert-Contains -Text $detailedPlan -Needle '| F24 | Done | `Capture Android MechLab touch evidence` |' -Label "detailed queue F24"
Assert-Contains -Text $detailedPlan -Needle '| F25 | Done | `Capture Android battle command touch evidence` |' -Label "detailed queue F25"
Assert-Contains -Text $detailedPlan -Needle '| F26 | Done | `Reduce Android combat effect log noise` |' -Label "detailed queue F26"
Assert-Contains -Text $detailedPlan -Needle '| F27 | Done | `Audit Android entity placeholder collision path` |' -Label "detailed queue F27"
Assert-Contains -Text $detailedPlan -Needle '| F28 | Done | `Capture Android entity placeholder collision runtime evidence` |' -Label "detailed queue F28"
Assert-Contains -Text $detailedPlan -Needle '| F29 | Done | `Audit PC controlled-demo visual readability` |' -Label "detailed queue F29"
Assert-Contains -Text $detailedPlan -Needle '| F30 | Done | `Implement PC controlled-demo visual readability fixes` |' -Label "detailed queue F30"
Assert-Contains -Text $detailedPlan -Needle '| F31 | Done | `Refresh PC controlled-demo visual evidence after readability fixes` |' -Label "detailed queue F31"
Assert-Contains -Text $detailedPlan -Needle '| F32 | Done | `Audit PC controlled-demo command readability and formation feel` |' -Label "detailed queue F32"
Assert-Contains -Text $detailedPlan -Needle '| F33 | Done | `Implement PC controlled-demo command readability and formation fixes` |' -Label "detailed queue F33"
Assert-Contains -Text $detailedPlan -Needle '| F34 | Done | `Refresh PC controlled-demo command evidence after readability fixes` |' -Label "detailed queue F34"
Assert-Contains -Text $detailedPlan -Needle '| F35 | Done | `Audit post-F34 PC controlled-demo playable flow polish` |' -Label "detailed queue F35"
Assert-Contains -Text $detailedPlan -Needle '| F36 | Done | `Implement post-F34 PC controlled-demo playable flow polish fixes` |' -Label "detailed queue F36"
Assert-Contains -Text $detailedPlan -Needle '| F37 | Done | `Refresh PC controlled-demo playable-flow evidence after polish fixes` |' -Label "detailed queue F37"
Assert-Contains -Text $detailedPlan -Needle '| F38 | Done | `Audit post-F37 PC controlled-demo investor readiness` |' -Label "detailed queue F38"
Assert-Contains -Text $detailedPlan -Needle '| F39 | Done | `Implement post-F37 PC controlled-demo investor readiness fixes` |' -Label "detailed queue F39"
Assert-Contains -Text $detailedPlan -Needle '| F40 | Done | `Refresh PC controlled-demo investor-readiness evidence after fixes` |' -Label "detailed queue F40"
Assert-Contains -Text $detailedPlan -Needle '| F41 | Done | `Audit post-F40 PC controlled-demo investor evidence package` |' -Label "detailed queue F41"
Assert-Contains -Text $detailedPlan -Needle '| F42 | Done | `Implement post-F41 PC controlled-demo investor evidence package fixes` |' -Label "detailed queue F42"
Assert-Contains -Text $detailedPlan -Needle '| F43 | Done | `Refresh PC controlled-demo investor evidence package after fixes` |' -Label "detailed queue F43"
Assert-Contains -Text $detailedPlan -Needle '| F44 | Done | `Audit post-F43 PC controlled-demo investor evidence refresh` |' -Label "detailed queue F44"
Assert-Contains -Text $detailedPlan -Needle '| F45 | Done | `Implement post-F44 PC controlled-demo investor evidence polish fixes` |' -Label "detailed queue F45"
Assert-Contains -Text $detailedPlan -Needle '| F46 | Done | `Refresh PC controlled-demo investor route evidence after polish fixes` |' -Label "detailed queue F46"
Assert-Contains -Text $detailedPlan -Needle '| F47 | Done | `Audit post-F46 PC controlled-demo investor route evidence refresh` |' -Label "detailed queue F47"
Assert-Contains -Text $detailedPlan -Needle '| F48 | Done | `Implement post-F47 PC controlled-demo investor route evidence audit fixes` |' -Label "detailed queue F48"
Assert-Contains -Text $detailedPlan -Needle '| F49 | Done | `Refresh PC controlled-demo investor route evidence after audit fixes` |' -Label "detailed queue F49"
Assert-Contains -Text $detailedPlan -Needle '| F50 | Done | `Audit post-F49 PC controlled-demo investor route evidence refresh` |' -Label "detailed queue F50"
Assert-Contains -Text $detailedPlan -Needle '| F51 | Done | `Implement post-F50 PC controlled-demo investor route evidence refresh audit fixes` |' -Label "detailed queue F51"
Assert-Contains -Text $detailedPlan -Needle '| F52 | Done | `Refresh PC controlled-demo investor route evidence after F50 audit fixes` |' -Label "detailed queue F52"
Assert-Contains -Text $detailedPlan -Needle '| F53 | Done | `Audit post-F52 PC controlled-demo investor route evidence refresh` |' -Label "detailed queue F53"
Assert-Contains -Text $detailedPlan -Needle '| F54 | Done | `Implement post-F53 PC controlled-demo investor route evidence refresh audit fixes` |' -Label "detailed queue F54"
Assert-Contains -Text $detailedPlan -Needle '| F55 | Done | `Refresh PC controlled-demo investor route evidence after F53 audit fixes` |' -Label "detailed queue F55"
Assert-Contains -Text $detailedPlan -Needle '| F56 | Done | `Audit post-F55 PC controlled-demo investor route evidence refresh` |' -Label "detailed queue F56"
Assert-Contains -Text $detailedPlan -Needle '| F57 | Done | `Implement post-F56 PC controlled-demo investor route evidence refresh audit fixes` |' -Label "detailed queue F57"
Assert-Contains -Text $detailedPlan -Needle '| F58 | Done | `Refresh PC controlled-demo investor route evidence after F56 audit fixes` |' -Label "detailed queue F58"
Assert-Contains -Text $detailedPlan -Needle '| F59 | Done | `Audit post-F58 PC controlled-demo investor route evidence refresh` |' -Label "detailed queue F59"
Assert-Contains -Text $detailedPlan -Needle '| F60 | Done | `Implement post-F59 PC controlled-demo investor route evidence refresh audit fixes` |' -Label "detailed queue F60"
Assert-Contains -Text $detailedPlan -Needle '| F61 | Done | `Refresh PC controlled-demo investor route evidence after F59 audit fixes` |' -Label "detailed queue F61"
Assert-Contains -Text $detailedPlan -Needle '| F62 | Done | `Audit post-F61 PC controlled-demo investor route evidence refresh` |' -Label "detailed queue F62"
Assert-Contains -Text $detailedPlan -Needle '| F63 | Next | `Implement post-F62 PC controlled-demo investor route evidence refresh audit fixes` |' -Label "detailed queue F63"

$handoff = Read-RequiredText -RelativePath "docs-machine-handoff-plan-2026-06-07.md"
Assert-Contains -Text $handoff -Needle 'Current formal next development task after handoff: `F63 implement post-F62 PC controlled-demo investor route evidence refresh audit fixes`' -Label "handoff next task"
Assert-Contains -Text $handoff -Needle 'Next planned work: `F63 implement post-F62 PC controlled-demo investor route evidence refresh audit fixes`' -Label "handoff next planned work"

$currentGate = Read-RequiredText -RelativePath "scripts\unity\check_current_plan_gate.ps1"
Assert-Contains -Text $currentGate -Needle 'CommandFileSmoke: True' -Label "current gate script marker"
Assert-Contains -Text $currentGate -Needle 'WpdOnlyAndroidProbe: True' -Label "current gate device diagnosis marker"
Assert-Contains -Text $currentGate -Needle 'AdbSetupHint: True' -Label "current gate adb setup marker"
Assert-Contains -Text $currentGate -Needle 'AdbDriverPackageProbe: True' -Label "current gate adb driver package marker"
Assert-Contains -Text $currentGate -Needle 'AdbWatchHint: True' -Label "current gate adb watch marker"
Assert-Contains -Text $currentGate -Needle 'G3DeviceStatusReport: True' -Label "current gate g3 device status marker"
Assert-Contains -Text $currentGate -Needle 'G3WhenReady: True' -Label "current gate g3 when-ready marker"
Assert-Contains -Text $currentGate -Needle 'SmokeSuccessMarker: MC2 loadout compact assertion OK' -Label "current gate success marker"
Assert-Contains -Text $currentGate -Needle 'LandscapeScreenshot: True' -Label "current gate landscape screenshot marker"
Assert-Contains -Text $currentGate -Needle 'Mobile performance budget check OK.' -Label "current gate performance marker"
Assert-Contains -Text $currentGate -Needle 'iOS feasibility gate check OK.' -Label "current gate iOS feasibility marker"
Assert-Contains -Text $currentGate -Needle 'Map authoring contract check OK.' -Label "current gate map authoring marker"
Assert-Contains -Text $currentGate -Needle 'Web ranking contract check OK.' -Label "current gate web ranking marker"
Assert-Contains -Text $currentGate -Needle 'Creator economy boundary check OK.' -Label "current gate creator economy marker"
Assert-Contains -Text $currentGate -Needle 'Server implementation boundary check OK.' -Label "current gate server boundary marker"
Assert-Contains -Text $currentGate -Needle 'Mobile landscape contract check OK.' -Label "current gate mobile landscape marker"
Assert-Contains -Text $currentGate -Needle 'Local main-server prototype check OK.' -Label "current gate local main-server marker"
Assert-Contains -Text $currentGate -Needle 'Unity main-server integration contract check OK.' -Label "current gate unity main-server integration marker"
Assert-Contains -Text $currentGate -Needle 'Optional Unity main-server client adapter check OK.' -Label "current gate optional unity main-server client marker"
Assert-Contains -Text $currentGate -Needle 'Optional Unity main-server launch/debrief smoke check OK.' -Label "current gate optional unity main-server launch/debrief marker"
Assert-Contains -Text $currentGate -Needle 'Optional Unity inventory bootstrap smoke check OK.' -Label "current gate optional unity inventory bootstrap marker"
Assert-Contains -Text $currentGate -Needle 'Inventory-to-MechBay binding boundary check OK.' -Label "current gate inventory-to-MechBay binding marker"
Assert-Contains -Text $currentGate -Needle 'Optional inventory-to-MechBay preview binding check OK.' -Label "current gate inventory-to-MechBay preview marker"
Assert-Contains -Text $currentGate -Needle 'Inventory MechBay preview evidence capture OK.' -Label "current gate inventory-to-MechBay preview evidence marker"
Assert-Contains -Text $currentGate -Needle 'Landscape-phone MechLab source-line evidence capture OK.' -Label "current gate landscape-phone MechLab source-line evidence marker"
Assert-Contains -Text $currentGate -Needle 'Landscape MechLab touch evidence capture OK.' -Label "current gate landscape MechLab touch evidence marker"
Assert-Contains -Text $currentGate -Needle 'Android MechLab touch evidence capture plan OK.' -Label "current gate Android MechLab touch evidence marker"
Assert-Contains -Text $currentGate -Needle 'Android battle command touch evidence capture plan OK.' -Label "current gate Android battle command touch evidence marker"
Assert-Contains -Text $currentGate -Needle 'Android combat effect log noise check OK.' -Label "current gate Android combat effect log noise marker"
Assert-Contains -Text $currentGate -Needle 'Android entity placeholder collision path check OK.' -Label "current gate Android entity placeholder collision path marker"
Assert-Contains -Text $currentGate -Needle 'Android entity placeholder collision runtime evidence capture plan OK.' -Label "current gate Android entity placeholder collision runtime evidence marker"
Assert-Contains -Text $currentGate -Needle 'audit_pc_controlled_demo_visual_readability.ps1' -Label "current gate PC controlled-demo visual readability audit script marker"
Assert-Contains -Text $currentGate -Needle 'PC controlled-demo visual readability audit plan OK.' -Label "current gate PC controlled-demo visual readability audit marker"
Assert-Contains -Text $currentGate -Needle 'check_pc_controlled_demo_visual_readability_fixes.ps1' -Label "current gate PC controlled-demo visual readability fixes script marker"
Assert-Contains -Text $currentGate -Needle 'PC controlled-demo visual readability fixes plan OK.' -Label "current gate PC controlled-demo visual readability fixes marker"
Assert-Contains -Text $currentGate -Needle 'capture_pc_controlled_demo_visual_evidence.ps1' -Label "current gate PC controlled-demo visual evidence script marker"
Assert-Contains -Text $currentGate -Needle 'PC controlled-demo visual evidence refresh plan OK.' -Label "current gate PC controlled-demo visual evidence marker"
Assert-Contains -Text $currentGate -Needle 'audit_pc_controlled_demo_command_readability_formation.ps1' -Label "current gate PC controlled-demo command readability audit script marker"
Assert-Contains -Text $currentGate -Needle 'PC controlled-demo command readability formation audit plan OK.' -Label "current gate PC controlled-demo command readability audit marker"
Assert-Contains -Text $currentGate -Needle 'check_pc_controlled_demo_command_readability_fixes.ps1' -Label "current gate PC controlled-demo command readability fixes script marker"
Assert-Contains -Text $currentGate -Needle 'PC controlled-demo command readability fixes plan OK.' -Label "current gate PC controlled-demo command readability fixes marker"
Assert-Contains -Text $currentGate -Needle 'capture_pc_controlled_demo_command_evidence.ps1' -Label "current gate PC controlled-demo command evidence script marker"
Assert-Contains -Text $currentGate -Needle 'PC controlled-demo command evidence refresh plan OK.' -Label "current gate PC controlled-demo command evidence marker"
Assert-Contains -Text $currentGate -Needle 'audit_pc_controlled_demo_playable_flow_polish.ps1' -Label "current gate PC controlled-demo playable flow audit script marker"
Assert-Contains -Text $currentGate -Needle 'PC controlled-demo playable flow polish audit plan OK.' -Label "current gate PC controlled-demo playable flow audit marker"
Assert-Contains -Text $currentGate -Needle 'check_pc_controlled_demo_playable_flow_polish_fixes.ps1' -Label "current gate PC controlled-demo playable flow fixes script marker"
Assert-Contains -Text $currentGate -Needle 'PC controlled-demo playable flow polish fixes plan OK.' -Label "current gate PC controlled-demo playable flow fixes marker"
Assert-Contains -Text $currentGate -Needle 'audit_pc_controlled_demo_investor_readiness.ps1' -Label "current gate PC controlled-demo investor readiness audit script marker"
Assert-Contains -Text $currentGate -Needle 'PC controlled-demo investor readiness audit plan OK.' -Label "current gate PC controlled-demo investor readiness audit marker"
Assert-Contains -Text $currentGate -Needle 'check_pc_controlled_demo_investor_readiness_fixes.ps1' -Label "current gate PC controlled-demo investor readiness fixes script marker"
Assert-Contains -Text $currentGate -Needle 'PC controlled-demo investor readiness fixes plan OK.' -Label "current gate PC controlled-demo investor readiness fixes marker"
Assert-Contains -Text $currentGate -Needle 'audit_pc_controlled_demo_investor_evidence_package.ps1' -Label "current gate PC controlled-demo investor evidence package audit script marker"
Assert-Contains -Text $currentGate -Needle 'PC controlled-demo investor evidence package audit plan OK.' -Label "current gate PC controlled-demo investor evidence package audit marker"
Assert-Contains -Text $currentGate -Needle 'check_pc_controlled_demo_investor_evidence_package_fixes.ps1' -Label "current gate PC controlled-demo investor evidence package fixes script marker"
Assert-Contains -Text $currentGate -Needle 'PC controlled-demo investor evidence package fixes plan OK.' -Label "current gate PC controlled-demo investor evidence package fixes marker"
Assert-Contains -Text $currentGate -Needle 'audit_pc_controlled_demo_investor_evidence_refresh.ps1' -Label "current gate PC controlled-demo investor evidence refresh audit script marker"
Assert-Contains -Text $currentGate -Needle 'PC controlled-demo investor evidence refresh audit plan OK.' -Label "current gate PC controlled-demo investor evidence refresh audit marker"
Assert-Contains -Text $currentGate -Needle 'check_pc_controlled_demo_investor_evidence_polish_fixes.ps1' -Label "current gate PC controlled-demo investor evidence polish fixes script marker"
Assert-Contains -Text $currentGate -Needle 'PC controlled-demo investor evidence polish fixes plan OK.' -Label "current gate PC controlled-demo investor evidence polish fixes marker"
Assert-Contains -Text $currentGate -Needle 'check_pc_controlled_demo_investor_route_evidence_refresh.ps1' -Label "current gate PC controlled-demo investor route evidence refresh script marker"
Assert-Contains -Text $currentGate -Needle 'PC controlled-demo investor route evidence refresh plan OK.' -Label "current gate PC controlled-demo investor route evidence refresh marker"
Assert-Contains -Text $currentGate -Needle 'audit_pc_controlled_demo_investor_route_evidence_refresh.ps1' -Label "current gate PC controlled-demo investor route evidence refresh audit script marker"
Assert-Contains -Text $currentGate -Needle 'PC controlled-demo investor route evidence refresh audit plan OK.' -Label "current gate PC controlled-demo investor route evidence refresh audit marker"
Assert-Contains -Text $currentGate -Needle 'check_pc_controlled_demo_investor_route_evidence_audit_fixes.ps1' -Label "current gate PC controlled-demo investor route evidence audit fixes script marker"
Assert-Contains -Text $currentGate -Needle 'PC controlled-demo investor route evidence audit fixes plan OK.' -Label "current gate PC controlled-demo investor route evidence audit fixes marker"
Assert-Contains -Text $currentGate -Needle 'check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh.ps1' -Label "current gate PC controlled-demo investor route evidence audit fix refresh script marker"
Assert-Contains -Text $currentGate -Needle 'PC controlled-demo investor route evidence audit fix refresh plan OK.' -Label "current gate PC controlled-demo investor route evidence audit fix refresh marker"
Assert-Contains -Text $currentGate -Needle 'audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh.ps1' -Label "current gate PC controlled-demo investor route evidence audit fix refresh audit script marker"
Assert-Contains -Text $currentGate -Needle 'PC controlled-demo investor route evidence audit fix refresh audit plan OK.' -Label "current gate PC controlled-demo investor route evidence audit fix refresh audit marker"
Assert-Contains -Text $currentGate -Needle 'check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fixes.ps1' -Label "current gate PC controlled-demo investor route evidence audit fix refresh audit fixes script marker"
Assert-Contains -Text $currentGate -Needle 'PC controlled-demo investor route evidence audit fix refresh audit fixes plan OK.' -Label "current gate PC controlled-demo investor route evidence audit fix refresh audit fixes marker"
Assert-Contains -Text $currentGate -Needle 'check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh.ps1' -Label "current gate PC controlled-demo investor route evidence audit fix refresh audit fix refresh script marker"
Assert-Contains -Text $currentGate -Needle 'PC controlled-demo investor route evidence audit fix refresh audit fix refresh plan OK.' -Label "current gate PC controlled-demo investor route evidence audit fix refresh audit fix refresh marker"
Assert-Contains -Text $currentGate -Needle 'audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh.ps1' -Label "current gate PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit script marker"
Assert-Contains -Text $currentGate -Needle 'PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit plan OK.' -Label "current gate PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit marker"
Assert-Contains -Text $currentGate -Needle 'Server-backed receipt slice plan check OK.' -Label "current gate server-backed receipt slice marker"
Assert-Contains -Text $currentGate -Needle 'Server-backed receipt evidence capture OK.' -Label "current gate server-backed receipt evidence marker"
Assert-Contains -Text $currentGate -Needle 'Post-receipt inventory refresh boundary check OK.' -Label "current gate post-receipt inventory refresh marker"
Assert-Contains -Text $currentGate -Needle 'Post-receipt inventory refresh binding check OK.' -Label "current gate post-receipt inventory refresh binding marker"
Assert-Contains -Text $currentGate -Needle 'Post-receipt refresh evidence capture OK.' -Label "current gate post-receipt refresh evidence marker"
Assert-Contains -Text $currentGate -Needle 'Landscape touch UI ergonomics check OK.' -Label "current gate landscape touch ergonomics marker"
Assert-Contains -Text $currentGate -Needle 'Landscape MechLab touch controls check OK.' -Label "current gate landscape MechLab touch controls marker"

$handoffScript = Read-RequiredText -RelativePath "scripts\unity\check_controlled_demo_handoff.ps1"
Assert-Contains -Text $handoffScript -Needle 'CommandFileSmoke: True' -Label "handoff script marker"
Assert-Contains -Text $handoffScript -Needle 'WpdOnlyAndroidProbe: True' -Label "handoff script device diagnosis marker"
Assert-Contains -Text $handoffScript -Needle 'AdbSetupHint: True' -Label "handoff script adb setup marker"
Assert-Contains -Text $handoffScript -Needle 'AdbDriverPackageProbe: True' -Label "handoff script adb driver package marker"
Assert-Contains -Text $handoffScript -Needle 'AdbWatchHint: True' -Label "handoff script adb watch marker"
Assert-Contains -Text $handoffScript -Needle 'G3DeviceStatusReport: True' -Label "handoff script g3 device status marker"
Assert-Contains -Text $handoffScript -Needle 'G3WhenReady: True' -Label "handoff script g3 when-ready marker"
Assert-Contains -Text $handoffScript -Needle 'SmokeSuccessMarker: MC2 loadout compact assertion OK' -Label "handoff script success marker"
Assert-Contains -Text $handoffScript -Needle 'Web ranking contract check OK' -Label "handoff script web ranking marker"
Assert-Contains -Text $handoffScript -Needle 'Creator economy boundary check OK' -Label "handoff script creator economy marker"
Assert-Contains -Text $handoffScript -Needle 'Server implementation boundary check OK' -Label "handoff script server boundary marker"
Assert-Contains -Text $handoffScript -Needle 'Mobile landscape contract check OK' -Label "handoff script mobile landscape marker"
Assert-Contains -Text $handoffScript -Needle 'Local main-server prototype check OK' -Label "handoff script local main-server marker"
Assert-Contains -Text $handoffScript -Needle 'Unity main-server integration contract check OK' -Label "handoff script unity main-server integration marker"
Assert-Contains -Text $handoffScript -Needle 'Optional Unity main-server client adapter check OK' -Label "handoff script optional unity main-server client marker"
Assert-Contains -Text $handoffScript -Needle 'Optional Unity main-server launch/debrief smoke check OK' -Label "handoff script optional unity main-server launch/debrief marker"
Assert-Contains -Text $handoffScript -Needle 'Optional Unity inventory bootstrap smoke check OK' -Label "handoff script optional unity inventory bootstrap marker"
Assert-Contains -Text $handoffScript -Needle 'Inventory-to-MechBay binding boundary check OK' -Label "handoff script inventory-to-MechBay binding marker"
Assert-Contains -Text $handoffScript -Needle 'Optional inventory-to-MechBay preview binding check OK' -Label "handoff script inventory-to-MechBay preview marker"
Assert-Contains -Text $handoffScript -Needle 'Inventory MechBay preview evidence capture OK' -Label "handoff script inventory-to-MechBay preview evidence marker"
Assert-Contains -Text $handoffScript -Needle 'Landscape-phone MechLab source-line evidence capture OK' -Label "handoff script landscape-phone MechLab source-line evidence marker"
Assert-Contains -Text $handoffScript -Needle 'Landscape MechLab touch evidence capture OK' -Label "handoff script landscape MechLab touch evidence marker"
Assert-Contains -Text $handoffScript -Needle 'Android MechLab touch evidence capture OK' -Label "handoff script Android MechLab touch evidence marker"
Assert-Contains -Text $handoffScript -Needle 'capture_android_battle_command_touch_evidence.ps1' -Label "handoff script Android battle command touch evidence script marker"
Assert-Contains -Text $handoffScript -Needle 'Android battle command touch evidence capture OK' -Label "handoff script Android battle command touch evidence marker"
Assert-Contains -Text $handoffScript -Needle 'check_android_combat_effect_log_noise.ps1' -Label "handoff script Android combat effect log noise script marker"
Assert-Contains -Text $handoffScript -Needle 'Android combat effect log noise check OK' -Label "handoff script Android combat effect log noise marker"
Assert-Contains -Text $handoffScript -Needle 'check_android_entity_placeholder_collision_path.ps1' -Label "handoff script Android entity placeholder collision path script marker"
Assert-Contains -Text $handoffScript -Needle 'Android entity placeholder collision path check OK' -Label "handoff script Android entity placeholder collision path marker"
Assert-Contains -Text $handoffScript -Needle 'F27 audit Android entity placeholder collision path' -Label "handoff script F27 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'capture_android_entity_placeholder_collision_runtime_evidence.ps1' -Label "handoff script Android entity placeholder collision runtime evidence script marker"
Assert-Contains -Text $handoffScript -Needle 'Android entity placeholder collision runtime evidence capture OK' -Label "handoff script Android entity placeholder collision runtime evidence marker"
Assert-Contains -Text $handoffScript -Needle 'F28 capture Android entity placeholder collision runtime evidence' -Label "handoff script F28 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'F29 audit PC controlled-demo visual readability' -Label "handoff script F29 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'F30 implement PC controlled-demo visual readability fixes' -Label "handoff script F30 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'F31 refresh PC controlled-demo visual evidence after readability fixes' -Label "handoff script F31 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'capture_pc_controlled_demo_visual_evidence.ps1' -Label "handoff script F31 visual evidence script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo visual evidence refresh OK' -Label "handoff script F31 visual evidence marker"
Assert-Contains -Text $handoffScript -Needle 'F32 audit PC controlled-demo command readability and formation feel' -Label "handoff script F32 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'audit_pc_controlled_demo_command_readability_formation.ps1' -Label "handoff script F32 command readability audit script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo command readability formation audit OK' -Label "handoff script F32 command readability audit marker"
Assert-Contains -Text $handoffScript -Needle 'F33 implement PC controlled-demo command readability and formation fixes' -Label "handoff script F33 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'check_pc_controlled_demo_command_readability_fixes.ps1' -Label "handoff script F33 command readability fixes script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo command readability fixes check OK' -Label "handoff script F33 command readability fixes marker"
Assert-Contains -Text $handoffScript -Needle 'F34 refresh PC controlled-demo command evidence after readability fixes' -Label "handoff script F34 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'capture_pc_controlled_demo_command_evidence.ps1' -Label "handoff script F34 command evidence script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo command evidence refresh OK' -Label "handoff script F34 command evidence marker"
Assert-Contains -Text $handoffScript -Needle 'F35 audit post-F34 PC controlled-demo playable flow polish' -Label "handoff script F35 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'audit_pc_controlled_demo_playable_flow_polish.ps1' -Label "handoff script F35 playable flow audit script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo playable flow polish audit OK' -Label "handoff script F35 playable flow audit marker"
Assert-Contains -Text $handoffScript -Needle 'F36 implement post-F34 PC controlled-demo playable flow polish fixes' -Label "handoff script F36 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'check_pc_controlled_demo_playable_flow_polish_fixes.ps1' -Label "handoff script F36 playable flow fixes script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo playable flow polish fixes check OK' -Label "handoff script F36 playable flow fixes marker"
Assert-Contains -Text $handoffScript -Needle 'F37 refresh PC controlled-demo playable-flow evidence after polish fixes' -Label "handoff script F37 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'F38 audit post-F37 PC controlled-demo investor readiness' -Label "handoff script F38 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'audit_pc_controlled_demo_investor_readiness.ps1' -Label "handoff script F38 investor readiness audit script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo investor readiness audit OK' -Label "handoff script F38 investor readiness audit marker"
Assert-Contains -Text $handoffScript -Needle 'F39 implement post-F37 PC controlled-demo investor readiness fixes' -Label "handoff script F39 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'check_pc_controlled_demo_investor_readiness_fixes.ps1' -Label "handoff script F39 investor readiness fixes script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo investor readiness fixes check OK' -Label "handoff script F39 investor readiness fixes marker"
Assert-Contains -Text $handoffScript -Needle 'F40 refresh PC controlled-demo investor-readiness evidence after fixes' -Label "handoff script F40 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'capture_pc_controlled_demo_command_evidence.ps1' -Label "handoff script F40 command evidence script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo command evidence refresh OK' -Label "handoff script F40 command evidence marker"
Assert-Contains -Text $handoffScript -Needle 'F41 audit post-F40 PC controlled-demo investor evidence package' -Label "handoff script F41 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'audit_pc_controlled_demo_investor_evidence_package.ps1' -Label "handoff script F41 investor evidence package audit script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo investor evidence package audit OK' -Label "handoff script F41 investor evidence package audit marker"
Assert-Contains -Text $handoffScript -Needle 'F42 implement post-F41 PC controlled-demo investor evidence package fixes' -Label "handoff script F42 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'check_pc_controlled_demo_investor_evidence_package_fixes.ps1' -Label "handoff script F42 investor evidence package fixes script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo investor evidence package fixes check OK' -Label "handoff script F42 investor evidence package fixes marker"
Assert-Contains -Text $handoffScript -Needle 'F43 refresh PC controlled-demo investor evidence package after fixes' -Label "handoff script F43 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'check_pc_controlled_demo_investor_evidence_refresh.ps1' -Label "handoff script F43 investor evidence refresh script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo investor evidence refresh check OK' -Label "handoff script F43 investor evidence refresh marker"
Assert-Contains -Text $handoffScript -Needle 'F44 audit post-F43 PC controlled-demo investor evidence refresh' -Label "handoff script F44 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'audit_pc_controlled_demo_investor_evidence_refresh.ps1' -Label "handoff script F44 investor evidence refresh audit script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo investor evidence refresh audit OK' -Label "handoff script F44 investor evidence refresh audit marker"
Assert-Contains -Text $handoffScript -Needle 'F45 implement post-F44 PC controlled-demo investor evidence polish fixes' -Label "handoff script F45 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'check_pc_controlled_demo_investor_evidence_polish_fixes.ps1' -Label "handoff script F45 investor evidence polish fixes script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo investor evidence polish fixes check OK' -Label "handoff script F45 investor evidence polish fixes marker"
Assert-Contains -Text $handoffScript -Needle 'F46 refresh PC controlled-demo investor route evidence after polish fixes' -Label "handoff script F46 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'check_pc_controlled_demo_investor_route_evidence_refresh.ps1' -Label "handoff script F46 investor route evidence refresh script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo investor route evidence refresh check OK' -Label "handoff script F46 investor route evidence refresh marker"
Assert-Contains -Text $handoffScript -Needle 'F47 audit post-F46 PC controlled-demo investor route evidence refresh' -Label "handoff script F47 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'audit_pc_controlled_demo_investor_route_evidence_refresh.ps1' -Label "handoff script F47 investor route evidence refresh audit script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo investor route evidence refresh audit OK' -Label "handoff script F47 investor route evidence refresh audit marker"
Assert-Contains -Text $handoffScript -Needle 'F48 implement post-F47 PC controlled-demo investor route evidence audit fixes' -Label "handoff script F48 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'check_pc_controlled_demo_investor_route_evidence_audit_fixes.ps1' -Label "handoff script F48 investor route evidence audit fixes script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo investor route evidence audit fixes check OK' -Label "handoff script F48 investor route evidence audit fixes marker"
Assert-Contains -Text $handoffScript -Needle 'F49 refresh PC controlled-demo investor route evidence after audit fixes' -Label "handoff script F49 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh.ps1' -Label "handoff script F49 investor route evidence audit fix refresh script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo investor route evidence audit fix refresh check OK' -Label "handoff script F49 investor route evidence audit fix refresh marker"
Assert-Contains -Text $handoffScript -Needle 'F50 audit post-F49 PC controlled-demo investor route evidence refresh' -Label "handoff script F50 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh.ps1' -Label "handoff script F50 investor route evidence audit fix refresh audit script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo investor route evidence audit fix refresh audit OK' -Label "handoff script F50 investor route evidence audit fix refresh audit marker"
Assert-Contains -Text $handoffScript -Needle 'F51 implement post-F50 PC controlled-demo investor route evidence refresh audit fixes' -Label "handoff script F51 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fixes.ps1' -Label "handoff script F51 investor route evidence audit fix refresh audit fixes script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo investor route evidence audit fix refresh audit fixes check OK' -Label "handoff script F51 investor route evidence audit fix refresh audit fixes marker"
Assert-Contains -Text $handoffScript -Needle 'F52 refresh PC controlled-demo investor route evidence after F50 audit fixes' -Label "handoff script F52 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh.ps1' -Label "handoff script F52 investor route evidence audit fix refresh audit fix refresh script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo investor route evidence audit fix refresh audit fix refresh check OK' -Label "handoff script F52 investor route evidence audit fix refresh audit fix refresh marker"
Assert-Contains -Text $handoffScript -Needle 'F53 audit post-F52 PC controlled-demo investor route evidence refresh' -Label "handoff script F53 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh.ps1' -Label "handoff script F53 investor route evidence audit fix refresh audit fix refresh audit script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit OK' -Label "handoff script F53 investor route evidence audit fix refresh audit fix refresh audit marker"
Assert-Contains -Text $handoffScript -Needle 'F54 implement post-F53 PC controlled-demo investor route evidence refresh audit fixes' -Label "handoff script F54 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1' -Label "handoff script F54 investor route evidence audit fix refresh audit fix refresh audit fixes script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fixes check OK' -Label "handoff script F54 investor route evidence audit fix refresh audit fix refresh audit fixes marker"
Assert-Contains -Text $handoffScript -Needle 'F55 refresh PC controlled-demo investor route evidence after F53 audit fixes' -Label "handoff script F55 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1' -Label "handoff script F55 investor route evidence audit fix refresh audit fix refresh audit fix refresh script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh check OK' -Label "handoff script F55 investor route evidence audit fix refresh audit fix refresh audit fix refresh marker"
Assert-Contains -Text $handoffScript -Needle 'F56 audit post-F55 PC controlled-demo investor route evidence refresh' -Label "handoff script F56 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1' -Label "handoff script F56 investor route evidence audit fix refresh audit fix refresh audit fix refresh audit script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit OK' -Label "handoff script F56 investor route evidence audit fix refresh audit fix refresh audit fix refresh audit marker"
Assert-Contains -Text $handoffScript -Needle 'F57 implement post-F56 PC controlled-demo investor route evidence refresh audit fixes' -Label "handoff script F57 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1' -Label "handoff script F57 investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fixes script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fixes check OK' -Label "handoff script F57 investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fixes marker"
Assert-Contains -Text $handoffScript -Needle 'F58 refresh PC controlled-demo investor route evidence after F56 audit fixes' -Label "handoff script F58 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1' -Label "handoff script F58 investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh check OK' -Label "handoff script F58 investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh marker"
Assert-Contains -Text $handoffScript -Needle 'F59 audit post-F58 PC controlled-demo investor route evidence refresh' -Label "handoff script F59 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit.ps1' -Label "handoff script F59 investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit OK' -Label "handoff script F59 investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit marker"
Assert-Contains -Text $handoffScript -Needle 'F60 implement post-F59 PC controlled-demo investor route evidence refresh audit fixes' -Label "handoff script F60 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fixes.ps1' -Label "handoff script F60 investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes check OK' -Label "handoff script F60 investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fixes marker"
Assert-Contains -Text $handoffScript -Needle 'F61 refresh PC controlled-demo investor route evidence after F59 audit fixes' -Label "handoff script F61 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'check_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh.ps1' -Label "handoff script F61 investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh check OK' -Label "handoff script F61 investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh marker"
Assert-Contains -Text $handoffScript -Needle 'F62 audit post-F61 PC controlled-demo investor route evidence refresh' -Label "handoff script F62 completed task marker"
Assert-Contains -Text $handoffScript -Needle 'audit_pc_controlled_demo_investor_route_evidence_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit_fix_refresh_audit.ps1' -Label "handoff script F62 investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit script marker"
Assert-Contains -Text $handoffScript -Needle 'PC controlled-demo investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit OK' -Label "handoff script F62 investor route evidence audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit fix refresh audit marker"
Assert-Contains -Text $handoffScript -Needle 'F63 implement post-F62 PC controlled-demo investor route evidence refresh audit fixes' -Label "handoff script F63 next task marker"
Assert-Contains -Text $handoffScript -Needle 'Server-backed receipt slice plan check OK' -Label "handoff script server-backed receipt slice marker"
Assert-Contains -Text $handoffScript -Needle 'Server-backed receipt evidence capture OK' -Label "handoff script server-backed receipt evidence marker"
Assert-Contains -Text $handoffScript -Needle 'Post-receipt inventory refresh boundary check OK' -Label "handoff script post-receipt inventory refresh marker"
Assert-Contains -Text $handoffScript -Needle 'Post-receipt inventory refresh binding check OK' -Label "handoff script post-receipt inventory refresh binding marker"
Assert-Contains -Text $handoffScript -Needle 'Post-receipt refresh evidence capture OK' -Label "handoff script post-receipt refresh evidence marker"

$tracked = @(& git -C $RepoRoot ls-files 2>$null | ForEach-Object { $_.ToString() })
if ($LASTEXITCODE -ne 0) {
    throw "git ls-files failed with exit code $LASTEXITCODE"
}

$staged = @(& git -C $RepoRoot diff --cached --name-only --diff-filter=ACMR 2>$null | ForEach-Object { $_.ToString() })
if ($LASTEXITCODE -ne 0) {
    throw "git diff --cached --name-only failed with exit code $LASTEXITCODE"
}

$sourcePaths = @($tracked + $staged)
$unexpectedPlanCopies = @($sourcePaths | Where-Object {
    $_ -match '(^|/)docs/' -or $_ -match 'current-plan-copy' -or $_ -match 'plan-backup'
})
if ($unexpectedPlanCopies.Count -gt 0) {
    foreach ($path in $unexpectedPlanCopies) {
        Add-Failure "Unexpected plan copy/source path: $path"
    }
}
else {
    Add-Row -Check "tracked/staged plan path shape" -Detail "$($sourcePaths.Count) path(s)"
}

if ($failures.Count -gt 0) {
    Write-Host "Current plan queue consistency check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) current plan queue consistency check(s) failed."
}

Write-Host "Current plan queue consistency check OK."
Write-Host "Repo: $RepoRoot"
$rows | Format-Table -AutoSize

param(
    [string]$LogPath = "",
    [string]$PackageName = "com.DefaultCompany.unitymc2demo",
    [int]$MaxFindings = 20,
    [switch]$SelfTest
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ($MaxFindings -lt 1) {
    throw "MaxFindings must be at least 1."
}

function Find-AndroidSmokeLogFindings {
    param(
        [string[]]$Lines,
        [string]$Package
    )

    $findings = New-Object System.Collections.Generic.List[object]
    $escapedPackage = [regex]::Escape($Package)
    $patterns = @(
        @{ Code = "fatal-exception"; Regex = "FATAL EXCEPTION" },
        @{ Code = "fatal-signal"; Regex = "Fatal signal" },
        @{ Code = "sigsegv"; Regex = "\bSIGSEGV\b" },
        @{ Code = "sigabrt"; Regex = "\bSIGABRT\b" },
        @{ Code = "anr"; Regex = "ANR in $escapedPackage" },
        @{ Code = "process-died"; Regex = "Process $escapedPackage .*has died" },
        @{ Code = "force-finish"; Regex = "Force finishing activity .*?$escapedPackage" },
        @{ Code = "unity-crash"; Regex = "\bUnity\b.*\bCrash" }
    )

    for ($index = 0; $index -lt $Lines.Count; $index++) {
        $line = $Lines[$index]
        foreach ($pattern in $patterns) {
            if ($line -match $pattern.Regex) {
                [void]$findings.Add([pscustomobject]@{
                    Line = $index + 1
                    Code = $pattern.Code
                    Text = $line.Trim()
                })
                break
            }
        }
    }

    return $findings
}

if ($SelfTest) {
    $cleanLines = @(
        "06-12 12:00:00.000 I Unity   : MC2 mobile smoke launch",
        "06-12 12:00:01.000 I ActivityTaskManager: Displayed com.DefaultCompany.unitymc2demo/com.unity3d.player.UnityPlayerGameActivity"
    )
    $badLines = @(
        "06-12 12:00:02.000 E AndroidRuntime: FATAL EXCEPTION: UnityMain",
        "06-12 12:00:02.010 E AndroidRuntime: Process: com.DefaultCompany.unitymc2demo, PID: 1001"
    )

    $cleanFindings = @(Find-AndroidSmokeLogFindings -Lines $cleanLines -Package $PackageName)
    $badFindings = @(Find-AndroidSmokeLogFindings -Lines $badLines -Package $PackageName)

    if ($cleanFindings.Count -ne 0) {
        throw "Android smoke log self-test failed: clean sample reported findings."
    }

    if ($badFindings.Count -eq 0) {
        throw "Android smoke log self-test failed: crash sample was not detected."
    }

    Write-Host "Android smoke log check self-test OK."
    return
}

if ([string]::IsNullOrWhiteSpace($LogPath)) {
    throw "LogPath is required unless -SelfTest is used."
}

if (-not (Test-Path -LiteralPath $LogPath)) {
    throw "Android smoke log missing: $LogPath"
}

$lines = @(Get-Content -LiteralPath $LogPath)
$findings = @(Find-AndroidSmokeLogFindings -Lines $lines -Package $PackageName)

if ($findings.Count -gt 0) {
    Write-Host "Android smoke log check failed."
    $findings | Select-Object -First $MaxFindings | Format-Table -AutoSize
    throw "$($findings.Count) Android smoke log crash marker(s) found."
}

Write-Host "Android smoke log check OK."
Write-Host "Log: $LogPath"
Write-Host "Package: $PackageName"
Write-Host "Lines: $($lines.Count)"

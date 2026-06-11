param(
    [string]$RepoRoot = "",
    [string]$ApkPath = "",
    [string[]]$ExpectedAbis = @("arm64-v8a"),
    [string[]]$RequiredEntries = @(
        "AndroidManifest.xml",
        "classes.dex",
        "resources.arsc",
        "lib/arm64-v8a/libc++_shared.so",
        "lib/arm64-v8a/libgame.so",
        "lib/arm64-v8a/libil2cpp.so",
        "lib/arm64-v8a/libmain.so",
        "lib/arm64-v8a/libunity.so",
        "assets/bin/Data/Managed/Metadata/global-metadata.dat",
        "assets/bin/Data/RuntimeInitializeOnLoads.json",
        "assets/bin/Data/ScriptingAssemblies.json",
        "assets/bin/Data/boot.config",
        "assets/bin/Data/globalgamemanagers",
        "assets/bin/Data/level0",
        "assets/bin/Data/unity_app_guid"
    )
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
else {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
}

if ([string]::IsNullOrWhiteSpace($ApkPath)) {
    $ApkPath = Join-Path $RepoRoot "unity-mc2-demo\Builds\Android\MC2UnityDemo.apk"
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

function Compare-Set {
    param(
        [string[]]$Actual,
        [string[]]$Expected
    )

    $actualSorted = @($Actual | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Sort-Object)
    $expectedSorted = @($Expected | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Sort-Object)

    return [pscustomobject]@{
        Matches = (($actualSorted -join "|") -eq ($expectedSorted -join "|"))
        Actual = $actualSorted
        Expected = $expectedSorted
    }
}

function Get-ApkPayloadFacts {
    param([string]$Apk)

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::OpenRead($Apk)
    try {
        $entries = @($zip.Entries | ForEach-Object { $_.FullName })
        $entrySet = New-Object 'System.Collections.Generic.HashSet[string]'
        foreach ($entry in $entries) {
            [void]$entrySet.Add($entry)
        }

        $abis = @(
            $entries |
                Where-Object { $_ -match "^lib/([^/]+)/" } |
                ForEach-Object { [regex]::Match($_, "^lib/([^/]+)/").Groups[1].Value } |
                Sort-Object -Unique
        )

        $dataEntries = @($entries | Where-Object { $_ -like "assets/bin/Data/*" })
        $nativeEntries = @($entries | Where-Object { $_ -like "lib/*" })

        return [pscustomobject]@{
            Entries = $entrySet
            Abis = $abis
            DataEntryCount = $dataEntries.Count
            NativeEntryCount = $nativeEntries.Count
        }
    }
    finally {
        $zip.Dispose()
    }
}

if (-not (Test-Path -LiteralPath $ApkPath -PathType Leaf)) {
    Add-Failure "Missing Android APK: $ApkPath"
}

if (Test-Path -LiteralPath $ApkPath -PathType Leaf) {
    $facts = Get-ApkPayloadFacts -Apk $ApkPath

    $abiComparison = Compare-Set -Actual $facts.Abis -Expected $ExpectedAbis
    if (-not $abiComparison.Matches) {
        Add-Failure "APK ABI folders mismatch. Expected $($abiComparison.Expected -join ', '), got $($abiComparison.Actual -join ', ')."
    }
    else {
        Add-Row -Check "ABI folders" -Detail ($abiComparison.Actual -join ", ")
    }

    $missingEntries = New-Object System.Collections.Generic.List[string]
    foreach ($entry in $RequiredEntries) {
        if (-not $facts.Entries.Contains($entry)) {
            [void]$missingEntries.Add($entry)
        }
    }

    if ($missingEntries.Count -gt 0) {
        Add-Failure "APK missing required payload entries: $($missingEntries -join ', ')."
    }
    else {
        Add-Row -Check "required entries" -Detail "$($RequiredEntries.Count) entry(s)"
    }

    if ($facts.DataEntryCount -lt 8) {
        Add-Failure "APK Unity Data payload looks too small: $($facts.DataEntryCount) assets/bin/Data entry(s)."
    }
    else {
        Add-Row -Check "Unity Data entries" -Detail "$($facts.DataEntryCount) entry(s)"
    }

    if ($facts.NativeEntryCount -lt 5) {
        Add-Failure "APK native payload looks too small: $($facts.NativeEntryCount) lib entry(s)."
    }
    else {
        Add-Row -Check "native entries" -Detail "$($facts.NativeEntryCount) entry(s)"
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Android APK payload check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) Android APK payload check(s) failed."
}

Write-Host "Android APK payload check OK."
Write-Host "Repo: $RepoRoot"
Write-Host "APK: $ApkPath"
Write-Host "ExpectedAbis: $($ExpectedAbis -join ', ')"
Write-Host "RequiredEntries: $($RequiredEntries.Count)"
$rows | Format-Table -AutoSize

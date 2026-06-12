param(
    [string]$RepoRoot = "",
    [string]$SummaryPath = "",
    [string]$ExpectedPackageName = "com.DefaultCompany.unitymc2demo",
    [switch]$SelfTest
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}
else {
    $RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
}

if ([string]::IsNullOrWhiteSpace($SummaryPath)) {
    $SummaryPath = Join-Path $RepoRoot "analysis-output\android-device-smoke-summary.json"
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

function Test-HasProperty {
    param(
        [object]$Object,
        [string]$Name
    )

    return $null -ne $Object.PSObject.Properties[$Name]
}

function Get-PropertyValue {
    param(
        [object]$Object,
        [string]$Name
    )

    if (-not (Test-HasProperty -Object $Object -Name $Name)) {
        Add-Failure "Summary missing field: $Name"
        return $null
    }

    return $Object.PSObject.Properties[$Name].Value
}

function Assert-NotBlank {
    param(
        [object]$Value,
        [string]$Name
    )

    if ($null -eq $Value -or [string]::IsNullOrWhiteSpace([string]$Value)) {
        Add-Failure "Summary field must not be blank: $Name"
    }
}

function Assert-Boolean {
    param(
        [object]$Value,
        [string]$Name
    )

    if ($null -eq $Value -or -not ($Value -is [bool])) {
        Add-Failure "Summary field must be Boolean: $Name"
    }
}

function Assert-SummaryObject {
    param(
        [object]$Summary,
        [string]$Source
    )

    if ($null -eq $Summary) {
        Add-Failure "Summary object is null: $Source"
        return
    }

    $requiredFields = @(
        "result",
        "timestampUtc",
        "deviceId",
        "model",
        "androidVersion",
        "packageName",
        "activityName",
        "process",
        "apkPath",
        "logPath",
        "screenshotPath",
        "commandFileSmoke",
        "commandFilePath",
        "deviceCommandFilePath",
        "unityArguments",
        "smokeTestPassed",
        "launchWaitSeconds",
        "installed",
        "launched",
        "logChecked",
        "screenshotCaptured"
    )

    foreach ($field in $requiredFields) {
        [void](Get-PropertyValue -Object $Summary -Name $field)
    }

    if ($failures.Count -gt 0) {
        return
    }

    if ($Summary.result -ne "completed") {
        Add-Failure "Summary result must be completed: $($Summary.result)"
    }

    Assert-NotBlank -Value $Summary.timestampUtc -Name "timestampUtc"
    try {
        [void][DateTimeOffset]::Parse(
            [string]$Summary.timestampUtc,
            [Globalization.CultureInfo]::InvariantCulture,
            [Globalization.DateTimeStyles]::RoundtripKind
        )
    }
    catch {
        Add-Failure "Summary timestampUtc is not a round-trip timestamp: $($Summary.timestampUtc)"
    }

    Assert-NotBlank -Value $Summary.deviceId -Name "deviceId"
    Assert-NotBlank -Value $Summary.model -Name "model"
    Assert-NotBlank -Value $Summary.androidVersion -Name "androidVersion"
    Assert-NotBlank -Value $Summary.packageName -Name "packageName"
    Assert-NotBlank -Value $Summary.apkPath -Name "apkPath"
    Assert-NotBlank -Value $Summary.logPath -Name "logPath"
    Assert-Boolean -Value $Summary.commandFileSmoke -Name "commandFileSmoke"
    Assert-Boolean -Value $Summary.smokeTestPassed -Name "smokeTestPassed"

    if ($Summary.commandFileSmoke -eq $true) {
        Assert-NotBlank -Value $Summary.commandFilePath -Name "commandFilePath"
        Assert-NotBlank -Value $Summary.deviceCommandFilePath -Name "deviceCommandFilePath"
        Assert-NotBlank -Value $Summary.unityArguments -Name "unityArguments"

        if ($Summary.smokeTestPassed -ne $true) {
            Add-Failure "Summary smokeTestPassed must be true when commandFileSmoke is true."
        }

        if ([string]$Summary.unityArguments -notlike "*-mc2CommandFile*") {
            Add-Failure "Summary unityArguments must include -mc2CommandFile: $($Summary.unityArguments)"
        }
    }
    else {
        Assert-NotBlank -Value $Summary.process -Name "process"
    }

    if (-not [string]::IsNullOrWhiteSpace($ExpectedPackageName) -and $Summary.packageName -ne $ExpectedPackageName) {
        Add-Failure "Summary packageName mismatch: expected $ExpectedPackageName, got $($Summary.packageName)"
    }

    $launchWait = 0
    if (-not [int]::TryParse([string]$Summary.launchWaitSeconds, [ref]$launchWait) -or $launchWait -le 0) {
        Add-Failure "Summary launchWaitSeconds must be a positive integer: $($Summary.launchWaitSeconds)"
    }

    Assert-Boolean -Value $Summary.installed -Name "installed"
    Assert-Boolean -Value $Summary.launched -Name "launched"
    Assert-Boolean -Value $Summary.logChecked -Name "logChecked"
    Assert-Boolean -Value $Summary.screenshotCaptured -Name "screenshotCaptured"

    if ($Summary.screenshotCaptured -eq $true) {
        Assert-NotBlank -Value $Summary.screenshotPath -Name "screenshotPath"
    }

    if ($failures.Count -eq 0) {
        Add-Row -Check "summary source" -Detail $Source
        Add-Row -Check "package" -Detail ([string]$Summary.packageName)
        Add-Row -Check "device" -Detail "$($Summary.deviceId) / $($Summary.model) / Android $($Summary.androidVersion)"
        Add-Row -Check "evidence paths" -Detail "log=$($Summary.logPath); screenshot=$($Summary.screenshotPath)"
        Add-Row -Check "command-file smoke" -Detail "enabled=$($Summary.commandFileSmoke); passed=$($Summary.smokeTestPassed)"
        Add-Row -Check "execution flags" -Detail "installed=$($Summary.installed); launched=$($Summary.launched); logChecked=$($Summary.logChecked); screenshotCaptured=$($Summary.screenshotCaptured)"
    }
}

if ($SelfTest) {
    $sample = [pscustomobject]@{
        result = "completed"
        timestampUtc = [DateTime]::UtcNow.ToString("o")
        deviceId = "selftest-device"
        model = "SelfTest Phone"
        androidVersion = "15"
        packageName = $ExpectedPackageName
        activityName = "com.unity3d.player.UnityPlayerGameActivity"
        process = "12345"
        apkPath = "unity-mc2-demo\Builds\Android\MC2UnityDemo.apk"
        logPath = "analysis-output\android-device-smoke.log"
        screenshotPath = "analysis-output\android-device-smoke.png"
        commandFileSmoke = $true
        commandFilePath = "unity-mc2-demo\Assets\StreamingAssets\CommanderScripts\mc2_01-visible-flow-audit.txt"
        deviceCommandFilePath = "/sdcard/Android/data/com.DefaultCompany.unitymc2demo/files/mc2_01-visible-flow-audit.txt"
        unityArguments = "-mc2CommandFile /sdcard/Android/data/com.DefaultCompany.unitymc2demo/files/mc2_01-visible-flow-audit.txt"
        smokeTestPassed = $true
        launchWaitSeconds = 12
        installed = $true
        launched = $true
        logChecked = $true
        screenshotCaptured = $true
    }

    Assert-SummaryObject -Summary $sample -Source "self-test"
}
else {
    if (-not (Test-Path -LiteralPath $SummaryPath)) {
        Add-Failure "Summary file missing: $SummaryPath"
    }
    else {
        try {
            $summary = Get-Content -LiteralPath $SummaryPath -Raw | ConvertFrom-Json
            Assert-SummaryObject -Summary $summary -Source $SummaryPath
        }
        catch {
            Add-Failure "Summary file is not valid JSON: $SummaryPath :: $($_.Exception.Message)"
        }
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Android smoke summary check failed."
    foreach ($failure in $failures) {
        Write-Host " - $failure"
    }

    throw "$($failures.Count) Android smoke summary check(s) failed."
}

Write-Host "Android smoke summary check OK."
Write-Host "Repo: $RepoRoot"
if (-not $SelfTest) {
    Write-Host "Summary: $SummaryPath"
}
$rows | Format-Table -AutoSize

if ($SelfTest) {
    Write-Host "Android smoke summary check self-test OK."
}

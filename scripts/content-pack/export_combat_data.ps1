param(
    [Parameter(Mandatory = $false)]
    [string]$PackPath = "",

    [Parameter(Mandatory = $false)]
    [string]$OutputRoot = "",

    [Parameter(Mandatory = $false)]
    [string]$UnityOutputPath = "",

    [Parameter(Mandatory = $false)]
    [string]$UnpackRoot = "",

    [Parameter(Mandatory = $false)]
    [string]$MakeFstPath = "",

    [Parameter(Mandatory = $false)]
    [string]$PakToolPath = ""
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
$validateScript = Join-Path $PSScriptRoot "validate_content_pack.ps1"

if ([string]::IsNullOrWhiteSpace($PackPath)) {
    $linkedDevPack = Join-Path $repoRoot "content-packs\project-owned-linked-dev"
    if (Test-Path -LiteralPath $linkedDevPack -PathType Container) {
        $PackPath = $linkedDevPack
    }
    else {
        $PackPath = Join-Path $repoRoot "content-packs\mc2-original.local.example.json"
    }
}

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot "analysis-output\unity-combat-data"
}

if ([string]::IsNullOrWhiteSpace($UnityOutputPath)) {
    $UnityOutputPath = Join-Path $repoRoot "unity-mc2-demo\Assets\StreamingAssets\Data\combat-data.json"
}

if ([string]::IsNullOrWhiteSpace($UnpackRoot)) {
    $UnpackRoot = Join-Path $repoRoot "analysis-output\fst-unpack"
}

if ([string]::IsNullOrWhiteSpace($MakeFstPath)) {
    $MakeFstPath = Join-Path $repoRoot "build64\out\data_tools\Release\makefst.exe"
}

if ([string]::IsNullOrWhiteSpace($PakToolPath)) {
    $PakToolPath = Join-Path $repoRoot "build64\out\data_tools\Release\pak.exe"
}

function Resolve-PackRoot {
    param(
        [Parameter(Mandatory = $true)]
        [string]$InputPath
    )

    if (Test-Path -LiteralPath $InputPath -PathType Leaf) {
        $manifestFile = (Resolve-Path -LiteralPath $InputPath).Path
        $manifestDir = Split-Path -Parent $manifestFile
        $manifest = Get-Content -LiteralPath $manifestFile -Raw | ConvertFrom-Json
        if ([string]::IsNullOrWhiteSpace($manifest.sourcePath)) {
            throw "Manifest does not include sourcePath: $manifestFile"
        }

        if ([System.IO.Path]::IsPathRooted($manifest.sourcePath)) {
            return (Resolve-Path -LiteralPath $manifest.sourcePath).Path
        }

        return (Resolve-Path -LiteralPath (Join-Path $manifestDir $manifest.sourcePath)).Path
    }

    if (Test-Path -LiteralPath $InputPath -PathType Container) {
        return (Resolve-Path -LiteralPath $InputPath).Path
    }

    throw "PackPath does not exist: $InputPath"
}

function Read-PackManifest {
    param(
        [Parameter(Mandatory = $true)]
        [string]$InputPath,

        [Parameter(Mandatory = $true)]
        [string]$PackRoot
    )

    if (Test-Path -LiteralPath $InputPath -PathType Leaf) {
        return Get-Content -LiteralPath (Resolve-Path -LiteralPath $InputPath).Path -Raw | ConvertFrom-Json
    }

    $packManifest = Join-Path $PackRoot "pack.json"
    if (Test-Path -LiteralPath $packManifest -PathType Leaf) {
        return Get-Content -LiteralPath $packManifest -Raw | ConvertFrom-Json
    }

    return $null
}

function Import-LooseCsv {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $false)]
        [int]$ColumnCount = 48
    )

    $headers = 0..($ColumnCount - 1) | ForEach-Object { "c$_" }
    return @(Import-Csv -LiteralPath $Path -Header $headers)
}

function Convert-ToFloat {
    param(
        [Parameter(Mandatory = $false)]
        [AllowNull()]
        [string]$Value,

        [Parameter(Mandatory = $false)]
        [double]$Default = 0
    )

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return $Default
    }

    $clean = $Value.Trim()
    if ($clean -match "^(NA|N/A|na)$") {
        return $Default
    }

    $parsed = 0.0
    if ([double]::TryParse(
            $clean,
            [System.Globalization.NumberStyles]::Float,
            [System.Globalization.CultureInfo]::InvariantCulture,
            [ref]$parsed)) {
        return $parsed
    }

    return $Default
}

function Convert-ToInt {
    param(
        [Parameter(Mandatory = $false)]
        [AllowNull()]
        [string]$Value,

        [Parameter(Mandatory = $false)]
        [int]$Default = 0
    )

    return [int](Convert-ToFloat -Value $Value -Default $Default)
}

function Round-Number {
    param(
        [Parameter(Mandatory = $true)]
        [double]$Value,

        [Parameter(Mandatory = $false)]
        [int]$Digits = 2
    )

    return [Math]::Round($Value, $Digits)
}

function Get-RowValue {
    param(
        [Parameter(Mandatory = $true)]
        [object[]]$Rows,

        [Parameter(Mandatory = $true)]
        [string]$Key,

        [Parameter(Mandatory = $true)]
        [string]$Column
    )

    $row = $Rows | Where-Object { $_.c0 -eq $Key } | Select-Object -First 1
    if ($null -eq $row) {
        return $null
    }

    return $row.$Column
}

function Get-CurrentArmor {
    param(
        [Parameter(Mandatory = $true)]
        [object[]]$Rows
    )

    $row = $Rows | Where-Object { $_.c0 -eq "CurrentArmorPoints" } | Select-Object -First 1
    if ($null -eq $row) {
        $row = $Rows | Where-Object { $_.c0 -eq "BaseArmorPoints" } | Select-Object -First 1
    }

    if ($null -eq $row) {
        return @{}
    }

    return [ordered]@{
        head = Convert-ToFloat $row.c1
        leftArm = Convert-ToFloat $row.c2
        rightArm = Convert-ToFloat $row.c3
        leftTorso = Convert-ToFloat $row.c4
        rightTorso = Convert-ToFloat $row.c5
        centerTorso = Convert-ToFloat $row.c6
        leftLeg = Convert-ToFloat $row.c7
        rightLeg = Convert-ToFloat $row.c8
        rearLeftTorso = Convert-ToFloat $row.c9
        rearRightTorso = Convert-ToFloat $row.c10
        rearCenterTorso = Convert-ToFloat $row.c11
    }
}

function Get-InternalStructure {
    param(
        [Parameter(Mandatory = $true)]
        [object[]]$Rows
    )

    $row = $Rows | Where-Object { $_.c0 -eq "BaseInternalStructure" } | Select-Object -First 1
    if ($null -eq $row) {
        return @{}
    }

    return [ordered]@{
        head = Convert-ToFloat $row.c1
        leftArm = Convert-ToFloat $row.c2
        rightArm = Convert-ToFloat $row.c3
        leftTorso = Convert-ToFloat $row.c4
        rightTorso = Convert-ToFloat $row.c5
        centerTorso = Convert-ToFloat $row.c6
        leftLeg = Convert-ToFloat $row.c7
        rightLeg = Convert-ToFloat $row.c8
    }
}

function Sum-Values {
    param(
        [Parameter(Mandatory = $true)]
        [hashtable]$Map
    )

    $sum = 0.0
    foreach ($value in $Map.Values) {
        $sum += [double]$value
    }

    return $sum
}

function Build-SectionProfile {
    param(
        [Parameter(Mandatory = $true)]
        [hashtable]$Armor,

        [Parameter(Mandatory = $true)]
        [hashtable]$Internal
    )

    $head = $Armor.head + $Internal.head
    $torso = $Armor.centerTorso + $Armor.leftTorso + $Armor.rightTorso +
        $Armor.rearLeftTorso + $Armor.rearRightTorso + $Armor.rearCenterTorso +
        $Internal.centerTorso + $Internal.leftTorso + $Internal.rightTorso
    $leftArm = $Armor.leftArm + $Internal.leftArm
    $rightArm = $Armor.rightArm + $Internal.rightArm
    $legs = $Armor.leftLeg + $Armor.rightLeg + $Internal.leftLeg + $Internal.rightLeg

    return @(
        [ordered]@{ name = "Cockpit"; structure = Round-Number $head; critical = $true },
        [ordered]@{ name = "Torso"; structure = Round-Number $torso; critical = $false },
        [ordered]@{ name = "Left Arm"; structure = Round-Number $leftArm; critical = $false },
        [ordered]@{ name = "Right Arm"; structure = Round-Number $rightArm; critical = $false },
        [ordered]@{ name = "Legs"; structure = Round-Number $legs; critical = $false }
    )
}

function Get-RangeMax {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Range
    )

    switch ($Range.ToLowerInvariant()) {
        "short" { return 100.0 }
        "medium" { return 150.0 }
        "long" { return 225.0 }
        default { return 100.0 }
    }
}

function Read-ComponentMap {
    param(
        [Parameter(Mandatory = $true)]
        [string]$CompbasPath
    )

    $map = @{}
    foreach ($row in (Import-LooseCsv -Path $CompbasPath)) {
        if ($row.c0 -notmatch "^\d+$") {
            continue
        }

        $id = Convert-ToInt $row.c0 -Default -1
        if ($id -lt 0) {
            continue
        }

        $component = [ordered]@{
            id = $id
            type = $row.c1
            name = $row.c2
            criticalSlots = Convert-ToFloat $row.c3
            recycleTime = Convert-ToFloat $row.c4
            heat = Convert-ToFloat $row.c5
            weight = Convert-ToFloat $row.c6
            damage = Convert-ToFloat $row.c7
            battleRating = Convert-ToFloat $row.c8
            price = Convert-ToFloat $row.c9
            rangeBand = if ([string]::IsNullOrWhiteSpace($row.c10)) { "" } else { $row.c10.Trim() }
            ammoType = $row.c19
            flags = Convert-ToInt $row.c20
            specialEffect = Convert-ToInt $row.c21
            ammoMasterId = Convert-ToInt $row.c22
        }

        $map[$id] = $component
    }

    return $map
}

function Read-MechProfile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$CsvPath,

        [Parameter(Mandatory = $true)]
        [hashtable]$Components
    )

    $rows = Import-LooseCsv -Path $CsvPath
    $unitType = Get-RowValue -Rows $rows -Key "MechName" -Column "c1"
    if ([string]::IsNullOrWhiteSpace($unitType)) {
        $unitType = [System.IO.Path]::GetFileNameWithoutExtension($CsvPath)
    }

    $armor = Get-CurrentArmor -Rows $rows
    $internal = Get-InternalStructure -Rows $rows
    $sections = Build-SectionProfile -Armor $armor -Internal $internal
    $maxStructure = ($sections | ForEach-Object { [double]$_.structure } | Measure-Object -Sum).Sum

    $weapons = @()
    $armorPlateCount = 0
    foreach ($row in ($rows | Where-Object { $_.c0 -match "^Item\d+MasterId$" })) {
        $componentId = Convert-ToInt $row.c4 -Default 255
        if (-not $Components.ContainsKey($componentId)) {
            continue
        }

        $component = $Components[$componentId]
        if ($component.type -eq "Bulk" -or $component.name -eq "Armor Plating") {
            $armorPlateCount++
        }

        if ($component.type -notmatch "Weapon") {
            continue
        }

        $damage = Convert-ToFloat $row.c9 -Default $component.damage
        $damagePerTenSeconds = Convert-ToFloat $row.c10 -Default 0
        if ($damagePerTenSeconds -le 0 -and $component.recycleTime -gt 0) {
            $damagePerTenSeconds = ($damage * 10.0) / $component.recycleTime
        }

        $rangeBand = if ([string]::IsNullOrWhiteSpace($row.c6) -or $row.c6 -match "^(NA|N/A)$") {
            $component.rangeBand
        }
        else {
            $row.c6.Trim()
        }

        $weapons += [ordered]@{
            componentId = $componentId
            name = if ([string]::IsNullOrWhiteSpace($row.c5)) { $component.name } else { $row.c5.Trim() }
            type = $component.type
            rangeBand = $rangeBand
            rangeMax = Get-RangeMax -Range $rangeBand
            recycleTime = Round-Number $component.recycleTime
            heat = Round-Number (Convert-ToFloat $row.c7 -Default $component.heat)
            weight = Round-Number (Convert-ToFloat $row.c8 -Default $component.weight)
            damage = Round-Number $damage
            damagePerTenSeconds = Round-Number $damagePerTenSeconds
            battleRating = Convert-ToFloat $row.c11 -Default $component.battleRating
            price = Convert-ToFloat $row.c12 -Default $component.price
            ammoMasterId = $component.ammoMasterId
            specialEffect = $component.specialEffect
        }
    }

    $totalDamagePerTenSeconds = ($weapons | ForEach-Object { [double]$_.damagePerTenSeconds } | Measure-Object -Sum).Sum
    $maxWeaponRange = ($weapons | ForEach-Object { [double]$_.rangeMax } | Measure-Object -Maximum).Maximum
    if ($null -eq $maxWeaponRange -or $maxWeaponRange -le 0) {
        $maxWeaponRange = 100.0
    }

    $weaponCooldown = 3.0
    $weaponDamage = [Math]::Max(4.0, ($totalDamagePerTenSeconds / 10.0) * $weaponCooldown)
    $maxRunSpeed = Convert-ToFloat (Get-RowValue -Rows $rows -Key "EncyclopediaID" -Column "c4")
    $moveSpeed = [Math]::Max(80.0, $maxRunSpeed * 9.0)
    $maxStructure += ($armorPlateCount * 8.0)

    return [ordered]@{
        unitType = $unitType
        source = "misc.fst:data/objects/" + [System.IO.Path]::GetFileName($CsvPath)
        sourceKind = "mc2-mech-csv"
        variantName = Get-RowValue -Rows $rows -Key "VariantName" -Column "c1"
        tonnage = Convert-ToFloat (Get-RowValue -Rows $rows -Key "MechName" -Column "c4")
        heatIndex = Convert-ToFloat (Get-RowValue -Rows $rows -Key "HouseID" -Column "c4")
        loadIndex = Convert-ToFloat (Get-RowValue -Rows $rows -Key "DescIndex" -Column "c4")
        totalArmor = Convert-ToFloat (Get-RowValue -Rows $rows -Key "AbbrIndex" -Column "c4")
        maxArmor = Convert-ToFloat (Get-RowValue -Rows $rows -Key "MechParts" -Column "c4")
        maxRunSpeed = Round-Number $maxRunSpeed
        chassisBattleRating = Convert-ToFloat (Get-RowValue -Rows $rows -Key "Animation" -Column "c4")
        armorPlateCount = $armorPlateCount
        armor = $armor
        internalStructure = $internal
        sections = $sections
        weapons = $weapons
        combatProfile = [ordered]@{
            maxStructure = Round-Number $maxStructure
            moveSpeed = Round-Number $moveSpeed
            weaponRange = Round-Number $maxWeaponRange
            weaponDamage = Round-Number $weaponDamage
            weaponCooldown = Round-Number $weaponCooldown
            sourceDamagePerTenSeconds = Round-Number $totalDamagePerTenSeconds
        }
    }
}

function Read-FitSections {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $sections = @{}
    $currentSection = ""
    foreach ($rawLine in Get-Content -LiteralPath $Path) {
        $line = ($rawLine -replace "//.*$", "").Trim()
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        if ($line -match "^\[(.+)\]$") {
            $currentSection = $matches[1].Trim()
            if (-not $sections.ContainsKey($currentSection)) {
                $sections[$currentSection] = @{}
            }
            continue
        }

        if ($line -match "^(?:[A-Za-z]+\s+)?([A-Za-z0-9_]+)\s*=\s*(.+)$") {
            if (-not $sections.ContainsKey($currentSection)) {
                $sections[$currentSection] = @{}
            }

            $key = $matches[1].Trim()
            $value = $matches[2].Trim().Trim('"')
            $sections[$currentSection][$key] = $value
        }
    }

    return $sections
}

function Get-FitValue {
    param(
        [Parameter(Mandatory = $true)]
        [hashtable]$Sections,

        [Parameter(Mandatory = $true)]
        [string]$Section,

        [Parameter(Mandatory = $true)]
        [string]$Key,

        [Parameter(Mandatory = $false)]
        [string]$Default = ""
    )

    if ($Sections.ContainsKey($Section) -and $Sections[$Section].ContainsKey($Key)) {
        return $Sections[$Section][$Key]
    }

    return $Default
}

function Build-VehicleSections {
    param(
        [Parameter(Mandatory = $true)]
        [hashtable]$Fit
    )

    $sectionNames = @("Front", "Left", "Right", "Rear", "Turret")
    $sections = @()
    foreach ($sectionName in $sectionNames) {
        $armor = Convert-ToFloat (Get-FitValue -Sections $Fit -Section $sectionName -Key "CurArmorPoints" -Default "0")
        $internal = Convert-ToFloat (Get-FitValue -Sections $Fit -Section $sectionName -Key "CurInternalStructure" -Default "0")
        if (($armor + $internal) -le 0) {
            continue
        }

        $sections += [ordered]@{
            name = $sectionName
            structure = Round-Number ($armor + $internal)
            critical = $false
        }
    }

    return $sections
}

function Read-WeaponFromComponent {
    param(
        [Parameter(Mandatory = $true)]
        [hashtable]$Component
    )

    $damagePerTenSeconds = 0.0
    if ($Component.recycleTime -gt 0) {
        $damagePerTenSeconds = ([double]$Component.damage * 10.0) / [double]$Component.recycleTime
    }

    return [ordered]@{
        componentId = $Component.id
        name = $Component.name
        type = $Component.type
        rangeBand = $Component.rangeBand
        rangeMax = Get-RangeMax -Range $Component.rangeBand
        recycleTime = Round-Number $Component.recycleTime
        heat = Round-Number $Component.heat
        weight = Round-Number $Component.weight
        damage = Round-Number $Component.damage
        damagePerTenSeconds = Round-Number $damagePerTenSeconds
        battleRating = Round-Number $Component.battleRating
        price = Round-Number $Component.price
        ammoMasterId = $Component.ammoMasterId
        specialEffect = $Component.specialEffect
    }
}

function Build-CombatProfileFromWeapons {
    param(
        [Parameter(Mandatory = $true)]
        [object[]]$Sections,

        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [object[]]$Weapons,

        [Parameter(Mandatory = $true)]
        [double]$MoveSpeed
    )

    $maxStructure = ($Sections | ForEach-Object { [double]$_.structure } | Measure-Object -Sum).Sum
    $totalDamagePerTenSeconds = ($Weapons | ForEach-Object { [double]$_.damagePerTenSeconds } | Measure-Object -Sum).Sum
    $maxWeaponRange = ($Weapons | ForEach-Object { [double]$_.rangeMax } | Measure-Object -Maximum).Maximum
    if ($null -eq $maxWeaponRange -or $maxWeaponRange -le 0) {
        $maxWeaponRange = 100.0
    }

    $weaponCooldown = 3.0
    $weaponDamage = [Math]::Max(2.0, ($totalDamagePerTenSeconds / 10.0) * $weaponCooldown)

    return [ordered]@{
        maxStructure = Round-Number $maxStructure
        moveSpeed = Round-Number $MoveSpeed
        weaponRange = Round-Number $maxWeaponRange
        weaponDamage = Round-Number $weaponDamage
        weaponCooldown = Round-Number $weaponCooldown
        sourceDamagePerTenSeconds = Round-Number $totalDamagePerTenSeconds
    }
}

function Read-VehicleProfile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$PacketPath,

        [Parameter(Mandatory = $true)]
        [string]$UnitType,

        [Parameter(Mandatory = $true)]
        [int]$ObjectNumber,

        [Parameter(Mandatory = $true)]
        [hashtable]$Components
    )

    $fit = Read-FitSections -Path $PacketPath
    $sections = Build-VehicleSections -Fit $fit
    if ($sections.Count -eq 0) {
        return $null
    }

    $weapons = @()
    foreach ($sectionName in $fit.Keys | Where-Object { $_ -match "^Item:\d+$" }) {
        $masterId = Convert-ToInt (Get-FitValue -Sections $fit -Section $sectionName -Key "MasterID" -Default "255") -Default 255
        if (-not $Components.ContainsKey($masterId)) {
            continue
        }

        $component = $Components[$masterId]
        if ($component.type -match "Weapon") {
            $weapons += Read-WeaponFromComponent -Component $component
        }
    }

    $moveSpeed = (Convert-ToFloat (Get-FitValue -Sections $fit -Section "VehicleDynamics" -Key "MaxVelocity" -Default "10")) * 9.0
    $objectName = Get-FitValue -Sections $fit -Section "ObjectType" -Key "Name" -Default $UnitType

    return [ordered]@{
        unitType = $UnitType
        source = "object2.pak:packet_$ObjectNumber"
        sourceKind = "mc2-vehicle-fit"
        sourceObjectNumber = $ObjectNumber
        objectName = $objectName
        appearanceName = Get-FitValue -Sections $fit -Section "ObjectType" -Key "AppearanceName" -Default ""
        tonnage = Convert-ToFloat (Get-FitValue -Sections $fit -Section "General" -Key "CurTonnage" -Default "0")
        battleRating = Convert-ToFloat (Get-FitValue -Sections $fit -Section "General" -Key "BattleRating" -Default "0")
        maxVelocity = Convert-ToFloat (Get-FitValue -Sections $fit -Section "VehicleDynamics" -Key "MaxVelocity" -Default "0")
        maxAcceleration = Convert-ToFloat (Get-FitValue -Sections $fit -Section "VehicleDynamics" -Key "MaxAccel" -Default "0")
        explosionDamage = Convert-ToFloat (Get-FitValue -Sections $fit -Section "General" -Key "ExplosionDamage" -Default "0")
        explosionRadius = Convert-ToFloat (Get-FitValue -Sections $fit -Section "General" -Key "ExplosionRadius" -Default "0")
        sections = $sections
        weapons = $weapons
        combatProfile = Build-CombatProfileFromWeapons -Sections $sections -Weapons $weapons -MoveSpeed $moveSpeed
    }
}

function Add-FallbackProfileIfMissing {
    param(
        [Parameter(Mandatory = $true)]
        [ref]$Profiles,

        [Parameter(Mandatory = $true)]
        [string]$UnitType,

        [Parameter(Mandatory = $true)]
        [double]$MaxStructure,

        [Parameter(Mandatory = $true)]
        [double]$MoveSpeed,

        [Parameter(Mandatory = $true)]
        [double]$WeaponRange,

        [Parameter(Mandatory = $true)]
        [double]$WeaponDamage,

        [Parameter(Mandatory = $true)]
        [double]$WeaponCooldown
    )

    $exists = @($Profiles.Value | Where-Object { $_.unitType -eq $UnitType }).Count -gt 0
    if (-not $exists) {
        $Profiles.Value += New-FallbackProfile `
            -UnitType $UnitType `
            -MaxStructure $MaxStructure `
            -MoveSpeed $MoveSpeed `
            -WeaponRange $WeaponRange `
            -WeaponDamage $WeaponDamage `
            -WeaponCooldown $WeaponCooldown
    }
}

function New-FallbackProfile {
    param(
        [Parameter(Mandatory = $true)][string]$UnitType,
        [Parameter(Mandatory = $true)][double]$MaxStructure,
        [Parameter(Mandatory = $true)][double]$MoveSpeed,
        [Parameter(Mandatory = $true)][double]$WeaponRange,
        [Parameter(Mandatory = $true)][double]$WeaponDamage,
        [Parameter(Mandatory = $true)][double]$WeaponCooldown
    )

    return [ordered]@{
        unitType = $UnitType
        source = "demo-fallback"
        sourceKind = "temporary-vehicle-or-infantry-default"
        sections = @(
            [ordered]@{ name = "Cockpit"; structure = Round-Number ($MaxStructure * 0.12); critical = $true },
            [ordered]@{ name = "Torso"; structure = Round-Number ($MaxStructure * 0.34); critical = $false },
            [ordered]@{ name = "Left Arm"; structure = Round-Number ($MaxStructure * 0.17); critical = $false },
            [ordered]@{ name = "Right Arm"; structure = Round-Number ($MaxStructure * 0.17); critical = $false },
            [ordered]@{ name = "Legs"; structure = Round-Number ($MaxStructure * 0.20); critical = $false }
        )
        weapons = @()
        combatProfile = [ordered]@{
            maxStructure = Round-Number $MaxStructure
            moveSpeed = Round-Number $MoveSpeed
            weaponRange = Round-Number $WeaponRange
            weaponDamage = Round-Number $WeaponDamage
            weaponCooldown = Round-Number $WeaponCooldown
            sourceDamagePerTenSeconds = 0
        }
    }
}

$packRoot = Resolve-PackRoot -InputPath $PackPath
& $validateScript -PackPath $packRoot | Out-Host
$manifest = Read-PackManifest -InputPath $PackPath -PackRoot $packRoot
$packId = if ($null -ne $manifest -and $manifest.id) { $manifest.id } else { Split-Path -Leaf $packRoot }

if (-not (Test-Path -LiteralPath $MakeFstPath -PathType Leaf)) {
    throw "makefst.exe is missing: $MakeFstPath"
}

if (-not (Test-Path -LiteralPath $PakToolPath -PathType Leaf)) {
    throw "pak.exe is missing: $PakToolPath"
}

$miscFst = Join-Path $packRoot "misc.fst"
if (-not (Test-Path -LiteralPath $miscFst -PathType Leaf)) {
    throw "misc.fst is missing from pack: $packRoot"
}

$resolvedUnpackRoot = Join-Path (Resolve-Path -LiteralPath (Split-Path -Parent $UnpackRoot)).Path (Split-Path -Leaf $UnpackRoot)
$miscUnpackRoot = Join-Path $resolvedUnpackRoot "misc.fst"
$compbasPath = Join-Path $miscUnpackRoot "data\objects\compbas.csv"

if (-not (Test-Path -LiteralPath $compbasPath -PathType Leaf)) {
    New-Item -ItemType Directory -Path $resolvedUnpackRoot -Force | Out-Null
    Write-Output "Unpacking misc.fst to: $resolvedUnpackRoot"
    Push-Location $packRoot
    try {
        & $MakeFstPath -d -f "misc.fst" -p $resolvedUnpackRoot | Out-Host
    }
    finally {
        Pop-Location
    }
}
else {
    Write-Output "Using existing unpack output: $miscUnpackRoot"
}

if (-not (Test-Path -LiteralPath $compbasPath -PathType Leaf)) {
    throw "compbas.csv not found after unpack: $compbasPath"
}

$artFst = Join-Path $packRoot "art.fst"
if (-not (Test-Path -LiteralPath $artFst -PathType Leaf)) {
    throw "art.fst is missing from pack: $packRoot"
}

$artUnpackRoot = Join-Path $resolvedUnpackRoot "art.fst"
$buildingsPath = Join-Path $artUnpackRoot "data\art\buildings.csv"
if (-not (Test-Path -LiteralPath $buildingsPath -PathType Leaf)) {
    Write-Output "Unpacking art.fst to: $resolvedUnpackRoot"
    Push-Location $packRoot
    try {
        & $MakeFstPath -d -f "art.fst" -p $resolvedUnpackRoot | Out-Host
    }
    finally {
        Pop-Location
    }
}
else {
    Write-Output "Using existing unpack output: $artUnpackRoot"
}

if (-not (Test-Path -LiteralPath $buildingsPath -PathType Leaf)) {
    throw "buildings.csv not found after unpack: $buildingsPath"
}

$objectPak = Join-Path $packRoot "data\objects\object2.pak"
if (-not (Test-Path -LiteralPath $objectPak -PathType Leaf)) {
    throw "object2.pak is missing from pack: $objectPak"
}

$pakUnpackRoot = Join-Path $repoRoot "analysis-output\pak-unpack\object2.pak"
if (-not (Test-Path -LiteralPath (Join-Path $pakUnpackRoot "packet_0") -PathType Leaf)) {
    New-Item -ItemType Directory -Path $pakUnpackRoot -Force | Out-Null
    Write-Output "Unpacking object2.pak to: $pakUnpackRoot"
    & $PakToolPath -d -f $objectPak -p $pakUnpackRoot | Out-Host
}
else {
    Write-Output "Using existing unpack output: $pakUnpackRoot"
}

$components = Read-ComponentMap -CompbasPath $compbasPath
$objectRoot = Join-Path $miscUnpackRoot "data\objects"
$mechCsvNames = @("werewolf.csv", "bushwacker.csv", "starslayer.csv", "urbanmech.csv")
$profiles = @()

foreach ($csvName in $mechCsvNames) {
    $csvPath = Join-Path $objectRoot $csvName
    if (-not (Test-Path -LiteralPath $csvPath -PathType Leaf)) {
        Write-Warning "Skipping missing mech CSV: $csvPath"
        continue
    }

    $profiles += Read-MechProfile -CsvPath $csvPath -Components $components
}

$vehicleRows = @(
    Import-Csv -LiteralPath $buildingsPath |
        Where-Object { $_.Type -eq "VEHICLE" -and $_.FitID -match "^\d+$" } |
        Sort-Object "File Name"
)

foreach ($row in $vehicleRows) {
    $objectNumber = Convert-ToInt $row.FitID -Default -1
    if ($objectNumber -lt 0) {
        continue
    }

    $packetPath = Join-Path $pakUnpackRoot "packet_$objectNumber"
    if (-not (Test-Path -LiteralPath $packetPath -PathType Leaf)) {
        Write-Warning "Skipping vehicle profile with missing packet: $($row.'File Name') packet_$objectNumber"
        continue
    }

    $profile = Read-VehicleProfile `
        -PacketPath $packetPath `
        -UnitType $row.'File Name' `
        -ObjectNumber $objectNumber `
        -Components $components

    if ($null -ne $profile) {
        $profiles += $profile
    }
}

Add-FallbackProfileIfMissing -Profiles ([ref]$profiles) -UnitType "Centipede" -MaxStructure 95 -MoveSpeed 230 -WeaponRange 520 -WeaponDamage 14 -WeaponCooldown 1.25
Add-FallbackProfileIfMissing -Profiles ([ref]$profiles) -UnitType "Harasser" -MaxStructure 70 -MoveSpeed 315 -WeaponRange 480 -WeaponDamage 11 -WeaponCooldown 1.05
Add-FallbackProfileIfMissing -Profiles ([ref]$profiles) -UnitType "LRMC" -MaxStructure 80 -MoveSpeed 185 -WeaponRange 1120 -WeaponDamage 18 -WeaponCooldown 2.15
Add-FallbackProfileIfMissing -Profiles ([ref]$profiles) -UnitType "Infantry" -MaxStructure 32 -MoveSpeed 170 -WeaponRange 360 -WeaponDamage 5 -WeaponCooldown 0.9

$weaponTable = @(
    $components.Values |
        Where-Object { $_.type -match "Weapon" } |
        Sort-Object id |
        ForEach-Object {
            [ordered]@{
                id = $_.id
                name = $_.name
                type = $_.type
                rangeBand = $_.rangeBand
                rangeMax = Get-RangeMax -Range $_.rangeBand
                recycleTime = Round-Number $_.recycleTime
                heat = Round-Number $_.heat
                weight = Round-Number $_.weight
                damage = Round-Number $_.damage
                battleRating = Round-Number $_.battleRating
                price = Round-Number $_.price
                ammoMasterId = $_.ammoMasterId
                specialEffect = $_.specialEffect
            }
        }
)

$contract = [ordered]@{
    schema = "mc2-unity-combat-data-v1"
    createdAt = (Get-Date).ToString("o")
    pack = [ordered]@{
        id = $packId
        title = if ($null -ne $manifest) { $manifest.title } else { $packId }
        root = $packRoot
    }
    source = [ordered]@{
        miscFst = $miscFst
        unpackRoot = $miscUnpackRoot
        artUnpackRoot = $artUnpackRoot
        objectPak = $objectPak
        objectPakUnpackRoot = $pakUnpackRoot
        compbas = $compbasPath
        buildings = $buildingsPath
        note = "Original-derived local validation data. Keep ignored; replace as a whole content pack before distribution."
    }
    rangeBands = @(
        [ordered]@{ id = "short"; min = 0; max = 100 },
        [ordered]@{ id = "medium"; min = 50; max = 150 },
        [ordered]@{ id = "long"; min = 100; max = 225 }
    )
    weapons = $weaponTable
    unitProfiles = $profiles
}

$outputDir = Join-Path $OutputRoot $packId
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
$outputPath = Join-Path $outputDir "combat-data.json"
$json = $contract | ConvertTo-Json -Depth 20
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)
[System.IO.File]::WriteAllText($outputPath, $json, $utf8NoBom)

$unityOutputDir = Split-Path -Parent $UnityOutputPath
New-Item -ItemType Directory -Path $unityOutputDir -Force | Out-Null
[System.IO.File]::WriteAllText($UnityOutputPath, $json, $utf8NoBom)

Write-Output "Combat data exported:"
Write-Output "  Analysis: $outputPath"
Write-Output "  Unity:    $UnityOutputPath"
Write-Output ("  Unit profiles: {0}" -f $profiles.Count)
Write-Output ("  Weapon records: {0}" -f $weaponTable.Count)

param(
    [Parameter(Mandatory = $false)]
    [string]$MissionPakPath = "",

    [Parameter(Mandatory = $false)]
    [string]$ContentIndexPath = "",

    [Parameter(Mandatory = $false)]
    [string]$OutputRoot = ""
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
if ([string]::IsNullOrWhiteSpace($MissionPakPath)) {
    $MissionPakPath = Join-Path $repoRoot "analysis-output\mission-extract\project-owned-linked-dev\mc2_01\data\missions\mc2_01.pak"
}

if ([string]::IsNullOrWhiteSpace($ContentIndexPath)) {
    $ContentIndexPath = Join-Path $repoRoot "analysis-output\content-index\project-owned-linked-dev.content-index.json"
}

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot "analysis-output\mission-packet-audit"
}

function Read-UInt32LE {
    param(
        [Parameter(Mandatory = $true)]
        [byte[]]$Bytes,

        [Parameter(Mandatory = $true)]
        [int]$Offset
    )

    return [BitConverter]::ToUInt32($Bytes, $Offset)
}

$pakFile = Get-Item -LiteralPath (Resolve-Path -LiteralPath $MissionPakPath).Path
$pakBytes = [IO.File]::ReadAllBytes($pakFile.FullName)
$version = Read-UInt32LE -Bytes $pakBytes -Offset 0
$firstPacketOffset = [int](Read-UInt32LE -Bytes $pakBytes -Offset 4)
$declaredPacketCount = [int](($firstPacketOffset / 4) - 2)

$validPacketCount = 0
$firstInvalidEntry = $null
$packets = @()
for ($index = 0; $index -lt $declaredPacketCount; $index++) {
    $entry = Read-UInt32LE -Bytes $pakBytes -Offset (8 + 4 * $index)
    $entryBits = [uint64]$entry
    $storageType = [int]($entryBits -shr 29)
    $offset = [int]($entryBits -band 0x1fffffff)
    $nextOffset = $pakBytes.Length
    if ($index -lt ($declaredPacketCount - 1)) {
        $nextEntry = Read-UInt32LE -Bytes $pakBytes -Offset (8 + 4 * ($index + 1))
        $nextOffset = [int](([uint64]$nextEntry) -band 0x1fffffff)
    }

    if ($offset -lt $firstPacketOffset -or $offset -gt $pakBytes.Length -or $nextOffset -le $offset -or $nextOffset -gt $pakBytes.Length) {
        $firstInvalidEntry = [ordered]@{
            index = $index
            storageType = $storageType
            offset = $offset
            nextOffset = $nextOffset
            entry = ("0x{0:X8}" -f $entry)
        }
        break
    }

    $validPacketCount++
    $packets += [ordered]@{
        index = $index
        storageType = $storageType
        offset = $offset
        packedSize = $nextOffset - $offset
    }
}

$fstEntry = $null
if (Test-Path -LiteralPath $ContentIndexPath -PathType Leaf) {
    $indexJson = Get-Content -LiteralPath $ContentIndexPath -Raw | ConvertFrom-Json
    $missionFastFile = $indexJson.fastFiles.files | Where-Object { $_.name -eq "mission.fst" } | Select-Object -First 1
    if ($missionFastFile -and $missionFastFile.entries) {
        $fstEntry = $missionFastFile.entries |
            Where-Object { $_.name -eq "data/missions/mc2_01.pak" } |
            Select-Object -First 1
    }
}

$packetOne = $packets | Where-Object { $_.index -eq 1 } | Select-Object -First 1
$allDeclaredPacketsValid = $validPacketCount -eq $declaredPacketCount
$notes = if ($allDeclaredPacketsValid) {
    @(
        "All declared mission packet entries are sequentially valid.",
        "Packet 1 is the terrain object payload read by GameObjectManager::countTerrainObjects.",
        "The decoded terrain object payload should begin with an int count followed by 40-byte ObjDataLoader records.",
        "If extracted bytes differ from the FST real size, check for text-mode output in the fast-file unpack path."
    )
}
else {
    @(
        "Source code reads terrain objects from packet 1 through GameObjectManager::countTerrainObjects.",
        "The decoded terrain object payload should begin with an int count followed by 40-byte ObjDataLoader records.",
        "The packet table becomes invalid before all declared packets are reached.",
        "Check the fast-file extraction path before trusting packet offsets."
    )
}

$audit = [ordered]@{
    schema = "mc2-mission-packet-audit-v1"
    createdAt = (Get-Date).ToString("o")
    missionPak = [ordered]@{
        path = $pakFile.FullName
        bytes = [int64]$pakFile.Length
        version = ("0x{0:X8}" -f $version)
        firstPacketOffset = $firstPacketOffset
        declaredPacketCount = $declaredPacketCount
        validSequentialPackets = $validPacketCount
        firstInvalidEntry = $firstInvalidEntry
    }
    fastFileEntry = if ($fstEntry) {
        [ordered]@{
            name = $fstEntry.name
            offset = $fstEntry.offset
            packedBytes = $fstEntry.packedBytes
            realBytes = $fstEntry.realBytes
            hash = $fstEntry.hash
            extractedByteDelta = [int64]$pakFile.Length - [int64]$fstEntry.realBytes
        }
    }
    else {
        $null
    }
    knownPackets = [ordered]@{
        terrain = ($packets | Where-Object { $_.index -eq 0 } | Select-Object -First 1)
        terrainObjects = $packetOne
        movementStartsAt = ($packets | Where-Object { $_.index -eq 4 } | Select-Object -First 1)
    }
    notes = $notes
}

$outputDir = Join-Path (Join-Path $OutputRoot "project-owned-linked-dev") "mc2_01"
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
$jsonPath = Join-Path $outputDir "mission-packet-audit.json"
$audit | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $jsonPath -Encoding UTF8

$markdownPath = Join-Path $outputDir "mission-packet-audit.md"
$lines = @(
    "# mc2_01 Mission Packet Audit",
    "",
    ('- Pak: `{0}`' -f $pakFile.FullName),
    ('- Pak bytes: `{0}`' -f $pakFile.Length),
    ('- Packet version: `{0}`' -f $audit.missionPak.version),
    ('- First packet offset: `{0}`' -f $firstPacketOffset),
    ('- Declared packet count: `{0}`' -f $declaredPacketCount),
    ('- Valid sequential packets before table leaves file bounds: `{0}`' -f $validPacketCount),
    ('- First invalid table entry: `{0}`' -f $(if ($firstInvalidEntry) { $firstInvalidEntry.index } else { "none" })),
    "",
    "## FST Entry",
    "",
    ('- Real bytes from content index: `{0}`' -f $audit.fastFileEntry.realBytes),
    ('- Packed bytes from content index: `{0}`' -f $audit.fastFileEntry.packedBytes),
    ('- Extracted byte delta: `{0}`' -f $audit.fastFileEntry.extractedByteDelta),
    "",
    "## Known Packet Roles",
    "",
    "- Packet 0: terrain payload",
    "- Packet 1: terrain objects, expected by GameObjectManager::countTerrainObjects",
    "- Packet 4 onward: movement/pathfinding data",
    "",
    "## Notes",
    "",
    ($audit.notes | ForEach-Object { "- $_" })
)
$lines | Set-Content -LiteralPath $markdownPath -Encoding UTF8

Write-Output "Mission packet audit written: $jsonPath"
Write-Output "Markdown: $markdownPath"
Write-Output "Valid sequential packets: $validPacketCount / $declaredPacketCount"
if ($fstEntry) {
    Write-Output "FST real bytes: $($fstEntry.realBytes); extracted bytes: $($pakFile.Length)"
}

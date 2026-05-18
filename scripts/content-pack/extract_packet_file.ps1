param(
    [Parameter(Mandatory = $true)]
    [string]$PacketFilePath,

    [Parameter(Mandatory = $false)]
    [string]$OutputDir = ""
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
    $name = [IO.Path]::GetFileName($PacketFilePath)
    $OutputDir = Join-Path $repoRoot ("analysis-output\pak-unpack\" + $name + "-safe")
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

function Get-PacketTypeName {
    param(
        [Parameter(Mandatory = $true)]
        [int]$StorageType
    )

    switch ($StorageType) {
        0 { return "raw" }
        1 { return "file-within-file" }
        2 { return "lz-compressed" }
        3 { return "huffman-compressed" }
        4 { return "zlib-compressed" }
        7 { return "null" }
        default { return "unknown" }
    }
}

function Expand-ZlibPacket {
    param(
        [Parameter(Mandatory = $true)]
        [byte[]]$Source,

        [Parameter(Mandatory = $true)]
        [int]$Offset,

        [Parameter(Mandatory = $true)]
        [int]$Length,

        [Parameter(Mandatory = $true)]
        [int]$ExpectedLength
    )

    $input = New-Object IO.MemoryStream
    $input.Write($Source, $Offset, $Length)
    $input.Position = 0

    $output = New-Object IO.MemoryStream
    $zlib = [IO.Compression.ZLibStream]::new($input, [IO.Compression.CompressionMode]::Decompress)
    try {
        $zlib.CopyTo($output)
    }
    finally {
        $zlib.Dispose()
        $input.Dispose()
    }

    $result = $output.ToArray()
    $output.Dispose()

    if ($ExpectedLength -ne $result.Length) {
        throw "ZLib packet length mismatch. Expected $ExpectedLength bytes, got $($result.Length)."
    }

    return $result
}

$packetFile = (Resolve-Path -LiteralPath $PacketFilePath).Path
$packetBytes = [IO.File]::ReadAllBytes($packetFile)
if ($packetBytes.Length -lt 12) {
    throw "Packet file is too small: $packetFile"
}

$version = Read-UInt32LE -Bytes $packetBytes -Offset 0
$packetFileVersion = [uint32]4277009102
if ($version -ne $packetFileVersion) {
    throw ("Unsupported packet header 0x{0:X8}: {1}" -f $version, $packetFile)
}

$firstPacketOffset = [int](Read-UInt32LE -Bytes $packetBytes -Offset 4)
if (($firstPacketOffset % 4) -ne 0 -or $firstPacketOffset -lt 12) {
    throw "Invalid first packet offset $firstPacketOffset in $packetFile"
}

$declaredPacketCount = [int](($firstPacketOffset / 4) - 2)
$packetEntries = @()
for ($packetIndex = 0; $packetIndex -lt $declaredPacketCount; $packetIndex++) {
    $entry = Read-UInt32LE -Bytes $packetBytes -Offset (8 + (4 * $packetIndex))
    $entryBits = [uint64]$entry
    $storageType = [int]($entryBits -shr 29)
    $packetOffset = [int]($entryBits -band 0x1fffffff)
    if ($packetOffset -lt $firstPacketOffset -or $packetOffset -gt $packetBytes.Length) {
        break
    }

    $nextOffset = $packetBytes.Length
    $terminalPacket = $true
    if ($packetIndex -lt ($declaredPacketCount - 1)) {
        $nextEntry = Read-UInt32LE -Bytes $packetBytes -Offset (8 + (4 * ($packetIndex + 1)))
        $candidateNextOffset = [int](([uint64]$nextEntry) -band 0x1fffffff)
        if ($candidateNextOffset -gt $packetOffset -and $candidateNextOffset -le $packetBytes.Length) {
            $nextOffset = $candidateNextOffset
            $terminalPacket = $false
        }
    }

    $packetEntries += [ordered]@{
        index = $packetIndex
        entry = $entry
        storageType = $storageType
        offset = $packetOffset
        nextOffset = $nextOffset
        terminalPacket = $terminalPacket
    }

    if ($terminalPacket) {
        break
    }
}

$packetCount = $packetEntries.Count
if ($packetCount -eq 0) {
    throw "No valid packet entries found in $packetFile"
}
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

$index = @()
$typeCounts = @{}
foreach ($packetEntry in $packetEntries) {
    $packetIndex = $packetEntry.index
    $storageType = $packetEntry.storageType
    $packetOffset = $packetEntry.offset
    $nextOffset = $packetEntry.nextOffset

    $packedSize = $nextOffset - $packetOffset
    if ($packetOffset -lt 0 -or $packetOffset -gt $packetBytes.Length -or $packedSize -lt 0 -or ($packetOffset + $packedSize) -gt $packetBytes.Length) {
        throw "Invalid packet offset table entry at packet $packetIndex"
    }

    $typeName = Get-PacketTypeName -StorageType $storageType
    if (-not $typeCounts.ContainsKey($typeName)) {
        $typeCounts[$typeName] = 0
    }
    $typeCounts[$typeName]++

    $unpackedSize = $packedSize
    $packetOutputPath = $null
    $note = ""

    if ($storageType -eq 4) {
        if ($packedSize -lt 4) {
            throw "Compressed packet $packetIndex has invalid packed size $packedSize"
        }

        $declaredUnpackedSize = Read-UInt32LE -Bytes $packetBytes -Offset $packetOffset
        if ($declaredUnpackedSize -le [uint32]2147483647) {
            $unpackedSize = [int]$declaredUnpackedSize
            try {
                $payload = Expand-ZlibPacket `
                    -Source $packetBytes `
                    -Offset ($packetOffset + 4) `
                    -Length ($packedSize - 4) `
                    -ExpectedLength $unpackedSize
                $packetOutputPath = Join-Path $OutputDir ("packet_{0:D4}.bin" -f $packetIndex)
                [IO.File]::WriteAllBytes($packetOutputPath, $payload)
            }
            catch {
                $unpackedSize = $packedSize
                $note = "ZLib decode failed; wrote packed payload. $($_.Exception.Message)"
                $payload = New-Object byte[] $packedSize
                [Array]::Copy($packetBytes, $packetOffset, $payload, 0, $packedSize)
                $packetOutputPath = Join-Path $OutputDir ("packet_{0:D4}.packed.bin" -f $packetIndex)
                [IO.File]::WriteAllBytes($packetOutputPath, $payload)
            }
        }
        else {
            $unpackedSize = $packedSize
            $note = "Compressed packet has implausible declared size $declaredUnpackedSize; wrote packed payload."
            $payload = New-Object byte[] $packedSize
            [Array]::Copy($packetBytes, $packetOffset, $payload, 0, $packedSize)
            $packetOutputPath = Join-Path $OutputDir ("packet_{0:D4}.packed.bin" -f $packetIndex)
            [IO.File]::WriteAllBytes($packetOutputPath, $payload)
        }
    }
    elseif ($storageType -eq 0 -or $storageType -eq 1) {
        $payload = New-Object byte[] $packedSize
        [Array]::Copy($packetBytes, $packetOffset, $payload, 0, $packedSize)
        $packetOutputPath = Join-Path $OutputDir ("packet_{0:D4}.bin" -f $packetIndex)
        [IO.File]::WriteAllBytes($packetOutputPath, $payload)
    }
    elseif ($storageType -eq 7) {
        $unpackedSize = 0
        $note = "Null packet"
    }
    else {
        $note = "Unsupported storage type; payload was not extracted"
    }

    $index += [ordered]@{
        index = $packetIndex
        storageType = $storageType
        storageTypeName = $typeName
        offset = $packetOffset
        packedSize = $packedSize
        unpackedSize = $unpackedSize
        output = $packetOutputPath
        terminalPacket = $packetEntry.terminalPacket
        note = $note
    }
}

$manifest = [ordered]@{
    schema = "mc2-packet-extract-v1"
    createdAt = (Get-Date).ToString("o")
    source = $packetFile
    outputDir = (Resolve-Path -LiteralPath $OutputDir).Path
    version = ("0x{0:X8}" -f $version)
    firstPacketOffset = $firstPacketOffset
    declaredPacketCount = $declaredPacketCount
    packetCount = $packetCount
    typeCounts = $typeCounts
    packets = $index
}

$manifestPath = Join-Path $OutputDir "packet-index.json"
$manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $manifestPath -Encoding UTF8

Write-Output "Packet file extracted: $packetFile"
Write-Output "Output: $OutputDir"
Write-Output "Packets: $packetCount"
foreach ($key in @($typeCounts.Keys | Sort-Object)) {
    Write-Output ("{0}: {1}" -f $key, $typeCounts[$key])
}

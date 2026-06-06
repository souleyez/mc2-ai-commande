using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using MC2Demo.BattleCore;
using UnityEngine;

namespace MC2Demo.Presentation
{
    internal static class ReferenceTerrainTextureLibrary
    {
        private const int CompositeTileSize = 8;
        private const int MaxCompositeSide = 2048;
        private static readonly Dictionary<int, LoadedTerrainTexture> TextureCache = new();
        private static readonly HashSet<int> MissingTextureIds = new();
        private static TerrainTextureManifest manifestCache;
        private static string manifestPathCache;
        private static bool manifestLoadAttempted;

        public static bool TryBuildCompositeTexture(MissionContract contract, out Texture2D texture, out string summary)
        {
            texture = null;
            summary = "terrain textures unavailable";
            TerrainMeshDefinition terrain = contract?.terrainMesh;
            if (terrain?.samples == null || terrain.samples.Length == 0 || terrain.sampleSide <= 1)
            {
                return false;
            }

            if (!TryLoadManifest(out TerrainTextureManifest manifest, out string manifestPath))
            {
                summary = "terrain texture manifest missing";
                return false;
            }

            int side = terrain.sampleSide;
            int tileSize = Mathf.Max(2, Mathf.Min(CompositeTileSize, MaxCompositeSide / side));
            int textureSide = side * tileSize;
            Color32[] pixels = new Color32[textureSide * textureSide];
            int loadedSamples = 0;
            int missingSamples = 0;
            float waterElevation = contract.mission?.terrain == null ? 350f : contract.mission.terrain.waterElevation;

            for (int row = 0; row < side; row++)
            {
                for (int col = 0; col < side; col++)
                {
                    int sampleIndex = row * side + col;
                    TerrainMeshSample sample = terrain.samples[Mathf.Clamp(sampleIndex, 0, terrain.samples.Length - 1)];
                    int textureId = SourceTextureId(sample);
                    LoadedTerrainTexture source = TryLoadSourceTexture(textureId, manifest, manifestPath);
                    if (source == null)
                    {
                        missingSamples++;
                    }
                    else
                    {
                        loadedSamples++;
                    }

                    PaintCompositeTile(pixels, textureSide, tileSize, row, col, source, sample, waterElevation);
                }
            }

            if (loadedSamples == 0)
            {
                summary = "terrain texture manifest has no loadable tile textures";
                return false;
            }

            texture = new Texture2D(textureSide, textureSide, TextureFormat.RGBA32, true)
            {
                name = "MC2 Source Terrain Composite",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            texture.SetPixels32(pixels);
            texture.Apply(true, false);
            TryWriteDebugCompositeTexture(texture);
            summary = "terrain texture composite "
                + textureSide.ToString(CultureInfo.InvariantCulture)
                + "px loadedSamples="
                + loadedSamples.ToString(CultureInfo.InvariantCulture)
                + " missingSamples="
                + missingSamples.ToString(CultureInfo.InvariantCulture)
                + " manifestTextures="
                + (manifest.textures?.Length ?? 0).ToString(CultureInfo.InvariantCulture)
                + " "
                + PixelLumaSummary(pixels);
            return true;
        }

        private static void TryWriteDebugCompositeTexture(Texture2D texture)
        {
            if (texture == null || !string.Equals(Environment.GetEnvironmentVariable("MC2_WRITE_TERRAIN_DEBUG"), "1", StringComparison.Ordinal))
            {
                return;
            }

            try
            {
                string outputDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "analysis-output", "reference-visual-captures"));
                Directory.CreateDirectory(outputDirectory);
                string outputPath = Path.Combine(outputDirectory, "terrain-composite-debug.png");
                File.WriteAllBytes(outputPath, texture.EncodeToPNG());
                Debug.Log("MC2 terrain composite debug written: " + outputPath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Failed to write terrain composite debug texture: " + ex.Message);
            }
        }

        private static void PaintCompositeTile(
            Color32[] pixels,
            int textureSide,
            int tileSize,
            int row,
            int col,
            LoadedTerrainTexture source,
            TerrainMeshSample sample,
            float waterElevation)
        {
            int startX = col * tileSize;
            int startY = row * tileSize;
            float light = TerrainLightMultiplier(sample);
            for (int y = 0; y < tileSize; y++)
            {
                for (int x = 0; x < tileSize; x++)
                {
                    Color32 color = source == null
                        ? FallbackDetailColor(sample, x, y)
                        : source.Sample((col * tileSize) + x, (row * tileSize) + y);
                    pixels[(startY + y) * textureSide + startX + x] = ApplyLight(ColorGradeTerrainTexture(color, sample, waterElevation), light);
                }
            }
        }

        private static Color32 FallbackDetailColor(TerrainMeshSample sample, int x, int y)
        {
            int seed = SourceTextureId(sample) * 37 + sample.terrainType * 11 + x * 3 + y * 5;
            byte value = (byte)(116 + Mathf.Abs(seed % 48));
            return new Color32(value, value, value, 255);
        }

        private static string PixelLumaSummary(Color32[] pixels)
        {
            if (pixels == null || pixels.Length == 0)
            {
                return "luma=empty";
            }

            double total = 0.0;
            int minimum = 255;
            int maximum = 0;
            for (int index = 0; index < pixels.Length; index++)
            {
                Color32 color = pixels[index];
                int luma = Mathf.Clamp(Mathf.RoundToInt(color.r * 0.2126f + color.g * 0.7152f + color.b * 0.0722f), 0, 255);
                minimum = Mathf.Min(minimum, luma);
                maximum = Mathf.Max(maximum, luma);
                total += luma;
            }

            double average = total / pixels.Length;
            return "luma="
                + minimum.ToString(CultureInfo.InvariantCulture)
                + "/"
                + Math.Round(average, 1).ToString(CultureInfo.InvariantCulture)
                + "/"
                + maximum.ToString(CultureInfo.InvariantCulture);
        }

        private static Color32 ColorGradeTerrainTexture(Color32 color, TerrainMeshSample sample, float waterElevation)
        {
            Color graded = new(color.r / 255f, color.g / 255f, color.b / 255f, 1f);
            graded.r = Mathf.Pow(Mathf.Clamp01(graded.r), 0.82f);
            graded.g = Mathf.Pow(Mathf.Clamp01(graded.g), 0.82f);
            graded.b = Mathf.Pow(Mathf.Clamp01(graded.b), 0.82f);
            graded = AdjustContrast(graded, IsRunwayOrRoad(sample) ? 1.16f : 1.08f);
            Color semantic = SemanticTerrainColor(sample, waterElevation);

            if (sample.elevation <= waterElevation + 4f)
            {
                graded = Color.Lerp(graded, new Color(0.12f, 0.42f, 0.54f), 0.30f);
            }
            else if (sample.elevation <= waterElevation + 24f)
            {
                graded = Color.Lerp(graded, new Color(0.44f, 0.53f, 0.34f), 0.24f);
            }

            float textureBlend = IsRunwayOrRoad(sample) ? 0.34f : 0.20f;
            if (sample.elevation <= waterElevation + 24f)
            {
                textureBlend = 0.16f;
            }

            graded = Color.Lerp(semantic, graded, textureBlend);
            graded = ClampTerrainMinimum(graded, semantic);

            return new Color32(
                (byte)Mathf.Clamp(Mathf.RoundToInt(graded.r * 255f), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(graded.g * 255f), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(graded.b * 255f), 0, 255),
                255);
        }

        private static Color SemanticTerrainColor(TerrainMeshSample sample, float waterElevation)
        {
            if (sample.elevation <= waterElevation + 4f)
            {
                return new Color(0.10f, 0.39f, 0.50f);
            }

            if (sample.elevation <= waterElevation + 24f)
            {
                return new Color(0.40f, 0.50f, 0.32f);
            }

            if (IsRunwayOrRoad(sample))
            {
                return sample.terrainType == 14
                    ? new Color(0.61f, 0.60f, 0.52f)
                    : new Color(0.53f, 0.54f, 0.47f);
            }

            if (sample.terrainType == 20)
            {
                return new Color(0.55f, 0.42f, 0.27f);
            }

            int textureId = SourceTextureId(sample);
            if (textureId > 2)
            {
                return new Color(0.44f, 0.48f, 0.31f);
            }

            return new Color(0.34f, 0.47f, 0.24f);
        }

        private static bool IsRunwayOrRoad(TerrainMeshSample sample)
        {
            return sample.terrainType == 13
                || sample.terrainType == 14
                || sample.terrainType == 15
                || sample.terrainType == 16;
        }

        private static Color ClampTerrainMinimum(Color graded, Color semantic)
        {
            float minimum = Mathf.Clamp01(Mathf.Max(0.16f, Mathf.Min(semantic.r, Mathf.Min(semantic.g, semantic.b)) * 0.72f));
            return new Color(
                Mathf.Max(graded.r, minimum),
                Mathf.Max(graded.g, minimum),
                Mathf.Max(graded.b, minimum),
                graded.a);
        }

        private static Color AdjustContrast(Color color, float contrast)
        {
            return new Color(
                Mathf.Clamp01((color.r - 0.5f) * contrast + 0.5f),
                Mathf.Clamp01((color.g - 0.5f) * contrast + 0.5f),
                Mathf.Clamp01((color.b - 0.5f) * contrast + 0.5f),
                color.a);
        }

        private static Color32 ApplyLight(Color32 color, float light)
        {
            return new Color32(
                (byte)Mathf.Clamp(Mathf.RoundToInt(color.r * light), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(color.g * light), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(color.b * light), 0, 255),
                255);
        }

        private static float TerrainLightMultiplier(TerrainMeshSample sample)
        {
            if (sample.light <= 0)
            {
                return 1f;
            }

            int lowByte = (int)(sample.light & 0xffL);
            return Mathf.Lerp(0.88f, 1.22f, lowByte / 255f);
        }

        private static int SourceTextureId(TerrainMeshSample sample)
        {
            return sample.textureId > 0 ? sample.textureId : (int)(sample.textureData & 0xffffL);
        }

        private static LoadedTerrainTexture TryLoadSourceTexture(
            int textureId,
            TerrainTextureManifest manifest,
            string manifestPath)
        {
            if (TextureCache.TryGetValue(textureId, out LoadedTerrainTexture cached))
            {
                return cached;
            }

            if (MissingTextureIds.Contains(textureId))
            {
                return null;
            }

            TerrainTextureEntry entry = FindEntry(manifest, textureId);
            string texturePath = ResolveTexturePath(entry, manifestPath);
            if (string.IsNullOrWhiteSpace(texturePath) || !File.Exists(texturePath))
            {
                MissingTextureIds.Add(textureId);
                return null;
            }

            try
            {
                LoadedTerrainTexture loaded = LoadTexture(texturePath, textureId, Path.GetDirectoryName(manifestPath) ?? "");
                TextureCache[textureId] = loaded;
                return loaded;
            }
            catch (Exception ex)
            {
                MissingTextureIds.Add(textureId);
                Debug.LogWarning("Failed to load terrain reference texture "
                    + textureId.ToString(CultureInfo.InvariantCulture)
                    + " from "
                    + texturePath
                    + ": "
                    + ex.Message);
                return null;
            }
        }

        private static TerrainTextureEntry FindEntry(TerrainTextureManifest manifest, int textureId)
        {
            foreach (TerrainTextureEntry entry in manifest.textures ?? Array.Empty<TerrainTextureEntry>())
            {
                if (entry != null && entry.textureId == textureId)
                {
                    return entry;
                }
            }

            return null;
        }

        private static string ResolveTexturePath(TerrainTextureEntry entry, string manifestPath)
        {
            if (entry == null)
            {
                return "";
            }

            if (!string.IsNullOrWhiteSpace(entry.outputPath) && File.Exists(entry.outputPath))
            {
                return entry.outputPath;
            }

            string manifestDirectory = Path.GetDirectoryName(manifestPath) ?? "";
            if (!string.IsNullOrWhiteSpace(entry.relativePath))
            {
                string relative = Path.GetFullPath(Path.Combine(manifestDirectory, entry.relativePath));
                if (File.Exists(relative))
                {
                    return relative;
                }
            }

            return "";
        }

        private static LoadedTerrainTexture LoadTexture(string path, int textureId, string rootDirectory)
        {
            string extension = Path.GetExtension(path).ToLowerInvariant();
            if (extension == ".txm")
            {
                return LoadTxmTexture(path, textureId);
            }

            if (extension == ".tga")
            {
                return LoadTgaTexture(path, textureId);
            }

            if (extension == ".lst")
            {
                return LoadLstTexture(path, textureId, rootDirectory);
            }

            throw new NotSupportedException("Unsupported terrain texture format " + extension + ".");
        }

        private static LoadedTerrainTexture LoadLstTexture(string path, int textureId, string rootDirectory)
        {
            foreach (string listedPath in ReadLstTexturePaths(path))
            {
                string normalized = listedPath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
                string candidatePath = Path.IsPathRooted(normalized)
                    ? normalized
                    : Path.GetFullPath(Path.Combine(rootDirectory, normalized));
                if (!File.Exists(candidatePath))
                {
                    continue;
                }

                string extension = Path.GetExtension(candidatePath).ToLowerInvariant();
                if (extension == ".lst")
                {
                    continue;
                }

                return LoadTexture(candidatePath, textureId, rootDirectory);
            }

            throw new FileNotFoundException("Terrain texture list has no exported loadable frames.", path);
        }

        private static IEnumerable<string> ReadLstTexturePaths(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            string text = Encoding.ASCII.GetString(bytes);
            string[] parts = text.Split(new[] { '\0', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                string trimmed = part.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    yield return trimmed;
                }
            }
        }

        private static LoadedTerrainTexture LoadTxmTexture(string path, int textureId)
        {
            byte[] compressed = File.ReadAllBytes(path);
            byte[] raw = InflateZlibPayload(compressed);
            int pixelCount = raw.Length / 4;
            int side = Mathf.RoundToInt(Mathf.Sqrt(pixelCount));
            if (side <= 0 || side * side * 4 != raw.Length)
            {
                throw new InvalidDataException("TXM payload is not a square 32-bit texture.");
            }

            Color32[] pixels = new Color32[side * side];
            for (int index = 0; index < pixels.Length; index++)
            {
                int offset = index * 4;
                pixels[index] = new Color32(raw[offset + 2], raw[offset + 1], raw[offset], raw[offset + 3]);
            }

            return new LoadedTerrainTexture(textureId, side, side, pixels);
        }

        private static LoadedTerrainTexture LoadTgaTexture(string path, int textureId)
        {
            byte[] data = File.ReadAllBytes(path);
            if (data.Length < 18)
            {
                throw new InvalidDataException("TGA header is truncated.");
            }

            int idLength = data[0];
            int colorMapType = data[1];
            int imageType = data[2];
            int width = data[12] | (data[13] << 8);
            int height = data[14] | (data[15] << 8);
            int bitsPerPixel = data[16];
            int descriptor = data[17];
            if (colorMapType != 0)
            {
                throw new NotSupportedException("Color-mapped TGA is not supported.");
            }

            if (width <= 0 || height <= 0)
            {
                throw new InvalidDataException("TGA dimensions are invalid.");
            }

            int bytesPerPixel = bitsPerPixel / 8;
            if (bytesPerPixel != 1 && bytesPerPixel != 3 && bytesPerPixel != 4)
            {
                throw new NotSupportedException("Only 8-bit, 24-bit, and 32-bit TGA textures are supported.");
            }

            int offset = 18 + idLength;
            Color32[] pixels = new Color32[width * height];
            if (imageType == 2 || imageType == 3)
            {
                DecodeRawTga(data, offset, bytesPerPixel, descriptor, width, height, pixels);
            }
            else if (imageType == 10 || imageType == 11)
            {
                DecodeRleTga(data, offset, bytesPerPixel, descriptor, width, height, pixels);
            }
            else
            {
                throw new NotSupportedException("Unsupported TGA image type " + imageType.ToString(CultureInfo.InvariantCulture) + ".");
            }

            return new LoadedTerrainTexture(textureId, width, height, pixels);
        }

        private static void DecodeRawTga(
            byte[] data,
            int offset,
            int bytesPerPixel,
            int descriptor,
            int width,
            int height,
            Color32[] pixels)
        {
            int cursor = offset;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (cursor + bytesPerPixel > data.Length)
                    {
                        throw new InvalidDataException("TGA pixel data is truncated.");
                    }

                    SetTgaPixel(data, cursor, bytesPerPixel, descriptor, width, height, x, y, pixels);
                    cursor += bytesPerPixel;
                }
            }
        }

        private static void DecodeRleTga(
            byte[] data,
            int offset,
            int bytesPerPixel,
            int descriptor,
            int width,
            int height,
            Color32[] pixels)
        {
            int cursor = offset;
            int pixelIndex = 0;
            int pixelCount = width * height;
            while (pixelIndex < pixelCount)
            {
                if (cursor >= data.Length)
                {
                    throw new InvalidDataException("TGA RLE packet is truncated.");
                }

                int header = data[cursor++];
                int runLength = (header & 0x7F) + 1;
                bool runPacket = (header & 0x80) != 0;
                if (runPacket)
                {
                    if (cursor + bytesPerPixel > data.Length)
                    {
                        throw new InvalidDataException("TGA RLE color is truncated.");
                    }

                    for (int index = 0; index < runLength && pixelIndex < pixelCount; index++)
                    {
                        int x = pixelIndex % width;
                        int y = pixelIndex / width;
                        SetTgaPixel(data, cursor, bytesPerPixel, descriptor, width, height, x, y, pixels);
                        pixelIndex++;
                    }

                    cursor += bytesPerPixel;
                    continue;
                }

                for (int index = 0; index < runLength && pixelIndex < pixelCount; index++)
                {
                    if (cursor + bytesPerPixel > data.Length)
                    {
                        throw new InvalidDataException("TGA RLE raw data is truncated.");
                    }

                    int x = pixelIndex % width;
                    int y = pixelIndex / width;
                    SetTgaPixel(data, cursor, bytesPerPixel, descriptor, width, height, x, y, pixels);
                    cursor += bytesPerPixel;
                    pixelIndex++;
                }
            }
        }

        private static void SetTgaPixel(
            byte[] data,
            int offset,
            int bytesPerPixel,
            int descriptor,
            int width,
            int height,
            int sourceX,
            int sourceY,
            Color32[] pixels)
        {
            bool topOrigin = (descriptor & 0x20) != 0;
            int targetY = topOrigin ? height - 1 - sourceY : sourceY;
            int targetIndex = targetY * width + sourceX;
            if (bytesPerPixel == 1)
            {
                byte value = data[offset];
                pixels[targetIndex] = new Color32(value, value, value, 255);
                return;
            }

            byte blue = data[offset];
            byte green = data[offset + 1];
            byte red = data[offset + 2];
            byte alpha = bytesPerPixel == 4 ? data[offset + 3] : (byte)255;
            pixels[targetIndex] = new Color32(red, green, blue, alpha);
        }

        private static byte[] InflateZlibPayload(byte[] compressed)
        {
            if (compressed.Length < 7 || compressed[0] != 0x78)
            {
                throw new InvalidDataException("TXM payload is not zlib-compressed.");
            }

            using MemoryStream source = new(compressed, 2, compressed.Length - 6);
            using DeflateStream deflate = new(source, CompressionMode.Decompress);
            using MemoryStream target = new();
            deflate.CopyTo(target);
            return target.ToArray();
        }

        private static bool TryLoadManifest(out TerrainTextureManifest manifest, out string manifestPath)
        {
            if (manifestLoadAttempted)
            {
                manifest = manifestCache;
                manifestPath = manifestPathCache;
                return manifest != null;
            }

            manifestLoadAttempted = true;
            foreach (string candidate in CandidateManifestPaths())
            {
                if (string.IsNullOrWhiteSpace(candidate) || !File.Exists(candidate))
                {
                    continue;
                }

                try
                {
                    manifest = JsonUtility.FromJson<TerrainTextureManifest>(File.ReadAllText(candidate));
                    if (manifest?.textures == null || manifest.textures.Length == 0)
                    {
                        continue;
                    }

                    manifestCache = manifest;
                    manifestPathCache = candidate;
                    manifestPath = candidate;
                    Debug.Log("Loaded private terrain texture manifest: " + candidate);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Failed to load private terrain texture manifest " + candidate + ": " + ex.Message);
                }
            }

            manifest = null;
            manifestPath = "";
            return false;
        }

        private static IEnumerable<string> CandidateManifestPaths()
        {
            yield return NormalizePath(Path.Combine(
                Directory.GetCurrentDirectory(),
                "analysis-output",
                "terrain-reference-textures",
                "mc2_01",
                "manifest.json"));
            yield return NormalizePath(Path.Combine(
                Application.dataPath,
                "..",
                "..",
                "..",
                "..",
                "analysis-output",
                "terrain-reference-textures",
                "mc2_01",
                "manifest.json"));
            yield return NormalizePath(Path.Combine(
                Application.dataPath,
                "..",
                "..",
                "analysis-output",
                "terrain-reference-textures",
                "mc2_01",
                "manifest.json"));
        }

        private static string NormalizePath(string path)
        {
            try
            {
                return Path.GetFullPath(path);
            }
            catch
            {
                return path;
            }
        }

        private sealed class LoadedTerrainTexture
        {
            private readonly Color32[] pixels;

            public LoadedTerrainTexture(int textureId, int width, int height, Color32[] pixels)
            {
                TextureId = textureId;
                Width = width;
                Height = height;
                this.pixels = pixels;
            }

            public int TextureId { get; }
            public int Width { get; }
            public int Height { get; }

            public Color32 Sample(int x, int y)
            {
                if (pixels == null || pixels.Length == 0 || Width <= 0 || Height <= 0)
                {
                    return new Color32(160, 160, 160, 255);
                }

                int sx = PositiveModulo(x, Width);
                int sy = PositiveModulo(y, Height);
                return pixels[sy * Width + sx];
            }

            private static int PositiveModulo(int value, int modulus)
            {
                int result = value % modulus;
                return result < 0 ? result + modulus : result;
            }
        }

        [Serializable]
        private sealed class TerrainTextureManifest
        {
            public string schema;
            public string missionId;
            public string outputRoot;
            public TerrainTextureEntry[] textures;
        }

        [Serializable]
        private sealed class TerrainTextureEntry
        {
            public int textureId;
            public string sourceName;
            public string relativePath;
            public string outputPath;
            public string format;
            public int width;
            public int height;
            public int bitsPerPixel;
        }
    }
}

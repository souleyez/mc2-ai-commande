using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MC2Demo.BattleCore;
using UnityEngine;

namespace MC2Demo.Presentation
{
    internal static class ReferenceObjMeshLibrary
    {
        private const float ReferenceVisualScale = 3.0f;
        private static readonly Dictionary<string, Mesh> MeshCache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, bool> MissingCache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Texture2D> TextureCache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, bool> MissingTextureCache = new(StringComparer.OrdinalIgnoreCase);

        public static bool TryAttachReferenceVisual(UnitState unit, Transform parent, Color color, out Renderer renderer)
        {
            renderer = null;
            if (unit == null || parent == null)
            {
                return false;
            }

            string assetName = AssetNameForUnitType(unit.UnitType);
            if (string.IsNullOrWhiteSpace(assetName))
            {
                return false;
            }

            if (!TryLoadMesh(assetName, out Mesh mesh))
            {
                return false;
            }

            GameObject visual = new(unit.Id + " reference " + assetName);
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one * ReferenceVisualScale;

            MeshFilter filter = visual.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            MeshRenderer meshRenderer = visual.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = CreateMaterial(assetName, unit.IsPlayerUnit, color);
            renderer = meshRenderer;
            return true;
        }

        public static bool IsTallReferenceUnit(string unitType)
        {
            string assetName = AssetNameForUnitType(unitType);
            return string.Equals(assetName, "werewolf", StringComparison.OrdinalIgnoreCase)
                || string.Equals(assetName, "bushwacker", StringComparison.OrdinalIgnoreCase)
                || string.Equals(assetName, "urbanmech", StringComparison.OrdinalIgnoreCase)
                || string.Equals(assetName, "starslayer", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryLoadMesh(string assetName, out Mesh mesh)
        {
            if (MeshCache.TryGetValue(assetName, out mesh))
            {
                return true;
            }

            if (MissingCache.ContainsKey(assetName))
            {
                return false;
            }

            string objPath = FindObjPath(assetName);
            if (string.IsNullOrWhiteSpace(objPath))
            {
                MissingCache[assetName] = true;
                return false;
            }

            try
            {
                mesh = LoadObjMesh(objPath, assetName);
                MeshCache[assetName] = mesh;
                Debug.Log("Loaded private reference OBJ mesh: " + objPath);
                return true;
            }
            catch (Exception ex)
            {
                MissingCache[assetName] = true;
                Debug.LogWarning("Failed to load private reference OBJ mesh " + objPath + ": " + ex.Message);
                mesh = null;
                return false;
            }
        }

        private static Mesh LoadObjMesh(string path, string assetName)
        {
            List<Vector3> sourceVertices = new();
            List<Vector2> sourceUvs = new();
            List<Vector3> vertices = new();
            List<Vector2> uvs = new();
            List<int> triangles = new();

            foreach (string rawLine in File.ReadLines(path))
            {
                string line = rawLine.Trim();
                if (line.Length == 0 || line[0] == '#')
                {
                    continue;
                }

                if (line.StartsWith("v ", StringComparison.Ordinal))
                {
                    string[] parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    sourceVertices.Add(new Vector3(ParseFloat(parts[1]), ParseFloat(parts[2]), ParseFloat(parts[3])));
                    continue;
                }

                if (line.StartsWith("vt ", StringComparison.Ordinal))
                {
                    string[] parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    sourceUvs.Add(new Vector2(ParseFloat(parts[1]), ParseFloat(parts[2])));
                    continue;
                }

                if (!line.StartsWith("f ", StringComparison.Ordinal))
                {
                    continue;
                }

                string[] faceParts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                if (faceParts.Length < 4)
                {
                    continue;
                }

                int faceStart = vertices.Count;
                for (int index = 1; index < faceParts.Length; index++)
                {
                    ParseFaceToken(faceParts[index], out int vertexIndex, out int uvIndex);
                    if (vertexIndex < 0 || vertexIndex >= sourceVertices.Count)
                    {
                        vertices.Add(Vector3.zero);
                    }
                    else
                    {
                        vertices.Add(sourceVertices[vertexIndex]);
                    }

                    if (uvIndex >= 0 && uvIndex < sourceUvs.Count)
                    {
                        uvs.Add(sourceUvs[uvIndex]);
                    }
                    else
                    {
                        uvs.Add(Vector2.zero);
                    }
                }

                for (int index = 1; index < faceParts.Length - 2; index++)
                {
                    triangles.Add(faceStart);
                    triangles.Add(faceStart + index);
                    triangles.Add(faceStart + index + 1);
                }
            }

            Mesh mesh = new()
            {
                name = "Reference " + assetName
            };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void ParseFaceToken(string token, out int vertexIndex, out int uvIndex)
        {
            vertexIndex = -1;
            uvIndex = -1;
            string[] parts = token.Split('/');
            if (parts.Length > 0 && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int vertex))
            {
                vertexIndex = vertex - 1;
            }

            if (parts.Length > 1 && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int uv))
            {
                uvIndex = uv - 1;
            }
        }

        private static float ParseFloat(string value)
        {
            return float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        private static string FindObjPath(string assetName)
        {
            foreach (string root in CandidateRoots())
            {
                string candidate = Path.Combine(root, assetName, assetName + ".obj");
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return "";
        }

        private static IEnumerable<string> CandidateRoots()
        {
            string current = Directory.GetCurrentDirectory();
            string dataPath = Application.dataPath;
            yield return FullPath(current, "analysis-output", "tgl-obj");
            yield return FullPath(current, "..", "analysis-output", "tgl-obj");
            yield return FullPath(dataPath, "..", "..", "analysis-output", "tgl-obj");
            yield return FullPath(dataPath, "..", "..", "..", "analysis-output", "tgl-obj");
            yield return FullPath(dataPath, "..", "..", "..", "..", "analysis-output", "tgl-obj");
        }

        private static string FullPath(params string[] parts)
        {
            return Path.GetFullPath(Path.Combine(parts));
        }

        private static Material CreateMaterial(string assetName, bool isPlayerUnit, Color fallbackColor)
        {
            Shader shader = Shader.Find("Standard") ?? Shader.Find("Hidden/Internal-Colored");
            Color color = isPlayerUnit
                ? Color.Lerp(fallbackColor, new Color(0.72f, 0.90f, 0.96f), 0.34f)
                : Color.Lerp(fallbackColor, new Color(0.86f, 0.72f, 0.62f), 0.30f);
            Material material = new(shader)
            {
                name = isPlayerUnit ? "PrivateReferencePlayer" : "PrivateReferenceHostile",
                color = color
            };
            if (TryLoadTexture(assetName, out Texture2D texture))
            {
                material.mainTexture = texture;
                material.color = new Color(0.66f, 0.70f, 0.72f, 1f);
            }

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", 0.12f);
            }

            return material;
        }

        private static bool TryLoadTexture(string assetName, out Texture2D texture)
        {
            if (TextureCache.TryGetValue(assetName, out texture))
            {
                return true;
            }

            if (MissingTextureCache.ContainsKey(assetName))
            {
                return false;
            }

            string texturePath = FindTexturePath(assetName);
            if (string.IsNullOrWhiteSpace(texturePath))
            {
                MissingTextureCache[assetName] = true;
                return false;
            }

            try
            {
                texture = LoadTgaTexture(texturePath, assetName);
                TextureCache[assetName] = texture;
                Debug.Log("Loaded private reference TGA texture: " + texturePath);
                return true;
            }
            catch (Exception ex)
            {
                MissingTextureCache[assetName] = true;
                Debug.LogWarning("Failed to load private reference TGA texture " + texturePath + ": " + ex.Message);
                texture = null;
                return false;
            }
        }

        private static string FindTexturePath(string assetName)
        {
            foreach (string root in CandidateRoots())
            {
                string folder = Path.Combine(root, assetName);
                if (!Directory.Exists(folder))
                {
                    continue;
                }

                string[] files = Directory.GetFiles(folder, "*.tga");
                if (files.Length > 0)
                {
                    Array.Sort(files, StringComparer.OrdinalIgnoreCase);
                    return files[0];
                }
            }

            return "";
        }

        private static Texture2D LoadTgaTexture(string path, string assetName)
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
            if (bytesPerPixel != 3 && bytesPerPixel != 4)
            {
                throw new NotSupportedException("Only 24-bit and 32-bit TGA textures are supported.");
            }

            int offset = 18 + idLength;
            Color32[] pixels = new Color32[width * height];
            if (imageType == 2)
            {
                DecodeRawTga(data, offset, bytesPerPixel, descriptor, width, height, pixels);
            }
            else if (imageType == 10)
            {
                DecodeRleTga(data, offset, bytesPerPixel, descriptor, width, height, pixels);
            }
            else
            {
                throw new NotSupportedException("Unsupported TGA image type " + imageType.ToString(CultureInfo.InvariantCulture) + ".");
            }

            Texture2D texture = new(width, height, TextureFormat.RGBA32, true)
            {
                name = "Reference Texture " + assetName,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };
            texture.SetPixels32(pixels);
            texture.Apply(true, false);
            return texture;
        }

        private static void DecodeRawTga(byte[] data, int offset, int bytesPerPixel, int descriptor, int width, int height, Color32[] pixels)
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

        private static void DecodeRleTga(byte[] data, int offset, int bytesPerPixel, int descriptor, int width, int height, Color32[] pixels)
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

        private static void SetTgaPixel(byte[] data, int cursor, int bytesPerPixel, int descriptor, int width, int height, int sourceX, int sourceY, Color32[] pixels)
        {
            bool originTop = (descriptor & 0x20) != 0;
            bool originRight = (descriptor & 0x10) != 0;
            int x = originRight ? width - 1 - sourceX : sourceX;
            int y = originTop ? height - 1 - sourceY : sourceY;
            int target = y * width + x;
            byte blue = data[cursor];
            byte green = data[cursor + 1];
            byte red = data[cursor + 2];
            byte alpha = bytesPerPixel == 4 ? data[cursor + 3] : (byte)255;
            pixels[target] = NormalizeReferencePalette(new Color32(red, green, blue, alpha));
        }

        private static Color32 NormalizeReferencePalette(Color32 pixel)
        {
            bool likelyTeamBlue = pixel.b > 72
                && pixel.b > pixel.r + 28
                && pixel.b > pixel.g + 18;
            if (!likelyTeamBlue)
            {
                return pixel;
            }

            byte shade = (byte)Mathf.Clamp(Mathf.RoundToInt(34f + pixel.b * 0.25f + Mathf.Max(pixel.r, pixel.g) * 0.07f), 36, 112);
            return new Color32(shade, (byte)Mathf.Clamp(shade + 4, 0, 255), (byte)Mathf.Clamp(shade + 9, 0, 255), pixel.a);
        }

        private static string AssetNameForUnitType(string unitType)
        {
            if (string.IsNullOrWhiteSpace(unitType))
            {
                return "";
            }

            switch (unitType.Trim().ToLowerInvariant())
            {
                case "werewolf":
                    return "werewolf";
                case "bushwacker":
                    return "bushwacker";
                case "centipede":
                    return "centipede";
                case "harasser":
                    return "harasser";
                case "lrmc":
                    return "lrmc";
                case "urbanmech":
                    return "urbanmech";
                case "starslayer":
                    return "starslayer";
                default:
                    return "";
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace MC2Demo.Presentation
{
    internal static class DemoPrimitiveVisualFactory
    {
        private static readonly Dictionary<PrimitiveType, Mesh> MeshCache = new();

        public static GameObject Create(PrimitiveType primitive, string objectName)
        {
            GameObject visual = new(objectName);
            MeshFilter filter = visual.AddComponent<MeshFilter>();
            filter.sharedMesh = MeshForPrimitive(primitive);
            visual.AddComponent<MeshRenderer>();
            return visual;
        }

        private static Mesh MeshForPrimitive(PrimitiveType primitive)
        {
            if (MeshCache.TryGetValue(primitive, out Mesh cached))
            {
                return cached;
            }

            Mesh mesh = LoadBuiltinMesh(primitive);
            MeshCache[primitive] = mesh;
            return mesh;
        }

        private static Mesh LoadBuiltinMesh(PrimitiveType primitive)
        {
            string meshName = primitive switch
            {
                PrimitiveType.Sphere => "Sphere.fbx",
                PrimitiveType.Capsule => "Capsule.fbx",
                PrimitiveType.Cylinder => "Cylinder.fbx",
                PrimitiveType.Plane => "Plane.fbx",
                PrimitiveType.Quad => "Quad.fbx",
                _ => "Cube.fbx",
            };

            Mesh mesh = Resources.GetBuiltinResource<Mesh>(meshName);
            if (mesh != null)
            {
                return mesh;
            }

            Mesh fallback = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            if (fallback != null)
            {
                return fallback;
            }

            throw new System.InvalidOperationException("Unity built-in primitive mesh is unavailable: " + meshName);
        }
    }
}

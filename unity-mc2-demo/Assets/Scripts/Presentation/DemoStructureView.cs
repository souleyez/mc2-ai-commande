using MC2Demo.BattleCore;
using System.Collections.Generic;
using UnityEngine;

namespace MC2Demo.Presentation
{
    public sealed class DemoStructureView : MonoBehaviour
    {
        public StructureState Structure { get; private set; }

        private Vector3 liveScale;
        private Renderer structureRenderer;
        private Color liveColor;
        private Color hitFlashColor = Color.white;
        private float hitFlashRemaining;
        private float hitFlashDuration = 0.18f;
        private bool destroyedPoseApplied;
        private bool mediumDamageCueApplied;
        private bool criticalDamageCueApplied;
        private readonly List<Material> ownedMaterials = new();

        public void Bind(StructureState structure)
        {
            Structure = structure;
            name = structure.Id + " " + structure.ObjectType;
            liveScale = transform.localScale;
            structureRenderer = GetComponent<Renderer>();
            if (structureRenderer != null && structureRenderer.sharedMaterial != null)
            {
                liveColor = structureRenderer.sharedMaterial.color;
            }

            transform.position = GroundedPosition(structure.MissionPosition, liveScale.y);
        }

        public void PulseHit(Color color, float duration = 0.18f)
        {
            if (Structure == null || Structure.IsDestroyed)
            {
                return;
            }

            hitFlashColor = color;
            hitFlashDuration = Mathf.Max(0.05f, duration);
            hitFlashRemaining = hitFlashDuration;
            ApplyDamageColor();
        }

        private void Update()
        {
            if (Structure == null)
            {
                return;
            }

            ApplyPersistentDamageCues();
            ApplyDamageColor();
            if (!Structure.IsDestroyed || destroyedPoseApplied)
            {
                return;
            }

            SpawnDestructionEffects();
            SpawnDestroyedPersistentCue();
            transform.localScale = new Vector3(liveScale.x, liveScale.y * 0.28f, liveScale.z);
            transform.position = GroundedPosition(Structure.MissionPosition, transform.localScale.y);
            destroyedPoseApplied = true;
        }

        public static string StructureDamageCueSummary()
        {
            return "Structure=scar+smoke+collapse";
        }

        private void ApplyPersistentDamageCues()
        {
            if (Structure.IsDestroyed)
            {
                return;
            }

            if (!mediumDamageCueApplied && Structure.Structure < 0.70f)
            {
                SpawnMediumDamageCue();
                mediumDamageCueApplied = true;
            }

            if (!criticalDamageCueApplied && Structure.Structure < 0.35f)
            {
                SpawnCriticalDamageCue();
                criticalDamageCueApplied = true;
            }
        }

        private void ApplyDamageColor()
        {
            if (structureRenderer == null || structureRenderer.sharedMaterial == null || Structure.IsDestroyed)
            {
                return;
            }

            Color damaged = new(0.55f, 0.20f, 0.12f);
            Color color = Color.Lerp(damaged, liveColor, Structure.Structure);
            if (hitFlashRemaining > 0f)
            {
                float t = Mathf.Clamp01(hitFlashRemaining / hitFlashDuration);
                color = Color.Lerp(color, hitFlashColor, t);
                hitFlashRemaining = Mathf.Max(0f, hitFlashRemaining - Time.deltaTime);
            }

            structureRenderer.sharedMaterial.color = color;
        }

        private void SpawnMediumDamageCue()
        {
            CreatePersistentCue("Structure Wall Scorch", PrimitiveType.Cube, new Vector3(0f, 0.05f, -0.51f), new Vector3(0.62f, 0.18f, 0.035f), new Color(0.05f, 0.04f, 0.03f, 1f));
            CreatePersistentCue("Structure Roof Scorch", PrimitiveType.Cube, new Vector3(-0.22f, 0.50f, 0.10f), new Vector3(0.46f, 0.035f, 0.32f), new Color(0.08f, 0.06f, 0.04f, 1f));
            CreatePersistentCue("Structure Damage Spark", PrimitiveType.Cube, new Vector3(0.28f, 0.32f, -0.53f), new Vector3(0.20f, 0.045f, 0.045f), new Color(1f, 0.42f, 0.08f, 1f));
        }

        private void SpawnCriticalDamageCue()
        {
            CreatePersistentCue("Structure Smoke Vent", PrimitiveType.Sphere, new Vector3(0.18f, 0.70f, -0.05f), new Vector3(0.18f, 0.28f, 0.18f), new Color(0.08f, 0.08f, 0.08f, 0.72f), transparent: true);
            CreatePersistentCue("Structure Open Flame", PrimitiveType.Cylinder, new Vector3(-0.26f, 0.42f, -0.52f), new Vector3(0.055f, 0.22f, 0.055f), new Color(1f, 0.32f, 0.06f, 0.92f), transparent: true);
            CreatePersistentCue("Structure Critical Beacon", PrimitiveType.Cube, new Vector3(0.0f, 0.78f, 0.12f), new Vector3(0.54f, 0.055f, 0.08f), new Color(1f, 0.18f, 0.08f, 1f));
        }

        private void SpawnDestructionEffects()
        {
            Vector3 center = GroundedPosition(Structure.MissionPosition, liveScale.y) + new Vector3(0f, liveScale.y * 0.25f, 0f);
            CreateEffect("Structure Blast", PrimitiveType.Sphere, center, new Color(1f, 0.42f, 0.08f, 0.72f), 0.55f, new Vector3(0.4f, 0.4f, 0.4f), new Vector3(4.6f, 1.6f, 4.6f));
            CreateEffect("Structure Smoke", PrimitiveType.Sphere, center + new Vector3(0f, 0.5f, 0f), new Color(0.12f, 0.12f, 0.12f, 0.52f), 1.8f, new Vector3(0.9f, 0.5f, 0.9f), new Vector3(3.8f, 3.2f, 3.8f));
        }

        private void SpawnDestroyedPersistentCue()
        {
            CreatePersistentCue("Structure Rubble Slab A", PrimitiveType.Cube, new Vector3(-0.18f, -0.34f, -0.18f), new Vector3(0.58f, 0.10f, 0.36f), new Color(0.13f, 0.11f, 0.09f, 1f));
            CreatePersistentCue("Structure Rubble Slab B", PrimitiveType.Cube, new Vector3(0.24f, -0.30f, 0.12f), new Vector3(0.42f, 0.09f, 0.48f), new Color(0.18f, 0.13f, 0.09f, 1f));
            CreatePersistentCue("Structure Collapse Marker", PrimitiveType.Cylinder, new Vector3(0f, 0.30f, 0f), new Vector3(0.10f, 0.38f, 0.10f), new Color(1f, 0.22f, 0.08f, 0.88f), transparent: true);
        }

        private static Vector3 GroundedPosition(Vector2 missionPosition, float height)
        {
            Vector3 position = DemoUnitView.MissionToWorld(missionPosition);
            position.y = DemoTerrainView.HeightAt(missionPosition) + height * 0.5f;
            return position;
        }

        private void CreateEffect(string effectName, PrimitiveType primitive, Vector3 position, Color color, float duration, Vector3 fromScale, Vector3 toScale)
        {
            GameObject effect = GameObject.CreatePrimitive(primitive);
            effect.name = effectName;
            effect.transform.position = position;
            Collider effectCollider = effect.GetComponent<Collider>();
            if (effectCollider != null)
            {
                Destroy(effectCollider);
            }

            DemoTransientEffect transient = effect.AddComponent<DemoTransientEffect>();
            transient.Begin(color, duration, fromScale, toScale);
        }

        private void CreatePersistentCue(
            string cueName,
            PrimitiveType primitive,
            Vector3 localPosition,
            Vector3 localScale,
            Color color,
            bool transparent = false)
        {
            GameObject cue = GameObject.CreatePrimitive(primitive);
            cue.name = Structure.Id + " " + cueName;
            cue.transform.SetParent(transform, false);
            cue.transform.localPosition = localPosition;
            cue.transform.localScale = localScale;
            Renderer cueRenderer = cue.GetComponent<Renderer>();
            if (cueRenderer != null)
            {
                cueRenderer.sharedMaterial = CreateOwnedMaterial(color, transparent);
            }

            Collider cueCollider = cue.GetComponent<Collider>();
            if (cueCollider != null)
            {
                Destroy(cueCollider);
            }
        }

        private Material CreateOwnedMaterial(Color color, bool transparent)
        {
            Shader shader = Shader.Find("Standard") ?? Shader.Find("Hidden/Internal-Colored");
            Material material = new(shader)
            {
                color = color
            };
            if (transparent)
            {
                material.SetFloat("_Mode", 3f);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }

            ownedMaterials.Add(material);
            return material;
        }

        private void OnDestroy()
        {
            for (int index = 0; index < ownedMaterials.Count; index++)
            {
                if (ownedMaterials[index] != null)
                {
                    Destroy(ownedMaterials[index]);
                }
            }
        }
    }
}

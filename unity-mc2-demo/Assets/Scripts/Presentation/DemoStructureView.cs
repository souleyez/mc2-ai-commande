using MC2Demo.BattleCore;
using UnityEngine;

namespace MC2Demo.Presentation
{
    public sealed class DemoStructureView : MonoBehaviour
    {
        public StructureState Structure { get; private set; }

        private Vector3 liveScale;
        private Renderer structureRenderer;
        private Color liveColor;
        private bool destroyedPoseApplied;

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

        private void Update()
        {
            if (Structure == null)
            {
                return;
            }

            ApplyDamageColor();
            if (!Structure.IsDestroyed || destroyedPoseApplied)
            {
                return;
            }

            SpawnDestructionEffects();
            transform.localScale = new Vector3(liveScale.x, liveScale.y * 0.28f, liveScale.z);
            transform.position = GroundedPosition(Structure.MissionPosition, transform.localScale.y);
            destroyedPoseApplied = true;
        }

        private void ApplyDamageColor()
        {
            if (structureRenderer == null || structureRenderer.sharedMaterial == null || Structure.IsDestroyed)
            {
                return;
            }

            Color damaged = new(0.55f, 0.20f, 0.12f);
            structureRenderer.sharedMaterial.color = Color.Lerp(damaged, liveColor, Structure.Structure);
        }

        private void SpawnDestructionEffects()
        {
            Vector3 center = GroundedPosition(Structure.MissionPosition, liveScale.y) + new Vector3(0f, liveScale.y * 0.25f, 0f);
            CreateEffect("Structure Blast", PrimitiveType.Sphere, center, new Color(1f, 0.42f, 0.08f, 0.72f), 0.55f, new Vector3(0.4f, 0.4f, 0.4f), new Vector3(4.6f, 1.6f, 4.6f));
            CreateEffect("Structure Smoke", PrimitiveType.Sphere, center + new Vector3(0f, 0.5f, 0f), new Color(0.12f, 0.12f, 0.12f, 0.52f), 1.8f, new Vector3(0.9f, 0.5f, 0.9f), new Vector3(3.8f, 3.2f, 3.8f));
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
    }
}

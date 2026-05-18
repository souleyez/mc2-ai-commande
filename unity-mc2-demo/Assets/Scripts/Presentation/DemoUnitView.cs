using System;
using System.Collections.Generic;
using MC2Demo.BattleCore;
using UnityEngine;

namespace MC2Demo.Presentation
{
    public sealed class DemoUnitView : MonoBehaviour
    {
        public UnitState Unit { get; private set; }
        private Vector3 liveScale;
        private Renderer unitRenderer;
        private Color liveColor = Color.white;
        private Color hitFlashColor = Color.white;
        private float hitFlashRemaining;
        private float hitFlashDuration = 0.18f;
        private bool destroyedPoseApplied;
        private bool ejectionEffectSpawned;
        private readonly Dictionary<string, GameObject> sectionParts = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> destroyedSections = new(StringComparer.OrdinalIgnoreCase);

        public void Bind(UnitState unit)
        {
            Unit = unit;
            name = unit.Id + " " + unit.UnitType;
            liveScale = transform.localScale;
            unitRenderer = GetComponent<Renderer>();
            if (unitRenderer != null && unitRenderer.sharedMaterial != null)
            {
                liveColor = unitRenderer.sharedMaterial.color;
            }

            CreateSectionParts();
            ApplyPosition();
        }

        private void Update()
        {
            ApplyPosition();
            ApplyHitFlash();
            ApplySectionDamageVisuals();
            ApplyDamagePose();
        }

        public void PulseHit(Color color, float duration = 0.18f)
        {
            if (Unit == null || Unit.IsDestroyed)
            {
                return;
            }

            hitFlashColor = color;
            hitFlashDuration = Mathf.Max(0.05f, duration);
            hitFlashRemaining = hitFlashDuration;
            ApplyHitFlash();
        }

        private void ApplyPosition()
        {
            if (Unit == null)
            {
                return;
            }

            Vector3 position = MissionToWorld(Unit.MissionPosition);
            position.y = DemoTerrainView.HeightAt(Unit.MissionPosition) + Mathf.Max(0.2f, transform.localScale.y * 0.5f);
            if (Unit.IsJumping)
            {
                position.y += Unit.JumpLift * 2.6f;
            }

            transform.position = position;
        }

        private void ApplySectionDamageVisuals()
        {
            if (Unit == null || Unit.Sections == null)
            {
                return;
            }

            for (int index = 0; index < Unit.Sections.Length; index++)
            {
                DamageSection section = Unit.Sections[index];
                if (!section.IsDestroyed || !destroyedSections.Add(section.Name))
                {
                    continue;
                }

                SpawnSectionDestroyedEffect(section.Name);
            }
        }

        private void ApplyHitFlash()
        {
            if (unitRenderer == null || unitRenderer.sharedMaterial == null || Unit == null || Unit.IsDestroyed)
            {
                return;
            }

            if (hitFlashRemaining <= 0f)
            {
                unitRenderer.sharedMaterial.color = liveColor;
                return;
            }

            float t = Mathf.Clamp01(hitFlashRemaining / hitFlashDuration);
            unitRenderer.sharedMaterial.color = Color.Lerp(liveColor, hitFlashColor, t);
            hitFlashRemaining = Mathf.Max(0f, hitFlashRemaining - Time.deltaTime);
        }

        private void ApplyDamagePose()
        {
            if (Unit == null || !Unit.IsDestroyed || destroyedPoseApplied)
            {
                return;
            }

            transform.localScale = new Vector3(liveScale.x, liveScale.y * 0.18f, liveScale.z);
            transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            destroyedPoseApplied = true;
        }

        private void CreateSectionParts()
        {
            if (unitRenderer == null || unitRenderer.sharedMaterial == null)
            {
                return;
            }

            CreateSectionPart("Cockpit", PrimitiveType.Sphere, new Vector3(0f, 0.55f, 0.30f), new Vector3(0.20f, 0.16f, 0.20f));
            CreateSectionPart("Left Arm", PrimitiveType.Cube, new Vector3(-0.58f, 0.05f, 0f), new Vector3(0.16f, 0.42f, 0.18f));
            CreateSectionPart("Right Arm", PrimitiveType.Cube, new Vector3(0.58f, 0.05f, 0f), new Vector3(0.16f, 0.42f, 0.18f));
            CreateSectionPart("Left Leg", PrimitiveType.Cube, new Vector3(-0.20f, -0.52f, 0f), new Vector3(0.14f, 0.28f, 0.16f));
            CreateSectionPart("Right Leg", PrimitiveType.Cube, new Vector3(0.20f, -0.52f, 0f), new Vector3(0.14f, 0.28f, 0.16f));
        }

        private void CreateSectionPart(string sectionName, PrimitiveType primitive, Vector3 localPosition, Vector3 localScale)
        {
            GameObject part = GameObject.CreatePrimitive(primitive);
            part.name = Unit.Id + " " + sectionName;
            part.transform.SetParent(transform, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            Renderer partRenderer = part.GetComponent<Renderer>();
            if (partRenderer != null)
            {
                partRenderer.sharedMaterial = unitRenderer.sharedMaterial;
            }

            sectionParts[sectionName] = part;
        }

        private void SpawnSectionDestroyedEffect(string sectionName)
        {
            if (string.Equals(sectionName, "Cockpit", StringComparison.OrdinalIgnoreCase))
            {
                DetachPart("Cockpit", new Vector3(0f, 0.8f, 0.45f), new Color(1f, 0.65f, 0.18f, 0.72f));
                SpawnCockpitEjection();
                return;
            }

            if (string.Equals(sectionName, "Left Arm", StringComparison.OrdinalIgnoreCase))
            {
                DetachPart("Left Arm", new Vector3(-0.9f, 0.25f, 0.15f), new Color(1f, 0.42f, 0.08f, 0.68f));
                SpawnDamageBurst(SectionWorldPoint(new Vector3(-0.62f, 0.08f, 0f)), 0.8f);
                return;
            }

            if (string.Equals(sectionName, "Right Arm", StringComparison.OrdinalIgnoreCase))
            {
                DetachPart("Right Arm", new Vector3(0.9f, 0.25f, 0.15f), new Color(1f, 0.42f, 0.08f, 0.68f));
                SpawnDamageBurst(SectionWorldPoint(new Vector3(0.62f, 0.08f, 0f)), 0.8f);
                return;
            }

            if (string.Equals(sectionName, "Legs", StringComparison.OrdinalIgnoreCase))
            {
                DetachPart("Left Leg", new Vector3(-0.38f, 0.12f, -0.25f), new Color(0.82f, 0.32f, 0.12f, 0.66f));
                DetachPart("Right Leg", new Vector3(0.38f, 0.12f, -0.25f), new Color(0.82f, 0.32f, 0.12f, 0.66f));
                SpawnDamageBurst(SectionWorldPoint(new Vector3(0f, -0.45f, 0f)), 1.0f);
                return;
            }

            SpawnDamageBurst(SectionWorldPoint(new Vector3(0f, 0.08f, 0f)), 1.15f);
        }

        private void DetachPart(string partName, Vector3 localImpulse, Color color)
        {
            if (!sectionParts.TryGetValue(partName, out GameObject part) || part == null)
            {
                return;
            }

            sectionParts.Remove(partName);
            Vector3 startScale = part.transform.lossyScale;
            part.transform.SetParent(null, true);
            part.transform.position += transform.TransformDirection(localImpulse);
            part.transform.rotation = Quaternion.Euler(18f, partName.GetHashCode() % 360, 72f);

            Collider partCollider = part.GetComponent<Collider>();
            if (partCollider != null)
            {
                Destroy(partCollider);
            }

            DemoTransientEffect transient = part.AddComponent<DemoTransientEffect>();
            transient.Begin(color, 1.25f, startScale, startScale * 0.42f);
        }

        private void SpawnDamageBurst(Vector3 position, float scale)
        {
            CreateTransient("Section Spark", PrimitiveType.Sphere, position, new Color(1f, 0.50f, 0.08f, 0.72f), 0.42f, Vector3.one * (0.10f * scale), Vector3.one * (0.70f * scale));
            CreateTransient("Section Smoke", PrimitiveType.Sphere, position + Vector3.up * 0.18f, new Color(0.10f, 0.10f, 0.10f, 0.45f), 1.25f, Vector3.one * (0.18f * scale), Vector3.one * (0.95f * scale));
        }

        private void SpawnCockpitEjection()
        {
            if (ejectionEffectSpawned)
            {
                return;
            }

            ejectionEffectSpawned = true;
            Vector3 start = SectionWorldPoint(new Vector3(0f, 0.7f, 0.34f));
            Vector3 end = start + Vector3.up * 1.9f + transform.TransformDirection(new Vector3(0.35f, 0f, 0.28f));
            CreateBeam("Ejection Trail", start, end, new Color(0.72f, 0.9f, 1f, 0.58f), 0.9f, 0.035f);
            CreateTransient("Pilot Ejection Pod", PrimitiveType.Sphere, end, new Color(0.88f, 0.95f, 1f, 0.82f), 1.35f, Vector3.one * 0.16f, Vector3.one * 0.32f);
            SpawnDamageBurst(start, 0.9f);
        }

        private Vector3 SectionWorldPoint(Vector3 localPoint)
        {
            return transform.TransformPoint(localPoint);
        }

        private void CreateTransient(string effectName, PrimitiveType primitive, Vector3 position, Color color, float duration, Vector3 fromScale, Vector3 toScale)
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

        private void CreateBeam(string effectName, Vector3 from, Vector3 to, Color color, float duration, float radius)
        {
            Vector3 direction = to - from;
            float length = direction.magnitude;
            if (length <= 0.01f)
            {
                return;
            }

            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beam.name = effectName;
            beam.transform.position = (from + to) * 0.5f;
            beam.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
            Collider beamCollider = beam.GetComponent<Collider>();
            if (beamCollider != null)
            {
                Destroy(beamCollider);
            }

            DemoTransientEffect transient = beam.AddComponent<DemoTransientEffect>();
            transient.Begin(color, duration, new Vector3(radius, length * 0.5f, radius), new Vector3(radius * 0.35f, length * 0.5f, radius * 0.35f));
        }

        public static Vector3 MissionToWorld(Vector2 missionPoint)
        {
            return DemoTerrainView.MissionToWorld(missionPoint);
        }

        public static Vector2 WorldToMission(Vector3 worldPoint)
        {
            return DemoTerrainView.WorldToMission(worldPoint);
        }
    }
}

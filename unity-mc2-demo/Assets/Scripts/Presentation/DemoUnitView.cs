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
        private bool legFailurePoseApplied;
        private bool ejectionEffectSpawned;
        private readonly Dictionary<string, GameObject> sectionParts = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> destroyedSections = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<Material> ownedMaterials = new();

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
            ApplySectionPartDamageColors();
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

        private void ApplySectionPartDamageColors()
        {
            if (Unit == null || Unit.Sections == null)
            {
                return;
            }

            for (int index = 0; index < Unit.Sections.Length; index++)
            {
                DamageSection section = Unit.Sections[index];
                ApplySectionPartDamageColor(section.Name, SectionDamageColor(section));
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
            if (Unit == null)
            {
                return;
            }

            if (!Unit.IsDestroyed && !legFailurePoseApplied && HasDestroyedSection("Legs"))
            {
                transform.localScale = new Vector3(liveScale.x, liveScale.y * 0.62f, liveScale.z);
                transform.rotation = Quaternion.Euler(0f, 0f, Unit.IsPlayerUnit ? -7f : 7f);
                legFailurePoseApplied = true;
            }

            if (!Unit.IsDestroyed || destroyedPoseApplied)
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
                partRenderer.sharedMaterial = CreateOwnedMaterial(liveColor);
            }

            sectionParts[sectionName] = part;
        }

        private void SpawnSectionDestroyedEffect(string sectionName)
        {
            if (string.Equals(sectionName, "Cockpit", StringComparison.OrdinalIgnoreCase))
            {
                CreateDamageScar("Cockpit Breach", new Vector3(0f, 0.45f, 0.34f), new Vector3(0.32f, 0.08f, 0.24f), new Color(0.02f, 0.02f, 0.02f, 1f));
                CreateDamageBeacon("Cockpit Ejection Beacon", new Vector3(0f, 0.98f, 0.36f), new Color(0.58f, 0.92f, 1f, 1f), 0.055f, 0.42f);
                CreateDamageFlag("Cockpit Lost Flag", new Vector3(0f, 1.12f, 0.18f), new Vector3(0.46f, 0.07f, 0.11f), new Color(0.62f, 0.95f, 1f, 1f));
                CreateDamageLink("Cockpit Ejection Rail", new Vector3(-0.16f, 0.64f, 0.36f), new Vector3(0.16f, 1.05f, 0.46f), new Color(0.58f, 0.92f, 1f, 0.92f), 0.018f);
                DetachPart("Cockpit", new Vector3(0f, 0.8f, 0.45f), new Color(1f, 0.65f, 0.18f, 0.72f));
                SpawnCockpitEjection();
                return;
            }

            if (string.Equals(sectionName, "Left Arm", StringComparison.OrdinalIgnoreCase))
            {
                CreateDamageScar("Left Shoulder Breach", new Vector3(-0.44f, 0.10f, 0.02f), new Vector3(0.16f, 0.34f, 0.20f), new Color(0.06f, 0.04f, 0.02f, 1f));
                CreateDamageBeacon("Left Arm Missing Beacon", new Vector3(-0.58f, 0.42f, 0.06f), new Color(1f, 0.34f, 0.08f, 1f), 0.045f, 0.32f);
                CreateDamageFlag("Left Arm Lost Flag", new Vector3(-0.32f, 0.92f, 0.12f), new Vector3(0.28f, 0.08f, 0.10f), new Color(1f, 0.32f, 0.08f, 1f));
                CreateDamageLink("Left Broken Cable", new Vector3(-0.48f, 0.25f, 0.08f), new Vector3(-0.78f, 0.02f, 0.20f), new Color(1f, 0.52f, 0.12f, 0.92f), 0.015f);
                DetachPart("Left Arm", new Vector3(-0.9f, 0.25f, 0.15f), new Color(1f, 0.42f, 0.08f, 0.68f));
                SpawnDamageBurst(SectionWorldPoint(new Vector3(-0.62f, 0.08f, 0f)), 0.8f);
                return;
            }

            if (string.Equals(sectionName, "Right Arm", StringComparison.OrdinalIgnoreCase))
            {
                CreateDamageScar("Right Shoulder Breach", new Vector3(0.44f, 0.10f, 0.02f), new Vector3(0.16f, 0.34f, 0.20f), new Color(0.06f, 0.04f, 0.02f, 1f));
                CreateDamageBeacon("Right Arm Missing Beacon", new Vector3(0.58f, 0.42f, 0.06f), new Color(1f, 0.34f, 0.08f, 1f), 0.045f, 0.32f);
                CreateDamageFlag("Right Arm Lost Flag", new Vector3(0.32f, 0.92f, 0.12f), new Vector3(0.28f, 0.08f, 0.10f), new Color(1f, 0.32f, 0.08f, 1f));
                CreateDamageLink("Right Broken Cable", new Vector3(0.48f, 0.25f, 0.08f), new Vector3(0.78f, 0.02f, 0.20f), new Color(1f, 0.52f, 0.12f, 0.92f), 0.015f);
                DetachPart("Right Arm", new Vector3(0.9f, 0.25f, 0.15f), new Color(1f, 0.42f, 0.08f, 0.68f));
                SpawnDamageBurst(SectionWorldPoint(new Vector3(0.62f, 0.08f, 0f)), 0.8f);
                return;
            }

            if (string.Equals(sectionName, "Legs", StringComparison.OrdinalIgnoreCase))
            {
                CreateDamageScar("Leg Failure Scorch", new Vector3(0f, -0.36f, 0.05f), new Vector3(0.52f, 0.12f, 0.26f), new Color(0.05f, 0.04f, 0.03f, 1f));
                CreateDamageBeacon("Leg Failure Beacon", new Vector3(0f, -0.10f, 0.12f), new Color(1f, 0.22f, 0.10f, 1f), 0.06f, 0.30f);
                CreateDamageFlag("Leg Failure Flag", new Vector3(0f, 0.78f, 0.14f), new Vector3(0.54f, 0.08f, 0.10f), new Color(1f, 0.12f, 0.08f, 1f));
                CreateDamageFlag("Leg Failure Crossbar", new Vector3(0f, -0.28f, 0.18f), new Vector3(0.70f, 0.08f, 0.10f), new Color(1f, 0.16f, 0.08f, 1f));
                CreateDamageLink("Left Leg Broken Cable", new Vector3(-0.18f, -0.28f, 0.08f), new Vector3(-0.42f, -0.58f, 0.18f), new Color(1f, 0.42f, 0.10f, 0.90f), 0.014f);
                CreateDamageLink("Right Leg Broken Cable", new Vector3(0.18f, -0.28f, 0.08f), new Vector3(0.42f, -0.58f, 0.18f), new Color(1f, 0.42f, 0.10f, 0.90f), 0.014f);
                DetachPart("Left Leg", new Vector3(-0.38f, 0.12f, -0.25f), new Color(0.82f, 0.32f, 0.12f, 0.66f));
                DetachPart("Right Leg", new Vector3(0.38f, 0.12f, -0.25f), new Color(0.82f, 0.32f, 0.12f, 0.66f));
                SpawnDamageBurst(SectionWorldPoint(new Vector3(0f, -0.45f, 0f)), 1.0f);
                return;
            }

            CreateDamageScar("Core Damage Scorch", new Vector3(0f, 0.10f, 0.16f), new Vector3(0.40f, 0.12f, 0.26f), new Color(0.05f, 0.04f, 0.03f, 1f));
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
            Vector3 startPosition = part.transform.position;
            Vector3 endPosition = startPosition + transform.TransformDirection(localImpulse);
            part.transform.SetParent(null, true);
            part.transform.position = startPosition;
            part.transform.rotation = Quaternion.Euler(18f, partName.GetHashCode() % 360, 72f);

            Collider partCollider = part.GetComponent<Collider>();
            if (partCollider != null)
            {
                Destroy(partCollider);
            }

            DemoTransientEffect transient = part.AddComponent<DemoTransientEffect>();
            transient.BeginMoving(color, 2.85f, startScale, startScale * 0.62f, endPosition);
        }

        private void SpawnDamageBurst(Vector3 position, float scale)
        {
            CreateTransient("Section Spark", PrimitiveType.Sphere, position, new Color(1f, 0.50f, 0.08f, 0.72f), 0.42f, Vector3.one * (0.10f * scale), Vector3.one * (0.70f * scale));
            CreateTransient("Section Smoke", PrimitiveType.Sphere, position + Vector3.up * 0.18f, new Color(0.10f, 0.10f, 0.10f, 0.45f), 1.25f, Vector3.one * (0.18f * scale), Vector3.one * (0.95f * scale));
        }

        public static string SectionDamageCueSummary()
        {
            return "Arms=missing-socket+flag Legs=collapse+red-cross Cockpit=breach+ejection-pod";
        }

        private void SpawnCockpitEjection()
        {
            if (ejectionEffectSpawned)
            {
                return;
            }

            ejectionEffectSpawned = true;
            Vector3 start = SectionWorldPoint(new Vector3(0f, 0.7f, 0.34f));
            Vector3 end = start + Vector3.up * 2.65f + transform.TransformDirection(new Vector3(0.45f, 0f, 0.36f));
            CreateBeam("Ejection Trail", start, end, new Color(0.72f, 0.9f, 1f, 0.68f), 1.2f, 0.045f);
            CreateBeam("Ejection Smoke Trail", start + Vector3.down * 0.08f, end + Vector3.down * 0.35f, new Color(0.08f, 0.10f, 0.12f, 0.36f), 1.6f, 0.075f);
            CreateTransient("Ejection Flash", PrimitiveType.Sphere, start, new Color(1f, 0.75f, 0.28f, 0.78f), 0.48f, Vector3.one * 0.18f, Vector3.one * 0.95f);
            CreateMovingTransient("Pilot Ejection Pod", PrimitiveType.Sphere, start, end, new Color(0.88f, 0.95f, 1f, 0.92f), 1.75f, Vector3.one * 0.18f, Vector3.one * 0.38f);
            SpawnDamageBurst(start, 1.05f);
        }

        private bool HasDestroyedSection(string sectionName)
        {
            if (Unit == null || Unit.Sections == null)
            {
                return false;
            }

            for (int index = 0; index < Unit.Sections.Length; index++)
            {
                DamageSection section = Unit.Sections[index];
                if (section.IsDestroyed && string.Equals(section.Name, sectionName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void ApplySectionPartDamageColor(string sectionName, Color color)
        {
            if (IsSectionName(sectionName, "Cockpit") || IsSectionName(sectionName, "Turret"))
            {
                SetSectionPartColor("Cockpit", color);
                return;
            }

            if (IsSectionName(sectionName, "Left Arm") || IsSectionName(sectionName, "Left"))
            {
                SetSectionPartColor("Left Arm", color);
                return;
            }

            if (IsSectionName(sectionName, "Right Arm") || IsSectionName(sectionName, "Right"))
            {
                SetSectionPartColor("Right Arm", color);
                return;
            }

            if (IsSectionName(sectionName, "Legs"))
            {
                SetSectionPartColor("Left Leg", color);
                SetSectionPartColor("Right Leg", color);
            }
        }

        private void SetSectionPartColor(string partName, Color color)
        {
            if (!sectionParts.TryGetValue(partName, out GameObject part) || part == null)
            {
                return;
            }

            Renderer partRenderer = part.GetComponent<Renderer>();
            if (partRenderer != null && partRenderer.sharedMaterial != null)
            {
                partRenderer.sharedMaterial.color = color;
            }
        }

        private static bool IsSectionName(string sectionName, string expected)
        {
            return string.Equals(sectionName, expected, StringComparison.OrdinalIgnoreCase);
        }

        private Color SectionDamageColor(DamageSection section)
        {
            if (section.IsDestroyed)
            {
                return new Color(0.10f, 0.08f, 0.06f, 1f);
            }

            if (section.Ratio < 0.35f)
            {
                return Color.Lerp(liveColor, new Color(1f, 0.16f, 0.08f, 1f), 0.82f);
            }

            if (section.Ratio < 0.70f)
            {
                return Color.Lerp(liveColor, new Color(1f, 0.72f, 0.12f, 1f), 0.62f);
            }

            return liveColor;
        }

        private void CreateDamageScar(string scarName, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject scar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            scar.name = Unit.Id + " " + scarName;
            scar.transform.SetParent(transform, false);
            scar.transform.localPosition = localPosition;
            scar.transform.localScale = localScale;
            Renderer scarRenderer = scar.GetComponent<Renderer>();
            if (scarRenderer != null)
            {
                scarRenderer.sharedMaterial = CreateOwnedMaterial(color);
            }

            Collider scarCollider = scar.GetComponent<Collider>();
            if (scarCollider != null)
            {
                Destroy(scarCollider);
            }
        }

        private void CreateDamageBeacon(string beaconName, Vector3 localPosition, Color color, float radius, float height)
        {
            GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beacon.name = Unit.Id + " " + beaconName;
            beacon.transform.SetParent(transform, false);
            beacon.transform.localPosition = localPosition;
            beacon.transform.localScale = new Vector3(radius, height * 0.5f, radius);
            Renderer beaconRenderer = beacon.GetComponent<Renderer>();
            if (beaconRenderer != null)
            {
                beaconRenderer.sharedMaterial = CreateOwnedMaterial(color);
            }

            Collider beaconCollider = beacon.GetComponent<Collider>();
            if (beaconCollider != null)
            {
                Destroy(beaconCollider);
            }
        }

        private void CreateDamageFlag(string flagName, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject flag = GameObject.CreatePrimitive(PrimitiveType.Cube);
            flag.name = Unit.Id + " " + flagName;
            flag.transform.SetParent(transform, false);
            flag.transform.localPosition = localPosition;
            flag.transform.localScale = localScale;
            Renderer flagRenderer = flag.GetComponent<Renderer>();
            if (flagRenderer != null)
            {
                flagRenderer.sharedMaterial = CreateOwnedMaterial(color);
            }

            Collider flagCollider = flag.GetComponent<Collider>();
            if (flagCollider != null)
            {
                Destroy(flagCollider);
            }
        }

        private void CreateDamageLink(string linkName, Vector3 localStart, Vector3 localEnd, Color color, float radius)
        {
            Vector3 direction = localEnd - localStart;
            float length = direction.magnitude;
            if (length <= 0.01f)
            {
                return;
            }

            GameObject link = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            link.name = Unit.Id + " " + linkName;
            link.transform.SetParent(transform, false);
            link.transform.localPosition = (localStart + localEnd) * 0.5f;
            link.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
            link.transform.localScale = new Vector3(radius, length * 0.5f, radius);
            Renderer linkRenderer = link.GetComponent<Renderer>();
            if (linkRenderer != null)
            {
                linkRenderer.sharedMaterial = CreateOwnedMaterial(color);
            }

            Collider linkCollider = link.GetComponent<Collider>();
            if (linkCollider != null)
            {
                Destroy(linkCollider);
            }
        }

        private Material CreateOwnedMaterial(Color color)
        {
            Shader shader = Shader.Find("Standard") ?? Shader.Find("Hidden/Internal-Colored");
            Material material = new(shader)
            {
                color = color
            };
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

        private void CreateMovingTransient(string effectName, PrimitiveType primitive, Vector3 start, Vector3 end, Color color, float duration, Vector3 fromScale, Vector3 toScale)
        {
            GameObject effect = GameObject.CreatePrimitive(primitive);
            effect.name = effectName;
            effect.transform.position = start;
            Collider effectCollider = effect.GetComponent<Collider>();
            if (effectCollider != null)
            {
                Destroy(effectCollider);
            }

            DemoTransientEffect transient = effect.AddComponent<DemoTransientEffect>();
            transient.BeginMoving(color, duration, fromScale, toScale, end);
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

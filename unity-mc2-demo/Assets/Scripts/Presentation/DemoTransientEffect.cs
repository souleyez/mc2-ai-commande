using UnityEngine;

namespace MC2Demo.Presentation
{
    public sealed class DemoTransientEffect : MonoBehaviour
    {
        private Material material;
        private Color startColor;
        private float lifetime = 1f;
        private float age;
        private Vector3 startScale;
        private Vector3 endScale;
        private bool hasEndPosition;
        private Vector3 startPosition;
        private Vector3 endPosition;

        public void Begin(Color color, float duration, Vector3 fromScale, Vector3 toScale)
        {
            startColor = color;
            lifetime = Mathf.Max(0.05f, duration);
            startScale = fromScale;
            endScale = toScale;
            transform.localScale = startScale;
            hasEndPosition = false;

            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Standard") ?? Shader.Find("Hidden/Internal-Colored");
                material = new Material(shader)
                {
                    color = color
                };
                material.SetFloat("_Mode", 3f);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                renderer.sharedMaterial = material;
            }
        }

        public void BeginMoving(Color color, float duration, Vector3 fromScale, Vector3 toScale, Vector3 targetPosition)
        {
            Begin(color, duration, fromScale, toScale);
            hasEndPosition = true;
            startPosition = transform.position;
            endPosition = targetPosition;
        }

        private void Update()
        {
            age += Time.deltaTime;
            float t = Mathf.Clamp01(age / lifetime);
            float easedT = 1f - Mathf.Pow(1f - t, 2f);
            if (hasEndPosition)
            {
                transform.position = Vector3.Lerp(startPosition, endPosition, easedT);
            }

            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            if (material != null)
            {
                Color color = startColor;
                color.a *= 1f - t;
                material.color = color;
            }

            if (t >= 1f)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (material != null)
            {
                Destroy(material);
            }
        }
    }
}

using UnityEngine;

namespace ImmortalOrgan
{
    public sealed class PulseEffect : MonoBehaviour
    {
        private float age;
        private float lifetime;
        private float radius;
        private Material material;
        private Color original;

        public static void Create(Vector3 worldPosition, Color color, float maxRadius, float duration)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "L-Cell pulse (temporary)";
            Destroy(go.GetComponent<Collider>());
            go.transform.position = worldPosition;
            go.transform.localScale = Vector3.one * .12f;
            var effect = go.AddComponent<PulseEffect>();
            effect.lifetime = duration;
            effect.radius = maxRadius;
            effect.original = color;
            effect.material = WorldFactory.MakeMaterial(color, true);
            go.GetComponent<Renderer>().sharedMaterial = effect.material;
        }

        private void Update()
        {
            age += Time.deltaTime;
            float t = Mathf.Clamp01(age / Mathf.Max(.05f, lifetime));
            transform.localScale = Vector3.one * Mathf.Lerp(.1f, radius, t);
            material.color = Color.Lerp(original, new Color(original.r, original.g, original.b, 0f), t);
            if (age >= lifetime) Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (material != null) Destroy(material);
        }
    }
}

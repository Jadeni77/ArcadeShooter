using System.Collections.Generic;
using ArcadeShooter.Enemies;
using UnityEngine;

namespace ArcadeShooter.Combat
{
    public enum ImpactSurface : byte { Concrete, Metal, Enemy }

    // A small pool keeps automatic fire from allocating a GameObject for every impact.
    public sealed class ImpactEffect : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float lifetime = 0.25f;
        private static readonly Dictionary<GameObject, Stack<ImpactEffect>> Pools = new Dictionary<GameObject, Stack<ImpactEffect>>();
        private const int PoolLimit = 48;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly Color ConcreteColor = new Color(0.72f, 0.72f, 0.66f);
        private static readonly Color MetalColor = new Color(1f, 0.72f, 0.18f);
        private static readonly Color EnemyColor = new Color(1f, 0.22f, 0.12f);

        private GameObject sourcePrefab;
        private Renderer effectRenderer;
        private MaterialPropertyBlock colorBlock;
        private Vector3 baseScale;
        private float currentSize;
        private float expiresAt;
        private readonly Transform[] fragments = new Transform[4];
        private readonly Vector3[] directions = new Vector3[4];
        private ImpactSurface currentSurface;

        public static ImpactSurface Classify(Collider collider)
        {
            if (collider.GetComponentInParent<EnemyHealth>() != null) return ImpactSurface.Enemy;
            SurfaceFeedback surface = collider.GetComponentInParent<SurfaceFeedback>();
            if (surface != null) return surface.Surface;
            Renderer renderer = collider.GetComponent<Renderer>() ?? collider.GetComponentInParent<Renderer>();
            string materialName = renderer != null && renderer.sharedMaterial != null
                ? renderer.sharedMaterial.name : string.Empty;
            return materialName.IndexOf("metal", System.StringComparison.OrdinalIgnoreCase) >= 0
                ? ImpactSurface.Metal : ImpactSurface.Concrete;
        }

        public static void Spawn(GameObject prefab, Vector3 position, Vector3 normal, ImpactSurface surface)
        {
            if (prefab == null) return;
            if (!Pools.TryGetValue(prefab, out Stack<ImpactEffect> pool))
            {
                pool = new Stack<ImpactEffect>();
                Pools.Add(prefab, pool);
            }
            ImpactEffect effect = null;
            while (pool.Count > 0 && effect == null) effect = pool.Pop();
            if (effect == null)
            {
                GameObject instance = Instantiate(prefab);
                effect = instance.GetComponent<ImpactEffect>();
                if (effect == null) { Destroy(instance); return; }
                effect.sourcePrefab = prefab;
            }
            effect.transform.SetPositionAndRotation(position + normal * 0.015f,
                Quaternion.FromToRotation(Vector3.forward, normal));
            effect.gameObject.SetActive(true);
            effect.Show(surface);
        }

        private void Awake()
        {
            effectRenderer = GetComponentInChildren<Renderer>();
            colorBlock = new MaterialPropertyBlock();
            baseScale = transform.localScale;
            MeshFilter sourceMesh = GetComponent<MeshFilter>();
            if (sourceMesh == null || effectRenderer == null) return;
            effectRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            for (int i = 0; i < fragments.Length; i++)
            {
                var piece = new GameObject("Impact Speck", typeof(MeshFilter), typeof(MeshRenderer));
                piece.transform.SetParent(transform, false);
                piece.GetComponent<MeshFilter>().sharedMesh = sourceMesh.sharedMesh;
                MeshRenderer pieceRenderer = piece.GetComponent<MeshRenderer>();
                pieceRenderer.sharedMaterial = effectRenderer.sharedMaterial;
                pieceRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                pieceRenderer.receiveShadows = false;
                fragments[i] = piece.transform;
            }
        }

        private void Show(ImpactSurface surface)
        {
            expiresAt = Time.time + lifetime;
            currentSurface = surface;
            currentSize = surface == ImpactSurface.Enemy ? 1.5f : surface == ImpactSurface.Metal ? 0.7f : 1f;
            transform.localScale = baseScale * currentSize;
            if (effectRenderer == null) return;
            colorBlock.SetColor(BaseColor, surface == ImpactSurface.Enemy ? EnemyColor :
                surface == ImpactSurface.Metal ? MetalColor : ConcreteColor);
            effectRenderer.SetPropertyBlock(colorBlock);
            for (int i = 0; i < fragments.Length; i++)
            {
                if (fragments[i] == null) continue;
                float angle = (i + Random.Range(-0.12f, 0.12f)) * Mathf.PI * 0.5f;
                directions[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle),
                    surface == ImpactSurface.Metal ? 0.45f : 0.2f).normalized;
                fragments[i].GetComponent<Renderer>().SetPropertyBlock(colorBlock);
            }
            UpdateFragments(0f);
        }

        private void UpdateFragments(float progress)
        {
            for (int i = 0; i < fragments.Length; i++)
            {
                if (fragments[i] == null) continue;
                fragments[i].localPosition = directions[i] * (progress * 1.5f);
                fragments[i].localRotation = Quaternion.LookRotation(directions[i]);
                Vector3 shape = currentSurface == ImpactSurface.Metal
                    ? new Vector3(0.12f, 0.12f, 0.55f) : Vector3.one * 0.35f;
                fragments[i].localScale = shape * Mathf.Max(0.01f, 1f - progress);
            }
        }

        private void Update()
        {
            if (expiresAt <= 0f) return;
            float remaining = (expiresAt - Time.time) / lifetime;
            if (remaining <= 0f)
            {
                expiresAt = 0f;
                gameObject.SetActive(false);
                if (sourcePrefab != null && Pools.TryGetValue(sourcePrefab, out Stack<ImpactEffect> pool) &&
                    pool.Count < PoolLimit) pool.Push(this);
                else Destroy(gameObject);
                return;
            }
            transform.localScale = baseScale * (currentSize * remaining);
            UpdateFragments(1f - Mathf.Clamp01(remaining));
        }
    }
}

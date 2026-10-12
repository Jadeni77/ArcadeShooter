using UnityEngine;

namespace ArcadeShooter.Combat
{
    [RequireComponent(typeof(Collider))]
    public sealed class ShootingTarget : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1f)] private float maxHealth = 60f;
        [SerializeField, Min(0.1f)] private float resetDelay = 4f;
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Color damagedColor = new Color(1f, 0.25f, 0.12f);

        private Collider targetCollider;
        private MaterialPropertyBlock properties;
        private float health;
        private float resetAt;
        private float flashUntil;
        private bool destroyed;

        public float Health => health;
        public float MaxHealth => maxHealth;

        private void Awake()
        {
            targetCollider = GetComponent<Collider>();
            if (targetRenderer == null)
                targetRenderer = GetComponent<Renderer>();
            properties = new MaterialPropertyBlock();
            ResetTarget();
        }

        private void Update()
        {
            if (destroyed && Time.time >= resetAt)
                ResetTarget();
            else if (!destroyed && flashUntil > 0f && Time.time >= flashUntil)
            {
                flashUntil = 0f;
                targetRenderer?.SetPropertyBlock(null);
            }
        }

        public bool ApplyDamage(float amount, Vector3 hitPoint)
        {
            if (destroyed || amount <= 0f)
                return false;

            health = Mathf.Max(0f, health - amount);
            if (health <= 0f)
            {
                destroyed = true;
                targetCollider.enabled = false;
                if (targetRenderer != null)
                    targetRenderer.enabled = false;
                resetAt = Time.time + resetDelay;
            }
            else if (targetRenderer != null)
            {
                if (properties == null)
                    properties = new MaterialPropertyBlock();
                properties.SetColor("_BaseColor", damagedColor);
                targetRenderer.SetPropertyBlock(properties);
                flashUntil = Time.time + 0.12f;
            }
            return true;
        }

        [ContextMenu("Reset Target")]
        public void ResetTarget()
        {
            health = maxHealth;
            destroyed = false;
            resetAt = 0f;
            flashUntil = 0f;
            if (targetCollider == null)
                targetCollider = GetComponent<Collider>();
            targetCollider.enabled = true;
            if (targetRenderer == null)
                targetRenderer = GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                targetRenderer.enabled = true;
                targetRenderer.SetPropertyBlock(null);
            }
        }

        private void OnValidate()
        {
            maxHealth = Mathf.Max(1f, maxHealth);
            resetDelay = Mathf.Max(0.1f, resetDelay);
        }
    }
}

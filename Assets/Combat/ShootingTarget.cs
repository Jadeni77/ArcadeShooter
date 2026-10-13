using UnityEngine;
using Unity.Netcode;

namespace ArcadeShooter.Combat
{
    [RequireComponent(typeof(Collider))]
    public sealed class ShootingTarget : NetworkBehaviour, IDamageable
    {
        [SerializeField, Min(1f)] private float maxHealth = 60f;
        [SerializeField, Min(0.1f)] private float resetDelay = 4f;
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Color damagedColor = new Color(1f, 0.25f, 0.12f);

        private Collider targetCollider;
        private MaterialPropertyBlock properties;
        private readonly NetworkVariable<float> health = new NetworkVariable<float>(60f);
        private float resetAt;
        private float flashUntil;

        public float Health => health.Value;
        public float MaxHealth => maxHealth;

        private void Awake()
        {
            targetCollider = GetComponent<Collider>();
            if (targetRenderer == null)
                targetRenderer = GetComponent<Renderer>();
            properties = new MaterialPropertyBlock();
        }

        public override void OnNetworkSpawn()
        {
            health.OnValueChanged += OnHealthChanged;
            if (IsServer) health.Value = maxHealth;
            ApplyVisual();
        }

        public override void OnNetworkDespawn() => health.OnValueChanged -= OnHealthChanged;

        private void Update()
        {
            if (IsSpawned && IsServer && Health <= 0f && Time.time >= resetAt)
                ResetTarget();
            else if (Health > 0f && flashUntil > 0f && Time.time >= flashUntil)
            {
                flashUntil = 0f;
                targetRenderer?.SetPropertyBlock(null);
            }
        }

        public bool ApplyDamage(float amount, Vector3 hitPoint)
        {
            if (!IsServer || Health <= 0f || amount <= 0f)
                return false;

            health.Value = Mathf.Max(0f, Health - amount);
            return true;
        }

        private void OnHealthChanged(float previous, float current)
        {
            if (current < previous && current <= 0f && IsServer)
                resetAt = Time.time + resetDelay;
            ApplyVisual();
        }

        private void ApplyVisual()
        {
            bool destroyed = Health <= 0f;
            if (targetCollider != null) targetCollider.enabled = !destroyed;
            if (targetRenderer != null) targetRenderer.enabled = !destroyed;
            if (!destroyed && targetRenderer != null && Health < maxHealth)
            {
                properties.SetColor("_BaseColor", damagedColor);
                targetRenderer.SetPropertyBlock(properties);
                flashUntil = Time.time + 0.12f;
            }
        }

        [ContextMenu("Reset Target")]
        public void ResetTarget()
        {
            if (Application.isPlaying && !IsServer) return;
            health.Value = maxHealth;
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

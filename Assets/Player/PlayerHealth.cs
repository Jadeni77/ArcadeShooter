using System;
using ArcadeShooter.Combat;
using ArcadeShooter.Missions;
using Unity.Netcode;
using UnityEngine;

namespace ArcadeShooter.Player
{
    public sealed class PlayerHealth : NetworkBehaviour, IDamageable
    {
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField] private FirstPersonMovement movement;
        [SerializeField] private AutomaticRifle rifle;

        private readonly NetworkVariable<float> health = new NetworkVariable<float>(100f);
        public float Health => health.Value;
        public float MaxHealth => maxHealth;
        public bool IsDead => Health <= 0f;
        public event Action<float, float> HealthChanged;
        public event Action Died;

        private void Awake()
        {
            if (movement == null) movement = GetComponent<FirstPersonMovement>();
            if (rifle == null) rifle = GetComponent<AutomaticRifle>();
        }

        public override void OnNetworkSpawn()
        {
            health.OnValueChanged += OnHealthChanged;
            if (IsServer) health.Value = maxHealth;
            OnHealthChanged(maxHealth, health.Value);
        }

        public override void OnNetworkDespawn()
        {
            health.OnValueChanged -= OnHealthChanged;
        }

        public bool ApplyDamage(float amount, Vector3 hitPoint)
        {
            if (!IsServer || IsDead || amount <= 0f || float.IsNaN(amount) || float.IsInfinity(amount) ||
                (MissionController.Instance != null && !MissionController.Instance.IsRunning)) return false;
            health.Value = Mathf.Max(0f, Health - amount);
            return true;
        }

        private void OnHealthChanged(float previous, float current)
        {
            HealthChanged?.Invoke(current, maxHealth);
            if (current <= 0f && previous > 0f)
            {
                if (rifle != null) rifle.enabled = false;
                if (movement != null) movement.enabled = false;
                Died?.Invoke();
            }
        }

        private void OnValidate() => maxHealth = Mathf.Max(1f, maxHealth);
    }
}

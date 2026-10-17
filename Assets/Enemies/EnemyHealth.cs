using System;
using System.Collections;
using ArcadeShooter.Combat;
using ArcadeShooter.Missions;
using Unity.Netcode;
using UnityEngine;

namespace ArcadeShooter.Enemies
{
    public sealed class EnemyHealth : NetworkBehaviour, IDamageable
    {
        [SerializeField, Min(1f)] private float maxHealth = 60f;
        private readonly NetworkVariable<float> health = new NetworkVariable<float>(60f);
        private ulong? damageSourcePlayer;
        public float Health => health.Value;
        public float MaxHealth => maxHealth;
        public bool IsDead => Health <= 0f;
        public event Action Damaged;
        public event Action Died;

        public override void OnNetworkSpawn()
        {
            health.OnValueChanged += OnHealthChanged;
            if (IsServer)
            {
                health.Value = maxHealth;
                MissionScoreTracker.Instance?.RegisterEnemySpawn(NetworkObjectId);
            }
            if (IsDead) Died?.Invoke();
        }

        public override void OnNetworkDespawn()
        {
            health.OnValueChanged -= OnHealthChanged;
            if (IsDead && NetworkObject.InScenePlaced)
                gameObject.SetActive(false);
        }

        public bool ApplyDamage(float amount, Vector3 hitPoint)
        {
            if (!IsServer || IsDead || amount <= 0f || float.IsNaN(amount) || float.IsInfinity(amount) ||
                (MissionController.Instance != null && !MissionController.Instance.IsRunning)) return false;
            health.Value = Mathf.Max(0f, Health - amount);
            return true;
        }

        public bool ApplyDamageFromPlayer(float amount, Vector3 hitPoint, ulong playerId)
        {
            // The source only exists while the authoritative health mutation raises
            // its existing callbacks. Unattributed damage never awards player points.
            damageSourcePlayer = playerId;
            try { return ApplyDamage(amount, hitPoint); }
            finally { damageSourcePlayer = null; }
        }

        private void OnHealthChanged(float previous, float current)
        {
            if (IsServer && current < previous && damageSourcePlayer.HasValue)
            {
                MissionScoreTracker.Instance?.RecordHit(damageSourcePlayer.Value);
                if (current <= 0f && previous > 0f)
                    MissionScoreTracker.Instance?.RecordKill(damageSourcePlayer.Value, NetworkObjectId);
            }
            if (current < previous) Damaged?.Invoke();
            if (current <= 0f && previous > 0f)
            {
                Died?.Invoke();
                if (IsServer) StartCoroutine(DespawnAfterFeedback());
            }
        }

        private IEnumerator DespawnAfterFeedback()
        {
            yield return new WaitForSeconds(0.6f);
            if (NetworkObject != null && NetworkObject.IsSpawned)
            {
                bool sceneObject = NetworkObject.InScenePlaced;
                NetworkObject.Despawn(!sceneObject);
                if (sceneObject) gameObject.SetActive(false);
            }
        }

        private void OnValidate() => maxHealth = Mathf.Max(1f, maxHealth);
    }
}

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
        private bool deathPresented;
        public float Health => health.Value;
        public float MaxHealth => maxHealth * (MissionController.Instance != null &&
            MissionController.Instance.Difficulty == ArcadeShooter.Progression.MissionDifficulty.Hard ? 1.25f : 1f);
        public bool IsDead => Health <= 0f;
        public event Action Damaged;
        public event Action Died;
        public event Action AttackPresented;
        public event Action AlertPresented;

        public override void OnNetworkSpawn()
        {
            health.OnValueChanged += OnHealthChanged;
            if (IsServer)
            {
                health.Value = MaxHealth;
                MissionScoreTracker.Instance?.RegisterEnemySpawn(NetworkObjectId);
            }
            if (IsDead) PresentDeath();
        }

        public void ApplyMissionDifficulty()
        {
            if (IsServer && !IsDead) health.Value = MaxHealth;
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
                PresentDeath();
                if (IsServer) StartCoroutine(DespawnAfterFeedback());
            }
        }

        private void PresentDeath()
        {
            if (deathPresented) return;
            deathPresented = true;
            Died?.Invoke();
        }

        public void BroadcastAttack()
        {
            if (IsServer && IsSpawned && !IsDead) PresentAttackRpc();
        }

        public void BroadcastAlert()
        {
            if (IsServer && IsSpawned && !IsDead) PresentAlertRpc();
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void PresentAttackRpc()
        {
            if (!IsDead) AttackPresented?.Invoke();
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void PresentAlertRpc()
        {
            if (!IsDead) AlertPresented?.Invoke();
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

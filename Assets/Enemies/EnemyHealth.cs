using System;
using ArcadeShooter.Combat;
using UnityEngine;

namespace ArcadeShooter.Enemies
{
    public sealed class EnemyHealth : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1f)] private float maxHealth = 60f;
        public float Health { get; private set; }
        public float MaxHealth => maxHealth;
        public bool IsDead => Health <= 0f;
        public event Action Damaged;
        public event Action Died;

        private void Awake() => Health = maxHealth;

        public bool ApplyDamage(float amount, Vector3 hitPoint)
        {
            if (IsDead || amount <= 0f) return false;
            Health = Mathf.Max(0f, Health - amount);
            Damaged?.Invoke();
            if (IsDead) Died?.Invoke();
            return true;
        }

        private void OnValidate() => maxHealth = Mathf.Max(1f, maxHealth);
    }
}

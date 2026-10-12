using System;
using ArcadeShooter.Combat;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ArcadeShooter.Player
{
    // Local health for this test slice. A future host can own ApplyDamage and replicate Health.
    public sealed class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField] private FirstPersonMovement movement;
        [SerializeField] private AutomaticRifle rifle;

        public float Health { get; private set; }
        public float MaxHealth => maxHealth;
        public bool IsDead => Health <= 0f;
        public event Action<float, float> HealthChanged;
        public event Action Died;

        private void Awake()
        {
            if (movement == null) movement = GetComponent<FirstPersonMovement>();
            if (rifle == null) rifle = GetComponent<AutomaticRifle>();
            Health = maxHealth;
        }

        private void Start() => HealthChanged?.Invoke(Health, maxHealth);

        private void Update()
        {
            if (IsDead && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                RestartTestScene();
        }

        [ContextMenu("Restart Test Scene")]
        public void RestartTestScene()
        {
            if (Application.isPlaying)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public bool ApplyDamage(float amount, Vector3 hitPoint)
        {
            if (IsDead || amount <= 0f) return false;
            Health = Mathf.Max(0f, Health - amount);
            HealthChanged?.Invoke(Health, maxHealth);
            if (IsDead)
            {
                if (rifle != null) rifle.enabled = false;
                if (movement != null) movement.enabled = false;
                Died?.Invoke();
            }
            return true;
        }

        private void OnValidate() => maxHealth = Mathf.Max(1f, maxHealth);
    }
}

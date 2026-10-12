using ArcadeShooter.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcadeShooter.UI
{
    public sealed class PlayerHealthHud : MonoBehaviour
    {
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI deathText;
        [SerializeField] private Image damageOverlay;
        [SerializeField, Min(0f)] private float damageFlashDuration = 0.22f;
        private float flashUntil;

        private void OnEnable()
        {
            if (playerHealth == null) return;
            playerHealth.HealthChanged += Refresh;
            playerHealth.Died += ShowDeath;
        }

        private void Start()
        {
            if (playerHealth == null) return;
            Refresh(playerHealth.Health, playerHealth.MaxHealth);
            if (deathText != null) deathText.enabled = playerHealth.IsDead;
        }

        private void OnDisable()
        {
            if (playerHealth == null) return;
            playerHealth.HealthChanged -= Refresh;
            playerHealth.Died -= ShowDeath;
        }

        private void Update()
        {
            if (damageOverlay != null && damageOverlay.enabled && Time.time >= flashUntil)
                damageOverlay.enabled = false;
        }

        private void Refresh(float health, float maximum)
        {
            if (healthText != null) healthText.text = $"HEALTH  {Mathf.CeilToInt(health)} / {Mathf.CeilToInt(maximum)}";
            if (damageOverlay != null && health < maximum && !playerHealth.IsDead)
            {
                damageOverlay.enabled = true;
                flashUntil = Time.time + damageFlashDuration;
            }
        }

        private void ShowDeath()
        {
            if (deathText != null) deathText.enabled = true;
            if (damageOverlay != null) damageOverlay.enabled = false;
        }
    }
}

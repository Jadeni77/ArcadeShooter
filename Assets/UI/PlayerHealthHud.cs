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
        private float previousHealth = float.NaN;
        private readonly Image[] healthTicks = new Image[12];

        private void Awake()
        {
            if (healthText == null) return;
            Image panel = CombatHudVisuals.Plate("Health Readout", transform, Vector2.zero,
                Vector2.zero, new Vector2(28, 20), new Vector2(485, 150), CombatHudVisuals.Night);
            panel.transform.SetAsFirstSibling();
            CombatHudVisuals.Plate("Health Edge", transform, Vector2.zero, Vector2.zero,
                new Vector2(28, 168), new Vector2(485, 3), CombatHudVisuals.Cyan);
            TextMeshProUGUI title = CombatHudVisuals.Label("Health Label", transform, Vector2.zero,
                Vector2.zero, new Vector2(42, 124), new Vector2(450, 28), 21,
                CombatHudVisuals.Cyan, TextAlignmentOptions.Left);
            title.text = "HEALTH  /  OPERATOR";
            RectTransform healthRect = healthText.rectTransform;
            healthRect.anchorMin = healthRect.anchorMax = Vector2.zero;
            healthRect.pivot = Vector2.zero;
            healthRect.anchoredPosition = new Vector2(42, 47);
            healthRect.sizeDelta = new Vector2(430, 68);
            healthText.fontSize = 57;
            healthText.fontStyle = FontStyles.Bold;
            healthText.alignment = TextAlignmentOptions.Left;
            healthText.color = CombatHudVisuals.White;
            for (int i = 0; i < healthTicks.Length; i++)
                healthTicks[i] = CombatHudVisuals.Plate("Health Tick " + (i + 1), transform,
                    Vector2.zero, Vector2.zero, new Vector2(42 + i * 36, 30), new Vector2(31, 5),
                    CombatHudVisuals.White);
        }

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
            if (healthText != null)
            {
                healthText.text = Mathf.CeilToInt(health).ToString("000");
                healthText.color = health <= maximum * 0.3f ? CombatHudVisuals.Red : CombatHudVisuals.White;
            }
            int lit = Mathf.CeilToInt(Mathf.Clamp01(health / maximum) * healthTicks.Length);
            for (int i = 0; i < healthTicks.Length; i++)
                if (healthTicks[i] != null) healthTicks[i].color = i < lit
                    ? health <= maximum * 0.3f ? CombatHudVisuals.Red : CombatHudVisuals.White
                    : new Color(0.28f, 0.33f, 0.34f, 0.62f);
            if (damageOverlay != null && !float.IsNaN(previousHealth) && health < previousHealth && !playerHealth.IsDead)
            {
                damageOverlay.enabled = true;
                flashUntil = Time.time + damageFlashDuration;
            }
            previousHealth = health;
        }

        private void ShowDeath()
        {
            if (deathText != null) deathText.enabled = true;
            if (damageOverlay != null) damageOverlay.enabled = false;
        }
    }
}

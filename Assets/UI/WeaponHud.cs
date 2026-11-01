using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcadeShooter.UI
{
    public sealed class WeaponHud : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI ammoText;
        [SerializeField] private UnityEngine.UI.Image hitMarker;
        [SerializeField, Min(0f)] private float hitMarkerDuration = 0.12f;
        private float hitMarkerUntil;
        private Color normalHitColor;
        private TextMeshProUGUI reloadText;
        private readonly Image[] ammoTicks = new Image[10];

        private void Awake()
        {
            if (GetComponent<TeammateHud>() == null) gameObject.AddComponent<TeammateHud>();
            if (hitMarker != null) normalHitColor = hitMarker.color;
            if (ammoText == null) return;
            Image panel = CombatHudVisuals.Plate("Ammo Readout", transform, new Vector2(1, 0),
                new Vector2(1, 0), new Vector2(-27, 20), new Vector2(490, 150), CombatHudVisuals.Night);
            panel.transform.SetAsFirstSibling();
            CombatHudVisuals.Plate("Ammo Edge", transform, new Vector2(1, 0),
                new Vector2(1, 0), new Vector2(-28, 168), new Vector2(488, 3), CombatHudVisuals.Amber);
            TextMeshProUGUI weapon = CombatHudVisuals.Label("Weapon Name", transform, new Vector2(1, 0),
                new Vector2(1, 0), new Vector2(-44, 124), new Vector2(420, 28), 21,
                CombatHudVisuals.Cyan, TextAlignmentOptions.Right);
            weapon.text = "ASSAULT RIFLE  /  AUTO";
            reloadText = CombatHudVisuals.Label("Reload Status", transform, new Vector2(1, 0),
                new Vector2(1, 0), new Vector2(-44, 102), new Vector2(420, 24), 19,
                CombatHudVisuals.Amber, TextAlignmentOptions.Right);
            reloadText.text = "RELOADING";
            reloadText.enabled = false;
            RectTransform ammoRect = ammoText.rectTransform;
            ammoRect.anchorMin = ammoRect.anchorMax = new Vector2(1, 0);
            ammoRect.pivot = new Vector2(1, 0);
            ammoRect.anchoredPosition = new Vector2(-40, 42);
            ammoRect.sizeDelta = new Vector2(440, 68);
            ammoText.fontSize = 57;
            ammoText.fontStyle = FontStyles.Bold;
            ammoText.alignment = TextAlignmentOptions.Right;
            ammoText.color = CombatHudVisuals.White;
            for (int i = 0; i < ammoTicks.Length; i++)
                ammoTicks[i] = CombatHudVisuals.Plate("Magazine Tick " + (i + 1), transform,
                    new Vector2(1, 0), new Vector2(1, 0), new Vector2(-45 - (ammoTicks.Length - 1 - i) * 35, 30),
                    new Vector2(29, 5), CombatHudVisuals.Cyan);
            if (hitMarker != null) normalHitColor = hitMarker.color = CombatHudVisuals.Cyan;
        }

        public void SetAmmo(int magazine, int reserve, bool reloading)
        {
            if (ammoText != null) ammoText.text = $"{magazine:00} / {reserve:00}";
            if (reloadText != null) reloadText.enabled = reloading;
            int lit = Mathf.CeilToInt(Mathf.Clamp01(magazine / 30f) * ammoTicks.Length);
            for (int i = 0; i < ammoTicks.Length; i++)
                if (ammoTicks[i] != null) ammoTicks[i].color = i < lit ? CombatHudVisuals.Cyan :
                    new Color(0.28f, 0.33f, 0.34f, 0.62f);
        }

        public void ConfirmHit(bool killed = false)
        {
            if (hitMarker == null)
                return;
            hitMarker.color = killed ? new Color(1f, 0.72f, 0.2f, normalHitColor.a) : normalHitColor;
            hitMarker.enabled = true;
            hitMarkerUntil = Time.time + hitMarkerDuration;
        }

        private void Update()
        {
            if (hitMarker != null && hitMarker.enabled && Time.time >= hitMarkerUntil)
                hitMarker.enabled = false;
        }
    }
}

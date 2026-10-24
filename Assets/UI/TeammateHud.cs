using System.Linq;
using ArcadeShooter.Networking;
using ArcadeShooter.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcadeShooter.UI
{
    // Reads replicated player health and cosmetic names; never owns gameplay state.
    [RequireComponent(typeof(Canvas))]
    public sealed class TeammateHud : MonoBehaviour
    {
        private readonly TextMeshProUGUI[] rows = new TextMeshProUGUI[4];
        private readonly TextMeshProUGUI[] healthValues = new TextMeshProUGUI[4];
        private float nextRefresh;
        private string lastSignature;
        private Canvas ownerCanvas;

        private void Awake()
        {
            ownerCanvas = GetComponent<Canvas>();
            var root = new GameObject("Squad Status", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(transform, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(42, -290);
            rect.sizeDelta = new Vector2(380, 184);
            Image plate = root.GetComponent<Image>();
            plate.color = new Color(0.005f, 0.018f, 0.023f, 0.6f);
            plate.raycastTarget = false;
            for (int i = 0; i < rows.Length; i++)
            {
                var row = new GameObject("Squad Member " + (i + 1), typeof(RectTransform), typeof(TextMeshProUGUI));
                row.transform.SetParent(root.transform, false);
                RectTransform rowRect = row.GetComponent<RectTransform>();
                rowRect.anchorMin = rowRect.anchorMax = new Vector2(0, 1);
                rowRect.pivot = new Vector2(0, 1);
                rowRect.anchoredPosition = new Vector2(16, -12 - i * 40);
                rowRect.sizeDelta = new Vector2(255, 33);
                TextMeshProUGUI label = row.GetComponent<TextMeshProUGUI>();
                label.font = TMP_Settings.defaultFontAsset;
                label.fontSize = 21;
                label.fontStyle = FontStyles.Bold;
                label.color = new Color(0.89f, 0.95f, 0.93f);
                label.alignment = TextAlignmentOptions.Left;
                label.overflowMode = TextOverflowModes.Ellipsis;
                label.raycastTarget = false;
                rows[i] = label;
                var value = new GameObject("Health Value " + (i + 1), typeof(RectTransform), typeof(TextMeshProUGUI));
                value.transform.SetParent(root.transform, false);
                RectTransform valueRect = value.GetComponent<RectTransform>();
                valueRect.anchorMin = valueRect.anchorMax = new Vector2(0, 1);
                valueRect.pivot = new Vector2(0, 1);
                valueRect.anchoredPosition = new Vector2(272, -12 - i * 40);
                valueRect.sizeDelta = new Vector2(88, 33);
                TextMeshProUGUI healthText = value.GetComponent<TextMeshProUGUI>();
                healthText.font = TMP_Settings.defaultFontAsset;
                healthText.fontSize = 21;
                healthText.fontStyle = FontStyles.Bold;
                healthText.alignment = TextAlignmentOptions.Right;
                healthText.raycastTarget = false;
                healthValues[i] = healthText;
            }
        }

        private void Update()
        {
            if (ownerCanvas == null || !ownerCanvas.enabled) return;
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.25f;
            NetworkPlayer[] players = FindObjectsByType<NetworkPlayer>()
                .Where(player => player.IsSpawned && player.InMatch)
                .OrderBy(player => player.OwnerClientId).Take(rows.Length).ToArray();
            string signature = string.Empty;
            for (int i = 0; i < players.Length; i++)
            {
                PlayerHealth health = players[i].GetComponent<PlayerHealth>();
                signature += players[i].OwnerClientId + ":" + players[i].DisplayName + ":" +
                             Mathf.CeilToInt(health.Health) + ":" + players[i].IsOwner + "|";
            }
            if (signature == lastSignature) return;
            lastSignature = signature;
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i].enabled = i < players.Length;
                healthValues[i].enabled = i < players.Length;
                if (i >= players.Length) continue;
                PlayerHealth health = players[i].GetComponent<PlayerHealth>();
                string name = players[i].IsOwner ? players[i].DisplayName + " / YOU" : players[i].DisplayName;
                rows[i].text = name.ToUpperInvariant();
                healthValues[i].text = health.IsDead ? "DOWN" : Mathf.CeilToInt(health.Health).ToString("000");
                rows[i].color = health.IsDead ? new Color(1f, 0.45f, 0.40f) :
                    health.Health <= 30 ? new Color(1f, 0.72f, 0.38f) :
                    new Color(0.89f, 0.95f, 0.93f);
                healthValues[i].color = rows[i].color;
            }
        }
    }
}

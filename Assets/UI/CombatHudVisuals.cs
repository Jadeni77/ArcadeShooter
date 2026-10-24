using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcadeShooter.UI
{
    internal static class CombatHudVisuals
    {
        internal static readonly Color Night = new Color(0.005f, 0.018f, 0.023f, 0.77f);
        internal static readonly Color White = new Color(0.93f, 0.96f, 0.94f);
        internal static readonly Color Cyan = new Color(0.48f, 0.84f, 0.86f);
        internal static readonly Color Amber = new Color(0.89f, 0.68f, 0.33f);
        internal static readonly Color Red = new Color(1f, 0.35f, 0.33f);

        internal static Image Plate(string name, Transform parent, Vector2 anchor, Vector2 pivot,
            Vector2 position, Vector2 size, Color color)
        {
            var item = new GameObject(name, typeof(RectTransform), typeof(Image));
            item.transform.SetParent(parent, false);
            RectTransform rect = item.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = item.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        internal static TextMeshProUGUI Label(string name, Transform parent, Vector2 anchor, Vector2 pivot,
            Vector2 position, Vector2 size, int fontSize, Color color, TextAlignmentOptions alignment)
        {
            var item = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            item.transform.SetParent(parent, false);
            RectTransform rect = item.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            TextMeshProUGUI text = item.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = fontSize;
            text.fontStyle = FontStyles.Bold;
            text.color = color;
            text.alignment = alignment;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false;
            return text;
        }
    }
}

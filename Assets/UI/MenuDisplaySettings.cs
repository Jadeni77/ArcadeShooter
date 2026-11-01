using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcadeShooter.UI
{
    // Adds local input/display controls to the existing menu settings page.
    public sealed class MenuDisplaySettings : MonoBehaviour
    {
        private TMP_Text modeLabel;
        private TMP_Text resolutionLabel;
        private Resolution[] resolutions;
        private int resolutionIndex;

        public void Initialize(GameObject page)
        {
            if (!Application.isEditor) LocalGameSettings.ApplySavedDisplay();
            Transform parent = page.transform;
            Color white = new Color(0.94f, 0.97f, 0.96f);
            Color amber = new Color(0.89f, 0.68f, 0.33f);
            TMP_Text heading = parent.Find("Settings Title")?.GetComponent<TMP_Text>();
            if (heading != null) heading.text = "FIELD SETTINGS";
            TMP_Text eyebrow = parent.Find("Settings Eyebrow")?.GetComponent<TMP_Text>();
            if (eyebrow != null) eyebrow.text = "LOCAL CONTROLS  /  AUDIO + DISPLAY";
            Image veil = Block(parent, "Settings Legibility Veil", 992, 319, 800, 535,
                new Color(0.005f, 0.02f, 0.025f, 0.69f));
            veil.transform.SetAsFirstSibling();
            veil.raycastTarget = false;
            Label(parent, "INPUT / DISPLAY", 1030, 340, 700, 44, 31, white);
            Label(parent, "MOUSE SENSITIVITY", 1030, 412, 700, 34, 21, amber);
            GameObject sliderObject = new GameObject("Sensitivity Slider", typeof(RectTransform), typeof(Slider));
            sliderObject.transform.SetParent(parent, false);
            Place(sliderObject.GetComponent<RectTransform>(), 1030, 459, 700, 37);
            Image track = Block(sliderObject.transform, "Track", 0, 14, 700, 9, new Color(0.13f, 0.23f, 0.25f));
            Image fill = Block(sliderObject.transform, "Fill", 0, 14, 700, 9, new Color(0.48f, 0.84f, 0.86f));
            Image handle = Block(sliderObject.transform, "Handle", 0, 4, 19, 28, white);
            fill.rectTransform.anchorMin = new Vector2(0, 0.5f);
            fill.rectTransform.anchorMax = new Vector2(1, 0.5f);
            fill.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            fill.rectTransform.offsetMin = new Vector2(0, -4.5f);
            fill.rectTransform.offsetMax = new Vector2(0, 4.5f);
            handle.rectTransform.anchorMin = handle.rectTransform.anchorMax = new Vector2(0, 0.5f);
            handle.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            handle.rectTransform.anchoredPosition = Vector2.zero;
            handle.rectTransform.sizeDelta = new Vector2(19, 28);
            Slider slider = sliderObject.GetComponent<Slider>();
            slider.minValue = 0.04f;
            slider.maxValue = 0.4f;
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.SetValueWithoutNotify(LocalGameSettings.MouseSensitivity);
            slider.onValueChanged.AddListener(value => LocalGameSettings.MouseSensitivity = value);
            track.raycastTarget = false;
            modeLabel = Label(parent, "", 1030, 550, 700, 34, 21, amber);
            Button(parent, "TOGGLE FULLSCREEN", 1030, 597, 700, 64, () =>
            { LocalGameSettings.Fullscreen = !LocalGameSettings.Fullscreen; Refresh(); });
            resolutionLabel = Label(parent, "", 1030, 701, 700, 34, 21, amber);
            Button(parent, "NEXT RESOLUTION  ›", 1030, 748, 700, 64, () =>
            {
                if (resolutions.Length == 0) return;
                resolutionIndex = (resolutionIndex + 1) % resolutions.Length;
                LocalGameSettings.SetResolution(resolutions[resolutionIndex]);
                Refresh();
            });
            resolutions = Screen.resolutions.Where(r => r.width >= 1280 && r.height >= 720)
                .GroupBy(r => (r.width, r.height)).Select(g => g.First()).ToArray();
            resolutionIndex = Array.FindIndex(resolutions, r => r.width == Screen.width && r.height == Screen.height);
            Refresh();
        }

        private void Refresh()
        {
            modeLabel.text = "DISPLAY MODE  /  " + (LocalGameSettings.Fullscreen ? "FULLSCREEN" : "WINDOWED");
            resolutionLabel.text = resolutions.Length == 0 || Screen.width < 1280 ? "RESOLUTION  /  SYSTEM DEFAULT"
                : $"RESOLUTION  /  {Screen.width} × {Screen.height}";
        }

        private static Button Button(Transform parent, string text, float x, float y, float width,
            float height, UnityEngine.Events.UnityAction action)
        {
            Image image = Block(parent, text, x, y, width, height, new Color(0.035f, 0.11f, 0.13f));
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.48f, 0.84f, 0.86f);
            button.colors = colors;
            button.onClick.AddListener(action);
            Label(image.transform, text, 22, 12, width - 44, height - 24, 24, Color.white);
            return button;
        }

        private static Image Block(Transform parent, string name, float x, float y, float width, float height, Color color)
        {
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(Image));
            root.transform.SetParent(parent, false);
            Place(root.GetComponent<RectTransform>(), x, y, width, height);
            Image image = root.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static TMP_Text Label(Transform parent, string text, float x, float y, float width,
            float height, int size, Color color)
        {
            GameObject root = new GameObject("Setting Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            root.transform.SetParent(parent, false);
            Place(root.GetComponent<RectTransform>(), x, y, width, height);
            TextMeshProUGUI label = root.GetComponent<TextMeshProUGUI>();
            label.font = TMP_Settings.defaultFontAsset;
            label.fontSize = size;
            label.fontStyle = FontStyles.Bold;
            label.color = color;
            label.text = text;
            label.raycastTarget = false;
            return label;
        }

        private static void Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }
    }
}

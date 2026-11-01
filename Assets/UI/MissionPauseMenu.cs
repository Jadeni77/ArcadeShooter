using System;
using System.Linq;
using ArcadeShooter.Audio;
using ArcadeShooter.Missions;
using ArcadeShooter.Networking;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ArcadeShooter.UI
{
    // The overlay changes only the local owner's input. The online mission keeps running.
    [DisallowMultipleComponent]
    public sealed class MissionPauseMenu : MonoBehaviour
    {
        private static readonly Color White = new Color(0.94f, 0.97f, 0.96f);
        private static readonly Color Cyan = new Color(0.48f, 0.84f, 0.86f);
        private static readonly Color Amber = new Color(0.89f, 0.68f, 0.33f);
        private static readonly Color Soft = new Color(0.73f, 0.8f, 0.8f);
        private GameObject overlay;
        private GameObject mainPage;
        private GameObject settingsPage;
        private GameObject leaveConfirm;
        private TMP_Text fullscreenLabel;
        private TMP_Text resolutionLabel;
        private Resolution[] resolutions;
        private int resolutionIndex;
        private NetworkPlayer owner;

        private void Awake()
        {
            Build();
            overlay.SetActive(false);
        }

        private void Update()
        {
            MissionController mission = MissionController.Instance;
            if (mission == null || !mission.IsSpawned || mission.IsFinished)
            {
                if (overlay.activeSelf) Hide(false);
                return;
            }
            if (owner == null)
                owner = NetworkManager.Singleton?.LocalClient?.PlayerObject?.GetComponent<NetworkPlayer>();
            if (owner == null || !owner.InMatch) return;
            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;
            if (!overlay.activeSelf) Show();
            else if (leaveConfirm.activeSelf) leaveConfirm.SetActive(false);
            else if (settingsPage.activeSelf) { settingsPage.SetActive(false); mainPage.SetActive(true); }
            else Hide(true);
        }

        private void Show()
        {
            mainPage.SetActive(true);
            settingsPage.SetActive(false);
            leaveConfirm.SetActive(false);
            overlay.SetActive(true);
            owner.SetLocalMenuOpen(true);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        private void Hide(bool restoreControl)
        {
            overlay.SetActive(false);
            if (restoreControl && owner != null && owner.InMatch) owner.SetLocalMenuOpen(false);
        }

        private void Build()
        {
            if (EventSystem.current == null)
                new GameObject("Mission UI Event System", typeof(EventSystem), typeof(InputSystemUIInputModule));
            GameObject canvasObject = new GameObject("Pause UI", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 80;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            overlay = Block("Pause Overlay", canvasObject.transform, new Color(0.005f, 0.015f, 0.02f, 0.88f));
            Stretch(overlay.GetComponent<RectTransform>());
            Block("Amber Rule", overlay.transform, Amber, 80, 128, 5, 850);
            Block("Top Rail", overlay.transform, new Color(0, 0, 0, 0.4f), 0, 0, 1920, 108);
            Text("Brand", overlay.transform, "ARCADE / SHOOTER", 78, 27, 590, 65, 43, White);
            Text("Online Notice", overlay.transform, "LIVE SESSION  /  YOUR SQUAD IS STILL IN THE FIELD", 1010, 47, 830, 35, 20, Cyan);
            Text("Footer", overlay.transform, "ESC  RESUME    /    YOUR SESSION CONTINUES WHILE THIS MENU IS OPEN", 82, 1022, 1350, 34, 18, Soft);

            mainPage = new GameObject("Pause Actions", typeof(RectTransform));
            mainPage.transform.SetParent(overlay.transform, false);
            Stretch(mainPage.GetComponent<RectTransform>());
            Text("Eyebrow", mainPage.transform, "OPERATIONS  /  LOCAL MENU", 116, 206, 790, 38, 23, Cyan);
            Text("Heading", mainPage.transform, "MISSION PAUSED", 113, 265, 1160, 120, 91, White);
            Text("Explainer", mainPage.transform, "Your controls are suspended. The online operation continues.", 117, 388, 1120, 45, 26, Soft);
            Button("Resume", mainPage.transform, "RESUME MISSION  ›", 116, 509, 645, 77, () => Hide(true), true);
            Button("Settings", mainPage.transform, "SETTINGS  /  CONTROLS", 116, 596, 645, 77, OpenSettings);
            Button("Leave", mainPage.transform, "LEAVE MATCH", 116, 683, 645, 77, () => leaveConfirm.SetActive(true));
            Text("Mission Note", mainPage.transform, "CO-OP MISSION\n\nYour squad can continue without you while this menu is open.",
                1210, 585, 560, 220, 26, Soft);

            settingsPage = new GameObject("Settings Page", typeof(RectTransform));
            settingsPage.transform.SetParent(overlay.transform, false);
            Stretch(settingsPage.GetComponent<RectTransform>());
            Text("Settings Eyebrow", settingsPage.transform, "LOCAL CONTROLS  /  AUDIO + DISPLAY", 116, 172, 1050, 37, 23, Cyan);
            Text("Settings Heading", settingsPage.transform, "FIELD SETTINGS", 113, 220, 1140, 105, 82, White);
            AudioMixSettings mix = MultiplayerSessionController.Instance?.GetComponent<AudioMixSettings>();
            Slider("MASTER VOLUME", settingsPage.transform, 116, 368, AudioMixSettings.Bus.Master, "ArcadeShooter.Audio.Master", mix);
            Slider("MUSIC VOLUME", settingsPage.transform, 116, 465, AudioMixSettings.Bus.Music, "ArcadeShooter.Audio.Music", mix);
            Slider("SFX VOLUME", settingsPage.transform, 116, 562, AudioMixSettings.Bus.Sfx, "ArcadeShooter.Audio.Sfx", mix);
            Text("Sensitivity Label", settingsPage.transform, "MOUSE SENSITIVITY", 990, 371, 700, 35, 21, Amber);
            UnityEngine.UI.Slider sensitivity = MakeSlider(settingsPage.transform, 990, 410, 700);
            sensitivity.minValue = 0.04f;
            sensitivity.maxValue = 0.4f;
            sensitivity.SetValueWithoutNotify(LocalGameSettings.MouseSensitivity);
            sensitivity.onValueChanged.AddListener(value => LocalGameSettings.MouseSensitivity = value);
            fullscreenLabel = Text("Fullscreen Label", settingsPage.transform, "", 990, 493, 710, 35, 21, Amber);
            Button("Fullscreen", settingsPage.transform, "TOGGLE FULLSCREEN", 990, 535, 700, 65, ToggleFullscreen);
            resolutionLabel = Text("Resolution Label", settingsPage.transform, "", 990, 626, 710, 35, 21, Amber);
            Button("Resolution", settingsPage.transform, "NEXT RESOLUTION  ›", 990, 668, 700, 65, NextResolution);
            Button("Settings Back", settingsPage.transform, "‹    BACK TO MISSION MENU", 116, 827, 645, 70, () =>
            { settingsPage.SetActive(false); mainPage.SetActive(true); });
            resolutions = Screen.resolutions.Where(r => r.width >= 1280 && r.height >= 720)
                .GroupBy(r => (r.width, r.height)).Select(g => g.First()).ToArray();
            resolutionIndex = Array.FindIndex(resolutions, r => r.width == Screen.width && r.height == Screen.height);
            RefreshDisplayLabels();
            settingsPage.SetActive(false);

            leaveConfirm = Block("Leave Confirmation", overlay.transform, new Color(0.005f, 0.012f, 0.017f, 0.98f), 475, 284, 970, 480);
            Block("Confirm Rule", leaveConfirm.transform, Amber, 0, 0, 970, 5);
            Text("Confirm Heading", leaveConfirm.transform, "LEAVE THIS MATCH?", 62, 72, 850, 72, 52, White);
            Text("Confirm Detail", leaveConfirm.transform, "You will return to the main menu. Rejoining requires the squad's join code.",
                64, 170, 830, 90, 25, Soft);
            Button("Cancel", leaveConfirm.transform, "STAY WITH SQUAD", 64, 333, 390, 73, () => leaveConfirm.SetActive(false), true);
            Button("Confirm Leave", leaveConfirm.transform, "LEAVE SESSION", 510, 333, 390, 73,
                () => { Hide(false); MultiplayerSessionController.Instance?.LeaveGame(); });
            leaveConfirm.SetActive(false);
        }

        private void OpenSettings()
        {
            mainPage.SetActive(false);
            settingsPage.SetActive(true);
            RefreshDisplayLabels();
        }

        private void ToggleFullscreen()
        {
            LocalGameSettings.Fullscreen = !LocalGameSettings.Fullscreen;
            RefreshDisplayLabels();
        }

        private void NextResolution()
        {
            if (resolutions.Length == 0) return;
            resolutionIndex = (resolutionIndex + 1) % resolutions.Length;
            LocalGameSettings.SetResolution(resolutions[resolutionIndex]);
            RefreshDisplayLabels();
        }

        private void RefreshDisplayLabels()
        {
            fullscreenLabel.text = "DISPLAY MODE  /  " + (LocalGameSettings.Fullscreen ? "FULLSCREEN" : "WINDOWED");
            resolutionLabel.text = resolutions.Length == 0 || Screen.width < 1280 ? "RESOLUTION  /  SYSTEM DEFAULT"
                : $"RESOLUTION  /  {Screen.width} × {Screen.height}";
        }

        private static void Slider(string title, Transform parent, float x, float y,
            AudioMixSettings.Bus bus, string key, AudioMixSettings mix)
        {
            Text(title + " Label", parent, title, x, y, 700, 30, 21, Amber);
            UnityEngine.UI.Slider slider = MakeSlider(parent, x, y + 37, 700);
            slider.SetValueWithoutNotify(PlayerPrefs.GetFloat(key, 1f));
            slider.onValueChanged.AddListener(value =>
            {
                mix?.SetLevel(bus, value);
                PlayerPrefs.SetFloat(key, value);
                PlayerPrefs.Save();
            });
        }

        private static UnityEngine.UI.Slider MakeSlider(Transform parent, float x, float y, float width)
        {
            GameObject root = new GameObject("Setting Slider", typeof(RectTransform), typeof(Slider));
            root.transform.SetParent(parent, false);
            Place(root.GetComponent<RectTransform>(), x, y, width, 35);
            Block("Track", root.transform, new Color(0.13f, 0.23f, 0.25f), 0, 13, width, 9);
            GameObject fill = Block("Fill", root.transform, Cyan, 0, 13, width, 9);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0.5f);
            fillRect.anchorMax = new Vector2(1, 0.5f);
            fillRect.pivot = new Vector2(0.5f, 0.5f);
            fillRect.offsetMin = new Vector2(0, -4.5f);
            fillRect.offsetMax = new Vector2(0, 4.5f);
            GameObject handle = Block("Handle", root.transform, White, 0, 4, 18, 27);
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.anchorMin = handleRect.anchorMax = new Vector2(0, 0.5f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            handleRect.anchoredPosition = Vector2.zero;
            handleRect.sizeDelta = new Vector2(18, 27);
            Slider slider = root.GetComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
            return slider;
        }

        private static Button Button(string name, Transform parent, string title, float x, float y,
            float width, float height, UnityEngine.Events.UnityAction action, bool primary = false)
        {
            GameObject root = Block(name, parent, primary ? Amber : new Color(0.035f, 0.11f, 0.13f, 0.98f), x, y, width, height);
            Button button = root.AddComponent<Button>();
            button.targetGraphic = root.GetComponent<Image>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = Cyan;
            colors.pressedColor = White;
            button.colors = colors;
            button.onClick.AddListener(action);
            Text("Label", root.transform, title, 20, 10, width - 40, height - 20, 28,
                primary ? new Color(0.025f, 0.05f, 0.06f) : White);
            return button;
        }

        private static GameObject Block(string name, Transform parent, Color color,
            float x = 0, float y = 0, float width = 0, float height = 0)
        {
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(Image));
            root.transform.SetParent(parent, false);
            Image image = root.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = true;
            if (width > 0 && height > 0) Place(root.GetComponent<RectTransform>(), x, y, width, height);
            return root;
        }

        private static TMP_Text Text(string name, Transform parent, string value, float x, float y,
            float width, float height, int size, Color color)
        {
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            root.transform.SetParent(parent, false);
            Place(root.GetComponent<RectTransform>(), x, y, width, height);
            TextMeshProUGUI label = root.GetComponent<TextMeshProUGUI>();
            label.font = TMP_Settings.defaultFontAsset;
            label.fontSize = size;
            label.color = color;
            label.text = value;
            label.fontStyle = FontStyles.Bold;
            label.raycastTarget = false;
            label.overflowMode = TextOverflowModes.Ellipsis;
            return label;
        }

        private static void Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}

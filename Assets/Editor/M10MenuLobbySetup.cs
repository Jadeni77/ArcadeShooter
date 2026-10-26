using System;
using System.IO;
using ArcadeShooter.Audio;
using ArcadeShooter.Networking;
using ArcadeShooter.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace ArcadeShooter.Editor
{
    // One-time scene authoring; edits the current M9 menu rather than regenerating it.
    public static class M10MenuLobbySetup
    {
        private const string ScenePath = "Assets/Scenes/MainMenu.unity";
        private static readonly Color Night = Hex(0x02080C);
        private static readonly Color White = Hex(0xEFF4F2);
        private static readonly Color Soft = Hex(0xBFCBC9);
        private static readonly Color Cyan = Hex(0x7AD7DB);
        private static readonly Color Amber = Hex(0xE3AD55);
        private static readonly Color Green = Hex(0xA8D797);
        private static readonly Color Clear = new Color(0, 0, 0, 0);

        [MenuItem("ArcadeShooter/M10/Build Cinematic Menu and Lobby")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject system = GameObject.Find("Multiplayer System");
            if (system == null) throw new InvalidOperationException("Existing multiplayer system is missing.");
            MultiplayerSessionController controller = system.GetComponent<MultiplayerSessionController>();
            if (controller == null) throw new InvalidOperationException("Existing session controller is missing.");
            Transform canvas = system.transform.Find("Multiplayer UI");
            Transform main = canvas?.Find("Main Menu");
            Transform lobby = canvas?.Find("Lobby");
            if (main == null || lobby == null) throw new InvalidOperationException("Existing menu/lobby panels are missing.");
            if (main.Find("M10 Background") != null) throw new InvalidOperationException("M10 UI has already been authored.");

            Texture2D menuArt = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/UI/Art/MenuFacilityConcept.png");
            Texture2D lobbyArt = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/UI/Art/LobbySquadConcept.png");
            if (menuArt == null || lobbyArt == null) throw new InvalidOperationException("Original concept backdrops are missing.");
            main.GetComponent<UnityEngine.UI.Image>().color = Clear;
            lobby.GetComponent<UnityEngine.UI.Image>().color = Clear;

            // Main backdrop and rails are siblings of the pages so navigation never covers the art.
            Background(main, menuArt);
            Block("Left Legibility Veil", main, 0, 0, 905, 1080, new Color(0.005f, 0.015f, 0.02f, 0.65f)).SetSiblingIndex(1);
            Block("Top Rail", main, 0, 0, 1920, 112, new Color(0.005f, 0.014f, 0.02f, 0.82f)).SetSiblingIndex(2);
            Block("Bottom Rail", main, 0, 1015, 1920, 65, new Color(0.005f, 0.014f, 0.02f, 0.90f)).SetSiblingIndex(3);
            Label("Brand", main, "ARCADE / SHOOTER", 76, 20, 580, 60, 42, White, true);
            Label("Online Status", main, "OPERATOR STATUS  /  ONLINE", 1500, 37, 350, 33, 20, Soft, true);
            Label("Footer Hint", main, "ESC  BACK     /     ENTER  SELECT", 76, 1030, 780, 30, 18, Soft, true);

            Transform title = Require(main, "Title");
            Transform subtitle = Require(main, "Subtitle");
            Transform host = Require(main, "Host Game");
            Transform join = Require(main, "Join Game");
            Transform codeInput = Require(main, "Join Code Input");
            GameObject landing = Page("Landing Page", main);
            GameObject multiplayer = Page("Multiplayer Page", main);
            GameObject profile = Page("Profile Page", main);
            GameObject settings = Page("Settings Page", main);
            title.SetParent(landing.transform, false);
            SetTopLeft(title.GetComponent<RectTransform>(), 113, 196, 760, 205);
            TextMeshProUGUI titleText = title.GetComponent<TextMeshProUGUI>();
            titleText.text = "CONTAINMENT\nBREACH";
            titleText.alignment = TextAlignmentOptions.TopLeft;
            titleText.fontSize = 102;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = White;
            titleText.characterSpacing = 2;
            subtitle.SetParent(landing.transform, false);
            SetTopLeft(subtitle.GetComponent<RectTransform>(), 116, 453, 750, 54);
            TextMeshProUGUI subtitleText = subtitle.GetComponent<TextMeshProUGUI>();
            subtitleText.text = "The facility is lost. The squad is not.";
            subtitleText.alignment = TextAlignmentOptions.Left;
            subtitleText.fontSize = 28;
            subtitleText.color = Soft;
            Label("Incident", landing.transform, "RESEARCH FACILITY  //  INCIDENT 01", 115, 167, 780, 34, 22, Cyan, true);
            Block("Title Accent", landing.transform, 84, 167, 4, 680, Amber);
            Label("Operations Kicker", landing.transform, "OPERATIONS  /  01", 116, 527, 550, 31, 20, Amber, true);
            var play = Action("Play Multiplayer", landing.transform, "01     PLAY / MULTIPLAYER", 115, 575, 570, 74, true);
            var profileAction = Action("Player Profile", landing.transform, "02     PLAYER PROFILE", 115, 652, 570, 74);
            var settingsAction = Action("Settings", landing.transform, "03     SETTINGS", 115, 729, 570, 74);
            var quitAction = Action("Quit Game", landing.transform, "04     QUIT GAME", 115, 806, 570, 74);
            Transform dossier = Block("Operation Dossier", landing.transform, 1293, 790, 550, 191, new Color(0.005f, 0.02f, 0.025f, 0.82f));
            Block("Dossier Edge", dossier, 0, 0, 550, 4, Amber);
            Label("Dossier Label", dossier, "ACTIVE OPERATION  /  01", 28, 17, 490, 27, 19, Amber, true);
            Label("Dossier Title", dossier, "CONTAINMENT BREACH", 28, 46, 500, 55, 39, White, true);
            Label("Dossier Description", dossier, "Clear both danger zones. Recover the research sample.\nExtract as a team.", 28, 105, 500, 58, 20, Soft);
            Label("Dossier Meta", dossier, "CO-OP 2–4    /    SECTOR 03", 28, 163, 490, 26, 18, Cyan, true);

            Label("Multiplayer Eyebrow", multiplayer.transform, "FIRETEAM  /  CREATE OR JOIN", 115, 180, 820, 40, 24, Cyan, true);
            Label("Multiplayer Title", multiplayer.transform, "DEPLOY TOGETHER", 115, 240, 880, 105, 79, White, true);
            Label("Player Name Label", multiplayer.transform, "YOUR OPERATOR NAME", 115, 390, 650, 35, 19, Amber, true);
            TMP_InputField nameInput = Input("Display Name Input", multiplayer.transform, "OPERATOR", 115, 430, 570, 70, 16);
            Label("Host Label", multiplayer.transform, "CREATE A PRIVATE FOUR-PLAYER FIRETEAM", 115, 535, 730, 29, 18, Soft, true);
            host.SetParent(multiplayer.transform, false);
            StyleExistingButton(host, 115, 571, 570, 69, "HOST GAME  ›", true);
            Label("Join Label", multiplayer.transform, "OR ENTER A FRIEND'S JOIN CODE", 115, 677, 730, 29, 18, Soft, true);
            codeInput.SetParent(multiplayer.transform, false);
            SetTopLeft(codeInput.GetComponent<RectTransform>(), 115, 716, 345, 70);
            codeInput.GetComponent<UnityEngine.UI.Image>().color = new Color(0.025f, 0.055f, 0.068f, 0.96f);
            TMP_InputField joinInput = codeInput.GetComponent<TMP_InputField>();
            joinInput.textComponent.fontSize = 28;
            join.SetParent(multiplayer.transform, false);
            StyleExistingButton(join, 479, 716, 207, 70, "JOIN  ›", false);
            UnityEngine.UI.Button multiplayerBack = Action("Back From Multiplayer", multiplayer.transform, "‹    BACK TO OPERATIONS", 115, 849, 570, 65);

            Label("Profile Eyebrow", profile.transform, "LOCAL PROFILE  /  01", 115, 174, 800, 38, 23, Cyan, true);
            Label("Profile Title", profile.transform, "OPERATOR PROFILE", 115, 235, 930, 110, 80, White, true);
            TMP_Text profileDetails = Label("Profile Details", profile.transform, "", 115, 384, 770, 310, 29, White);
            UnityEngine.UI.Button profileBack = Action("Back From Profile", profile.transform, "‹    BACK TO OPERATIONS", 115, 849, 570, 65);

            Label("Settings Eyebrow", settings.transform, "LOCAL CONTROLS  /  AUDIO", 115, 174, 800, 38, 23, Cyan, true);
            Label("Settings Title", settings.transform, "AUDIO SETTINGS", 115, 235, 930, 110, 80, White, true);
            var master = Slider("Master Volume", settings.transform, 115, 405);
            var music = Slider("Music Volume", settings.transform, 115, 530);
            var sfx = Slider("SFX Volume", settings.transform, 115, 655);
            UnityEngine.UI.Button settingsBack = Action("Back From Settings", settings.transform, "‹    BACK TO OPERATIONS", 115, 849, 570, 65);

            // Lobby: keep the art visible, darken open lanes, and put live state on a compact rail.
            Background(lobby, lobbyArt);
            var washes = new UnityEngine.UI.Image[4];
            for (int i = 0; i < 4; i++)
            {
                Transform wash = Block("Open Slot Wash " + (i + 1), lobby, i * 480, 0, 480, 1015, Clear);
                washes[i] = wash.GetComponent<UnityEngine.UI.Image>();
                washes[i].raycastTarget = false;
                if (i > 0) Block("Lane Divider " + i, lobby, i * 480, 115, 2, 900, new Color(1, 1, 1, 0.16f));
            }
            Block("Lobby Top Rail", lobby, 0, 0, 1920, 137, new Color(0.005f, 0.014f, 0.02f, 0.86f));
            Block("Roster Rail", lobby, 0, 798, 1920, 217, new Color(0.005f, 0.014f, 0.02f, 0.90f));
            Block("Roster Edge", lobby, 0, 798, 1920, 3, Amber);
            Block("Lobby Bottom Rail", lobby, 0, 1015, 1920, 65, new Color(0.005f, 0.014f, 0.02f, 0.98f));
            Transform lobbyTitle = Require(lobby, "Lobby Title");
            TMP_Text heading = lobbyTitle.GetComponent<TMP_Text>();
            heading.text = "FIRETEAM / PRIVATE SESSION";
            heading.fontSize = 44; heading.alignment = TextAlignmentOptions.Left; heading.color = White;
            SetTopLeft(heading.rectTransform, 54, 21, 800, 64);
            lobbyTitle.SetAsLastSibling();
            TMP_Text difficulty = Label("Mission and Difficulty", lobby, "OPERATION 01  /  CONTAINMENT BREACH  /  NORMAL", 56, 86, 950, 32, 21, Amber, true);
            TMP_Text count = Label("Operator Count", lobby, "01 / 04 OPERATORS", 850, 42, 410, 40, 28, White, true);
            Transform joinCode = Require(lobby, "Join Code");
            TMP_Text code = joinCode.GetComponent<TMP_Text>();
            code.text = ""; code.fontSize = 32; code.alignment = TextAlignmentOptions.Left; code.color = White;
            SetTopLeft(code.rectTransform, 1510, 62, 220, 49);
            joinCode.SetAsLastSibling();
            Label("Join Code Kicker", lobby, "PRIVATE SESSION  /  JOIN CODE", 1510, 28, 360, 30, 17, Soft, true);
            UnityEngine.UI.Button copy = Action("Copy Join Code", lobby, "COPY  ↗", 1736, 61, 144, 48);
            Transform players = Require(lobby, "Players");
            players.GetComponent<TMP_Text>().enabled = false; // Kept for existing controller data and font reference.
            var names = new TMP_Text[4]; var details = new TMP_Text[4];
            var equipment = new TMP_Text[4]; var readiness = new TMP_Text[4];
            for (int i = 0; i < 4; i++)
            {
                int x = i * 480 + 37;
                Label("Lane Number " + (i + 1), lobby, (i + 1).ToString("00"), x, 738, 155, 47, 38, White, true);
                Block("Lane Registration " + (i + 1), lobby, x, 781, 280, 2, Cyan);
                names[i] = Label("Member Name " + (i + 1), lobby, "AWAITING OPERATOR", x, 816, 415, 58, 42, White, true);
                details[i] = Label("Member Details " + (i + 1), lobby, "SLOT OPEN", x, 872, 417, 28, 18, Soft, true);
                equipment[i] = Label("Member Equipment " + (i + 1), lobby, "NO LOADOUT", x, 910, 420, 27, 17, Cyan, true);
                Block("Ready Rule " + (i + 1), lobby, x, 948, 397, 2, Green);
                readiness[i] = Label("Member Readiness " + (i + 1), lobby, "OPEN", x, 960, 415, 28, 21, Green, true);
            }
            Transform start = Require(lobby, "Start Game");
            StyleExistingButton(start, 1539, 1023, 310, 48, "START MISSION  ›", true);
            start.SetAsLastSibling();
            Transform leave = Require(lobby, "Leave Game");
            StyleExistingButton(leave, 56, 1023, 240, 48, "LEAVE LOBBY", false);
            leave.SetAsLastSibling();
            UnityEngine.UI.Button ready = Action("Toggle Ready", lobby, "READY UP", 1230, 1023, 275, 48);
            TMP_Text readyLabel = ready.GetComponentInChildren<TMP_Text>();
            UnityEngine.UI.Button loadout = Action("Loadout", lobby, "LOADOUT / DIFFICULTY", 327, 1023, 360, 48);
            Transform extras = Block("Loadout Overlay", lobby, 1080, 160, 770, 574, new Color(0.005f, 0.017f, 0.022f, 0.96f));
            Block("Loadout Edge", extras, 0, 0, 770, 4, Cyan);
            Label("Loadout Title", extras, "OPERATOR / MISSION OPTIONS", 42, 35, 690, 65, 42, White, true);
            UnityEngine.UI.Button closeExtras = Action("Close Loadout", extras, "CLOSE  ×", 566, 29, 165, 50);
            Transform progressionRoot = Block("Progression Controls", extras, 75, 110, 620, 420, Clear);
            progressionRoot.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            SerializedObject controllerData = new SerializedObject(controller);
            controllerData.FindProperty("lobbyExtrasRoot").objectReferenceValue = progressionRoot.GetComponent<RectTransform>();
            controllerData.ApplyModifiedPropertiesWithoutUndo();

            Transform status = Require(canvas, "Status");
            TMP_Text statusText = status.GetComponent<TMP_Text>();
            statusText.fontSize = 17; statusText.color = White; statusText.alignment = TextAlignmentOptions.Center;
            RectTransform statusRect = status.GetComponent<RectTransform>();
            statusRect.anchorMin = statusRect.anchorMax = new Vector2(0, 0);
            statusRect.pivot = new Vector2(0, 0);
            statusRect.anchoredPosition = new Vector2(711, 18);
            statusRect.sizeDelta = new Vector2(500, 39);

            AudioMixSettings mix = system.GetComponent<AudioMixSettings>();
            if (mix == null) mix = system.AddComponent<AudioMixSettings>();
            SerializedObject mixData = new SerializedObject(mix);
            mixData.FindProperty("mixer").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/CombatAudio.mixer");
            mixData.ApplyModifiedPropertiesWithoutUndo();

            MenuLobbyView view = system.AddComponent<MenuLobbyView>();
            SerializedObject data = new SerializedObject(view);
            Assign(data, "landingPage", landing); Assign(data, "multiplayerPage", multiplayer);
            Assign(data, "profilePage", profile); Assign(data, "settingsPage", settings);
            Assign(data, "loadoutPage", extras.gameObject);
            Assign(data, "playButton", play); Assign(data, "profileButton", profileAction);
            Assign(data, "settingsButton", settingsAction); Assign(data, "quitButton", quitAction);
            Array(data, "backButtons", multiplayerBack, profileBack, settingsBack);
            Assign(data, "displayNameInput", nameInput); Assign(data, "profileDetails", profileDetails);
            Assign(data, "playerCount", count); Assign(data, "sessionHeading", heading);
            Assign(data, "sessionCode", code); Assign(data, "difficultyText", difficulty);
            Array(data, "memberNames", names); Array(data, "memberDetails", details);
            Array(data, "memberEquipment", equipment); Array(data, "memberReadiness", readiness);
            Array(data, "emptySlotWashes", washes);
            Assign(data, "copyCodeButton", copy); Assign(data, "readyButton", ready);
            Assign(data, "readyButtonLabel", readyLabel); Assign(data, "loadoutButton", loadout);
            Assign(data, "closeLoadoutButton", closeExtras);
            Assign(data, "masterSlider", master); Assign(data, "musicSlider", music);
            Assign(data, "sfxSlider", sfx); Assign(data, "audioSettings", mix);
            data.ApplyModifiedPropertiesWithoutUndo();
            multiplayer.SetActive(false); profile.SetActive(false); settings.SetActive(false);
            extras.gameObject.SetActive(false);
            ApplyVisualCorrections(system.transform);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("M10 cinematic menu and lobby authored in the existing scene.");
        }

        // A second pass and direct camera renders for visual QA in an isolated batch Editor.
        public static void Review()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject system = GameObject.Find("Multiplayer System");
            Transform canvasRoot = system.transform.Find("Multiplayer UI");
            Transform main = canvasRoot.Find("Main Menu");
            Transform lobby = canvasRoot.Find("Lobby");
            ApplyVisualCorrections(system.transform);
            TMP_Text status = canvasRoot.Find("Status").GetComponent<TMP_Text>();
            status.fontSize = 17;
            status.alignment = TextAlignmentOptions.Center;
            RectTransform statusRect = status.rectTransform;
            statusRect.anchorMin = statusRect.anchorMax = Vector2.zero;
            statusRect.pivot = Vector2.zero;
            statusRect.anchoredPosition = new Vector2(711, 18);
            statusRect.sizeDelta = new Vector2(500, 39);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Canvas canvas = canvasRoot.GetComponent<Canvas>();
            Camera camera = system.transform.Find("Menu Camera").GetComponent<Camera>();
            RenderMode originalMode = canvas.renderMode;
            Camera originalWorldCamera = canvas.worldCamera;
            float originalPlane = canvas.planeDistance;
            RenderTexture originalTarget = camera.targetTexture;
            GameObject gameplayControls = canvasRoot.Find("Gameplay Controls").gameObject;
            bool gameplayWasActive = gameplayControls.activeSelf;
            gameplayControls.SetActive(false);
            string originalStatus = status.text;
            status.text = string.Empty;
            try
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 1;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Night;
                string output = Path.Combine(Application.dataPath, "../docs/m10-review");
                Directory.CreateDirectory(output);
                main.gameObject.SetActive(true);
                lobby.gameObject.SetActive(false);
                Capture(canvas, camera, Path.Combine(output, "main-menu-1600x900.png"));
                Capture(canvas, camera, Path.Combine(output, "main-menu-1280x720.png"), 1280, 720);
                Capture(canvas, camera, Path.Combine(output, "main-menu-2560x1600.png"), 2560, 1600);
                main.Find("Landing Page").gameObject.SetActive(false);
                main.Find("Multiplayer Page").gameObject.SetActive(true);
                Capture(canvas, camera, Path.Combine(output, "multiplayer-entry-1600x900.png"));
                main.Find("Multiplayer Page").gameObject.SetActive(false);
                main.Find("Profile Page").gameObject.SetActive(true);
                Capture(canvas, camera, Path.Combine(output, "profile-entry-1600x900.png"));
                main.Find("Profile Page").gameObject.SetActive(false);
                main.Find("Settings Page").gameObject.SetActive(true);
                Capture(canvas, camera, Path.Combine(output, "settings-entry-1600x900.png"));
                main.Find("Settings Page").gameObject.SetActive(false);
                main.Find("Landing Page").gameObject.SetActive(true);
                main.gameObject.SetActive(false);
                lobby.gameObject.SetActive(true);
                Capture(canvas, camera, Path.Combine(output, "lobby-empty-1600x900.png"));
                Capture(canvas, camera, Path.Combine(output, "lobby-empty-1280x720.png"), 1280, 720);
                Capture(canvas, camera, Path.Combine(output, "lobby-empty-2560x1600.png"), 2560, 1600);
            }
            finally
            {
                camera.targetTexture = originalTarget;
                canvas.renderMode = originalMode;
                canvas.worldCamera = originalWorldCamera;
                canvas.planeDistance = originalPlane;
                main.gameObject.SetActive(true);
                lobby.gameObject.SetActive(false);
                gameplayControls.SetActive(gameplayWasActive);
                status.text = originalStatus;
            }
            Debug.Log("M10 scene review captures written to docs/m10-review.");
        }

        private static void ApplyVisualCorrections(Transform system)
        {
            Transform canvas = system.Find("Multiplayer UI");
            Transform main = canvas.Find("Main Menu");
            Transform lobby = canvas.Find("Lobby");
            foreach (Transform panel in new[] { main, lobby })
            {
                Transform background = panel.Find("M10 Background");
                var fitter = background.GetComponent<UnityEngine.UI.AspectRatioFitter>();
                if (fitter == null) fitter = background.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
                fitter.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.EnvelopeParent;
                fitter.aspectRatio = 1672f / 941f;
            }
            StretchTop(main.Find("Top Rail").GetComponent<RectTransform>(), 112);
            StretchBottom(main.Find("Bottom Rail").GetComponent<RectTransform>(), 65);
            StretchTop(lobby.Find("Lobby Top Rail").GetComponent<RectTransform>(), 137);
            StretchBottom(lobby.Find("Lobby Bottom Rail").GetComponent<RectTransform>(), 65);
            StretchBottom(lobby.Find("Roster Rail").GetComponent<RectTransform>(), 282, 65);
            StretchBottom(lobby.Find("Roster Edge").GetComponent<RectTransform>(), 285, 282);
            TopRight(main.Find("Online Status").GetComponent<RectTransform>(), 76, 37, 350, 33);
            main.Find("Online Status").GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Right;
            BottomRight(main.Find("Landing Page/Operation Dossier").GetComponent<RectTransform>(), 77, 99, 550, 191);
            TextMeshProUGUI title = main.Find("Landing Page/Title").GetComponent<TextMeshProUGUI>();
            title.fontSize = 78;
            title.textWrappingMode = TextWrappingModes.NoWrap;
            title.overflowMode = TextOverflowModes.Overflow;
            SetTopLeft(title.rectTransform, 112, 205, 900, 185);
            TMP_Text subtitle = main.Find("Landing Page/Subtitle").GetComponent<TMP_Text>();
            SetTopLeft(subtitle.rectTransform, 116, 432, 790, 55);
            TMP_Text dossierTitle = main.Find("Landing Page/Operation Dossier/Dossier Title").GetComponent<TMP_Text>();
            dossierTitle.fontSize = 30;
            dossierTitle.textWrappingMode = TextWrappingModes.NoWrap;
            TopRight(lobby.Find("Join Code Kicker").GetComponent<RectTransform>(), 50, 28, 360, 30);
            TopRight(lobby.Find("Join Code").GetComponent<RectTransform>(), 190, 62, 220, 49);
            TopRight(lobby.Find("Copy Join Code").GetComponent<RectTransform>(), 40, 61, 144, 48);
            BottomRight(lobby.Find("Start Game").GetComponent<RectTransform>(), 71, 9, 310, 48);
            BottomRight(lobby.Find("Toggle Ready").GetComponent<RectTransform>(), 415, 9, 275, 48);
            RectTransform count = lobby.Find("Operator Count").GetComponent<RectTransform>();
            count.anchorMin = count.anchorMax = new Vector2(0.5f, 1);
            count.pivot = new Vector2(0.5f, 1);
            count.anchoredPosition = new Vector2(95, -42);
            count.sizeDelta = new Vector2(410, 40);
            for (int i = 1; i <= 4; i++)
            {
                int slot = i - 1;
                Transform wash = lobby.Find("Open Slot Wash " + i);
                RectTransform washRect = wash.GetComponent<RectTransform>();
                washRect.anchorMin = new Vector2(slot / 4f, 0);
                washRect.anchorMax = new Vector2(i / 4f, 1);
                washRect.offsetMin = washRect.offsetMax = Vector2.zero;
                if (i > 1)
                {
                    RectTransform divider = lobby.Find("Lane Divider " + slot).GetComponent<RectTransform>();
                    divider.anchorMin = new Vector2(slot / 4f, 0);
                    divider.anchorMax = new Vector2(slot / 4f, 1);
                    divider.offsetMin = new Vector2(0, 65);
                    divider.offsetMax = new Vector2(2, -115);
                }
                SlotRect(lobby.Find("Lane Number " + i).GetComponent<RectTransform>(), slot, 738, 47, 155);
                SlotRect(lobby.Find("Lane Registration " + i).GetComponent<RectTransform>(), slot, 781, 2, 280);
                SlotRect(lobby.Find("Member Name " + i).GetComponent<RectTransform>(), slot, 816, 58);
                SlotRect(lobby.Find("Member Details " + i).GetComponent<RectTransform>(), slot, 872, 28);
                SlotRect(lobby.Find("Member Equipment " + i).GetComponent<RectTransform>(), slot, 910, 27);
                SlotRect(lobby.Find("Ready Rule " + i).GetComponent<RectTransform>(), slot, 928, 2);
                SlotRect(lobby.Find("Member Readiness " + i).GetComponent<RectTransform>(), slot, 940, 28);
                TMP_Text name = lobby.Find("Member Name " + i).GetComponent<TMP_Text>();
                name.text = "AWAITING";
                name.fontSize = 34;
                name.textWrappingMode = TextWrappingModes.NoWrap;
                name.overflowMode = TextOverflowModes.Ellipsis;
            }
            TMP_Text loadout = lobby.Find("Loadout/Label").GetComponent<TMP_Text>();
            loadout.fontSize = 20;
            loadout.textWrappingMode = TextWrappingModes.NoWrap;
            Transform settings = main.Find("Settings Page");
            CorrectSlider(settings.Find("Master Volume"));
            CorrectSlider(settings.Find("Music Volume"));
            CorrectSlider(settings.Find("SFX Volume"));
        }

        private static void TopRight(RectTransform rect, float right, float top, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-right, -top);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void BottomRight(RectTransform rect, float right, float bottom, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(1, 0);
            rect.anchoredPosition = new Vector2(-right, bottom);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void StretchTop(RectTransform rect, float height)
        {
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = new Vector2(0, -height);
            rect.offsetMax = Vector2.zero;
        }

        private static void StretchBottom(RectTransform rect, float top, float bottom = 0)
        {
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.offsetMin = new Vector2(0, bottom);
            rect.offsetMax = new Vector2(0, top);
        }

        private static void SlotRect(RectTransform rect, int slot, float top, float height, float fixedWidth = 0)
        {
            float bottom = 1080 - top - height;
            if (fixedWidth > 0)
            {
                rect.anchorMin = rect.anchorMax = new Vector2(slot / 4f, 0);
                rect.pivot = Vector2.zero;
                rect.anchoredPosition = new Vector2(37, bottom);
                rect.sizeDelta = new Vector2(fixedWidth, height);
            }
            else
            {
                rect.anchorMin = new Vector2(slot / 4f, 0);
                rect.anchorMax = new Vector2((slot + 1) / 4f, 0);
                rect.offsetMin = new Vector2(37, bottom);
                rect.offsetMax = new Vector2(-27, bottom + height);
            }
        }

        private static void CorrectSlider(Transform slider)
        {
            RectTransform track = slider.Find("Track").GetComponent<RectTransform>();
            Transform fillTransform = slider.Find("Fill");
            if (fillTransform == null) fillTransform = slider.Find("Fill Area/Fill");
            RectTransform fill = fillTransform.GetComponent<RectTransform>();
            RectTransform handle = slider.Find("Handle").GetComponent<RectTransform>();
            track.anchorMin = new Vector2(0, 0.5f);
            track.anchorMax = new Vector2(1, 0.5f);
            track.pivot = new Vector2(0.5f, 0.5f);
            track.anchoredPosition = Vector2.zero;
            track.sizeDelta = new Vector2(0, 7);
            Transform fillArea = slider.Find("Fill Area");
            if (fillArea == null)
            {
                var area = new GameObject("Fill Area", typeof(RectTransform));
                area.transform.SetParent(slider, false);
                fillArea = area.transform;
            }
            RectTransform areaRect = fillArea.GetComponent<RectTransform>();
            areaRect.anchorMin = new Vector2(0, 0.5f);
            areaRect.anchorMax = new Vector2(1, 0.5f);
            areaRect.pivot = new Vector2(0.5f, 0.5f);
            areaRect.anchoredPosition = Vector2.zero;
            areaRect.sizeDelta = new Vector2(0, 7);
            fill.SetParent(fillArea, false);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.pivot = new Vector2(0.5f, 0.5f);
            fill.offsetMin = fill.offsetMax = Vector2.zero;
            fillArea.SetAsLastSibling();
            handle.SetAsLastSibling();
            handle.anchorMin = handle.anchorMax = new Vector2(0, 0.5f);
            handle.pivot = new Vector2(0.5f, 0.5f);
            handle.anchoredPosition = Vector2.zero;
            handle.sizeDelta = new Vector2(14, 26);
        }

        private static void Capture(Canvas canvas, Camera camera, string path, int width = 1600, int height = 900)
        {
            RenderTexture target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            target.Create();
            camera.targetTexture = target;
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            Texture2D result = new Texture2D(width, height, TextureFormat.RGB24, false);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();
            File.WriteAllBytes(path, result.EncodeToPNG());
            RenderTexture.active = previous;
            camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(result);
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
        }

        private static Color Hex(uint value) => new Color(((value >> 16) & 255) / 255f, ((value >> 8) & 255) / 255f, (value & 255) / 255f);
        private static Transform Require(Transform parent, string name) =>
            parent.Find(name) ?? throw new InvalidOperationException($"Expected '{name}' under '{parent.name}'.");
        private static void SetTopLeft(RectTransform rect, float x, float y, float w, float h)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(w, h);
        }
        private static GameObject Page(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return go;
        }
        private static void Background(Transform parent, Texture2D texture)
        {
            var go = new GameObject("M10 Background", typeof(RectTransform), typeof(UnityEngine.UI.RawImage));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<UnityEngine.UI.RawImage>();
            image.texture = texture; image.raycastTarget = false;
            go.transform.SetAsFirstSibling();
        }
        private static Transform Block(string name, Transform parent, float x, float y, float w, float h, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            go.transform.SetParent(parent, false);
            SetTopLeft(go.GetComponent<RectTransform>(), x, y, w, h);
            var image = go.GetComponent<UnityEngine.UI.Image>();
            image.color = color; image.raycastTarget = false;
            return go.transform;
        }
        private static TMP_Text Label(string name, Transform parent, string value, float x, float y, float w, float h, float size, Color color, bool bold = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            SetTopLeft(go.GetComponent<RectTransform>(), x, y, w, h);
            var label = go.GetComponent<TextMeshProUGUI>();
            label.text = value; label.font = TMP_Settings.defaultFontAsset; label.fontSize = size;
            label.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            label.color = color; label.alignment = TextAlignmentOptions.Left;
            label.characterSpacing = bold ? 1.7f : 0;
            label.enableAutoSizing = false; label.raycastTarget = false;
            return label;
        }
        private static UnityEngine.UI.Button Action(string name, Transform parent, string value, float x, float y, float w, float h, bool primary = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
            go.transform.SetParent(parent, false);
            SetTopLeft(go.GetComponent<RectTransform>(), x, y, w, h);
            var image = go.GetComponent<UnityEngine.UI.Image>();
            image.color = primary ? Amber : new Color(0.005f, 0.018f, 0.023f, 0.76f);
            var button = go.GetComponent<UnityEngine.UI.Button>();
            button.targetGraphic = image;
            button.colors = new UnityEngine.UI.ColorBlock {
                normalColor = Color.white, highlightedColor = primary ? new Color(1, 0.93f, 0.78f) : new Color(0.19f, 0.38f, 0.41f),
                pressedColor = primary ? new Color(0.85f, 0.64f, 0.32f) : new Color(0.08f, 0.24f, 0.27f),
                selectedColor = primary ? new Color(1, 0.93f, 0.78f) : new Color(0.19f, 0.38f, 0.41f),
                disabledColor = new Color(0.36f, 0.40f, 0.41f, 0.45f), colorMultiplier = 1, fadeDuration = 0.16f
            };
            Label("Label", go.transform, value, 18, 4, w - 36, h - 8, primary ? 31 : 27, primary ? Night : White, true);
            return button;
        }
        private static void StyleExistingButton(Transform t, float x, float y, float w, float h, string value, bool primary)
        {
            SetTopLeft(t.GetComponent<RectTransform>(), x, y, w, h);
            t.GetComponent<UnityEngine.UI.Image>().color = primary ? Amber : new Color(0.005f, 0.018f, 0.023f, 0.82f);
            var button = t.GetComponent<UnityEngine.UI.Button>();
            button.targetGraphic = t.GetComponent<UnityEngine.UI.Image>();
            button.colors = new UnityEngine.UI.ColorBlock {
                normalColor = Color.white, highlightedColor = primary ? new Color(1, 0.93f, 0.78f) : new Color(0.19f, 0.38f, 0.41f),
                pressedColor = primary ? new Color(0.85f, 0.64f, 0.32f) : new Color(0.08f, 0.24f, 0.27f),
                selectedColor = Color.white, disabledColor = new Color(0.36f, 0.40f, 0.41f, 0.5f),
                colorMultiplier = 1, fadeDuration = 0.16f
            };
            TMP_Text label = t.GetComponentInChildren<TMP_Text>();
            label.text = value; label.fontSize = h < 60 ? 25 : 30;
            label.fontStyle = FontStyles.Bold; label.color = primary ? Night : White;
            label.characterSpacing = 1.5f;
            RectTransform rect = label.rectTransform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        private static TMP_InputField Input(string name, Transform parent, string placeholder, float x, float y, float w, float h, int limit)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(TMP_InputField));
            go.transform.SetParent(parent, false);
            SetTopLeft(go.GetComponent<RectTransform>(), x, y, w, h);
            go.GetComponent<UnityEngine.UI.Image>().color = new Color(0.025f, 0.055f, 0.068f, 0.97f);
            TMP_Text value = Label("Value", go.transform, "", 20, 12, w - 38, h - 17, 28, White);
            TMP_Text hint = Label("Placeholder", go.transform, placeholder, 20, 12, w - 38, h - 17, 28, Soft);
            TMP_InputField input = go.GetComponent<TMP_InputField>();
            input.textViewport = go.GetComponent<RectTransform>();
            input.textComponent = (TextMeshProUGUI)value;
            input.placeholder = (TextMeshProUGUI)hint;
            input.characterLimit = limit;
            return input;
        }
        private static UnityEngine.UI.Slider Slider(string name, Transform parent, float x, float y)
        {
            Label(name + " Label", parent, name.ToUpperInvariant(), x, y, 500, 36, 25, White, true);
            var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Slider));
            go.transform.SetParent(parent, false);
            SetTopLeft(go.GetComponent<RectTransform>(), x, y + 49, 620, 35);
            Transform track = Block("Track", go.transform, 0, 14, 620, 7, new Color(1, 1, 1, 0.25f));
            Transform fill = Block("Fill", go.transform, 0, 14, 620, 7, Cyan);
            RectTransform trackRect = track.GetComponent<RectTransform>();
            trackRect.anchorMin = new Vector2(0, 0.5f); trackRect.anchorMax = new Vector2(1, 0.5f);
            trackRect.pivot = new Vector2(0.5f, 0.5f); trackRect.anchoredPosition = Vector2.zero;
            trackRect.sizeDelta = new Vector2(0, 7);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0.5f); fillRect.anchorMax = new Vector2(1, 0.5f);
            fillRect.pivot = new Vector2(0.5f, 0.5f); fillRect.anchoredPosition = Vector2.zero;
            fillRect.sizeDelta = new Vector2(0, 7);
            var handle = Block("Handle", go.transform, 0, 4, 14, 26, White).GetComponent<RectTransform>();
            handle.anchorMin = handle.anchorMax = new Vector2(0, 0.5f);
            handle.pivot = new Vector2(0.5f, 0.5f); handle.anchoredPosition = Vector2.zero;
            handle.sizeDelta = new Vector2(14, 26);
            var slider = go.GetComponent<UnityEngine.UI.Slider>();
            slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
            slider.minValue = 0; slider.maxValue = 1; slider.value = 1;
            slider.fillRect = fillRect; slider.handleRect = handle;
            slider.targetGraphic = handle.GetComponent<UnityEngine.UI.Image>();
            return slider;
        }
        private static void Assign(SerializedObject data, string field, UnityEngine.Object value) =>
            data.FindProperty(field).objectReferenceValue = value;
        private static void Array<T>(SerializedObject data, string field, params T[] values) where T : UnityEngine.Object
        {
            SerializedProperty property = data.FindProperty(field);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}

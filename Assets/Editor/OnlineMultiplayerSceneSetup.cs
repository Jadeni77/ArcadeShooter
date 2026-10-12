using ArcadeShooter.Networking;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ArcadeShooter.Editor
{
    public static class OnlineMultiplayerSceneSetup
    {
        private const string PlayerPath = "Assets/Prefabs/FirstPersonPlayer.prefab";
        private const string EnemyPath = "Assets/Prefabs/MeleeEnemy.prefab";
        private const string GameplayPath = "Assets/Scenes/FirstPersonTest.unity";
        private const string MenuPath = "Assets/Scenes/MainMenu.unity";
        private const string BodyMaterialPath = "Assets/Materials/Remote Player Placeholder.mat";

        [MenuItem("ArcadeShooter/Configure Online Multiplayer Scenes")]
        public static void Build()
        {
            if (EditorApplication.isPlaying)
                throw new System.InvalidOperationException("Exit Play mode before configuring multiplayer.");
            PreparePrefabs();
            ConfigureGameplay();
            ConfigureMenu();
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MenuPath, true),
                new EditorBuildSettingsScene(GameplayPath, true)
            };
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(MenuPath, OpenSceneMode.Single);
            Debug.Log("Online multiplayer scene setup complete. Open MainMenu to test.");
        }

        private static void PreparePrefabs()
        {
            Material bodyMaterial = AssetDatabase.LoadAssetAtPath<Material>(BodyMaterialPath);
            if (bodyMaterial == null)
            {
                bodyMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                bodyMaterial.SetColor("_BaseColor", new Color(0.1f, 0.8f, 0.9f));
                AssetDatabase.CreateAsset(bodyMaterial, BodyMaterialPath);
            }
            GameObject player = PrefabUtility.LoadPrefabContents(PlayerPath);
            try
            {
                if (player.GetComponent<NetworkObject>() == null)
                    player.AddComponent<NetworkObject>();
                if (player.GetComponent<OwnerNetworkTransform>() == null)
                    player.AddComponent<OwnerNetworkTransform>();
                if (player.GetComponent<NetworkRifle>() == null)
                    player.AddComponent<NetworkRifle>();
                NetworkPlayer avatar = player.GetComponent<NetworkPlayer>();
                if (avatar == null) avatar = player.AddComponent<NetworkPlayer>();
                Transform body = player.transform.Find("Remote Player Body");
                if (body == null)
                {
                    GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    capsule.name = "Remote Player Body";
                    capsule.transform.SetParent(player.transform, false);
                    capsule.transform.localPosition = new Vector3(0f, 0.9f, 0f);
                    capsule.transform.localScale = new Vector3(0.65f, 0.9f, 0.45f);
                    Object.DestroyImmediate(capsule.GetComponent<Collider>());
                    capsule.GetComponent<Renderer>().sharedMaterial = bodyMaterial;
                    body = capsule.transform;
                }
                SerializedObject avatarData = new SerializedObject(avatar);
                avatarData.FindProperty("remoteBody").objectReferenceValue = body.gameObject;
                avatarData.ApplyModifiedPropertiesWithoutUndo();
                Transform deathLabel = FindChild(player.transform, "Death and Restart");
                if (deathLabel != null)
                    deathLabel.GetComponent<TextMeshProUGUI>().text = "YOU DIED\nLeave to start a new match";
                PrefabUtility.SaveAsPrefabAsset(player, PlayerPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(player); }

            GameObject enemy = PrefabUtility.LoadPrefabContents(EnemyPath);
            try
            {
                if (enemy.GetComponent<NetworkObject>() == null)
                    enemy.AddComponent<NetworkObject>();
                if (enemy.GetComponent<NetworkTransform>() == null)
                    enemy.AddComponent<NetworkTransform>();
                PrefabUtility.SaveAsPrefabAsset(enemy, EnemyPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(enemy); }
        }

        private static void ConfigureGameplay()
        {
            Scene scene = EditorSceneManager.OpenScene(GameplayPath, OpenSceneMode.Single);
            GameObject oldPlayer = GameObject.Find("FirstPersonPlayer");
            if (oldPlayer != null) Object.DestroyImmediate(oldPlayer);
            GameObject zones = GameObject.Find("Danger Zones");
            if (zones == null) throw new System.InvalidOperationException("M4 danger zones missing.");
            for (int i = 0; i < zones.transform.childCount; i++)
            {
                GameObject zone = zones.transform.GetChild(i).gameObject;
                if (zone.GetComponent<NetworkObject>() == null) zone.AddComponent<NetworkObject>();
            }
            foreach (ArcadeShooter.Combat.ShootingTarget target in
                     Object.FindObjectsByType<ArcadeShooter.Combat.ShootingTarget>())
                if (target.GetComponent<NetworkObject>() == null)
                    target.gameObject.AddComponent<NetworkObject>();
            if (GameObject.Find("Multiplayer Spawn Points") == null)
            {
                GameObject root = new GameObject("Multiplayer Spawn Points");
                Vector3[] positions =
                {
                    new Vector3(-13f, 1.05f, -2f), new Vector3(-11f, 1.05f, -2f),
                    new Vector3(-13f, 1.05f, 1f), new Vector3(-11f, 1.05f, 1f)
                };
                for (int i = 0; i < positions.Length; i++)
                {
                    GameObject marker = new GameObject("Player " + (i + 1));
                    marker.transform.SetParent(root.transform);
                    marker.transform.position = positions[i];
                }
            }
            else
            {
                Transform points = GameObject.Find("Multiplayer Spawn Points").transform;
                for (int i = 0; i < points.childCount; i++)
                {
                    Vector3 position = points.GetChild(i).position;
                    position.y = 1.05f;
                    points.GetChild(i).position = position;
                }
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void ConfigureMenu()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject system = new GameObject("Multiplayer System");
            UnityTransport transport = system.AddComponent<UnityTransport>();
            NetworkManager manager = system.AddComponent<NetworkManager>();
            manager.NetworkConfig.NetworkTransport = transport;
            manager.NetworkConfig.PlayerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPath);
            manager.NetworkConfig.EnableSceneManagement = true;
            manager.NetworkConfig.ConnectionApproval = true;

            GameObject cameraObject = new GameObject("Menu Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.transform.SetParent(system.transform);
            Camera menuCamera = cameraObject.GetComponent<Camera>();
            menuCamera.clearFlags = CameraClearFlags.SolidColor;
            menuCamera.backgroundColor = new Color(0.035f, 0.065f, 0.095f);

            GameObject canvasObject = new GameObject("Multiplayer UI", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(system.transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject main = Panel("Main Menu", canvasObject.transform);
            Label("Title", main.transform, "ARCADE SHOOTER", 64, new Vector2(0, 235), new Vector2(1100, 100));
            Label("Subtitle", main.transform, "Online cooperative survival", 28, new Vector2(0, 155), new Vector2(900, 50));
            Button host = MakeButton("Host Game", main.transform, new Vector2(0, 45), new Vector2(340, 70));
            TMP_InputField codeInput = MakeInput(main.transform, new Vector2(0, -55));
            Button join = MakeButton("Join Game", main.transform, new Vector2(0, -145), new Vector2(340, 70));

            GameObject lobby = Panel("Lobby", canvasObject.transform);
            Label("Lobby Title", lobby.transform, "GAME LOBBY", 58, new Vector2(0, 260), new Vector2(900, 90));
            TMP_Text joinCode = Label("Join Code", lobby.transform, "JOIN CODE", 38,
                new Vector2(0, 155), new Vector2(900, 70));
            TMP_Text players = Label("Players", lobby.transform, "Players 1 / 4", 30,
                new Vector2(0, 5), new Vector2(900, 220));
            Button start = MakeButton("Start Game", lobby.transform, new Vector2(0, -180), new Vector2(340, 70));
            Button lobbyLeave = MakeButton("Leave Game", lobby.transform, new Vector2(0, -275), new Vector2(340, 70));

            GameObject gameplay = new GameObject("Gameplay Controls", typeof(RectTransform));
            gameplay.transform.SetParent(canvasObject.transform, false);
            Stretch(gameplay.GetComponent<RectTransform>());
            Button gameLeave = MakeButton("Leave Game", gameplay.transform,
                new Vector2(-155, -55), new Vector2(250, 65));
            RectTransform leaveRect = gameLeave.GetComponent<RectTransform>();
            leaveRect.anchorMin = leaveRect.anchorMax = new Vector2(1, 1);
            leaveRect.pivot = new Vector2(0.5f, 0.5f);

            TMP_Text status = Label("Status", canvasObject.transform, "Host a game or enter a join code.",
                25, new Vector2(0, 42), new Vector2(1600, 60));
            RectTransform statusRect = status.GetComponent<RectTransform>();
            statusRect.anchorMin = statusRect.anchorMax = new Vector2(0.5f, 0f);

            GameObject events = new GameObject("Event System", typeof(EventSystem),
                typeof(InputSystemUIInputModule));
            events.transform.SetParent(system.transform);

            MultiplayerSessionController controller = system.AddComponent<MultiplayerSessionController>();
            SerializedObject data = new SerializedObject(controller);
            data.FindProperty("mainPanel").objectReferenceValue = main;
            data.FindProperty("lobbyPanel").objectReferenceValue = lobby;
            data.FindProperty("gameplayPanel").objectReferenceValue = gameplay;
            data.FindProperty("joinCodeInput").objectReferenceValue = codeInput;
            data.FindProperty("joinCodeText").objectReferenceValue = joinCode;
            data.FindProperty("playersText").objectReferenceValue = players;
            data.FindProperty("statusText").objectReferenceValue = status;
            data.FindProperty("hostButton").objectReferenceValue = host;
            data.FindProperty("joinButton").objectReferenceValue = join;
            data.FindProperty("startButton").objectReferenceValue = start;
            data.FindProperty("lobbyLeaveButton").objectReferenceValue = lobbyLeave;
            data.FindProperty("gameplayLeaveButton").objectReferenceValue = gameLeave;
            data.FindProperty("menuCamera").objectReferenceValue = menuCamera;
            data.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene, MenuPath);
        }

        private static Transform FindChild(Transform parent, string childName)
        {
            if (parent.name == childName) return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform found = FindChild(parent.GetChild(i), childName);
                if (found != null) return found;
            }
            return null;
        }

        private static GameObject Panel(string name, Transform parent)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            panel.transform.SetParent(parent, false);
            Stretch(panel.GetComponent<RectTransform>());
            panel.GetComponent<UnityEngine.UI.Image>().color = new Color(0.035f, 0.065f, 0.095f, 0.96f);
            return panel;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static TMP_Text Label(string name, Transform parent, string content, float size,
            Vector2 position, Vector2 dimensions)
        {
            GameObject text = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            text.transform.SetParent(parent, false);
            RectTransform rect = text.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = dimensions;
            rect.anchoredPosition = position;
            TextMeshProUGUI label = text.GetComponent<TextMeshProUGUI>();
            label.font = TMP_Settings.defaultFontAsset;
            label.fontSize = size;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;
            label.text = content;
            return label;
        }

        private static Button MakeButton(string text, Transform parent, Vector2 position, Vector2 size)
        {
            GameObject root = new GameObject(text, typeof(RectTransform), typeof(UnityEngine.UI.Image),
                typeof(Button));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            root.GetComponent<UnityEngine.UI.Image>().color = new Color(0.08f, 0.45f, 0.56f);
            Label("Label", root.transform, text, 30, Vector2.zero, size);
            return root.GetComponent<Button>();
        }

        private static TMP_InputField MakeInput(Transform parent, Vector2 position)
        {
            GameObject root = new GameObject("Join Code Input", typeof(RectTransform),
                typeof(UnityEngine.UI.Image), typeof(TMP_InputField));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(420, 70);
            rect.anchoredPosition = position;
            root.GetComponent<UnityEngine.UI.Image>().color = new Color(0.14f, 0.2f, 0.25f);
            TMP_Text value = Label("Text", root.transform, "", 30, Vector2.zero, new Vector2(390, 65));
            TMP_Text hint = Label("Placeholder", root.transform, "Enter join code", 28,
                Vector2.zero, new Vector2(390, 65));
            hint.color = new Color(0.65f, 0.7f, 0.73f);
            TMP_InputField input = root.GetComponent<TMP_InputField>();
            input.textViewport = rect;
            input.textComponent = (TextMeshProUGUI)value;
            input.placeholder = (TextMeshProUGUI)hint;
            input.characterLimit = 12;
            return input;
        }
    }
}

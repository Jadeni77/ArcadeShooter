using System;
using System.IO;
using ArcadeShooter.Combat;
using ArcadeShooter.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ArcadeShooter.Editor
{
    // Stage D authoring pass over the existing cinematic menu. The runtime still uses the
    // M9 profile, rifle, and session controller; this script only builds their presentation.
    public static class M10LoadoutProfileSetup
    {
        private const string ScenePath = "Assets/Scenes/MainMenu.unity";
        private const string PreviewPath = "Assets/UI/Art/AssaultRiflePreview.png";
        private const string TealPreviewPath = "Assets/UI/Art/AssaultRiflePreviewTeal.png";
        private const string RifleArtRoot = "Assets/Weapons/Art/QuaterniusSciFiEssentials/";
        private const string RifleModelPath = RifleArtRoot + "Gun_Rifle.fbx";
        private const string RifleMaterialPath = RifleArtRoot + "Gun_Rifle_URP.mat";
        private static readonly Color Night = Hex(0x02080C);
        private static readonly Color White = Hex(0xEFF4F2);
        private static readonly Color Soft = Hex(0xBFCBC9);
        private static readonly Color Cyan = Hex(0x7AD7DB);
        private static readonly Color Amber = Hex(0xE3AD55);

        [MenuItem("ArcadeShooter/M10/Build Loadout and Profile")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject system = GameObject.Find("Multiplayer System");
            Transform canvas = system?.transform.Find("Multiplayer UI");
            Transform main = canvas?.Find("Main Menu");
            Transform profile = main?.Find("Profile Page");
            Transform lobby = canvas?.Find("Lobby");
            Transform overlay = lobby?.Find("Loadout Overlay");
            Transform progression = overlay?.Find("Progression Controls");
            if (profile == null || progression == null)
                throw new InvalidOperationException("Build the M10 menu/lobby before Stage D.");
            if (profile.Find("M10D Level") != null)
                throw new InvalidOperationException("Stage D has already been authored.");

            Texture2D rifleImage = AssetDatabase.LoadAssetAtPath<Texture2D>(PreviewPath);
            if (rifleImage == null) rifleImage = RenderRiflePreview();
            Texture2D tealRifleImage = AssetDatabase.LoadAssetAtPath<Texture2D>(TealPreviewPath);
            if (tealRifleImage == null) tealRifleImage = RenderRiflePreview(true);
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FirstPersonPlayer.prefab");
            AutomaticRifle rifle = playerPrefab?.GetComponent<AutomaticRifle>();
            if (rifle == null) throw new InvalidOperationException("The existing rifle prefab is missing.");

            // The menu backdrop stays visible; large type and restrained rails carry the profile.
            profile.Find("Profile Details").gameObject.SetActive(false);
            Block("M10D Header Rule", profile, 115, 355, 1680, 3, Amber);
            TMP_Text level = Label("M10D Level", profile, "01", 105, 376, 395, 260, 215, White, true);
            Label("M10D Rank Kicker", profile, "SERVICE RANK", 119, 635, 500, 35, 22, Amber, true);
            Label("M10D Callsign Kicker", profile, "CALLSIGN  /  LOCAL OPERATOR", 530, 402, 680, 34, 21, Cyan, true);
            TMP_Text callsign = Label("M10D Callsign", profile, "OPERATOR", 525, 448, 760, 90, 60, White, true);
            Label("M10D Progress Kicker", profile, "PROGRESSION TO NEXT RANK", 530, 548, 670, 32, 21, Soft, true);
            Block("M10D XP Track", profile, 530, 596, 720, 14, new Color(1, 1, 1, 0.22f));
            UnityEngine.UI.Image fill = Block("M10D XP Fill", profile, 530, 596, 1, 14, Cyan).GetComponent<UnityEngine.UI.Image>();
            TMP_Text xp = Label("M10D XP", profile, "", 530, 625, 720, 42, 23, White);
            Block("M10D Record Veil", profile, 1300, 391, 500, 364, new Color(0.005f, 0.018f, 0.024f, 0.84f));
            Block("M10D Record Rule", profile, 1300, 391, 500, 4, Cyan);
            Label("M10D Record Title", profile, "SERVICE RECORD", 1333, 419, 450, 45, 32, White, true);
            TMP_Text record = Label("M10D Record", profile, "", 1333, 480, 453, 92, 21, White);
            Label("M10D Unlock Kicker", profile, "AVAILABLE REWARDS", 1333, 588, 430, 33, 19, Amber, true);
            TMP_Text unlocks = Label("M10D Unlocks", profile, "", 1333, 630, 453, 110, 17, Soft);
            TMP_Text notice = Label("M10D Notice", profile, "PROGRESSION SAVED LOCALLY", 530, 706, 740, 40, 18, Cyan, true);

            // The existing lobby overlay becomes a dedicated armory, preserving its live buttons.
            RectTransform overlayRect = overlay.GetComponent<RectTransform>();
            Place(overlayRect, 0, 0, 1920, 1080);
            overlay.GetComponent<UnityEngine.UI.Image>().color = new Color(0.006f, 0.017f, 0.023f, 0.98f);
            Transform edge = overlay.Find("Loadout Edge");
            Place(edge.GetComponent<RectTransform>(), 0, 0, 1920, 4);
            TMP_Text title = overlay.Find("Loadout Title").GetComponent<TMP_Text>();
            title.text = "OPERATOR ARMORY";
            title.fontSize = 76;
            Place(title.rectTransform, 74, 68, 1150, 106);
            Label("M10D Armory Kicker", overlay, "FIRETEAM  /  EQUIPMENT AND IDENTITY", 76, 30, 1050, 33, 22, Cyan, true);
            Transform close = overlay.Find("Close Loadout");
            Place(close.GetComponent<RectTransform>(), 1605, 58, 240, 61);
            close.GetComponentInChildren<TMP_Text>().text = "CLOSE ARMORY  ×";
            Block("M10D Divider", overlay, 73, 195, 1772, 3, Amber);
            Block("M10D Preview Back", overlay, 73, 233, 1000, 684, new Color(0.015f, 0.045f, 0.055f, 1));
            Raw("M10D Rifle Preview", overlay, rifleImage, 80, 240, 986, 670);
            Label("M10D Preview Caption", overlay, "AR-01  /  PROJECT ISSUE ASSAULT RIFLE", 92, 857, 900, 42, 22, White, true);
            Label("M10D Preview Foot", overlay, "ONE WEAPON PLATFORM  •  COSMETIC FINISHES UNLOCK THROUGH PLAY", 92, 927, 980, 33, 18, Soft, true);
            Block("M10D Spec Back", overlay, 1130, 233, 715, 684, new Color(0.010f, 0.027f, 0.035f, 0.95f));
            Block("M10D Spec Edge", overlay, 1130, 233, 715, 4, Cyan);
            Label("M10D Spec Kicker", overlay, "PRIMARY WEAPON  /  EQUIPPED", 1170, 262, 620, 33, 20, Amber, true);
            Label("M10D Rifle Name", overlay, "ASSAULT RIFLE", 1165, 306, 650, 73, 54, White, true);
            Label("M10D Role", overlay, "AUTOMATIC  •  MID-RANGE  •  TEAM ISSUE", 1170, 388, 650, 34, 19, Cyan, true);
            Block("M10D Spec Rule", overlay, 1170, 440, 625, 2, new Color(1, 1, 1, 0.22f));
            TMP_Text stats = Label("M10D Rifle Stats", overlay, "", 1170, 458, 625, 148, 25, White);
            Label("M10D Configuration", overlay, "CONFIGURATION", 1170, 624, 570, 38, 22, Amber, true);
            TMP_Text finish = Label("M10D Finish State", overlay, "", 1170, 667, 620, 31, 18, Cyan, true);
            TMP_Text playerTitle = Label("M10D Title State", overlay, "", 1170, 709, 620, 31, 18, Cyan, true);
            Place(progression.GetComponent<RectTransform>(), 1170, 756, 620, 248);
            progression.SetAsLastSibling();
            close.SetAsLastSibling();

            M10LoadoutProfileView view = system.AddComponent<M10LoadoutProfileView>();
            SerializedObject data = new SerializedObject(view);
            Assign(data, "progressionRoot", progression.GetComponent<RectTransform>());
            Assign(data, "profileCallsign", callsign); Assign(data, "profileLevel", level);
            Assign(data, "profileXp", xp); Assign(data, "profileXpFill", fill);
            Assign(data, "profileRecord", record); Assign(data, "profileUnlocks", unlocks);
            Assign(data, "profileNotice", notice); Assign(data, "rifleStats", stats);
            Assign(data, "finishState", finish); Assign(data, "titleState", playerTitle);
            Assign(data, "riflePreview", overlay.Find("M10D Rifle Preview").GetComponent<UnityEngine.UI.RawImage>());
            Assign(data, "standardRiflePreview", rifleImage);
            Assign(data, "tealRiflePreview", tealRifleImage);
            Assign(data, "riflePrefab", rifle);
            data.ApplyModifiedPropertiesWithoutUndo();
            ApplyArmoryPolish(overlay);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("M10 Stage D armory and profile authored in MainMenu.");
        }

        [MenuItem("ArcadeShooter/M10/Refresh Rifle Preview")]
        public static void RefreshPreview()
        {
            RenderRiflePreview();
            RenderRiflePreview(true);
        }

        [MenuItem("ArcadeShooter/M10/Prepare Quaternius Rifle Art")]
        public static void PrepareQuaterniusRifleArt()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(RifleModelPath) == null)
                throw new InvalidOperationException("The approved Quaternius rifle FBX has not imported.");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(RifleMaterialPath);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) throw new InvalidOperationException("URP Lit shader is missing.");
                material = new Material(shader) { name = "Gun Rifle URP" };
                AssetDatabase.CreateAsset(material, RifleMaterialPath);
            }
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(RifleArtRoot + "T_Guns_Batch1_BaseColor.png"));
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Metallic", 0.34f);
            material.SetFloat("_Smoothness", 0.42f);
            string normalPath = RifleArtRoot + "T_Guns_Batch1_Normal.png";
            TextureImporter importer = AssetImporter.GetAtPath(normalPath) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.NormalMap)
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.SaveAndReimport();
            }
            material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath));
            material.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            RefreshPreview();
        }

        [MenuItem("ArcadeShooter/M10/Polish Armory")]
        public static void PolishArmory()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform overlay = GameObject.Find("Multiplayer System").transform
                .Find("Multiplayer UI/Lobby/Loadout Overlay");
            if (overlay?.Find("M10D Rifle Preview") == null)
                throw new InvalidOperationException("Build Stage D before polishing its armory.");
            ApplyArmoryPolish(overlay);
            M10LoadoutProfileView view = GameObject.Find("Multiplayer System").GetComponent<M10LoadoutProfileView>();
            SerializedObject data = new SerializedObject(view);
            Assign(data, "riflePreview", overlay.Find("M10D Rifle Preview").GetComponent<UnityEngine.UI.RawImage>());
            Assign(data, "standardRiflePreview", AssetDatabase.LoadAssetAtPath<Texture2D>(PreviewPath));
            Assign(data, "tealRiflePreview", AssetDatabase.LoadAssetAtPath<Texture2D>(TealPreviewPath));
            data.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void ApplyArmoryPolish(Transform overlay)
        {
            Texture2D squadArt = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/UI/Art/LobbySquadConcept.png");
            if (squadArt == null) throw new InvalidOperationException("The original lobby artwork is missing.");
            RectTransform overlayRect = overlay.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = overlayRect.offsetMax = Vector2.zero;
            overlay.GetComponent<UnityEngine.UI.Image>().color = Night;
            Transform art = overlay.Find("M10D Operator Art");
            if (art == null)
            {
                Raw("M10D Operator Art", overlay, squadArt, 0, 0, 1110, 1080);
                art = overlay.Find("M10D Operator Art");
            }
            art.SetSiblingIndex(0);
            var artImage = art.GetComponent<UnityEngine.UI.RawImage>();
            artImage.texture = squadArt;
            artImage.uvRect = new Rect(0.14f, 0, 0.58f, 1);
            Transform rail = overlay.Find("M10D Header Veil");
            if (rail == null) rail = Block("M10D Header Veil", overlay, 0, 0, 1920, 198, new Color(0.005f, 0.014f, 0.020f, 0.88f));
            rail.SetSiblingIndex(1);
            Transform lower = overlay.Find("M10D Lower Veil");
            if (lower == null) lower = Block("M10D Lower Veil", overlay, 0, 914, 1130, 166, new Color(0.005f, 0.014f, 0.020f, 0.84f));
            lower.SetSiblingIndex(2);
            Place(overlay.Find("M10D Preview Back").GetComponent<RectTransform>(), 490, 632, 420, 262);
            overlay.Find("M10D Preview Back").GetComponent<UnityEngine.UI.Image>().color = new Color(0.003f, 0.018f, 0.024f, 0.96f);
            Place(overlay.Find("M10D Rifle Preview").GetComponent<RectTransform>(), 498, 640, 404, 240);
            TMP_Text caption = overlay.Find("M10D Preview Caption").GetComponent<TMP_Text>();
            caption.text = "AR-01  /  RIFLE VISUAL";
            caption.fontSize = 20;
            Place(caption.rectTransform, 506, 850, 380, 35);
            TMP_Text foot = overlay.Find("M10D Preview Foot").GetComponent<TMP_Text>();
            foot.text = "ONE PLATFORM  /  FINISHES EARNED IN PLAY";
            foot.fontSize = 19;
            Place(foot.rectTransform, 80, 955, 850, 40);
            Right(overlay.Find("M10D Spec Back"), 75, 233, 715, 777);
            Right(overlay.Find("M10D Spec Edge"), 75, 233, 715, 4);
            Right(overlay.Find("M10D Spec Kicker"), 130, 262, 620, 33);
            Right(overlay.Find("M10D Rifle Name"), 105, 306, 650, 73);
            Right(overlay.Find("M10D Role"), 100, 388, 650, 34);
            Right(overlay.Find("M10D Spec Rule"), 125, 440, 625, 2);
            Right(overlay.Find("M10D Rifle Stats"), 125, 458, 625, 148);
            Right(overlay.Find("M10D Configuration"), 180, 624, 570, 38);
            Right(overlay.Find("M10D Finish State"), 130, 667, 620, 31);
            Right(overlay.Find("M10D Title State"), 130, 709, 620, 31);
            Right(overlay.Find("Progression Controls"), 130, 756, 620, 248);
            var close = overlay.Find("Close Loadout");
            Right(close, 75, 58, 240, 61);
            var closeLabel = close.GetComponentInChildren<TMP_Text>();
            closeLabel.text = "CLOSE  ×";
            closeLabel.fontSize = 22;
            RectTransform closeRect = closeLabel.rectTransform;
            closeRect.anchorMin = Vector2.zero;
            closeRect.anchorMax = Vector2.one;
            closeRect.offsetMin = closeRect.offsetMax = Vector2.zero;
            close.SetAsLastSibling();
        }

        // Representative text for an Edit-mode visual check; no profile or session is modified.
        [MenuItem("ArcadeShooter/M10/Review Loadout and Profile")]
        public static void Review()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject system = GameObject.Find("Multiplayer System");
            Transform canvasRoot = system.transform.Find("Multiplayer UI");
            Transform main = canvasRoot.Find("Main Menu");
            Transform lobby = canvasRoot.Find("Lobby");
            Transform profile = main.Find("Profile Page");
            Transform overlay = lobby.Find("Loadout Overlay");
            Canvas canvas = canvasRoot.GetComponent<Canvas>();
            Camera camera = system.transform.Find("Menu Camera").GetComponent<Camera>();
            RenderMode mode = canvas.renderMode;
            Camera worldCamera = canvas.worldCamera;
            float plane = canvas.planeDistance;
            RenderTexture target = camera.targetTexture;
            Transform controls = canvasRoot.Find("Gameplay Controls");
            bool controlsActive = controls.gameObject.activeSelf;
            Transform status = canvasRoot.Find("Status");
            bool statusActive = status.gameObject.activeSelf;
            try
            {
                controls.gameObject.SetActive(false);
                status.gameObject.SetActive(false);
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 1;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Night;
                string output = Path.Combine(Application.dataPath, "../docs/m10-stage-d-review");
                Directory.CreateDirectory(output);
                main.gameObject.SetActive(true);
                lobby.gameObject.SetActive(false);
                main.Find("Landing Page").gameObject.SetActive(false);
                profile.gameObject.SetActive(true);
                profile.Find("M10D Level").GetComponent<TMP_Text>().text = "07";
                profile.Find("M10D Callsign").GetComponent<TMP_Text>().text = "VANGUARD";
                profile.Find("M10D XP").GetComponent<TMP_Text>().text = "280 / 1,200 XP  /  NEXT RANK";
                Place(profile.Find("M10D XP Fill").GetComponent<RectTransform>(), 530, 596, 168, 14);
                profile.Find("M10D Record").GetComponent<TMP_Text>().text = "MISSIONS COMPLETED     12\nMISSIONS FAILED           03";
                profile.Find("M10D Unlocks").GetComponent<TMP_Text>().text =
                    "RIFLE FINISH  /  TEAL      UNLOCKED\nCALLSIGN  /  PATHFINDER   UNLOCKED\nEQUIPPED  /  TEAL FINISH  +  PATHFINDER TITLE";
                Capture(canvas, camera, Path.Combine(output, "profile-1600x900.png"), 1600, 900);
                Capture(canvas, camera, Path.Combine(output, "profile-2560x1600.png"), 2560, 1600);

                profile.gameObject.SetActive(false);
                main.gameObject.SetActive(false);
                lobby.gameObject.SetActive(true);
                overlay.gameObject.SetActive(true);
                overlay.Find("M10D Rifle Stats").GetComponent<TMP_Text>().text =
                    "DAMAGE     20\nFIRE RATE    8 ROUNDS / SEC\nMAGAZINE     30 ROUNDS\nRELOAD       1.6 SEC";
                overlay.Find("M10D Finish State").GetComponent<TMP_Text>().text = "TEAL FINISH  /  EQUIPPED";
                overlay.Find("M10D Title State").GetComponent<TMP_Text>().text = "PATHFINDER TITLE  /  EQUIPPED";
                overlay.Find("M10D Rifle Preview").GetComponent<UnityEngine.UI.RawImage>().texture =
                    AssetDatabase.LoadAssetAtPath<Texture2D>(TealPreviewPath);
                Capture(canvas, camera, Path.Combine(output, "armory-1600x900.png"), 1600, 900);
                Capture(canvas, camera, Path.Combine(output, "armory-2560x1600.png"), 2560, 1600);
            }
            finally
            {
                camera.targetTexture = target;
                canvas.renderMode = mode;
                canvas.worldCamera = worldCamera;
                canvas.planeDistance = plane;
                controls.gameObject.SetActive(controlsActive);
                status.gameObject.SetActive(statusActive);
            }
            Debug.Log("M10 Stage D representative captures written to docs/m10-stage-d-review.");
        }

        private static Texture2D RenderRiflePreview(bool teal = false)
        {
            GameObject approvedRifle = AssetDatabase.LoadAssetAtPath<GameObject>(RifleModelPath);
            GameObject prefab = approvedRifle == null ? AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FirstPersonPlayer.prefab") : null;
            Transform source = approvedRifle == null ? FindDescendant(prefab?.transform, "Assault Rifle View") : null;
            if (approvedRifle == null && source == null) throw new InvalidOperationException("The rifle visual is missing.");
            GameObject model = UnityEngine.Object.Instantiate(approvedRifle ?? source.gameObject);
            model.name = "Temporary Rifle Preview";
            model.transform.SetPositionAndRotation(new Vector3(0, 1000, 0), Quaternion.identity);
            if (approvedRifle != null)
            {
                Material material = AssetDatabase.LoadAssetAtPath<Material>(RifleMaterialPath);
                if (material == null) throw new InvalidOperationException("Prepare the approved rifle material first.");
                foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
                {
                    renderer.sharedMaterial = material;
                    if (teal)
                    {
                        var properties = new MaterialPropertyBlock();
                        properties.SetColor("_BaseColor", new Color(0.1f, 0.8f, 0.9f));
                        renderer.SetPropertyBlock(properties);
                    }
                }
            }
            foreach (MonoBehaviour component in model.GetComponentsInChildren<MonoBehaviour>(true))
                UnityEngine.Object.DestroyImmediate(component);
            foreach (Collider collider in model.GetComponentsInChildren<Collider>(true))
                UnityEngine.Object.DestroyImmediate(collider);
            SetLayer(model.transform, 30);
            var renderers = model.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) throw new InvalidOperationException("The rifle has no renderable geometry.");
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            GameObject cameraObject = new GameObject("Temporary Rifle Camera", typeof(Camera));
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.cullingMask = 1 << 30;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Hex(0x0B1B22);
            camera.orthographic = true;
            camera.orthographicSize = bounds.extents.magnitude * 0.72f;
            camera.transform.position = bounds.center + new Vector3(0.35f, 0.42f, 2f).normalized * bounds.extents.magnitude * 4f;
            camera.transform.LookAt(bounds.center);
            GameObject lightObject = new GameObject("Temporary Rifle Key", typeof(Light));
            Light light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.68f, 0.9f, 1f);
            light.intensity = 4f;
            light.cullingMask = 1 << 30;
            lightObject.transform.rotation = Quaternion.Euler(20, -65, 0);
            RenderTexture target = new RenderTexture(1200, 800, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            Texture2D image = new Texture2D(1200, 800, TextureFormat.RGBA32, false);
            RenderTexture previous = RenderTexture.active;
            AmbientMode previousAmbientMode = RenderSettings.ambientMode;
            Color previousAmbient = RenderSettings.ambientLight;
            try
            {
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.55f, 0.69f, 0.76f);
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1200, 800), 0, 0);
                image.Apply();
                File.WriteAllBytes(teal ? TealPreviewPath : PreviewPath, image.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                RenderSettings.ambientMode = previousAmbientMode;
                RenderSettings.ambientLight = previousAmbient;
                camera.targetTexture = null;
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(image);
                UnityEngine.Object.DestroyImmediate(lightObject);
                UnityEngine.Object.DestroyImmediate(cameraObject);
                UnityEngine.Object.DestroyImmediate(model);
            }
            string path = teal ? TealPreviewPath : PreviewPath;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                Transform found = FindDescendant(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private static void SetLayer(Transform root, int layer)
        {
            root.gameObject.layer = layer;
            foreach (Transform child in root) SetLayer(child, layer);
        }

        private static Color Hex(uint rgb) => new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f);
        private static void Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void Right(Transform transform, float right, float top, float width, float height)
        {
            RectTransform rect = transform.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = Vector2.one;
            rect.pivot = Vector2.one;
            rect.anchoredPosition = new Vector2(-right, -top);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static Transform Block(string name, Transform parent, float x, float y, float width, float height, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            go.transform.SetParent(parent, false);
            Place(go.GetComponent<RectTransform>(), x, y, width, height);
            var image = go.GetComponent<UnityEngine.UI.Image>();
            image.color = color;
            image.raycastTarget = false;
            return go.transform;
        }

        private static TMP_Text Label(string name, Transform parent, string value, float x, float y, float width, float height, float size, Color color, bool bold = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            Place(go.GetComponent<RectTransform>(), x, y, width, height);
            var label = go.GetComponent<TextMeshProUGUI>();
            label.text = value;
            label.font = TMP_Settings.defaultFontAsset;
            label.fontSize = size;
            label.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            label.color = color;
            label.alignment = TextAlignmentOptions.Left;
            label.characterSpacing = bold ? 1.5f : 0;
            label.enableAutoSizing = false;
            label.raycastTarget = false;
            return label;
        }

        private static void Raw(string name, Transform parent, Texture texture, float x, float y, float width, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.RawImage));
            go.transform.SetParent(parent, false);
            Place(go.GetComponent<RectTransform>(), x, y, width, height);
            var image = go.GetComponent<UnityEngine.UI.RawImage>();
            image.texture = texture;
            image.raycastTarget = false;
        }

        private static void Assign(SerializedObject data, string name, UnityEngine.Object value) =>
            data.FindProperty(name).objectReferenceValue = value;

        private static void Capture(Canvas canvas, Camera camera, string path, int width, int height)
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
    }
}

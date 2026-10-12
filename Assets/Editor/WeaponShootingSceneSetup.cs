using ArcadeShooter.Combat;
using ArcadeShooter.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcadeShooter.Editor
{
    // Adds M2 content to the existing M1 assets. Never rebuilds the graybox scene.
    public static class WeaponShootingSceneSetup
    {
        private const string ScenePath = "Assets/Scenes/FirstPersonTest.unity";
        private const string PlayerPath = "Assets/Prefabs/FirstPersonPlayer.prefab";
        private const string ImpactPath = "Assets/Prefabs/BulletImpact.prefab";

        [MenuItem("ArcadeShooter/Add Weapon Shooting Test Content")]
        public static void Build()
        {
            Material rifleMaterial = Material("Rifle Placeholder", new Color(0.12f, 0.17f, 0.19f));
            Material metalMaterial = Material("Rifle Metal", new Color(0.34f, 0.39f, 0.4f));
            Material flashMaterial = Material("Muzzle Flash", new Color(1f, 0.8f, 0.12f));
            Material targetMaterial = Material("Target Placeholder", new Color(0.12f, 0.73f, 0.73f));
            Material standMaterial = Material("Target Stand", new Color(0.26f, 0.28f, 0.3f));
            GameObject impact = BuildImpactPrefab(flashMaterial);
            BuildPlayerPrefab(rifleMaterial, metalMaterial, flashMaterial, impact);

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (GameObject.Find("Shooting Targets") == null)
            {
                GameObject group = new GameObject("Shooting Targets");
                MakeTarget(group.transform, "Room 1 Target", new Vector3(-7f, 1.3f, 0f), targetMaterial, standMaterial);
                MakeTarget(group.transform, "Room 2 Target A", new Vector3(-1f, 1.3f, 1.8f), targetMaterial, standMaterial);
                MakeTarget(group.transform, "Room 2 Target B", new Vector3(2f, 1.3f, -1.5f), targetMaterial, standMaterial);
                MakeTarget(group.transform, "Room 3 Target A", new Vector3(8f, 1.3f, -1.8f), targetMaterial, standMaterial);
                MakeTarget(group.transform, "Room 3 Target B", new Vector3(12f, 1.3f, 0.8f), targetMaterial, standMaterial);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Weapon shooting content added to the existing test scene and player prefab.");
        }

        private static void BuildPlayerPrefab(Material rifle, Material metal, Material flash, GameObject impact)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PlayerPath);
            try
            {
                Camera camera = root.GetComponentInChildren<Camera>();
                if (camera == null)
                    throw new System.InvalidOperationException("Player prefab has no camera.");
                if (root.GetComponent<AutomaticRifle>() != null)
                {
                    TextMeshProUGUI existingAmmo = root.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (existingAmmo != null && existingAmmo.font == null)
                    {
                        existingAmmo.font = TMP_Settings.defaultFontAsset;
                        PrefabUtility.SaveAsPrefabAsset(root, PlayerPath);
                    }
                    return;
                }

                GameObject model = new GameObject("Assault Rifle Placeholder");
                model.transform.SetParent(camera.transform, false);
                model.transform.localPosition = new Vector3(0.34f, -0.31f, 0.67f);
                Box("Receiver", model.transform, new Vector3(0f, 0f, 0f), new Vector3(0.22f, 0.16f, 0.43f), rifle);
                Box("Barrel", model.transform, new Vector3(0f, 0.025f, 0.35f), new Vector3(0.075f, 0.075f, 0.4f), metal);
                Box("Stock", model.transform, new Vector3(0f, -0.015f, -0.31f), new Vector3(0.16f, 0.11f, 0.28f), rifle);
                Box("Grip", model.transform, new Vector3(0f, -0.19f, -0.08f), new Vector3(0.09f, 0.27f, 0.12f), rifle);
                Box("Magazine", model.transform, new Vector3(0f, -0.18f, 0.13f), new Vector3(0.11f, 0.24f, 0.15f), metal);
                Box("Sight", model.transform, new Vector3(0f, 0.12f, 0.08f), new Vector3(0.06f, 0.09f, 0.13f), metal);
                GameObject muzzle = Box("Muzzle Flash", model.transform, new Vector3(0f, 0.025f, 0.57f),
                    new Vector3(0.12f, 0.12f, 0.16f), flash);
                muzzle.SetActive(false);

                WeaponHud hud = BuildHud(camera.transform);
                AudioSource source = root.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                AutomaticRifle weapon = root.AddComponent<AutomaticRifle>();
                SerializedObject serialized = new SerializedObject(weapon);
                serialized.FindProperty("aimCamera").objectReferenceValue = camera;
                serialized.FindProperty("weaponModel").objectReferenceValue = model.transform;
                serialized.FindProperty("muzzleFlash").objectReferenceValue = muzzle;
                serialized.FindProperty("impactPrefab").objectReferenceValue = impact;
                serialized.FindProperty("hud").objectReferenceValue = hud;
                serialized.FindProperty("gunfireSound").objectReferenceValue = Audio("Assets/Audio/Gunfire.wav");
                serialized.FindProperty("impactSound").objectReferenceValue = Audio("Assets/Audio/Impact.wav");
                serialized.FindProperty("reloadSound").objectReferenceValue = Audio("Assets/Audio/Reload.wav");
                serialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, PlayerPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static WeaponHud BuildHud(Transform camera)
        {
            GameObject canvasObject = new GameObject("Weapon HUD", typeof(RectTransform), typeof(Canvas),
                typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            canvasObject.transform.SetParent(camera, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            UnityEngine.UI.CanvasScaler scaler = canvasObject.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.GetComponent<UnityEngine.UI.GraphicRaycaster>().enabled = false;

            MakeImage("Crosshair Horizontal", canvasObject.transform, Vector2.zero, new Vector2(22f, 2f), Color.white);
            MakeImage("Crosshair Vertical", canvasObject.transform, Vector2.zero, new Vector2(2f, 22f), Color.white);
            UnityEngine.UI.Image hit = MakeImage("Hit Confirmation", canvasObject.transform, Vector2.zero,
                new Vector2(9f, 9f), new Color(1f, 0.85f, 0.15f));
            hit.enabled = false;

            GameObject ammoObject = new GameObject("Ammunition", typeof(RectTransform), typeof(TextMeshProUGUI));
            ammoObject.transform.SetParent(canvasObject.transform, false);
            RectTransform ammoRect = ammoObject.GetComponent<RectTransform>();
            ammoRect.anchorMin = ammoRect.anchorMax = new Vector2(1f, 0f);
            ammoRect.pivot = new Vector2(1f, 0f);
            ammoRect.anchoredPosition = new Vector2(-42f, 36f);
            ammoRect.sizeDelta = new Vector2(420f, 70f);
            TextMeshProUGUI ammo = ammoObject.GetComponent<TextMeshProUGUI>();
            ammo.fontSize = 32f;
            ammo.font = TMP_Settings.defaultFontAsset;
            ammo.alignment = TextAlignmentOptions.Right;
            ammo.color = Color.white;
            ammo.raycastTarget = false;
            ammo.text = "30 / 90";

            WeaponHud hud = canvasObject.AddComponent<WeaponHud>();
            SerializedObject serialized = new SerializedObject(hud);
            serialized.FindProperty("ammoText").objectReferenceValue = ammo;
            serialized.FindProperty("hitMarker").objectReferenceValue = hit;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return hud;
        }

        private static UnityEngine.UI.Image MakeImage(string name, Transform parent, Vector2 position,
            Vector2 size, Color color)
        {
            GameObject child = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            child.transform.SetParent(parent, false);
            RectTransform rect = child.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            UnityEngine.UI.Image image = child.GetComponent<UnityEngine.UI.Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static GameObject BuildImpactPrefab(Material material)
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(ImpactPath);
            if (existing != null)
                return existing;
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Bullet Impact";
            sphere.transform.localScale = Vector3.one * 0.09f;
            Object.DestroyImmediate(sphere.GetComponent<Collider>());
            sphere.GetComponent<Renderer>().sharedMaterial = material;
            sphere.AddComponent<ImpactEffect>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(sphere, ImpactPath);
            Object.DestroyImmediate(sphere);
            return prefab;
        }

        private static void MakeTarget(Transform parent, string name, Vector3 position, Material target, Material stand)
        {
            GameObject assembly = new GameObject(name);
            assembly.transform.SetParent(parent, false);
            Box("Stand", assembly.transform, new Vector3(position.x, 0.55f, position.z),
                new Vector3(0.12f, 1.1f, 0.12f), stand);
            GameObject plate = Box("Target Plate", assembly.transform, position,
                new Vector3(0.85f, 1.05f, 0.18f), target);
            plate.AddComponent<ShootingTarget>();
        }

        private static GameObject Box(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = position;
            box.transform.localScale = scale;
            box.GetComponent<Renderer>().sharedMaterial = material;
            if (parent.GetComponentInParent<Camera>() != null)
                Object.DestroyImmediate(box.GetComponent<Collider>());
            return box;
        }

        private static Material Material(string name, Color color)
        {
            string path = $"Assets/Materials/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static AudioClip Audio(string path)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null)
                Debug.LogWarning($"Missing generated placeholder audio: {path}");
            return clip;
        }
    }
}

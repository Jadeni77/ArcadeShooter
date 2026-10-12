using ArcadeShooter.Encounters;
using ArcadeShooter.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ArcadeShooter.Editor
{
    // Additive test-scene setup. Existing enemies in encounter rooms are retained but disabled.
    public static class DangerZoneSceneSetup
    {
        private const string ScenePath = "Assets/Scenes/FirstPersonTest.unity";
        private const string EnemyPath = "Assets/Prefabs/MeleeEnemy.prefab";

        [MenuItem("ArcadeShooter/Add Danger Zone Test Content")]
        public static void Build()
        {
            if (EditorApplication.isPlaying)
                throw new System.InvalidOperationException("Exit Play mode before adding danger zones.");
            GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPath);
            if (enemyPrefab == null)
                throw new System.InvalidOperationException("MeleeEnemy prefab is missing; build M3 content first.");

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (GameObject.Find("Three Room Graybox") == null)
                throw new System.InvalidOperationException("The three-room graybox is missing.");
            if (GameObject.Find("Danger Zones") != null)
                throw new System.InvalidOperationException("Danger zones already exist in this scene.");

            GameObject root = new GameObject("Danger Zones");
            DangerZone zoneA = CreateZone(root.transform, "Danger Zone A", "HOSTILES INCOMING!",
                2f, 0f, new[] { 3 }, new[] { 0f }, new[]
                {
                    new Vector3(-2.5f, 0f, -3.3f),
                    new Vector3(-2.5f, 0f, 3.3f),
                    new Vector3(2.7f, 0f, 3.2f)
                }, enemyPrefab);
            DangerZone zoneB = CreateZone(root.transform, "Danger Zone B", "HOSTILES INCOMING!",
                2f, 10f, new[] { 2, 4 }, new[] { 0f, 3f }, new[]
                {
                    new Vector3(7.5f, 0f, -3.4f),
                    new Vector3(9.8f, 0f, -3.4f),
                    new Vector3(12.4f, 0f, -3.4f),
                    new Vector3(13f, 0f, 3.2f),
                    new Vector3(7.5f, 0f, 3.2f),
                    new Vector3(13f, 0f, 1.7f)
                }, enemyPrefab);

            GameObject oldEnemies = GameObject.Find("Melee Enemies");
            if (oldEnemies != null)
            {
                DisableOldEnemy(oldEnemies.transform, "Room 2 Melee Enemy");
                DisableOldEnemy(oldEnemies.transform, "Room 3 Melee Enemy");
            }
            CreateHud(zoneA, zoneB);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("M4 danger zones added to rooms 2 and 3. Existing room 2/3 test enemies were disabled, not removed.");
        }

        private static DangerZone CreateZone(Transform parent, string zoneName, string warning,
            float warningDuration, float centerX, int[] counts, float[] delays,
            Vector3[] spawnPositions, GameObject prefab)
        {
            GameObject objectRoot = new GameObject(zoneName);
            objectRoot.transform.SetParent(parent);
            objectRoot.transform.position = new Vector3(centerX, 0f, 0f);
            BoxCollider trigger = objectRoot.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 1.1f, 0f);
            trigger.size = new Vector3(7.2f, 2.2f, 8f);
            EnemyWaveSpawner spawner = objectRoot.AddComponent<EnemyWaveSpawner>();
            DangerZone zone = objectRoot.AddComponent<DangerZone>();
            AudioSource audio = objectRoot.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.spatialBlend = 0f;

            Transform[] points = new Transform[spawnPositions.Length];
            for (int i = 0; i < spawnPositions.Length; i++)
            {
                GameObject marker = new GameObject($"Spawn Point {i + 1}");
                marker.transform.SetParent(objectRoot.transform);
                marker.transform.position = spawnPositions[i];
                points[i] = marker.transform;
            }

            SerializedObject spawnData = new SerializedObject(spawner);
            spawnData.FindProperty("enemyPrefab").objectReferenceValue = prefab;
            SerializedProperty spawnPoints = spawnData.FindProperty("spawnPoints");
            spawnPoints.arraySize = points.Length;
            for (int i = 0; i < points.Length; i++)
                spawnPoints.GetArrayElementAtIndex(i).objectReferenceValue = points[i];
            SerializedProperty waves = spawnData.FindProperty("waves");
            waves.arraySize = counts.Length;
            for (int i = 0; i < counts.Length; i++)
            {
                waves.GetArrayElementAtIndex(i).FindPropertyRelative("enemyCount").intValue = counts[i];
                waves.GetArrayElementAtIndex(i).FindPropertyRelative("delayBeforeWave").floatValue = delays[i];
            }
            spawnData.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject zoneData = new SerializedObject(zone);
            zoneData.FindProperty("zoneName").stringValue = zoneName;
            zoneData.FindProperty("warningMessage").stringValue = warning;
            zoneData.FindProperty("warningDuration").floatValue = warningDuration;
            zoneData.FindProperty("audioSource").objectReferenceValue = audio;
            zoneData.ApplyModifiedPropertiesWithoutUndo();
            return zone;
        }

        private static void DisableOldEnemy(Transform parent, string name)
        {
            Transform oldEnemy = parent.Find(name);
            if (oldEnemy != null) oldEnemy.gameObject.SetActive(false);
        }

        private static void CreateHud(DangerZone zoneA, DangerZone zoneB)
        {
            GameObject canvasRoot = new GameObject("Encounter HUD", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5;
            CanvasScaler scaler = canvasRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            TextMeshProUGUI notice = MakeText("Encounter Notice", canvasRoot.transform, 34f,
                new Vector2(0f, -95f), new Vector2(1250f, 70f));
            TextMeshProUGUI progress = MakeText("Wave Progress", canvasRoot.transform, 28f,
                new Vector2(0f, -88f), new Vector2(600f, 100f));
            EncounterHud hud = canvasRoot.AddComponent<EncounterHud>();
            SerializedObject data = new SerializedObject(hud);
            SerializedProperty zones = data.FindProperty("zones");
            zones.arraySize = 2;
            zones.GetArrayElementAtIndex(0).objectReferenceValue = zoneA;
            zones.GetArrayElementAtIndex(1).objectReferenceValue = zoneB;
            data.FindProperty("noticeText").objectReferenceValue = notice;
            data.FindProperty("progressText").objectReferenceValue = progress;
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static TextMeshProUGUI MakeText(string name, Transform parent, float size,
            Vector2 position, Vector2 dimensions)
        {
            GameObject text = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            text.transform.SetParent(parent, false);
            RectTransform rect = text.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = dimensions;
            TextMeshProUGUI label = text.GetComponent<TextMeshProUGUI>();
            label.font = TMP_Settings.defaultFontAsset;
            label.fontSize = size;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;
            label.enabled = false;
            return label;
        }
    }
}

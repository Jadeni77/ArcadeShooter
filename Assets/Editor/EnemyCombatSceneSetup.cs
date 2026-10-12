using ArcadeShooter.Enemies;
using ArcadeShooter.Player;
using ArcadeShooter.UI;
using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ArcadeShooter.Editor
{
    // Additive M3 setup: retains the M1 graybox and M2 targets, rifle, and HUD.
    public static class EnemyCombatSceneSetup
    {
        private const string ScenePath = "Assets/Scenes/FirstPersonTest.unity";
        private const string PlayerPath = "Assets/Prefabs/FirstPersonPlayer.prefab";
        private const string EnemyPath = "Assets/Prefabs/MeleeEnemy.prefab";
        private const string NavPath = "Assets/Scenes/FirstPersonTestNavMesh.asset";

        [MenuItem("ArcadeShooter/Add Enemy Combat Test Content")]
        public static void Build()
        {
            if (EditorApplication.isPlaying)
                throw new System.InvalidOperationException("Exit Play mode before adding enemy combat content.");

            AddPlayerHealth();
            GameObject enemyPrefab = MakeEnemyPrefab();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject environment = GameObject.Find("Three Room Graybox");
            if (environment == null)
                throw new System.InvalidOperationException("The three-room test graybox was not found.");
            AddNavigation(environment);
            if (GameObject.Find("Melee Enemies") == null)
            {
                GameObject group = new GameObject("Melee Enemies");
                Spawn(enemyPrefab, group.transform, "Room 1 Melee Enemy", new Vector3(-13f, 0f, 3.5f));
                Spawn(enemyPrefab, group.transform, "Room 2 Melee Enemy", new Vector3(1.5f, 0f, 3.2f));
                Spawn(enemyPrefab, group.transform, "Room 3 Melee Enemy", new Vector3(11.5f, 0f, -2.5f));
            }
            ValidatePath(new Vector3(-10f, 0f, 0f), new Vector3(0f, 0f, 0f));
            ValidatePath(new Vector3(0f, 0f, 0f), new Vector3(10f, 0f, 0f));
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("M3 enemy combat content saved. Navigation paths connect all three rooms.");
        }

        private static void AddPlayerHealth()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PlayerPath);
            try
            {
                if (root.GetComponent<PlayerHealth>() != null) return;
                PlayerHealth health = root.AddComponent<PlayerHealth>();
                Camera camera = root.GetComponentInChildren<Camera>();
                if (camera == null) throw new System.InvalidOperationException("Player camera was not found.");
                Canvas canvas = camera.GetComponentInChildren<Canvas>();
                if (canvas == null) throw new System.InvalidOperationException("Weapon HUD canvas was not found.");

                TextMeshProUGUI healthText = Text("Health", canvas.transform, "HEALTH  100 / 100", 30f,
                    new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                    new Vector2(42f, 36f), new Vector2(450f, 70f), TextAlignmentOptions.Left);
                TextMeshProUGUI deathText = Text("Death and Restart", canvas.transform,
                    "YOU DIED\nPress R to restart", 50f,
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    Vector2.zero, new Vector2(800f, 190f), TextAlignmentOptions.Center);
                deathText.enabled = false;
                GameObject overlayObject = new GameObject("Damage Flash", typeof(RectTransform), typeof(Image));
                overlayObject.transform.SetParent(canvas.transform, false);
                RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
                overlayRect.anchorMin = Vector2.zero;
                overlayRect.anchorMax = Vector2.one;
                overlayRect.offsetMin = Vector2.zero;
                overlayRect.offsetMax = Vector2.zero;
                Image overlay = overlayObject.GetComponent<Image>();
                overlay.color = new Color(1f, 0.08f, 0.04f, 0.16f);
                overlay.raycastTarget = false;
                overlay.enabled = false;
                // Keep health and death copy above the translucent flash.
                healthText.transform.SetAsLastSibling();
                deathText.transform.SetAsLastSibling();

                PlayerHealthHud hud = canvas.gameObject.AddComponent<PlayerHealthHud>();
                SerializedObject serialized = new SerializedObject(hud);
                serialized.FindProperty("playerHealth").objectReferenceValue = health;
                serialized.FindProperty("healthText").objectReferenceValue = healthText;
                serialized.FindProperty("deathText").objectReferenceValue = deathText;
                serialized.FindProperty("damageOverlay").objectReferenceValue = overlay;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, PlayerPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static TextMeshProUGUI Text(string name, Transform parent, string content, float size,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 dimensions,
            TextAlignmentOptions alignment)
        {
            GameObject child = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            child.transform.SetParent(parent, false);
            RectTransform rect = child.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = dimensions;
            TextMeshProUGUI label = child.GetComponent<TextMeshProUGUI>();
            label.font = TMP_Settings.defaultFontAsset;
            label.fontSize = size;
            label.alignment = alignment;
            label.color = Color.white;
            label.raycastTarget = false;
            label.text = content;
            return label;
        }

        private static GameObject MakeEnemyPrefab()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPath);
            if (existing != null) return existing;
            Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Melee Enemy Placeholder.mat");
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.SetColor("_BaseColor", new Color(0.65f, 0.19f, 0.2f));
                AssetDatabase.CreateAsset(material, "Assets/Materials/Melee Enemy Placeholder.mat");
            }

            GameObject root = new GameObject("Melee Enemy");
            NavMeshAgent agent = root.AddComponent<NavMeshAgent>();
            agent.radius = 0.35f;
            agent.height = 1.8f;
            agent.speed = 3.5f;
            agent.angularSpeed = 540f;
            agent.acceleration = 15f;
            root.AddComponent<EnemyHealth>();
            root.AddComponent<MeleeEnemy>();
            root.AddComponent<EnemyFeedback>();
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body and Hit Collider";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
            body.GetComponent<Renderer>().sharedMaterial = material;
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "Facing Marker";
            eye.transform.SetParent(root.transform, false);
            eye.transform.localPosition = new Vector3(0f, 1.35f, 0.32f);
            eye.transform.localScale = Vector3.one * 0.15f;
            Object.DestroyImmediate(eye.GetComponent<Collider>());
            eye.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Muzzle Flash.mat");
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, EnemyPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void AddNavigation(GameObject environment)
        {
            NavMeshSurface surface = environment.GetComponent<NavMeshSurface>();
            if (surface == null) surface = environment.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = ~0;
            if (surface.navMeshData == null)
            {
                NavMeshData savedData = AssetDatabase.LoadAssetAtPath<NavMeshData>(NavPath);
                if (savedData != null)
                    surface.navMeshData = savedData;
                else
                {
                    surface.BuildNavMesh();
                    if (surface.navMeshData == null)
                        throw new System.InvalidOperationException("NavMesh bake produced no data.");
                    AssetDatabase.CreateAsset(surface.navMeshData, NavPath);
                }
            }
            surface.AddData();
        }

        private static void Spawn(GameObject prefab, Transform parent, string name, Vector3 position)
        {
            GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            enemy.name = name;
            enemy.transform.position = position;
        }

        private static void ValidatePath(Vector3 from, Vector3 to)
        {
            if (!NavMesh.SamplePosition(from, out NavMeshHit start, 1.5f, NavMesh.AllAreas) ||
                !NavMesh.SamplePosition(to, out NavMeshHit end, 1.5f, NavMesh.AllAreas))
                throw new System.InvalidOperationException("NavMesh does not cover a room center.");
            NavMeshPath path = new NavMeshPath();
            if (!NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, path) ||
                path.status != NavMeshPathStatus.PathComplete)
                throw new System.InvalidOperationException("NavMesh path through a doorway is incomplete.");
        }
    }
}

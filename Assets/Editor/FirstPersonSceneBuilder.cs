using System.Collections.Generic;
using ArcadeShooter.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ArcadeShooter.Editor
{
    public static class FirstPersonSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/FirstPersonTest.unity";
        private const string PrefabPath = "Assets/Prefabs/FirstPersonPlayer.prefab";
        private const string InputPath = "Assets/InputSystem_Actions.inputactions";

        [MenuItem("ArcadeShooter/Build First-Person Test Scene")]
        public static void Build()
        {
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            if (actions == null)
            {
                Debug.LogError($"Missing Input System actions asset: {InputPath}");
                return;
            }

            EnsureFolder("Assets/Prefabs");
            EnsureFolder("Assets/Materials");

            Material floor = MakeMaterial("Floor", new Color(0.42f, 0.46f, 0.5f));
            Material wall = MakeMaterial("Wall", new Color(0.67f, 0.7f, 0.73f));
            Material obstacle = MakeMaterial("Obstacle", new Color(0.75f, 0.48f, 0.26f));

            GameObject playerPrefab = BuildPlayerPrefab(actions);
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "FirstPersonTest";

            GameObject environment = new GameObject("Three Room Graybox");
            Transform world = environment.transform;

            for (int room = 0; room < 3; room++)
            {
                float x = (room - 1) * 10f;
                MakeBox($"Room {room + 1} Floor", world, new Vector3(x, -0.25f, 0f),
                    new Vector3(10f, 0.5f, 10f), floor);
            }

            // The two shared walls leave a 3 m wide, 2.4 m high passage.
            foreach (float x in new[] { -5f, 5f })
            {
                MakeBox("Passage Wall North", world, new Vector3(x, 1.6f, 3.25f),
                    new Vector3(0.3f, 3.2f, 3.5f), wall);
                MakeBox("Passage Wall South", world, new Vector3(x, 1.6f, -3.25f),
                    new Vector3(0.3f, 3.2f, 3.5f), wall);
                MakeBox("Passage Lintel", world, new Vector3(x, 2.8f, 0f),
                    new Vector3(0.3f, 0.8f, 3f), wall);
            }

            MakeBox("West Outer Wall", world, new Vector3(-15f, 1.6f, 0f),
                new Vector3(0.3f, 3.2f, 10.3f), wall);
            MakeBox("East Outer Wall", world, new Vector3(15f, 1.6f, 0f),
                new Vector3(0.3f, 3.2f, 10.3f), wall);
            MakeBox("North Outer Wall", world, new Vector3(0f, 1.6f, 5f),
                new Vector3(30.3f, 3.2f, 0.3f), wall);
            MakeBox("South Outer Wall", world, new Vector3(0f, 1.6f, -5f),
                new Vector3(30.3f, 3.2f, 0.3f), wall);

            MakeBox("Room 1 Crate", world, new Vector3(-11f, 0.6f, 2.5f),
                new Vector3(1.5f, 1.2f, 1.5f), obstacle);
            MakeBox("Room 2 Low Barrier", world, new Vector3(0f, 0.55f, -2.5f),
                new Vector3(2.4f, 1.1f, 0.8f), obstacle);
            MakeBox("Room 3 Crate", world, new Vector3(10f, 0.8f, 2.6f),
                new Vector3(1.7f, 1.6f, 1.7f), obstacle);

            GameObject spawn = new GameObject("Player Spawn Point");
            spawn.transform.position = new Vector3(-10f, 1f, 0f);
            spawn.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
            player.transform.SetPositionAndRotation(spawn.transform.position, spawn.transform.rotation);

            GameObject sun = new GameObject("Directional Light");
            sun.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.5f;
            light.shadows = LightShadows.Soft;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.5f, 0.53f, 0.58f);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
            {
                if (existing.path != ScenePath)
                    buildScenes.Add(new EditorBuildSettingsScene(existing.path, false));
            }
            EditorBuildSettings.scenes = buildScenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log($"M1 test scene saved at {ScenePath}. Press Play to explore all three rooms.");
        }

        private static GameObject BuildPlayerPrefab(InputActionAsset actions)
        {
            GameObject root = new GameObject("FirstPersonPlayer");
            CharacterController controller = root.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = Vector3.zero;
            controller.stepOffset = 0.3f;
            controller.slopeLimit = 45f;

            PlayerInput input = root.AddComponent<PlayerInput>();
            input.actions = actions;
            input.defaultActionMap = "Player";
            input.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
            root.AddComponent<FirstPersonMovement>();

            GameObject pivot = new GameObject("Camera Pivot");
            pivot.transform.SetParent(root.transform, false);
            pivot.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            GameObject cameraObject = new GameObject("First-Person Camera");
            cameraObject.transform.SetParent(pivot.transform, false);
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.nearClipPlane = 0.05f;
            cameraObject.AddComponent<AudioListener>();

            FirstPersonLook look = root.AddComponent<FirstPersonLook>();
            SerializedObject serializedLook = new SerializedObject(look);
            serializedLook.FindProperty("cameraPivot").objectReferenceValue = pivot.transform;
            serializedLook.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject MakeBox(string name, Transform parent, Vector3 position,
            Vector3 scale, Material material)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent);
            box.transform.position = position;
            box.transform.localScale = scale;
            box.GetComponent<MeshRenderer>().sharedMaterial = material;
            return box;
        }

        private static Material MakeMaterial(string name, Color color)
        {
            string path = $"Assets/Materials/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                    throw new System.InvalidOperationException("URP Lit shader was not found.");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder("Assets", path.Substring("Assets/".Length));
        }
    }
}

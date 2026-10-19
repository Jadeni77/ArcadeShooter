using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ArcadeShooter.Editor
{
    // Authoring only. Visuals are outside the collider hierarchy used by NavMeshSurface.
    public static class EnvironmentVisualsSceneSetup
    {
        private const string Scene = "Assets/Scenes/FirstPersonTest.unity";
        private const string Models = "Assets/Environment/Models/Quaternius/";
        private const string Materials = "Assets/Environment/Materials/";
        private static Transform root;

        public static void BuildRoomOne()
        {
            EditorSceneManager.OpenScene(Scene);
            Prepare();
            BuildRoom(1, -10);
            Finish();
            CaptureRoomOne();
            ValidateScene();
        }

        public static void CompleteMap()
        {
            EditorSceneManager.OpenScene(Scene);
            Prepare();
            BuildRoom(1, -10);
            BuildRoom(2, 0);
            BuildRoom(3, 10);
            Atmosphere();
            Finish();
            CaptureRoomOne();
            Capture(new Vector3(-2.8f, 1.7f, -3.9f), new Vector3(2.8f, 1.4f, 0),
                "/private/tmp/ArcadeShooter-M8A-room2.png");
            Capture(new Vector3(7.2f, 1.7f, -3.9f), new Vector3(12.8f, 1.4f, 0),
                "/private/tmp/ArcadeShooter-M8A-room3.png");
            Capture(new Vector3(-7.7f, 1.7f, 2.5f), new Vector3(-12.1f, 1.2f, -2.8f),
                "/private/tmp/ArcadeShooter-M8A-extraction.png");
            ValidateScene();
        }

        private static void Prepare()
        {
            var old = GameObject.Find("Environment Visuals");
            if (old) UnityEngine.Object.DestroyImmediate(old);
            root = new GameObject("Environment Visuals").transform;
            Directory.CreateDirectory(Materials);
            var importer = (TextureImporter)AssetImporter.GetAtPath(
                "Assets/Environment/Textures/PolyHaven/metal_plate_nor_gl_1k.png");
            if (importer && importer.textureType != TextureImporterType.NormalMap)
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.SaveAndReimport();
            }
            Palette();
            foreach (var obj in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (!obj.transform.IsChildOf(GameObject.Find("Three Room Graybox").transform)) continue;
                if (obj.name.Contains("Outer Wall") || obj.name.Contains("Passage Wall") ||
                    obj.name == "Passage Lintel" || obj.name == "Room 2 Low Barrier")
                {
                    var renderer = obj.GetComponent<Renderer>();
                    if (renderer) renderer.sharedMaterial = Mat("Shadow Steel");
                }
                if (obj.name == "Room 1 Crate" || obj.name == "Room 3 Crate")
                {
                    var renderer = obj.GetComponent<Renderer>();
                    if (renderer) renderer.enabled = false;
                }
            }
        }

        private static void BuildRoom(int number, float x)
        {
            var room = Group(root, "Room " + number + " Facility");
            var accent = number == 1 ? "Wayfinding Cyan" : number == 2 ? "Warning Amber" : "Lab Blue";
            var floor = Group(room, "Modular Floor");
            for (int ix = -2; ix <= 2; ix++)
                for (int iz = -2; iz <= 2; iz++)
                    Model("floor_tile_basic", floor, new Vector3(x + ix * 2, -0.065f, iz * 2),
                        Quaternion.identity, Vector3.one, accent, true);

            var walls = Group(room, "Wall Panels");
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = -1; i <= 1; i += 2)
                    Panel(walls, new Vector3(x + i * 2.5f, 0, side * 4.83f),
                        side < 0 ? 0 : 180, 1.25f, accent);
                if (number == 1 || number == 3)
                {
                    float edge = number == 1 ? x - 4.83f : x + 4.83f;
                    for (int i = -1; i <= 1; i += 2)
                        Panel(walls, new Vector3(edge, 0, i * 2.5f), 90, 1.25f, accent);
                }
            }
            if (number < 3) Passage(walls, x + 5, accent);

            var ceiling = Group(room, "Ceiling Panels");
            for (int ix = -2; ix <= 2; ix++)
                for (int iz = -2; iz <= 2; iz++)
                    Model((ix + iz + number) % 3 == 0 ? "roof_tile_details" : "roof_tile_empty",
                        ceiling, new Vector3(x + ix * 2, 3.21f, iz * 2),
                        Quaternion.identity, Vector3.one, accent);

            var details = Group(room, "Facility Details");
            Model("props_computer", details, new Vector3(x - 4.25f, 0, -3.55f),
                Quaternion.Euler(0, 45, 0), Vector3.one, accent);
            Model("props_shelf", details, new Vector3(x + 3.6f, 0, 4.25f),
                Quaternion.Euler(0, 180, 0), Vector3.one, accent);
            Model("details_vent_1", details, new Vector3(x - 3, 2.52f, -4.62f),
                Quaternion.identity, Vector3.one * 1.5f, accent);
            if (number == 1)
                Model("props_crate", details, new Vector3(-11, 0, 2.5f),
                    Quaternion.identity, new Vector3(1.875f, 1.5f, 1.875f), accent);
            if (number == 3)
                Model("props_crate", details, new Vector3(10, 0, 2.6f),
                    Quaternion.identity, new Vector3(2.125f, 2, 2.125f), accent);
            for (int i = -1; i <= 1; i += 2)
                Marker(details, "Wall Guide", new Vector3(x + i * 2.7f, 1.18f, -4.5f),
                    new Vector3(1.1f, 0.06f, 0.025f), accent);
            if (number == 1)
            {
                // Existing extraction marker and label keep their mission state.
                foreach (float edge in new[] { -13.9f, -10.1f })
                    Marker(details, "Extraction Approach", new Vector3(edge, 0.028f, -2.8f),
                        new Vector3(0.12f, 0.02f, 2.4f), accent);
            }
            else
            {
                for (int sx = -1; sx <= 1; sx += 2)
                    for (int sz = -1; sz <= 1; sz += 2)
                        Marker(details, "Danger Zone Corner", new Vector3(x + sx * 3.3f, 0.03f, sz * 3.3f),
                            new Vector3(0.8f, 0.025f, 0.12f), "Warning Amber");
                for (int side = -1; side <= 1; side += 2)
                {
                    Marker(details, "Containment Boundary", new Vector3(x + side * 3.3f, 0.03f, 0),
                        new Vector3(0.09f, 0.025f, 5.6f), "Warning Amber");
                    Marker(details, "Containment Boundary", new Vector3(x, 0.03f, side * 3.3f),
                        new Vector3(5.6f, 0.025f, 0.09f), "Warning Amber");
                }
            }

            var fixtures = Group(room, "Practical Lighting");
            for (int z = -2; z <= 2; z += 4)
            {
                Marker(fixtures, "Ceiling Diffuser", new Vector3(x, 3.12f, z),
                    new Vector3(2.1f, 0.04f, 0.34f), "Soft White Emission");
                Lamp(fixtures, "Overhead Light", new Vector3(x, 2.9f, z),
                    new Color(0.72f, 0.9f, 1), 2.2f, 7.5f);
                if (number > 1)
                {
                    Marker(fixtures, "Zone Warning Fixture", new Vector3(x + 4.55f, 2.3f, z),
                        new Vector3(0.08f, 0.48f, 0.48f), "Warning Emission");
                    Lamp(fixtures, "Zone Warning Light", new Vector3(x + 4.25f, 2.3f, z),
                        new Color(1, 0.39f, 0.07f), 0.65f, 2.5f);
                }
            }
        }

        private static void Passage(Transform parent, float x, string accent)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Panel(parent, new Vector3(x, 0, side * 3.28f), 90, 0.86f, accent);
                Model("column_slim", parent, new Vector3(x, 0, side * 1.77f),
                    Quaternion.identity, Vector3.one * 0.72f, accent);
                Model(side < 0 ? "door_double_l" : "door_double_r", parent,
                    new Vector3(x - 0.15f, 0, side * 2.45f),
                    Quaternion.Euler(0, 90, 0), new Vector3(0.65f, 0.84f, 0.84f), accent);
            }
            Marker(parent, "Open Passage Header", new Vector3(x, 2.87f, 0),
                new Vector3(0.38f, 0.55f, 3.08f), "Shadow Steel");
            Marker(parent, "Passage Light", new Vector3(x, 2.52f, 0),
                new Vector3(0.4f, 0.055f, 2.8f), "Soft White Emission");
        }

        private static void Panel(Transform parent, Vector3 pos, float yaw, float width, string accent)
        {
            Model("wall_1", parent, pos, Quaternion.Euler(0, yaw, 0),
                new Vector3(width, 0.72f, 0.7f), accent);
        }

        private static Transform Group(Transform parent, string name)
        {
            var obj = new GameObject(name).transform;
            obj.SetParent(parent);
            return obj;
        }

        private static GameObject Model(string name, Transform parent, Vector3 pos, Quaternion rot,
            Vector3 scale, string accent, bool floor = false)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(Models + name + ".obj");
            if (!source) throw new InvalidOperationException("Missing approved model: " + name);
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(source);
            obj.name = name;
            obj.transform.SetParent(parent);
            obj.transform.SetPositionAndRotation(pos, rot);
            obj.transform.localScale = scale;
            foreach (var collider in obj.GetComponentsInChildren<Collider>(true))
                UnityEngine.Object.DestroyImmediate(collider);
            var sourceRenderer = source.GetComponentInChildren<Renderer>();
            var original = sourceRenderer.sharedMaterials;
            foreach (var renderer in obj.GetComponentsInChildren<Renderer>(true))
            {
                var assigned = renderer.sharedMaterials;
                for (int i = 0; i < assigned.Length; i++)
                {
                    string slot = i < original.Length && original[i] ? original[i].name : "main";
                    assigned[i] = floor && slot == "main" ? Mat("Metal Plate") :
                        slot == "accent" ? Mat(accent) :
                        slot == "light" ? Mat("Soft White Emission") :
                        slot == "dark_grey" || slot == "pipes" ? Mat("Shadow Steel") :
                        slot == "black" ? Mat("Near Black") :
                        slot == "dark_accent" ? Mat("Dark Teal") : Mat("Porcelain Panels");
                }
                renderer.sharedMaterials = assigned;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
            }
            return obj;
        }

        private static void Marker(Transform parent, string name, Vector3 pos, Vector3 scale, string material)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.SetParent(parent);
            obj.transform.position = pos;
            obj.transform.localScale = scale;
            UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
            obj.GetComponent<Renderer>().sharedMaterial = Mat(material);
            obj.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        }

        private static void Lamp(Transform parent, string name, Vector3 pos,
            Color color, float intensity, float range)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent);
            obj.transform.position = pos;
            var light = obj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
        }

        private static Material Mat(string name) =>
            AssetDatabase.LoadAssetAtPath<Material>(Materials + name + ".mat");

        private static void Palette()
        {
            Make("Porcelain Panels", new Color(0.53f, 0.65f, 0.69f), 0.32f, 0.55f);
            Make("Shadow Steel", new Color(0.105f, 0.15f, 0.18f), 0.65f, 0.48f);
            Make("Wayfinding Cyan", new Color(0.025f, 0.55f, 0.59f), 0.25f, 0.53f);
            Make("Dark Teal", new Color(0.025f, 0.19f, 0.22f), 0.5f, 0.5f);
            Make("Lab Blue", new Color(0.12f, 0.32f, 0.62f), 0.28f, 0.48f);
            Make("Warning Amber", new Color(0.97f, 0.42f, 0.065f), 0.18f, 0.4f);
            Make("Near Black", new Color(0.045f, 0.065f, 0.07f), 0.3f, 0.4f);
            Make("Enemy Threat", new Color(0.92f, 0.20f, 0.075f), 0.18f, 0.38f);
            Make("Soft White Emission", new Color(0.64f, 0.84f, 0.88f), 0.1f, 0.45f,
                new Color(0.75f, 1, 1.05f) * 1.25f);
            Make("Warning Emission", new Color(0.95f, 0.24f, 0.025f), 0.1f, 0.45f,
                new Color(1.3f, 0.3f, 0.015f));
            var floor = Make("Metal Plate", new Color(0.43f, 0.54f, 0.57f), 0.52f, 0.46f);
            floor.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/Environment/Textures/PolyHaven/metal_plate_diff_1k.jpg"));
            floor.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/Environment/Textures/PolyHaven/metal_plate_nor_gl_1k.png"));
            floor.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(floor);
        }

        private static Material Make(string name, Color color, float metal, float smooth, Color? emission = null)
        {
            var material = Mat(name);
            if (!material)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, Materials + name + ".mat");
            }
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metal);
            material.SetFloat("_Smoothness", smooth);
            if (emission.HasValue)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission.Value);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void Atmosphere()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.27f, 0.36f, 0.4f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.18f, 0.26f, 0.3f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 18;
            RenderSettings.fogEndDistance = 38;
            var sun = GameObject.Find("Directional Light");
            if (sun)
            {
                sun.GetComponent<Light>().intensity = 0.6f;
                sun.GetComponent<Light>().color = new Color(0.71f, 0.84f, 1);
            }
            const string path = "Assets/Environment/FacilityVolume.asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (!profile)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }
            if (!profile.TryGet(out Tonemapping tone)) tone = profile.Add<Tonemapping>();
            tone.mode.Override(TonemappingMode.Neutral);
            if (!profile.TryGet(out Bloom bloom)) bloom = profile.Add<Bloom>();
            bloom.threshold.Override(1.3f);
            bloom.intensity.Override(0.13f);
            bloom.highQualityFiltering.Override(false);
            EditorUtility.SetDirty(profile);
            var obj = new GameObject("Facility Atmosphere Volume");
            obj.transform.SetParent(root);
            var volume = obj.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;

            const string prefab = "Assets/Prefabs/FirstPersonPlayer.prefab";
            var player = PrefabUtility.LoadPrefabContents(prefab);
            var camera = player.GetComponentInChildren<Camera>(true);
            if (!camera) throw new InvalidOperationException("Player camera missing");
            var data = camera.GetComponent<UniversalAdditionalCameraData>();
            if (!data) data = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing = true;
            PrefabUtility.SaveAsPrefabAsset(player, prefab);
            PrefabUtility.UnloadPrefabContents(player);

            const string enemyPath = "Assets/Prefabs/MeleeEnemy.prefab";
            var enemy = PrefabUtility.LoadPrefabContents(enemyPath);
            var body = enemy.transform.Find("Body and Hit Collider");
            var facing = enemy.transform.Find("Facing Marker");
            if (!body || !facing) throw new InvalidOperationException("Enemy visual roots missing");
            body.GetComponent<Renderer>().sharedMaterial = Mat("Enemy Threat");
            facing.GetComponent<Renderer>().sharedMaterial = Mat("Warning Emission");
            PrefabUtility.SaveAsPrefabAsset(enemy, enemyPath);
            PrefabUtility.UnloadPrefabContents(enemy);
        }

        private static void Finish()
        {
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
        }

        public static void ValidateScene()
        {
            var visuals = GameObject.Find("Environment Visuals");
            var graybox = GameObject.Find("Three Room Graybox");
            if (!visuals || !graybox || visuals.transform.IsChildOf(graybox.transform))
                throw new InvalidOperationException("Visual or gameplay hierarchy missing");
            if (visuals.GetComponentsInChildren<Collider>(true).Length != 0)
                throw new InvalidOperationException("Decorative colliders could alter navigation");
            var positions = new[] { new Vector3(-10, 0, 0), new Vector3(0, 0, 0), new Vector3(10, 0, 0) };
            for (int i = 0; i < positions.Length - 1; i++)
            {
                var path = new NavMeshPath();
                if (!NavMesh.CalculatePath(positions[i], positions[i + 1], NavMesh.AllAreas, path) ||
                    path.status != NavMeshPathStatus.PathComplete)
                    throw new InvalidOperationException("Saved NavMesh route failed: " + i);
            }
            Debug.Log("M8A validation passed: no decorative colliders and both room-to-room NavMesh paths complete.");
        }

        public static void CaptureRoomOne()
        {
            Capture(new Vector3(-13.15f, 1.7f, -3.5f), new Vector3(-7.2f, 1.5f, 0),
                "/private/tmp/ArcadeShooter-M8A-room1.png");
        }

        private static void Capture(Vector3 position, Vector3 lookAt, string output)
        {
            var obj = new GameObject("M8A Preview Camera");
            var camera = obj.AddComponent<Camera>();
            obj.transform.position = position;
            obj.transform.LookAt(lookAt);
            camera.fieldOfView = 70;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 45;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            var target = new RenderTexture(1280, 720, 24);
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            image.Apply();
            File.WriteAllBytes(output, image.EncodeToPNG());
            RenderTexture.active = null;
            camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(image);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(obj);
        }
    }
}

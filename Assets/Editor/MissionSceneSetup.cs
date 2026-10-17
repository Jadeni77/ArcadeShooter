using System;
using ArcadeShooter.Encounters;
using ArcadeShooter.Missions;
using ArcadeShooter.UI;
using TMPro;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcadeShooter.Editor
{
    // Adds the M6 mission to the existing three-room scene, or upgrades mission content that an
    // earlier version of this command created. Running it again changes nothing.
    public static class MissionSceneSetup
    {
        private const string PlayerPath = "Assets/Prefabs/FirstPersonPlayer.prefab";
        internal const string DeathMessage = "YOU DIED\nYour squad can still extract";

        [MenuItem("ArcadeShooter/Add Mission Extraction and Scoring")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/FirstPersonTest.unity", OpenSceneMode.Single);
            MissionController mission = UnityEngine.Object.FindAnyObjectByType<MissionController>();
            if (mission == null) mission = CreateMission();
            UpgradeExtractionIndicators(mission.Extraction);
            MigrateRankThresholds(mission.GetComponent<MissionScoreTracker>());
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            UpdateDeathMessage();
            AssetDatabase.SaveAssets();
            Debug.Log("M6 mission content is up to date in the three-room scene.");
        }

        private static MissionController CreateMission()
        {
            DangerZone zoneA = GameObject.Find("Danger Zone A")?.GetComponent<DangerZone>();
            DangerZone zoneB = GameObject.Find("Danger Zone B")?.GetComponent<DangerZone>();
            if (zoneA == null || zoneB == null) throw new InvalidOperationException("Existing zones A and B are required.");

            var missionRoot = new GameObject("Mission Control", typeof(NetworkObject), typeof(MissionScoreTracker), typeof(MissionController), typeof(MissionHud));
            var zoneObjectiveA = Objective(zoneA.gameObject, "Clear Danger Zone A", zoneA);
            var zoneObjectiveB = Objective(zoneB.gameObject, "Clear Danger Zone B", zoneB);

            Material amber = MaterialAsset("Mission Sample", new Color(1f, 0.64f, 0.1f));
            var sampleRoot = new GameObject("Research Sample", typeof(NetworkObject));
            sampleRoot.transform.position = new Vector3(13.2f, 1.2f, -1.8f);
            var sampleObjective = Objective(sampleRoot, "Collect the research sample", null);
            var sample = sampleRoot.AddComponent<ResearchSample>();
            GameObject sampleVisual = Primitive("Sample Canister", sampleRoot.transform, PrimitiveType.Cylinder,
                new Vector3(0.35f, 0.35f, 0.35f), amber);
            GameObject sampleLabel = Label("Research Sample Label", sampleRoot.transform, "RESEARCH SAMPLE\n[E] COLLECT", new Vector3(0, 1.05f, 0), Color.yellow);
            sampleLabel.transform.SetParent(sampleVisual.transform, true);
            SetReference(sample, "sampleVisual", sampleVisual);

            var extractionRoot = new GameObject("Extraction Point", typeof(NetworkObject), typeof(BoxCollider), typeof(ExtractionPoint));
            extractionRoot.transform.position = new Vector3(-12, 0, -2.8f);
            var box = extractionRoot.GetComponent<BoxCollider>();
            box.isTrigger = true;
            box.center = new Vector3(0, 1.4f, 0);
            box.size = new Vector3(4, 2.8f, 3.2f);

            var data = new SerializedObject(missionRoot.GetComponent<MissionController>());
            var objectives = data.FindProperty("objectives");
            objectives.arraySize = 3;
            objectives.GetArrayElementAtIndex(0).objectReferenceValue = zoneObjectiveA;
            objectives.GetArrayElementAtIndex(1).objectReferenceValue = zoneObjectiveB;
            objectives.GetArrayElementAtIndex(2).objectReferenceValue = sampleObjective;
            data.FindProperty("extraction").objectReferenceValue = extractionRoot.GetComponent<ExtractionPoint>();
            data.ApplyModifiedPropertiesWithoutUndo();
            return missionRoot.GetComponent<MissionController>();
        }

        // Each indicator is an unscaled group holding a flattened plate and a camera-facing label.
        // A label parented directly to the flattened plate is sheared whenever it turns to the camera.
        private static void UpgradeExtractionIndicators(ExtractionPoint extraction)
        {
            if (extraction == null) throw new InvalidOperationException("The mission has no extraction point.");
            var data = new SerializedObject(extraction);
            SerializedProperty active = data.FindProperty("activeIndicator");
            SerializedProperty locked = data.FindProperty("lockedIndicator");
            if (IsIndicatorGroup(active.objectReferenceValue as GameObject) &&
                IsIndicatorGroup(locked.objectReferenceValue as GameObject)) return;
            if (active.objectReferenceValue != null) UnityEngine.Object.DestroyImmediate(active.objectReferenceValue);
            if (locked.objectReferenceValue != null) UnityEngine.Object.DestroyImmediate(locked.objectReferenceValue);

            GameObject open = Indicator(extraction.transform, "Extraction Active", "Extraction Beacon",
                MaterialAsset("Mission Extraction", new Color(0.05f, 0.8f, 0.7f)), "EXTRACTION\nREGROUP HERE", Color.cyan);
            open.SetActive(false);
            GameObject closed = Indicator(extraction.transform, "Extraction Locked", "Locked Beacon",
                MaterialAsset("Mission Locked", new Color(0.28f, 0.31f, 0.36f)), "EXTRACTION LOCKED\nCOMPLETE ALL OBJECTIVES", Color.gray);
            active.objectReferenceValue = open;
            locked.objectReferenceValue = closed;
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static bool IsIndicatorGroup(GameObject indicator) =>
            indicator != null && indicator.GetComponent<Renderer>() == null && indicator.transform.localScale == Vector3.one;

        private static GameObject Indicator(Transform root, string name, string labelName, Material material, string text, Color color)
        {
            var group = new GameObject(name);
            group.transform.SetParent(root, false);
            GameObject plate = Primitive(name + " Plate", group.transform, PrimitiveType.Cube, new Vector3(4, 0.05f, 3.2f), material);
            plate.transform.localPosition = new Vector3(0, 0.035f, 0);
            Label(labelName, group.transform, text, new Vector3(0, 2.7f, 0), color);
            return group;
        }

        // Keeps values a designer tuned; only the first M6 defaults are replaced.
        private static void MigrateRankThresholds(MissionScoreTracker tracker)
        {
            var data = new SerializedObject(tracker);
            SerializedProperty rules = data.FindProperty("scoring");
            SerializedProperty b = rules.FindPropertyRelative("RankBScore");
            SerializedProperty a = rules.FindPropertyRelative("RankAScore");
            SerializedProperty s = rules.FindPropertyRelative("RankSScore");
            if (b.intValue != 1800 || a.intValue != 2400 || s.intValue != 2800) return;
            var defaults = new MissionScoringRules();
            b.intValue = defaults.RankBScore;
            a.intValue = defaults.RankAScore;
            s.intValue = defaults.RankSScore;
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        // M5's "Leave to start a new match" would end the session for everyone if the host followed it.
        private static void UpdateDeathMessage()
        {
            GameObject player = PrefabUtility.LoadPrefabContents(PlayerPath);
            try
            {
                TextMeshProUGUI label = FindChild(player.transform, "Death and Restart")?.GetComponent<TextMeshProUGUI>();
                if (label == null || label.text == DeathMessage) return;
                label.text = DeathMessage;
                PrefabUtility.SaveAsPrefabAsset(player, PlayerPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(player); }
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

        private static MissionObjective Objective(GameObject root, string label, DangerZone zone)
        {
            var objective = root.AddComponent<MissionObjective>();
            var data = new SerializedObject(objective);
            data.FindProperty("label").stringValue = label;
            data.FindProperty("dangerZone").objectReferenceValue = zone;
            data.ApplyModifiedPropertiesWithoutUndo();
            return objective;
        }
        private static void SetReference(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            var data = new SerializedObject(target);
            data.FindProperty(field).objectReferenceValue = value;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static Material MaterialAsset(string name, Color color)
        {
            string path = "Assets/Materials/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", color);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
        private static GameObject Primitive(string name, Transform parent, PrimitiveType type, Vector3 scale, Material material)
        {
            GameObject visual = GameObject.CreatePrimitive(type);
            visual.name = name;
            visual.transform.SetParent(parent, false);
            visual.transform.localScale = scale;
            UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.GetComponent<Renderer>().sharedMaterial = material;
            return visual;
        }
        private static GameObject Label(string name, Transform parent, string text, Vector3 position, Color color)
        {
            GameObject root = new GameObject(name, typeof(TextMeshPro), typeof(MissionWorldLabel));
            root.transform.SetParent(parent, false);
            root.transform.localPosition = position;
            var label = root.GetComponent<TextMeshPro>();
            label.font = TMP_Settings.defaultFontAsset;
            label.text = text;
            label.fontSize = 2.6f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = color;
            label.rectTransform.sizeDelta = new Vector2(4, 1.2f);
            return root;
        }
    }
}

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
    public static class MissionSceneSetup
    {
        [MenuItem("ArcadeShooter/Add Mission Extraction and Scoring")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/FirstPersonTest.unity", OpenSceneMode.Single);
            DangerZone zoneA = GameObject.Find("Danger Zone A")?.GetComponent<DangerZone>();
            DangerZone zoneB = GameObject.Find("Danger Zone B")?.GetComponent<DangerZone>();
            if (zoneA == null || zoneB == null) throw new InvalidOperationException("Existing zones A and B are required.");
            if (UnityEngine.Object.FindAnyObjectByType<MissionController>() != null)
                throw new InvalidOperationException("Mission content already exists; edit its existing components.");

            var missionRoot = new GameObject("Mission Control", typeof(NetworkObject), typeof(MissionScoreTracker), typeof(MissionController), typeof(MissionHud));
            var zoneObjectiveA = Objective(zoneA.gameObject, "Clear Danger Zone A", zoneA);
            var zoneObjectiveB = Objective(zoneB.gameObject, "Clear Danger Zone B", zoneB);

            Material amber = MaterialAsset("Mission Sample", new Color(1f, 0.64f, 0.1f));
            Material cyan = MaterialAsset("Mission Extraction", new Color(0.05f, 0.8f, 0.7f));
            Material locked = MaterialAsset("Mission Locked", new Color(0.28f, 0.31f, 0.36f));
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
            GameObject active = Primitive("Extraction Active", extractionRoot.transform, PrimitiveType.Cube, new Vector3(4, 0.05f, 3.2f), cyan);
            active.transform.localPosition = new Vector3(0, 0.035f, 0);
            GameObject activeLabel = Label("Extraction Beacon", extractionRoot.transform, "EXTRACTION\nREGROUP HERE", new Vector3(0, 2.7f, 0), Color.cyan);
            activeLabel.transform.SetParent(active.transform, true);
            active.SetActive(false);
            GameObject inactive = Primitive("Extraction Locked", extractionRoot.transform, PrimitiveType.Cube, new Vector3(4, 0.05f, 3.2f), locked);
            inactive.transform.localPosition = new Vector3(0, 0.035f, 0);
            GameObject lockedLabel = Label("Locked Beacon", extractionRoot.transform, "EXTRACTION LOCKED\nCOMPLETE ALL OBJECTIVES", new Vector3(0, 2.7f, 0), Color.gray);
            lockedLabel.transform.SetParent(inactive.transform, true);
            var extraction = extractionRoot.GetComponent<ExtractionPoint>();
            SetReference(extraction, "activeIndicator", active);
            SetReference(extraction, "lockedIndicator", inactive);

            var data = new SerializedObject(missionRoot.GetComponent<MissionController>());
            var objectives = data.FindProperty("objectives");
            objectives.arraySize = 3;
            objectives.GetArrayElementAtIndex(0).objectReferenceValue = zoneObjectiveA;
            objectives.GetArrayElementAtIndex(1).objectReferenceValue = zoneObjectiveB;
            objectives.GetArrayElementAtIndex(2).objectReferenceValue = sampleObjective;
            data.FindProperty("extraction").objectReferenceValue = extraction;
            data.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("M6 mission content added to the existing three-room scene.");
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

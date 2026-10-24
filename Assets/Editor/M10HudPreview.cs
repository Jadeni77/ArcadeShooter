using System.IO;
using System.Reflection;
using ArcadeShooter.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcadeShooter.Editor
{
    // Captures an Editor-only visual sample of the real scene and UI builders.
    // Labels are representative because a direct scene load has no live session.
    public static class M10HudPreview
    {
        public static void Capture()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/FirstPersonTest.unity", OpenSceneMode.Single);
            Transform spawn = GameObject.Find("Player Spawn Point")?.transform;
            if (spawn == null) throw new System.InvalidOperationException("Player spawn point missing.");
            var cameraObject = new GameObject("M10 Review Camera", typeof(Camera));
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.transform.position = spawn.position + Vector3.up * 0.75f;
            camera.transform.rotation = spawn.rotation;
            camera.fieldOfView = 74;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 180f;
            camera.clearFlags = CameraClearFlags.Skybox;

            GameObject playerPrefab = PrefabUtility.LoadPrefabContents("Assets/Prefabs/FirstPersonPlayer.prefab");
            Canvas sourceHud = playerPrefab.GetComponentInChildren<WeaponHud>(true).GetComponent<Canvas>();
            GameObject hud = Object.Instantiate(sourceHud.gameObject);
            hud.name = "M10 Review Player HUD";
            PrefabUtility.UnloadPrefabContents(playerPrefab);
            hud.SetActive(true);
            Canvas playerCanvas = hud.GetComponent<Canvas>();
            SetupCanvas(playerCanvas, camera, 40);
            WeaponHud weapon = hud.GetComponent<WeaponHud>();
            if (hud.transform.Find("Ammo Readout") == null) Invoke(weapon, "Awake");
            weapon.SetAmmo(24, 90, false);
            PlayerHealthHud health = hud.GetComponent<PlayerHealthHud>();
            if (hud.transform.Find("Health Readout") == null) Invoke(health, "Awake");
            Invoke(health, "Refresh", 100f, 100f);
            hud.transform.Find("Death and Restart").GetComponent<TMP_Text>().enabled = false;
            hud.transform.Find("Damage Flash").GetComponent<UnityEngine.UI.Image>().enabled = false;
            hud.transform.Find("Hit Confirmation").GetComponent<UnityEngine.UI.Image>().enabled = false;
            TeammateHud squad = hud.GetComponent<TeammateHud>();
            if (squad != null && hud.transform.Find("Squad Status") == null) Invoke(squad, "Awake");
            Transform squadRoot = hud.transform.Find("Squad Status");
            if (squadRoot != null)
            {
                string[] names = { "ECHO / YOU", "ROOK", "VEX" };
                string[] values = { "100", "072", "095" };
                for (int i = 0; i < 4; i++)
                {
                    TMP_Text label = squadRoot.Find("Squad Member " + (i + 1)).GetComponent<TMP_Text>();
                    TMP_Text healthValue = squadRoot.Find("Health Value " + (i + 1)).GetComponent<TMP_Text>();
                    label.enabled = i < values.Length;
                    healthValue.enabled = i < values.Length;
                    if (i < values.Length) { label.text = names[i]; healthValue.text = values[i]; }
                }
            }

            MissionHud mission = Object.FindAnyObjectByType<MissionHud>();
            if (mission == null) throw new System.InvalidOperationException("Mission HUD missing.");
            Transform missionCanvas = mission.transform.Find("Mission UI");
            if (missionCanvas == null) { Invoke(mission, "Awake"); missionCanvas = mission.transform.Find("Mission UI"); }
            SetupCanvas(missionCanvas.GetComponent<Canvas>(), camera, 50);
            missionCanvas.Find("Mission Objectives").gameObject.SetActive(true);
            missionCanvas.Find("Extraction Tracker").gameObject.SetActive(true);
            SetText(missionCanvas, "Mission Objectives/Mission Name", "OPERATION 01 / CONTAINMENT BREACH");
            SetText(missionCanvas, "Mission Objectives/Mission Progress", "OBJECTIVES 1 / 3     04:28");
            SetText(missionCanvas, "Mission Objectives/Objective List", "<color=#A8D797>[DONE] CLEAR ZONE A</color>\n[  ] CLEAR ZONE B\n[  ] RECOVER SAMPLE");
            SetText(missionCanvas, "Extraction Tracker/Extraction Status", "EXTRACTION LOCKED  •  COMPLETE OBJECTIVES");

            EncounterHud encounter = Object.FindAnyObjectByType<EncounterHud>();
            if (encounter == null) throw new System.InvalidOperationException("Encounter HUD missing.");
            Canvas encounterCanvas = encounter.GetComponent<Canvas>();
            if (encounter.transform.Find("Encounter Banner") == null) Invoke(encounter, "Awake");
            SetupCanvas(encounterCanvas, camera, 55);
            Transform banner = encounter.transform.Find("Encounter Banner");
            banner.GetComponent<UnityEngine.UI.Image>().enabled = true;
            banner.GetComponent<CanvasGroup>().alpha = 1;
            TMP_Text warning = banner.Find("Encounter Notice").GetComponent<TMP_Text>();
            warning.text = "CONTAINMENT BREACH\nHOSTILES INBOUND / ZONE B";
            warning.enabled = true;
            Transform tracker = encounter.transform.Find("Wave Tracker");
            tracker.GetComponent<UnityEngine.UI.Image>().enabled = true;
            TMP_Text wave = tracker.Find("Wave Progress").GetComponent<TMP_Text>();
            wave.text = "ZONE B / WAVE 02\n04 HOSTILES REMAINING";
            wave.enabled = true;

            string output = Path.Combine(Application.dataPath, "../docs/m10-combat-review");
            Directory.CreateDirectory(output);
            Render(camera, Path.Combine(output, "combat-hud-editor-1600x900.png"), 1600, 900);
            Debug.Log("M10 combat HUD Editor preview written. Scenario labels are representative.");
        }

        private static void SetupCanvas(Canvas canvas, Camera camera, int order)
        {
            canvas.enabled = true;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 0.05f;
            canvas.sortingOrder = order;
        }

        private static void SetText(Transform root, string path, string value)
        {
            Transform item = root.Find(path);
            if (item == null) throw new System.InvalidOperationException("Missing UI: " + path);
            item.GetComponent<TMP_Text>().text = value;
        }

        private static void Invoke(object target, string name, params object[] args)
        {
            MethodInfo method = target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) throw new System.InvalidOperationException("Missing method: " + name);
            method.Invoke(target, args);
        }

        private static void Render(Camera camera, string path, int width, int height)
        {
            RenderTexture target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            target.Create();
            camera.targetTexture = target;
            camera.aspect = (float)width / height;
            foreach (TextMeshProUGUI label in Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include))
            {
                label.SetAllDirty();
                label.ForceMeshUpdate(true, true);
            }
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture old = RenderTexture.active;
            RenderTexture.active = target;
            Texture2D image = new Texture2D(width, height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            image.Apply();
            File.WriteAllBytes(path, image.EncodeToPNG());
            RenderTexture.active = old;
            camera.targetTexture = null;
            Object.DestroyImmediate(image);
            target.Release();
            Object.DestroyImmediate(target);
        }
    }
}

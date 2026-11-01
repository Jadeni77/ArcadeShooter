using System;
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
    // Representative Edit-mode captures of the real runtime builders. No scene is saved here.
    public static class M10StageEReviewCapture
    {
        [MenuItem("ArcadeShooter/M10/Capture Stage E Review")]
        public static void Capture()
        {
            string directory = Path.Combine(Application.dataPath, "../docs/m10-stage-e-review");
            Directory.CreateDirectory(directory);
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/FirstPersonTest.unity", OpenSceneMode.Single);
            GameObject review = new GameObject("Temporary Stage E Review UI");
            try
            {
                MissionHud hud = review.AddComponent<MissionHud>();
                Invoke(hud, "BuildUi");
                MissionPauseMenu pause = review.AddComponent<MissionPauseMenu>();
                Invoke(pause, "Build");
                Transform missionCanvas = review.transform.Find("Mission UI");
                Transform pauseCanvas = review.transform.Find("Pause UI");
                Transform results = missionCanvas.Find("Mission Results");
                Transform overlay = pauseCanvas.Find("Pause Overlay");
                missionCanvas.Find("Mission Objectives").gameObject.SetActive(false);
                missionCanvas.Find("Research Sample Prompt").gameObject.SetActive(false);
                results.gameObject.SetActive(true);
                overlay.gameObject.SetActive(false);
                Set(results, "Results Content/Outcome", "OPERATION COMPLETE");
                Set(results, "Results Content/Mission", "CONTAINMENT BREACH   •   HARD");
                Set(results, "Results Content/Outcome Reason", "Extracted: 4 of 4 players made it out.");
                Set(results, "Results Content/Team Summary/DURATION/Value", "04:38");
                Set(results, "Results Content/Team Summary/TEAM SCORE/Value", "4,320");
                Set(results, "Results Content/Team Summary/MISSION RANK/Value", "S");
                Set(results, "Results Content/Score Breakdown", "Combat 1,240   +   Objectives 1,500   +   Time 640   +   Accuracy 540   +   Survival 400");
                Set(results, "Results Content/XP and Unlocks", "+950 XP   LEVEL 4   /   NEW: Pathfinder title");
                results.Find("Results Content/XP Track/XP Progress").GetComponent<UnityEngine.UI.Image>().fillAmount = 0.72f;
                string[] rows = { "Player 1 (you)", "Player 2", "Player 3", "Player 4" };
                for (int i = 0; i < rows.Length; i++)
                {
                    Transform row = results.Find("Results Content/Player Statistics " + (i + 1));
                    Set(row, "PLAYER", rows[i]);
                    Set(row, "KILLS", (8 - i).ToString());
                    Set(row, "SHOTS", (93 - i * 13).ToString());
                    Set(row, "HITS", (64 - i * 8).ToString());
                    Set(row, "ACCURACY", "68.8%");
                    Set(row, "OBJECTIVES", i == 0 ? "1" : "0");
                    Set(row, "SCORE", (1150 - i * 110).ToString("N0"));
                }
                Transform team = results.Find("Results Content/Team Statistics");
                Set(team, "KILLS", "26"); Set(team, "SHOTS", "294");
                Set(team, "HITS", "203"); Set(team, "ACCURACY", "69.0%");
                Set(team, "OBJECTIVES", "3 / 3"); Set(team, "SCORE", "4,320");
                Set(results, "Results Content/Session Guidance", "Replay together, or return the session to the lobby.");
                Camera camera = CreateCamera();
                CaptureCanvases(new[] { missionCanvas.GetComponent<Canvas>(), pauseCanvas.GetComponent<Canvas>() },
                    camera, Path.Combine(directory, "results-1600x900.png"), 1600, 900);
                CaptureCanvases(new[] { missionCanvas.GetComponent<Canvas>(), pauseCanvas.GetComponent<Canvas>() },
                    camera, Path.Combine(directory, "results-1280x720.png"), 1280, 720);
                CaptureCanvases(new[] { missionCanvas.GetComponent<Canvas>(), pauseCanvas.GetComponent<Canvas>() },
                    camera, Path.Combine(directory, "results-2560x1600.png"), 2560, 1600);
                results.gameObject.SetActive(false);
                overlay.gameObject.SetActive(true);
                CaptureCanvases(new[] { missionCanvas.GetComponent<Canvas>(), pauseCanvas.GetComponent<Canvas>() },
                    camera, Path.Combine(directory, "pause-1600x900.png"), 1600, 900);
                CaptureCanvases(new[] { missionCanvas.GetComponent<Canvas>(), pauseCanvas.GetComponent<Canvas>() },
                    camera, Path.Combine(directory, "pause-1280x720.png"), 1280, 720);
                overlay.Find("Pause Actions").gameObject.SetActive(false);
                overlay.Find("Settings Page").gameObject.SetActive(true);
                CaptureCanvases(new[] { missionCanvas.GetComponent<Canvas>(), pauseCanvas.GetComponent<Canvas>() },
                    camera, Path.Combine(directory, "settings-1600x900.png"), 1600, 900);
                UnityEngine.Object.DestroyImmediate(camera.gameObject);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(review);
                EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
            }
            CaptureMenuSettings(directory);
            Debug.Log("M10 Stage E review captures saved to docs/m10-stage-e-review.");
        }

        private static void CaptureMenuSettings(string directory)
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
            try
            {
                GameObject system = GameObject.Find("Multiplayer System");
                Transform canvasRoot = system.transform.Find("Multiplayer UI");
                Transform main = canvasRoot.Find("Main Menu");
                Transform settings = main.Find("Settings Page");
                system.AddComponent<MenuDisplaySettings>().Initialize(settings.gameObject);
                main.Find("Landing Page").gameObject.SetActive(false);
                main.Find("Multiplayer Page").gameObject.SetActive(false);
                main.Find("Profile Page").gameObject.SetActive(false);
                settings.gameObject.SetActive(true);
                canvasRoot.Find("Lobby").gameObject.SetActive(false);
                canvasRoot.Find("Gameplay Controls").gameObject.SetActive(false);
                canvasRoot.Find("Status").gameObject.SetActive(false);
                Camera camera = system.transform.Find("Menu Camera").GetComponent<Camera>();
                CaptureCanvases(new[] { canvasRoot.GetComponent<Canvas>() }, camera,
                    Path.Combine(directory, "menu-settings-1600x900.png"), 1600, 900);
            }
            finally
            {
                EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
            }
        }

        private static void Invoke(object component, string method) =>
            component.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(component, null);

        private static void Set(Transform parent, string path, string value)
        {
            Transform node = parent.Find(path) ?? throw new InvalidOperationException("Missing review UI: " + path);
            node.GetComponent<TMP_Text>().text = value;
        }

        private static Camera CreateCamera()
        {
            GameObject root = new GameObject("Temporary Review Camera", typeof(Camera));
            Camera camera = root.GetComponent<Camera>();
            camera.transform.position = new Vector3(-12f, 2.2f, -2.8f);
            camera.transform.rotation = Quaternion.Euler(0, 30, 0);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.02f, 0.06f, 0.08f);
            camera.fieldOfView = 70;
            return camera;
        }

        private static void CaptureCanvases(Canvas[] canvases, Camera camera, string path, int width, int height)
        {
            RenderTexture target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            target.Create();
            camera.targetTexture = target;
            foreach (Canvas canvas in canvases)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 1;
            }
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            Texture2D png = new Texture2D(width, height, TextureFormat.RGB24, false);
            png.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            png.Apply();
            File.WriteAllBytes(path, png.EncodeToPNG());
            RenderTexture.active = previous;
            camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(png);
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
        }
    }
}

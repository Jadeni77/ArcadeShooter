using System;
using System.IO;
using System.Reflection;
using ArcadeShooter.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ArcadeShooter.Editor
{
    // Temporary capture helper for the pre-M10 d03d873 checkout. It is not committed.
    public static class M10BaselineCapture
    {
        public static void Capture()
        {
            string output = Path.Combine(Application.dataPath, "../docs/m10-baseline");
            Directory.CreateDirectory(output);
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
            GameObject system = GameObject.Find("Multiplayer System");
            Transform ui = system.transform.Find("Multiplayer UI");
            ui.Find("Lobby").gameObject.SetActive(false);
            ui.Find("Gameplay Controls").gameObject.SetActive(false);
            ui.Find("Main Menu").gameObject.SetActive(true);
            Camera menuCamera = system.transform.Find("Menu Camera").GetComponent<Camera>();
            CaptureCanvas(ui.GetComponent<Canvas>(), menuCamera, Path.Combine(output, "main-menu-before-1600x900.png"));

            EditorSceneManager.OpenScene("Assets/Scenes/FirstPersonTest.unity");
            GameObject review = new GameObject("Temporary Pre-M10 Results");
            try
            {
                MissionHud hud = review.AddComponent<MissionHud>();
                typeof(MissionHud).GetMethod("BuildUi", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(hud, null);
                Transform canvas = review.transform.Find("Mission UI");
                canvas.Find("Mission Objectives").gameObject.SetActive(false);
                canvas.Find("Research Sample Prompt").gameObject.SetActive(false);
                Transform results = canvas.Find("Mission Results");
                results.gameObject.SetActive(true);
                Set(results, "Results Content/Outcome", "MISSION SUCCESS");
                Set(results, "Results Content/Mission", "Containment Breach   •   Hard");
                Set(results, "Results Content/Outcome Reason", "Extracted: 4 of 4 players made it out.");
                Set(results, "Results Content/Team Summary/DURATION/Value", "04:38");
                Set(results, "Results Content/Team Summary/TEAM SCORE/Value", "4,320");
                Set(results, "Results Content/Team Summary/MISSION RANK/Value", "S");
                Set(results, "Results Content/Score Breakdown",
                    "Combat 1,240   +   Objectives 1,500   +   Time 640   +   Accuracy 540   +   Survival 400");
                Set(results, "Results Content/XP and Unlocks", "+950 XP   Level 4   NEW: Pathfinder title");
                for (int i = 0; i < 4; i++)
                {
                    Transform row = results.Find("Results Content/Player Statistics " + (i + 1));
                    row.gameObject.SetActive(true);
                    Set(row, "PLAYER", "Player " + (i + 1));
                    Set(row, "KILLS", (8 - i).ToString());
                    Set(row, "SHOTS", (93 - i * 13).ToString());
                    Set(row, "HITS", (64 - i * 8).ToString());
                    Set(row, "ACCURACY", "68.8%");
                    Set(row, "OBJECTIVES", i == 0 ? "1" : "0");
                    Set(row, "SCORE", (1150 - i * 110).ToString("N0"));
                }
                Transform team = results.Find("Results Content/Team Statistics");
                team.gameObject.SetActive(true);
                Set(team, "KILLS", "26"); Set(team, "SHOTS", "294");
                Set(team, "HITS", "203"); Set(team, "ACCURACY", "69.0%");
                Set(team, "OBJECTIVES", "3 / 3"); Set(team, "SCORE", "4,320");
                Set(results, "Results Content/Session Guidance", "Replay together, or return the session to the lobby.");
                GameObject cameraRoot = new GameObject("Temporary Camera", typeof(Camera));
                Camera camera = cameraRoot.GetComponent<Camera>();
                camera.transform.position = new Vector3(-12f, 2.2f, -2.8f);
                camera.transform.rotation = Quaternion.Euler(0, 30, 0);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.02f, 0.06f, 0.08f);
                camera.fieldOfView = 70;
                CaptureCanvas(canvas.GetComponent<Canvas>(), camera,
                    Path.Combine(output, "results-before-1600x900.png"));
                UnityEngine.Object.DestroyImmediate(cameraRoot);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(review);
            }
            Debug.Log("Pre-M10 baseline captures saved.");
        }

        private static void Set(Transform parent, string path, string text)
        {
            Transform node = parent.Find(path) ?? throw new InvalidOperationException("Missing " + path);
            node.GetComponent<TMP_Text>().text = text;
        }

        private static void CaptureCanvas(Canvas canvas, Camera camera, string path)
        {
            const int width = 1600, height = 900;
            RenderTexture target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            target.Create();
            camera.targetTexture = target;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1;
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

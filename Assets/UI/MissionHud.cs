using System;
using ArcadeShooter.Missions;
using ArcadeShooter.Networking;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace ArcadeShooter.UI
{
    /// <summary>Presentation only: every mission and result value comes from the host's replicated state.</summary>
    [DisallowMultipleComponent]
    public sealed class MissionHud : MonoBehaviour
    {
        private static readonly Color PanelColor = new Color(0.035f, 0.065f, 0.095f, 0.94f);
        private static readonly Color MutedColor = new Color(0.72f, 0.8f, 0.84f);
        private static readonly Color SuccessColor = new Color(0.43f, 0.95f, 0.76f);
        private static readonly Color FailureColor = new Color(1f, 0.52f, 0.48f);
        private static readonly string[] Columns = { "PLAYER", "KILLS", "SHOTS", "HITS", "ACCURACY", "OBJECTIVES", "SCORE" };
        private static readonly float[] ColumnWidths = { 0.24f, 0.09f, 0.10f, 0.09f, 0.15f, 0.15f, 0.18f };

        private MissionController mission;
        private MissionScoreTracker score;
        private GameObject hudPanel;
        private GameObject promptPanel;
        private GameObject resultsPanel;
        private TextMeshProUGUI missionTitle;
        private TextMeshProUGUI missionStatus;
        private TextMeshProUGUI objectives;
        private TextMeshProUGUI extractionStatus;
        private TextMeshProUGUI resultTitle;
        private TextMeshProUGUI resultMission;
        private TextMeshProUGUI durationValue;
        private TextMeshProUGUI scoreValue;
        private TextMeshProUGUI rankValue;
        private TextMeshProUGUI resultStatus;
        private readonly GameObject[] playerRows = new GameObject[4];
        private readonly TextMeshProUGUI[,] playerCells = new TextMeshProUGUI[4, 7];
        private UnityEngine.UI.Button replayButton;
        private UnityEngine.UI.Button lobbyButton;
        private UnityEngine.UI.Button leaveButton;
        private float nextRefresh;

        private void Awake()
        {
            BuildUi();
            hudPanel.SetActive(false);
            promptPanel.SetActive(false);
            resultsPanel.SetActive(false);
        }

        private void Update()
        {
            MissionController current = MissionController.Instance;
            if (mission != current)
            {
                if (mission != null) mission.Changed -= Refresh;
                mission = current;
                score = mission != null ? mission.GetComponent<MissionScoreTracker>() : null;
                if (mission != null) mission.Changed += Refresh;
                Refresh();
            }

            // The small refresh interval also covers the replicated timer and score list.
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.1f;
            Refresh();
        }

        private void OnDestroy()
        {
            if (mission != null) mission.Changed -= Refresh;
        }

        private void Refresh()
        {
            if (hudPanel == null) return;
            bool available = mission != null && mission.IsSpawned;
            bool finished = available && mission.IsFinished;
            hudPanel.SetActive(available && !finished);
            resultsPanel.SetActive(finished);
            ResearchSample sample = ResearchSample.Instance;
            promptPanel.SetActive(available && mission.IsRunning && sample != null &&
                                  !sample.IsCollected && sample.CanLocalInteract);
            if (!available) return;
            if (finished)
            {
                RefreshResults();
                return;
            }

            missionTitle.text = mission.MissionName;
            missionStatus.text = mission.State == MissionState.Waiting
                ? "WAITING FOR THE TEAM"
                : $"OBJECTIVES {mission.CompletedObjectives} / {mission.ObjectiveCount}    {FormatDuration(mission.ElapsedSeconds)}";
            string objectiveText = string.Empty;
            for (int i = 0; i < mission.ObjectiveCount; i++)
            {
                if (i > 0) objectiveText += "\n";
                bool complete = mission.IsObjectiveComplete(i);
                objectiveText += complete ? "<color=#6EF2C2>[DONE] " : "<color=#FFFFFF>[  ] ";
                objectiveText += mission.GetObjectiveLabel(i) + "</color>";
            }
            objectives.text = objectiveText;

            bool extractionOpen = mission.State == MissionState.ObjectivesCompleted ||
                                  mission.State == MissionState.ExtractionActive;
            extractionStatus.color = extractionOpen ? SuccessColor : MutedColor;
            if (!extractionOpen)
            {
                extractionStatus.text = "Complete all objectives to unlock extraction.";
                return;
            }

            ExtractionPoint extraction = mission.Extraction;
            if (extraction == null)
            {
                extractionStatus.text = "OBJECTIVES COMPLETE\nExtraction opening...";
                return;
            }
            string occupants = $"{extraction.PlayersInside} / {extraction.LivingPlayers} living players inside";
            extractionStatus.text = extraction.CountdownRunning
                ? $"EXTRACTING  {Mathf.CeilToInt(extraction.CountdownRemaining)}s\n{occupants}\nStay inside until the countdown ends."
                : $"EXTRACTION OPEN\n{occupants}\nGather at the green extraction marker.";
        }

        private void RefreshResults()
        {
            bool succeeded = mission.State == MissionState.Success;
            resultTitle.text = succeeded ? "MISSION SUCCESS" : "MISSION FAILED";
            resultTitle.color = succeeded ? SuccessColor : FailureColor;
            resultMission.text = mission.MissionName;
            bool ready = score != null && score.ResultsReady;
            durationValue.text = ready ? FormatDuration(score.Duration) : "--:--";
            scoreValue.text = ready ? score.TeamScore.ToString("N0") : "--";
            rankValue.text = ready ? score.Rank.ToString() : "--";

            int count = ready ? Mathf.Min(score.Stats.Count, playerRows.Length) : 0;
            for (int row = 0; row < playerRows.Length; row++)
            {
                playerRows[row].SetActive(row < count);
                if (row >= count) continue;
                PlayerMissionStats stats = score.Stats[row];
                bool local = NetworkManager.Singleton != null &&
                             stats.ClientId == NetworkManager.Singleton.LocalClientId;
                playerCells[row, 0].text = $"Player {stats.ClientId + 1}" + (local ? " (you)" : "");
                playerCells[row, 0].color = local ? SuccessColor : Color.white;
                playerCells[row, 1].text = stats.Kills.ToString();
                playerCells[row, 2].text = stats.ShotsFired.ToString();
                playerCells[row, 3].text = stats.Hits.ToString();
                playerCells[row, 4].text = stats.ShotsFired > 0
                    ? (100d * stats.Hits / stats.ShotsFired).ToString("0.0") + "%" : "0.0%";
                playerCells[row, 5].text = stats.ObjectivesCompleted.ToString();
                playerCells[row, 6].text = stats.Score.ToString("N0");
            }

            MultiplayerSessionController session = MultiplayerSessionController.Instance;
            bool host = NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
            replayButton.gameObject.SetActive(host);
            lobbyButton.gameObject.SetActive(host);
            replayButton.interactable = ready && session != null && session.CanManageMission;
            lobbyButton.interactable = ready && session != null && session.CanManageMission;
            leaveButton.interactable = session != null;
            resultStatus.text = !ready ? "Receiving final results..."
                : host ? "Replay together, or return the session to the lobby."
                : "Waiting for the host to replay or return everyone to the lobby.";
        }

        private void BuildUi()
        {
            GameObject canvasObject = new GameObject("Mission UI", typeof(RectTransform), typeof(Canvas),
                typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            UnityEngine.UI.CanvasScaler scaler = canvasObject.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // The existing encounter banner occupies the top-centre band; this card starts below it.
            hudPanel = Panel("Mission Objectives", canvasObject.transform, PanelColor, false);
            RectTransform hudRect = hudPanel.GetComponent<RectTransform>();
            hudRect.anchorMin = hudRect.anchorMax = hudRect.pivot = new Vector2(0, 1);
            hudRect.anchoredPosition = new Vector2(32, -190);
            hudRect.sizeDelta = new Vector2(520, 360);
            UnityEngine.UI.VerticalLayoutGroup hudLayout = Vertical(hudPanel, 12, new RectOffset(22, 22, 18, 18));
            hudLayout.childAlignment = TextAnchor.UpperLeft;
            missionTitle = Label("Mission Name", hudPanel.transform, 29, TextAlignmentOptions.Left);
            Height(missionTitle.gameObject, 38);
            missionStatus = Label("Mission Progress", hudPanel.transform, 21, TextAlignmentOptions.Left);
            missionStatus.color = MutedColor;
            Height(missionStatus.gameObject, 28);
            objectives = Label("Objective List", hudPanel.transform, 25, TextAlignmentOptions.Left);
            objectives.lineSpacing = 12;
            Height(objectives.gameObject, 112);
            extractionStatus = Label("Extraction Status", hudPanel.transform, 23, TextAlignmentOptions.Left);
            Height(extractionStatus.gameObject, 90);

            promptPanel = Panel("Research Sample Prompt", canvasObject.transform, PanelColor, false);
            RectTransform promptRect = promptPanel.GetComponent<RectTransform>();
            promptRect.anchorMin = promptRect.anchorMax = promptRect.pivot = new Vector2(0.5f, 0);
            promptRect.anchoredPosition = new Vector2(0, 170);
            promptRect.sizeDelta = new Vector2(550, 62);
            TextMeshProUGUI prompt = Label("Interact Label", promptPanel.transform, 28, TextAlignmentOptions.Center);
            Stretch(prompt.rectTransform);
            prompt.text = "[E]  Collect research sample";

            // A raycastable backdrop covers older gameplay UI, preventing clicks through the results.
            resultsPanel = Panel("Mission Results", canvasObject.transform,
                new Color(0.025f, 0.045f, 0.065f, 0.99f), true);
            Stretch(resultsPanel.GetComponent<RectTransform>());
            GameObject content = new GameObject("Results Content", typeof(RectTransform));
            content.transform.SetParent(resultsPanel.transform, false);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.07f, 0.08f);
            contentRect.anchorMax = new Vector2(0.93f, 0.92f);
            contentRect.offsetMin = contentRect.offsetMax = Vector2.zero;
            Vertical(content, 12, new RectOffset()).childAlignment = TextAnchor.MiddleCenter;

            resultTitle = Label("Outcome", content.transform, 54, TextAlignmentOptions.Center);
            Height(resultTitle.gameObject, 68);
            resultMission = Label("Mission", content.transform, 30, TextAlignmentOptions.Center);
            resultMission.color = MutedColor;
            Height(resultMission.gameObject, 40);

            GameObject summary = new GameObject("Team Summary", typeof(RectTransform));
            summary.transform.SetParent(content.transform, false);
            Height(summary, 100);
            UnityEngine.UI.HorizontalLayoutGroup summaryLayout = Horizontal(summary, 16);
            summaryLayout.childForceExpandWidth = true;
            durationValue = SummaryCard(summary.transform, "DURATION");
            scoreValue = SummaryCard(summary.transform, "TEAM SCORE");
            rankValue = SummaryCard(summary.transform, "MISSION RANK");

            GameObject header = TableRow("Statistics Header", content.transform, 42, new Color(0.08f, 0.18f, 0.23f));
            for (int column = 0; column < Columns.Length; column++)
            {
                TextMeshProUGUI label = TableCell(header.transform, column, 21);
                label.text = Columns[column];
                label.color = MutedColor;
            }
            for (int row = 0; row < playerRows.Length; row++)
            {
                playerRows[row] = TableRow("Player Statistics " + (row + 1), content.transform, 62,
                    row % 2 == 0 ? new Color(0.06f, 0.11f, 0.15f) : new Color(0.045f, 0.085f, 0.12f));
                for (int column = 0; column < Columns.Length; column++)
                    playerCells[row, column] = TableCell(playerRows[row].transform, column, 27);
            }
            resultStatus = Label("Session Guidance", content.transform, 25, TextAlignmentOptions.Center);
            resultStatus.color = MutedColor;
            Height(resultStatus.gameObject, 48);
            GameObject actions = new GameObject("Results Actions", typeof(RectTransform));
            actions.transform.SetParent(content.transform, false);
            Height(actions, 68);
            Horizontal(actions, 20);
            replayButton = Button(actions.transform, "Replay Mission", () => MultiplayerSessionController.Instance?.ReplayMission());
            lobbyButton = Button(actions.transform, "Return to Lobby", () => MultiplayerSessionController.Instance?.ReturnToLobby());
            leaveButton = Button(actions.transform, "Leave Session", () => MultiplayerSessionController.Instance?.LeaveGame());
        }

        private static TextMeshProUGUI SummaryCard(Transform parent, string title)
        {
            GameObject card = Panel(title, parent, new Color(0.06f, 0.14f, 0.19f), false);
            UnityEngine.UI.LayoutElement size = card.AddComponent<UnityEngine.UI.LayoutElement>();
            size.flexibleWidth = 1;
            Vertical(card, 0, new RectOffset(16, 16, 8, 8));
            TextMeshProUGUI label = Label("Heading", card.transform, 20, TextAlignmentOptions.Center);
            label.text = title;
            label.color = MutedColor;
            Height(label.gameObject, 28);
            TextMeshProUGUI value = Label("Value", card.transform, 38, TextAlignmentOptions.Center);
            Height(value.gameObject, 54);
            return value;
        }

        private static GameObject TableRow(string name, Transform parent, float height, Color color)
        {
            GameObject row = Panel(name, parent, color, false);
            Height(row, height);
            return row;
        }

        private static TextMeshProUGUI TableCell(Transform parent, int column, float size)
        {
            TextMeshProUGUI label = Label(Columns[column], parent, size,
                column == 0 ? TextAlignmentOptions.Left : TextAlignmentOptions.Center);
            float start = 0;
            for (int i = 0; i < column; i++) start += ColumnWidths[i];
            label.rectTransform.anchorMin = new Vector2(start, 0);
            label.rectTransform.anchorMax = new Vector2(start + ColumnWidths[column], 1);
            label.rectTransform.offsetMin = new Vector2(16, 4);
            label.rectTransform.offsetMax = new Vector2(-16, -4);
            return label;
        }

        private static UnityEngine.UI.Button Button(Transform parent, string title, UnityEngine.Events.UnityAction action)
        {
            GameObject root = Panel(title, parent, new Color(0.08f, 0.45f, 0.56f), true);
            UnityEngine.UI.LayoutElement size = root.AddComponent<UnityEngine.UI.LayoutElement>();
            size.preferredWidth = 340;
            size.minWidth = 260;
            UnityEngine.UI.Button button = root.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = root.GetComponent<UnityEngine.UI.Image>();
            UnityEngine.UI.ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.8f, 1f, 1f);
            colors.pressedColor = new Color(0.65f, 0.85f, 0.9f);
            colors.disabledColor = new Color(0.45f, 0.55f, 0.6f);
            button.colors = colors;
            button.onClick.AddListener(action);
            TextMeshProUGUI label = Label("Label", root.transform, 28, TextAlignmentOptions.Center);
            Stretch(label.rectTransform);
            label.text = title;
            return button;
        }

        private static GameObject Panel(string name, Transform parent, Color color, bool raycast)
        {
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            root.transform.SetParent(parent, false);
            UnityEngine.UI.Image image = root.GetComponent<UnityEngine.UI.Image>();
            image.color = color;
            image.raycastTarget = raycast;
            return root;
        }

        private static TextMeshProUGUI Label(string name, Transform parent, float size, TextAlignmentOptions alignment)
        {
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            root.transform.SetParent(parent, false);
            TextMeshProUGUI label = root.GetComponent<TextMeshProUGUI>();
            label.font = TMP_Settings.defaultFontAsset;
            label.fontSize = size;
            label.alignment = alignment;
            label.color = Color.white;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = TextOverflowModes.Ellipsis;
            return label;
        }

        private static UnityEngine.UI.VerticalLayoutGroup Vertical(GameObject root, int spacing, RectOffset padding)
        {
            UnityEngine.UI.VerticalLayoutGroup layout = root.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return layout;
        }

        private static UnityEngine.UI.HorizontalLayoutGroup Horizontal(GameObject root, int spacing)
        {
            UnityEngine.UI.HorizontalLayoutGroup layout = root.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
            return layout;
        }

        private static void Height(GameObject root, float height)
        {
            UnityEngine.UI.LayoutElement layout = root.AddComponent<UnityEngine.UI.LayoutElement>();
            layout.minHeight = layout.preferredHeight = height;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static string FormatDuration(double seconds)
        {
            TimeSpan duration = TimeSpan.FromSeconds(Math.Max(0, seconds));
            return $"{(int)duration.TotalMinutes:00}:{duration.Seconds:00}";
        }
    }
}

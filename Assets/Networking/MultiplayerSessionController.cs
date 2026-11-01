using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArcadeShooter.Missions;
using ArcadeShooter.Progression;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ArcadeShooter.Networking
{
    // Let NetworkManager initialize its cleanup dependencies, then discard a reloaded
    // menu's duplicate before its camera or UI can render. NGO keeps the existing singleton.
    [DefaultExecutionOrder(10000)]
    [RequireComponent(typeof(NetworkManager))]
    public sealed class MultiplayerSessionController : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject lobbyPanel;
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private TMP_InputField joinCodeInput;
        [SerializeField] private TMP_Text joinCodeText;
        [SerializeField] private TMP_Text playersText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button startButton;
        [SerializeField] private Button lobbyLeaveButton;
        [SerializeField] private Button gameplayLeaveButton;
        [SerializeField] private Camera menuCamera;
        [SerializeField] private RectTransform lobbyExtrasRoot;
        [SerializeField] private ProgressionRules progressionRules = new ProgressionRules();

        private NetworkManager network;
        private NetworkSceneManager registeredScenes;
        private ISession session;
        private bool busy;
        private bool leaving;
        private bool matchStarted;
        private bool returningToMenu;
        private bool difficultySaved = true;
        private bool readySaving;
        private string status = "Host a game or enter a join code.";
        private float nextRefresh;
        private static string menuReturnStatus;
        private const string GameplayScene = "FirstPersonTest";
        private const string MenuScene = "MainMenu";
        private const string DifficultyProperty = "difficulty";
        private const string DisplayNameProperty = "displayName";
        private const string ReadyProperty = "ready";
        private const string FinishProperty = "rifleFinish";
        private const string TitleProperty = "playerTitle";
        private const string SavedDisplayNameKey = "ArcadeShooter.DisplayName";
        private TMP_Text progressionText;
        private Button finishButton;
        private Button titleButton;
        private Button difficultyButton;
        public LocalProfile Profile { get; private set; }
        public ProgressionRules ProgressionRules => progressionRules ??= new ProgressionRules();
        public int LastEarnedXp { get; private set; }
        public int LastOldLevel { get; private set; }
        public int LastNewLevel { get; private set; }
        public bool HasLocalReward { get; private set; }
        public MissionDifficulty SelectedDifficulty => session != null &&
            session.Properties.TryGetValue(DifficultyProperty, out SessionProperty property) &&
            property.Value == "Hard" ? MissionDifficulty.Hard : MissionDifficulty.Normal;
        // Explains the cursor without prompting players to leave; Leave Game stays in the corner.
        private const string MatchStatus = "Press Esc to release the cursor; click the map to aim again.";
        // Prefixes every reason the host gives when it removes a client, so that client can tell
        // "the host removed me" apart from "the host's session ended".
        private const string RemovedByHostPrefix = "Removed by host: ";

        public static MultiplayerSessionController Instance { get; private set; }
        public bool CanManageMission => !busy && !leaving && session != null && session.IsHost &&
                                        network != null && network.IsHost;
        public bool IsBusy => busy;
        public string Status => status;
        // The last replay or lobby-return failure, shown on the mission results screen.
        public string LastActionError { get; private set; }
        public event Action UiChanged;
        public bool HasSession => session != null;
        public bool IsSessionHost => session != null && session.IsHost;
        public int SessionPlayerCount => session?.PlayerCount ?? 0;
        public int SessionCapacity => session?.MaxPlayers ?? 4;
        public string SessionName => session?.Name ?? string.Empty;
        public string JoinCode => session?.Code ?? string.Empty;
        public IReadOnlyList<IReadOnlyPlayer> SessionPlayers => session?.Players;
        public string HostPlayerId => session?.Host ?? string.Empty;
        public string DisplayName => PlayerPrefs.GetString(SavedDisplayNameKey, "OPERATOR");
        public bool LocalReady => session?.CurrentPlayer != null && IsReady(session.CurrentPlayer);
        public bool CanToggleReady => session != null && !busy && !readySaving && !matchStarted;
        public bool AllPlayersReady => session != null && session.PlayerCount > 0 && session.Players.All(IsReady);
        public bool CanStartMission => CanManageMission && !matchStarted && difficultySaved && AllPlayersReady;

        public static bool IsReady(IReadOnlyPlayer player) =>
            player != null && player.Properties != null &&
            player.Properties.TryGetValue(ReadyProperty, out PlayerProperty property) && property.Value == "1";

        public static string PlayerDisplayName(IReadOnlyPlayer player, int index)
        {
            if (player?.Properties != null &&
                player.Properties.TryGetValue(DisplayNameProperty, out PlayerProperty property) &&
                !string.IsNullOrWhiteSpace(property.Value))
                return property.Value;
            return "OPERATOR " + (index + 1).ToString("00");
        }

        public static string PlayerFinish(IReadOnlyPlayer player) =>
            player?.Properties != null && player.Properties.TryGetValue(FinishProperty, out PlayerProperty property)
                ? property.Value : "STANDARD";

        public void SaveDisplayName(string value)
        {
            value = new string((value ?? string.Empty).Trim().Take(16)
                .Where(character => char.IsLetterOrDigit(character) || character == ' ' || character == '_')
                .ToArray()).Trim();
            if (value.Length == 0) value = "OPERATOR";
            PlayerPrefs.SetString(SavedDisplayNameKey, value);
            PlayerPrefs.Save();
            if (session != null) _ = PublishLocalPlayerDataAsync();
            UiChanged?.Invoke();
        }

        public void CopyJoinCode()
        {
            if (session == null || string.IsNullOrEmpty(session.Code)) return;
            GUIUtility.systemCopyBuffer = session.Code;
            status = "Join code copied.";
            Refresh();
        }

        public void ToggleReady()
        {
            if (!CanToggleReady) return;
            _ = SetReadyAsync(!LocalReady);
        }

        private async Task SetReadyAsync(bool ready)
        {
            ISession current = session;
            if (current?.CurrentPlayer == null) return;
            bool previous = IsReady(current.CurrentPlayer);
            readySaving = true;
            try
            {
                current.CurrentPlayer.SetProperty(ReadyProperty, new PlayerProperty(ready ? "1" : "0"));
                await current.SaveCurrentPlayerDataAsync();
                if (session == current) status = ready ? "Ready for deployment." : "Ready state cleared.";
            }
            catch (Exception exception)
            {
                current.CurrentPlayer.SetProperty(ReadyProperty, new PlayerProperty(previous ? "1" : "0"));
                if (session == current) status = "Could not update ready state: " + exception.Message;
                Debug.LogWarning(exception);
            }
            finally { readySaving = false; Refresh(); }
        }

        private async Task PublishLocalPlayerDataAsync()
        {
            ISession current = session;
            if (current?.CurrentPlayer == null) return;
            try
            {
                current.CurrentPlayer.SetProperty(DisplayNameProperty, new PlayerProperty(DisplayName));
                current.CurrentPlayer.SetProperty(FinishProperty, new PlayerProperty(Profile.Data.selectedFinish.ToString()));
                current.CurrentPlayer.SetProperty(TitleProperty, new PlayerProperty(Profile.Data.selectedTitle.ToString()));
                if (!current.CurrentPlayer.Properties.ContainsKey(ReadyProperty))
                    current.CurrentPlayer.SetProperty(ReadyProperty, new PlayerProperty("0"));
                await current.SaveCurrentPlayerDataAsync();
            }
            catch (Exception exception)
            {
                if (session == current) status = "Connected, but profile details did not sync: " + exception.Message;
                Debug.LogWarning(exception);
            }
            finally { Refresh(); }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Profile = new LocalProfile(Application.persistentDataPath, ProgressionRules);
            DontDestroyOnLoad(gameObject);
            network = GetComponent<NetworkManager>();
            network.ConnectionApprovalCallback = ApproveConnection;
            network.OnClientDisconnectCallback += OnDisconnected;
            hostButton.onClick.AddListener(() => _ = HostAsync());
            joinButton.onClick.AddListener(() => _ = JoinAsync());
            startButton.onClick.AddListener(() => _ = LoadSessionSceneAsync(GameplayScene));
            lobbyLeaveButton.onClick.AddListener(LeaveGame);
            gameplayLeaveButton.onClick.AddListener(LeaveGame);
            BuildProgressionUi();
            SceneManager.sceneLoaded += OnSceneLoaded;
            if (!string.IsNullOrEmpty(menuReturnStatus))
            {
                status = menuReturnStatus;
                menuReturnStatus = null;
            }
            Refresh();
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            UnsubscribeSceneEvents();
            if (network != null)
            {
                network.OnClientDisconnectCallback -= OnDisconnected;
                network.ConnectionApprovalCallback = null;
            }
            UnsubscribeSession();
        }

        private void Update()
        {
            SubscribeSceneEvents();
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.25f;
            Refresh();
        }

        public void ReplayMission()
        {
            if (CanManageMission && MissionController.Instance != null && MissionController.Instance.IsFinished)
                _ = LoadSessionSceneAsync(GameplayScene);
        }

        public void ReturnToLobby()
        {
            if (CanManageMission && MissionController.Instance != null && MissionController.Instance.IsFinished)
                _ = LoadSessionSceneAsync(MenuScene);
        }

        public void LeaveGame() => _ = LeaveAsync();

        public void GrantLocalResult(string resultId, int earnedXp, bool extracted)
        {
            if (Profile == null || !Profile.Claim(resultId, earnedXp, extracted, out int before, out int after)) return;
            LastEarnedXp = earnedXp;
            LastOldLevel = before;
            LastNewLevel = after;
            HasLocalReward = true;
            Refresh();
        }

        private async Task SetDifficultyAsync()
        {
            if (!CanManageMission || matchStarted) return;
            busy = true;
            MissionDifficulty previous = SelectedDifficulty;
            try
            {
                IHostSession host = session.AsHost();
                host.SetProperty(DifficultyProperty, new SessionProperty(
                    previous == MissionDifficulty.Normal ? "Hard" : "Normal"));
                await host.SavePropertiesAsync();
                difficultySaved = true;
            }
            catch (Exception exception)
            {
                if (session != null && session.IsHost)
                    try { session.AsHost().SetProperty(DifficultyProperty, new SessionProperty(previous.ToString())); }
                    catch (Exception rollbackException) { Debug.LogWarning("Difficulty rollback: " + rollbackException.Message); }
                difficultySaved = false;
                status = "Could not set difficulty: " + exception.Message;
                Debug.LogWarning(exception);
            }
            finally { busy = false; Refresh(); }
        }

        private void BuildProgressionUi()
        {
            if (lobbyPanel == null || startButton == null) return;
            GameObject labelObject = new GameObject("Player Progression", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(lobbyExtrasRoot != null ? lobbyExtrasRoot : lobbyPanel.transform, false);
            progressionText = labelObject.GetComponent<TextMeshProUGUI>();
            progressionText.font = playersText.font;
            progressionText.fontSize = 23;
            progressionText.alignment = TextAlignmentOptions.Center;
            progressionText.color = new Color(0.72f, 0.9f, 0.92f);
            progressionText.raycastTarget = false;
            RectTransform labelRect = progressionText.rectTransform;
            labelRect.anchorMin = labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = lobbyExtrasRoot != null ? new Vector2(0, 120) : new Vector2(590, 100);
            labelRect.sizeDelta = new Vector2(580, 130);
            finishButton = ProgressionButton("Rifle Finish", lobbyExtrasRoot != null ? 0 : 590, lobbyExtrasRoot != null ? 20 : 10, () =>
            {
                RifleFinish next = Profile.Data.selectedFinish == RifleFinish.Standard ? RifleFinish.Teal : RifleFinish.Standard;
                if (Profile.SelectFinish(next)) { _ = PublishLocalPlayerDataAsync(); Refresh(); }
            });
            titleButton = ProgressionButton("Player Title", lobbyExtrasRoot != null ? 0 : 590, lobbyExtrasRoot != null ? -65 : -75, () =>
            {
                PlayerTitle next = Profile.Data.selectedTitle == PlayerTitle.Recruit ? PlayerTitle.Pathfinder : PlayerTitle.Recruit;
                if (Profile.SelectTitle(next)) { _ = PublishLocalPlayerDataAsync(); Refresh(); }
            });
            difficultyButton = ProgressionButton("Difficulty", lobbyExtrasRoot != null ? 0 : 590, lobbyExtrasRoot != null ? -150 : -160, () => _ = SetDifficultyAsync());
        }

        private Button ProgressionButton(string name, float x, float y, UnityEngine.Events.UnityAction action)
        {
            Button button = Instantiate(startButton, lobbyExtrasRoot != null ? lobbyExtrasRoot : lobbyPanel.transform);
            button.name = name;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(450, 66);
            return button;
        }

        private async Task InitializeAsync()
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                string profile = "arcade" + System.Diagnostics.Process.GetCurrentProcess().Id;
                await UnityServices.InitializeAsync(new InitializationOptions().SetProfile(profile));
            }
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        private async Task HostAsync()
        {
            if (busy || session != null) return;
            busy = true;
            status = "Creating game...";
            Refresh();
            try
            {
                await InitializeAsync();
                session = await MultiplayerService.Instance.CreateSessionAsync(
                    new SessionOptions { MaxPlayers = 4, Name = "ArcadeShooter", IsPrivate = true,
                        SessionProperties = new Dictionary<string, SessionProperty>
                        { [DifficultyProperty] = new SessionProperty("Normal") } }
                        .WithRelayNetwork());
                SubscribeSession();
                difficultySaved = true;
                status = "Lobby ready. Share the join code.";
                await PublishLocalPlayerDataAsync();
            }
            catch (Exception exception)
            {
                status = "Could not host: " + exception.Message;
                Debug.LogWarning(exception);
            }
            finally { busy = false; Refresh(); }
        }

        private async Task JoinAsync()
        {
            if (busy || session != null) return;
            string code = joinCodeInput.text.Trim().ToUpperInvariant();
            if (code.Length == 0) { status = "Enter a join code."; Refresh(); return; }
            busy = true;
            status = "Joining game...";
            Refresh();
            try
            {
                await InitializeAsync();
                session = await MultiplayerService.Instance.JoinSessionByCodeAsync(code);
                SubscribeSession();
                status = "Connected. Waiting for host to start.";
                await PublishLocalPlayerDataAsync();
            }
            catch (Exception exception)
            {
                status = "Could not join: " + exception.Message;
                Debug.LogWarning(exception);
            }
            finally { busy = false; Refresh(); }
        }

        private async Task LoadSessionSceneAsync(string sceneName)
        {
            if (!CanManageMission) return;
            if (sceneName == GameplayScene && SceneManager.GetActiveScene().name == MenuScene && !CanStartMission)
                return;
            if (matchStarted && (MissionController.Instance == null || !MissionController.Instance.IsFinished)) return;
            busy = true;
            matchStarted = true; // Also rejects new connections during the lobby lock request.
            LastActionError = null;
            status = sceneName == GameplayScene ? "Loading mission..." : "Returning to lobby...";
            bool loadStarted = false;
            Refresh();
            try
            {
                IHostSession host = session.AsHost();
                host.IsLocked = true;
                await host.SavePropertiesAsync();
                if (leaving || session == null || !network.IsHost) return;
                SubscribeSceneEvents();
                // A single-mode network load despawns every avatar and wave enemy (all spawned to be
                // destroyed with the scene) only once it has started, so a refused load keeps the
                // current match intact. Fresh avatars reset health, ammo, input, camera, and HUD.
                SceneEventProgressStatus result = network.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
                if (result != SceneEventProgressStatus.Started)
                    throw new InvalidOperationException("Network scene load failed: " + result);
                loadStarted = true;
            }
            catch (Exception exception)
            {
                matchStarted = SceneManager.GetActiveScene().name == GameplayScene;
                if (!matchStarted) await UnlockLobbyAsync();
                status = "Could not load scene: " + exception.Message;
                LastActionError = status;
                Debug.LogError(exception);
            }
            finally
            {
                // Keep the buttons locked until every remaining client has finished the scene load.
                if (!loadStarted) busy = false;
                Refresh();
            }
        }

        private void SubscribeSceneEvents()
        {
            if (network == null || !network.IsListening || network.SceneManager == registeredScenes) return;
            UnsubscribeSceneEvents();
            registeredScenes = network.SceneManager;
            if (registeredScenes == null) return;
            registeredScenes.OnLoad += OnNetworkLoad;
            registeredScenes.OnLoadEventCompleted += OnLoadComplete;
        }

        private void UnsubscribeSceneEvents()
        {
            if (registeredScenes == null) return;
            registeredScenes.OnLoad -= OnNetworkLoad;
            registeredScenes.OnLoadEventCompleted -= OnLoadComplete;
            registeredScenes = null;
        }

        private void OnNetworkLoad(ulong clientId, string sceneName, LoadSceneMode mode, AsyncOperation operation)
        {
            if (sceneName != GameplayScene && sceneName != MenuScene) return;
            busy = true;
            if (sceneName == GameplayScene) HasLocalReward = false;
            matchStarted = true;
            status = sceneName == GameplayScene ? "Loading mission..." : "Returning to lobby...";
            Refresh();
        }

        private void OnLoadComplete(string sceneName, LoadSceneMode mode,
            List<ulong> completed, List<ulong> timedOut)
        {
            if (session == null || (sceneName != GameplayScene && sceneName != MenuScene)) return;
            if (network.IsServer)
            {
                foreach (ulong id in timedOut)
                    if (id != network.LocalClientId && network.ConnectedClients.ContainsKey(id))
                        network.DisconnectClient(id, RemovedByHostPrefix + "scene loading timed out.");
                if (!completed.Contains(network.LocalClientId))
                {
                    _ = EndFailedLoadAsync("The host could not finish loading the scene.");
                    return;
                }
            }
            if (sceneName == MenuScene)
            {
                status = "Lobby ready. The host can start another mission.";
                if (network.IsServer) _ = UnlockLobbyAsync();
                else { matchStarted = false; busy = false; }
                Refresh();
                return;
            }
            if (network.IsServer)
            {
                try
                {
                    if (MissionController.Instance == null)
                        throw new InvalidOperationException("The map has no mission controller.");
                    SpawnMissionPlayers(completed, timedOut);
                    MissionController.Instance.ConfigureMission(SelectedDifficulty);
                    MissionController.Instance.BeginMission();
                }
                catch (Exception exception)
                {
                    Debug.LogError(exception);
                    _ = EndFailedLoadAsync("Could not start mission: " + exception.Message);
                    return;
                }
            }
            matchStarted = true;
            busy = false;
            status = timedOut.Count == 0 ? MatchStatus : "A player could not load and was disconnected. " + MatchStatus;
            Refresh();
        }

        private void SpawnMissionPlayers(List<ulong> completed, List<ulong> timedOut)
        {
            Transform spawnRoot = GameObject.Find("Multiplayer Spawn Points")?.transform;
            var players = new List<NetworkPlayer>();
            var positions = new List<Vector3>();
            foreach (ulong id in network.ConnectedClientsIds.OrderBy(id => id).ToArray())
            {
                if (!completed.Contains(id) || timedOut.Contains(id)) continue;
                int index = players.Count;
                Vector3 position = spawnRoot != null && index < spawnRoot.childCount
                    ? spawnRoot.GetChild(index).position
                    : new Vector3(-13f + index * 2f, 1.05f, 0f);
                GameObject avatar = Instantiate(network.NetworkConfig.PlayerPrefab, position, Quaternion.identity);
                NetworkObject playerObject = avatar.GetComponent<NetworkObject>();
                playerObject.SpawnAsPlayerObject(id, true);
                players.Add(avatar.GetComponent<NetworkPlayer>());
                positions.Add(position);
            }
            // Instantiate every avatar first: disabling input on a new remote avatar must not
            // release the cursor after the owner has already entered the mission.
            for (int i = 0; i < players.Count; i++)
            {
                players[i].ShowMatchRpc();
                players[i].BeginMatchRpc(positions[i]);
            }
        }

        private async Task UnlockLobbyAsync()
        {
            try
            {
                if (session != null && session.IsHost)
                {
                    IHostSession host = session.AsHost();
                    host.IsLocked = false;
                    await host.SavePropertiesAsync();
                }
                matchStarted = false;
            }
            catch (Exception exception)
            {
                // Existing players can replay even if the service could not reopen joining.
                matchStarted = false;
                status = "Lobby ready, but new players cannot join yet: " + exception.Message;
                Debug.LogWarning(status);
            }
            finally { busy = false; Refresh(); }
        }

        private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            response.Approved = !matchStarted && network.ConnectedClientsIds.Count < 4;
            // The lobby needs no gameplay avatars. The host spawns them after scene loading.
            response.CreatePlayerObject = false;
            response.Reason = response.Approved ? string.Empty : "Game is full or already started.";
            response.Pending = false;
        }

        private async Task EndFailedLoadAsync(string message)
        {
            leaving = true;
            try
            {
                if (session != null && session.IsHost) await session.AsHost().DeleteAsync();
            }
            catch (Exception exception) { Debug.LogWarning("Session cleanup: " + exception.Message); }
            finally { ReturnToMenu(message); leaving = false; }
        }

        private async Task LeaveAsync()
        {
            if (leaving) return;
            leaving = true;
            busy = true;
            status = "Leaving game...";
            Refresh();
            try
            {
                if (session != null)
                {
                    if (session.IsHost) await session.AsHost().DeleteAsync();
                    else await session.LeaveAsync();
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Session cleanup: " + exception.Message);
            }
            finally
            {
                ReturnToMenu("Left game.");
                leaving = false;
                busy = false;
            }
        }

        private void OnDisconnected(ulong clientId)
        {
            if (leaving || session == null) return;
            if (clientId != network.LocalClientId && network.IsServer)
            {
                Refresh();
                return;
            }
            // NGO reports the host's reason when it removed this client; otherwise the session ended.
            string reason = network.DisconnectReason;
            if (!network.IsServer && reason != null && reason.StartsWith(RemovedByHostPrefix, StringComparison.Ordinal))
            {
                // The session continues without this client, so leave it to keep the lobby list accurate.
                ISession removedFrom = session;
                ReturnToMenu(reason);
                _ = LeaveQuietlyAsync(removedFrom);
                return;
            }
            ReturnToMenu("Host disconnected. Session ended.");
        }

        private static async Task LeaveQuietlyAsync(ISession removedFrom)
        {
            try { await removedFrom.LeaveAsync(); }
            catch (Exception exception) { Debug.LogWarning("Session cleanup: " + exception.Message); }
        }

        private void OnSessionRemoved()
        {
            if (!leaving) ReturnToMenu("Session ended.");
        }
        private void OnSessionChanged() => Refresh();
        private void SubscribeSession()
        {
            session.Changed += OnSessionChanged;
            session.RemovedFromSession += OnSessionRemoved;
            session.Deleted += OnSessionRemoved;
            SubscribeSceneEvents();
        }

        private void UnsubscribeSession()
        {
            if (session == null) return;
            session.Changed -= OnSessionChanged;
            session.RemovedFromSession -= OnSessionRemoved;
            session.Deleted -= OnSessionRemoved;
        }

        private void ReturnToMenu(string message)
        {
            if (returningToMenu) return;
            returningToMenu = true;
            UnsubscribeSession();
            session = null;
            matchStarted = false;
            busy = false;
            UnsubscribeSceneEvents();
            if (network.IsListening) network.Shutdown();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (SceneManager.GetActiveScene().name == MenuScene)
            {
                status = message;
                returningToMenu = false;
                Refresh();
                return;
            }
            menuReturnStatus = message;
            Destroy(gameObject);
            SceneManager.LoadSceneAsync(MenuScene);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == GameplayScene && session != null)
                status = "Waiting for all players to load...";
            if (menuCamera != null)
            {
                bool inMenu = scene.name == MenuScene;
                menuCamera.enabled = inMenu;
                menuCamera.GetComponent<AudioListener>().enabled = inMenu;
            }
            Refresh();
        }

        private void Refresh()
        {
            bool gameplay = SceneManager.GetActiveScene().name == GameplayScene && session != null;
            if (mainPanel != null) mainPanel.SetActive(session == null);
            if (lobbyPanel != null) lobbyPanel.SetActive(session != null && !gameplay);
            if (gameplayPanel != null) gameplayPanel.SetActive(gameplay);
            if (hostButton != null) hostButton.interactable = !busy;
            if (joinButton != null) joinButton.interactable = !busy;
            if (startButton != null) startButton.gameObject.SetActive(session != null && session.IsHost);
            if (startButton != null) startButton.interactable = CanStartMission;
            if (joinCodeText != null) joinCodeText.text = session == null ? string.Empty : "JOIN CODE  " + session.Code;
            if (playersText != null && session != null)
                playersText.text = "Players  " + session.PlayerCount + " / 4\n" +
                    string.Join("\n", session.Players.Select((player, index) => "Player " + (index + 1)));
            if (progressionText != null && Profile != null)
            {
                int level = Profile.Level;
                progressionText.text = $"LEVEL {level}   XP {Profile.XpIntoLevel:N0} / {Profile.XpForNextLevel:N0}   (total {Profile.Data.xp:N0})\n" +
                    $"Teal finish: {(progressionRules.FinishUnlocked(level, RifleFinish.Teal) ? "UNLOCKED" : "LOCKED (level " + progressionRules.tealFinishLevel + ")")}   " +
                    $"Pathfinder: {(progressionRules.TitleUnlocked(level, PlayerTitle.Pathfinder) ? "UNLOCKED" : "LOCKED (level " + progressionRules.pathfinderTitleLevel + ")")}" +
                    (string.IsNullOrEmpty(Profile.LoadWarning) ? "" : "\n" + Profile.LoadWarning);
                finishButton.GetComponentInChildren<TMP_Text>(true).text =
                    progressionRules.FinishUnlocked(level, RifleFinish.Teal) ? "CHANGE RIFLE FINISH  ›" : "TEAL FINISH  /  LEVEL " + progressionRules.tealFinishLevel;
                titleButton.GetComponentInChildren<TMP_Text>(true).text =
                    progressionRules.TitleUnlocked(level, PlayerTitle.Pathfinder) ? "CHANGE CALLSIGN TITLE  ›" : "PATHFINDER  /  LEVEL " + progressionRules.pathfinderTitleLevel;
                difficultyButton.GetComponentInChildren<TMP_Text>(true).text = "MISSION DIFFICULTY  /  " + SelectedDifficulty.ToString().ToUpperInvariant();
                finishButton.interactable = progressionRules.FinishUnlocked(level, RifleFinish.Teal);
                titleButton.interactable = progressionRules.TitleUnlocked(level, PlayerTitle.Pathfinder);
                difficultyButton.interactable = CanManageMission && !matchStarted;
                difficultyButton.gameObject.SetActive(session != null && session.IsHost);
            }
            if (statusText != null) statusText.text = gameplay && MissionController.Instance != null &&
                MissionController.Instance.IsFinished && !busy ? "Mission ended. The host can replay or return everyone to the lobby." : status;
            UiChanged?.Invoke();
        }
    }
}

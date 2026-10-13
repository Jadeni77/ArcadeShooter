using System;
using System.Linq;
using System.Threading.Tasks;
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

        private NetworkManager network;
        private ISession session;
        private bool busy;
        private bool leaving;
        private bool matchStarted;
        private string status = "Host a game or enter a join code.";
        private float nextRefresh;
        private static string menuReturnStatus;
        private const string MatchStatus = "Match started. Press Esc to use Leave Game; click the map to aim again.";

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            network = GetComponent<NetworkManager>();
            network.ConnectionApprovalCallback = ApproveConnection;
            network.OnClientDisconnectCallback += OnDisconnected;
            hostButton.onClick.AddListener(() => _ = HostAsync());
            joinButton.onClick.AddListener(() => _ = JoinAsync());
            startButton.onClick.AddListener(() => _ = StartMatchAsync());
            lobbyLeaveButton.onClick.AddListener(() => _ = LeaveAsync());
            gameplayLeaveButton.onClick.AddListener(() => _ = LeaveAsync());
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
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (network != null)
            {
                network.OnClientDisconnectCallback -= OnDisconnected;
                network.ConnectionApprovalCallback = null;
                if (network.SceneManager != null)
                    network.SceneManager.OnLoadEventCompleted -= OnLoadComplete;
            }
            UnsubscribeSession();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.25f;
            Refresh();
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
                    new SessionOptions { MaxPlayers = 4, Name = "ArcadeShooter", IsPrivate = true }
                        .WithRelayNetwork());
                SubscribeSession();
                status = "Lobby ready. Share the join code.";
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
            }
            catch (Exception exception)
            {
                status = "Could not join: " + exception.Message;
                Debug.LogWarning(exception);
            }
            finally { busy = false; Refresh(); }
        }

        private async Task StartMatchAsync()
        {
            if (busy || session == null || !session.IsHost || matchStarted || !network.IsHost) return;
            busy = true;
            status = "Loading map...";
            Refresh();
            try
            {
                IHostSession host = session.AsHost();
                host.IsLocked = true;
                await host.SavePropertiesAsync();
                network.SceneManager.OnLoadEventCompleted += OnLoadComplete;
                matchStarted = true;
                SceneEventProgressStatus result = network.SceneManager.LoadScene("FirstPersonTest", LoadSceneMode.Single);
                if (result != SceneEventProgressStatus.Started)
                    throw new InvalidOperationException("Network scene load failed: " + result);
            }
            catch (Exception exception)
            {
                matchStarted = false;
                if (network.SceneManager != null)
                    network.SceneManager.OnLoadEventCompleted -= OnLoadComplete;
                if (session != null && session.IsHost)
                {
                    try
                    {
                        IHostSession host = session.AsHost();
                        host.IsLocked = false;
                        await host.SavePropertiesAsync();
                    }
                    catch (Exception unlockException)
                    {
                        Debug.LogWarning("Could not reopen lobby: " + unlockException.Message);
                    }
                }
                status = "Could not start match: " + exception.Message;
                Debug.LogError(exception);
            }
            finally { busy = false; Refresh(); }
        }

        private void OnLoadComplete(string sceneName, LoadSceneMode mode,
            System.Collections.Generic.List<ulong> completed, System.Collections.Generic.List<ulong> timedOut)
        {
            if (!network.IsServer || sceneName != "FirstPersonTest") return;
            if (timedOut.Count > 0)
            {
                status = "Some players could not load the map.";
                Refresh();
            }
            Transform spawnRoot = GameObject.Find("Multiplayer Spawn Points")?.transform;
            int index = 0;
            foreach (ulong id in network.ConnectedClientsIds.OrderBy(id => id))
            {
                if (!network.ConnectedClients.TryGetValue(id, out NetworkClient client) ||
                    client.PlayerObject == null) continue;
                Vector3 position = spawnRoot != null && index < spawnRoot.childCount
                    ? spawnRoot.GetChild(index).position
                    : new Vector3(-13f + index * 2f, 1.05f, 0f);
                NetworkPlayer player = client.PlayerObject.GetComponent<NetworkPlayer>();
                player.ShowMatchRpc();
                player.BeginMatchRpc(position);
                index++;
            }
            status = MatchStatus;
            Refresh();
        }

        private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            response.Approved = !matchStarted && network.ConnectedClientsIds.Count < 4;
            response.CreatePlayerObject = response.Approved;
            response.Reason = response.Approved ? string.Empty : "Game is full or already started.";
            response.Pending = false;
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
            if (clientId == network.LocalClientId || !network.IsServer)
                ReturnToMenu("Host disconnected. Session ended.");
            else
                Refresh();
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
            if (session == null && SceneManager.GetActiveScene().name == "MainMenu") return;
            UnsubscribeSession();
            session = null;
            matchStarted = false;
            if (network.SceneManager != null)
                network.SceneManager.OnLoadEventCompleted -= OnLoadComplete;
            if (network.IsListening) network.Shutdown();
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                status = message;
                Refresh();
                return;
            }
            menuReturnStatus = message;
            Destroy(gameObject);
            SceneManager.LoadSceneAsync("MainMenu");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "FirstPersonTest" && session != null)
                status = MatchStatus;
            if (menuCamera != null)
            {
                bool inMenu = scene.name == "MainMenu";
                menuCamera.enabled = inMenu;
                menuCamera.GetComponent<AudioListener>().enabled = inMenu;
            }
            Refresh();
        }

        private void Refresh()
        {
            bool gameplay = SceneManager.GetActiveScene().name == "FirstPersonTest" && session != null;
            if (mainPanel != null) mainPanel.SetActive(session == null);
            if (lobbyPanel != null) lobbyPanel.SetActive(session != null && !gameplay);
            if (gameplayPanel != null) gameplayPanel.SetActive(gameplay);
            if (hostButton != null) hostButton.interactable = !busy;
            if (joinButton != null) joinButton.interactable = !busy;
            if (startButton != null) startButton.gameObject.SetActive(session != null && session.IsHost);
            if (startButton != null) startButton.interactable = !busy && !matchStarted;
            if (joinCodeText != null) joinCodeText.text = session == null ? string.Empty : "JOIN CODE  " + session.Code;
            if (playersText != null && session != null)
                playersText.text = "Players  " + session.PlayerCount + " / 4\n" +
                    string.Join("\n", session.Players.Select((player, index) => "Player " + (index + 1)));
            if (statusText != null) statusText.text = status;
        }
    }
}

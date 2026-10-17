using System;
using System.Collections.Generic;
using System.Linq;
using ArcadeShooter.Networking;
using ArcadeShooter.Player;
using Unity.Netcode;
using UnityEngine;

namespace ArcadeShooter.Missions
{
    public enum MissionState { Waiting, InProgress, ObjectivesCompleted, ExtractionActive, Success, Failure }

    // One scene-owned mission. Only the server can mutate progression or publish a result.
    [RequireComponent(typeof(NetworkObject), typeof(MissionScoreTracker))]
    public sealed class MissionController : NetworkBehaviour
    {
        [SerializeField] private string missionName = "Research Recovery";
        [SerializeField] private MissionObjective[] objectives = Array.Empty<MissionObjective>();
        [SerializeField] private ExtractionPoint extraction;
        private readonly NetworkVariable<MissionState> state = new NetworkVariable<MissionState>(MissionState.Waiting);
        private readonly NetworkVariable<uint> completed = new NetworkVariable<uint>();
        private readonly NetworkVariable<double> startedAt = new NetworkVariable<double>();
        private readonly NetworkVariable<float> duration = new NetworkVariable<float>();
        private readonly List<PlayerHealth> observedPlayers = new List<PlayerHealth>();
        private MissionScoreTracker scores;

        public static MissionController Instance { get; private set; }
        public MissionState State => state.Value;
        public bool IsRunning => State == MissionState.InProgress || State == MissionState.ObjectivesCompleted || State == MissionState.ExtractionActive;
        public bool IsFinished => State == MissionState.Success || State == MissionState.Failure;
        public string MissionName => missionName;
        public int ObjectiveCount => objectives.Length;
        public int CompletedObjectives => MissionRules.CountCompleted(completed.Value, objectives.Length);
        public float ElapsedSeconds => IsRunning && NetworkManager != null
            ? Mathf.Max(0f, (float)(NetworkManager.ServerTime.Time - startedAt.Value)) : duration.Value;
        public ExtractionPoint Extraction => extraction;
        public event Action Changed;

        private void Awake()
        {
            Instance = this;
            scores = GetComponent<MissionScoreTracker>();
        }

        public override void OnNetworkSpawn()
        {
            state.OnValueChanged += OnStateChanged;
            completed.OnValueChanged += OnObjectivesChanged;
            if (IsServer)
                foreach (MissionObjective objective in objectives)
                    if (objective != null) objective.CompletionRequested += OnCompletionRequested;
            if (IsFinished) FreezePlayers();
            Changed?.Invoke();
        }

        public override void OnNetworkDespawn()
        {
            state.OnValueChanged -= OnStateChanged;
            completed.OnValueChanged -= OnObjectivesChanged;
            foreach (MissionObjective objective in objectives)
                if (objective != null) objective.CompletionRequested -= OnCompletionRequested;
            foreach (PlayerHealth health in observedPlayers)
                if (health != null) health.Died -= OnPlayerDied;
            observedPlayers.Clear();
            if (Instance == this) Instance = null;
        }

        public override void OnDestroy()
        {
            if (Instance == this) Instance = null;
            base.OnDestroy();
        }

        public string GetObjectiveLabel(int index) => index >= 0 && index < objectives.Length && objectives[index] != null
            ? objectives[index].Label : string.Empty;
        public bool IsObjectiveComplete(int index) => MissionRules.IsComplete(completed.Value, index, objectives.Length);
        public bool IsObjectiveComplete(MissionObjective objective) => IsObjectiveComplete(Array.IndexOf(objectives, objective));

        public void BeginMission()
        {
            if (!IsServer || !IsSpawned || State != MissionState.Waiting) return;
            if (objectives.Length == 0 || objectives.Length > 32 || objectives.Any(o => o == null) || extraction == null)
            {
                Debug.LogError("Mission needs 1–32 objectives and an extraction point.", this);
                return;
            }
            var players = ConnectedPlayers().ToArray();
            if (players.Length == 0) return;
            completed.Value = 0;
            duration.Value = 0;
            startedAt.Value = NetworkManager.ServerTime.Time;
            scores.BeginMission(players.Select(p => p.OwnerClientId));
            foreach (NetworkPlayer player in players)
            {
                var health = player.GetComponent<PlayerHealth>();
                observedPlayers.Add(health);
                health.Died += OnPlayerDied;
            }
            state.Value = MissionState.InProgress;
        }

        public IEnumerable<NetworkPlayer> ConnectedPlayers()
        {
            if (!IsServer || NetworkManager == null) yield break;
            foreach (NetworkClient client in NetworkManager.ConnectedClients.Values)
            {
                if (client.PlayerObject == null || !client.PlayerObject.IsSpawned) continue;
                NetworkPlayer player = client.PlayerObject.GetComponent<NetworkPlayer>();
                if (player != null && player.gameObject.scene == gameObject.scene) yield return player;
            }
        }

        public bool TryCompleteObjective(MissionObjective objective, ulong? creditedPlayer = null)
        {
            if (!IsServer || !IsRunning) return false;
            int index = Array.IndexOf(objectives, objective);
            if (!MissionRules.TryComplete(completed.Value, index, objectives.Length, out uint next)) return false;
            // A sample credit must belong to a connected, living participant.
            if (creditedPlayer.HasValue && !ConnectedPlayers().Any(p => p.OwnerClientId == creditedPlayer.Value &&
                    p.InMatch && !p.GetComponent<PlayerHealth>().IsDead)) return false;
            completed.Value = next;
            scores.CompleteObjective(creditedPlayer);
            if (CompletedObjectives == ObjectiveCount) state.Value = MissionState.ObjectivesCompleted;
            return true;
        }

        private void OnCompletionRequested(MissionObjective objective) => TryCompleteObjective(objective);
        private void OnPlayerDied() => CheckForFailure();

        private void Update()
        {
            if (!IsServer || !IsRunning) return;
            // Also covers disconnections, after NGO removes them from ConnectedClients.
            CheckForFailure();
            if (State == MissionState.ObjectivesCompleted) state.Value = MissionState.ExtractionActive;
        }

        private void CheckForFailure()
        {
            if (!IsServer || !IsRunning) return;
            if (!ConnectedPlayers().Any(p => !p.GetComponent<PlayerHealth>().IsDead)) Finish(false);
        }

        public void CompleteExtraction()
        {
            if (!IsServer || State != MissionState.ExtractionActive || extraction == null ||
                !extraction.CanComplete) return;
            Finish(true);
        }

        private void Finish(bool success)
        {
            if (!IsServer || !IsRunning) return;
            duration.Value = ElapsedSeconds;
            // Publish the immutable score snapshot before publishing the terminal state.
            scores.FinalizeResults(success, duration.Value, CompletedObjectives);
            state.Value = success ? MissionState.Success : MissionState.Failure;
        }

        private void OnStateChanged(MissionState previous, MissionState current)
        {
            if (IsFinished) FreezePlayers();
            Changed?.Invoke();
        }
        private void OnObjectivesChanged(uint previous, uint current) => Changed?.Invoke();
        private static void FreezePlayers()
        {
            foreach (NetworkPlayer player in FindObjectsByType<NetworkPlayer>()) player.FreezeForResults();
        }
    }
}

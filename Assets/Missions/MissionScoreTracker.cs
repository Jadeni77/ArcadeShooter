using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ArcadeShooter.Missions
{
    // Has no client RPCs: accepted shots, enemy damage/deaths and mission objectives
    // are the only producers. A fresh scene creates a fresh tracker for replay.
    public sealed class MissionScoreTracker : NetworkBehaviour
    {
        [SerializeField] private MissionScoringRules scoring = new MissionScoringRules();
        private NetworkList<PlayerMissionStats> stats;
        private readonly NetworkVariable<int> teamScore = new NetworkVariable<int>();
        private readonly NetworkVariable<MissionRank> rank = new NetworkVariable<MissionRank>();
        private readonly NetworkVariable<float> duration = new NetworkVariable<float>();
        private readonly NetworkVariable<bool> resultsReady = new NetworkVariable<bool>();
        private readonly HashSet<ulong> creditedEnemies = new HashSet<ulong>();
        private MissionController mission;
        private bool begun;
        private int sharedObjectives;

        public static MissionScoreTracker Instance { get; private set; }
        public NetworkList<PlayerMissionStats> Stats => stats;
        public int TeamScore => teamScore.Value;
        public MissionRank Rank => rank.Value;
        public float Duration => duration.Value;
        public bool ResultsReady => resultsReady.Value;

        private bool CanRecord => IsServer && begun && !ResultsReady && mission != null && mission.IsRunning;

        private void Awake()
        {
            Instance = this;
            stats = new NetworkList<PlayerMissionStats>();
            mission = GetComponent<MissionController>();
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == this) Instance = null;
        }

        public override void OnDestroy()
        {
            if (Instance == this) Instance = null;
            base.OnDestroy();
        }

        internal void RegisterEnemySpawn(ulong enemyNetworkObjectId)
        {
            // NGO can recycle an ID after 120 s. A new server-side spawn is a new
            // enemy lifetime; a second damage/death callback is not.
            if (IsServer && !ResultsReady) creditedEnemies.Remove(enemyNetworkObjectId);
        }

        public void BeginMission(IEnumerable<ulong> playerIds)
        {
            if (!IsServer || begun || ResultsReady || playerIds == null) return;
            begun = true;
            stats.Clear();
            creditedEnemies.Clear();
            sharedObjectives = 0;
            teamScore.Value = 0;
            foreach (ulong id in playerIds)
                if (FindPlayer(id) < 0) stats.Add(new PlayerMissionStats { ClientId = id });
        }

        public void RecordShot(ulong playerId)
        {
            int index = FindWritablePlayer(playerId);
            if (index < 0) return;
            PlayerMissionStats value = stats[index];
            value.ShotsFired++;
            stats[index] = value;
        }

        public void RecordHit(ulong playerId)
        {
            int index = FindWritablePlayer(playerId);
            if (index < 0) return;
            PlayerMissionStats value = stats[index];
            // One hitscan bullet can score at most one hit.
            if (value.Hits >= value.ShotsFired) return;
            value.Hits++;
            StoreCombatStats(index, value);
        }

        public void RecordKill(ulong playerId, ulong enemyNetworkObjectId)
        {
            int index = FindWritablePlayer(playerId);
            if (index < 0 || !creditedEnemies.Add(enemyNetworkObjectId)) return;
            PlayerMissionStats value = stats[index];
            value.Kills++;
            StoreCombatStats(index, value);
        }

        public void CompleteObjective(ulong? creditedPlayer)
        {
            if (!CanRecord) return;
            // The controller updates its objective count before notifying scoring.
            // Comparing the count also prevents a duplicate notification paying twice.
            if (mission.CompletedObjectives <= sharedObjectives) return;
            sharedObjectives = mission.CompletedObjectives;
            if (creditedPlayer.HasValue)
            {
                int index = FindPlayer(creditedPlayer.Value);
                if (index >= 0)
                {
                    PlayerMissionStats value = stats[index];
                    value.ObjectivesCompleted++;
                    stats[index] = value;
                }
            }
            UpdateTeamScore(false);
        }

        public void FinalizeResults(bool success, float elapsedSeconds, int completedObjectives)
        {
            if (!IsServer || !begun || ResultsReady) return;
            sharedObjectives = Mathf.Max(0, completedObjectives);
            duration.Value = float.IsNaN(elapsedSeconds) || float.IsInfinity(elapsedSeconds)
                ? 0f : Mathf.Max(0f, elapsedSeconds);
            UpdateTeamScore(success);
            rank.Value = scoring.RankFor(teamScore.Value, success);
            // Written last, so clients can wait for the complete result snapshot.
            resultsReady.Value = true;
        }

        private int FindWritablePlayer(ulong playerId) => CanRecord ? FindPlayer(playerId) : -1;

        private int FindPlayer(ulong playerId)
        {
            for (int i = 0; i < stats.Count; i++)
                if (stats[i].ClientId == playerId) return i;
            return -1;
        }

        private void StoreCombatStats(int index, PlayerMissionStats value)
        {
            value.Score = scoring.CombatScore(value.Kills, value.Hits);
            stats[index] = value;
            UpdateTeamScore(false);
        }

        private void UpdateTeamScore(bool success)
        {
            int combatTotal = 0;
            for (int i = 0; i < stats.Count; i++) combatTotal += stats[i].Score;
            teamScore.Value = scoring.TeamScore(combatTotal, sharedObjectives, success);
        }
    }
}

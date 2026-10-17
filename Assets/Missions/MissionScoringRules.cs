using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ArcadeShooter.Missions
{
    public enum MissionRank : byte { C, B, A, S }

    // Team score = combat (every player's kill and hit points) + shared objectives, counted once.
    // A successful run adds performance bonuses: time under par, team accuracy, and the share of
    // the starting squad that extracted. Every mission kills a fixed set of enemies, so these
    // bonuses are what separate a C run from an S run.
    [Serializable]
    public sealed class MissionScoringRules
    {
        [Min(0)] public int PointsPerKill = 100;
        [Min(0)] public int PointsPerHit = 10;
        [Min(0)] public int PointsPerObjective = 500;
        [Min(0)] public int SuccessBonus;
        [Min(0)] public float ParTimeSeconds = 300f;
        [Min(0)] public int TimeBonusPerSecondUnderPar = 4;
        [Tooltip("Awarded in full at 100% team accuracy.")]
        [Min(0)] public int AccuracyBonus = 800;
        [Tooltip("Awarded in full when every starting player extracts.")]
        [Min(0)] public int SurvivalBonus = 500;
        [Min(0)] public int RankBScore = 3400;
        [Min(0)] public int RankAScore = 3900;
        [Min(0)] public int RankSScore = 4300;

        public int CombatScore(int kills, int hits) =>
            Mathf.Max(0, kills) * Mathf.Max(0, PointsPerKill) +
            Mathf.Max(0, hits) * Mathf.Max(0, PointsPerHit);

        public MissionScoreBreakdown Calculate(int combatTotal, int objectives, bool success, float durationSeconds,
            int teamHits, int teamShots, int survivors, int participants)
        {
            int shots = Math.Max(0, teamShots);
            int squad = Math.Max(0, participants);
            var score = new MissionScoreBreakdown
            {
                Combat = Math.Max(0, combatTotal),
                Objectives = Saturate((long)Math.Max(0, objectives) * Math.Max(0, PointsPerObjective)),
                Survivors = Mathf.Clamp(survivors, 0, squad),
                Participants = squad
            };
            if (success)
            {
                // An unknown duration earns no time bonus; a negative one cannot exceed the full bonus.
                double duration = float.IsNaN(durationSeconds) || float.IsInfinity(durationSeconds)
                    ? double.PositiveInfinity : Math.Max(0d, durationSeconds);
                score.TimeBonus = Saturate(Math.Floor(Math.Max(0d, Math.Max(0f, ParTimeSeconds) - duration) *
                                                      Math.Max(0, TimeBonusPerSecondUnderPar)));
                score.AccuracyBonus = shots > 0
                    ? Saturate((long)Math.Max(0, AccuracyBonus) * Mathf.Clamp(teamHits, 0, shots) / shots) : 0;
                score.SurvivalBonus = squad > 0 ? Saturate((long)Math.Max(0, SurvivalBonus) * score.Survivors / squad) : 0;
                score.SuccessBonus = Math.Max(0, SuccessBonus);
            }
            score.Total = Saturate((long)score.Combat + score.Objectives + score.TimeBonus + score.AccuracyBonus +
                                   score.SurvivalBonus + score.SuccessBonus);
            return score;
        }

        // Extreme Inspector values cap at int.MaxValue instead of wrapping into negative scores.
        private static int Saturate(long value) => (int)Math.Max(0L, Math.Min(int.MaxValue, value));
        private static int Saturate(double value) => value >= int.MaxValue ? int.MaxValue : value <= 0d ? 0 : (int)value;

        public MissionRank RankFor(int score, bool success)
        {
            if (!success) return MissionRank.C;
            int b = Mathf.Max(0, RankBScore);
            int a = Mathf.Max(b, RankAScore);
            int s = Mathf.Max(a, RankSScore);
            if (score >= s) return MissionRank.S;
            if (score >= a) return MissionRank.A;
            return score >= b ? MissionRank.B : MissionRank.C;
        }
    }

    public struct MissionScoreBreakdown : INetworkSerializable, IEquatable<MissionScoreBreakdown>
    {
        public int Combat;
        public int Objectives;
        public int TimeBonus;
        public int AccuracyBonus;
        public int SurvivalBonus;
        public int SuccessBonus;
        public int Total;
        public int Survivors;
        public int Participants;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Combat);
            serializer.SerializeValue(ref Objectives);
            serializer.SerializeValue(ref TimeBonus);
            serializer.SerializeValue(ref AccuracyBonus);
            serializer.SerializeValue(ref SurvivalBonus);
            serializer.SerializeValue(ref SuccessBonus);
            serializer.SerializeValue(ref Total);
            serializer.SerializeValue(ref Survivors);
            serializer.SerializeValue(ref Participants);
        }

        public bool Equals(MissionScoreBreakdown other) =>
            Combat == other.Combat && Objectives == other.Objectives && TimeBonus == other.TimeBonus &&
            AccuracyBonus == other.AccuracyBonus && SurvivalBonus == other.SurvivalBonus &&
            SuccessBonus == other.SuccessBonus && Total == other.Total &&
            Survivors == other.Survivors && Participants == other.Participants;
    }

    public struct MissionTeamTotals
    {
        public int Kills;
        public int ShotsFired;
        public int Hits;
        public int Score;
        public float Accuracy;

        public static MissionTeamTotals From(IEnumerable<PlayerMissionStats> players)
        {
            var totals = new MissionTeamTotals();
            if (players == null) return totals;
            foreach (PlayerMissionStats player in players)
            {
                totals.Kills += player.Kills;
                totals.ShotsFired += player.ShotsFired;
                totals.Hits += player.Hits;
                totals.Score += player.Score;
            }
            totals.Accuracy = totals.ShotsFired > 0 ? Mathf.Clamp01((float)totals.Hits / totals.ShotsFired) * 100f : 0f;
            return totals;
        }
    }

    public static class MissionRoster
    {
        // Numbers players by NGO client ID, i.e. network connection order: the host is Player 1,
        // and the numbers match the spawn point order.
        public static List<PlayerMissionStats> CreateStats(IEnumerable<ulong> clientIds)
        {
            var ids = new SortedSet<ulong>(clientIds ?? Array.Empty<ulong>());
            var roster = new List<PlayerMissionStats>(ids.Count);
            foreach (ulong id in ids)
                roster.Add(new PlayerMissionStats { ClientId = id, PlayerNumber = roster.Count + 1 });
            return roster;
        }
    }

    public struct PlayerMissionStats : INetworkSerializable, IEquatable<PlayerMissionStats>
    {
        public ulong ClientId;
        public int PlayerNumber;
        public int Kills;
        public int ShotsFired;
        public int Hits;
        // Only directly attributable interactions (the sample) credit a player.
        public int ObjectivesCompleted;
        public int Score;
        public float Accuracy => ShotsFired > 0 ? Mathf.Clamp01((float)Hits / ShotsFired) * 100f : 0f;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PlayerNumber);
            serializer.SerializeValue(ref Kills);
            serializer.SerializeValue(ref ShotsFired);
            serializer.SerializeValue(ref Hits);
            serializer.SerializeValue(ref ObjectivesCompleted);
            serializer.SerializeValue(ref Score);
        }

        public bool Equals(PlayerMissionStats other) =>
            ClientId == other.ClientId && PlayerNumber == other.PlayerNumber && Kills == other.Kills &&
            ShotsFired == other.ShotsFired && Hits == other.Hits &&
            ObjectivesCompleted == other.ObjectivesCompleted && Score == other.Score;
    }
}

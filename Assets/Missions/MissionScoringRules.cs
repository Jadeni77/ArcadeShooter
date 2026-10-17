using System;
using Unity.Netcode;
using UnityEngine;

namespace ArcadeShooter.Missions
{
    public enum MissionRank : byte { C, B, A, S }

    [Serializable]
    public sealed class MissionScoringRules
    {
        [Min(0)] public int PointsPerKill = 100;
        [Min(0)] public int PointsPerHit = 10;
        [Min(0)] public int PointsPerObjective = 500;
        [Min(0)] public int SuccessBonus;
        [Min(0)] public int RankBScore = 1800;
        [Min(0)] public int RankAScore = 2400;
        [Min(0)] public int RankSScore = 2800;

        public int CombatScore(int kills, int hits) =>
            Mathf.Max(0, kills) * Mathf.Max(0, PointsPerKill) +
            Mathf.Max(0, hits) * Mathf.Max(0, PointsPerHit);

        // Shared objectives are added once, rather than once per squad member.
        public int TeamScore(int individualCombatTotal, int objectives, bool success) =>
            Mathf.Max(0, individualCombatTotal) +
            Mathf.Max(0, objectives) * Mathf.Max(0, PointsPerObjective) +
            (success ? Mathf.Max(0, SuccessBonus) : 0);

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

    public struct PlayerMissionStats : INetworkSerializable, IEquatable<PlayerMissionStats>
    {
        public ulong ClientId;
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
            serializer.SerializeValue(ref Kills);
            serializer.SerializeValue(ref ShotsFired);
            serializer.SerializeValue(ref Hits);
            serializer.SerializeValue(ref ObjectivesCompleted);
            serializer.SerializeValue(ref Score);
        }

        public bool Equals(PlayerMissionStats other) =>
            ClientId == other.ClientId && Kills == other.Kills && ShotsFired == other.ShotsFired &&
            Hits == other.Hits && ObjectivesCompleted == other.ObjectivesCompleted && Score == other.Score;
    }
}

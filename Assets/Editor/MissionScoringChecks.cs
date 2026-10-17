using System;
using ArcadeShooter.Missions;
using UnityEditor;
using UnityEngine;

namespace ArcadeShooter.Editor
{
    // Deterministic checks callable from a batch/connected Editor without changing
    // production assembly boundaries merely to expose these small pure rules.
    public static class MissionScoringChecks
    {
        [MenuItem("ArcadeShooter/Validation/Check Mission Scoring")]
        public static void Run()
        {
            var rules = new MissionScoringRules();
            Expect(new PlayerMissionStats().Accuracy == 0f, "Zero shots must have zero accuracy.");
            Expect(new PlayerMissionStats { ShotsFired = 8, Hits = 3 }.Accuracy == 37.5f,
                "Accuracy must count confirmed enemy hits over accepted shots.");
            Expect(new PlayerMissionStats { ShotsFired = 2, Hits = 3 }.Accuracy == 100f,
                "Accuracy must remain bounded even for malformed input.");
            Expect(rules.CombatScore(2, 6) == 260, "Default kill/hit scoring must be 100/10.");
            Expect(rules.TeamScore(260 + 130, 3, true) == 1890,
                "Shared objectives must contribute 1500 once for the whole team.");
            Expect(rules.TeamScore(0, 0, false) == 0, "An empty failed run must score zero.");
            Expect(rules.RankFor(10000, false) == MissionRank.C, "A failed run must rank C.");
            Expect(rules.RankFor(1799, true) == MissionRank.C && rules.RankFor(1800, true) == MissionRank.B,
                "The B threshold must be inclusive.");
            Expect(rules.RankFor(2399, true) == MissionRank.B && rules.RankFor(2400, true) == MissionRank.A,
                "The A threshold must be inclusive.");
            Expect(rules.RankFor(2799, true) == MissionRank.A && rules.RankFor(2800, true) == MissionRank.S,
                "The S threshold must be inclusive.");
            rules.SuccessBonus = 200;
            Expect(rules.TeamScore(130, 1, true) == 830 && rules.TeamScore(130, 1, false) == 630,
                "A configured success bonus must apply once and only on success.");
            rules.PointsPerKill = -100;
            rules.PointsPerHit = -10;
            rules.PointsPerObjective = -500;
            Expect(rules.CombatScore(-1, -1) == 0 && rules.CombatScore(1, 1) == 0 &&
                rules.TeamScore(-100, 1, false) == 0, "Negative configuration must not produce negative rewards.");
            Debug.Log("Mission scoring checks passed (12 checks).");
        }

        private static void Expect(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}

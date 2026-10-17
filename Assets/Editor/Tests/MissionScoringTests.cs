using System.Collections.Generic;
using ArcadeShooter.Missions;
using NUnit.Framework;

namespace ArcadeShooter.Editor.Tests
{
    // Expected totals are worked out by hand from the default rules:
    // 100/kill, 10/hit, 500/objective, (300 s - duration) x 4 time bonus, 800 x team accuracy,
    // 500 x surviving share of the squad; ranks B 3,400, A 3,900, S 4,300.
    public sealed class MissionScoringTests
    {
        [Test]
        public void AccuracyIsConfirmedHitsOverAcceptedShotsAndZeroWithoutShots()
        {
            Assert.That(new PlayerMissionStats().Accuracy, Is.EqualTo(0f));
            Assert.That(new PlayerMissionStats { ShotsFired = 8, Hits = 3 }.Accuracy, Is.EqualTo(37.5f));
            Assert.That(new PlayerMissionStats { ShotsFired = 2, Hits = 3 }.Accuracy, Is.EqualTo(100f));
        }

        [Test]
        public void CombatScoreIsOneHundredPerKillAndTenPerHit()
        {
            Assert.That(new MissionScoringRules().CombatScore(2, 6), Is.EqualTo(260));
        }

        [Test]
        public void FailedRunEarnsCombatAndObjectivePointsWithoutBonuses()
        {
            var rules = new MissionScoringRules();
            MissionScoreBreakdown score = rules.Calculate(390, 2, false, 120f, 9, 20, 0, 2);
            Assert.That(score.Combat, Is.EqualTo(390));
            Assert.That(score.Objectives, Is.EqualTo(1000));
            Assert.That(score.TimeBonus + score.AccuracyBonus + score.SurvivalBonus + score.SuccessBonus, Is.EqualTo(0));
            Assert.That(score.Total, Is.EqualTo(1390));
            Assert.That(rules.RankFor(score.Total, false), Is.EqualTo(MissionRank.C));
        }

        [Test]
        public void QuickAccurateFullSquadRunRanksA()
        {
            var rules = new MissionScoringRules();
            MissionScoreBreakdown score = rules.Calculate(1170, 3, true, 180f, 27, 60, 2, 2);
            Assert.That(score.TimeBonus, Is.EqualTo(480));
            Assert.That(score.AccuracyBonus, Is.EqualTo(360));
            Assert.That(score.SurvivalBonus, Is.EqualTo(500));
            Assert.That(score.Total, Is.EqualTo(4010));
            Assert.That(rules.RankFor(score.Total, true), Is.EqualTo(MissionRank.A));
        }

        [Test]
        public void FastAccurateRunWithTheOptionalKillRanksS()
        {
            var rules = new MissionScoringRules();
            MissionScoreBreakdown score = rules.Calculate(1300, 3, true, 120f, 30, 50, 2, 2);
            Assert.That(score.Total, Is.EqualTo(4500));
            Assert.That(rules.RankFor(score.Total, true), Is.EqualTo(MissionRank.S));
        }

        [Test]
        public void AverageRunRanksB()
        {
            var rules = new MissionScoringRules();
            MissionScoreBreakdown score = rules.Calculate(1170, 3, true, 240f, 27, 77, 2, 2);
            Assert.That(score.AccuracyBonus, Is.EqualTo(280));
            Assert.That(score.Total, Is.EqualTo(3690));
            Assert.That(rules.RankFor(score.Total, true), Is.EqualTo(MissionRank.B));
        }

        [Test]
        public void SlowInaccurateRunThatLostAPlayerCanStillRankC()
        {
            var rules = new MissionScoringRules();
            MissionScoreBreakdown score = rules.Calculate(1170, 3, true, 600f, 27, 270, 1, 2);
            Assert.That(score.TimeBonus, Is.EqualTo(0));
            Assert.That(score.AccuracyBonus, Is.EqualTo(80));
            Assert.That(score.SurvivalBonus, Is.EqualTo(250));
            Assert.That(score.Total, Is.EqualTo(3000));
            Assert.That(rules.RankFor(score.Total, true), Is.EqualTo(MissionRank.C));
        }

        [Test]
        public void ZeroShotsGiveNoAccuracyBonus()
        {
            MissionScoreBreakdown score = new MissionScoringRules().Calculate(0, 3, true, 100f, 0, 0, 1, 1);
            Assert.That(score.AccuracyBonus, Is.EqualTo(0));
            Assert.That(score.Total, Is.EqualTo(2800));
        }

        [Test]
        public void MalformedInputCannotInflateBonuses()
        {
            var rules = new MissionScoringRules();
            // More hits than shots and more survivors than players are capped at 100%.
            Assert.That(rules.Calculate(0, 0, true, 300f, 10, 5, 3, 2).Total, Is.EqualTo(1300));
            // A negative duration cannot exceed the full par-time bonus; NaN earns none.
            Assert.That(rules.Calculate(0, 0, true, -5f, 0, 0, 1, 1).TimeBonus, Is.EqualTo(1200));
            Assert.That(rules.Calculate(0, 0, true, float.NaN, 0, 0, 1, 1).TimeBonus, Is.EqualTo(0));
            Assert.That(rules.Calculate(0, 0, true, 0f, 0, 0, 0, 0).SurvivalBonus, Is.EqualTo(0));
        }

        [Test]
        public void RankThresholdsAreInclusiveAndFailureIsAlwaysC()
        {
            var rules = new MissionScoringRules();
            Assert.That(rules.RankFor(3399, true), Is.EqualTo(MissionRank.C));
            Assert.That(rules.RankFor(3400, true), Is.EqualTo(MissionRank.B));
            Assert.That(rules.RankFor(3899, true), Is.EqualTo(MissionRank.B));
            Assert.That(rules.RankFor(3900, true), Is.EqualTo(MissionRank.A));
            Assert.That(rules.RankFor(4299, true), Is.EqualTo(MissionRank.A));
            Assert.That(rules.RankFor(4300, true), Is.EqualTo(MissionRank.S));
            Assert.That(rules.RankFor(99999, false), Is.EqualTo(MissionRank.C));
        }

        [Test]
        public void SuccessBonusAppliesOnlyOnSuccess()
        {
            var rules = new MissionScoringRules { SuccessBonus = 200 };
            Assert.That(rules.Calculate(130, 1, true, 300f, 3, 3, 1, 1).Total, Is.EqualTo(2130));
            Assert.That(rules.Calculate(130, 1, false, 300f, 3, 3, 1, 1).Total, Is.EqualTo(630));
        }

        [Test]
        public void NegativeConfigurationNeverProducesNegativeRewards()
        {
            var rules = new MissionScoringRules
            {
                PointsPerKill = -100, PointsPerHit = -10, PointsPerObjective = -500, SuccessBonus = -1,
                TimeBonusPerSecondUnderPar = -4, AccuracyBonus = -800, SurvivalBonus = -500
            };
            Assert.That(rules.CombatScore(1, 1), Is.EqualTo(0));
            MissionScoreBreakdown score = rules.Calculate(-100, 1, true, 10f, 5, 4, 3, 2);
            Assert.That(score.Total, Is.EqualTo(0));
        }

        [Test]
        public void HugeConfigurationValuesSaturateInsteadOfOverflowing()
        {
            var rules = new MissionScoringRules
            {
                PointsPerObjective = int.MaxValue, TimeBonusPerSecondUnderPar = int.MaxValue,
                AccuracyBonus = int.MaxValue, SurvivalBonus = int.MaxValue
            };
            MissionScoreBreakdown score = rules.Calculate(int.MaxValue, 3, true, 0f, 480, 480, 4, 4);
            Assert.That(score.Objectives, Is.EqualTo(int.MaxValue));
            Assert.That(score.TimeBonus, Is.EqualTo(int.MaxValue));
            Assert.That(score.AccuracyBonus, Is.EqualTo(int.MaxValue));
            Assert.That(score.SurvivalBonus, Is.EqualTo(int.MaxValue));
            Assert.That(score.Total, Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void TeamTotalsAddUpPlayersAndUseTeamAccuracy()
        {
            var players = new List<PlayerMissionStats>
            {
                new PlayerMissionStats { ClientId = 0, Kills = 4, ShotsFired = 11, Hits = 11, Score = 510 },
                new PlayerMissionStats { ClientId = 1, Kills = 5, ShotsFired = 17, Hits = 16, ObjectivesCompleted = 1, Score = 660 }
            };
            MissionTeamTotals totals = MissionTeamTotals.From(players);
            Assert.That(totals.Kills, Is.EqualTo(9));
            Assert.That(totals.ShotsFired, Is.EqualTo(28));
            Assert.That(totals.Hits, Is.EqualTo(27));
            Assert.That(totals.Score, Is.EqualTo(1170));
            Assert.That(totals.Accuracy, Is.EqualTo(96.43f).Within(0.01f));
            Assert.That(MissionTeamTotals.From(new List<PlayerMissionStats>()).Accuracy, Is.EqualTo(0f));
        }

        [Test]
        public void PlayersAreNumberedInConnectionOrderWithoutDuplicates()
        {
            List<PlayerMissionStats> roster = MissionRoster.CreateStats(new ulong[] { 5, 0, 3, 3 });
            Assert.That(roster.Count, Is.EqualTo(3));
            Assert.That(roster[0].ClientId, Is.EqualTo(0ul));
            Assert.That(roster[0].PlayerNumber, Is.EqualTo(1));
            Assert.That(roster[1].ClientId, Is.EqualTo(3ul));
            Assert.That(roster[1].PlayerNumber, Is.EqualTo(2));
            Assert.That(roster[2].ClientId, Is.EqualTo(5ul));
            Assert.That(roster[2].PlayerNumber, Is.EqualTo(3));
        }
    }
}

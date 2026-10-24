using System;
using ArcadeShooter.Missions;
using UnityEngine;

namespace ArcadeShooter.Progression
{
    public enum MissionDifficulty : byte { Normal, Hard }
    public enum RifleFinish : byte { Standard, Teal }
    public enum PlayerTitle : byte { Recruit, Pathfinder }

    [Serializable]
    public sealed class ProgressionRules
    {
        [Min(1)] public int firstLevelXp = 400;
        [Min(0)] public int additionalXpPerLevel = 200;
        [Min(0)] public int extractionXp = 300;
        [Min(0)] public int scoreDivisor = 20;
        [Range(0, 100)] public int hardBonusPercent = 25;
        [Range(0, 100)] public int failedSharePercent = 25;
        [Min(1)] public int tealFinishLevel = 2;
        [Min(1)] public int pathfinderTitleLevel = 3;

        public int ThresholdForLevel(int level)
        {
            long n = Math.Max(0, level - 1);
            long total = n * Math.Max(1, firstLevelXp) + n * (n - 1) / 2 * Math.Max(0, additionalXpPerLevel);
            return (int)Math.Min(int.MaxValue, total);
        }

        public int LevelForXp(int xp)
        {
            int level = 1;
            while (level < 10000 && ThresholdForLevel(level + 1) <= Math.Max(0, xp)) level++;
            return level;
        }

        public int Reward(int personalScore, bool extracted, MissionDifficulty difficulty)
        {
            long baseXp = (long)Math.Max(0, personalScore) / Math.Max(1, scoreDivisor);
            if (extracted) baseXp += Math.Max(0, extractionXp);
            else baseXp = baseXp * Mathf.Clamp(failedSharePercent, 0, 100) / 100;
            if (extracted && difficulty == MissionDifficulty.Hard)
                baseXp += baseXp * Mathf.Clamp(hardBonusPercent, 0, 100) / 100;
            return (int)Math.Min(int.MaxValue, baseXp);
        }

        public bool FinishUnlocked(int level, RifleFinish finish) =>
            finish == RifleFinish.Standard || finish == RifleFinish.Teal && level >= tealFinishLevel;
        public bool TitleUnlocked(int level, PlayerTitle title) =>
            title == PlayerTitle.Recruit || title == PlayerTitle.Pathfinder && level >= pathfinderTitleLevel;
    }
}

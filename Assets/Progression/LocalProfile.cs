using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ArcadeShooter.Progression
{
    [Serializable]
    public sealed class ProfileData
    {
        public int version = 1;
        public int xp;
        public int unlockedRewards;
        public RifleFinish selectedFinish;
        public PlayerTitle selectedTitle;
        public int completedMissions;
        public int failedMissions;
        public string[] claimedResults = Array.Empty<string>();
    }

    public sealed class LocalProfile
    {
        public const string FileName = "player-profile-v1.json";
        private readonly string path;
        private readonly ProgressionRules rules;
        private ProfileData data = new ProfileData();
        private bool canSave = true;
        public ProfileData Data => data;
        public int Level => rules.LevelForXp(data.xp);
        public int NextLevelXp => rules.ThresholdForLevel(Level + 1);
        public int XpIntoLevel => data.xp - rules.ThresholdForLevel(Level);
        public int XpForNextLevel => NextLevelXp - rules.ThresholdForLevel(Level);
        public string LoadWarning { get; private set; }
        public ProgressionRules Rules => rules;
        public event Action Changed;

        public LocalProfile(string directory, ProgressionRules rules)
        {
            path = Path.Combine(directory, FileName);
            this.rules = rules ?? new ProgressionRules();
            Load();
        }

        public void Load()
        {
            if (!File.Exists(path)) return;
            try
            {
                ProfileData loaded = JsonUtility.FromJson<ProfileData>(File.ReadAllText(path));
                if (loaded == null || loaded.version != 1 || loaded.xp < 0 || loaded.claimedResults == null)
                    throw new InvalidDataException("Unsupported or invalid profile data.");
                data = loaded;
                canSave = true;
                LoadWarning = null;
                Changed?.Invoke();
            }
            catch (Exception exception)
            {
                // An unknown version or damaged file remains intact for recovery.
                canSave = false;
                LoadWarning = $"Profile could not be loaded: {exception.Message}";
                Debug.LogWarning(LoadWarning);
            }
        }

        public bool Claim(string resultId, int xp, bool extracted, out int oldLevel, out int newLevel)
        {
            oldLevel = newLevel = Level;
            if (!canSave || string.IsNullOrEmpty(resultId) || xp < 0 || Array.IndexOf(data.claimedResults, resultId) >= 0)
                return false;
            ProfileData previous = Clone(data);
            var ids = new List<string>(data.claimedResults) { resultId };
            data.claimedResults = ids.ToArray();
            data.xp = (int)Math.Min(int.MaxValue, (long)data.xp + xp);
            if (extracted) data.completedMissions++;
            else data.failedMissions++;
            newLevel = Level;
            if (newLevel >= rules.tealFinishLevel) data.unlockedRewards |= 1;
            if (newLevel >= rules.pathfinderTitleLevel) data.unlockedRewards |= 2;
            if (!Save()) { data = previous; newLevel = oldLevel; return false; }
            Changed?.Invoke();
            return true;
        }

        public bool SelectFinish(RifleFinish finish)
        {
            if (!canSave || !rules.FinishUnlocked(Level, finish)) return false;
            RifleFinish previous = data.selectedFinish;
            data.selectedFinish = finish;
            if (!Save()) { data.selectedFinish = previous; return false; }
            Changed?.Invoke();
            return true;
        }

        public bool SelectTitle(PlayerTitle title)
        {
            if (!canSave || !rules.TitleUnlocked(Level, title)) return false;
            PlayerTitle previous = data.selectedTitle;
            data.selectedTitle = title;
            if (!Save()) { data.selectedTitle = previous; return false; }
            Changed?.Invoke();
            return true;
        }

        private bool Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                string temp = path + ".tmp";
                File.WriteAllText(temp, JsonUtility.ToJson(data, true));
                if (File.Exists(path)) File.Replace(temp, path, path + ".bak");
                else File.Move(temp, path);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError("Could not save player profile: " + exception.Message);
                return false;
            }
        }

        private static ProfileData Clone(ProfileData source) =>
            JsonUtility.FromJson<ProfileData>(JsonUtility.ToJson(source));
    }
}

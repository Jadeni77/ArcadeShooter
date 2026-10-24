using System;
using System.IO;
using ArcadeShooter.Progression;
using ArcadeShooter.Encounters;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcadeShooter.Editor.Tests
{
    public sealed class ProgressionTests
    {
        private string directory;

        [SetUp]
        public void SetUp() => directory = Path.Combine(Path.GetTempPath(), "ArcadeShooterProfile-" + Guid.NewGuid().ToString("N"));

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }

        [Test]
        public void LevelCurveAndMultiLevelReward()
        {
            var rules = new ProgressionRules();
            Assert.That(rules.ThresholdForLevel(2), Is.EqualTo(400));
            Assert.That(rules.ThresholdForLevel(3), Is.EqualTo(1000));
            Assert.That(rules.LevelForXp(1000), Is.EqualTo(3));
            var profile = new LocalProfile(directory, rules);
            Assert.That(profile.Claim("match-a", 1100, true, out int before, out int after), Is.True);
            Assert.That(before, Is.EqualTo(1));
            Assert.That(after, Is.EqualTo(3));
            Assert.That(profile.Data.unlockedRewards, Is.EqualTo(3));
        }

        [Test]
        public void RewardUsesHostScoreAndDifficulty()
        {
            var rules = new ProgressionRules();
            Assert.That(rules.Reward(1000, true, MissionDifficulty.Normal), Is.EqualTo(350));
            Assert.That(rules.Reward(1000, true, MissionDifficulty.Hard), Is.EqualTo(437));
            Assert.That(rules.Reward(1000, false, MissionDifficulty.Hard), Is.EqualTo(12));
        }

        [Test]
        public void SaveReloadAndDuplicateResultKeepIndependentProgress()
        {
            var rules = new ProgressionRules();
            var first = new LocalProfile(directory, rules);
            Assert.That(first.Claim("match-one", 410, true, out _, out _), Is.True);
            Assert.That(first.SelectFinish(RifleFinish.Teal), Is.True);
            var reopened = new LocalProfile(directory, rules);
            Assert.That(reopened.Level, Is.EqualTo(2));
            Assert.That(reopened.Data.selectedFinish, Is.EqualTo(RifleFinish.Teal));
            Assert.That(reopened.Claim("match-one", 410, true, out _, out _), Is.False);
            Assert.That(reopened.Data.xp, Is.EqualTo(410));
            Assert.That(reopened.Claim("match-two", 410, true, out _, out _), Is.True);
            Assert.That(new LocalProfile(Path.Combine(directory, "other-computer"), rules).Data.xp, Is.Zero);
        }

        [Test]
        public void LockedRewardAndUnknownSaveVersionAreNotOverwritten()
        {
            var profile = new LocalProfile(directory, new ProgressionRules());
            Assert.That(profile.SelectFinish(RifleFinish.Teal), Is.False);
            Assert.That(profile.SelectTitle(PlayerTitle.Pathfinder), Is.False);
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, LocalProfile.FileName);
            const string unknownVersion = "{\"version\":99,\"xp\":9000,\"claimedResults\":[]}";
            File.WriteAllText(path, unknownVersion);
            profile.Load();
            Assert.That(profile.LoadWarning, Is.Not.Empty);
            Assert.That(profile.Claim("match", 100, true, out _, out _), Is.False);
            Assert.That(File.ReadAllText(path), Is.EqualTo(unknownVersion));
        }

        [Test]
        public void ZoneBHasTwoBalancedConfigurationsOnExistingMarkers()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/FirstPersonTest.unity", OpenSceneMode.Single);
            EnemyWaveSpawner zoneB = null;
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (EnemyWaveSpawner spawner in root.GetComponentsInChildren<EnemyWaveSpawner>(true))
                    if (spawner.name == "Danger Zone B") zoneB = spawner;
            Assert.That(zoneB, Is.Not.Null);
            var serialized = new SerializedObject(zoneB);
            Assert.That(serialized.FindProperty("spawnPoints").arraySize, Is.EqualTo(6));
            var normal = serialized.FindProperty("waves");
            var alternate = serialized.FindProperty("alternateWaves");
            Assert.That(normal.arraySize, Is.EqualTo(2));
            Assert.That(alternate.arraySize, Is.EqualTo(2));
            Assert.That(normal.GetArrayElementAtIndex(0).FindPropertyRelative("enemyCount").intValue, Is.EqualTo(2));
            Assert.That(normal.GetArrayElementAtIndex(1).FindPropertyRelative("enemyCount").intValue, Is.EqualTo(4));
            Assert.That(alternate.GetArrayElementAtIndex(0).FindPropertyRelative("enemyCount").intValue, Is.EqualTo(3));
            Assert.That(alternate.GetArrayElementAtIndex(1).FindPropertyRelative("enemyCount").intValue, Is.EqualTo(3));
        }
    }
}

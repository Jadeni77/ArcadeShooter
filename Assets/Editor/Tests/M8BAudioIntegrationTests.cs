using ArcadeShooter.Audio;
using ArcadeShooter.Combat;
using ArcadeShooter.Enemies;
using ArcadeShooter.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace ArcadeShooter.Editor.Tests
{
    public sealed class M8BAudioIntegrationTests
    {
        [Test]
        public void RequiredPlaceholdersAreAssignedToGameplayAssets()
        {
            GameObject player = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/FirstPersonPlayer.prefab");
            GameObject enemy = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/MeleeEnemy.prefab");
            Assert.That(player, Is.Not.Null);
            Assert.That(enemy, Is.Not.Null);
            AssertReferences(player.GetComponent<WeaponAudio>(), "magazineOut", "magazineIn",
                "reloadComplete", "empty", "sfxGroup");
            AssertReferences(player.GetComponent<PlayerFoley>(), "jump", "land", "damaged", "sfxGroup");
            AssertClipArray(player.GetComponent<PlayerFoley>(), "footsteps", 3);
            EnemyAudio enemyAudio = enemy.GetComponent<EnemyAudio>();
            AssertReferences(enemyAudio, "sfxGroup");
            foreach (string field in new[] { "idle", "alert", "attack", "damage", "death" })
                AssertClipArray(enemyAudio, field, 1);

            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/FirstPersonTest.unity", OpenSceneMode.Single);
            MusicManager music = null;
            AudioMixSettings mix = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (music == null) music = root.GetComponentInChildren<MusicManager>(true);
                if (mix == null) mix = root.GetComponentInChildren<AudioMixSettings>(true);
            }
            AssertReferences(music, "exploration", "dangerWarning", "combat", "facilityAmbience",
                "warningCue", "areaClearedCue", "extractionActiveCue", "countdownStartCue",
                "countdownTickCue", "successCue", "failureCue", "musicGroup", "ambienceGroup", "uiGroup");
            AssertReferences(mix, "mixer");
        }

        [Test]
        public void MixerContainsRoutedBusesWithCallableVolumeParameters()
        {
            AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/CombatAudio.mixer");
            Assert.That(mixer, Is.Not.Null);
            foreach (string group in new[] { "Master", "Music", "SFX", "Ambience", "UI" })
                Assert.That(mixer.FindMatchingGroups(group), Has.Some.Property("name").EqualTo(group));
            foreach (string parameter in new[] { "MasterVolume", "MusicVolume", "SfxVolume",
                         "AmbienceVolume", "UiVolume" })
                Assert.That(mixer.GetFloat(parameter, out _), Is.True, parameter);

            Assert.That(mixer.GetFloat("MusicVolume", out float original), Is.True);
            var gameObject = new GameObject("M8B mixer test");
            try
            {
                AudioMixSettings settings = gameObject.AddComponent<AudioMixSettings>();
                var serialized = new SerializedObject(settings);
                serialized.FindProperty("mixer").objectReferenceValue = mixer;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                settings.SetLevel(AudioMixSettings.Bus.Music, 0.25f);
                Assert.That(mixer.GetFloat("MusicVolume", out float decibels), Is.True);
                Assert.That(decibels, Is.EqualTo(-12.0412f).Within(0.01f));
            }
            finally
            {
                mixer.SetFloat("MusicVolume", original);
                Object.DestroyImmediate(gameObject);
            }
        }

        private static void AssertReferences(Object component, params string[] names)
        {
            Assert.That(component, Is.Not.Null);
            var serialized = new SerializedObject(component);
            foreach (string name in names)
            {
                SerializedProperty property = serialized.FindProperty(name);
                Assert.That(property, Is.Not.Null, name);
                Assert.That(property.objectReferenceValue, Is.Not.Null, name);
            }
        }

        private static void AssertClipArray(Object component, string name, int count)
        {
            Assert.That(component, Is.Not.Null);
            SerializedProperty property = new SerializedObject(component).FindProperty(name);
            Assert.That(property, Is.Not.Null, name);
            Assert.That(property.arraySize, Is.EqualTo(count), name);
            for (int i = 0; i < count; i++)
                Assert.That(property.GetArrayElementAtIndex(i).objectReferenceValue, Is.Not.Null,
                    $"{name}[{i}]");
        }
    }
}

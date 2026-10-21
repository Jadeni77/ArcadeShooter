using System;
using ArcadeShooter.Audio;
using ArcadeShooter.Combat;
using ArcadeShooter.Encounters;
using ArcadeShooter.Enemies;
using ArcadeShooter.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace ArcadeShooter.Editor
{
    // Reapplies project-authored placeholder clips without changing gameplay state.
    public static class PlaceholderAudioSetup
    {
        private const string MixerPath = "Assets/CombatAudio.mixer";
        private const string PlayerPath = "Assets/Prefabs/FirstPersonPlayer.prefab";
        private const string EnemyPath = "Assets/Prefabs/MeleeEnemy.prefab";
        private const string ImpactPath = "Assets/Prefabs/BulletImpact.prefab";
        private const string ScenePath = "Assets/Scenes/FirstPersonTest.unity";

        [MenuItem("ArcadeShooter/Assign M8B Placeholder Audio")]
        public static void Assign()
        {
            AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            if (mixer == null) throw new InvalidOperationException($"Create the Audio Mixer at {MixerPath} first.");
            AudioMixerGroup music = Group(mixer, "Music");
            AudioMixerGroup sfx = Group(mixer, "SFX");
            AudioMixerGroup ambience = Group(mixer, "Ambience");
            AudioMixerGroup ui = Group(mixer, "UI");

            ConfigurePlayer(sfx);
            ConfigureEnemy(sfx);
            ConfigureImpact(sfx);
            ConfigureScene(mixer, music, sfx, ambience, ui);
            AssetDatabase.SaveAssets();
            Debug.Log("M8B placeholder audio assigned to player, enemy, impact, and test scene.");
        }

        private static AudioMixerGroup Group(AudioMixer mixer, string name)
        {
            foreach (AudioMixerGroup group in mixer.FindMatchingGroups(name))
                if (group.name == name) return group;
            throw new InvalidOperationException($"Missing {name} group in {MixerPath}.");
        }

        private static AudioClip Clip(string name)
        {
            string path = $"Assets/Audio/Placeholders/{name}.wav";
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null) throw new InvalidOperationException($"Missing audio clip: {path}");
            return clip;
        }

        private static void Set(SerializedObject target, string property, UnityEngine.Object value)
        {
            SerializedProperty field = target.FindProperty(property);
            if (field == null) throw new InvalidOperationException($"Missing {property} on {target.targetObject.name}");
            field.objectReferenceValue = value;
        }

        private static void SetArray(SerializedObject target, string property, params string[] names)
        {
            SerializedProperty field = target.FindProperty(property);
            if (field == null) throw new InvalidOperationException($"Missing {property} on {target.targetObject.name}");
            field.arraySize = names.Length;
            for (int i = 0; i < names.Length; i++) field.GetArrayElementAtIndex(i).objectReferenceValue = Clip(names[i]);
        }

        private static void ConfigurePlayer(AudioMixerGroup sfx)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PlayerPath);
            try
            {
                WeaponAudio weapon = root.GetComponent<WeaponAudio>();
                PlayerFoley foley = root.GetComponent<PlayerFoley>();
                if (weapon == null || foley == null) throw new InvalidOperationException("Player audio components are missing.");
                var weaponData = new SerializedObject(weapon);
                Set(weaponData, "magazineOut", Clip("MagazineOut"));
                Set(weaponData, "magazineIn", Clip("MagazineIn"));
                Set(weaponData, "reloadComplete", Clip("ReloadComplete"));
                Set(weaponData, "empty", Clip("EmptyClick"));
                Set(weaponData, "sfxGroup", sfx);
                weaponData.ApplyModifiedPropertiesWithoutUndo();

                var foleyData = new SerializedObject(foley);
                SetArray(foleyData, "footsteps", "FootstepA", "FootstepB", "FootstepC");
                Set(foleyData, "jump", Clip("Jump"));
                Set(foleyData, "land", Clip("Land"));
                Set(foleyData, "damaged", Clip("PlayerDamage"));
                Set(foleyData, "sfxGroup", sfx);
                foleyData.ApplyModifiedPropertiesWithoutUndo();

                AudioSource[] sources = root.GetComponents<AudioSource>();
                foreach (AudioSource source in sources) source.outputAudioMixerGroup = sfx;
                PrefabUtility.SaveAsPrefabAsset(root, PlayerPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void ConfigureEnemy(AudioMixerGroup sfx)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(EnemyPath);
            try
            {
                EnemyAudio enemy = root.GetComponent<EnemyAudio>();
                if (enemy == null) throw new InvalidOperationException("EnemyAudio is missing.");
                var data = new SerializedObject(enemy);
                SetArray(data, "idle", "EnemyIdle");
                SetArray(data, "alert", "EnemyAlert");
                SetArray(data, "attack", "EnemyAttack");
                SetArray(data, "damage", "EnemyDamage");
                SetArray(data, "death", "EnemyDeath");
                Set(data, "sfxGroup", sfx);
                data.ApplyModifiedPropertiesWithoutUndo();
                root.GetComponent<AudioSource>().outputAudioMixerGroup = sfx;
                PrefabUtility.SaveAsPrefabAsset(root, EnemyPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void ConfigureImpact(AudioMixerGroup sfx)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(ImpactPath);
            try
            {
                AudioSource source = root.GetComponent<AudioSource>();
                if (source == null) throw new InvalidOperationException("Impact AudioSource is missing.");
                source.outputAudioMixerGroup = sfx;
                PrefabUtility.SaveAsPrefabAsset(root, ImpactPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void ConfigureScene(AudioMixer mixer, AudioMixerGroup music,
            AudioMixerGroup sfx, AudioMixerGroup ambience, AudioMixerGroup ui)
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                MusicManager manager = null;
                AudioMixSettings settings = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    if (manager == null) manager = root.GetComponentInChildren<MusicManager>(true);
                    if (settings == null) settings = root.GetComponentInChildren<AudioMixSettings>(true);
                    foreach (DangerZone zone in root.GetComponentsInChildren<DangerZone>(true))
                    {
                        AudioSource source = zone.GetComponent<AudioSource>();
                        if (source != null) source.outputAudioMixerGroup = sfx;
                    }
                }
                if (manager == null || settings == null) throw new InvalidOperationException("Scene music or mix component is missing.");
                var data = new SerializedObject(manager);
                Set(data, "exploration", Clip("ExplorationLoop"));
                Set(data, "dangerWarning", Clip("WarningLoop"));
                Set(data, "combat", Clip("CombatLoop"));
                Set(data, "facilityAmbience", Clip("FacilityAmbienceLoop"));
                Set(data, "warningCue", Clip("WarningCue"));
                Set(data, "areaClearedCue", Clip("AreaClearedCue"));
                Set(data, "extractionActiveCue", Clip("ExtractionActiveCue"));
                Set(data, "countdownStartCue", Clip("CountdownStartCue"));
                Set(data, "countdownTickCue", Clip("CountdownTickCue"));
                Set(data, "successCue", Clip("SuccessCue"));
                Set(data, "failureCue", Clip("FailureCue"));
                Set(data, "musicGroup", music);
                Set(data, "ambienceGroup", ambience);
                Set(data, "uiGroup", ui);
                data.ApplyModifiedPropertiesWithoutUndo();
                var mixData = new SerializedObject(settings);
                Set(mixData, "mixer", mixer);
                mixData.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }
    }
}

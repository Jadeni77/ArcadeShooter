using ArcadeShooter.Audio;
using ArcadeShooter.Combat;
using ArcadeShooter.Encounters;
using ArcadeShooter.Enemies;
using ArcadeShooter.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcadeShooter.Editor
{
    public static class CombatAudioVfxSetup
    {
        private const string PlayerPath = "Assets/Prefabs/FirstPersonPlayer.prefab";
        private const string EnemyPath = "Assets/Prefabs/MeleeEnemy.prefab";
        private const string ImpactPath = "Assets/Prefabs/BulletImpact.prefab";
        private const string ScenePath = "Assets/Scenes/FirstPersonTest.unity";

        [MenuItem("ArcadeShooter/Configure Combat Audio and VFX")]
        public static void Configure()
        {
            GameObject impact = AssetDatabase.LoadAssetAtPath<GameObject>(ImpactPath);
            if (impact == null) throw new System.InvalidOperationException("Bullet impact prefab is missing.");
            ConfigurePlayer();
            ConfigureEnemy(impact);
            ConfigureScene();
            AssetDatabase.SaveAssets();
            Debug.Log("Combat audio and VFX components configured.");
        }

        private static void ConfigurePlayer()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PlayerPath);
            try
            {
                PlayerFoley foley = root.GetComponent<PlayerFoley>();
                if (foley == null) foley = root.AddComponent<PlayerFoley>();
                AudioSource[] sources = root.GetComponents<AudioSource>();
                AudioSource foleySource = sources.Length > 1 ? sources[1] : root.AddComponent<AudioSource>();
                foleySource.playOnAwake = false;
                var foleyData = new SerializedObject(foley);
                foleyData.FindProperty("foleySource").objectReferenceValue = foleySource;
                foleyData.ApplyModifiedPropertiesWithoutUndo();

                RemoteWeaponPresentation remote = root.GetComponentInChildren<RemoteWeaponPresentation>(true);
                var remoteData = new SerializedObject(remote);
                if (remoteData.FindProperty("muzzleFlash").objectReferenceValue == null)
                {
                    var flash = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    flash.name = "Remote Muzzle Flash";
                    Object.DestroyImmediate(flash.GetComponent<Collider>());
                    flash.transform.SetParent(remote.transform, false);
                    flash.transform.localPosition = new Vector3(0f, 0.02f, 0.53f);
                    flash.transform.localScale = new Vector3(0.10f, 0.10f, 0.10f);
                    var localFlash = root.GetComponent<AutomaticRifle>();
                    var localData = new SerializedObject(localFlash);
                    GameObject sourceFlash = localData.FindProperty("muzzleFlash").objectReferenceValue as GameObject;
                    if (sourceFlash != null)
                        flash.GetComponent<Renderer>().sharedMaterial = sourceFlash.GetComponent<Renderer>().sharedMaterial;
                    flash.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    flash.SetActive(false);
                    remoteData.FindProperty("muzzleFlash").objectReferenceValue = flash;
                    remoteData.ApplyModifiedPropertiesWithoutUndo();
                }
                PrefabUtility.SaveAsPrefabAsset(root, PlayerPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void ConfigureEnemy(GameObject impact)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(EnemyPath);
            try
            {
                if (root.GetComponent<EnemyAudio>() == null) root.AddComponent<EnemyAudio>();
                EnemyFeedback feedback = root.GetComponent<EnemyFeedback>();
                var data = new SerializedObject(feedback);
                data.FindProperty("deathEffect").objectReferenceValue = impact;
                data.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, EnemyPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void ConfigureScene()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            bool found = false;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.GetComponent<MusicManager>() != null)
                {
                    if (root.GetComponent<AudioMixSettings>() == null) root.AddComponent<AudioMixSettings>();
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                var music = new GameObject("Adaptive Music and Ambience", typeof(MusicManager), typeof(AudioMixSettings));
                SceneManager.MoveGameObjectToScene(music, scene);
            }
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (DangerZone zone in root.GetComponentsInChildren<DangerZone>(true))
                {
                    AudioSource source = zone.GetComponent<AudioSource>();
                    if (source == null) continue;
                    source.playOnAwake = false;
                    source.spatialBlend = 1f;
                    source.minDistance = 2f;
                    source.maxDistance = 22f;
                    source.rolloffMode = AudioRolloffMode.Logarithmic;
                }
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
                    if (collider.name == "Room 1 Crate" || collider.name == "Room 2 Low Barrier" ||
                        collider.name == "Room 3 Crate")
                    {
                        SurfaceFeedback feedback = collider.GetComponent<SurfaceFeedback>();
                        if (feedback == null) feedback = collider.gameObject.AddComponent<SurfaceFeedback>();
                        var data = new SerializedObject(feedback);
                        data.FindProperty("surface").enumValueIndex = (int)ImpactSurface.Metal;
                        data.ApplyModifiedPropertiesWithoutUndo();
                    }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}

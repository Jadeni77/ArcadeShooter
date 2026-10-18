using ArcadeShooter.Combat;
using UnityEditor;
using UnityEngine;

namespace ArcadeShooter.Editor
{
    public static class WeaponVisualsSetup
    {
        private const string PlayerPath = "Assets/Prefabs/FirstPersonPlayer.prefab";

        [MenuItem("ArcadeShooter/Upgrade Weapon Visuals")]
        public static void Upgrade()
        {
            GameObject player = PrefabUtility.LoadPrefabContents(PlayerPath);
            try
            {
                AutomaticRifle rifle = player.GetComponent<AutomaticRifle>();
                Camera camera = player.GetComponentInChildren<Camera>(true);
                if (rifle == null || camera == null)
                    throw new System.InvalidOperationException("FirstPersonPlayer needs its rifle and camera.");

                WeaponAudio audio = player.GetComponent<WeaponAudio>();
                if (audio == null) audio = player.AddComponent<WeaponAudio>();
                Set(audio, "rifle", rifle);
                Set(audio, "gunfire", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Gunfire.wav"));
                Set(audio, "impact", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Impact.wav"));
                Set(audio, "reloadHandling", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Reload.wav"));

                WeaponAim aim = player.GetComponent<WeaponAim>();
                if (aim == null) aim = player.AddComponent<WeaponAim>();
                Set(aim, "aimCamera", camera);

                Transform view = camera.transform.Find("Assault Rifle Placeholder");
                if (view == null) view = camera.transform.Find("Assault Rifle View");
                if (view == null) throw new System.InvalidOperationException("First-person rifle view is missing.");
                view.name = "Assault Rifle View";
                WeaponViewModel motion = view.GetComponent<WeaponViewModel>();
                Set(motion, "aim", aim);
                Set(motion, "magazine", view.Find("Magazine"));
                if (view.GetComponent<WeaponRigAnimator>() == null)
                    view.gameObject.AddComponent<WeaponRigAnimator>();

                Material polymer = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Rifle Placeholder.mat");
                Material metal = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Rifle Metal.mat");
                AddBox(view, "Upper Receiver", new Vector3(0f, 0.085f, 0.01f), new Vector3(0.19f, 0.055f, 0.36f), metal);
                AddBox(view, "Handguard", new Vector3(0f, 0.015f, 0.29f), new Vector3(0.15f, 0.13f, 0.3f), polymer);
                AddBox(view, "Top Rail", new Vector3(0f, 0.14f, 0.15f), new Vector3(0.14f, 0.025f, 0.58f), metal);
                AddBox(view, "Rear Sight", new Vector3(0f, 0.175f, -0.11f), new Vector3(0.10f, 0.06f, 0.04f), metal);
                AddBox(view, "Front Sight", new Vector3(0f, 0.175f, 0.42f), new Vector3(0.10f, 0.06f, 0.04f), metal);
                AddBox(view, "Muzzle Brake", new Vector3(0f, 0.025f, 0.56f), new Vector3(0.11f, 0.10f, 0.08f), metal);
                AddBox(view, "Magazine Well", new Vector3(0f, -0.10f, 0.13f), new Vector3(0.14f, 0.11f, 0.19f), polymer);
                AddBox(view, "Ejection Port", new Vector3(0.112f, 0.052f, -0.005f), new Vector3(0.01f, 0.045f, 0.12f), metal);
                AddBox(view, "Charging Handle", new Vector3(0f, 0.125f, -0.19f), new Vector3(0.12f, 0.035f, 0.07f), metal);
                AddBox(view, "Trigger Guard", new Vector3(0f, -0.14f, -0.05f), new Vector3(0.11f, 0.025f, 0.16f), metal);

                Transform body = player.transform.Find("Remote Player Body");
                if (body == null) throw new System.InvalidOperationException("Remote player body is missing.");
                Transform remote = body.Find("Remote Rifle");
                if (remote == null)
                {
                    remote = new GameObject("Remote Rifle").transform;
                    remote.SetParent(body, false);
                    remote.localPosition = new Vector3(0.54f, 0.05f, 0.5f);
                    remote.localScale = new Vector3(1.4f, 1.1f, 2f);
                    remote.gameObject.AddComponent<RemoteWeaponPresentation>();
                }
                AddBox(remote, "Receiver", Vector3.zero, new Vector3(0.2f, 0.14f, 0.38f), polymer);
                AddBox(remote, "Handguard", new Vector3(0f, 0f, 0.28f), new Vector3(0.15f, 0.10f, 0.26f), polymer);
                AddBox(remote, "Barrel", new Vector3(0f, 0.02f, 0.49f), new Vector3(0.07f, 0.07f, 0.24f), metal);
                AddBox(remote, "Stock", new Vector3(0f, -0.02f, -0.26f), new Vector3(0.14f, 0.1f, 0.22f), polymer);
                AddBox(remote, "Magazine", new Vector3(0f, -0.16f, 0.1f), new Vector3(0.1f, 0.2f, 0.12f), metal);
                AddBox(remote, "Sight", new Vector3(0f, 0.11f, 0.04f), new Vector3(0.07f, 0.07f, 0.12f), metal);

                PrefabUtility.SaveAsPrefabAsset(player, PlayerPath);
                Debug.Log("Upgraded first-person and remote rifle presentations.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(player);
            }
        }

        private static void Set(Object target, string name, Object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty(name).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddBox(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            if (parent.Find(name) != null) return;
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = position;
            box.transform.localScale = scale;
            Object.DestroyImmediate(box.GetComponent<Collider>());
            box.GetComponent<Renderer>().sharedMaterial = material;
        }
    }
}

using ArcadeShooter.Combat;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ArcadeShooter.Editor.Tests
{
    public sealed class WeaponPresentationTests
    {
        [Test]
        public void PlayerPrefabKeepsLocalAndRemotePresentationsSeparate()
        {
            GameObject player = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/FirstPersonPlayer.prefab");
            Assert.That(player, Is.Not.Null);
            Camera camera = player.GetComponentInChildren<Camera>(true);
            Assert.That(camera, Is.Not.Null);
            Assert.That(camera.transform.Find("Assault Rifle View"), Is.Not.Null);
            Assert.That(camera.GetComponentInChildren<WeaponViewModel>(true), Is.Not.Null);
            Assert.That(player.GetComponent<WeaponAim>(), Is.Not.Null);
            Assert.That(player.GetComponent<WeaponAudio>(), Is.Not.Null);

            Transform remoteBody = player.transform.Find("Remote Player Body");
            Assert.That(remoteBody, Is.Not.Null);
            Assert.That(remoteBody.Find("Remote Rifle"), Is.Not.Null);
            Assert.That(remoteBody.GetComponentInChildren<RemoteWeaponPresentation>(true), Is.Not.Null);
            Assert.That(remoteBody.GetComponentInChildren<Camera>(true), Is.Null);
        }

        [Test]
        public void AimIsBoundToRightMouseInPlayerActions()
        {
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                "Assets/InputSystem_Actions.inputactions");
            Assert.That(actions, Is.Not.Null);
            InputAction aim = actions.FindAction("Player/Aim", true);
            Assert.That(aim.bindings.Count, Is.GreaterThan(0));
            Assert.That(aim.bindings[0].effectivePath, Is.EqualTo("<Mouse>/rightButton"));
        }
    }
}

using ArcadeShooter.Encounters;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ArcadeShooter.Editor.Tests
{
    public sealed class EnvironmentVisualsTests
    {
        [Test]
        public void FacilityVisualsPreserveRouteAndGameplayObjects()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/FirstPersonTest.unity");
            GameObject visuals = GameObject.Find("Environment Visuals");
            GameObject gameplay = GameObject.Find("Three Room Graybox");
            Assert.That(visuals, Is.Not.Null);
            Assert.That(gameplay, Is.Not.Null);
            Assert.That(visuals.transform.IsChildOf(gameplay.transform), Is.False);
            Assert.That(visuals.GetComponentsInChildren<Collider>(true), Is.Empty);
            Assert.That(visuals.GetComponentsInChildren<Renderer>(true).Length, Is.GreaterThan(100));
            Assert.That(GameObject.Find("Player Spawn Point").transform.position.x,
                Is.EqualTo(-10).Within(0.01));
            Assert.That(GameObject.Find("Extraction Point"), Is.Not.Null);
            Assert.That(GameObject.Find("Research Sample"), Is.Not.Null);
            Assert.That(GameObject.Find("Danger Zone A").GetComponent<DangerZone>(), Is.Not.Null);
            Assert.That(GameObject.Find("Danger Zone B").GetComponent<DangerZone>(), Is.Not.Null);
            Assert.That(GameObject.Find("Room 1 Crate").GetComponent<BoxCollider>(), Is.Not.Null);
            Assert.That(GameObject.Find("Room 3 Crate").GetComponent<BoxCollider>(), Is.Not.Null);
            Assert.That(GameObject.Find("Room 2 Low Barrier").GetComponent<BoxCollider>(),
                Is.Not.Null);
            Assert.That(GameObject.Find("Room 1 Crate").GetComponent<Renderer>().enabled, Is.False);
            var route = new[] { new Vector3(-10, 0, 0), new Vector3(0, 0, 0),
                new Vector3(10, 0, 0) };
            for (int i = 0; i < 2; i++)
            {
                var path = new NavMeshPath();
                Assert.That(NavMesh.CalculatePath(route[i], route[i + 1], NavMesh.AllAreas, path),
                    Is.True, "NavMesh query " + i);
                Assert.That(path.status, Is.EqualTo(NavMeshPathStatus.PathComplete));
            }
            Assert.That(GameObject.Find("Facility Atmosphere Volume").GetComponent<Volume>()
                .sharedProfile, Is.Not.Null);
        }

        [Test]
        public void PresentationsUseUrpWithoutChangingEnemyCollision()
        {
            GameObject player = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/FirstPersonPlayer.prefab");
            Camera camera = player.GetComponentInChildren<Camera>(true);
            Assert.That(camera.GetComponent<UniversalAdditionalCameraData>().renderPostProcessing,
                Is.True);
            GameObject enemy = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/MeleeEnemy.prefab");
            Assert.That(enemy.GetComponent<NavMeshAgent>().radius, Is.EqualTo(0.35f).Within(0.001f));
            Assert.That(enemy.GetComponent<NavMeshAgent>().height, Is.EqualTo(1.8f).Within(0.001f));
            CapsuleCollider hit = enemy.transform.Find("Body and Hit Collider")
                .GetComponent<CapsuleCollider>();
            Assert.That(hit.radius, Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(hit.height, Is.EqualTo(2f).Within(0.001f));
            Renderer body = enemy.transform.Find("Body and Hit Collider").GetComponent<Renderer>();
            Assert.That(body.sharedMaterial.shader.name, Does.StartWith("Universal Render Pipeline/"));
        }
    }
}

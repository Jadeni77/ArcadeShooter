using System.Collections.Generic;
using ArcadeShooter.Encounters;
using NUnit.Framework;
using UnityEngine;

namespace ArcadeShooter.Editor.Tests
{
    public sealed class SpawnPlannerTests
    {
        private static readonly Vector3[] Markers = { new Vector3(0, 0, 0), new Vector3(10, 0, 0), new Vector3(20, 0, 0) };
        private static readonly Vector3[] FarPlayer = { new Vector3(0, 1, 30) };

        private static bool OpenGround(Vector3 marker, Vector3 candidate, out Vector3 ground)
        {
            ground = new Vector3(candidate.x, 0f, candidate.z);
            return true;
        }

        private static float Flat(Vector3 a, Vector3 b) => new Vector2(a.x - b.x, a.z - b.z).magnitude;

        [Test]
        public void UsesEachMarkerWhenNothingBlocksIt()
        {
            var result = new List<Vector3>();
            Assert.That(SpawnPlanner.TryPlan(3, Markers, 0, FarPlayer, 2f, 0.8f, OpenGround, result), Is.True);
            Assert.That(result, Is.EqualTo(new[] { Markers[0], Markers[1], Markers[2] }));
        }

        [Test]
        public void StartsFromTheRequestedMarkerAndWraps()
        {
            var result = new List<Vector3>();
            Assert.That(SpawnPlanner.TryPlan(2, Markers, 2, FarPlayer, 2f, 0.8f, OpenGround, result), Is.True);
            Assert.That(result, Is.EqualTo(new[] { Markers[2], Markers[0] }));
        }

        [Test]
        public void MovesASpawnAwayFromAPlayerStandingOnItsMarker()
        {
            var result = new List<Vector3>();
            var player = new[] { new Vector3(0, 1, 0) };
            Assert.That(SpawnPlanner.TryPlan(3, Markers, 0, player, 2f, 0.8f, OpenGround, result), Is.True);
            Assert.That(Flat(result[0], player[0]), Is.GreaterThanOrEqualTo(2f));
            Assert.That(Flat(result[0], Markers[0]), Is.LessThanOrEqualTo(2.5f + 1e-4f), "stays near its own marker");
            Assert.That(result[1], Is.EqualTo(Markers[1]));
            Assert.That(result[2], Is.EqualTo(Markers[2]));
        }

        [Test]
        public void KeepsEverySpawnAwayFromEveryPlayer()
        {
            var result = new List<Vector3>();
            var players = new[] { new Vector3(10.5f, 1, 0), new Vector3(20, 1, 0.5f) };
            Assert.That(SpawnPlanner.TryPlan(3, Markers, 0, players, 2f, 0.8f, OpenGround, result), Is.True);
            foreach (Vector3 spawn in result)
                foreach (Vector3 player in players)
                    Assert.That(Flat(spawn, player), Is.GreaterThanOrEqualTo(2f));
        }

        [Test]
        public void BorrowsSpaceNearAnotherMarkerWhenAMarkerIsUnusable()
        {
            bool AwayFromFirstMarker(Vector3 marker, Vector3 candidate, out Vector3 ground)
            {
                ground = new Vector3(candidate.x, 0f, candidate.z);
                return Flat(ground, Markers[0]) > 3f;
            }

            var result = new List<Vector3>();
            Assert.That(SpawnPlanner.TryPlan(3, Markers, 0, FarPlayer, 2f, 0.8f, AwayFromFirstMarker, result), Is.True);
            Assert.That(result.Count, Is.EqualTo(3));
            for (int i = 0; i < result.Count; i++)
            {
                Assert.That(Flat(result[i], Markers[0]), Is.GreaterThan(3f));
                for (int j = i + 1; j < result.Count; j++)
                    Assert.That(Flat(result[i], result[j]), Is.GreaterThanOrEqualTo(0.8f));
            }
        }

        [Test]
        public void FailsWhenThereIsNoUsableSpace()
        {
            bool Blocked(Vector3 marker, Vector3 candidate, out Vector3 ground)
            {
                ground = default;
                return false;
            }

            Assert.That(SpawnPlanner.TryPlan(3, Markers, 0, FarPlayer, 2f, 0.8f, Blocked, new List<Vector3>()), Is.False);
        }
    }
}

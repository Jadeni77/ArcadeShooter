using System;
using System.Collections.Generic;
using ArcadeShooter.Enemies;
using ArcadeShooter.Missions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace ArcadeShooter.Encounters
{
    public sealed class EnemyWaveSpawner : MonoBehaviour
    {
        [Serializable]
        public sealed class Wave
        {
            [Min(1)] public int enemyCount = 1;
            [Min(0f)] public float delayBeforeWave = 2f;
        }

        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Transform[] spawnPoints = Array.Empty<Transform>();
        [SerializeField] private Wave[] waves = Array.Empty<Wave>();
        [SerializeField] private Wave[] alternateWaves = Array.Empty<Wave>();
        [SerializeField, Min(0)] private int alternateSpawnOffset = 1;
        [SerializeField, Min(0.5f)] private float minimumPlayerDistance = 2f;
        [SerializeField, Min(0.1f)] private float navigationSampleRadius = 0.8f;

        private readonly HashSet<EnemyHealth> livingEnemies = new HashSet<EnemyHealth>();
        private int activeWave = -1;

        private Wave[] ActiveWaves => MissionController.Instance != null &&
            MissionController.Instance.EncounterVariant == 1 && alternateWaves != null && alternateWaves.Length > 0
                ? alternateWaves : waves;
        public int WaveCount => ActiveWaves?.Length ?? 0;
        public int RemainingEnemies => livingEnemies.Count;
        public event Action<int> RemainingChanged;
        public event Action WaveCompleted;

        public float DelayBeforeWave(int index) => ActiveWaves[index].delayBeforeWave;

        // Configuration problems cannot fix themselves, so they are reported once and not retried.
        public bool IsWaveConfigured(int index)
        {
            Wave[] active = ActiveWaves;
            bool valid = index >= 0 && index < WaveCount && active[index] != null && active[index].enemyCount >= 1 &&
                         enemyPrefab != null && !enemyPrefab.scene.IsValid() &&
                         enemyPrefab.GetComponent<NetworkObject>() != null &&
                         enemyPrefab.GetComponent<EnemyHealth>() != null &&
                         enemyPrefab.GetComponent<MeleeEnemy>() != null &&
                         enemyPrefab.GetComponent<NavMeshAgent>() != null &&
                         spawnPoints != null && Array.Exists(spawnPoints, point => point != null);
            if (!valid) Debug.LogError($"{name}: wave {index + 1} has an invalid configuration.", this);
            return valid;
        }

        // Returns false without logging when players or bodies leave no room yet; the caller retries.
        public bool TrySpawnWave(int index, IReadOnlyList<Vector3> playerPositions)
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer ||
                (MissionController.Instance != null && !MissionController.Instance.IsRunning) ||
                activeWave >= 0 || index < 0 || index >= WaveCount || ActiveWaves[index] == null ||
                enemyPrefab == null || spawnPoints == null) return false;

            Wave[] active = ActiveWaves;
            int count = active[index].enemyCount;
            int firstPoint = MissionController.Instance != null && MissionController.Instance.EncounterVariant == 1
                ? alternateSpawnOffset : 0;
            for (int previous = 0; previous < index; previous++)
                if (active[previous] != null) firstPoint += Mathf.Max(0, active[previous].enemyCount);
            var markers = new List<Vector3>(spawnPoints.Length);
            foreach (Transform point in spawnPoints)
                if (point != null) markers.Add(point.position);
            var positions = new List<Vector3>(count);
            if (!SpawnPlanner.TryPlan(count, markers, firstPoint, playerPositions, minimumPlayerDistance, 0.8f,
                    ResolveSpawn, positions)) return false;

            activeWave = index;
            foreach (Vector3 position in positions)
            {
                GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
                enemy.GetComponent<NetworkObject>().Spawn(true);
                EnemyHealth health = enemy.GetComponent<EnemyHealth>();
                livingEnemies.Add(health);
                health.Died += OnEnemyDied;
            }
            RemainingChanged?.Invoke(livingEnemies.Count);
            return true;
        }

        // A candidate must be on the NavMesh, reachable from its marker without crossing a wall,
        // and leave room for an enemy body. Triggers (including the zone) are ignored.
        private bool ResolveSpawn(Vector3 marker, Vector3 candidate, out Vector3 ground)
        {
            ground = default;
            if (!NavMesh.SamplePosition(marker, out NavMeshHit markerHit, navigationSampleRadius, NavMesh.AllAreas) ||
                !NavMesh.SamplePosition(candidate, out NavMeshHit hit, navigationSampleRadius, NavMesh.AllAreas) ||
                NavMesh.Raycast(markerHit.position, hit.position, out _, NavMesh.AllAreas)) return false;
            Vector3 bottom = hit.position + Vector3.up * 0.4f;
            Vector3 top = hit.position + Vector3.up * 1.4f;
            if (Physics.CheckCapsule(bottom, top, 0.38f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                return false;
            ground = hit.position;
            return true;
        }

        public void StopTracking()
        {
            foreach (EnemyHealth health in livingEnemies)
                if (health != null) health.Died -= OnEnemyDied;
            livingEnemies.Clear();
            activeWave = -1;
        }

        private void OnEnemyDied()
        {
            // The event has no sender, so only inspect the small set owned by this spawner.
            EnemyHealth dead = null;
            foreach (EnemyHealth health in livingEnemies)
                if (health != null && health.IsDead) { dead = health; break; }
            if (dead == null) return;
            dead.Died -= OnEnemyDied;
            livingEnemies.Remove(dead);
            RemainingChanged?.Invoke(livingEnemies.Count);
            if (livingEnemies.Count != 0) return;
            activeWave = -1;
            WaveCompleted?.Invoke();
        }

        private void OnDestroy() => StopTracking();
    }
}

using System;
using System.Collections.Generic;
using ArcadeShooter.Enemies;
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
        [SerializeField, Min(0.5f)] private float minimumPlayerDistance = 2f;
        [SerializeField, Min(0.1f)] private float navigationSampleRadius = 0.8f;

        private readonly HashSet<EnemyHealth> livingEnemies = new HashSet<EnemyHealth>();
        private int activeWave = -1;

        public int WaveCount => waves?.Length ?? 0;
        public int RemainingEnemies => livingEnemies.Count;
        public event Action<int> RemainingChanged;
        public event Action WaveCompleted;

        public float DelayBeforeWave(int index) => waves[index].delayBeforeWave;

        public bool TrySpawnWave(int index, Transform player)
        {
            if (activeWave >= 0 || index < 0 || index >= WaveCount || player == null ||
                enemyPrefab == null || enemyPrefab.scene.IsValid() ||
                enemyPrefab.GetComponent<EnemyHealth>() == null ||
                enemyPrefab.GetComponent<MeleeEnemy>() == null ||
                enemyPrefab.GetComponent<NavMeshAgent>() == null || waves[index] == null ||
                waves[index].enemyCount < 1 || spawnPoints == null)
            {
                Debug.LogError($"{name}: invalid wave configuration or a wave is already active.", this);
                return false;
            }

            int count = waves[index].enemyCount;
            var positions = new List<Vector3>(count);
            int firstPoint = 0;
            for (int previous = 0; previous < index; previous++)
                if (waves[previous] != null) firstPoint += Mathf.Max(0, waves[previous].enemyCount);
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (positions.Count == count) break;
                Transform point = spawnPoints[(firstPoint + i) % spawnPoints.Length];
                if (point == null || !NavMesh.SamplePosition(point.position, out NavMeshHit hit,
                        navigationSampleRadius, NavMesh.AllAreas)) continue;
                Vector3 position = hit.position;
                if ((position - player.position).sqrMagnitude < minimumPlayerDistance * minimumPlayerDistance)
                    continue;
                // Check the agent's standing space. Triggers (including the zone) are ignored.
                Vector3 bottom = position + Vector3.up * 0.4f;
                Vector3 top = position + Vector3.up * 1.4f;
                if (Physics.CheckCapsule(bottom, top, 0.38f, Physics.DefaultRaycastLayers,
                        QueryTriggerInteraction.Ignore)) continue;
                bool overlapsSelected = false;
                foreach (Vector3 selected in positions)
                    if ((selected - position).sqrMagnitude < 0.8f * 0.8f) overlapsSelected = true;
                if (!overlapsSelected) positions.Add(position);
            }

            if (positions.Count != count)
            {
                Debug.LogError($"{name}: wave {index + 1} needs {count} clear NavMesh spawn points; found {positions.Count}.", this);
                return false;
            }

            activeWave = index;
            foreach (Vector3 position in positions)
            {
                GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity, transform);
                EnemyHealth health = enemy.GetComponent<EnemyHealth>();
                livingEnemies.Add(health);
                health.Died += OnEnemyDied;
            }
            RemainingChanged?.Invoke(livingEnemies.Count);
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

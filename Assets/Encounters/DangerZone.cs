using System;
using System.Collections;
using System.Collections.Generic;
using ArcadeShooter.Player;
using ArcadeShooter.Missions;
using Unity.Netcode;
using UnityEngine;

namespace ArcadeShooter.Encounters
{
    [RequireComponent(typeof(BoxCollider), typeof(EnemyWaveSpawner))]
    public sealed class DangerZone : NetworkBehaviour
    {
        public enum ZoneState { Inactive, Warning, Active, Cleared, Failed }

        [SerializeField] private string zoneName = "Danger Zone";
        [SerializeField] private string warningMessage = "HOSTILES INCOMING!";
        [SerializeField, Min(0f)] private float warningDuration = 2f;
        [Tooltip("How long a wave keeps waiting for spawn room (players or bodies on its markers) before the encounter fails.")]
        [SerializeField, Min(0f)] private float spawnRetrySeconds = 8f;
        [SerializeField] private AudioClip warningSound;
        [SerializeField] private AudioClip clearSound;
        [SerializeField] private AudioSource audioSource;

        private EnemyWaveSpawner spawner;
        private Coroutine pendingWave;
        private readonly NetworkVariable<int> state = new NetworkVariable<int>((int)ZoneState.Inactive);
        private readonly NetworkVariable<int> currentWave = new NetworkVariable<int>();
        private readonly NetworkVariable<int> remainingEnemies = new NetworkVariable<int>();

        public string ZoneName => zoneName;
        public string WarningMessage => warningMessage;
        public int CurrentWave => currentWave.Value;
        public int TotalWaves => spawner != null ? spawner.WaveCount : 0;
        public int RemainingEnemies => remainingEnemies.Value;
        public ZoneState State => (ZoneState)state.Value;
        public bool IsStopped => State == ZoneState.Failed;
        public event Action<DangerZone> Changed;

        private void Awake()
        {
            spawner = GetComponent<EnemyWaveSpawner>();
            GetComponent<BoxCollider>().isTrigger = true;
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            spawner.RemainingChanged += OnRemainingChanged;
            spawner.WaveCompleted += OnWaveCompleted;
        }

        private void OnDisable()
        {
            spawner.RemainingChanged -= OnRemainingChanged;
            spawner.WaveCompleted -= OnWaveCompleted;
            if (pendingWave != null) StopCoroutine(pendingWave);
            pendingWave = null;
            if (spawner != null) spawner.StopTracking();
        }

        public override void OnNetworkSpawn()
        {
            state.OnValueChanged += OnStateChanged;
            currentWave.OnValueChanged += OnIntegerChanged;
            remainingEnemies.OnValueChanged += OnIntegerChanged;
            Changed?.Invoke(this);
        }

        public override void OnNetworkDespawn()
        {
            state.OnValueChanged -= OnStateChanged;
            currentWave.OnValueChanged -= OnIntegerChanged;
            remainingEnemies.OnValueChanged -= OnIntegerChanged;
        }

        private void OnStateChanged(int previous, int current) => Changed?.Invoke(this);
        private void OnIntegerChanged(int previous, int current) => Changed?.Invoke(this);

        private static bool MissionAllowsProgress => MissionController.Instance == null || MissionController.Instance.IsRunning;

        private void Update()
        {
            if (IsServer && !MissionAllowsProgress)
            {
                if (pendingWave != null) StopCoroutine(pendingWave);
                pendingWave = null;
                spawner.StopTracking();
                return;
            }
            if (!IsSpawned || !IsServer ||
                (State != ZoneState.Warning && State != ZoneState.Active)) return;
            if (FindLivingPlayer() != null) return;
            state.Value = (int)ZoneState.Failed;
            if (pendingWave != null) StopCoroutine(pendingWave);
            pendingWave = null;
            spawner.StopTracking();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsSpawned || !IsServer || !MissionAllowsProgress || State != ZoneState.Inactive ||
                !(other is CharacterController)) return;
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player == null || player.IsDead || !player.GetComponent<ArcadeShooter.Networking.NetworkPlayer>().InMatch) return;
            Activate();
        }

        private void Activate()
        {
            state.Value = (int)ZoneState.Warning;
            Play(warningSound);
            pendingWave = StartCoroutine(StartAfterDelay(warningDuration, 0));
        }

        private IEnumerator StartAfterDelay(float delay, int waveIndex)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            if (!CanContinueWave()) { pendingWave = null; yield break; }
            if (!spawner.IsWaveConfigured(waveIndex))
            {
                pendingWave = null;
                StopEncounter($"wave {waveIndex + 1} is not configured");
                yield break;
            }
            // A player or body standing on the spawn markers only delays the wave.
            float giveUpAt = Time.time + spawnRetrySeconds;
            while (true)
            {
                if (!CanContinueWave()) { pendingWave = null; yield break; }
                if (spawner.TrySpawnWave(waveIndex, LivingPlayerPositions())) break;
                if (Time.time >= giveUpAt)
                {
                    pendingWave = null;
                    StopEncounter($"wave {waveIndex + 1} found no room to spawn for {spawnRetrySeconds:0.#} s");
                    yield break;
                }
                yield return new WaitForSeconds(0.5f);
            }
            pendingWave = null;
            currentWave.Value = waveIndex + 1;
            state.Value = (int)ZoneState.Active;
        }

        // Update() stops the encounter itself when the mission ends or nobody is left alive.
        private bool CanContinueWave() => MissionAllowsProgress && State != ZoneState.Failed &&
                                          State != ZoneState.Cleared && FindLivingPlayer() != null;

        private void StopEncounter(string reason)
        {
            state.Value = (int)ZoneState.Failed;
            Debug.LogError($"{name}: encounter stopped because {reason}.", this);
        }

        private void OnRemainingChanged(int count)
        {
            if (IsServer && MissionAllowsProgress) remainingEnemies.Value = count;
        }

        private void OnWaveCompleted()
        {
            if (!IsServer || !MissionAllowsProgress || State != ZoneState.Active) return;
            if (CurrentWave >= spawner.WaveCount)
            {
                state.Value = (int)ZoneState.Cleared;
                Play(clearSound);
                return;
            }
            pendingWave = StartCoroutine(StartAfterDelay(spawner.DelayBeforeWave(CurrentWave), CurrentWave));
        }

        private static PlayerHealth FindLivingPlayer()
        {
            foreach (PlayerHealth player in FindObjectsByType<PlayerHealth>())
                if (!player.IsDead && player.GetComponent<ArcadeShooter.Networking.NetworkPlayer>().InMatch)
                    return player;
            return null;
        }

        private static List<Vector3> LivingPlayerPositions()
        {
            var positions = new List<Vector3>();
            foreach (PlayerHealth player in FindObjectsByType<PlayerHealth>())
                if (!player.IsDead && player.GetComponent<ArcadeShooter.Networking.NetworkPlayer>().InMatch)
                    positions.Add(player.transform.position);
            return positions;
        }

        private void Play(AudioClip clip)
        {
            if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);
        }

        private void OnDrawGizmos()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box == null) return;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = State == ZoneState.Cleared ? Color.green : new Color(1f, 0.3f, 0.1f, 0.8f);
            Gizmos.DrawWireCube(box.center, box.size);
        }

        private void OnValidate()
        {
            warningDuration = Mathf.Max(0f, warningDuration);
            spawnRetrySeconds = Mathf.Max(0f, spawnRetrySeconds);
        }
    }
}

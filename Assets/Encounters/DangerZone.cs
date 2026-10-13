using System;
using System.Collections;
using ArcadeShooter.Player;
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

        private void Update()
        {
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
            if (!IsSpawned || !IsServer || State != ZoneState.Inactive ||
                !(other is CharacterController)) return;
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player == null || player.IsDead) return;
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
            pendingWave = null;
            if (State == ZoneState.Failed || State == ZoneState.Cleared) yield break;
            PlayerHealth player = FindLivingPlayer();
            if (player == null || !spawner.TrySpawnWave(waveIndex, player.transform))
            {
                state.Value = (int)ZoneState.Failed;
                Debug.LogError($"{name}: encounter stopped because wave {waveIndex + 1} could not spawn.", this);
                yield break;
            }
            currentWave.Value = waveIndex + 1;
            state.Value = (int)ZoneState.Active;
        }

        private void OnRemainingChanged(int count)
        {
            if (IsServer) remainingEnemies.Value = count;
        }

        private void OnWaveCompleted()
        {
            if (!IsServer || State != ZoneState.Active) return;
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

        private void OnValidate() => warningDuration = Mathf.Max(0f, warningDuration);
    }
}

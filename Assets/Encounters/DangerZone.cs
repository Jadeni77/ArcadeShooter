using System;
using System.Collections;
using ArcadeShooter.Player;
using UnityEngine;

namespace ArcadeShooter.Encounters
{
    [RequireComponent(typeof(BoxCollider), typeof(EnemyWaveSpawner))]
    public sealed class DangerZone : MonoBehaviour
    {
        public enum ZoneState { Inactive, Warning, Active, Cleared }

        [SerializeField] private string zoneName = "Danger Zone";
        [SerializeField] private string warningMessage = "HOSTILES INCOMING!";
        [SerializeField, Min(0f)] private float warningDuration = 2f;
        [SerializeField] private AudioClip warningSound;
        [SerializeField] private AudioClip clearSound;
        [SerializeField] private AudioSource audioSource;

        private EnemyWaveSpawner spawner;
        private PlayerHealth activatingPlayer;
        private Coroutine pendingWave;
        private bool progressionStopped;
        private int currentWave;

        public string ZoneName => zoneName;
        public string WarningMessage => warningMessage;
        public int CurrentWave => currentWave;
        public int TotalWaves => spawner != null ? spawner.WaveCount : 0;
        public int RemainingEnemies => spawner != null ? spawner.RemainingEnemies : 0;
        public ZoneState State { get; private set; } = ZoneState.Inactive;
        public bool IsStopped => progressionStopped;
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
            if (activatingPlayer != null) activatingPlayer.Died -= OnPlayerDied;
            if (pendingWave != null) StopCoroutine(pendingWave);
            pendingWave = null;
            spawner.StopTracking();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (State != ZoneState.Inactive || !(other is CharacterController)) return;
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player == null || player.IsDead) return;
            Activate(player);
        }

        private void Activate(PlayerHealth player)
        {
            activatingPlayer = player;
            player.Died += OnPlayerDied;
            State = ZoneState.Warning;
            Changed?.Invoke(this);
            Play(warningSound);
            pendingWave = StartCoroutine(StartAfterDelay(warningDuration, 0));
        }

        private IEnumerator StartAfterDelay(float delay, int waveIndex)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            pendingWave = null;
            if (progressionStopped || State == ZoneState.Cleared) yield break;
            if (!spawner.TrySpawnWave(waveIndex, activatingPlayer.transform))
            {
                progressionStopped = true;
                Debug.LogError($"{name}: encounter stopped because wave {waveIndex + 1} could not spawn.", this);
                Changed?.Invoke(this);
                yield break;
            }
            currentWave = waveIndex + 1;
            State = ZoneState.Active;
            Changed?.Invoke(this);
        }

        private void OnRemainingChanged(int count)
        {
            if (!progressionStopped) Changed?.Invoke(this);
        }

        private void OnWaveCompleted()
        {
            if (progressionStopped || State != ZoneState.Active) return;
            if (currentWave >= spawner.WaveCount)
            {
                State = ZoneState.Cleared;
                activatingPlayer.Died -= OnPlayerDied;
                Play(clearSound);
                Changed?.Invoke(this);
                return;
            }
            pendingWave = StartCoroutine(StartAfterDelay(spawner.DelayBeforeWave(currentWave), currentWave));
            Changed?.Invoke(this);
        }

        private void OnPlayerDied()
        {
            progressionStopped = true;
            if (pendingWave != null) StopCoroutine(pendingWave);
            pendingWave = null;
            spawner.StopTracking();
            Changed?.Invoke(this);
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

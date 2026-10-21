using UnityEngine;
using UnityEngine.Audio;

namespace ArcadeShooter.Enemies
{
    // Cosmetic playback follows replicated health and server-broadcast attack/alert events.
    [RequireComponent(typeof(EnemyHealth), typeof(AudioSource))]
    public sealed class EnemyAudio : MonoBehaviour
    {
        [SerializeField] private AudioClip[] idle;
        [SerializeField] private AudioClip[] alert;
        [SerializeField] private AudioClip[] attack;
        [SerializeField] private AudioClip[] damage;
        [SerializeField] private AudioClip[] death;
        [SerializeField, Range(0f, 1f)] private float level = 0.65f;
        [SerializeField, Min(1f)] private float audibleDistance = 22f;
        [SerializeField] private AudioMixerGroup sfxGroup;

        private static float budgetWindow;
        private static int soundsInWindow;
        private EnemyHealth health;
        private AudioSource source;
        private float nextIdleAt;
        private float nextAlertAt;

        private void Awake()
        {
            health = GetComponent<EnemyHealth>();
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.minDistance = 2f;
            source.maxDistance = audibleDistance;
            if (sfxGroup != null) source.outputAudioMixerGroup = sfxGroup;
            nextIdleAt = Time.time + Random.Range(5f, 11f);
        }

        private void OnEnable()
        {
            health.Damaged += OnDamaged;
            health.Died += OnDied;
            health.AttackPresented += OnAttack;
            health.AlertPresented += OnAlert;
        }

        private void OnDisable()
        {
            health.Damaged -= OnDamaged;
            health.Died -= OnDied;
            health.AttackPresented -= OnAttack;
            health.AlertPresented -= OnAlert;
            source.Stop();
        }

        private void Update()
        {
            if (health.IsDead || Time.time < nextIdleAt) return;
            nextIdleAt = Time.time + Random.Range(8f, 15f);
            Play(idle);
        }

        private void OnDamaged() { if (!health.IsDead) Play(damage); }
        private void OnAttack() { if (!health.IsDead) Play(attack); }
        private void OnAlert()
        {
            if (health.IsDead || Time.time < nextAlertAt) return;
            nextAlertAt = Time.time + 3f;
            Play(alert);
        }

        private void OnDied()
        {
            source.Stop();
            Play(death);
        }

        private void Play(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0) return;
            Camera listenerCamera = Camera.main;
            if (listenerCamera == null || (listenerCamera.transform.position - transform.position).sqrMagnitude >
                audibleDistance * audibleDistance) return;
            if (Time.time - budgetWindow >= 0.1f) { budgetWindow = Time.time; soundsInWindow = 0; }
            if (soundsInWindow >= 5) return;
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            if (clip == null) return;
            soundsInWindow++;
            source.pitch = Random.Range(0.96f, 1.04f);
            source.PlayOneShot(clip, level);
        }
    }
}

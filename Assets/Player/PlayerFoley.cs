using ArcadeShooter.Networking;
using UnityEngine;
using UnityEngine.Audio;

namespace ArcadeShooter.Player
{
    [RequireComponent(typeof(NetworkPlayer), typeof(PlayerHealth))]
    public sealed class PlayerFoley : MonoBehaviour
    {
        [SerializeField] private AudioClip[] footsteps;
        [SerializeField] private AudioClip jump;
        [SerializeField] private AudioClip land;
        [SerializeField] private AudioClip damaged;
        [SerializeField, Range(0f, 1f)] private float level = 0.5f;
        [SerializeField, Min(0.1f)] private float walkStepSeconds = 0.43f;
        [SerializeField, Min(0.1f)] private float sprintStepSeconds = 0.29f;
        [SerializeField] private AudioSource foleySource;
        [SerializeField] private AudioMixerGroup sfxGroup;

        private NetworkPlayer player;
        private PlayerHealth health;
        private FirstPersonMovement movement;
        private AudioSource source;
        private Vector3 lastPosition;
        private float lastHealth;
        private float nextStepAt;

        private void Awake()
        {
            player = GetComponent<NetworkPlayer>();
            health = GetComponent<PlayerHealth>();
            movement = GetComponent<FirstPersonMovement>();
            source = foleySource != null ? foleySource : GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.maxDistance = 18f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            if (sfxGroup != null) source.outputAudioMixerGroup = sfxGroup;
            lastPosition = transform.position;
            lastHealth = health.Health;
        }

        private void OnEnable()
        {
            movement.Jumped += OnJumped;
            movement.Landed += OnLanded;
            health.HealthChanged += OnHealthChanged;
        }

        private void OnDisable()
        {
            movement.Jumped -= OnJumped;
            movement.Landed -= OnLanded;
            health.HealthChanged -= OnHealthChanged;
            source.Stop();
        }

        private void Update()
        {
            Vector3 displacement = transform.position - lastPosition;
            lastPosition = transform.position;
            if (!player.InMatch || health.IsDead) return;
            source.spatialBlend = player.IsOwner ? 0f : 1f;
            float speed = player.IsOwner ? movement.HorizontalSpeed :
                new Vector2(displacement.x, displacement.z).magnitude / Mathf.Max(Time.deltaTime, 0.001f);
            bool grounded = player.IsOwner ? movement.Grounded :
                Physics.Raycast(transform.position + Vector3.up * 0.15f, Vector3.down, 0.4f,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            if (!grounded || speed < 0.6f || Time.time < nextStepAt) return;
            bool sprinting = player.IsOwner ? movement.Sprinting : speed > 6.4f;
            nextStepAt = Time.time + (sprinting ? sprintStepSeconds : walkStepSeconds);
            if (footsteps != null && footsteps.Length > 0)
                Play(footsteps[Random.Range(0, footsteps.Length)]);
        }

        private void OnJumped() { if (player.IsOwner && player.InMatch) Play(jump); }
        private void OnLanded() { if (player.IsOwner && player.InMatch) Play(land); }
        private void OnHealthChanged(float current, float maximum)
        {
            if (player.IsOwner && player.InMatch && current < lastHealth) Play(damaged);
            lastHealth = current;
        }

        private void Play(AudioClip clip)
        {
            if (clip != null) source.PlayOneShot(clip, level);
        }
    }
}

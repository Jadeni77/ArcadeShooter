using UnityEngine;
using UnityEngine.Audio;

namespace ArcadeShooter.Combat
{
    // Optional sounds follow presentation state. They never advance ammunition or reload time.
    [RequireComponent(typeof(AudioSource))]
    public sealed class WeaponAudio : MonoBehaviour
    {
        [SerializeField] private AutomaticRifle rifle;
        [SerializeField] private AudioClip gunfire;
        [SerializeField] private AudioClip[] gunfireVariants;
        [SerializeField] private AudioClip impact;
        [SerializeField] private AudioClip reloadHandling;
        [SerializeField] private AudioClip magazineOut;
        [SerializeField] private AudioClip magazineIn;
        [SerializeField] private AudioClip empty;
        [SerializeField] private AudioMixerGroup sfxGroup;
        [SerializeField, Range(0f, 1f)] private float gunfireLevel = 0.75f;
        [SerializeField, Range(0f, 1f)] private float handlingLevel = 0.65f;
        [SerializeField, Range(0f, 1f)] private float impactLevel = 0.45f;
        [SerializeField, Range(0f, 0.15f)] private float gunfirePitchVariation = 0.035f;

        private AudioSource source;
        private bool outPlayed;
        private bool inPlayed;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            if (sfxGroup != null) source.outputAudioMixerGroup = sfxGroup;
            if (rifle == null) rifle = GetComponent<AutomaticRifle>();
        }

        private void OnEnable()
        {
            if (rifle != null) rifle.ReloadStarted += OnReloadStarted;
        }

        private void OnDisable()
        {
            if (rifle != null) rifle.ReloadStarted -= OnReloadStarted;
        }

        private void Update()
        {
            if (rifle == null || !rifle.IsReloading) return;
            float progress = rifle.ReloadProgress;
            if (!outPlayed && progress >= 0.30f)
            {
                outPlayed = true;
                Play(magazineOut, handlingLevel);
            }
            if (!inPlayed && progress >= 0.72f)
            {
                inPlayed = true;
                Play(magazineIn, handlingLevel);
            }
        }

        private void OnReloadStarted()
        {
            outPlayed = inPlayed = false;
            Play(reloadHandling, handlingLevel);
        }

        public void PlayRemoteReload() => Play(reloadHandling, handlingLevel);
        public void PlayGunfire()
        {
            AudioClip clip = gunfireVariants != null && gunfireVariants.Length > 0
                ? gunfireVariants[Random.Range(0, gunfireVariants.Length)] : gunfire;
            if (clip == null) clip = gunfire;
            if (clip == null) return;
            source.pitch = 1f + Random.Range(-gunfirePitchVariation, gunfirePitchVariation);
            source.PlayOneShot(clip, gunfireLevel);
        }
        public void PlayImpact() => Play(impact, impactLevel);
        public void PlayEmpty() => Play(empty, handlingLevel);

        private void Play(AudioClip clip, float level)
        {
            if (clip == null) return;
            source.pitch = 1f;
            source.PlayOneShot(clip, level);
        }
    }
}

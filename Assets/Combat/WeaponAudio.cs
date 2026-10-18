using UnityEngine;

namespace ArcadeShooter.Combat
{
    // Optional sounds follow presentation state. They never advance ammunition or reload time.
    [RequireComponent(typeof(AudioSource))]
    public sealed class WeaponAudio : MonoBehaviour
    {
        [SerializeField] private AutomaticRifle rifle;
        [SerializeField] private AudioClip gunfire;
        [SerializeField] private AudioClip impact;
        [SerializeField] private AudioClip reloadHandling;
        [SerializeField] private AudioClip magazineOut;
        [SerializeField] private AudioClip magazineIn;
        [SerializeField] private AudioClip empty;

        private AudioSource source;
        private bool outPlayed;
        private bool inPlayed;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
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
                Play(magazineOut);
            }
            if (!inPlayed && progress >= 0.72f)
            {
                inPlayed = true;
                Play(magazineIn);
            }
        }

        private void OnReloadStarted()
        {
            outPlayed = inPlayed = false;
            Play(reloadHandling);
        }

        public void PlayRemoteReload() => Play(reloadHandling);
        public void PlayGunfire() => Play(gunfire);
        public void PlayImpact() => Play(impact);
        public void PlayEmpty() => Play(empty);

        private void Play(AudioClip clip)
        {
            if (clip != null) source.PlayOneShot(clip);
        }
    }
}

using UnityEngine;
using UnityEngine.Audio;

namespace ArcadeShooter.Audio
{
    // The mixer and its exposed parameters can be assigned once the bus asset exists.
    // UI sliders can call these methods without changing gameplay audio components.
    public sealed class AudioMixSettings : MonoBehaviour
    {
        public enum Bus { Master, Music, Sfx, Ambience, Ui }

        [SerializeField] private AudioMixer mixer;
        [SerializeField, Range(0f, 1f)] private float master = 1f;
        [SerializeField, Range(0f, 1f)] private float music = 1f;
        [SerializeField, Range(0f, 1f)] private float sfx = 1f;
        [SerializeField, Range(0f, 1f)] private float ambience = 1f;
        [SerializeField, Range(0f, 1f)] private float ui = 1f;

        private void Awake()
        {
            SetLevel(Bus.Master, master);
            SetLevel(Bus.Music, music);
            SetLevel(Bus.Sfx, sfx);
            SetLevel(Bus.Ambience, ambience);
            SetLevel(Bus.Ui, ui);
        }

        public void SetLevel(Bus bus, float level)
        {
            level = Mathf.Clamp01(level);
            switch (bus)
            {
                case Bus.Master: master = level; break;
                case Bus.Music: music = level; break;
                case Bus.Sfx: sfx = level; break;
                case Bus.Ambience: ambience = level; break;
                case Bus.Ui: ui = level; break;
            }
            if (mixer == null) return;
            string parameter = bus switch
            {
                Bus.Master => "MasterVolume",
                Bus.Music => "MusicVolume",
                Bus.Sfx => "SfxVolume",
                Bus.Ambience => "AmbienceVolume",
                _ => "UiVolume"
            };
            mixer.SetFloat(parameter, level <= 0f ? -80f : Mathf.Max(-80f, 20f * Mathf.Log10(level)));
        }
    }
}

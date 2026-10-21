using System.Collections.Generic;
using ArcadeShooter.Encounters;
using ArcadeShooter.Missions;
using UnityEngine;
using UnityEngine.Audio;

namespace ArcadeShooter.Audio
{
    // Reads replicated mission/encounter state on each client. No audio data is networked.
    public sealed class MusicManager : MonoBehaviour
    {
        public enum MusicState { Exploration, DangerWarning, Combat, Boss, Results }

        [Header("Loops")]
        [SerializeField] private AudioClip exploration;
        [SerializeField] private AudioClip dangerWarning;
        [SerializeField] private AudioClip combat;
        [SerializeField] private AudioClip boss; // Reserved for a future encounter type.
        [SerializeField] private AudioClip facilityAmbience;
        [Header("One-shot cues")]
        [SerializeField] private AudioClip warningCue;
        [SerializeField] private AudioClip areaClearedCue;
        [SerializeField] private AudioClip extractionActiveCue;
        [SerializeField] private AudioClip countdownStartCue;
        [SerializeField] private AudioClip countdownTickCue;
        [SerializeField] private AudioClip successCue;
        [SerializeField] private AudioClip failureCue;
        [Header("Mix")]
        [SerializeField] private AudioMixerGroup musicGroup;
        [SerializeField] private AudioMixerGroup ambienceGroup;
        [SerializeField] private AudioMixerGroup uiGroup;
        [SerializeField, Range(0f, 1f)] private float musicLevel = 0.35f;
        [SerializeField, Range(0f, 1f)] private float ambienceLevel = 0.24f;
        [SerializeField, Range(0f, 1f)] private float cueLevel = 0.55f;
        [SerializeField, Min(0.05f)] private float transitionSeconds = 1.6f;

        private readonly AudioSource[] loops = new AudioSource[2];
        private readonly Dictionary<DangerZone, DangerZone.ZoneState> previousZones =
            new Dictionary<DangerZone, DangerZone.ZoneState>();
        private AudioSource cues;
        private AudioSource ambience;
        private DangerZone[] zones;
        private AudioClip selectedLoop;
        private int activeLoop = -1;
        private MusicState state;
        private MissionState previousMissionState;
        private bool previousCountdown;
        private int previousCountdownSecond = -1;
        private bool bossRequested;

        public MusicState State => state;

        private void Awake()
        {
            for (int i = 0; i < loops.Length; i++)
            {
                loops[i] = gameObject.AddComponent<AudioSource>();
                Configure(loops[i], musicGroup, true);
            }
            cues = gameObject.AddComponent<AudioSource>();
            Configure(cues, uiGroup, false);
            ambience = gameObject.AddComponent<AudioSource>();
            Configure(ambience, ambienceGroup, true);
            ambience.clip = facilityAmbience;
            ambience.volume = ambienceLevel;
            if (facilityAmbience != null) ambience.Play();
        }

        private static void Configure(AudioSource source, AudioMixerGroup group, bool loop)
        {
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = group;
            source.volume = 0f;
        }

        private void Start()
        {
            zones = FindObjectsByType<DangerZone>();
            bool warningAtStart = false;
            foreach (DangerZone zone in zones)
            {
                previousZones.Add(zone, zone.State);
                warningAtStart |= zone.State == DangerZone.ZoneState.Warning;
            }
            if (warningAtStart) PlayCue(warningCue);
            MissionController mission = MissionController.Instance;
            previousMissionState = mission != null ? mission.State : MissionState.Waiting;
            state = ResolveState(mission);
            SelectLoop(LoopFor(state));
        }

        private void Update()
        {
            MissionController mission = MissionController.Instance;
            MusicState next = ResolveState(mission);
            if (next != state)
            {
                state = next;
                SelectLoop(LoopFor(state));
            }
            bool warningStarted = false;
            bool areaCleared = false;
            foreach (DangerZone zone in zones)
            {
                if (zone == null) continue;
                DangerZone.ZoneState current = zone.State;
                if (previousZones[zone] != current)
                {
                    warningStarted |= current == DangerZone.ZoneState.Warning;
                    areaCleared |= current == DangerZone.ZoneState.Cleared;
                }
                previousZones[zone] = current;
            }
            if (warningStarted) PlayCue(warningCue);
            if (areaCleared) PlayCue(areaClearedCue);
            if (mission != null && mission.State != previousMissionState)
            {
                if (mission.State == MissionState.ExtractionActive) PlayCue(extractionActiveCue);
                else if (mission.State == MissionState.Success) PlayCue(successCue);
                else if (mission.State == MissionState.Failure) PlayCue(failureCue);
                previousMissionState = mission.State;
            }
            UpdateCountdown(mission);
            float step = musicLevel * Time.unscaledDeltaTime / transitionSeconds;
            for (int i = 0; i < loops.Length; i++)
            {
                loops[i].volume = Mathf.MoveTowards(loops[i].volume, i == activeLoop ? musicLevel : 0f, step);
                if (i != activeLoop && loops[i].isPlaying && loops[i].volume <= 0f) loops[i].Stop();
            }
        }

        private void UpdateCountdown(MissionController mission)
        {
            ExtractionPoint extraction = mission != null ? mission.Extraction : null;
            bool running = extraction != null && extraction.CountdownRunning;
            if (running && !previousCountdown) PlayCue(countdownStartCue);
            if (running)
            {
                int second = Mathf.CeilToInt(extraction.CountdownRemaining);
                if (second <= 5 && second > 0 && second != previousCountdownSecond)
                    PlayCue(countdownTickCue);
                previousCountdownSecond = second;
            }
            else previousCountdownSecond = -1;
            previousCountdown = running;
        }

        private MusicState ResolveState(MissionController mission)
        {
            if (mission != null && mission.IsFinished) return MusicState.Results;
            if (bossRequested && boss != null) return MusicState.Boss;
            bool warning = false;
            if (zones != null)
                foreach (DangerZone zone in zones)
                {
                    if (zone == null) continue;
                    if (zone.State == DangerZone.ZoneState.Active) return MusicState.Combat;
                    if (zone.State == DangerZone.ZoneState.Warning) warning = true;
                }
            return warning ? MusicState.DangerWarning : MusicState.Exploration;
        }

        private AudioClip LoopFor(MusicState requested) => requested switch
        {
            MusicState.DangerWarning => dangerWarning != null ? dangerWarning : exploration,
            MusicState.Combat => combat != null ? combat : exploration,
            MusicState.Boss => boss != null ? boss : combat,
            MusicState.Results => null,
            _ => exploration
        };

        private void SelectLoop(AudioClip clip)
        {
            if (clip == selectedLoop) return;
            selectedLoop = clip;
            if (clip == null) { activeLoop = -1; return; }
            int next = activeLoop == 0 ? 1 : 0;
            loops[next].Stop();
            loops[next].clip = clip;
            loops[next].volume = 0f;
            loops[next].Play();
            activeLoop = next;
        }

        private void PlayCue(AudioClip clip)
        {
            if (clip != null) cues.PlayOneShot(clip, cueLevel);
        }

        // A future boss encounter can request this locally from its replicated state.
        public void RequestBossMusic(bool active) => bossRequested = active;
    }
}

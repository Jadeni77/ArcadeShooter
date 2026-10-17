using System;
using ArcadeShooter.Encounters;
using UnityEngine;

namespace ArcadeShooter.Missions
{
    // A zone-backed objective or an interaction objective; runtime completion lives in the mission.
    public sealed class MissionObjective : MonoBehaviour
    {
        [SerializeField] private string label = "Complete objective";
        [SerializeField] private DangerZone dangerZone;
        public string Label => label;
        public event Action<MissionObjective> CompletionRequested;

        private void OnEnable()
        {
            if (dangerZone != null) dangerZone.Changed += OnZoneChanged;
        }
        private void OnDisable()
        {
            if (dangerZone != null) dangerZone.Changed -= OnZoneChanged;
        }
        private void OnZoneChanged(DangerZone zone)
        {
            if (zone.State == DangerZone.ZoneState.Cleared) CompletionRequested?.Invoke(this);
        }
    }
}

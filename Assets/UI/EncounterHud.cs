using System.Collections;
using ArcadeShooter.Encounters;
using TMPro;
using UnityEngine;

namespace ArcadeShooter.UI
{
    public sealed class EncounterHud : MonoBehaviour
    {
        [SerializeField] private DangerZone[] zones;
        [SerializeField] private TextMeshProUGUI noticeText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField, Min(0f)] private float clearNoticeDuration = 4f;

        private DangerZone displayedZone;
        private Coroutine clearNotice;

        private void OnEnable()
        {
            if (zones == null) return;
            foreach (DangerZone zone in zones)
                if (zone != null) zone.Changed += OnZoneChanged;
        }

        private void Start() => Refresh();

        private void OnDisable()
        {
            if (zones == null) return;
            foreach (DangerZone zone in zones)
                if (zone != null) zone.Changed -= OnZoneChanged;
        }

        private void OnZoneChanged(DangerZone zone)
        {
            if (zone.State == DangerZone.ZoneState.Warning ||
                zone.State == DangerZone.ZoneState.Cleared ||
                zone.State == DangerZone.ZoneState.Failed || displayedZone == null)
            {
                displayedZone = zone;
                if (clearNotice != null) StopCoroutine(clearNotice);
                clearNotice = null;
            }
            if (zone != displayedZone) return;
            Refresh();
            if (zone.State == DangerZone.ZoneState.Cleared)
                clearNotice = StartCoroutine(HideClearNotice());
        }

        private void Refresh()
        {
            if (noticeText == null || progressText == null) return;
            if (displayedZone == null || displayedZone.State == DangerZone.ZoneState.Inactive)
            {
                noticeText.enabled = false;
                progressText.enabled = false;
                return;
            }

            if (displayedZone.IsStopped)
            {
                noticeText.text = $"{displayedZone.ZoneName} — ENCOUNTER STOPPED";
                noticeText.enabled = true;
                progressText.enabled = false;
                return;
            }

            switch (displayedZone.State)
            {
                case DangerZone.ZoneState.Warning:
                    noticeText.text = $"{displayedZone.ZoneName} — {displayedZone.WarningMessage}";
                    noticeText.enabled = true;
                    progressText.enabled = false;
                    break;
                case DangerZone.ZoneState.Active:
                    noticeText.enabled = false;
                    progressText.text = $"Wave: {displayedZone.CurrentWave} / {displayedZone.TotalWaves}\n" +
                                        $"Enemies Remaining: {displayedZone.RemainingEnemies}" +
                                        (displayedZone.RemainingEnemies == 0 &&
                                         displayedZone.CurrentWave < displayedZone.TotalWaves
                                            ? "\nNext wave incoming..." : "");
                    progressText.enabled = true;
                    break;
                case DangerZone.ZoneState.Cleared:
                    noticeText.text = $"{displayedZone.ZoneName} — AREA CLEARED";
                    noticeText.enabled = true;
                    progressText.enabled = false;
                    break;
                case DangerZone.ZoneState.Failed:
                    noticeText.text = $"{displayedZone.ZoneName} — ALL PLAYERS DOWN";
                    noticeText.enabled = true;
                    progressText.enabled = false;
                    break;
            }
        }

        private IEnumerator HideClearNotice()
        {
            yield return new WaitForSeconds(clearNoticeDuration);
            clearNotice = null;
            displayedZone = null;
            if (zones != null)
                foreach (DangerZone zone in zones)
                    if (zone != null && zone.State == DangerZone.ZoneState.Active &&
                        !zone.IsStopped) { displayedZone = zone; break; }
            Refresh();
        }
    }
}

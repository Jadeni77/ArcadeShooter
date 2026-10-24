using System.Collections;
using ArcadeShooter.Encounters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        private Image noticePlate;
        private Image progressPlate;
        private CanvasGroup noticeGroup;

        private void Awake()
        {
            if (noticeText == null || progressText == null) return;
            noticePlate = CombatHudVisuals.Plate("Encounter Banner", transform, new Vector2(0.5f, 1),
                new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(800, 132), CombatHudVisuals.Night);
            noticePlate.transform.SetAsFirstSibling();
            noticeGroup = noticePlate.gameObject.AddComponent<CanvasGroup>();
            noticeGroup.alpha = 0;
            noticeText.transform.SetParent(noticePlate.transform, false);
            RectTransform noticeRect = noticeText.rectTransform;
            noticeRect.anchorMin = Vector2.zero;
            noticeRect.anchorMax = Vector2.one;
            noticeRect.offsetMin = new Vector2(16, 8);
            noticeRect.offsetMax = new Vector2(-16, -8);
            noticeText.fontSize = 34;
            noticeText.fontStyle = FontStyles.Bold;
            noticeText.color = CombatHudVisuals.Amber;
            noticeText.alignment = TextAlignmentOptions.Center;
            progressPlate = CombatHudVisuals.Plate("Wave Tracker", transform, new Vector2(1, 1),
                new Vector2(1, 1), new Vector2(-28, -29), new Vector2(420, 106), CombatHudVisuals.Night);
            progressText.transform.SetParent(progressPlate.transform, false);
            RectTransform progressRect = progressText.rectTransform;
            progressRect.anchorMin = Vector2.zero;
            progressRect.anchorMax = Vector2.one;
            progressRect.offsetMin = new Vector2(12, 9);
            progressRect.offsetMax = new Vector2(-16, -9);
            progressText.fontSize = 25;
            progressText.fontStyle = FontStyles.Bold;
            progressText.color = CombatHudVisuals.White;
            progressText.alignment = TextAlignmentOptions.Right;
        }

        private void Update()
        {
            if (noticeGroup == null) return;
            float target = noticeText.enabled ? 1 : 0;
            noticeGroup.alpha = Mathf.MoveTowards(noticeGroup.alpha, target, Time.unscaledDeltaTime * 6f);
        }

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
                SyncPanels();
                return;
            }

            if (displayedZone.IsStopped)
            {
                noticeText.text = $"{displayedZone.ZoneName.ToUpperInvariant()}  /  ENCOUNTER STOPPED";
                noticeText.color = CombatHudVisuals.Red;
                noticeText.enabled = true;
                progressText.enabled = false;
                SyncPanels();
                return;
            }

            switch (displayedZone.State)
            {
                case DangerZone.ZoneState.Warning:
                    noticeText.text = $"CONTAINMENT BREACH\n{(displayedZone.WarningMessage ?? "HOSTILES INBOUND").ToUpperInvariant()}  /  {displayedZone.ZoneName.ToUpperInvariant()}";
                    noticeText.color = CombatHudVisuals.Amber;
                    noticeText.enabled = true;
                    progressText.enabled = false;
                    break;
                case DangerZone.ZoneState.Active:
                    noticeText.enabled = false;
                    progressText.text = $"{displayedZone.ZoneName.ToUpperInvariant()}  /  WAVE {displayedZone.CurrentWave:00}\n" +
                                        $"{displayedZone.RemainingEnemies:00}  HOSTILES REMAINING" +
                                        (displayedZone.RemainingEnemies == 0 &&
                                         displayedZone.CurrentWave < displayedZone.TotalWaves
                                            ? "\nNEXT WAVE INCOMING" : "");
                    progressText.enabled = true;
                    break;
                case DangerZone.ZoneState.Cleared:
                    noticeText.text = $"{displayedZone.ZoneName.ToUpperInvariant()}  /  AREA CLEARED";
                    noticeText.color = CombatHudVisuals.Cyan;
                    noticeText.enabled = true;
                    progressText.enabled = false;
                    break;
                case DangerZone.ZoneState.Failed:
                    noticeText.text = $"{displayedZone.ZoneName.ToUpperInvariant()}  /  ALL PLAYERS DOWN";
                    noticeText.color = CombatHudVisuals.Red;
                    noticeText.enabled = true;
                    progressText.enabled = false;
                    break;
            }
            SyncPanels();
        }

        private void SyncPanels()
        {
            if (noticePlate != null) noticePlate.enabled = noticeText.enabled;
            if (progressPlate != null) progressPlate.enabled = progressText.enabled;
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

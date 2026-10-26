using ArcadeShooter.Combat;
using ArcadeShooter.Networking;
using ArcadeShooter.Progression;
using TMPro;
using UnityEngine;

namespace ArcadeShooter.UI
{
    // Reads the existing local profile and weapon tuning. Session ownership remains in the controller.
    [DefaultExecutionOrder(11001)]
    [RequireComponent(typeof(MultiplayerSessionController))]
    public sealed class M10LoadoutProfileView : MonoBehaviour
    {
        [SerializeField] private RectTransform progressionRoot;
        [SerializeField] private TMP_Text profileCallsign;
        [SerializeField] private TMP_Text profileLevel;
        [SerializeField] private TMP_Text profileXp;
        [SerializeField] private UnityEngine.UI.Image profileXpFill;
        [SerializeField] private TMP_Text profileRecord;
        [SerializeField] private TMP_Text profileUnlocks;
        [SerializeField] private TMP_Text profileNotice;
        [SerializeField] private TMP_Text rifleStats;
        [SerializeField] private TMP_Text finishState;
        [SerializeField] private TMP_Text titleState;
        [SerializeField] private UnityEngine.UI.RawImage riflePreview;
        [SerializeField] private Texture2D standardRiflePreview;
        [SerializeField] private Texture2D tealRiflePreview;
        [SerializeField] private AutomaticRifle riflePrefab;

        private MultiplayerSessionController controller;
        private UnityEngine.UI.Button finishButton;
        private UnityEngine.UI.Button titleButton;
        private UnityEngine.UI.Button difficultyButton;
        private string lastSignature;

        private void Start()
        {
            controller = GetComponent<MultiplayerSessionController>();
            finishButton = FindButton("Rifle Finish");
            titleButton = FindButton("Player Title");
            difficultyButton = FindButton("Difficulty");
            RectTransform progressionText = progressionRoot.Find("Player Progression") as RectTransform;
            if (progressionText != null) progressionText.gameObject.SetActive(false);
            Place(finishButton, 0, 0, 580, 68);
            Place(titleButton, 0, 86, 580, 68);
            Place(difficultyButton, 0, 172, 580, 68);
            controller.UiChanged += Refresh;
            if (controller.Profile != null) controller.Profile.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (controller == null) return;
            controller.UiChanged -= Refresh;
            if (controller.Profile != null) controller.Profile.Changed -= Refresh;
        }

        private UnityEngine.UI.Button FindButton(string name) =>
            progressionRoot == null ? null : progressionRoot.Find(name)?.GetComponent<UnityEngine.UI.Button>();

        private static void Place(UnityEngine.UI.Button button, float x, float y, float width, float height)
        {
            if (button == null) return;
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private void Refresh()
        {
            if (controller?.Profile == null) return;
            LocalProfile profile = controller.Profile;
            int level = profile.Level;
            bool teal = profile.Rules.FinishUnlocked(level, RifleFinish.Teal);
            bool pathfinder = profile.Rules.TitleUnlocked(level, PlayerTitle.Pathfinder);
            string signature = controller.DisplayName + "|" + profile.Data.xp + "|" +
                               profile.Data.completedMissions + "|" + profile.Data.failedMissions + "|" +
                               profile.Data.selectedFinish + "|" + profile.Data.selectedTitle + "|" +
                               controller.SelectedDifficulty + "|" + controller.IsSessionHost + "|" + profile.LoadWarning;
            if (signature == lastSignature) return;
            lastSignature = signature;

            profileCallsign.text = controller.DisplayName.ToUpperInvariant();
            profileLevel.text = level.ToString("00");
            profileXp.text = $"{profile.XpIntoLevel:N0} / {profile.XpForNextLevel:N0} XP  /  NEXT RANK";
            float progress = profile.XpForNextLevel <= 0 ? 1f :
                Mathf.Clamp01((float)profile.XpIntoLevel / profile.XpForNextLevel);
            profileXpFill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 720f * progress);
            profileRecord.text = $"MISSIONS COMPLETED     {profile.Data.completedMissions:00}\n" +
                                 $"MISSIONS FAILED           {profile.Data.failedMissions:00}";
            profileUnlocks.text =
                $"RIFLE FINISH  /  TEAL      {(teal ? "UNLOCKED" : "LEVEL " + profile.Rules.tealFinishLevel.ToString("00"))}\n" +
                $"CALLSIGN  /  PATHFINDER   {(pathfinder ? "UNLOCKED" : "LEVEL " + profile.Rules.pathfinderTitleLevel.ToString("00"))}\n" +
                $"EQUIPPED  /  {profile.Data.selectedFinish.ToString().ToUpperInvariant()} FINISH  +  {profile.Data.selectedTitle.ToString().ToUpperInvariant()} TITLE";
            profileNotice.text = string.IsNullOrEmpty(profile.LoadWarning) ?
                "PROGRESSION SAVED LOCALLY" : profile.LoadWarning;

            if (riflePrefab != null)
                rifleStats.text = $"DAMAGE     {riflePrefab.Damage:0}\n" +
                                  $"FIRE RATE    {riflePrefab.RoundsPerSecond:0.#} ROUNDS / SEC\n" +
                                  $"MAGAZINE     {riflePrefab.MagazineSize:00} ROUNDS\n" +
                                  $"RELOAD       {riflePrefab.ReloadDuration:0.0} SEC";
            finishState.text = teal ?
                $"{profile.Data.selectedFinish.ToString().ToUpperInvariant()} FINISH  /  EQUIPPED" :
                $"STANDARD EQUIPPED  /  TEAL UNLOCKS AT LEVEL {profile.Rules.tealFinishLevel:00}";
            if (riflePreview != null)
                riflePreview.texture = profile.Data.selectedFinish == RifleFinish.Teal && tealRiflePreview != null ?
                    tealRiflePreview : standardRiflePreview;
            titleState.text = pathfinder ?
                $"{profile.Data.selectedTitle.ToString().ToUpperInvariant()} TITLE  /  EQUIPPED" :
                $"RECRUIT EQUIPPED  /  PATHFINDER UNLOCKS AT LEVEL {profile.Rules.pathfinderTitleLevel:00}";
        }

    }
}

using System;
using System.Collections;
using ArcadeShooter.Audio;
using ArcadeShooter.Networking;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ArcadeShooter.UI
{
    // Presentation only. Session, profile, and mission ownership stay in their existing systems.
    [DefaultExecutionOrder(11000)]
    [RequireComponent(typeof(MultiplayerSessionController))]
    public sealed class MenuLobbyView : MonoBehaviour
    {
        [SerializeField] private GameObject landingPage;
        [SerializeField] private GameObject multiplayerPage;
        [SerializeField] private GameObject profilePage;
        [SerializeField] private GameObject settingsPage;
        [SerializeField] private GameObject loadoutPage;
        [SerializeField] private UnityEngine.UI.Button playButton;
        [SerializeField] private UnityEngine.UI.Button profileButton;
        [SerializeField] private UnityEngine.UI.Button settingsButton;
        [SerializeField] private UnityEngine.UI.Button quitButton;
        [SerializeField] private UnityEngine.UI.Button[] backButtons;
        [SerializeField] private TMP_InputField displayNameInput;
        [SerializeField] private TMP_Text profileDetails;
        [SerializeField] private TMP_Text playerCount;
        [SerializeField] private TMP_Text sessionHeading;
        [SerializeField] private TMP_Text sessionCode;
        [SerializeField] private TMP_Text difficultyText;
        [SerializeField] private TMP_Text[] memberNames;
        [SerializeField] private TMP_Text[] memberDetails;
        [SerializeField] private TMP_Text[] memberEquipment;
        [SerializeField] private TMP_Text[] memberReadiness;
        [SerializeField] private UnityEngine.UI.Image[] emptySlotWashes;
        [SerializeField] private UnityEngine.UI.Button copyCodeButton;
        [SerializeField] private UnityEngine.UI.Button readyButton;
        [SerializeField] private TMP_Text readyButtonLabel;
        [SerializeField] private UnityEngine.UI.Button loadoutButton;
        [SerializeField] private UnityEngine.UI.Button closeLoadoutButton;
        [SerializeField] private UnityEngine.UI.Slider masterSlider;
        [SerializeField] private UnityEngine.UI.Slider musicSlider;
        [SerializeField] private UnityEngine.UI.Slider sfxSlider;
        [SerializeField] private AudioMixSettings audioSettings;

        private MultiplayerSessionController controller;
        private string lastRoster;
        private GameObject shownPage;
        private Coroutine pageReveal;

        private void Start()
        {
            controller = GetComponent<MultiplayerSessionController>();
            playButton.onClick.AddListener(() => Show(multiplayerPage));
            profileButton.onClick.AddListener(() => Show(profilePage));
            settingsButton.onClick.AddListener(() => Show(settingsPage));
            quitButton.onClick.AddListener(Application.Quit);
            foreach (UnityEngine.UI.Button button in backButtons)
                if (button != null) button.onClick.AddListener(() => Show(landingPage));
            displayNameInput.SetTextWithoutNotify(controller.DisplayName);
            displayNameInput.onEndEdit.AddListener(controller.SaveDisplayName);
            copyCodeButton.onClick.AddListener(controller.CopyJoinCode);
            readyButton.onClick.AddListener(controller.ToggleReady);
            loadoutButton.onClick.AddListener(() => loadoutPage.SetActive(true));
            closeLoadoutButton.onClick.AddListener(() => loadoutPage.SetActive(false));
            BindVolume(masterSlider, AudioMixSettings.Bus.Master, "ArcadeShooter.Audio.Master");
            BindVolume(musicSlider, AudioMixSettings.Bus.Music, "ArcadeShooter.Audio.Music");
            BindVolume(sfxSlider, AudioMixSettings.Bus.Sfx, "ArcadeShooter.Audio.Sfx");
            controller.UiChanged += Refresh;
            Show(landingPage);
            loadoutPage.SetActive(false);
            Refresh();
        }

        private void OnDestroy()
        {
            if (controller != null) controller.UiChanged -= Refresh;
        }

        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;
            if (loadoutPage.activeSelf) loadoutPage.SetActive(false);
            else if (!controller.HasSession && !landingPage.activeSelf) Show(landingPage);
        }

        private void BindVolume(UnityEngine.UI.Slider slider, AudioMixSettings.Bus bus, string key)
        {
            if (slider == null) return;
            float value = PlayerPrefs.GetFloat(key, 1f);
            slider.SetValueWithoutNotify(value);
            if (audioSettings != null) audioSettings.SetLevel(bus, value);
            slider.onValueChanged.AddListener(level =>
            {
                if (audioSettings != null) audioSettings.SetLevel(bus, level);
                PlayerPrefs.SetFloat(key, level);
                PlayerPrefs.Save();
            });
        }

        private void Show(GameObject page)
        {
            bool animate = shownPage != null && shownPage != page;
            shownPage = page;
            if (pageReveal != null) StopCoroutine(pageReveal);
            landingPage.SetActive(page == landingPage);
            multiplayerPage.SetActive(page == multiplayerPage);
            profilePage.SetActive(page == profilePage);
            settingsPage.SetActive(page == settingsPage);
            CanvasGroup group = page.GetComponent<CanvasGroup>();
            if (group == null) group = page.AddComponent<CanvasGroup>();
            group.alpha = animate ? 0 : 1;
            group.interactable = !animate;
            group.blocksRaycasts = !animate;
            if (animate) pageReveal = StartCoroutine(RevealPage(group));
            if (page == profilePage) RefreshProfile();
            if (EventSystem.current != null)
            {
                UnityEngine.UI.Button focus = page == landingPage ? playButton :
                    page == multiplayerPage ? backButtons[0] :
                    page == profilePage ? backButtons[1] :
                    page == settingsPage ? backButtons[2] : null;
                if (focus != null && focus.gameObject.activeInHierarchy)
                    EventSystem.current.SetSelectedGameObject(focus.gameObject);
            }
        }

        private IEnumerator RevealPage(CanvasGroup group)
        {
            const float duration = 0.18f;
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            group.alpha = 1;
            group.interactable = true;
            group.blocksRaycasts = true;
            pageReveal = null;
        }

        private void RefreshProfile()
        {
            if (controller?.Profile == null || profileDetails == null) return;
            var profile = controller.Profile;
            profileDetails.text = $"LEVEL {profile.Level:00}   •   {profile.XpIntoLevel:N0} / {profile.XpForNextLevel:N0} XP\n" +
                                  $"MISSIONS COMPLETE   {profile.Data.completedMissions:00}\n" +
                                  $"RIFLE FINISH   {profile.Data.selectedFinish.ToString().ToUpperInvariant()}\n" +
                                  $"TITLE   {profile.Data.selectedTitle.ToString().ToUpperInvariant()}" +
                                  (string.IsNullOrEmpty(profile.LoadWarning) ? "" : "\n" + profile.LoadWarning);
        }

        private void Refresh()
        {
            if (controller == null) return;
            RefreshProfile();
            if (!controller.HasSession)
            {
                lastRoster = null;
                loadoutPage.SetActive(false);
                return;
            }
            string signature = controller.JoinCode + "|" + controller.SessionPlayerCount + "|" +
                               controller.SelectedDifficulty + "|" + controller.IsBusy;
            var players = controller.SessionPlayers;
            if (players != null)
                for (int i = 0; i < players.Count; i++)
                    signature += "|" + players[i].Id + ":" + MultiplayerSessionController.PlayerDisplayName(players[i], i) +
                                 ":" + MultiplayerSessionController.IsReady(players[i]) +
                                 ":" + MultiplayerSessionController.PlayerFinish(players[i]);
            if (signature == lastRoster) return;
            lastRoster = signature;
            playerCount.text = $"{controller.SessionPlayerCount:00} / {controller.SessionCapacity:00} OPERATORS";
            sessionHeading.text = string.IsNullOrEmpty(controller.SessionName)
                ? "FIRETEAM / PRIVATE SESSION" : "FIRETEAM / " + controller.SessionName.ToUpperInvariant();
            sessionCode.text = controller.JoinCode;
            difficultyText.text = "OPERATION 01   /   CONTAINMENT BREACH   /   " +
                                  controller.SelectedDifficulty.ToString().ToUpperInvariant();
            copyCodeButton.interactable = !string.IsNullOrEmpty(controller.JoinCode);
            readyButton.interactable = controller.CanToggleReady;
            readyButtonLabel.text = controller.LocalReady ? "CANCEL READY" : "READY UP";
            for (int i = 0; i < memberNames.Length; i++)
            {
                bool occupied = players != null && i < players.Count;
                IReadOnlyPlayer player = occupied ? players[i] : null;
                memberNames[i].text = occupied ? MultiplayerSessionController.PlayerDisplayName(player, i).ToUpperInvariant() : "AWAITING OPERATOR";
                memberDetails[i].text = occupied
                    ? (player.Id == controller.HostPlayerId ? "HOST  /  CONNECTED" : "SQUAD MEMBER  /  CONNECTED")
                    : "SLOT " + (i + 1).ToString("00") + "  /  OPEN";
                memberEquipment[i].text = occupied ? "ASSAULT RIFLE  /  " + MultiplayerSessionController.PlayerFinish(player).ToUpperInvariant() : "NO LOADOUT";
                memberReadiness[i].text = occupied
                    ? (MultiplayerSessionController.IsReady(player) ? "READY" : "NOT READY") : "OPEN";
                emptySlotWashes[i].color = occupied ? new Color(0, 0, 0, 0) : new Color(0.01f, 0.03f, 0.04f, 0.68f);
            }
        }
    }
}

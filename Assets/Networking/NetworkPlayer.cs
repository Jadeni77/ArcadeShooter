using ArcadeShooter.Combat;
using ArcadeShooter.Player;
using ArcadeShooter.Progression;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ArcadeShooter.Networking
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class NetworkPlayer : NetworkBehaviour
    {
        [SerializeField] private GameObject remoteBody;
        private FirstPersonMovement movement;
        private FirstPersonLook look;
        private AutomaticRifle rifle;
        private PlayerInput input;
        private Camera playerCamera;
        private AudioListener listener;
        private Canvas[] localCanvases;
        private CharacterController controller;
        private Renderer remoteRenderer;
        private RemoteWeaponPresentation remoteWeapon;
        private MaterialPropertyBlock remoteColor;
        private float shotFlashEndsAt;
        private bool inMatch;
        private readonly NetworkVariable<RifleFinish> selectedFinish = new NetworkVariable<RifleFinish>(
            RifleFinish.Standard, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private readonly NetworkVariable<FixedString32Bytes> displayName = new NetworkVariable<FixedString32Bytes>(
            "OPERATOR", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private Renderer[] rifleRenderers;
        private Renderer[] localRifleRenderers;
        private MaterialPropertyBlock finishProperties;

        public bool InMatch => inMatch;
        public string DisplayName => displayName.Value.ToString();

        private void Awake()
        {
            movement = GetComponent<FirstPersonMovement>();
            look = GetComponent<FirstPersonLook>();
            rifle = GetComponent<AutomaticRifle>();
            input = GetComponent<PlayerInput>();
            controller = GetComponent<CharacterController>();
            playerCamera = GetComponentInChildren<Camera>(true);
            listener = GetComponentInChildren<AudioListener>(true);
            localCanvases = GetComponentsInChildren<Canvas>(true);
            if (remoteBody != null) remoteRenderer = remoteBody.GetComponentInChildren<Renderer>(true);
            if (remoteBody != null) remoteWeapon = remoteBody.GetComponentInChildren<RemoteWeaponPresentation>(true);
            if (remoteWeapon != null) rifleRenderers = remoteWeapon.GetComponentsInChildren<Renderer>(true);
            WeaponViewModel viewModel = GetComponentInChildren<WeaponViewModel>(true);
            if (viewModel != null) localRifleRenderers = viewModel.GetComponentsInChildren<Renderer>(true);
            finishProperties = new MaterialPropertyBlock();
            remoteColor = new MaterialPropertyBlock();
            SetControl(false);
            if (playerCamera != null) playerCamera.gameObject.SetActive(false);
            if (listener != null) listener.enabled = false;
            foreach (Canvas canvas in localCanvases) canvas.enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            if (remoteBody != null) remoteBody.SetActive(false);
            AudioSource weaponSource = GetComponent<AudioSource>();
            weaponSource.spatialBlend = IsOwner ? 0f : 1f;
            weaponSource.rolloffMode = AudioRolloffMode.Logarithmic;
            weaponSource.minDistance = 2f;
            weaponSource.maxDistance = 35f;
            GetComponent<PlayerHealth>().Died += OnDied;
            selectedFinish.OnValueChanged += OnFinishChanged;
            if (IsOwner)
            {
                SendSelectedFinish();
                SetDisplayNameRpc(PlayerPrefs.GetString("ArcadeShooter.DisplayName", "OPERATOR"));
            }
            ApplyFinish();
        }

        public override void OnNetworkDespawn()
        {
            GetComponent<PlayerHealth>().Died -= OnDied;
            selectedFinish.OnValueChanged -= OnFinishChanged;
            SetControl(false);
        }

        [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Server)]
        public void BeginMatchRpc(Vector3 spawnPosition)
        {
            if (!IsOwner) return;
            controller.enabled = false;
            transform.position = spawnPosition;
            controller.enabled = true;
            inMatch = true;
            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(true);
                playerCamera.enabled = true;
            }
            if (listener != null) listener.enabled = true;
            SetControl(!GetComponent<PlayerHealth>().IsDead);
            foreach (Canvas canvas in localCanvases) canvas.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        public void ShowMatchRpc()
        {
            inMatch = true;
            if (remoteBody != null) remoteBody.SetActive(!IsOwner);
        }

        public void EndMatch()
        {
            inMatch = false;
            SetControl(false);
            if (remoteBody != null) remoteBody.SetActive(false);
            if (playerCamera != null) playerCamera.gameObject.SetActive(false);
            if (listener != null) listener.enabled = false;
            foreach (Canvas canvas in localCanvases) canvas.enabled = false;
        }

        // Mission results retain the player's view while stopping all local gameplay input.
        // The mission controller calls this on every peer after its final state replicates.
        public void FreezeForResults()
        {
            inMatch = false;
            SetControl(false);
            shotFlashEndsAt = 0f;
            remoteRenderer?.SetPropertyBlock(null);
            if (IsOwner)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void FlashRemoteShot()
        {
            remoteWeapon?.Shot();
            if (remoteRenderer == null || !inMatch || IsOwner) return;
            remoteColor.SetColor("_BaseColor", new Color(1f, 0.75f, 0.12f));
            remoteRenderer.SetPropertyBlock(remoteColor);
            shotFlashEndsAt = Time.time + 0.1f;
        }

        [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Server)]
        public void GrantResultRpc(string matchResultId, int earnedXp, bool extracted)
        {
            if (!IsOwner) return;
            MultiplayerSessionController.Instance?.GrantLocalResult(matchResultId, earnedXp, extracted);
        }

        public void SendSelectedFinish()
        {
            if (!IsSpawned || !IsOwner) return;
            RifleFinish finish = MultiplayerSessionController.Instance?.Profile?.Data.selectedFinish ?? RifleFinish.Standard;
            SelectFinishRpc(finish);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void SetDisplayNameRpc(string requested)
        {
            string trimmed = (requested ?? string.Empty).Trim();
            var name = new System.Text.StringBuilder(16);
            foreach (char character in trimmed)
            {
                if (name.Length == 16) break;
                if (character >= 'A' && character <= 'Z' ||
                    character >= 'a' && character <= 'z' ||
                    character >= '0' && character <= '9' || character == ' ' || character == '_')
                    name.Append(character);
            }
            displayName.Value = name.Length == 0 ? "OPERATOR" : name.ToString();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void SelectFinishRpc(RifleFinish finish)
        {
            // This value is cosmetic. Local save unlocks are not a trusted online entitlement.
            selectedFinish.Value = finish == RifleFinish.Teal ? RifleFinish.Teal : RifleFinish.Standard;
        }

        private void OnFinishChanged(RifleFinish previous, RifleFinish current) => ApplyFinish();
        private void ApplyFinish()
        {
            ApplyFinishTo(rifleRenderers, selectedFinish.Value);
            if (IsOwner) ApplyFinishTo(localRifleRenderers, selectedFinish.Value);
        }

        private void ApplyFinishTo(Renderer[] renderers, RifleFinish finish)
        {
            if (renderers == null) return;
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null) continue;
                if (finish == RifleFinish.Teal)
                {
                    renderer.GetPropertyBlock(finishProperties);
                    finishProperties.SetColor("_BaseColor", new Color(0.1f, 0.8f, 0.9f));
                    renderer.SetPropertyBlock(finishProperties);
                }
                else renderer.SetPropertyBlock(null);
            }
        }

        public void SetRemoteReload(bool value)
        {
            if (!IsOwner) remoteWeapon?.SetReloading(value);
        }

        private void Update()
        {
            if (shotFlashEndsAt <= 0f || Time.time < shotFlashEndsAt) return;
            shotFlashEndsAt = 0f;
            remoteRenderer?.SetPropertyBlock(null);
        }

        private void OnDied()
        {
            if (IsOwner) SetControl(false);
        }

        private void SetControl(bool enabled)
        {
            if (input != null) input.enabled = enabled;
            if (movement != null) movement.enabled = enabled;
            if (look != null) look.enabled = enabled;
            if (rifle != null) rifle.enabled = enabled;
            WeaponAim aim = GetComponent<WeaponAim>();
            if (aim != null) aim.enabled = enabled;
        }
    }
}

using ArcadeShooter.Combat;
using ArcadeShooter.Player;
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

        public bool InMatch => inMatch;

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
            remoteColor = new MaterialPropertyBlock();
            SetControl(false);
            if (playerCamera != null) playerCamera.gameObject.SetActive(false);
            if (listener != null) listener.enabled = false;
            foreach (Canvas canvas in localCanvases) canvas.enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            if (remoteBody != null) remoteBody.SetActive(false);
            GetComponent<AudioSource>().spatialBlend = IsOwner ? 0f : 1f;
            GetComponent<PlayerHealth>().Died += OnDied;
        }

        public override void OnNetworkDespawn()
        {
            GetComponent<PlayerHealth>().Died -= OnDied;
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

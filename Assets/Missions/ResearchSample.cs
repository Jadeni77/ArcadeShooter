using ArcadeShooter.Networking;
using ArcadeShooter.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ArcadeShooter.Missions
{
    [RequireComponent(typeof(NetworkObject), typeof(MissionObjective))]
    public sealed class ResearchSample : NetworkBehaviour
    {
        [SerializeField, Min(0.1f)] private float interactionRange = 2.5f;
        [SerializeField] private GameObject sampleVisual;
        private MissionObjective objective;
        public static ResearchSample Instance { get; private set; }
        public bool IsCollected => MissionController.Instance != null && MissionController.Instance.IsObjectiveComplete(objective);
        public bool CanLocalInteract => CanInteract(NetworkManager != null ? NetworkManager.LocalClient?.PlayerObject : null);

        private void Awake() { Instance = this; objective = GetComponent<MissionObjective>(); }
        public override void OnDestroy()
        {
            if (Instance == this) Instance = null;
            base.OnDestroy();
        }
        private void Update()
        {
            if (sampleVisual != null) sampleVisual.SetActive(!IsCollected);
            if (!IsSpawned || !CanLocalInteract || Cursor.lockState != CursorLockMode.Locked) return;
            var local = NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerInput>();
            if (local != null && local.enabled && local.actions.FindAction("Interact").WasPressedThisFrame()) CollectRpc();
        }

        private bool CanInteract(NetworkObject playerObject)
        {
            if (playerObject == null || !playerObject.IsSpawned || IsCollected ||
                MissionController.Instance == null || !MissionController.Instance.IsRunning ||
                playerObject.gameObject.scene != gameObject.scene) return false;
            var player = playerObject.GetComponent<NetworkPlayer>();
            var health = playerObject.GetComponent<PlayerHealth>();
            if (player == null || !player.InMatch || health == null || health.IsDead) return false;
            Vector3 origin = player.transform.position + Vector3.up * 0.7f;
            Vector3 delta = transform.position - origin;
            if (delta.sqrMagnitude > interactionRange * interactionRange) return false;
            // The sample collider is a trigger. Solid geometry blocks collection through walls.
            return !Physics.Raycast(origin, delta.normalized, delta.magnitude,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void CollectRpc(RpcParams rpcParams = default)
        {
            if (!IsServer || !NetworkManager.ConnectedClients.TryGetValue(rpcParams.Receive.SenderClientId, out NetworkClient client) ||
                !CanInteract(client.PlayerObject)) return;
            MissionController.Instance.TryCompleteObjective(objective, client.ClientId);
        }
    }
}

using Unity.Netcode;
using UnityEngine;

namespace ArcadeShooter.Missions
{
    // Face only this client's view; this cosmetic rotation is never network-authoritative.
    public sealed class MissionWorldLabel : MonoBehaviour
    {
        private Camera view;
        private void LateUpdate()
        {
            if (view == null || !view.isActiveAndEnabled)
            {
                var player = NetworkManager.Singleton != null ? NetworkManager.Singleton.LocalClient?.PlayerObject : null;
                if (player != null) view = player.GetComponentInChildren<Camera>();
            }
            if (view != null) transform.rotation = view.transform.rotation;
        }
    }
}

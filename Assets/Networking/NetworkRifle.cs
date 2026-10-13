using ArcadeShooter.Combat;
using ArcadeShooter.Player;
using Unity.Netcode;
using UnityEngine;

namespace ArcadeShooter.Networking
{
    // Clients predict cosmetics/ammo. Only the host spends authoritative ammo and raycasts.
    [RequireComponent(typeof(NetworkObject), typeof(AutomaticRifle))]
    public sealed class NetworkRifle : NetworkBehaviour
    {
        private readonly NetworkVariable<int> magazine = new NetworkVariable<int>();
        private readonly NetworkVariable<int> reserve = new NetworkVariable<int>();
        private readonly NetworkVariable<bool> reloading = new NetworkVariable<bool>();
        private AutomaticRifle rifle;
        private PlayerHealth health;
        private NetworkPlayer player;
        private float nextShotAt;
        private float reloadEndsAt;

        private void Awake()
        {
            rifle = GetComponent<AutomaticRifle>();
            health = GetComponent<PlayerHealth>();
            player = GetComponent<NetworkPlayer>();
        }

        public override void OnNetworkSpawn()
        {
            magazine.OnValueChanged += OnAmmoChanged;
            reserve.OnValueChanged += OnAmmoChanged;
            reloading.OnValueChanged += OnReloadChanged;
            if (IsServer)
            {
                magazine.Value = rifle.MagazineSize;
                reserve.Value = rifle.StartingReserve;
            }
            if (IsOwner) rifle.SyncAmmo(magazine.Value, reserve.Value, reloading.Value);
        }

        public override void OnNetworkDespawn()
        {
            magazine.OnValueChanged -= OnAmmoChanged;
            reserve.OnValueChanged -= OnAmmoChanged;
            reloading.OnValueChanged -= OnReloadChanged;
        }

        private void Update()
        {
            if (!IsServer || !reloading.Value || Time.time < reloadEndsAt) return;
            int moved = Mathf.Min(rifle.MagazineSize - magazine.Value, reserve.Value);
            magazine.Value += moved;
            reserve.Value -= moved;
            reloading.Value = false;
        }

        public void RequestShot(Vector3 direction)
        {
            if (IsOwner && IsSpawned) FireRpc(direction);
        }

        public void RequestReload()
        {
            if (IsOwner && IsSpawned) ReloadRpc();
        }

        [Rpc(SendTo.Server)]
        private void FireRpc(Vector3 direction)
        {
            if (!player.InMatch || health.IsDead || reloading.Value || magazine.Value <= 0 ||
                Time.time + 0.001f < nextShotAt || !ValidAim(direction))
            {
                CorrectAmmoRpc(magazine.Value, reserve.Value, reloading.Value);
                return;
            }
            nextShotAt = Time.time + 1f / rifle.RoundsPerSecond;
            magazine.Value--;
            // The existing prefab's camera is 0.7 m above its CharacterController root.
            Vector3 origin = transform.position + Vector3.up * 0.7f;
            if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit,
                    rifle.Range, rifle.HitMask, QueryTriggerInteraction.Ignore))
            {
                IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
                if (target != null && target.ApplyDamage(rifle.Damage, hit.point))
                    ConfirmHitRpc();
            }
            RemoteShotRpc();
            if (magazine.Value == 0) StartReload();
        }

        private bool ValidAim(Vector3 direction)
        {
            if (float.IsNaN(direction.x) || float.IsNaN(direction.y) ||
                float.IsNaN(direction.z) || float.IsInfinity(direction.x) ||
                float.IsInfinity(direction.y) || float.IsInfinity(direction.z) ||
                direction.sqrMagnitude < 0.9f ||
                direction.sqrMagnitude > 1.1f) return false;
            Vector3 flat = Vector3.ProjectOnPlane(direction, Vector3.up);
            return flat.sqrMagnitude < 0.01f ||
                   Vector3.Dot(flat.normalized, transform.forward) >= 0.45f;
        }

        [Rpc(SendTo.Server)]
        private void ReloadRpc() => StartReload();

        private void StartReload()
        {
            if (!IsServer || reloading.Value || health.IsDead || !player.InMatch ||
                magazine.Value >= rifle.MagazineSize || reserve.Value <= 0) return;
            reloading.Value = true;
            reloadEndsAt = Time.time + rifle.ReloadDuration;
        }

        [Rpc(SendTo.Owner)]
        private void ConfirmHitRpc()
        {
            if (IsOwner) GetComponentInChildren<ArcadeShooter.UI.WeaponHud>(true)?.ConfirmHit();
        }

        [Rpc(SendTo.Owner)]
        private void CorrectAmmoRpc(int currentMagazine, int currentReserve, bool isReloading)
        {
            if (IsOwner) rifle.SyncAmmo(currentMagazine, currentReserve, isReloading);
        }

        [Rpc(SendTo.NotOwner)]
        private void RemoteShotRpc()
        {
            rifle.PlayRemoteShot();
            player.FlashRemoteShot();
        }

        private void OnAmmoChanged(int oldValue, int newValue) => SyncOwner();
        private void OnReloadChanged(bool oldValue, bool newValue) => SyncOwner();
        private void SyncOwner()
        {
            if (IsOwner) rifle.SyncAmmo(magazine.Value, reserve.Value, reloading.Value);
        }
    }
}

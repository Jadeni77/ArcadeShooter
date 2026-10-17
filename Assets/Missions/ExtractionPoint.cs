using ArcadeShooter.Player;
using Unity.Netcode;
using UnityEngine;

namespace ArcadeShooter.Missions
{
    [RequireComponent(typeof(NetworkObject), typeof(BoxCollider))]
    public sealed class ExtractionPoint : NetworkBehaviour
    {
        [SerializeField, Min(0.1f)] private float countdownSeconds = 10f;
        [SerializeField] private GameObject activeIndicator;
        [SerializeField] private GameObject lockedIndicator;
        private BoxCollider area;
        private readonly ExtractionCountdown countdown = new ExtractionCountdown();
        private readonly NetworkVariable<double> deadline = new NetworkVariable<double>(-1);
        private readonly NetworkVariable<int> living = new NetworkVariable<int>();
        private readonly NetworkVariable<int> inside = new NetworkVariable<int>();
        public int LivingPlayers => living.Value;
        public int PlayersInside => inside.Value;
        public bool CountdownRunning => deadline.Value >= 0;
        public float CountdownRemaining => !CountdownRunning || NetworkManager == null ? countdownSeconds :
            Mathf.Clamp((float)(deadline.Value - NetworkManager.ServerTime.Time), 0, countdownSeconds);
        public bool CanComplete => IsServer && CountdownRunning && CountdownRemaining <= 0 &&
            MissionRules.AllLivingInside(LivingPlayers, PlayersInside);

        private void Awake()
        {
            area = GetComponent<BoxCollider>();
            area.isTrigger = true;
        }

        private void Update()
        {
            MissionController mission = MissionController.Instance;
            bool active = mission != null && mission.State == MissionState.ExtractionActive;
            if (activeIndicator != null) activeIndicator.SetActive(active);
            if (lockedIndicator != null) lockedIndicator.SetActive(!active);
            if (!IsSpawned || !IsServer) return;
            if (!active)
            {
                countdown.Reset();
                deadline.Value = -1;
                return;
            }
            int livingCount = 0, insideCount = 0;
            foreach (var player in mission.ConnectedPlayers())
            {
                if (player.GetComponent<PlayerHealth>().IsDead) continue;
                livingCount++;
                if (Contains(player.transform.position)) insideCount++;
            }
            living.Value = livingCount;
            inside.Value = insideCount;
            bool complete = countdown.Tick(NetworkManager.ServerTime.Time, countdownSeconds, livingCount, insideCount);
            deadline.Value = countdown.Deadline;
            if (complete) mission.CompleteExtraction();
        }

        public bool Contains(Vector3 position)
        {
            Vector3 point = transform.InverseTransformPoint(position) - area.center;
            Vector3 half = area.size * 0.5f;
            return Mathf.Abs(point.x) <= half.x && Mathf.Abs(point.y) <= half.y && Mathf.Abs(point.z) <= half.z;
        }

        private void OnDrawGizmos()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box == null) return;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
}

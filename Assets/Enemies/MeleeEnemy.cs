using System;
using ArcadeShooter.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace ArcadeShooter.Enemies
{
    [RequireComponent(typeof(NavMeshAgent), typeof(EnemyHealth))]
    public sealed class MeleeEnemy : MonoBehaviour
    {
        public enum State { Idle, Chase, Attack, Dead }

        [Header("Perception and movement")]
        [SerializeField, Min(0.1f)] private float detectionRadius = 4.5f;
        [SerializeField, Min(0.1f)] private float movementSpeed = 3.5f;
        [SerializeField, Min(0.05f)] private float repathInterval = 0.2f;
        [Header("Melee attack")]
        [SerializeField, Min(0.1f)] private float attackRange = 1.7f;
        [SerializeField, Min(0f)] private float attackDamage = 12f;
        [SerializeField, Min(0.05f)] private float attackCooldown = 1.2f;
        [SerializeField, Min(0f)] private float eyeHeight = 1.25f;

        private NavMeshAgent agent;
        private EnemyHealth health;
        private NetworkObject networkObject;
        private PlayerHealth target;
        private float nextRepathAt;
        private float nextScanAt;
        private float nextAttackAt;
        public State CurrentState { get; private set; } = State.Idle;
        public event Action Attacked;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<EnemyHealth>();
            networkObject = GetComponent<NetworkObject>();
            agent.speed = movementSpeed;
            agent.stoppingDistance = Mathf.Max(0.1f, attackRange * 0.75f);
            health.Died += Die;
        }

        private void OnDestroy()
        {
            if (health != null) health.Died -= Die;
        }

        private void Start()
        {
            // NetworkTransform renders the host's movement on clients.
            if (networkObject != null && networkObject.IsSpawned &&
                NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
                agent.enabled = false;
        }

        private void Update()
        {
            if (networkObject == null || !networkObject.IsSpawned ||
                NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;
            if (CurrentState == State.Dead) return;
            if (Time.time >= nextScanAt)
            {
                target = FindClosestLivingPlayer();
                nextScanAt = Time.time + repathInterval;
            }
            if (target == null)
            {
                SetState(State.Idle);
                return;
            }

            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance <= attackRange && HasLineOfSight(target))
            {
                SetState(State.Attack);
                FaceTarget();
                if (Time.time >= nextAttackAt)
                {
                    nextAttackAt = Time.time + attackCooldown;
                    Vector3 hitPoint = target.transform.position + Vector3.up;
                    if (target.ApplyDamage(attackDamage, hitPoint)) Attacked?.Invoke();
                }
            }
            else
            {
                SetState(State.Chase);
                if (agent.isOnNavMesh && Time.time >= nextRepathAt)
                {
                    agent.SetDestination(target.transform.position);
                    nextRepathAt = Time.time + repathInterval;
                }
            }
        }

        private PlayerHealth FindClosestLivingPlayer()
        {
            PlayerHealth closest = null;
            float bestDistance = detectionRadius * detectionRadius;
            foreach (PlayerHealth candidate in FindObjectsByType<PlayerHealth>())
            {
                if (candidate.IsDead) continue;
                NetworkObject networkObject = candidate.GetComponent<NetworkObject>();
                if (networkObject == null || !networkObject.IsSpawned ||
                    !candidate.GetComponent<ArcadeShooter.Networking.NetworkPlayer>().InMatch) continue;
                float sqrDistance = (candidate.transform.position - transform.position).sqrMagnitude;
                if (sqrDistance > bestDistance) continue;
                bestDistance = sqrDistance;
                closest = candidate;
            }
            return closest;
        }

        private bool HasLineOfSight(PlayerHealth player)
        {
            Vector3 origin = transform.position + Vector3.up * eyeHeight;
            Vector3 destination = player.transform.position + Vector3.up * 0.7f;
            Vector3 direction = destination - origin;
            if (!Physics.Raycast(origin, direction.normalized, out RaycastHit hit,
                    direction.magnitude + 0.05f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                return false;
            return hit.collider.GetComponentInParent<PlayerHealth>() == player;
        }

        private void SetState(State state)
        {
            if (CurrentState == state) return;
            CurrentState = state;
            if (!agent.isOnNavMesh) return;
            if (state == State.Attack || state == State.Idle)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
            else if (state == State.Chase)
            {
                agent.isStopped = false;
                nextRepathAt = 0f;
            }
        }

        private void FaceTarget()
        {
            Vector3 direction = target.transform.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                    Quaternion.LookRotation(direction), 360f * Time.deltaTime);
        }

        private void Die()
        {
            CurrentState = State.Dead;
            target = null;
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
            agent.enabled = false;
        }

        private void OnValidate()
        {
            detectionRadius = Mathf.Max(0.1f, detectionRadius);
            movementSpeed = Mathf.Max(0.1f, movementSpeed);
            attackRange = Mathf.Max(0.1f, attackRange);
            attackCooldown = Mathf.Max(0.05f, attackCooldown);
            if (TryGetComponent(out NavMeshAgent navAgent))
            {
                navAgent.speed = movementSpeed;
                navAgent.stoppingDistance = Mathf.Max(0.1f, attackRange * 0.75f);
            }
        }
    }
}

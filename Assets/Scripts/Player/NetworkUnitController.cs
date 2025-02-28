using System.Collections.Generic;
using Interfaces;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using VContainer;

namespace Player
{
    public class NetworkUnitController : NetworkBehaviour
    {
        [SerializeField] private float attackRange = 2.0f;
        [SerializeField] private float detectionRadius = 10f;
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private int health = 100;
        [SerializeField] private int damage = 10;
        [SerializeField] private float attackCooldown = 1.5f;

        private NavMeshAgent agent;
        private Transform target;
        private float lastAttackTime;
        
        private IObjectResolver _objectResolver;
        private Transform gameOriginPoint;
        private bool dependencyResolved = false;
        private IGameManager gameManager;
        private List<NetworkObject> opponentTowers = new List<NetworkObject>();
        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.updatePosition = true;
            agent.updateRotation = true;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                InvokeRepeating(nameof(FindTarget), 0, 1f);
            }
        }

        private void Update()
        {
            if (!IsServer) return;

            if (!dependencyResolved)
                return;
            
            // if (target != null)
            // {
            //     agent.SetDestination(target.position);
            //
            //     float distance = Vector3.Distance(transform.position, target.position);
            //     if (distance <= attackRange)
            //     {
            //         TryAttack();
            //     }
            // }
            var closestTower = GetClosestTower();
            if(closestTower != null)
                agent.SetDestination(closestTower.transform.position);
            else
            {
                agent.SetDestination(gameOriginPoint.position);
            }
        }
        
        public void SetObjectResolver(IObjectResolver objectResolver,ulong clientId)
        {
            _objectResolver = objectResolver;
            
            if (_objectResolver != null)
            {
                gameManager = _objectResolver.Resolve<IGameManager>();
                gameOriginPoint = gameManager.GetGameOriginPoint();
                opponentTowers = gameManager.GetOpponentTowers(clientId);
                dependencyResolved = true;
                Debug.LogError("Dependency resolved");
            }
        }

        private void FindTarget()
        {
            // Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, targetLayer);
            // if (colliders.Length > 0)
            // {
            //     target = colliders[0].transform; // İlk bulunan hedefi belirle
            // }
        }

        private void TryAttack()
        {
            // if (target == null) return;
            //
            // if (Time.time - lastAttackTime >= attackCooldown)
            // {
            //     lastAttackTime = Time.time;
            //
            //     if (target.TryGetComponent(out NetworkUnitController enemyUnit))
            //     {
            //         enemyUnit.TakeDamage(damage);
            //     }
            // }
        }
        
        private NetworkObject GetClosestTower()
        {
            NetworkObject closestTower = null;
            float closestDistance = float.MaxValue;
            foreach (var tower in opponentTowers)
            {
                if (tower == null || !tower.IsSpawned)
                    continue;
                
                float distance = Vector3.Distance(transform.position, tower.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTower = tower;
                }
            }

            return closestTower;
        }

        [ServerRpc(RequireOwnership = false)]
        public void TakeDamageServerRpc(int damage)
        {
            health -= damage;
            if (health <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            NetworkObject networkObject = GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsSpawned)
            {
                networkObject.Despawn();
            }
        }
    }
}

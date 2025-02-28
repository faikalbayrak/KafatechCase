using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using VContainer;

namespace Player
{
    public class NetworkUnitController : NetworkBehaviour
    {
        [SerializeField] private float attackRange = 5.0f;
        [SerializeField] private float detectionRadius = 10f;
        [SerializeField] private LayerMask targetLayer;
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
            
            
            FindTarget();
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
            }
        }

        private void FindTarget()
        {
            NetworkObject closestEnemyUnit = null;
            NetworkObject closestEnemyTower = null;
            
            closestEnemyUnit = GetClosestEnemyUnit();
            
            if (closestEnemyUnit != null)
            {
                target = closestEnemyUnit.transform;
            }
            else
            {
                closestEnemyTower = GetClosestEnemyTower();
                
                if (closestEnemyTower != null)
                {
                    target = closestEnemyTower.transform;
                }
                else
                {
                    target = null;
                }
            }
            
            if (target != null)
            {
                agent.SetDestination(target.position);

                float distance = Vector3.Distance(transform.position, target.position);
                if (distance <= attackRange)
                {
                    TryAttack();
                }
            }
            else
            {
                Debug.Log("Hiçbir hedef bulunamadı.");
            }
        }

        private void TryAttack()
        {
            if (target == null) return;
            
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                lastAttackTime = Time.time;
            
                if (target.TryGetComponent(out NetworkHealthController enemyObject))
                {
                    enemyObject.TakeDamage(damage);
                }
            }
        }
        
        private NetworkObject GetClosestEnemyTower()
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

        private NetworkObject GetClosestEnemyUnit()
        {
            NetworkObject closestUnit = null;
            var spawnedUnits = gameManager.SpawnedUnits;
            float closestDistance = float.MaxValue;
            
            Vector3 currentPosition = transform.position;
            
            NetworkObject currentUnit = GetComponent<NetworkObject>();
            
            ulong currentOwnerId = currentUnit.OwnerClientId;

            foreach (var unitRef in spawnedUnits)
            {
                if (unitRef.TryGet(out NetworkObject unit))
                {
                    if (unit == currentUnit)
                        continue;
                    
                    if (unit.OwnerClientId == currentOwnerId)
                        continue;
                    
                    Vector3 unitPosition = unit.transform.position;
                    
                    float distance = Vector3.Distance(currentPosition, unitPosition);
                    
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestUnit = unit;
                    }
                }
            }

            return closestUnit;
        }
    }
}

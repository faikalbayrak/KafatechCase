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
        [SerializeField] private float attackRange = 5.1f;
        [SerializeField] private int damage = 10;
        [SerializeField] private float attackCooldown = 1.5f;

        private NavMeshAgent agent;
        private NetworkUnitAnimator animator;
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
            animator = GetComponent<NetworkUnitAnimator>();
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

        public void SetObjectResolver(IObjectResolver objectResolver, ulong clientId)
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
            NetworkObject closestEnemyUnit = GetClosestEnemyUnit();
            NetworkObject closestEnemyTower = GetClosestEnemyTower();
            
            if (closestEnemyUnit != null)
            {
                target = closestEnemyUnit.transform;
            }
            else if (closestEnemyTower != null)
            {
                target = closestEnemyTower.transform;
            }
            else
            {
                target = null;
            }

            
            if (target != null)
            {
                agent.SetDestination(target.position);

                float distance = Vector3.Distance(transform.position, target.position);
                if (distance <= attackRange)
                {
                    TryAttack();
                }
                else
                {
                    animator.SetWalkState(true);
                }
            }
            else
            {
                animator.SetWalkState(false);
            }
        }

        private void TryAttack()
        {
            if (target == null) return;

            if (Time.time - lastAttackTime >= attackCooldown)
            {
                lastAttackTime = Time.time;
                animator.SetAttackState();

                // Eğer animasyon bitmeden saldırı tekrar başlıyorsa burada bir zamanlayıcı ekleyerek kontrol et.
                Invoke(nameof(DealDamage), 0.5f); // 0.5 saniye sonra hasar ver
            }
        }

        private void DealDamage()
        {
            if (target == null) return;
            if (target.TryGetComponent(out NetworkHealthController enemyObject))
            {
                enemyObject.TakeDamage(damage);
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
                    if (unit == currentUnit || unit.OwnerClientId == currentOwnerId)
                        continue;

                    float distance = Vector3.Distance(currentPosition, unit.transform.position);
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
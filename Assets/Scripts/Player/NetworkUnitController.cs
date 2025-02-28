using System;
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

        [SerializeField] private GameObject hpBarCanvas;
        [SerializeField] private Transform hpBarPosPositive;
        [SerializeField] private Transform hpBarPosNegative;

        private NavMeshAgent agent;
        private NetworkUnitAnimator animator;
        private Transform target;

        private IObjectResolver _objectResolver;
        private Transform gameOriginPoint;
        private bool dependencyResolved = false;
        private IGameManager gameManager;
        private List<NetworkObject> opponentTowers = new List<NetworkObject>();
        private bool _canAttack = false;
        
        public bool CanAttack => _canAttack;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<NetworkUnitAnimator>();
            agent.updatePosition = true;
            agent.updateRotation = true;
        }

        private void Start()
        {
            SetHpBarPosition();
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
        
        private void SetHpBarPosition()
        {
            if (hpBarCanvas == null) return;

            hpBarCanvas.transform.position = OwnerClientId % 2 == 0 ? hpBarPosPositive.position : hpBarPosNegative.position;
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
                    _canAttack = true;
                    SetCanAttackClientRpc(true);
                    animator.SetAttackState();
                }
                else
                {
                    _canAttack = false;
                    SetCanAttackClientRpc(false);
                    animator.SetWalkState(true);
                }
            }
            else
            {
                _canAttack = false;
                SetCanAttackClientRpc(false);
                animator.SetWalkState(false);
            }
        }

        [ClientRpc]
        public void SetCanAttackClientRpc(bool canAttack)
        {
            _canAttack = canAttack;
        }

        public void DealDamage()
        {
            DealDamageServerRpc();
        }
        
        [ServerRpc]
        private void DealDamageServerRpc()
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
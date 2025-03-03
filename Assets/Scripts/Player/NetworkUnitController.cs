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
        #region Serializable Fields

        [SerializeField] private float attackRange = 5.1f;
        [SerializeField] private int damage = 10;
        [SerializeField] private GameObject hpBarCanvas;
        [SerializeField] private Transform hpBarPosPositive;
        [SerializeField] private Transform hpBarPosNegative;
        
        #endregion
        
        #region Fields
        
        private NavMeshAgent agent;
        private NetworkUnitAnimator animator;
        private Transform _target;
        private IObjectResolver _objectResolver;
        private Transform gameOriginPoint;
        private bool dependencyResolved = false;
        private IGameManager gameManager;
        private List<NetworkObject> opponentTowers = new List<NetworkObject>();
        private bool _canAttack = false;
        private Vector3 bulletTargetPosition;
        
        #endregion

        #region Properties

        public Vector3 BulletTargetPosition => bulletTargetPosition;
        public bool CanAttack => _canAttack;
        public Transform Target => _target;

        #endregion
        
        #region Unity Methods
        
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
        
        #endregion

        #region Public Methods
        
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                InvokeRepeating(nameof(FindTarget), 0, 1f);
            }
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
        
        public void PlayOneShot(string soundName)
        {
            if (gameManager != null)
            {
                gameManager.PlayOneShot(soundName);
            }
        }
        
        public void DealDamage(float delay = 0)
        {
            if(delay > 0)
                Invoke(nameof(DealDamageServerRpc), delay);
            else
                DealDamageServerRpc();
        }

        #endregion
        
        #region Private Methods
        
        private void SetHpBarPosition()
        {
            if (hpBarCanvas == null) return;

            hpBarCanvas.transform.position = OwnerClientId % 2 == 0 ? hpBarPosPositive.position : hpBarPosNegative.position;
        }

        private void FindTarget()
        {
            if (!IsServer) return;
            
            if (!dependencyResolved)
                return;
            
            NetworkObject closestEnemyUnit = GetClosestEnemyUnit();
            NetworkObject closestEnemyTower = GetClosestEnemyTower();

            if (closestEnemyUnit != null)
            {
                _target = closestEnemyUnit.transform;
            }
            else if (closestEnemyTower != null)
            {
                _target = closestEnemyTower.transform;
            }
            else
            {
                _target = null;
            }
            
            SetTargetPositionClientRpc(_target != null ? _target.position : Vector3.zero);

            if (_target != null)
            {
                agent.SetDestination(_target.position);
                float distance = Vector3.Distance(transform.position, _target.position);

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
        private void SetCanAttackClientRpc(bool canAttack)
        {
            _canAttack = canAttack;
        }
        
        [ClientRpc]
        private void SetTargetPositionClientRpc(Vector3 targetPosition)
        {
            bulletTargetPosition = targetPosition;
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void DealDamageServerRpc()
        {
            if (_target == null) return;
            if (_target.TryGetComponent(out NetworkHealthController enemyObject))
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
        
        #endregion
    }
}
using Interfaces;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using VContainer;

namespace Player
{
    public class NetworkUnitController : NetworkBehaviour
    {
        [SerializeField] private float attackRange = 2.0f;  // Saldırı menzili
        [SerializeField] private float detectionRadius = 10f; // Hedef algılama yarıçapı
        [SerializeField] private LayerMask targetLayer; // Hedef olabilecek diğer unit'ler
        [SerializeField] private int health = 100; // Unit canı
        [SerializeField] private int damage = 10;  // Unit saldırı gücü
        [SerializeField] private float attackCooldown = 1.5f; // Saldırı aralığı

        private NavMeshAgent agent;
        private Transform target;
        private float lastAttackTime;
        
        private IObjectResolver _objectResolver;
        private Transform gameOriginPoint;
        private bool dependencyResolved = false;
        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)  // Hareket ve saldırı sadece sunucuda işlenir
            {
                InvokeRepeating(nameof(FindTarget), 0, 1f); // Her saniye hedef arar
            }
        }

        private void Update()
        {
            if (!IsServer) return; // Sunucu olmayanlar hareket ve saldırıyı işlemez

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
            
            agent.SetDestination(gameOriginPoint.position);
        }
        
        public void SetObjectResolver(IObjectResolver objectResolver)
        {
            _objectResolver = objectResolver;
            
            if (_objectResolver != null)
            {
                gameOriginPoint = _objectResolver.Resolve<IGameManager>().GetGameOriginPoint();
                
                dependencyResolved = true;
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

        // [ServerRpc(RequireOwnership = false)]
        // public void TakeDamage(int damage)
        // {
        //     health -= damage;
        //     if (health <= 0)
        //     {
        //         Die();
        //     }
        // }

        private void Die()
        {
            NetworkObject networkObject = GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsSpawned)
            {
                networkObject.Despawn(); // Ağda unit'i kaldır
            }
        }
    }
}

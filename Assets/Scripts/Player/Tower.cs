using Interfaces;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace Player
{
    public class Tower : NetworkBehaviour, ITower
    {
        [SerializeField] private Sprite playerTowerSprite;
        [SerializeField] private Sprite enemyTowerSprite;
        [SerializeField] private GameObject unitPrefab;
        [SerializeField] private Transform[] spawnPoints;
    
        private SpriteRenderer spriteRenderer;
        private ulong ownerClientId;
        private IObjectResolver _objectResolver;
        private Transform gameOriginPoint;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        [ServerRpc(RequireOwnership = false)]
        private void SpawnUnit_ServerRpc(ulong clientId = 0)
        {
            var gameOriginPosition = gameOriginPoint.position;
            Vector3 spawnPosition = Vector3.Distance(spawnPoints[0].position,gameOriginPosition) < 
                                    Vector3.Distance(spawnPoints[1].position,gameOriginPosition) ? spawnPoints[0].position : spawnPoints[1].position;
            
            GameObject unit = Instantiate(unitPrefab, spawnPosition, Quaternion.identity);
            if (unit.TryGetComponent<NetworkUnitController>(out var networkUnitController))
            {
                networkUnitController.SetObjectResolver(_objectResolver, clientId);
            }
                
            NetworkObject networkObject = unit.GetComponent<NetworkObject>();
            
            if (networkObject != null)
            {
                networkObject.SpawnWithOwnership(clientId);
                
                _objectResolver.Resolve<IGameManager>().RegisterUnit(networkObject);
            }
            else
            {
                Debug.LogError("Unit prefab'ında NetworkObject bulunamadı!");
            }
        }
        
        public void SpawnUnit()
        {
            if (!IsOwner) return;
            
            SpawnUnit_ServerRpc(NetworkManager.LocalClient.ClientId);
        }
        
        public void SetOwner(ulong clientId)
        {
            if (!IsServer) return;

            ownerClientId = clientId;
            SetOwnerClientRpc(ownerClientId);
        }
        
        public ulong GetOwner()
        {
            return ownerClientId;
        }
        
        public void SetObjectResolver(IObjectResolver objectResolver)
        {
            if (objectResolver == null)
                return;
            
            _objectResolver = objectResolver;

            gameOriginPoint = _objectResolver.Resolve<GameManager>().GetGameOriginPoint();
        }

        [ClientRpc]
        private void SetOwnerClientRpc(ulong clientId)
        {
            ownerClientId = clientId;
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                SetTowerColor(true);
            }
        }

        private void SetTowerColor(bool isOwnTower)
        {
            if (spriteRenderer == null)
            {
                Debug.LogError("SpriteRenderer bulunamadı!");
                return;
            }
            
            spriteRenderer.sprite = isOwnTower ? playerTowerSprite : enemyTowerSprite;
        }
    }
}
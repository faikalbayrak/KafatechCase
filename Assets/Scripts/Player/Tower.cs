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
    
        private SpriteRenderer spriteRenderer;
        private ulong ownerClientId;
        private IObjectResolver _objectResolver;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        [ServerRpc(RequireOwnership = false)]
        private void SpawnUnit_ServerRpc()
        {
            Vector3 spawnPosition = transform.position + new Vector3(0,0.5f,0) + Vector3.forward * -2f;
            
            GameObject unit = Instantiate(unitPrefab, spawnPosition, Quaternion.identity);
            NetworkObject networkObject = unit.GetComponent<NetworkObject>();
            
            if (networkObject != null)
            {
                networkObject.Spawn(true);
            }
            else
            {
                Debug.LogError("Unit prefab'ında NetworkObject bulunamadı!");
            }
        }
        
        public void SpawnUnit()
        {
            if (!IsOwner) return;
            
            SpawnUnit_ServerRpc();
        }

        
        public void SetOwner(ulong clientId)
        {
            if (!IsServer) return;

            ownerClientId = clientId;
            SetOwnerClientRpc(ownerClientId);
        }
        
        public void SetObjectResolver(IObjectResolver objectResolver)
        {
            if (objectResolver == null)
                return;
            
            _objectResolver = objectResolver;
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
using System;
using Interfaces;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace Player
{
    public class Tower : NetworkBehaviour, ITower
    {
        #region Serializable Fields

        [SerializeField] private Sprite playerTowerSprite;
        [SerializeField] private Sprite enemyTowerSprite;
        [SerializeField] private GameObject unitPrefab;
        [SerializeField] private Transform[] spawnPoints;
        
        #endregion
        
        #region Fields
        
        private SpriteRenderer _spriteRenderer;
        private ulong ownerClientId;
        private IObjectResolver _objectResolver;
        private IGameManager _gameManager;
        private Transform gameOriginPoint;
        private NetworkHealthController _healthController;
        
        #endregion
        
        #region Unity Methods
        
        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _healthController = GetComponent<NetworkHealthController>();
        }

        private void OnEnable()
        {
            _healthController.OnDeath += DestroyedTower;
        }

        private void OnDisable()
        {
            _healthController.OnDeath -= DestroyedTower;
        }
        
        #endregion

        #region Public Methods
        
        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                SetTowerColor(true);
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
            _gameManager = _objectResolver.Resolve<IGameManager>();
            gameOriginPoint = _gameManager.GetGameOriginPoint();
        }
        
        #endregion
        
        #region Private Methods
        
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
                Debug.LogError("NetworkObject is null!");
            }
            
            _gameManager.PlayOneShot("UnitSpawn");
        }
        
        

        [ClientRpc]
        private void SetOwnerClientRpc(ulong clientId)
        {
            ownerClientId = clientId;
        }
        
        private void SetTowerColor(bool isOwnTower)
        {
            if (_spriteRenderer == null)
            {
                Debug.LogError("SpriteRenderer is null!");
                return;
            }
            
            _spriteRenderer.sprite = isOwnTower ? playerTowerSprite : enemyTowerSprite;
        }
        
        private void DestroyedTower()
        {
            if (IsServer)
            {
                _gameManager.OnTowerDestroyed(OwnerClientId);
            }
        }
        
        #endregion
    }
}
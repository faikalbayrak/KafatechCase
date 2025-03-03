using System;
using Interfaces;
using Managers;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Player
{
    public class NetworkGamePlayerController : NetworkBehaviour
    {
        #region Serializable Fields

        [SerializeField] private Camera playerCamera;
        
        #endregion
        
        #region Fields
        
        private ulong _clientId = 0;
        private IObjectResolver _objectResolver;
        private IGameManager _gameManager;
        private bool dependencyResolved = false;
        
        #endregion
        
        #region Unity Methods
        
        private void Update()
        {
            if (!IsOwner)
                return;
            
            if(Input.GetMouseButtonDown(0))
                CheckClick();
        }
        
        #endregion

        #region Public Methods
        
        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                bool isSecondPlayer = OwnerClientId % 2 == 1;

                playerCamera.transform.rotation = isSecondPlayer ? Quaternion.Euler(90, 0, 0) : Quaternion.Euler(90, 180, 0);
                
                if (Camera.main != null)
                {
                    Camera.main.gameObject.SetActive(false);
                }
                
                playerCamera.gameObject.SetActive(true);
            }
            else
            {
                playerCamera.gameObject.SetActive(false);
            }
        }
        
        public void SetObjectResolver(IObjectResolver objectResolver)
        {
            _objectResolver = objectResolver;
            
            if(_objectResolver != null)
                _gameManager = _objectResolver.Resolve<IGameManager>();
            
            dependencyResolved = true;
        }
        
        public void SetOwnerClientId(ulong clientId)
        {
            _clientId = clientId;
        }
        
        public ulong GetOwnerClientId()
        {
            return _clientId;
        }
        
        #endregion
        
        #region Private Methods
        
        private void CheckClick()
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            //Debug.DrawRay(ray.origin, ray.direction * 1000, Color.yellow, 2f);
            
            if (Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("Tower")))
            {
                Tower tower = hit.collider.GetComponentInParent<Tower>();
                if (tower != null)
                {
                    tower.SpawnUnit();
                }
            }
        }
        
        #endregion
    }
}

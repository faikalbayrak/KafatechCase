using System;
using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class NetworkGamePlayerController : NetworkBehaviour
    {
        [SerializeField] private Camera playerCamera;
        private ulong _clientId = 0;
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

        private void Update()
        {
            if (!IsOwner)
                return;
            
            if(Input.GetMouseButtonDown(0))
                CheckClick();
        }
        
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
            else
            {
                Debug.LogError("Did not hit tower");
            }
        }
        
        public void SetOwnerClientId(ulong clientId)
        {
            _clientId = clientId;
        }
        
        public ulong GetOwnerClientId()
        {
            return _clientId;
        }
    }
}

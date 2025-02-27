using System;
using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class NetworkGamePlayerController : NetworkBehaviour
    {
        [SerializeField] private Camera playerCamera;
        
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

            // Debug için ray'i çiz (sarı renk, 2 saniye)
            Debug.DrawRay(ray.origin, ray.direction * 1000, Color.yellow, 2f);

            Debug.LogError("Clicked");

            // LayerMask.GetMask kullanılarak düzeltilmiş (LayerMask.NameToLayer yerine)
            if (Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("Tower")))
            {
                Debug.LogError("Hit tower");

                Tower tower = hit.collider.GetComponentInParent<Tower>();
                if (tower != null)
                {
                    Debug.LogError("Hit tower 2");
                    tower.SpawnUnit();
                }
            }
            else
            {
                Debug.LogError("Did not hit tower");
            }
        }
    }
}

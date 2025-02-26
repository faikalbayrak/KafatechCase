using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class NetworkGamePlayerController : NetworkBehaviour
    {
        [SerializeField] private GameObject playerCamera;
        
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
                
                playerCamera.SetActive(true);
            }
            else
            {
                playerCamera.SetActive(false);
            }
        }
    }
}

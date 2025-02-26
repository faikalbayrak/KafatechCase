using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class NetworkGamePlayerController : NetworkBehaviour
    {
        [SerializeField] private GameObject playerCamera;
        
        public override void OnNetworkSpawn()
        {
            if(IsServer)
                return;
            
            if (IsOwner)
            {
                Camera.main.gameObject.SetActive(false); // Ana kamerayı kapat
                playerCamera.SetActive(true);
            }
        }
    }
}

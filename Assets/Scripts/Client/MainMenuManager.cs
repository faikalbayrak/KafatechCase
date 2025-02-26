using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Client
{
    public class MainMenuManager : NetworkBehaviour
    {
        [SerializeField] private Button startClientButton;

        private void Start()
        {
            startClientButton.onClick.AddListener(StartClient);
        }

        private void StartClient()
        {
            NetworkManager.StartClient();
        }
    }
}
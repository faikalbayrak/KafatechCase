using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Client
{
    public class ClientManager : NetworkBehaviour
    {
        [SerializeField] private Button startClientButton;
        [SerializeField] private Button startServerButton;
        
        private void Start()
        {
            startClientButton.onClick.AddListener(StartClient);
            startServerButton.onClick.AddListener(StartServer);
        }
        private void StartClient()
        {
            if (NetworkManager == null) return;

            NetworkManager.OnClientConnectedCallback += OnConnected;
            NetworkManager.OnClientDisconnectCallback += OnDisconnected;

            NetworkManager.StartClient();
            Debug.Log("Client started...");
            
            startClientButton.gameObject.SetActive(false);
            startServerButton.gameObject.SetActive(false);
        }

        private void StartServer()
        {
            SceneManager.LoadScene("StartServerScene", LoadSceneMode.Additive);
            
            startClientButton.gameObject.SetActive(false);
            startServerButton.gameObject.SetActive(false);
        }

        private void OnConnected(ulong clientId)
        {
            Debug.Log($"Connected to server as client {clientId}");
        }

        private void OnDisconnected(ulong clientId)
        {
            Debug.LogError("Disconnected from server");
        }
    }
}
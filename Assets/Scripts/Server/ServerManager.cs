using Unity.Netcode;
using UnityEngine;

namespace Server
{
    public class ServerManager : NetworkBehaviour
    {
        private int connectedPlayers = 0;
        private const int maxPlayers = 2;

        private void Start()
        {
            NetworkManager.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.StartServer();
            Debug.Log("Server started...");
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            response.Approved = true;
            response.CreatePlayerObject = true;
        }

        private void OnClientConnected(ulong clientId)
        {
            connectedPlayers++;

            Debug.Log($"Client {clientId} connected ({connectedPlayers}/{maxPlayers})");

            if (connectedPlayers == maxPlayers)
            {
                Debug.Log("All players connected, starting game...");
                LoadGameScene();
            }
        }

        private void LoadGameScene()
        {
            NetworkManager.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        }
    }
}
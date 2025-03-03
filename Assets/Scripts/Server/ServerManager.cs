using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Server
{
    public class ServerManager : NetworkBehaviour
    {
        #region Serializable Fields

        [SerializeField] private GameObject serverUIView;
        [SerializeField] private GameObject clientUIView;
        [SerializeField] private TMP_Text clientConnectedClientTxt;
        [SerializeField] private TMP_Text serverConnectedClientTxt;
        
        #endregion
        
        #region Fields
        
        private int _connectedPlayers = 0;
        private const int maxPlayers = 2;
        
        #endregion
        
        #region Unity Methods
        
        private void Start()
        {
            NetworkManager.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.StartServer();
            Debug.Log("Server started...");

            if (IsServer)
            {
                serverUIView.SetActive(true);
            }
            else
            {
                clientUIView.SetActive(true);
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            response.Approved = true;
            response.CreatePlayerObject = true;
        }

        private void OnClientConnected(ulong clientId)
        {
            _connectedPlayers++;

            Debug.Log($"Client {clientId} connected ({_connectedPlayers}/{maxPlayers})");

            if (IsServer)
            {
                serverConnectedClientTxt.text = $"Connected Clients: {_connectedPlayers}/{maxPlayers}";
                SetConnectedPlayersClientRpc(_connectedPlayers);
            }

            if (_connectedPlayers == maxPlayers)
            {
                Debug.Log("All players connected, starting game...");
                
                LoadGameScene();
            }
        }

        private void LoadGameScene()
        {
            serverUIView.SetActive(false);
            SetDisableClientUIClientRpc();
            NetworkManager.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        }
        
        [ClientRpc]
        private void SetDisableClientUIClientRpc()
        {
            clientUIView.SetActive(false);
        }
        
        [ClientRpc]
        private void SetConnectedPlayersClientRpc(int connectedPlayers)
        {
            clientConnectedClientTxt.text = $"Connected Players: {connectedPlayers}/{maxPlayers}";
        }
        
        #endregion
        
    }
}
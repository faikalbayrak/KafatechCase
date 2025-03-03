using System;
using Interfaces;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Client
{
    public class ClientManager : NetworkBehaviour, IClientManager
    {
        #region Serializable Fields

        [SerializeField] private Button startClientButton;
        [SerializeField] private Button startServerButton;
        
        #endregion
        
        #region Fields
        
        private GameObject canvasGo;
        
        #endregion
        
        #region Unity Methods
        
        private void Awake()
        {
            canvasGo = startClientButton.transform.parent.gameObject;
        }

        private void Start()
        {
            startClientButton.onClick.AddListener(StartClient);
            startServerButton.onClick.AddListener(StartServer);
        }
        
        #endregion
        
        #region Private Methods
        
        private void StartClient()
        {
            if (NetworkManager == null) return;

            NetworkManager.OnClientConnectedCallback += OnConnected;
            NetworkManager.OnClientDisconnectCallback += OnDisconnected;

            NetworkManager.StartClient();
            Debug.Log("Client started...");
            
            startClientButton.gameObject.SetActive(false);
            startServerButton.gameObject.SetActive(false);
            canvasGo.SetActive(false);
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
        
        #endregion
        
    }
}
using Unity.Netcode;
using UnityEngine;

namespace Client
{
    public class ClientManager : NetworkBehaviour
    {
        private void Start()
        {
            if (NetworkManager == null) return;

            NetworkManager.OnClientConnectedCallback += OnConnected;
            NetworkManager.OnClientDisconnectCallback += OnDisconnected;

            NetworkManager.StartClient();
            Debug.Log("Client started...");
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
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public GameObject playerPrefab;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                SpawnPlayer(client.ClientId);
            }
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        GameObject player = Instantiate(playerPrefab, GetSpawnPosition(clientId), Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    private Vector3 GetSpawnPosition(ulong clientId)
    {
        return Vector3.zero;
    }
}
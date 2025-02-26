using Player;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject mainTowerPrefab;
    [SerializeField] private GameObject sideTowerPrefab;

    [SerializeField] private Transform myMainTowerSpawnPoint;
    [SerializeField] private Transform mySide1TowerSpawnPoint;
    [SerializeField] private Transform mySide2TowerSpawnPoint;
    [SerializeField] private Transform enemyMainTowerSpawnPoint;
    [SerializeField] private Transform enemySide1TowerSpawnPoint;
    [SerializeField] private Transform enemySide2TowerSpawnPoint;

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

        // Sadece oyuncu kendi kulelerini oluşturmalı!
        SpawnTowers(clientId);
    }

    private void SpawnTowers(ulong clientId)
    {
        bool isFirstPlayer = clientId % 2 == 0;
    
        Transform mainSpawnPoint = isFirstPlayer ? myMainTowerSpawnPoint : enemyMainTowerSpawnPoint;
        Transform side1SpawnPoint = isFirstPlayer ? mySide1TowerSpawnPoint : enemySide1TowerSpawnPoint;
        Transform side2SpawnPoint = isFirstPlayer ? mySide2TowerSpawnPoint : enemySide2TowerSpawnPoint;

        // Ana kule
        GameObject ownMainTower = Instantiate(mainTowerPrefab, mainSpawnPoint);
        ownMainTower.transform.localPosition = Vector3.zero;
        ownMainTower.transform.localRotation = Quaternion.identity;
        ownMainTower.transform.localScale = Vector3.one;
        ownMainTower.GetComponent<NetworkObject>().Spawn();
        ownMainTower.GetComponent<Tower>().SetTowerColorServerRpc(isFirstPlayer); // Server RPC çağrısı

        // Yan kule 1
        GameObject ownSide1Tower = Instantiate(sideTowerPrefab, side1SpawnPoint);
        ownSide1Tower.transform.localPosition = Vector3.zero;
        ownSide1Tower.transform.localRotation = Quaternion.identity;
        ownSide1Tower.transform.localScale = Vector3.one;
        ownSide1Tower.GetComponent<NetworkObject>().Spawn();
        ownSide1Tower.GetComponent<Tower>().SetTowerColorServerRpc(isFirstPlayer);

        // Yan kule 2
        GameObject ownSide2Tower = Instantiate(sideTowerPrefab, side2SpawnPoint);
        ownSide2Tower.transform.localPosition = Vector3.zero;
        ownSide2Tower.transform.localRotation = Quaternion.identity;
        ownSide2Tower.transform.localScale = Vector3.one;
        ownSide2Tower.GetComponent<NetworkObject>().Spawn();
        ownSide2Tower.GetComponent<Tower>().SetTowerColorServerRpc(isFirstPlayer);
    }


    private Vector3 GetSpawnPosition(ulong clientId)
    {
        return Vector3.zero;
    }
}

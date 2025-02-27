using System.Collections.Generic;
using Interfaces;
using Player;
using UnityEngine;
using Unity.Netcode;
using VContainer;

public class GameManager : NetworkBehaviour,IGameManager
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
    [SerializeField] private Transform gameOriginPoint;
    public List<NetworkObject> SpawnedTowers { get; private set; } = new List<NetworkObject>();
    public List<NetworkObject> SpawnedUnits { get; private set; } = new List<NetworkObject>();
    
    private IObjectResolver _objectResolver;

    [Inject]
    public void Init(IObjectResolver resolver)
    {
        _objectResolver = resolver;
    }
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
        
        SpawnTowers(clientId);
    }

    private void SpawnTowers(ulong clientId)
    {
        bool isFirstPlayer = clientId % 2 == 0;
    
        Transform mainSpawnPoint = isFirstPlayer ? myMainTowerSpawnPoint : enemyMainTowerSpawnPoint;
        Transform side1SpawnPoint = isFirstPlayer ? mySide1TowerSpawnPoint : enemySide1TowerSpawnPoint;
        Transform side2SpawnPoint = isFirstPlayer ? mySide2TowerSpawnPoint : enemySide2TowerSpawnPoint;

        // Main Tower
        SpawnTower(mainTowerPrefab, mainSpawnPoint, clientId);

        // Side Tower 1
        SpawnTower(sideTowerPrefab, side1SpawnPoint, clientId);
        
        // Side Tower 2
        SpawnTower(sideTowerPrefab, side2SpawnPoint, clientId);
    }
    
    private void SpawnTower(GameObject prefab, Transform spawnPoint, ulong clientId)
    {
        GameObject tower = Instantiate(prefab, spawnPoint);
        tower.transform.localPosition = Vector3.zero;
        tower.transform.localRotation = Quaternion.identity;
        tower.transform.localScale = Vector3.one;
        var towerComponent = tower.GetComponent<Tower>();
        towerComponent.SetOwner(clientId);
        towerComponent.SetObjectResolver(_objectResolver);
        NetworkObject networkObject = tower.GetComponent<NetworkObject>();
        networkObject.SpawnWithOwnership(clientId);
        SpawnedTowers.Add(networkObject);
    }
    
    private Vector3 GetSpawnPosition(ulong clientId)
    {
        return Vector3.zero;
    }
    
    public Transform GetGameOriginPoint()
    {
        return gameOriginPoint;
    }
}

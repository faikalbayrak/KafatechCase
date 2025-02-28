using System.Collections.Generic;
using Interfaces;
using Player;
using UnityEngine;
using Unity.Netcode;
using VContainer;

public class GameManager : NetworkBehaviour, IGameManager
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
    
    public NetworkList<NetworkObjectReference> SpawnedPlayers { get; private set; }
    public NetworkList<NetworkObjectReference> SpawnedTowers { get; private set; }
    public NetworkList<NetworkObjectReference> SpawnedUnits { get; private set; }

    private IObjectResolver _objectResolver;

    private void Awake()
    {
        SpawnedPlayers = new NetworkList<NetworkObjectReference>();
        SpawnedTowers = new NetworkList<NetworkObjectReference>();
        SpawnedUnits = new NetworkList<NetworkObjectReference>();
    }

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
        NetworkObject playerNetworkObject = player.GetComponent<NetworkObject>();
        playerNetworkObject.SpawnAsPlayerObject(clientId);
        
        SpawnedPlayers.Add(new NetworkObjectReference(playerNetworkObject));

        player.GetComponent<NetworkGamePlayerController>().SetOwnerClientId(clientId);
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
        
        SpawnedTowers.Add(new NetworkObjectReference(networkObject));
    }

    private Vector3 GetSpawnPosition(ulong clientId)
    {
        return Vector3.zero;
    }

    public Transform GetGameOriginPoint()
    {
        return gameOriginPoint;
    }
    
    public List<NetworkObject> GetOpponentTowers(ulong myClientId)
    {
        List<NetworkObject> opponentTowers = new List<NetworkObject>();

        foreach (var towerRef in SpawnedTowers)
        {
            if (towerRef.TryGet(out NetworkObject networkObject))
            {
                if (networkObject.OwnerClientId != myClientId)
                {
                    opponentTowers.Add(networkObject);
                }
            }
        }

        return opponentTowers;
    }
    
    public void RegisterUnit(NetworkObject unit)
    {
        if (IsServer)
        {
            SpawnedUnits.Add(new NetworkObjectReference(unit));
        }
    }
}
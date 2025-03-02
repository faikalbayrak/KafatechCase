using System.Collections.Generic;
using Client;
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
    [SerializeField] private GameObject EndGamePanel;
    
    private Dictionary<ulong, int> towerCountByOwner = new Dictionary<ulong, int>();

    public NetworkList<NetworkObjectReference> SpawnedPlayers { get; private set; }
    public NetworkList<NetworkObjectReference> SpawnedTowers { get; private set; }
    public NetworkList<NetworkObjectReference> SpawnedUnits { get; private set; }

    private IObjectResolver _objectResolver;
    
    private bool gameEnded = false;

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

        if(player.TryGetComponent<NetworkGamePlayerController>(out var networkGamePlayerController))
        {
            networkGamePlayerController.SetObjectResolver(_objectResolver);
            networkGamePlayerController.SetOwnerClientId(clientId);
        }
        
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
        
        if (!towerCountByOwner.ContainsKey(clientId))
        {
            towerCountByOwner[clientId] = 0;
        }
        towerCountByOwner[clientId]++;
    }

    public void OnTowerDestroyed(ulong ownerClientId)
    {
        if (!IsServer) return;
        
        if (towerCountByOwner.ContainsKey(ownerClientId))
        {
            towerCountByOwner[ownerClientId]--;
            if (towerCountByOwner[ownerClientId] <= 0)
            {
                ulong winnerId = FindOtherPlayer(ownerClientId);
                Debug.Log($"Player {ownerClientId} lost, Player {winnerId} is the winner!");
                
                ShowEndGameClientRpc(winnerId);
            }
        }
    }
    
    private ulong FindOtherPlayer(ulong loserClientId)
    {
        foreach (var kvp in towerCountByOwner)
        {
            if (kvp.Key != loserClientId)
                return kvp.Key;
        }
        return 99999;
    }

    [ClientRpc]
    private void ShowEndGameClientRpc(ulong winnerId)
    {
        gameEnded = true;
        Debug.Log($"End Game! Winner is client {winnerId}");
        EndGamePanel.SetActive(true);

        if (EndGamePanel.TryGetComponent<EndGamePanelController>(out var endGamePanelController))
        {
            if(winnerId == NetworkManager.LocalClient.ClientId)
                endGamePanelController.SetForWinner();
            else
                endGamePanelController.SetForLoser();
        }
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
    
    public bool IsGameEnded()
    {
        return gameEnded;
    }
    
    public void RegisterUnit(NetworkObject unit)
    {
        if (IsServer)
        {
            SpawnedUnits.Add(new NetworkObjectReference(unit));
        }
    }
}
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Interfaces
{
    public interface IGameManager
    {
        public NetworkList<NetworkObjectReference> SpawnedPlayers => new NetworkList<NetworkObjectReference>();
        public NetworkList<NetworkObjectReference> SpawnedTowers => new NetworkList<NetworkObjectReference>();
        public NetworkList<NetworkObjectReference> SpawnedUnits => new NetworkList<NetworkObjectReference>();
        public Transform GetGameOriginPoint();
        public List<NetworkObject> GetOpponentTowers(ulong myClientId);
        public void RegisterUnit(NetworkObject unit);
        public void OnTowerDestroyed(ulong ownerClientId);
        public bool IsGameEnded();
    }
}

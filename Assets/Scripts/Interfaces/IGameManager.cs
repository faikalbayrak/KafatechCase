using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Interfaces
{
    public interface IGameManager
    {
        public List<NetworkObject> SpawnedTowers => new List<NetworkObject>();
        public List<NetworkObject> SpawnedUnits => new List<NetworkObject>();
    }
}

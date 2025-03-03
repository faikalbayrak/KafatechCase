using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class NetworkBulletController : NetworkBehaviour
    {
        #region Fields
        
        private float speed = 40f;
        private Vector3 targetPosition;
        private bool initialized = false;
        
        #endregion
        
        #region Unity Methods
        
        private void Update()
        {
            if (!IsServer) return;
            if (!initialized) return;
            
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            //transform.LookAt(targetPosition);
        }
        
        #endregion

        #region Public Methods
        
        public void Initialize(Vector3 target)
        {
            targetPosition = target;
            Invoke(nameof(DestroyBullet), .5f);
            initialized = true;
        }

        #endregion
        
        #region Private Methods
        
        private void DestroyBullet()
        {
            if (IsServer)
            {
                NetworkObject.Despawn();
            }
        }
        
        #endregion
    }
}
using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class NetworkBulletController : NetworkBehaviour
    {
        private float speed = 40f;
        private Vector3 targetPosition;
        private bool initialized = false;

        public void Initialize(Vector3 target)
        {
            targetPosition = target;
            Invoke(nameof(DestroyBullet), .5f);
            initialized = true;
        }

        private void Update()
        {
            if (!IsServer) return;
            if (!initialized) return;
            
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            //transform.LookAt(targetPosition);
        }

        private void DestroyBullet()
        {
            if (IsServer)
            {
                NetworkObject.Despawn();
            }
        }
    }
}
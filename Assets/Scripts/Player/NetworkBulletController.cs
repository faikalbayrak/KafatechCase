using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class NetworkBulletController : NetworkBehaviour
    {
        public float speed = 30f;
        private Vector3 targetPosition;
        private bool initialized = false;

        public void Initialize(Vector3 target)
        {
            targetPosition = target;
            Invoke(nameof(DestroyBullet), .7f);
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
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Player
{
    public class NetworkUnitAnimator : NetworkBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite[] walkSprites;
        [SerializeField] private Sprite[] attackSprites;
        [SerializeField] private float walkingFrameRate = 0.1f;
        [SerializeField] private float attackFrameRate = 0.05f;
        [SerializeField] private GameObject bulletPrefab;

        private float attackCooldownTimer = 0f;
        private float attackCooldownDuration = 1.5f;
        private int directionIndex = 0;
        private float animationTimer = 0;
        private int currentFrame = 0;
        private NavMeshAgent _agent;
        private NetworkUnitController _unitController;

        private NetworkVariable<AnimationState> currentState = new NetworkVariable<AnimationState>(
            AnimationState.Idle, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<int> networkDirectionIndex = new NetworkVariable<int>(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private enum AnimationState { Idle, Walk, Attack }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _unitController = GetComponent<NetworkUnitController>();
        }

        public override void OnNetworkSpawn()
        {
            networkDirectionIndex.OnValueChanged += (oldDir, newDir) => directionIndex = newDir;

            currentState.OnValueChanged += (oldState, newState) =>
            {
                if (newState == AnimationState.Idle)
                {
                    spriteRenderer.sprite = walkSprites[directionIndex * 8];
                }
            };
        }

        private void Update()
        {
            if (IsServer)
            {
                UpdateDirectionIndex();
            }
        }

        private void FixedUpdate()
        {
            AnimateSprite();
        }

        private void UpdateDirectionIndex()
        {
            Vector3 moveDirection = _agent.velocity.normalized;
            if (moveDirection.magnitude > 0.1f)
            {
                int newDirection = GetDirectionIndex(moveDirection);
                if (IsServer) networkDirectionIndex.Value = newDirection;
                else UpdateDirection(newDirection);
            }
        }

        private void UpdateDirection(int newDirection)
        {
            directionIndex = newDirection;
        }

        private void AnimateSprite()
        {
            if (currentState.Value == AnimationState.Idle) 
            {
                if (walkSprites.Length > directionIndex * 8)
                    spriteRenderer.sprite = walkSprites[directionIndex * 8];
                return;
            }

            float frameRate = currentState.Value == AnimationState.Attack ? attackFrameRate : walkingFrameRate;
            
            if (currentState.Value == AnimationState.Attack && attackCooldownTimer > 0)
            {
                attackCooldownTimer -= Time.fixedDeltaTime;
                return;
            }

            animationTimer += Time.fixedDeltaTime;

            if (animationTimer >= frameRate)
            {
                animationTimer = 0;
                currentFrame = (currentFrame + 1) % 8;

                int spriteIndex = directionIndex * 8 + currentFrame;
                if (walkSprites.Length > spriteIndex && attackSprites.Length > spriteIndex)
                {
                    switch (currentState.Value)
                    {
                        case AnimationState.Walk:
                            spriteRenderer.sprite = walkSprites[spriteIndex];
                            break;
                        case AnimationState.Attack:
                            spriteRenderer.sprite = attackSprites[spriteIndex];
                            if (spriteIndex == 0)
                            {
                                if (_unitController.CanAttack)
                                {
                                    SpawnBullet(_unitController.BulletTargetPosition);
                                }
                            }
                            
                            if (currentFrame == 7)
                            {
                                SetWalkState(false);
                                if (_unitController.CanAttack)
                                {
                                    _unitController.DealDamage(.25f);
                                }
                                
                                attackCooldownTimer = attackCooldownDuration;
                            }
                            break;
                    }
                }
            }
        }

        private int GetDirectionIndex(Vector3 moveDirection)
        {
            if (moveDirection == Vector3.zero) return 0; // Hareket etmiyorsa varsayılan yön

            Vector3 reverseDirection = -moveDirection;

            float angle = Mathf.Atan2(reverseDirection.z, reverseDirection.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360; // Negatif açıyı pozitif hale getir

            if (angle >= 337.5f || angle < 22.5f) return 6;   // Sol (←)  -> Sağ (→) yerine
            if (angle >= 22.5f && angle < 67.5f) return 5;    // Sol-Alt (↙) -> Sağ-Alt (↘) yerine
            if (angle >= 67.5f && angle < 112.5f) return 4;   // Aşağı (↓) doğru
            if (angle >= 112.5f && angle < 157.5f) return 3;  // Sağ-Alt (↘) -> Sol-Alt (↙) yerine
            if (angle >= 157.5f && angle < 202.5f) return 2;  // Sağ (→) -> Sol (←) yerine
            if (angle >= 202.5f && angle < 247.5f) return 1;  // Sağ-Üst (↗) -> Sol-Üst (↖) yerine
            if (angle >= 247.5f && angle < 292.5f) return 0;  // Yukarı (↑) doğru
            return 7;                                         // Sol-Üst (↖) -> Sağ-Üst (↗) yerine
        }
        
        public void SetAttackState()
        {
            if (IsServer)
            {
                currentState.Value = AnimationState.Attack;
                currentFrame = 0;
                animationTimer = 0;
            }
            else
            {
                SetAttackStateServerRpc();
            }
        }

        [ServerRpc]
        private void SetAttackStateServerRpc()
        {
            currentState.Value = AnimationState.Attack;
            currentFrame = 0;
            animationTimer = 0;
        }

        public void SetWalkState(bool isWalking)
        {
            if (IsServer)
            {
                currentState.Value = isWalking ? AnimationState.Walk : AnimationState.Idle;
                currentFrame = 0;
                animationTimer = 0;
            }
            else
            {
                SetWalkStateServerRpc(isWalking);
            }
        }

        [ServerRpc]
        private void SetWalkStateServerRpc(bool isWalking)
        {
            currentState.Value = isWalking ? AnimationState.Walk : AnimationState.Idle;
            currentFrame = 0;
            animationTimer = 0;
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void SpawnBullet_ServerRpc(Vector3 targetPosition,ulong clientId = 0)
        {
            Vector3 spawnPosition = transform.position + transform.forward * 0.5f;
            
            GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);

            if (bullet.TryGetComponent<NetworkBulletController>(out var networkBulletController))
            {
                networkBulletController.Initialize(targetPosition);
            }
                
            NetworkObject networkObject = bullet.GetComponent<NetworkObject>();
            
            if (networkObject != null)
            {
                networkObject.SpawnWithOwnership(clientId);
            }
            else
            {
                Debug.LogError("Unit prefab'ında NetworkObject bulunamadı!");
            }
        }
        
        public void SpawnBullet(Vector3 _targetPosition)
        {
            if (!IsOwner) return;
            
            SpawnBullet_ServerRpc(_targetPosition,NetworkManager.LocalClient.ClientId);
        }
    }
}

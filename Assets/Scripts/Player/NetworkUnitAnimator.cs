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
        [SerializeField] private float frameRate = 0.1f;

        private int directionIndex = 0;
        private float animationTimer = 0;
        private int currentFrame = 0;
        private NavMeshAgent _agent;

        private NetworkVariable<AnimationState> currentState = new NetworkVariable<AnimationState>(
            AnimationState.Idle, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<int> networkDirectionIndex = new NetworkVariable<int>(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private enum AnimationState { Idle, Walk, Attack }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        public override void OnNetworkSpawn()
        {
            networkDirectionIndex.OnValueChanged += (oldDir, newDir) => directionIndex = newDir;

            currentState.OnValueChanged += (oldState, newState) =>
            {
                if (newState == AnimationState.Idle)
                {
                    spriteRenderer.sprite = walkSprites[directionIndex * 8]; // Idle için ilk frame
                }
            };
        }

        private void Update()
        {
            if (IsServer)
            {
                UpdateAnimationState();
                UpdateDirectionIndex();
            }
        }
        private void FixedUpdate()
        {
            AnimateSprite();
        }

        private void UpdateAnimationState()
        {
            if (_agent.velocity.magnitude < 0.1f)
            {
                SetAnimationState(AnimationState.Idle);
            }
            else
            {
                SetAnimationState(AnimationState.Walk);
            }
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

        private void AnimateSprite()
        {
            if (currentState.Value == AnimationState.Idle) return;

            animationTimer += Time.fixedDeltaTime;
            if (animationTimer >= frameRate)
            {
                animationTimer = 0;
                currentFrame = (currentFrame + 1) % 8;

                int spriteIndex = directionIndex * 8 + currentFrame;
                if (spriteIndex >= 64) return;

                spriteRenderer.sprite = currentState.Value == AnimationState.Walk ? walkSprites[spriteIndex] : attackSprites[spriteIndex];
            }
        }

        private int GetDirectionIndex(Vector3 moveDirection)
        {
            Vector3 reverseDirection = -moveDirection;
            
            float angle = Mathf.Atan2(reverseDirection.z, reverseDirection.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360;

            if (angle >= 337.5f || angle < 22.5f) return 2;
            if (angle >= 22.5f && angle < 67.5f) return 3;
            if (angle >= 67.5f && angle < 112.5f) return 4;
            if (angle >= 112.5f && angle < 157.5f) return 5;
            if (angle >= 157.5f && angle < 202.5f) return 6;
            if (angle >= 202.5f && angle < 247.5f) return 7;
            if (angle >= 247.5f && angle < 292.5f) return 0;
            return 1;
        }

        
        private void SetAnimationState(AnimationState newState)
        {
            currentState.Value = newState;
        }
        
        private void UpdateDirection(int newDirection)
        {
            networkDirectionIndex.Value = newDirection;
        }

        
        public void SetAttackState(bool isAttacking)
        {
            currentState.Value = isAttacking ? AnimationState.Attack : AnimationState.Walk;
            currentFrame = 0;
        }
    }
}

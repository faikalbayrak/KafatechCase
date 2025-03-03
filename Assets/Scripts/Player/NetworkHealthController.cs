using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class NetworkHealthController : NetworkBehaviour
    {
        #region Serializable Fields

        [SerializeField] private int maxHealth = 100;
        [SerializeField] private Image customHealthBar;
        [SerializeField] private bool isTower = false;
        [SerializeField] private bool isMainTower = false;
        [SerializeField] private bool isSideTower = false;
        #endregion
        
        #region Fields
        
        private NetworkVariable<int> currentHealth = new NetworkVariable<int>(
            100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
        );
        
        #endregion

        #region Event Actions

        public event Action<int, int> OnHealthChanged;
        public event Action OnDeath;

        #endregion

        #region Public Methods
        
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                currentHealth.Value = maxHealth;
                currentHealth.OnValueChanged += OnHealthValueChanged;
                UpdateHpBarClientRpc(currentHealth.Value, maxHealth);
            }
        }
        public void TakeDamage(int damage)
        {
            if (!IsServer) return;
            if (currentHealth.Value <= 0) return;

            currentHealth.Value -= damage;
            OnHealthChanged?.Invoke(currentHealth.Value, maxHealth);
            
            if (currentHealth.Value <= 0)
            {
                currentHealth.Value = 0;
                DespawnObject();
            }
        }

        public void Heal(int amount)
        {
            if (!IsServer) return;
            if (currentHealth.Value <= 0) return;

            currentHealth.Value = Mathf.Min(maxHealth, currentHealth.Value + amount);
            OnHealthChanged?.Invoke(currentHealth.Value, maxHealth);
        }

        #endregion
        
        #region Private Methods
        
        private void OnHealthValueChanged(int oldValue, int newValue)
        {
            UpdateHpBarClientRpc(newValue, maxHealth);
        }
        
        [ClientRpc]
        private void UpdateHpBarClientRpc(int currentHp, int maxHp)
        {
            if (customHealthBar != null)
            {
                RectTransform rt = customHealthBar.GetComponent<RectTransform>();

                float leftValue = 0;
                if(!isTower)
                    leftValue = 280 - ((float)currentHp / maxHp) * (280 - 5);
                else
                {
                    if(isMainTower)
                        leftValue = 600 - ((float)currentHp / maxHp) * 600;
                    else
                    {
                        leftValue = 500 - ((float)currentHp / maxHp) * 500;
                    }
                }
                
                rt.offsetMin = new Vector2(leftValue, rt.offsetMin.y);
            }
            else
            {
                Debug.LogError("HealthBar is null!");
            }
        }

        private void DespawnObject()
        {
            OnDeath?.Invoke();
            if (NetworkObject != null)
            {
                NetworkObject.Despawn();
            }
            else
            {
                Debug.LogError("NetworkObject is null!");
            }
        }
        
        #endregion
    }
}
using System;
using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class NetworkHealthController : NetworkBehaviour
    {
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private ProgressBarPro healthBar;

        private NetworkVariable<int> currentHealth = new NetworkVariable<int>(
            100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
        );

        public event Action<int, int> OnHealthChanged;
        public event Action OnDeath;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                currentHealth.Value = maxHealth;
            }
        }

        public void TakeDamage(int damage)
        {
            if (!IsServer) return;
            if (currentHealth.Value <= 0) return;

            currentHealth.Value = Mathf.Max(0, currentHealth.Value - damage);
            OnHealthChanged?.Invoke(currentHealth.Value, maxHealth);
            SetHpBarValue(currentHealth.Value, maxHealth);
            if (currentHealth.Value == 0)
            {
                DespawnObject();
            }
        }

        public void Heal(int amount)
        {
            if (!IsServer) return;
            if (currentHealth.Value <= 0) return;

            currentHealth.Value = Mathf.Min(maxHealth, currentHealth.Value + amount);
            OnHealthChanged?.Invoke(currentHealth.Value, maxHealth);
            SetHpBarValue(currentHealth.Value, maxHealth);
        }
        
        private void SetHpBarValue(int currentHp, int maxHp)
        {
            if (healthBar != null)
            {
                healthBar.SetValue(currentHp, maxHp);
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
                Debug.LogError("NetworkObject bulunamadı, despawn başarısız!");
            }
        }
    }
}

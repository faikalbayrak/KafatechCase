using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class NetworkHealthController : NetworkBehaviour
    {
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private Image customHealthBar;
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

            currentHealth.OnValueChanged += OnHealthValueChanged;
            UpdateHpBarClientRpc(currentHealth.Value, maxHealth);
        }

        private void OnHealthValueChanged(int oldValue, int newValue)
        {
            Debug.Log($"Health Değişti: {newValue}");
            UpdateHpBarClientRpc(newValue, maxHealth);
        }

        public void TakeDamage(int damage)
        {
            if (!IsServer) return;
            if (currentHealth.Value <= 0) return;

            currentHealth.Value = Mathf.Max(0, currentHealth.Value - damage);
            OnHealthChanged?.Invoke(currentHealth.Value, maxHealth);

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
        }

        [ClientRpc]
        private void UpdateHpBarClientRpc(int currentHp, int maxHp)
        {
            if (customHealthBar != null)
            {
                RectTransform rt = customHealthBar.GetComponent<RectTransform>();
                
                float leftValue = 280 - ((float)currentHp / maxHp) * (280 - 5);
                
                rt.offsetMin = new Vector2(leftValue, rt.offsetMin.y);
            }
            else
            {
                Debug.LogError("HealthBar referansı boş!");
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
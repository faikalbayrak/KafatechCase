using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class Tower : NetworkBehaviour
    {
        [SerializeField] private Sprite playerTowerSprite;
        [SerializeField] private Sprite enemyTowerSprite;

        private SpriteRenderer spriteRenderer;
    
        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        [ServerRpc] // Sunucuya gönderilir
        public void SetTowerColorServerRpc(bool isOwnTower)
        {
            SetTowerColorClientRpc(isOwnTower);
        }

        [ClientRpc] // Tüm istemcilere gönderilir
        private void SetTowerColorClientRpc(bool isOwnTower)
        {
            if (spriteRenderer == null)
            {
                Debug.LogError("SpriteRenderer bulunamadı!");
                return;
            }
            
            spriteRenderer.sprite = isOwnTower ? playerTowerSprite : enemyTowerSprite;
        }
    }
}
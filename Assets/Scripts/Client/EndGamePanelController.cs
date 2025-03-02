using UnityEngine;

namespace Client
{
    public class EndGamePanelController : MonoBehaviour
    {
        [SerializeField] private GameObject winnerPanel;
        [SerializeField] private GameObject loserPanel;
    
        public void SetForWinner()
        {
            winnerPanel.SetActive(true);
            loserPanel.SetActive(false);
        }
    
        public void SetForLoser()
        {
            winnerPanel.SetActive(false);
            loserPanel.SetActive(true);
        }
    }
}

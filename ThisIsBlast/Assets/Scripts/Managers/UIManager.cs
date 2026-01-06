using UnityEngine;

namespace Managers
{
    public class UIManager : MonoSingleton<UIManager>
    {
        [Header("Win-Lose Condition Panels")]
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;
        
        public void ShowWinPanel() => winPanel.SetActive(true); 
        public void HideWinPanel() => winPanel.SetActive(false);
        public void ShowLosePanel() => losePanel.SetActive(true); 
        public void HideLosePanel() => losePanel.SetActive(false);
        
        public void OnRestartButtonClicked()
        {
            HideWinPanel();        
            HideLosePanel();
            GameManager.Instance.RestartLevel();
        }
    }
}
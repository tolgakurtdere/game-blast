using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace TK.Blast
{
    public class HomeLayout : UILayout
    {
        [SerializeField] private Button playButton;
        [SerializeField] private TextMeshProUGUI levelText;

        protected override void Awake()
        {
            base.Awake();
            playButton.onClick.AddListener(LevelManager.StartReachedLevel);
        }

        public override async Task ShowAsync()
        {
            UpdateLevelText();
            await base.ShowAsync();
        }

        private void UpdateLevelText()
        {
            levelText.text = LevelManager.ReachedLevelNo > LevelManager.TotalLevelCount
                ? "Finished!"
                : $"Level {LevelManager.ReachedLevelNo}";
        }
    }
}
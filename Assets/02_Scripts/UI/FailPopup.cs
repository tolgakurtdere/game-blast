using UnityEngine;
using UnityEngine.UI;

namespace TK.Blast
{
    public class FailPopup : UIPopup
    {
        [SerializeField] private Button tryAgainButton;
        [SerializeField] private Button closeButton;

        protected override void Awake()
        {
            base.Awake();
            tryAgainButton.onClick.AddListener(OnTryAgainClicked);
            closeButton.onClick.AddListener(OnCloseClicked);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            tryAgainButton.onClick.RemoveListener(OnTryAgainClicked);
            closeButton.onClick.RemoveListener(OnCloseClicked);
        }

        private async void OnTryAgainClicked()
        {
            await HideAsync();
            LevelManager.StartReachedLevel();
        }

        private async void OnCloseClicked()
        {
            await HideAsync();
            await LevelManager.ReturnToMainMenuAsync();
        }
    }
}
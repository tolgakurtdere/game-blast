using TMPro;
using UnityEngine;

namespace TK.Blast
{
    public class GameplayLayout : UILayout
    {
        [SerializeField] private TextMeshProUGUI moveCountText;

        protected override void Awake()
        {
            base.Awake();
            LevelManager.OnMoveCountChanged += SetMoveCountText;
        }

        public void Init(int moveCount)
        {
            SetMoveCountText(moveCount);
        }

        private void SetMoveCountText(int remainingMoveCount)
        {
            moveCountText.text = remainingMoveCount.ToString();
        }
    }
}
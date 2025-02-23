using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TK.Blast
{
    public class GoalDisplayer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private Image image;
        [SerializeField] private Image doneIcon;
        private int _count;

        private int Count
        {
            get => _count;
            set
            {
                if (value < 0) return;

                _count = value;
                countText.text = _count.ToString();
            }
        }

        public void Initialize(Sprite sprite, int count)
        {
            countText.gameObject.SetActive(true);
            doneIcon.gameObject.SetActive(false);

            image.sprite = sprite;
            Count = count;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void DecreaseCount()
        {
            Count--;
            if (Count == 0)
            {
                SetDoneVisual();
            }
        }

        private void SetDoneVisual()
        {
            countText.gameObject.SetActive(false);
            doneIcon.gameObject.SetActive(true);
            doneIcon.transform.DOScale(1f, 0.2f).From(0f);
            doneIcon.DOFade(1f, 0.1f).From(0f);
        }
    }
}
using UnityEngine;
using System.Threading.Tasks;
using DG.Tweening;

namespace TK.Blast
{
    public abstract class UIPopup : UIBase
    {
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected RectTransform popupHolder;

        [Header("Animation Settings")]
        [SerializeField] protected int slideDistance = 1080;
        [SerializeField] protected float showDuration = 0.5f;
        [SerializeField] protected float hideDuration = 0.4f;
        [SerializeField] protected Ease showEase = Ease.InCubic;
        [SerializeField] protected Ease hideEase = Ease.InBack;

        private Vector2 _defaultAnchoredPosition;
        private Sequence _currentSequence;

        protected override void Awake()
        {
            base.Awake();
            _defaultAnchoredPosition = popupHolder.anchoredPosition;
        }

        public override async Task ShowAsync()
        {
            await base.ShowAsync();

            _currentSequence?.Kill();

            // Setup initial state
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;
            popupHolder.anchoredPosition = _defaultAnchoredPosition + new Vector2(slideDistance, 0);

            // Create animation sequence
            _currentSequence = DOTween.Sequence()
                .Join(popupHolder.DOAnchorPos(_defaultAnchoredPosition, showDuration).SetEase(showEase));

            await _currentSequence.AsyncWaitForCompletion();
            canvasGroup.interactable = true;
        }

        public override async Task HideAsync()
        {
            _currentSequence?.Kill();
            canvasGroup.interactable = false;

            // Create animation sequence
            _currentSequence = DOTween.Sequence()
                .Join(popupHolder.DOAnchorPos(_defaultAnchoredPosition - new Vector2(slideDistance, 0), hideDuration).SetEase(hideEase));

            await _currentSequence.AsyncWaitForCompletion();
            canvasGroup.blocksRaycasts = false;
            await base.HideAsync();
        }

        protected virtual void OnDestroy()
        {
            _currentSequence?.Kill();
        }
    }
}
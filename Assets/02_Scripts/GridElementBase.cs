using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TK.Blast
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public abstract class GridElementBase : MonoBehaviour, IPointerClickHandler
    {
        public abstract List<GridElementType> MatchTypes { get; }
        public bool IsActive { get; protected set; } = true;

        [SerializeField] private GridElementType elementType;
        [SerializeField] private AudioClip destroySfx;
        [SerializeField] private Sprite sprite;
        private SpriteRenderer _spriteRenderer;

        private SpriteRenderer SpriteRenderer => _spriteRenderer ??= GetComponentInChildren<SpriteRenderer>();

        public GridElementType ElementType => elementType;
        public Sprite Sprite => sprite;

        protected virtual void Awake()
        {
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsActive) return;
            OnClick();
        }

        protected virtual void OnClick()
        {
        }

        public void SetSortingOrder(int order)
        {
            SpriteRenderer.sortingOrder = order;
        }

        public Tween Move(Vector2 to)
        {
            IsActive = false;
            return transform.DOMove(to, 0.2f)
                .SetEase(Ease.InOutSine)
                .OnComplete(() => { IsActive = true; });
        }

        public virtual void Destroy()
        {
            // AudioManager.Instance.PlaySound(destroySfx);
            Destroy(gameObject);
        }
    }
}
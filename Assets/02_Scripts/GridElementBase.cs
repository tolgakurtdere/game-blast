using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TK.Blast
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public abstract class GridElementBase : MonoBehaviour, IPointerClickHandler
    {
        public virtual bool CanFall => true;
        public abstract List<GridElementType> MatchTypes { get; }
        public bool IsActive { get; protected set; } = true;

        [SerializeField] private GridElementType elementType;
        private SpriteRenderer _spriteRenderer;

        protected SpriteRenderer SpriteRenderer => _spriteRenderer ??= GetComponentInChildren<SpriteRenderer>();
        public GridElementType ElementType => elementType;
        public Vector2Int Coordinate { get; private set; } = new(-1, -1);

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

        public void SetCoordinate(Vector2Int newCoordinate)
        {
            Coordinate = newCoordinate;
        }

        public void SetSortingOrder(int order)
        {
            SpriteRenderer.sortingOrder = order;
        }

        public Tween Move(Vector2 to, Ease ease = Ease.InOutSine)
        {
            IsActive = false;
            return transform.DOMove(to, 0.2f)
                .SetEase(ease)
                .OnComplete(() => { IsActive = true; });
        }

        public abstract Task<bool> Perform();

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}
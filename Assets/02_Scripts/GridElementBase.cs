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
            SetSortingOrder(newCoordinate.y);
        }

        public virtual void Highlight()
        {
            SetSortingOrder(100);
            // TODO: change visual such as add outline etc.
        }

        private void SetSortingOrder(int order)
        {
            SpriteRenderer.sortingOrder = order;
        }

        public Tween Move(Vector2 to, Ease ease = Ease.InOutSine, float duration = 0.3f)
        {
            return MoveInternal(to, duration).SetEase(ease);
        }

        public Tween Move(Vector2 to, AnimationCurve animationCurve, float duration = 0.3f)
        {
            return MoveInternal(to, duration).SetEase(animationCurve);
        }

        private Tween MoveInternal(Vector2 to, float duration)
        {
            IsActive = false;
            return transform.DOMove(to, duration).OnComplete(() => { IsActive = true; });
        }

        public abstract Task<bool> Perform(bool vfx);

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}
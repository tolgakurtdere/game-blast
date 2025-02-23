using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TK.Blast
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public abstract class GridElementBase : MonoBehaviour, IPointerClickHandler
    {
        public bool IsActive { get; protected set; } = true;

        private SpriteRenderer _spriteRenderer;
        protected SpriteRenderer SpriteRenderer => _spriteRenderer ??= GetComponentInChildren<SpriteRenderer>();

        public GridElementModel Model { get; private set; }
        public GridElementType ElementType => Model.ElementType;
        public bool CanFall => Model.CanFall;
        public Vector2Int Coordinate => Model.Coordinate;

        protected abstract GridElementModel Initialize();
        public abstract bool Interact();

        private void Awake()
        {
            Model = Initialize();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsActive) return;
            OnClick();
        }

        protected virtual void OnClick()
        {
        }

        public bool IsSameWith(GridElementBase other)
        {
            return Model.IsSameWith(other.Model);
        }

        public bool CanMatchWith(GridElementBase other)
        {
            return Model.CanMatchWith(other.Model);
        }

        public void SetCoordinate(Vector2Int newCoordinate)
        {
            Model.SetCoordinate(newCoordinate);
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

        public Tween CombineTo(Vector2 to)
        {
            Highlight();
            return MoveInternal(to, 0.2f).SetEase(Ease.InBack);
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

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}
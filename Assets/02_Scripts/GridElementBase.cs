using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace TK.Blast
{
    [DisallowMultipleComponent]
    public abstract class GridElementBase : MonoBehaviour
    {
        public abstract List<Type> MatchTypes { get; }
        public bool IsActive { get; protected set; } = true;

        [SerializeField] private GridElementType elementType;
        [SerializeField] private AudioClip destroySfx;
        [SerializeField] private Sprite sprite;
        private SpriteRenderer _spriteRenderer;

        private SpriteRenderer SpriteRenderer => _spriteRenderer ??= GetComponentInChildren<SpriteRenderer>();
        public Sprite Sprite => sprite;

        public void SetSortingOrder(int order)
        {
            SpriteRenderer.sortingOrder = order;
        }

        public Tween Move(Vector2 to, Ease ease = Ease.InOutSine)
        {
            IsActive = false;
            return transform.DOMove(to, 0.2f).OnComplete(() => { IsActive = true; });
        }

        public virtual void Destroy()
        {
            // AudioManager.Instance.PlaySound(destroySfx);
            Destroy(gameObject);
        }
    }
}
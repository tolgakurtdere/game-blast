using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TK.Blast
{
    public class Cube : GridElementBase
    {
        public override List<GridElementType> MatchTypes =>
            new() { ElementType, GridElementType.Box, GridElementType.Vase };

        [SerializeField] private Sprite rocketState;
        [SerializeField] private ParticleSystem crackFx;
        private Sprite _defaultState;

        protected override void Awake()
        {
            base.Awake();
            _defaultState = SpriteRenderer.sprite;
        }

        public void SetState(GridElementType? elementType = null)
        {
            SpriteRenderer.sprite = elementType switch
            {
                GridElementType.VerticalRocket or GridElementType.HorizontalRocket => rocketState,
                _ => _defaultState
            };
        }

        protected override void OnClick()
        {
            base.OnClick();
            GridManager.Instance.PerformMatching(Coordinate);
        }

        public override Task<bool> Perform(bool vfx)
        {
            if (vfx)
            {
                crackFx.transform.SetParent(null);
                crackFx.Play();
            }

            Destroy();
            return Task.FromResult(true);
        }
    }
}
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
        private Sprite _defaultState;

        protected override void Awake()
        {
            base.Awake();
            _defaultState = SpriteRenderer.sprite;
        }

        public void ShowRocketIndicator(bool show)
        {
            SpriteRenderer.sprite = show ? rocketState : _defaultState;
        }

        protected override void OnClick()
        {
            base.OnClick();
            GridManager.Instance.PerformMatching(Coordinate);
        }

        public override Task<bool> Perform()
        {
            Destroy();
            return Task.FromResult(true);
        }
    }
}
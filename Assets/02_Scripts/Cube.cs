using System.Threading.Tasks;
using UnityEngine;

namespace TK.Blast
{
    public class Cube : GridElementBase
    {
        [SerializeField] private GridElementColor color;
        [SerializeField] private Sprite rocketState;
        [SerializeField] private ParticleSystem crackFx;
        private Sprite _defaultState;

        protected override GridElementModel Initialize()
        {
            _defaultState = SpriteRenderer.sprite;
            return new CubeModel(color);
        }

        public void SetState(GridElementType? elementType = null)
        {
            SpriteRenderer.sprite = elementType switch
            {
                GridElementType.Rocket => rocketState,
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
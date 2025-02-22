using UnityEngine;

namespace TK.Blast
{
    public class Vase : ObstacleBase
    {
        [SerializeField] private Sprite brokenState;

        protected override GridElementModel Initialize()
        {
            return new ObstacleModel(ObstacleKind.Vase, 2);
        }

        protected override void OnHpChanged()
        {
            base.OnHpChanged();
            if (Hp == 1)
            {
                SpriteRenderer.sprite = brokenState;
            }
        }
    }
}
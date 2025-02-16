using System.Collections.Generic;

namespace TK.Blast
{
    public class Cube : GridElementBase
    {
        public override List<GridElementType> MatchTypes => new() { ElementType };

        // [SerializeField] private ParticleSystem crackFx;
        protected override void OnClick()
        {
            base.OnClick();
            GridManager.Instance.PerformMatching(this);
        }

        public override void Destroy()
        {
            // crackFx.transform.SetParent(null);
            // crackFx.Play();

            base.Destroy();
        }
    }
}
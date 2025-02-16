using System.Collections.Generic;
using UnityEngine;

namespace TK.Blast
{
    public abstract class ObstacleBase : GridElementBase
    {
        public override List<GridElementType> MatchTypes => new();
        // [SerializeField] private ParticleSystem crackFx;

        public override void Destroy()
        {
            // crackFx.transform.SetParent(null);
            // crackFx.Play();

            base.Destroy();
        }
    }
}
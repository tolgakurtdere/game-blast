using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TK.Blast
{
    public abstract class ObstacleBase : GridElementBase
    {
        public override List<GridElementType> MatchTypes => new();
        protected int Hp { get; set; } = 1;
        [SerializeField] private ParticleSystem crackFx;

        public override Task<bool> Perform(bool vfx)
        {
            Hp--;
            OnHpChanged();

            crackFx.transform.SetParent(null);
            crackFx.Play();

            var isCleared = Hp <= 0;
            if (isCleared) Destroy();

            return Task.FromResult(isCleared);
        }

        protected virtual void OnHpChanged()
        {
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TK.Blast
{
    public abstract class ObstacleBase : GridElementBase
    {
        public override List<GridElementType> MatchTypes => new();
        protected int Hp { get; set; } = 1;

        public override Task<bool> Perform()
        {
            Hp--;
            OnHpChanged();

            var isCleared = Hp <= 0;
            if (isCleared) Destroy();

            return Task.FromResult(isCleared);
        }

        protected virtual void OnHpChanged()
        {
        }
    }
}
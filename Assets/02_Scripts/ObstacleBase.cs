using System.Collections.Generic;
using System.Threading.Tasks;

namespace TK.Blast
{
    public abstract class ObstacleBase : GridElementBase
    {
        public override List<GridElementType> MatchTypes => new();
        public virtual int Hp { get; private set; } = 1;

        public override Task<bool> Perform()
        {
            Hp--;
            var isCleared = Hp <= 0;
            if (isCleared) Destroy();
            return Task.FromResult(isCleared);
        }
    }
}
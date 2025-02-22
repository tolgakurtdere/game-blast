using System.Threading.Tasks;
using UnityEngine;

namespace TK.Blast
{
    public abstract class ObstacleBase : GridElementBase
    {
        [SerializeField] private ParticleSystem crackFx;
        private ObstacleModel ObstacleModel => (ObstacleModel)Model;
        protected int Hp => ObstacleModel.Hp;

        public override Task<bool> Perform(bool vfx)
        {
            ObstacleModel.DecreaseHp();
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
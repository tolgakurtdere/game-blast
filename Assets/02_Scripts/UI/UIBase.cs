using UnityEngine;
using System.Threading.Tasks;

namespace TK.Blast
{
    public abstract class UIBase : MonoBehaviour
    {
        protected virtual void Awake()
        {
            gameObject.SetActive(false);
        }

        public virtual Task ShowAsync()
        {
            gameObject.SetActive(true);
            return Task.CompletedTask;
        }

        public virtual Task HideAsync()
        {
            gameObject.SetActive(false);
            return Task.CompletedTask;
        }
    }
}
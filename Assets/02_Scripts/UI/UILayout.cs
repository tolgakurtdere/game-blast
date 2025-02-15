using System.Threading.Tasks;
using UnityEngine;

namespace TK.Blast
{
    public abstract class UILayout : UIBase
    {
        private UILayout _previousLayout;

        public override async Task ShowAsync()
        {
            try
            {
                if (UIManager.CurrentLayout && UIManager.CurrentLayout != this)
                {
                    _previousLayout = UIManager.CurrentLayout;
                    await _previousLayout.HideAsync();
                }

                await base.ShowAsync();
                UIManager.CurrentLayout = this;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error showing {GetType().Name}: {e.Message}");
                throw;
            }
        }

        public override async Task HideAsync()
        {
            try
            {
                await base.HideAsync();
                if (UIManager.CurrentLayout == this)
                {
                    UIManager.CurrentLayout = null;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error hiding {GetType().Name}: {e.Message}");
                throw;
            }
        }
    }
}
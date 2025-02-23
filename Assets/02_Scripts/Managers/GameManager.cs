using UnityEngine;

namespace TK.Blast
{
    public class GameManager : MonoBehaviour
    {
        private async void Awake()
        {
            var homeLayout = await UIManager.GetUIAsync<HomeLayout>();
            await homeLayout.ShowAsync();
        }
    }
}
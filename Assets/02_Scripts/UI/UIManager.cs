using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace TK.Blast
{
    public class UIManager : SingletonBehaviour<UIManager>
    {
        [SerializeField] private Transform uiContainer;
        [SerializeField] private Image disablerOverlayImage;

        private readonly Dictionary<Type, UIBase> _activeUIs = new();
        public static UILayout CurrentLayout { get; internal set; }

        public static void SetDisablerOverlay(bool isEnabled)
        {
            Instance.disablerOverlayImage.enabled = isEnabled;
        }

        public static async Task<T> GetUIAsync<T>() where T : UIBase
        {
            if (Instance._activeUIs.TryGetValue(typeof(T), out var ui))
            {
                return (T)ui;
            }

            // Load UI prefab from Resources
            var prefab = Resources.Load<T>($"UI/{typeof(T).Name}");
            if (!prefab)
            {
                Debug.LogError($"UI prefab not found: UI/{typeof(T).Name}");
                return null;
            }

            SetDisablerOverlay(true);
            var instance = (await InstantiateAsync(prefab, Instance.uiContainer))[0];
            SetDisablerOverlay(false);

            Instance._activeUIs[typeof(T)] = instance;
            return instance;
        }

        public static void UnregisterUI<T>() where T : UIBase
        {
            if (Instance._activeUIs.TryGetValue(typeof(T), out var ui))
            {
                Instance._activeUIs.Remove(typeof(T));
                if (ui)
                {
                    Destroy(ui.gameObject);
                }
            }
        }
    }
}
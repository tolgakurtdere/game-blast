using System.Collections.Generic;
using UnityEngine;

namespace TK.Blast
{
    public static class GridElementFactory
    {
        private const string FOLDER_PATH = "GridElements/";
        private static readonly Dictionary<string, GridElementBase> s_pathCache = new();

        public static GridElementBase CreateElement(GridElementModel elementModel)
        {
            return LoadPrefab(elementModel.LoadPath);
        }

        public static GridElementBase CreateRandomCube()
        {
            return CreateElement(CubeModel.GetRandom());
        }

        public static GridElementBase CreateRandomRocket()
        {
            return CreateElement(RocketModel.GetRandom());
        }

        public static Sprite GetSprite(GridElementModel elementModel)
        {
            var prefab = LoadPrefab(elementModel.LoadPath);
            return !prefab ? null : prefab.GetComponentInChildren<SpriteRenderer>()?.sprite;
        }

        private static GridElementBase LoadPrefab(string elementPath)
        {
            if (!s_pathCache.TryGetValue(elementPath, out var prefab) || !prefab)
            {
                var path = $"{FOLDER_PATH}{elementPath}";
                prefab = Resources.Load<GridElementBase>(path);
                if (!prefab)
                {
                    Debug.LogError($"Failed to load prefab: {path}");
                    return null;
                }

                s_pathCache[elementPath] = prefab;
            }

            return prefab;
        }

        public static void UnloadResources()
        {
            s_pathCache.Clear();
            Resources.UnloadUnusedAssets();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void ClearCache()
        {
            UnloadResources();
        }
#endif
    }
}
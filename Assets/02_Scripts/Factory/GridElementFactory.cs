using System.Collections.Generic;
using UnityEngine;

namespace TK.Blast
{
    public static class GridElementFactory
    {
        private const string FOLDER_PATH = "GridElements/";
        private static readonly Dictionary<GridElementType, GridElementBase> s_prefabCache = new();

        private static readonly GridElementType[] s_colorCubes =
        {
            GridElementType.RedCube,
            GridElementType.GreenCube,
            GridElementType.BlueCube,
            GridElementType.YellowCube
        };

        public static GridElementBase CreateElement(GridElementType elementType)
        {
            return LoadPrefab(elementType);
        }

        public static GridElementBase CreateRandomCube()
        {
            var randomIndex = Random.Range(0, s_colorCubes.Length);
            var randomCube = s_colorCubes[randomIndex];
            return CreateElement(randomCube);
        }

        public static GridElementBase CreateRandomRocket()
        {
            var rocketType = Random.Range(0, 2) == 0
                ? GridElementType.VerticalRocket
                : GridElementType.HorizontalRocket;
            return CreateElement(rocketType);
        }

        public static Sprite GetSprite(GridElementType elementType)
        {
            var prefab = LoadPrefab(elementType);
            return !prefab ? null : prefab.GetComponentInChildren<SpriteRenderer>()?.sprite;
        }

        private static GridElementBase LoadPrefab(GridElementType elementType)
        {
            if (!s_prefabCache.TryGetValue(elementType, out var prefab) || !prefab)
            {
                var path = $"{FOLDER_PATH}{elementType}";
                prefab = Resources.Load<GridElementBase>(path);
                if (!prefab)
                {
                    Debug.LogError($"Failed to load prefab: {path}");
                    return null;
                }

                s_prefabCache[elementType] = prefab;
            }

            return prefab;
        }

        public static void UnloadResources()
        {
            s_prefabCache.Clear();
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
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace TK.Blast
{
    public static class LevelLoader
    {
        private const string LEVELS_PATH = "Settings/Levels";
        private static int? s_totalLevelCount;

        private static readonly Dictionary<string, GridElementType?> s_elementTypes = new()
        {
            { "r", GridElementType.RedCube },
            { "g", GridElementType.GreenCube },
            { "b", GridElementType.BlueCube },
            { "y", GridElementType.YellowCube },
            { "vro", GridElementType.VerticalRocket },
            { "hro", GridElementType.HorizontalRocket },
            { "bo", GridElementType.Box },
            { "s", GridElementType.Stone },
            { "v", GridElementType.Vase },
            { "rand", null }
        };

        public static GridElementType? ParseElementType(string elementType)
        {
            if (!s_elementTypes.TryGetValue(elementType, out var type))
            {
                Debug.LogError($"Invalid element type: {elementType}");
                return null;
            }

            return type;
        }

        public static int TotalLevelCount
        {
            get
            {
                s_totalLevelCount ??= Resources.LoadAll<TextAsset>(LEVELS_PATH).Length;
                return s_totalLevelCount.Value;
            }
        }

        public static LevelData LoadLevel(int levelNumber)
        {
            var levelFileName = $"level_{levelNumber:D2}";
            var jsonText = Resources.Load<TextAsset>($"{LEVELS_PATH}/{levelFileName}")?.text;

            if (string.IsNullOrEmpty(jsonText))
            {
                Debug.LogError($"Level file not found: {levelFileName}");
                return null;
            }

            try
            {
                var levelData = JsonConvert.DeserializeObject<LevelData>(jsonText);
                if (levelData?.LevelNumber != levelNumber)
                {
                    Debug.LogError($"Level number mismatch in {levelFileName}.json: Expected {levelNumber}, got {levelData?.LevelNumber}");
                    return null;
                }

                return levelData;
            }
            catch (JsonException e)
            {
                Debug.LogError($"Error parsing level file {levelFileName}: {e.Message}");
                return null;
            }
        }
    }
}
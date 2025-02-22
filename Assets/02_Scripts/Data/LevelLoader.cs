using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace TK.Blast
{
    public static class LevelLoader
    {
        private const string LEVELS_PATH = "Settings/Levels";
        private static int? s_totalLevelCount;

        private static readonly Dictionary<string, GridElementModel> s_elementModels = new()
        {
            { "r", CubeModel.Red },
            { "g", CubeModel.Green },
            { "b", CubeModel.Blue },
            { "y", CubeModel.Yellow },
            { "vro", RocketModel.Vertical },
            { "hro", RocketModel.Horizontal },
            { "bo", ObstacleModel.Box },
            { "s", ObstacleModel.Stone },
            { "v", ObstacleModel.Vase },
            { "rand", null }
        };

        public static GridElementModel ParseElement(string elementCode)
        {
            if (!s_elementModels.TryGetValue(elementCode, out var model))
            {
                Debug.LogError($"Invalid element code: {elementCode}");
                return null;
            }

            return model;
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
                    Debug.LogError(
                        $"Level number mismatch in {levelFileName}.json: Expected {levelNumber}, got {levelData?.LevelNumber}");
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
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TK.Blast
{
    [JsonObject(MemberSerialization.OptIn, NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public record LevelData
    {
        public int LevelNumber { get; }
        public int GridWidth { get; }
        public int GridHeight { get; }
        public int MoveCount { get; }
        public string[] Grid { get; }

        [JsonConstructor]
        public LevelData(int levelNumber, int gridWidth, int gridHeight, int moveCount, string[] grid)
        {
            if (gridWidth <= 0 || gridHeight <= 0)
                throw new ArgumentException("Grid dimensions must be positive");

            if (grid == null || grid.Length != gridWidth * gridHeight)
                throw new ArgumentException("Grid data is invalid or doesn't match dimensions");

            LevelNumber = levelNumber;
            GridWidth = gridWidth;
            GridHeight = gridHeight;
            MoveCount = moveCount;
            Grid = grid;
        }

        public GridElementBase[,] GetGridElements()
        {
            var gridElements = new GridElementBase[GridWidth, GridHeight];

            for (var y = 0; y < GridHeight; y++)
            {
                for (var x = 0; x < GridWidth; x++)
                {
                    var index = y * GridWidth + x;
                    if (index >= Grid.Length) continue;

                    var elementModel = LevelLoader.ParseElement(Grid[index]);
                    gridElements[x, y] = elementModel == null
                        ? GridElementFactory.CreateRandomCube()
                        : GridElementFactory.CreateElement(elementModel);
                }
            }

            return gridElements;
        }
    }
}
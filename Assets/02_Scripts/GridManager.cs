using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace TK.Blast
{
    public class GridManager : SingletonBehaviour<GridManager>
    {
        public static event Action OnMovePerformed;
        public static event Action<GridElementType> OnCellCleared;

        [SerializeField] private SpriteRenderer border;

        private const float CELL_SIZE = 1.42f;
        private const float BORDER_PADDING_X = 0.3f;
        private const float BORDER_PADDING_Y = 0.5f;
        private const float SPAWN_HEIGHT_OFFSET = 3f;

        private static readonly Vector2Int[] s_adjacentDirections =
        {
            new(0, 1), // Up
            new(1, 0), // Right
            new(0, -1), // Down
            new(-1, 0) // Left
        };

        private GridElementBase[,] _grid;
        private Vector2[,] _coordinates;
        private readonly HashSet<Vector2Int> _matchedCoordinates = new();
        private readonly Queue<Vector2Int> _matchQueue = new();
        private readonly List<Vector2Int> _emptyPositions = new();

        private int GridSizeX => _grid.GetLength(0);
        private int GridSizeY => _grid.GetLength(1);

        public void Initialize(LevelData levelData)
        {
            if (levelData == null)
            {
                Debug.LogError("Level data is null!");
                return;
            }

            CleanupExistingGrid();
            InitializeGridArrays(levelData);
            SetupBorder(levelData);
            CreateGridElements(levelData);
        }

        private void CleanupExistingGrid()
        {
            if (_grid == null) return;

            for (var x = 0; x < _grid.GetLength(0); x++)
            {
                for (var y = 0; y < _grid.GetLength(1); y++)
                {
                    if (_grid[x, y]) _grid[x, y].Destroy();
                }
            }
        }

        private void InitializeGridArrays(LevelData levelData)
        {
            _grid = new GridElementBase[levelData.GridWidth, levelData.GridHeight];
            _coordinates = new Vector2[levelData.GridWidth, levelData.GridHeight];
        }

        private void SetupBorder(LevelData levelData)
        {
            var width = levelData.GridWidth * CELL_SIZE + BORDER_PADDING_X;
            var height = levelData.GridHeight * CELL_SIZE + BORDER_PADDING_Y;
            border.size = new Vector2(width, height);
        }

        private void CreateGridElements(LevelData levelData)
        {
            var gridElements = levelData.GetGridElements();
            var centerPosition = (Vector2)border.transform.position;
            var gridOffset = CalculateGridOffset(levelData);
            var cellOffset = CalculateCellOffset(levelData);

            for (var y = 0; y < levelData.GridHeight; y++)
            {
                for (var x = 0; x < levelData.GridWidth; x++)
                {
                    CreateGridElement(x, y, gridElements, centerPosition, gridOffset, cellOffset);
                }
            }
        }

        private Vector2 CalculateGridOffset(LevelData levelData)
        {
            // ReSharper disable PossibleLossOfFraction
            return new Vector2(levelData.GridWidth / 2, levelData.GridHeight / 2); // Offset to center the grid
            // ReSharper restore PossibleLossOfFraction
        }

        private Vector2 CalculateCellOffset(LevelData levelData)
        {
            return new Vector2(
                levelData.GridWidth % 2 == 0 ? CELL_SIZE / 2f : 0f,
                levelData.GridHeight % 2 == 0 ? CELL_SIZE / 2f : 0f
            );
        }

        private void CreateGridElement(int x, int y, GridElementBase[,] gridElements, Vector2 centerPosition,
            Vector2 gridOffset, Vector2 cellOffset)
        {
            var spawnPosition = CalculateElementPosition(x, y, centerPosition, gridOffset, cellOffset);
            _coordinates[x, y] = spawnPosition;

            var elementPrefab = gridElements[x, y];
            if (!elementPrefab)
            {
                Debug.LogError($"Null element found at position ({x}, {y}). This should not happen!");
                return;
            }

            var element = Instantiate(elementPrefab, spawnPosition, Quaternion.identity, border.transform);
            element.SetSortingOrder(y);
            _grid[x, y] = element;
        }

        private Vector2 CalculateElementPosition(int x, int y, Vector2 centerPosition, Vector2 gridOffset,
            Vector2 cellOffset)
        {
            return centerPosition + new Vector2(
                (x - gridOffset.x) * CELL_SIZE + cellOffset.x,
                (y - gridOffset.y) * CELL_SIZE + cellOffset.y
            );
        }

        private void AdjustSortingOrders()
        {
            for (var x = 0; x < GridSizeX; x++)
            {
                for (var y = 0; y < GridSizeY; y++)
                {
                    var gridElement = _grid[x, y];
                    if (gridElement)
                    {
                        gridElement.SetSortingOrder(y);
                    }
                }
            }
        }

        public void PerformMatching(GridElementBase sourceGridElement)
        {
            _matchedCoordinates.Clear();

            var sourceCoord = _grid.CoordinatesOf(sourceGridElement);
            if (!IsValidCoordinate(sourceCoord))
            {
                Debug.LogError("Invalid source coordinate for matching");
                return;
            }

            FindMatchingElements(sourceCoord);

            // Only match if we have at least 2 matching cubes (including source)
            var matchedCubeCount = _matchedCoordinates.Count(coord =>
            {
                var element = _grid[coord.x, coord.y];
                return element && element.ElementType.IsCube();
            });

            if (matchedCubeCount < 2) return;

            // Clear matched cells
            foreach (var coord in _matchedCoordinates)
            {
                ClearCell(coord);
            }

            // Create rocket if 5 or more cubes matched
            if (matchedCubeCount >= 5)
            {
                var rocketPrefab = GridElementFactory.CreateRandomRocket();
                var rocket = Instantiate(rocketPrefab, _coordinates[sourceCoord.x, sourceCoord.y], Quaternion.identity,
                    border.transform);
                _grid[sourceCoord.x, sourceCoord.y] = rocket;
            }

            Fall();
            OnMovePerformed?.Invoke();
        }

        private void FindMatchingElements(Vector2Int sourceCoord)
        {
            var sourceElement = _grid[sourceCoord.x, sourceCoord.y];
            if (!sourceElement || !sourceElement.IsActive) return;

            _matchQueue.Clear();
            _matchQueue.Enqueue(sourceCoord);
            _matchedCoordinates.Add(sourceCoord);

            while (_matchQueue.Count > 0)
            {
                var current = _matchQueue.Dequeue();
                foreach (var direction in s_adjacentDirections)
                {
                    var neighbor = current + direction;
                    if (!IsValidMatch(sourceElement, neighbor)) continue;

                    _matchedCoordinates.Add(neighbor);
                    _matchQueue.Enqueue(neighbor);
                }
            }
        }

        private bool IsValidMatch(GridElementBase sourceElement, Vector2Int coord)
        {
            if (!IsValidCoordinate(coord) || _matchedCoordinates.Contains(coord))
                return false;

            var element = _grid[coord.x, coord.y];
            if (!element || !element.IsActive)
                return false;

            return sourceElement.MatchTypes.Contains(element.ElementType);
        }

        private bool IsValidCoordinate(Vector2Int coord)
        {
            return coord.x >= 0 && coord.x < GridSizeX && coord.y >= 0 && coord.y < GridSizeY;
        }

        private void Fall()
        {
            var seq = DOTween.Sequence();
            _emptyPositions.Clear();
            CollectEmptyPositions();

            if (_emptyPositions.Count == 0) return;

            // Process each column
            for (var x = 0; x < GridSizeX; x++)
            {
                ProcessColumn(x, seq);
            }

            seq.OnComplete(AdjustSortingOrders);
        }

        private void CollectEmptyPositions()
        {
            for (var x = 0; x < GridSizeX; x++)
            {
                for (var y = 0; y < GridSizeY; y++)
                {
                    if (!_grid[x, y])
                    {
                        _emptyPositions.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        private void ProcessColumn(int x, Sequence seq)
        {
            var columnEmptySpaces = CountEmptySpacesInColumn(x);
            if (columnEmptySpaces == 0) return;

            MoveExistingElementsDown(x, seq);
            FillEmptySpaces(x, columnEmptySpaces, seq);
        }

        private int CountEmptySpacesInColumn(int x)
        {
            var count = 0;
            for (var y = 0; y < GridSizeY; y++)
            {
                if (!_grid[x, y]) count++;
            }

            return count;
        }

        private void MoveExistingElementsDown(int x, Sequence seq)
        {
            for (var y = 0; y < GridSizeY; y++)
            {
                if (!_grid[x, y]) continue;

                var emptySpacesBelow = CountEmptySpacesBelow(x, y);
                if (emptySpacesBelow == 0) continue;

                var element = _grid[x, y];
                var newY = y - emptySpacesBelow;

                seq.Join(element.Move(_coordinates[x, newY]));
                _grid[x, newY] = element;
                _grid[x, y] = null;
            }
        }

        private int CountEmptySpacesBelow(int x, int targetY)
        {
            var count = 0;
            for (var y = 0; y < targetY; y++)
            {
                if (!_grid[x, y]) count++;
            }

            return count;
        }

        private void FillEmptySpaces(int x, int columnEmptySpaces, Sequence seq)
        {
            for (var i = 0; i < columnEmptySpaces; i++)
            {
                var y = GridSizeY - 1 - i;
                var targetPos = _coordinates[x, y];
                var spawnPos = targetPos + Vector2.up * SPAWN_HEIGHT_OFFSET;

                var element = GridElementFactory.CreateRandomCube();
                if (!element)
                {
                    Debug.LogError($"Failed to create random element at position ({x}, {y})");
                    continue;
                }

                var obj = Instantiate(element, spawnPos, Quaternion.identity, border.transform);
                seq.Join(obj.Move(targetPos));
                _grid[x, y] = obj;
            }
        }

        private void ClearCell(Vector2Int coord)
        {
            var matchedObject = _grid[coord.x, coord.y];
            if (!matchedObject) return;

            _grid[coord.x, coord.y] = null;
            OnCellCleared?.Invoke(matchedObject.ElementType);
            matchedObject.Destroy();
        }
    }

    public static class MyExtensions
    {
        public static Vector2Int CoordinatesOf<T>(this T[,] matrix, T value)
        {
            var w = matrix.GetLength(0);
            var h = matrix.GetLength(1);

            for (var x = 0; x < w; x++)
            {
                for (var y = 0; y < h; y++)
                {
                    var element = matrix[x, y];
                    if (element != null && element.Equals(value))
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }

            return new Vector2Int(-1, -1);
        }
    }
}
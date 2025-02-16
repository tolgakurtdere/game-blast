using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace TK.Blast
{
    public class GridManager : SingletonBehaviour<GridManager>
    {
        public static event Action OnMovePerformed;
        public static event Action<Type> OnCellCleared;

        [SerializeField] private SpriteRenderer border;

        private const float CELL_SIZE = 1.42f;
        private const float BORDER_PADDING_X = 0.3f;
        private const float BORDER_PADDING_Y = 0.5f;

        private GridElementBase[,] _grid;
        private Vector2[,] _coordinates;
        private readonly List<Vector2Int> _matchedCoordinates = new();

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

        public void PerformRocket(Rocket rocket)
        {
            var coords1 = new List<Vector2Int>();
            var coords2 = new List<Vector2Int>();
            var sourceCoord = _grid.CoordinatesOf(rocket);
            _grid[sourceCoord.x, sourceCoord.y] = null;

            switch (rocket.Direction)
            {
                case RocketDirection.Horizontal:
                {
                    for (var x = sourceCoord.x - 1; x >= 0; x--)
                    {
                        coords1.Add(new Vector2Int(x, sourceCoord.y));
                    }

                    for (var x = sourceCoord.x + 1; x < GridSizeX; x++)
                    {
                        coords2.Add(new Vector2Int(x, sourceCoord.y));
                    }

                    break;
                }
                case RocketDirection.Vertical:
                {
                    for (var y = sourceCoord.y - 1; y >= 0; y--)
                    {
                        coords1.Add(new Vector2Int(sourceCoord.x, y));
                    }

                    for (var y = sourceCoord.y + 1; y < GridSizeY; y++)
                    {
                        coords2.Add(new Vector2Int(sourceCoord.x, y));
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var seq = DOTween.Sequence();
            for (var i = 0; i < coords1.Count; i++)
            {
                var coord = coords1[i];
                seq.Join(DOVirtual.DelayedCall(0.05f * i, () => { ClearCell(coord); }));
            }

            for (var i = 0; i < coords2.Count; i++)
            {
                var coord = coords2[i];
                seq.Join(DOVirtual.DelayedCall(0.05f * i, () => { ClearCell(coord); }));
            }

            seq.OnComplete(Fall);
            OnMovePerformed?.Invoke();
        }

        public void PerformMatching(GridElementBase sourceGridElement)
        {
            _matchedCoordinates.Clear();

            var sourceCoord = _grid.CoordinatesOf(sourceGridElement);
            RecursiveMatch(sourceCoord, sourceCoord);

            var matchedCubeCount = _matchedCoordinates
                .FindAll(a => _grid[a.x, a.y]?.GetType().IsSubclassOf(typeof(Cube)) ?? false).Count;

            if (matchedCubeCount < 2)
            {
                return;
            }

            foreach (var coord in _matchedCoordinates)
            {
                ClearCell(coord);
            }

            if (matchedCubeCount >= 5)
            {
                var rocketPrefab = GridElementFactory.CreateRandomRocket();
                var r = Instantiate(rocketPrefab, _coordinates[sourceCoord.x, sourceCoord.y], Quaternion.identity,
                    border.transform);
                _grid[sourceCoord.x, sourceCoord.y] = r;
            }

            Fall();
            OnMovePerformed?.Invoke();
        }

        private void RecursiveMatch(Vector2Int sourceCoord, Vector2Int currentCoord)
        {
            // Check if the current position is out of bounds
            if (currentCoord.x < 0 || currentCoord.x >= GridSizeX || currentCoord.y < 0 || currentCoord.y >= GridSizeY)
            {
                return;
            }

            // Check if the current object is null or not active
            var currentObj = _grid[currentCoord.x, currentCoord.y];
            if (!currentObj || !currentObj.IsActive)
            {
                return;
            }

            if (!_grid[sourceCoord.x, sourceCoord.y].MatchTypes.Contains(currentObj.GetType()))
            {
                return;
            }

            if (!_matchedCoordinates.Contains(currentCoord))
            {
                _matchedCoordinates.Add(currentCoord);

                // Recursive calls for adjacent objects (up, down, left, right)
                RecursiveMatch(currentCoord, new Vector2Int(currentCoord.x, currentCoord.y + 1));
                RecursiveMatch(currentCoord, new Vector2Int(currentCoord.x, currentCoord.y - 1));
                RecursiveMatch(currentCoord, new Vector2Int(currentCoord.x + 1, currentCoord.y));
                RecursiveMatch(currentCoord, new Vector2Int(currentCoord.x - 1, currentCoord.y));
            }
        }

        private void Fall()
        {
            var seq = DOTween.Sequence();

            for (var x = 0; x < GridSizeX; x++)
            {
                for (var y = 1; y < GridSizeY; y++)
                {
                    if (_grid[x, y] != null && _grid[x, y - 1] == null)
                    {
                        // Move the game object one step down
                        var obj = _grid[x, y];
                        seq.Join(obj.Move(_coordinates[x, y - 1]));
                        _grid[x, y - 1] = _grid[x, y];
                        _grid[x, y] = null;
                        y = Mathf.Max(y - 2, 0); // Check the same position again for multiple empty spaces in a column
                    }
                }
            }

            seq.OnComplete(FillEmptyCells);
        }

        private void FillEmptyCells()
        {
            for (var x = 0; x < GridSizeX; x++)
            {
                for (var y = GridSizeY - 1; y >= 0; y--)
                {
                    if (!_grid[x, y])
                    {
                        Vector3 targetPos = _coordinates[x, y];
                        Vector3 spawnPos = new Vector2(targetPos.x, 3);

                        var element = GridElementFactory.CreateRandomCube();
                        if (!element)
                        {
                            Debug.LogError($"Failed to create random element at position ({x}, {y})");
                            continue;
                        }

                        var obj = Instantiate(element, spawnPos, Quaternion.identity, border.transform);
                        obj.Move(targetPos, Ease.OutBounce);
                        _grid[x, y] = obj;
                    }
                }
            }

            AdjustSortingOrders();
        }

        private void ClearCell(Vector2Int coord)
        {
            var matchedObject = _grid[coord.x, coord.y];
            if (matchedObject == null) return;

            _grid[coord.x, coord.y] = null;
            OnCellCleared?.Invoke(matchedObject.GetType());
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
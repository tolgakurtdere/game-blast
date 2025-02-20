using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace TK.Blast
{
    public class GridManager : SingletonBehaviour<GridManager>
    {
        public static event Action OnMovePerformed;
        public static event Action<GridElementType> OnCellCleared;

        [SerializeField] private SpriteRenderer border;
        [SerializeField] private AnimationCurve cubeFallEase;

        public const float CELL_SIZE = 1.42f;
        private const float BORDER_PADDING_X = 0.3f;
        private const float BORDER_PADDING_Y = 0.5f;
        private const float SPAWN_HEIGHT_OFFSET = 10f;
        private const int ROCKET_CREATION_COUNT = 4;

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

        private int GridWidth { get; set; }
        private int GridHeight { get; set; }
        public bool IsGridActive { get; private set; }
        public Vector2 GridCenter => border.transform.position;

        private int _activeRocketOperations;
        private bool _isFalling;

        public void Initialize(LevelData levelData)
        {
            if (levelData == null)
            {
                Debug.LogError("Level data is null!");
                return;
            }

            CleanupExistingGrid();
            InitializeGrid(levelData);

            IsGridActive = true;
        }

        private void CleanupExistingGrid()
        {
            if (_grid == null) return;

            for (var x = 0; x < _grid.GetLength(0); x++)
            {
                for (var y = 0; y < _grid.GetLength(1); y++)
                {
                    var element = _grid[x, y];
                    if (element) element.Destroy();
                }
            }

            _grid = null;
        }

        [SuppressMessage("ReSharper", "PossibleLossOfFraction")]
        private void InitializeGrid(LevelData levelData)
        {
            GridWidth = levelData.GridWidth;
            GridHeight = levelData.GridHeight;
            _grid = new GridElementBase[GridWidth, GridHeight];
            _coordinates = new Vector2[GridWidth, GridHeight];

            // Setup Border
            var width = GridWidth * CELL_SIZE + BORDER_PADDING_X;
            var height = GridHeight * CELL_SIZE + BORDER_PADDING_Y;
            border.size = new Vector2(width, height);

            // Create Grid Elements
            var gridElements = levelData.GetGridElements();
            var centerPosition = (Vector2)border.transform.position;
            var gridOffset = new Vector2(GridWidth / 2, GridHeight / 2); // Offset to center the grid
            var cellOffset = new Vector2(
                GridWidth % 2 == 0 ? CELL_SIZE / 2f : 0f,
                GridHeight % 2 == 0 ? CELL_SIZE / 2f : 0f
            );

            for (var y = 0; y < GridHeight; y++)
            {
                for (var x = 0; x < GridWidth; x++)
                {
                    var elementPrefab = gridElements[x, y];
                    if (!elementPrefab)
                    {
                        Debug.LogError($"Null element found at position ({x}, {y}). This should not happen!");
                        return;
                    }

                    CreateGridElement(x, y, elementPrefab, centerPosition, gridOffset, cellOffset);
                }
            }
        }

        private void CreateGridElement(int x, int y, GridElementBase elementPrefab, Vector2 centerPosition,
            Vector2 gridOffset, Vector2 cellOffset)
        {
            var spawnPosition = centerPosition + new Vector2(
                (x - gridOffset.x) * CELL_SIZE + cellOffset.x,
                (y - gridOffset.y) * CELL_SIZE + cellOffset.y
            );
            _coordinates[x, y] = spawnPosition;

            var element = Instantiate(elementPrefab, spawnPosition, Quaternion.identity, border.transform);
            element.SetCoordinate(new Vector2Int(x, y));
            _grid[x, y] = element;

            // Check for potential rocket matches after element creation
            UpdateRocketIndicators();
        }

        public async void PerformMatching(Vector2Int sourceCoord)
        {
            if (!IsGridActive) return;

            if (!IsValidCoordinate(sourceCoord))
            {
                Debug.LogError("Invalid source coordinate for performing");
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
            OnMovePerformed?.Invoke();

            // Create rocket if 4 or more cubes matched
            var shouldCreateRocket = matchedCubeCount >= ROCKET_CREATION_COUNT;
            var seq = DOTween.Sequence();

            // Animate matched cubes to source position if creating rocket
            if (shouldCreateRocket)
            {
                IsGridActive = false;
                foreach (var coord in _matchedCoordinates)
                {
                    if (coord == sourceCoord) continue;
                    var element = _grid[coord.x, coord.y];
                    if (!element || !element.ElementType.IsCube()) continue;

                    seq.Join(element.Move(_coordinates[sourceCoord.x, sourceCoord.y], Ease.InBack));
                }
            }

            await seq.AsyncWaitForCompletion();

            // Clear matched cells after animation
            foreach (var coord in _matchedCoordinates)
            {
                await PerformCell(coord);
            }

            if (shouldCreateRocket)
            {
                var rocketPrefab = GridElementFactory.CreateRandomRocket();
                var rocket = Instantiate(rocketPrefab, _coordinates[sourceCoord.x, sourceCoord.y], Quaternion.identity,
                    border.transform);
                rocket.SetCoordinate(sourceCoord);
                _grid[sourceCoord.x, sourceCoord.y] = rocket;
            }

            await Fall();
        }

        public async void PerformRocket(Vector2Int sourceCoord)
        {
            if (!IsGridActive || _isFalling) return;

            if (!IsValidCoordinate(sourceCoord))
            {
                Debug.LogError("Invalid source coordinate for performing");
                return;
            }

            OnMovePerformed?.Invoke();

            IsGridActive = false;
            _activeRocketOperations++;

            await PerformCell(sourceCoord);
        }

        private async Task TryCompleteRocketChain()
        {
            if (_activeRocketOperations == 0)
            {
                _isFalling = true;
                await Fall();
                _isFalling = false;
            }
        }

        public void TryPerformCell(Vector2Int coord)
        {
            if (!IsValidCoordinate(coord)) return;

            var element = _grid[coord.x, coord.y];
            if (!element || !element.IsActive) return;

            if (element is Rocket)
            {
                _activeRocketOperations++;
            }

            _ = PerformCell(coord);
        }

        public async Task PerformCell(Vector2Int coord)
        {
            var gridElement = _grid[coord.x, coord.y];
            if (!gridElement) return;

            var isCleared = await gridElement.Perform();
            if (isCleared)
            {
                _grid[coord.x, coord.y] = null;

                if (gridElement is Rocket)
                {
                    _activeRocketOperations--;
                    await TryCompleteRocketChain();
                }

                OnCellCleared?.Invoke(gridElement.ElementType);
            }
        }

        private void FindMatchingElements(Vector2Int sourceCoord)
        {
            var sourceElement = _grid[sourceCoord.x, sourceCoord.y];
            if (!sourceElement || !sourceElement.IsActive) return;

            _matchedCoordinates.Clear();
            FindMatches(sourceCoord, (current, neighbor) =>
            {
                var currentElement = _grid[current.x, current.y];
                var neighborElement = _grid[neighbor.x, neighbor.y];
                return currentElement.MatchTypes.Contains(neighborElement.ElementType);
            }, _matchedCoordinates);
        }

        private bool IsValidCoordinate(Vector2Int coord)
        {
            return coord.x >= 0 && coord.x < GridWidth && coord.y >= 0 && coord.y < GridHeight;
        }

        private async Task Fall()
        {
            var seq = DOTween.Sequence();

            // Process each column
            for (var x = 0; x < GridWidth; x++)
            {
                ProcessColumn(x, seq);
            }

            await seq.AsyncWaitForCompletion();
            UpdateRocketIndicators(); // Check for potential rocket matches after falling

            IsGridActive = true;
        }

        private void ProcessColumn(int x, Sequence seq)
        {
            var emptySpacesCount = CountEmptySpacesBelow(x);
            if (emptySpacesCount == 0) return;

            MoveExistingElementsDown(x, seq);
            FillEmptySpaces(x, emptySpacesCount, seq);
        }

        private int CountEmptySpacesBelow(int x, int targetY = -1)
        {
            var count = 0;
            var startY = targetY >= 0 ? targetY - 1 : GridHeight - 1;

            for (var y = startY; y >= 0; y--)
            {
                var element = _grid[x, y];
                if (!element) count++;
                else if (!element.CanFall) break;
            }

            return count;
        }

        private void MoveExistingElementsDown(int x, Sequence seq)
        {
            for (var y = 0; y < GridHeight; y++)
            {
                var element = _grid[x, y];
                if (!element || !element.CanFall) continue;

                var emptySpacesCount = CountEmptySpacesBelow(x, y);
                if (emptySpacesCount == 0) continue;

                var newY = y - emptySpacesCount;
                element.SetCoordinate(new Vector2Int(x, newY));
                seq.Join(element.Move(_coordinates[x, newY]));

                _grid[x, newY] = element;
                _grid[x, y] = null;
            }
        }

        private void FillEmptySpaces(int x, int emptySpacesCount, Sequence seq)
        {
            for (var i = 0; i < emptySpacesCount; i++)
            {
                var y = GridHeight - 1 - i;
                var targetPos = _coordinates[x, y];
                var spawnPos = targetPos + Vector2.up * SPAWN_HEIGHT_OFFSET;

                var element = GridElementFactory.CreateRandomCube();
                if (!element)
                {
                    Debug.LogError($"Failed to create random element at position ({x}, {y})");
                    continue;
                }

                var cube = Instantiate(element, spawnPos, Quaternion.identity, border.transform);
                cube.SetCoordinate(new Vector2Int(x, y));
                seq.Join(cube.Move(targetPos, cubeFallEase));

                _grid[x, y] = cube;
            }
        }

        private void UpdateRocketIndicators()
        {
            // Clear all indicators first
            for (var x = 0; x < GridWidth; x++)
            {
                for (var y = 0; y < GridHeight; y++)
                {
                    var element = _grid[x, y];
                    if (element && element.ElementType.IsCube())
                    {
                        ((Cube)element).ShowRocketIndicator(false);
                    }
                }
            }

            // Check each cube for potential rocket matches
            for (var x = 0; x < GridWidth; x++)
            {
                for (var y = 0; y < GridHeight; y++)
                {
                    var element = _grid[x, y];
                    if (!element || !element.ElementType.IsCube() || !element.IsActive) continue;

                    CheckPotentialRocketCreation(new Vector2Int(x, y));
                }
            }
        }

        private void CheckPotentialRocketCreation(Vector2Int coord)
        {
            var matchedCoords = new HashSet<Vector2Int>();
            FindMatches(coord, (current, neighbor) =>
            {
                var currentElement = _grid[current.x, current.y];
                var neighborElement = _grid[neighbor.x, neighbor.y];
                return currentElement.ElementType == neighborElement.ElementType;
            }, matchedCoords);

            if (matchedCoords.Count >= ROCKET_CREATION_COUNT)
            {
                foreach (var matchCoord in matchedCoords)
                {
                    var element = _grid[matchCoord.x, matchCoord.y];
                    ((Cube)element).ShowRocketIndicator(true);
                }
            }
        }

        private void FindMatches(
            Vector2Int startCoord,
            Func<Vector2Int, Vector2Int, bool> matchCondition,
            HashSet<Vector2Int> matchedCoords)
        {
            var matchQueue = new Queue<Vector2Int>();
            matchQueue.Enqueue(startCoord);
            matchedCoords.Add(startCoord);

            while (matchQueue.Count > 0)
            {
                var current = matchQueue.Dequeue();
                foreach (var direction in s_adjacentDirections)
                {
                    var neighbor = current + direction;
                    if (!IsValidCoordinate(neighbor) || matchedCoords.Contains(neighbor)) continue;

                    var neighborElement = _grid[neighbor.x, neighbor.y];
                    if (!neighborElement || !neighborElement.IsActive) continue;

                    if (matchCondition(current, neighbor))
                    {
                        matchedCoords.Add(neighbor);
                        matchQueue.Enqueue(neighbor);
                    }
                }
            }
        }
    }
}
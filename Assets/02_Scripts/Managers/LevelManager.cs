using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TK.Blast
{
    public class LevelManager : MonoBehaviour
    {
        public static event Action<int> OnLevelStarted;
        public static event Action<int, bool> OnLevelFinished;
        public static event Action<int> OnMoveCountChanged;
        public static event Action<ObstacleKind> OnObstacleCountChanged;

        private const string REACHED_LEVEL_INDEX_KEY = "tk.blast.reachedLevelIndex";
        private static int? s_reachedLevelIndex;
        private static int s_remainingMoveCount;
        private static Dictionary<ObstacleKind, int> s_goalsDict;
        private static bool s_isLevelEnding;

        private static int ReachedLevelIndex
        {
            get
            {
                s_reachedLevelIndex ??= PlayerPrefs.GetInt(REACHED_LEVEL_INDEX_KEY, 0);
                return s_reachedLevelIndex.Value;
            }
            set
            {
                if (s_reachedLevelIndex == value) return;
                s_reachedLevelIndex = value;
                PlayerPrefs.SetInt(REACHED_LEVEL_INDEX_KEY, value);
                PlayerPrefs.Save(); // to make sure it's saved immediately
            }
        }

        public static int CurrentLevelNo { get; private set; }
        public static int HighestCompletedLevelNo => ReachedLevelIndex;
        public static int ReachedLevelNo => ReachedLevelIndex + 1;
        public static int TotalLevelCount => LevelLoader.TotalLevelCount;

        public static int RemainingMoveCount
        {
            get => s_remainingMoveCount;
            private set
            {
                if (s_remainingMoveCount == value) return;
                s_remainingMoveCount = value;
                OnMoveCountChanged?.Invoke(value);
            }
        }

        /// <summary>
        /// Sets the reached level number for testing purposes.
        /// This method should only be used in the Unity Editor for testing different level states.
        /// </summary>
        /// <param name="levelNo">The level number to set (1-based, between 1 and TotalLevelCount)</param>
        /// <remarks>
        /// This is a testing utility that allows setting any level as reached.
        /// It bypasses the normal level progression flow and should not be used in production builds.
        /// </remarks>
        public static void SetReachedLevel(int levelNo)
        {
            if (levelNo < 1 || levelNo > TotalLevelCount)
            {
                Debug.LogError($"Invalid level number: {levelNo}");
                return;
            }

            ReachedLevelIndex = levelNo - 1;
        }

        private static void OnMovePerformed()
        {
            RemainingMoveCount--;

            // Check if we ran out of moves
            if (RemainingMoveCount <= 0)
            {
                s_isLevelEnding = true;
                UIManager.SetDisablerOverlay(true);
            }
        }

        private static void OnCellCleared(GridElementModel elementModel)
        {
            if (!elementModel.ElementType.IsObstacle()) return;

            var obstacleKind = ((ObstacleModel)elementModel).Kind;
            if (!s_goalsDict.ContainsKey(obstacleKind)) return;

            var count = --s_goalsDict[obstacleKind];
            if (count == 0) s_goalsDict.Remove(obstacleKind);

            OnObstacleCountChanged?.Invoke(obstacleKind);

            // Check if all goals are completed
            if (s_goalsDict.Count == 0)
            {
                s_isLevelEnding = true;
                UIManager.SetDisablerOverlay(true);
            }
        }

        private static void OnFallCompleted()
        {
            if (!s_isLevelEnding) return;

            // Check if all goals are completed
            if (s_goalsDict.Count == 0)
            {
                FinishLevel(true);
                return;
            }

            // Then check if we ran out of moves
            if (RemainingMoveCount <= 0)
            {
                FinishLevel(false);
            }
        }

        public static void StartReachedLevel()
        {
            var levelNo = ReachedLevelNo;
            if (levelNo > TotalLevelCount)
            {
                Debug.Log("You have completed all levels!");
                return;
            }

            _ = StartLevelAsync(levelNo);
        }

        public static async void RestartLevel()
        {
            try
            {
                await SceneManager.UnloadSceneAsync(1);
                StartReachedLevel();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error restarting level: {e.Message}");
            }
        }

        private static async Task StartLevelAsync(int levelNo)
        {
            if (levelNo < 1 || levelNo > TotalLevelCount)
            {
                Debug.LogError("levelNo is invalid!");
                return;
            }

            try
            {
                s_isLevelEnding = false;
                CurrentLevelNo = levelNo;
                UIManager.SetDisablerOverlay(true);

                // Load level data
                var levelData = LevelLoader.LoadLevel(levelNo);
                if (levelData == null)
                {
                    Debug.LogError($"Failed to load level {levelNo}");
                    return;
                }

                // Load the level scene additively
                var loadOperation = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
                if (loadOperation == null)
                {
                    Debug.LogError("loadOperation is null!");
                    return;
                }

                // Wait for the scene to load
                while (!loadOperation.isDone)
                {
                    await Task.Yield();
                }

                // Initialize grid
                GridManager.Instance.Initialize(levelData);

                // Set initial move count
                RemainingMoveCount = levelData.MoveCount;

                // Initialize goals
                s_goalsDict = new Dictionary<ObstacleKind, int>();
                foreach (var elementCode in levelData.Grid)
                {
                    var elementModel = LevelLoader.ParseElement(elementCode);
                    if (elementModel == null || !elementModel.ElementType.IsObstacle()) continue;

                    var obstacleKind = ((ObstacleModel)elementModel).Kind;
                    if (!s_goalsDict.TryAdd(obstacleKind, 1))
                    {
                        s_goalsDict[obstacleKind]++;
                    }
                }

                // Initialize UI
                var gameplayLayout = await UIManager.GetUIAsync<GameplayLayout>();
                gameplayLayout.Init(RemainingMoveCount, s_goalsDict);
                await gameplayLayout.ShowAsync();

                // Subscribe to move events
                GridManager.OnMovePerformed += OnMovePerformed;
                GridManager.OnCellCleared += OnCellCleared;
                GridManager.OnFallCompleted += OnFallCompleted;

                // Notify level start
                OnLevelStarted?.Invoke(levelNo);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error starting level: {e.Message}");
            }
            finally
            {
                UIManager.SetDisablerOverlay(false);
            }
        }

        public static async void FinishLevel(bool isSucceed)
        {
            try
            {
                UIManager.SetDisablerOverlay(true);

                GridManager.OnMovePerformed -= OnMovePerformed;
                GridManager.OnCellCleared -= OnCellCleared;
                GridManager.OnFallCompleted -= OnFallCompleted;

                if (isSucceed) await HandleWinAsync();
                else await HandleLoseAsync();

                OnLevelFinished?.Invoke(CurrentLevelNo, isSucceed);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error finishing level: {e.Message}");
            }
            finally
            {
                UIManager.SetDisablerOverlay(false);
            }
        }

        private static async Task HandleWinAsync()
        {
            // Update progress
            ReachedLevelIndex++;

            // Perform remaining special elements as rockets
            await GridManager.Instance.InteractAllSpecialElements();

            // Play celebration particles
            await ParticleManager.PlayCelebrationAsync();

            // Return to main scene
            await ReturnToMainMenuAsync();
        }

        private static async Task HandleLoseAsync()
        {
            var failPopup = await UIManager.GetUIAsync<FailPopup>();
            await failPopup.ShowAsync();
        }

        public static async Task ReturnToMainMenuAsync()
        {
            // Return to main scene and show home layout
            await SceneManager.UnloadSceneAsync(1);
            var homeLayout = await UIManager.GetUIAsync<HomeLayout>();
            await homeLayout.ShowAsync();
        }
    }
}
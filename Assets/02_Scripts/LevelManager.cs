using System;
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

        private const string REACHED_LEVEL_INDEX_KEY = "tk.blast.reachedLevelIndex";
        private static int? s_reachedLevelIndex;
        private static int s_remainingMoves;

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

        public static int RemainingMoves
        {
            get => s_remainingMoves;
            private set
            {
                if (s_remainingMoves == value) return;
                s_remainingMoves = value;
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

        private static async Task StartLevelAsync(int levelNo)
        {
            if (levelNo < 1 || levelNo > TotalLevelCount)
            {
                Debug.LogError("levelNo is invalid!");
                return;
            }

            try
            {
                CurrentLevelNo = levelNo;
                UIManager.SetDisablerOverlay(true);

                // Load level data
                var levelData = LevelLoader.LoadLevel(levelNo);
                if (levelData == null)
                {
                    Debug.LogError($"Failed to load level {levelNo}");
                    return;
                }

                // Set initial move count
                RemainingMoves = levelData.MoveCount;

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

                // Initialize UI
                var gameplayLayout = await UIManager.GetUIAsync<GameplayLayout>();
                await gameplayLayout.ShowAsync();

                // Subscribe to move events
                GridManager.OnMovePerformed += OnMovePerformed;

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

        private static void OnMovePerformed()
        {
            RemainingMoves--;
            if (RemainingMoves <= 0)
            {
                FinishLevel(false);
            }
        }

        public static async void FinishLevel(bool isSucceed)
        {
            try
            {
                UIManager.SetDisablerOverlay(true);
                GridManager.OnMovePerformed -= OnMovePerformed;

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
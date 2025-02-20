using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TK.Blast
{
    public class GameplayLayout : UILayout
    {
        [SerializeField] private TextMeshProUGUI moveCountText;
        [SerializeField] private RectTransform goalsContent;
        [SerializeField] private GoalDisplayer goalDisplayerPrefab;
        private readonly Dictionary<GridElementType, GoalDisplayer> _goalDisplayerDict = new();

        private void OnEnable()
        {
            LevelManager.OnMoveCountChanged += SetMoveCountText;
            LevelManager.OnObstacleCountChanged += OnObstacleCountChanged;
        }

        private void OnDisable()
        {
            LevelManager.OnMoveCountChanged -= SetMoveCountText;
            LevelManager.OnObstacleCountChanged -= OnObstacleCountChanged;
        }

        public void Init(int moveCount, Dictionary<GridElementType, int> goals)
        {
            SetMoveCountText(moveCount);

            foreach (var keyValuePair in _goalDisplayerDict)
            {
                keyValuePair.Value.Hide();
            }

            foreach (var (elementType, count) in goals)
            {
                var sprite = GridElementFactory.GetSprite(elementType);
                if (!_goalDisplayerDict.TryGetValue(elementType, out var goalDisplayer))
                {
                    goalDisplayer = Instantiate(goalDisplayerPrefab, goalsContent);
                    _goalDisplayerDict[elementType] = goalDisplayer;
                }

                goalDisplayer.Initialize(sprite, count);
                goalDisplayer.Show();
            }
        }

        private void SetMoveCountText(int remainingMoveCount)
        {
            moveCountText.text = remainingMoveCount.ToString();
        }

        private void OnObstacleCountChanged(GridElementType elementType)
        {
            if (_goalDisplayerDict.TryGetValue(elementType, out var goalDisplayer))
                goalDisplayer.DecreaseCount();
        }
    }
}
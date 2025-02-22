using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace TK.Blast
{
    public class Rocket : GridElementBase
    {
        [SerializeField] private RocketDirection direction;
        [SerializeField] private Transform rocketDefault;
        [SerializeField] private Transform rocketRight;
        [SerializeField] private Transform rocketLeft;
        private const float EXPLOSION_SPEED = 0.5f;
        private const float EXPLOSION_DISTANCE = 10f;

        protected override void OnClick()
        {
            base.OnClick();
            GridManager.Instance.PerformRocket(Coordinate);
        }

        protected override GridElementModel Initialize()
        {
            return new RocketModel(direction);
        }

        public override async Task<bool> Perform(bool vfx)
        {
            IsActive = false;
            rocketDefault.gameObject.SetActive(false);
            rocketRight.gameObject.SetActive(true);
            rocketLeft.gameObject.SetActive(true);
            Highlight();

            var seq = DOTween.Sequence().SetEase(Ease.Linear);

            // Animate rocket parts based on type and clear cells during movement
            switch (direction)
            {
                case RocketDirection.Vertical:
                    AnimateVerticalRocket(seq);
                    break;
                case RocketDirection.Horizontal:
                    AnimateHorizontalRocket(seq);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ElementType), ElementType, "Invalid rocket type");
            }

            await seq.AsyncWaitForCompletion();

            Destroy();
            return true;
        }

        private void AnimateHorizontalRocket(Sequence seq)
        {
            var rightTargets = new List<Vector2Int>();
            var leftTargets = new List<Vector2Int>();
            var gridCenter = GridManager.Instance.GridCenter;
            var row = GridManager.Instance.GetRow(Coordinate.y);
            foreach (var rowIndex in row)
            {
                GridManager.Instance.PerformCell(rowIndex);
            }

            // Animate right part
            seq.Join(rocketRight.DOMoveX(gridCenter.x + EXPLOSION_DISTANCE, EXPLOSION_SPEED)
                .OnUpdate(() =>
                {
                    var currentX = Mathf.RoundToInt((rocketRight.position.x - transform.position.x) / GridManager.CELL_SIZE);
                    var targetCoord = new Vector2Int(Coordinate.x + currentX, Coordinate.y);
                    if (!rightTargets.Contains(targetCoord))
                    {
                        rightTargets.Add(targetCoord);
                        // GridManager.Instance.TryPerformCell(targetCoord);
                    }
                }));

            // Animate left part
            seq.Join(rocketLeft.DOMoveX(gridCenter.x - EXPLOSION_DISTANCE, EXPLOSION_SPEED)
                .OnUpdate(() =>
                {
                    var currentX = Mathf.RoundToInt((rocketLeft.position.x - transform.position.x) / GridManager.CELL_SIZE);
                    var targetCoord = new Vector2Int(Coordinate.x + currentX, Coordinate.y);
                    if (!leftTargets.Contains(targetCoord))
                    {
                        leftTargets.Add(targetCoord);
                        // GridManager.Instance.TryPerformCell(targetCoord);
                    }
                }));
        }

        private void AnimateVerticalRocket(Sequence seq)
        {
            var upTargets = new List<Vector2Int>();
            var downTargets = new List<Vector2Int>();
            var gridCenter = GridManager.Instance.GridCenter;
            var column = GridManager.Instance.GetColumn(Coordinate.x);
            foreach (var columnIndex in column)
            {
                GridManager.Instance.PerformCell(columnIndex);
            }

            // Animate up part
            seq.Join(rocketRight.DOMoveY(gridCenter.y + EXPLOSION_DISTANCE, EXPLOSION_SPEED)
                .OnUpdate(() =>
                {
                    var currentY = Mathf.RoundToInt((rocketRight.position.y - transform.position.y) / GridManager.CELL_SIZE);
                    var targetCoord = new Vector2Int(Coordinate.x, Coordinate.y + currentY);
                    if (!upTargets.Contains(targetCoord))
                    {
                        upTargets.Add(targetCoord);
                        // GridManager.Instance.TryPerformCell(targetCoord);
                    }
                }));

            // Animate down part
            seq.Join(rocketLeft.DOMoveY(gridCenter.y - EXPLOSION_DISTANCE, EXPLOSION_SPEED)
                .OnUpdate(() =>
                {
                    var currentY = Mathf.RoundToInt((rocketLeft.position.y - transform.position.y) / GridManager.CELL_SIZE);
                    var targetCoord = new Vector2Int(Coordinate.x, Coordinate.y + currentY);
                    if (!downTargets.Contains(targetCoord))
                    {
                        downTargets.Add(targetCoord);
                        // GridManager.Instance.TryPerformCell(targetCoord);
                    }
                }));
        }
    }
}
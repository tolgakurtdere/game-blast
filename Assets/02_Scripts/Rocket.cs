using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace TK.Blast
{
    public class Rocket : GridElementBase
    {
        public override List<GridElementType> MatchTypes => new()
        {
            // GridElementType.HorizontalRocket,
            // GridElementType.VerticalRocket
        };

        [SerializeField] private Transform rocketRight;
        [SerializeField] private Transform rocketLeft;
        private const float EXPLOSION_SPEED = 0.5f;
        private const float EXPLOSION_DISTANCE = 15f;

        protected override void OnClick()
        {
            base.OnClick();
            GridManager.Instance.PerformRocket(Coordinate);
        }

        public override async Task<bool> Perform()
        {
            IsActive = false;
            transform.GetChild(0).gameObject.SetActive(false);
            rocketRight.gameObject.SetActive(true);
            rocketLeft.gameObject.SetActive(true);

            var seq = DOTween.Sequence().SetEase(Ease.Linear);
            var sourceCoord = Coordinate;

            // Animate rocket parts based on type and clear cells during movement
            switch (ElementType)
            {
                case GridElementType.HorizontalRocket:
                    AnimateHorizontalRocket(seq, sourceCoord);
                    break;
                case GridElementType.VerticalRocket:
                    AnimateVerticalRocket(seq, sourceCoord);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ElementType), ElementType, "Invalid rocket type");
            }

            try
            {
                await seq.AsyncWaitForCompletionWithCancellation(destroyCancellationToken);
            }
            catch (TaskCanceledException)
            {
                seq.Kill();
                return true;
            }

            Destroy();
            return true;
        }

        private void AnimateHorizontalRocket(Sequence seq, Vector2Int sourceCoord)
        {
            var rightTargets = new List<Vector2Int>();
            var leftTargets = new List<Vector2Int>();
            var gridCenter = GridManager.Instance.GridCenter;

            // Animate right part
            seq.Join(rocketRight.DOMoveX(gridCenter.x + EXPLOSION_DISTANCE, EXPLOSION_SPEED)
                .OnUpdate(() =>
                {
                    var currentX = Mathf.RoundToInt((rocketRight.position.x - transform.position.x) / GridManager.CELL_SIZE);
                    var targetCoord = new Vector2Int(sourceCoord.x + currentX, sourceCoord.y);
                    if (!rightTargets.Contains(targetCoord))
                    {
                        rightTargets.Add(targetCoord);
                        GridManager.Instance.TryPerformCell(targetCoord);
                    }
                }));

            // Animate left part
            seq.Join(rocketLeft.DOMoveX(gridCenter.x - EXPLOSION_DISTANCE, EXPLOSION_SPEED)
                .OnUpdate(() =>
                {
                    var currentX = Mathf.RoundToInt((rocketLeft.position.x - transform.position.x) / GridManager.CELL_SIZE);
                    var targetCoord = new Vector2Int(sourceCoord.x + currentX, sourceCoord.y);
                    if (!leftTargets.Contains(targetCoord))
                    {
                        leftTargets.Add(targetCoord);
                        GridManager.Instance.TryPerformCell(targetCoord);
                    }
                }));
        }

        private void AnimateVerticalRocket(Sequence seq, Vector2Int sourceCoord)
        {
            var upTargets = new List<Vector2Int>();
            var downTargets = new List<Vector2Int>();
            var gridCenter = GridManager.Instance.GridCenter;

            // Animate up part
            seq.Join(rocketRight.DOMoveY(gridCenter.y + EXPLOSION_DISTANCE, EXPLOSION_SPEED)
                .OnUpdate(() =>
                {
                    var currentY = Mathf.RoundToInt((rocketRight.position.y - transform.position.y) / GridManager.CELL_SIZE);
                    var targetCoord = new Vector2Int(sourceCoord.x, sourceCoord.y + currentY);
                    if (!upTargets.Contains(targetCoord))
                    {
                        upTargets.Add(targetCoord);
                        GridManager.Instance.TryPerformCell(targetCoord);
                    }
                }));

            // Animate down part
            seq.Join(rocketLeft.DOMoveY(gridCenter.y - EXPLOSION_DISTANCE, EXPLOSION_SPEED)
                .OnUpdate(() =>
                {
                    var currentY = Mathf.RoundToInt((rocketLeft.position.y - transform.position.y) / GridManager.CELL_SIZE);
                    var targetCoord = new Vector2Int(sourceCoord.x, sourceCoord.y + currentY);
                    if (!downTargets.Contains(targetCoord))
                    {
                        downTargets.Add(targetCoord);
                        GridManager.Instance.TryPerformCell(targetCoord);
                    }
                }));
        }
    }
}
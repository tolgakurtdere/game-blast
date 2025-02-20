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
        private const float EXPLOSION_SPEED = 1f;
        private const float EXPLOSION_DISTANCE = 10f;

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

            await seq.AsyncWaitForCompletion();
            Destroy();
            return true;
        }

        private void AnimateHorizontalRocket(Sequence seq, Vector2Int sourceCoord)
        {
            // Animate right part
            seq.Join(rocketRight.DOMoveX(EXPLOSION_DISTANCE, EXPLOSION_SPEED)
                .OnUpdate(() =>
                {
                    var currentX = Mathf.RoundToInt((rocketRight.position.x - transform.position.x) / GridManager.CELL_SIZE);
                    var targetCoord = new Vector2Int(sourceCoord.x + currentX, sourceCoord.y);
                    GridManager.Instance.TryPerformCell(targetCoord);
                }));

            // Animate left part
            seq.Join(rocketLeft.DOMoveX(-EXPLOSION_DISTANCE, EXPLOSION_SPEED)
                .OnUpdate(() =>
                {
                    var currentX = Mathf.RoundToInt((rocketLeft.position.x - transform.position.x) / GridManager.CELL_SIZE);
                    var targetCoord = new Vector2Int(sourceCoord.x + currentX, sourceCoord.y);
                    GridManager.Instance.TryPerformCell(targetCoord);
                }));
        }

        private void AnimateVerticalRocket(Sequence seq, Vector2Int sourceCoord)
        {
            // Animate up part
            seq.Join(rocketRight.DOMoveY(EXPLOSION_DISTANCE, EXPLOSION_SPEED)
                .OnUpdate(() =>
                {
                    var currentY = Mathf.RoundToInt((rocketRight.position.y - transform.position.y) / GridManager.CELL_SIZE);
                    var targetCoord = new Vector2Int(sourceCoord.x, sourceCoord.y + currentY);
                    GridManager.Instance.TryPerformCell(targetCoord);
                }));

            // Animate down part
            seq.Join(rocketLeft.DOMoveY(-EXPLOSION_DISTANCE, EXPLOSION_SPEED)
                .OnUpdate(() =>
                {
                    var currentY = Mathf.RoundToInt((rocketLeft.position.y - transform.position.y) / GridManager.CELL_SIZE);
                    var targetCoord = new Vector2Int(sourceCoord.x, sourceCoord.y + currentY);
                    GridManager.Instance.TryPerformCell(targetCoord);
                }));
        }
    }
}
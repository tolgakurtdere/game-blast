using System;
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
        [SerializeField] private ParticleSystem trailFxRight;
        [SerializeField] private ParticleSystem trailFxLeft;
        private const float ANIMATION_UNIT_DURATION = 0.05f;

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

            var tween = direction switch
            {
                RocketDirection.Vertical => AnimateVerticalRocket(),
                RocketDirection.Horizontal => AnimateHorizontalRocket(),
                _ => throw new ArgumentOutOfRangeException()
            };

            await tween.AsyncWaitForCompletion();

            Destroy();
            return true;
        }

        private Tween AnimateHorizontalRocket()
        {
            var row = GridManager.Instance.GetRowCoords(Coordinate.y);
            return AnimateRocket(
                row,
                Coordinate.x,
                (endValue, duration) => rocketRight.DOMoveX(endValue, duration),
                (endValue, duration) => rocketLeft.DOMoveX(endValue, duration)
            );
        }

        private Tween AnimateVerticalRocket()
        {
            var column = GridManager.Instance.GetColumnCoords(Coordinate.x);
            return AnimateRocket(
                column,
                Coordinate.y,
                (endValue, duration) => rocketRight.DOMoveY(endValue, duration),
                (endValue, duration) => rocketLeft.DOMoveY(endValue, duration)
            );
        }

        private Tween AnimateRocket(
            Vector2Int[] cells,
            int currentPos,
            Func<float, float, Tween> rightMoveFunc,
            Func<float, float, Tween> leftMoveFunc)
        {
            var rightSeq = DOTween.Sequence();
            var leftSeq = DOTween.Sequence();

            // Forward animation
            for (var i = currentPos + 1; i < cells.Length; i++)
            {
                var coord = cells[i];
                var pos = GridManager.Instance.GetCellPosition(coord);
                rightSeq.Append(rocketRight.DOMove(pos, ANIMATION_UNIT_DURATION)
                    .OnComplete(() => GridManager.Instance.TryPerformCell(coord)));
            }

            // Backward animation
            for (var i = currentPos - 1; i >= 0; i--)
            {
                var coord = cells[i];
                var pos = GridManager.Instance.GetCellPosition(coord);
                leftSeq.Append(rocketLeft.DOMove(pos, ANIMATION_UNIT_DURATION)
                    .OnComplete(() => GridManager.Instance.TryPerformCell(coord)));
            }

            // Final animations
            const float finalDistance = GridManager.CELL_SIZE * 4;
            const float duration = ANIMATION_UNIT_DURATION * 4;
            rightSeq.Append(rightMoveFunc(finalDistance, duration).SetRelative())
                .AppendCallback(() =>
                {
                    trailFxRight.transform.SetParent(null);
                    rocketRight.gameObject.SetActive(false);
                });
            leftSeq.Append(leftMoveFunc(-finalDistance, duration).SetRelative())
                .AppendCallback(() =>
                {
                    trailFxLeft.transform.SetParent(null);
                    rocketLeft.gameObject.SetActive(false);
                });

            return rightSeq.Insert(0, leftSeq);
        }
    }
}
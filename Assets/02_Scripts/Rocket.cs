using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace TK.Blast
{
    public class Rocket : GridElementBase
    {
        public override List<GridElementType> MatchTypes => new();

        [SerializeField] private Transform rocketRight;
        [SerializeField] private Transform rocketLeft;
        [SerializeField] private ParticleSystem[] trailFxs;

        protected override void OnClick()
        {
            base.OnClick();
            IsActive = false;

            var seq = DOTween.Sequence().SetEase(Ease.Linear);
            switch (ElementType)
            {
                case GridElementType.HorizontalRocket:
                    seq.Join(rocketRight.DOMoveX(10, (10 - rocketRight.transform.position.x) / 10f))
                        .Join(rocketLeft.DOMoveX(-10, (10 + rocketLeft.transform.position.x) / 10f));
                    break;
                case GridElementType.VerticalRocket:
                    seq.Join(rocketRight.DOMoveY(10, (10 - rocketRight.transform.position.y) / 10f))
                        .Join(rocketLeft.DOMoveY(-10, (10 + rocketLeft.transform.position.y) / 10f));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            foreach (var fx in trailFxs)
            {
                fx.Play();
            }

            seq.OnComplete(Destroy);
            // GridManager.Instance.PerformRocket(this);
        }
    }
}
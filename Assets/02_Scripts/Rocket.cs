using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace TK.Blast
{
    public enum RocketDirection
    {
        Horizontal,
        Vertical
    }

    public class Rocket : GridElementBase, IPointerClickHandler
    {
        public override List<Type> MatchTypes => new();

        public RocketDirection Direction { get; private set; }
        [SerializeField] private Transform rocketRight;
        [SerializeField] private Transform rocketLeft;
        [SerializeField] private ParticleSystem[] trailFxs;

        private void Awake()
        {
            if (Random.Range(0, 2) == 0)
            {
                Direction = RocketDirection.Horizontal;
            }
            else
            {
                Direction = RocketDirection.Vertical;
                transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsActive)
            {
                return;
            }

            IsActive = false;

            var seq = DOTween.Sequence().SetEase(Ease.Linear);
            switch (Direction)
            {
                case RocketDirection.Horizontal:
                    seq.Join(rocketRight.DOMoveX(10, (10 - rocketRight.transform.position.x) / 10f))
                        .Join(rocketLeft.DOMoveX(-10, (10 + rocketLeft.transform.position.x) / 10f));
                    break;
                case RocketDirection.Vertical:
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
            GridManager.Instance.PerformRocket(this);
        }
    }
}
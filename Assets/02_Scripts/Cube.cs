using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TK.Blast
{
    [RequireComponent(typeof(Collider2D))]
    public class Cube : GridElementBase, IPointerClickHandler
    {
        public override List<Type> MatchTypes => new() { GetType() };
        [SerializeField] private ParticleSystem crackFx;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsActive)
            {
                return;
            }

            GridManager.Instance.PerformMatching(this);
        }

        public override void Destroy()
        {
            crackFx.transform.SetParent(null);
            crackFx.Play();

            base.Destroy();
        }
    }
}
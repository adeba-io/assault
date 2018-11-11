using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Techniques;
using Assault.Managers;

namespace Assault.Boxes
{
    public class Hitbox : InteractionBox
    {
        Attack _attackData;

        HitDamageable onHitDamageable;

        private void OnDisable()
        {
            _owner = null;
            _attackData = default(Attack);
            onHitDamageable = null;
        }

        protected override void AssignBoxColor()
        {
            for (int i = 0; i < _renderShapes.Length; i++) _renderShapes[i].GetComponent<SpriteRenderer>().color = FighterManager.FM.hitboxColor;
        }

        public void Enable(GameObject owner, InteractionBoxData boxData, Attack attackData, HitDamageable hitDamageable)
        {
            Enable(owner, boxData);
            _attackData = attackData;
            onHitDamageable += hitDamageable;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (onHitDamageable != null)
            {
                FighterDamageable damageable = collision.GetComponent<FighterDamageable>();
                if (damageable)
                {
                    onHitDamageable(_attackData, damageable);
                }
            }
        }

        
    }

    public delegate void HitDamageable(Attack attack, FighterDamageable damageable);
}

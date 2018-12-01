using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assault.Managers
{
    public class FighterManager : MonoBehaviour
    {
        [SerializeField] [Range(0.5f, 1f)] float _backDashSpeedMultiplier = 0.7f;
        [SerializeField] [Range(0.4f, 0.9f)] float _softAirSpeedMultiplier = 0.7f;
        [SerializeField] [Range(0.4f, 0.9f)] float _softWalkSpeedMultiplier = 0.7f;
        [SerializeField] [Range(0.4f, 0.9f)] float _softRunSpeedMultiplier = 0.7f;
        
        [SerializeField] bool _renderHitboxes;

        [SerializeField] Color _interactionBox;
        [SerializeField] Color _hitboxColor;
        [SerializeField] Color _hurtboxColor;
        [SerializeField] Color _grabboxColor;

        public static FighterManager FM { get; protected set; }

        public float backDashSpeedMultiplier { get { return _backDashSpeedMultiplier; } }
        public float softAirSpeedMultiplier { get { return _softAirSpeedMultiplier; } }
        public float softWalkSpeedMultplier { get { return _softWalkSpeedMultiplier; } }
        public float softRunSpeedMultplier { get { return _softRunSpeedMultiplier; } }

        public bool renderHitboxes  { get { return _renderHitboxes; } }
        public Color interactionBox { get { return _interactionBox; } }
        public Color hitboxColor    { get { return _hitboxColor; } }
        public Color hurtboxColor   { get { return _hurtboxColor; } }
        public Color grabboxColor   { get { return _grabboxColor; } }

        private void Reset()
        {
            transform.position = Vector3.zero;
        }

        public void Awake()
        {
            if (FM == null)
            {
                FM = this;
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(gameObject);
        }

        public void Assault(HitboxData offender, HurtboxData defender)
        {

            float knockbackDealt = 0;

            float defenderDamage = offender.attack.knockbackType == Types.KnockbackType.Set ? 10f : defender.damage + offender.attack.damage;

            knockbackDealt = defenderDamage / 10f;
            Debug.Log(knockbackDealt);
            knockbackDealt += (defenderDamage * offender.attack.damage) / 20f;
            Debug.Log(knockbackDealt);
            knockbackDealt *= 200f / (defender.weight + 100);
            Debug.Log(knockbackDealt);
            knockbackDealt *= 1.4f;
            Debug.Log(knockbackDealt);
            knockbackDealt += 18f;
            Debug.Log(knockbackDealt);
            knockbackDealt *= offender.attack.knockbackGrowth * 0.1f;
            Debug.Log(knockbackDealt);
            knockbackDealt += offender.attack.knockbackBase;

            knockbackDealt *= 0.01f;

            Debug.Log(knockbackDealt);

            Vector2 knockback = new Vector2();
            knockback.x = Mathf.Cos(offender.attack.launchAngle * Mathf.Deg2Rad) * knockbackDealt;
            knockback.y = Mathf.Sin(offender.attack.launchAngle * Mathf.Deg2Rad) * knockbackDealt;

            if (!offender.facingRight)
            {
                if (Mathf.Sign(knockback.x) == 1)
                    knockback.x *= -1f;
            }

            Debug.Log(knockback);

            HitData hitData = new HitData
            {
                damage = offender.attack.damage,
                knockback = knockback
            };

            defender.damageable.TakeHit(hitData);
        }
    }
}

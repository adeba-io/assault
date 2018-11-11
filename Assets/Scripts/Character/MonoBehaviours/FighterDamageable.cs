using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Boxes;
using Assault.Types;

namespace Assault
{
    [RequireComponent(typeof(FighterPhysics))]
    public class FighterDamageable : MonoBehaviour
    {
        public ArmourType currentArmour { get; set; }

        [SerializeField] float _damage;
        [SerializeField] int _weight;

        Hurtbox[] _hurtboxes;

        public float damage
        {
            get { return _damage; }
            set
            {
                _damage = value;
                _hurtboxData.damage = value;
            }
        }

        FighterPhysics _physics;

        HurtboxData _hurtboxData;

        public HurtboxData hurtboxData { get { return _hurtboxData; } }

        private void Reset()
        {
            Transform hurtboxes = transform.Find("--- Hurtboxes ---");
            if (!hurtboxes)
            {
                hurtboxes = new GameObject("--- Hurtboxes ---").transform;
                hurtboxes.SetParent(transform);
            }
            hurtboxes.localPosition = Vector3.zero;
        }

        private void Start()
        {
            _physics = GetComponent<FighterPhysics>();

            _hurtboxes = GetComponentsInChildren<Hurtbox>();

            _hurtboxData = new HurtboxData
            {
                damageable = this,
                damage = _damage,
                weight = _weight,
                armourType = currentArmour
            };
        }

        public void TakeHit(HitData hitData)
        {
            Debug.Log("Damage: " + hitData.damage + "; Knockback: " + hitData.knockback);
            damage += hitData.damage;
            _physics.ForceRigidbody(this, hitData.knockback, true, true);
        }
    }

    public struct HitData
    {
        public float damage;
        public Vector2 knockback;
    }

    [System.Serializable]
    public struct HurtboxData
    {
        public FighterDamageable damageable;

        public float damage;
        public int weight;
        public ArmourType armourType;
    }
}

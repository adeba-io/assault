using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Damager : MonoBehaviour
{
    [Serializable]
    public class DamageableEvent : UnityEvent<Damager, Damageable>
    { }

    [Serializable]
    public class NonDamageableEvent : UnityEvent<Damager>
    { }


}


[Serializable]
public class Technique
{
    [SerializeField] Attack[] _attacks;

    public Attack this[int index]
    {
        get
        {
            try
            {
                return _attacks[index];
            }
            catch
            {
                return null;
            }
        }
    }

    [Serializable]
    public class Attack
    {
        public enum HitstunType
        { Standard, Chain, Burst, NoDI }

        public float damage = 1.0f;

        public float baseKnockback = 10f;
        public float knockbackGrowth = 10f;
        public float launchAngle = 10f;
        public float launchSpeed = 10f;

        public HitstunType hitstunType = HitstunType.Standard;

        public HitBox hitbox;

        public bool useWeight = true;
        public bool knockbackSet = false;
    }
}

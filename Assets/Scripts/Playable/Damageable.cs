using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class Damageable : MonoBehaviour
{
    [Serializable]
    public class HealthEvent : UnityEvent<Damageable>
    { }

    [Serializable]
    public class DamageEvent : UnityEvent<PlayerFighter, Damageable>
    { }

    [Serializable]
    public class HealEvent : UnityEvent<int, Damageable>
    { }

    bool _invincible;
    bool _armourOn;

    Transform _hurtboxes;

    private void Reset()
    {
        _hurtboxes = transform.Find("--- Hurtboxes ---");
        if (!_hurtboxes)
        {
            GameObject hurtboxes = new GameObject("--- Hurtboxes ---");
            _hurtboxes = hurtboxes.transform;
            _hurtboxes.transform.parent = transform;
        }
    }

    public bool ReceiveInput(InputCombo inputCombo)
    {


        return false;
    }
}

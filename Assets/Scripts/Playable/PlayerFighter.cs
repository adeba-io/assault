using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(HitboxPool))]
public class PlayerFighter : MonoBehaviour
{
    [Serializable]
    public class DamageableEvent : UnityEvent<PlayerFighter, Damageable>
    { }

    [Serializable]
    public class NonDamageableEvent : UnityEvent<PlayerFighter>
    { }

    [SerializeField] bool _canDamage;

    PlayerControllerInherit _playerController;
    Damageable _playerDefender;
    PlayerInput Input;
    HitboxPool _hitboxPool;

    public Techniqu _currentTechnique = null;
    Queue<InputCombo> _inputQueue = new Queue<InputCombo>();
    public MoveSet moveset = new MoveSet();

    private void Start()
    {
        _hitboxPool = GetComponent<HitboxPool>();
    }

    public void EnableFighter() { _canDamage = true; }
    public void DisableFighter() { _canDamage = false; }

    public void Attack()
    {

    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerFighter : MonoBehaviour
{
    [Serializable]
    public class DamageableEvent : UnityEvent<PlayerFighter, Damageable>
    { }

    [Serializable]
    public class NonDamageableEvent : UnityEvent<PlayerFighter>
    { }

    [SerializeField] bool _canDamage;

    PlayerController _playerController;
    Damageable _playerDefender;
    PlayerInput Input;

    public Technique _currentTechnique = null;
    Queue<InputCombo> _inputQueue = new Queue<InputCombo>();
    public MoveSet moveset = new MoveSet();
    
    public void EnableFighter() { _canDamage = true; }
    public void DisableFighter() { _canDamage = false; }

    public void Attack()
    {

    }

    public bool ReceiveInput(InputCombo inputCombo)
    {

        return true;
    }
}

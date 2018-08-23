using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Technique", menuName = "Assault/Technqiue")]
public class Techniqu : ScriptableObject
{
    [SerializeField] string _name;

    PlayerControllerInherit _userController;
    [SerializeField] PlayerFighter _userFighter;
    HitboxPool _userHitboxPool;
    Damageable _userDefender;
    
    [SerializeField] Attack[] _attacks;

    public MoveSet _linkTechnqiues = new MoveSet();

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

    public PlayerFighter user
    {
        get { return _userFighter; }
        set
        {
            _userFighter = value;
            _userController = _userFighter.GetComponent<PlayerControllerInherit>();
            _userHitboxPool = _userFighter.GetComponent<HitboxPool>();
            _userDefender = _userFighter.GetComponent<Damageable>();
        }
    }
    
    public void Execute()
    {
        for (int i = 0; i < _attacks.Length; i++)
            _attacks[i].Initialize();
    }
}




using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Technique
{
    [SerializeField] string _name;

    PlayerController _userController;
    PlayerFighter _userFighter;
    Damageable _userDefender;

    [SerializeField] Attack[] _attacks;

    public RekkaLinks _rekkaLinks = new RekkaLinks();

    public Attack this[int index]
    {
        get
        {
            try { return _attacks[index];  }
            catch { return null; }
        }
    }

    public PlayerFighter user
    {
        get { return _userFighter; }
        set
        {
            _userFighter = value;
            _userController = _userFighter.GetComponent<PlayerController>();
            _userDefender = _userFighter.GetComponent<Damageable>();
        }
    }

    public void Execute()
    {
        for (int i = 0; i < _attacks.Length; i++)
            _attacks[i].Initialize();
    }
}

[Serializable]
public class Rekka
{
    [SerializeField] string _name;

    PlayerController _userController;
    PlayerFighter _userFighter;
    Damageable _userDefender;

    [SerializeField] Attack[] _attacks;

    public Attack this[int index]
    {
        get
        {
            try { return _attacks[index]; }
            catch { return null; }
        }
    }

    public PlayerFighter user
    {
        get { return _userFighter; }
        set
        {
            _userFighter = value;
            _userController = _userFighter.GetComponent<PlayerController>();
            _userDefender = _userFighter.GetComponent<Damageable>();
        }
    }

    public void Execute()
    {
        for (int i = 0; i < _attacks.Length; i++)
            _attacks[i].Initialize();
    }
}

[Serializable]
public class Attack
{
    public enum HitstunType
    { Standard, Chain, Burst, NoDI }

    public float damage = 1f;
    public float damageMultiplier = 1f;

    public float baseKnockback = 10f;
    public float knockbackGrowth = 10f;
    public float launchAngle = 10f;
    public float launchSpeed = 10f;

    public HitstunType hitstunType = HitstunType.Standard;

    public HitBox[] hitbox;

    public bool useWeight = true;
    public bool knockbackSet = false;

    public void Initialize()
    {
        for (int i = 0; i < hitbox.Length; i++)
            hitbox[i].OnHitboxEnter += Assault;
    }

    public void Assault(Collider2D collision, Damageable hitboxOwner)
    {

    }
}

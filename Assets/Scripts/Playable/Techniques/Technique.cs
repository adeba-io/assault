using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Assault/Technique")]
public class Technique : ScriptableObject
{
    [SerializeField] string _name;

    bool _canCancel;

    PlayerController _userController;
    PlayerFighter _userFighter;
    Damageable _userDefender;

    [SerializeField] Attack[] _attacks;

    [SerializeField] MoveSet _links = new MoveSet();

    // Animation fields
    int _totalFrameCount;
    int _currentFrame = 0;

    int _cancelStart = 0, _cancelEnd = 0;

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

    public bool canCancel { get { return _canCancel; } }
    public MoveSet links { get { return _links; } }

    public void Initialize()
    {
        for (int i = 0; i < _attacks.Length; i++)
            _attacks[i].Enable();
    }

    public void Update()
    {
        for (int i = 0; i < _attacks.Length; i++)
        {
            if (_currentFrame == _attacks[i].enableFrame)
            {
                _attacks[i].Enable();
            }
            else if (_currentFrame == _attacks[i].disableFrame)
            {
                _attacks[i].Disable();
            }
        }

        _currentFrame++;
    }

    public void End()
    {
        _currentFrame = 0;
    }
}

[Serializable]
public class Attack
{
    public enum HitstunType
    { Standard, Chain, Burst, NoDI }

    public int enableFrame = 0;
    public int disableFrame = 0;

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

    public void Enable()
    {
        for (int i = 0; i < hitbox.Length; i++)
        {
            hitbox[i].OnHitboxEnter += Assault;
            hitbox[i].enabled = true;
        }
    }

    public void Disable()
    {
        for (int i = 0; i < hitbox.Length; i++)
        {
            hitbox[i].OnHitboxEnter -= Assault;
            hitbox[i].enabled = true;
        }
    }

    public void Assault(Collider2D collision, Damageable hitboxOwner)
    {

    }

    #region Animation Methods
    


    #endregion 
}

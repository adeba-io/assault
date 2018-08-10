using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Platform : MonoBehaviour
{
    [Serializable]
    public class CaughtObject
    {
        public Rigidbody2D rigidbody;
        public Collider2D collider;
        public PhysicsObject physicsObject;

        public bool inContact;
        public bool checkedThisFrame;
    }

    public enum PlatformType { Standard, OneWay }

    [SerializeField] PlatformType _platformType = PlatformType.Standard;
    [SerializeField] PhysicsMaterial2D _physicsMaterial;
    [SerializeField] [Range(-90f, 90f)] float _slopeAngle;

    Rigidbody2D _rigidbody2D;
    BoxCollider2D _collider2D;

    public float slopeAngle { get { return _slopeAngle; } }
    public float friction { get { return _physicsMaterial.friction; } }
    public float bounciness { get { return _physicsMaterial.bounciness; } }
    public PlatformType platformType { get { return _platformType; } }

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _collider2D = GetComponent<BoxCollider2D>();

        _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;

        if (!_physicsMaterial)
            _physicsMaterial = _collider2D.sharedMaterial;

        if (_platformType == PlatformType.OneWay)
            transform.localScale = new Vector2(transform.localScale.x, 0.05f);
    }
}

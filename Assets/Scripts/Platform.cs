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

    [SerializeField] PhysicsMaterial2D _physicsMaterial;
    [SerializeField] [Range(-90f, 90f)] float _slopeAngle;

    Rigidbody2D _rigidbody2D;

    public float slopeAngle { get { return _slopeAngle; } }
    public float friction { get { return _physicsMaterial.friction; } }
    public float bounciness { get { return _physicsMaterial.bounciness; } }

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Managers;
using Assault.Types;

namespace Assault
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
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

            public void Move(Vector2 movement)
            {
                if (!inContact) return;

                if (physicsObject)
                    physicsObject.MoveRigidbody(movement);
                else
                    rigidbody.MovePosition(rigidbody.position + movement);
            }
        }

        [SerializeField] PlatformType _platformType = PlatformType.Standard;
        [SerializeField] PhysicsMaterial2D _physicsMaterial;
        [SerializeField] [Range(-90f, 90f)] float _slopeAngle;

        [SerializeField] ContactFilter2D _contactFilter;

        protected Rigidbody2D _rigidbody2D;
        protected BoxCollider2D _collider2D;

        protected List<CaughtObject> _caughtObjects = new List<CaughtObject>();
        protected ContactPoint2D[] _contactPoints = new ContactPoint2D[20];

        public float slopeAngle { get { return _slopeAngle; } }
        public PhysicsMaterial2D physicsMaterial { get { return _physicsMaterial; } set { _physicsMaterial = value; } }
        public float friction { get { return _physicsMaterial.friction; } }
        public float bounciness { get { return _physicsMaterial.bounciness; } }
        public PlatformType platformType { get { return _platformType; } }

        public int caughtObjectCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i< _caughtObjects.Count; i++)
                {
                    if (_caughtObjects[i].inContact) count++;
                }
                return count;
            }
        }

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

        private void FixedUpdate()
        {
            for (int i = 0; i < _caughtObjects.Count; i++)
            {
                CaughtObject caughtObject = _caughtObjects[i];
                caughtObject.inContact = false;
                caughtObject.checkedThisFrame = false;
            }

            CheckRigidbodyContacts(_rigidbody2D);
            
            bool checkAgain;
            do
            {
                for (int i = 0, count = _caughtObjects.Count; i < count; i++)
                {
                    CaughtObject caughtObject = _caughtObjects[i];

                    if (caughtObject.inContact)
                    {
                        if (!caughtObject.checkedThisFrame)
                        {
                            CheckRigidbodyContacts(caughtObject.rigidbody);
                            caughtObject.checkedThisFrame = true;
                        }
                    }
                    else
                    {
                        Collider2D caughtObjectCollider = _caughtObjects[i].collider;

                        // check if we are aligned with the moving platform, but we must check to see if we are within the platform's bounds
                        float caughtObjectCentre = (caughtObjectCollider.bounds.max.x + caughtObjectCollider.bounds.min.x) * 0.5f;
                        bool alignedHorizontally = (caughtObjectCentre > _collider2D.bounds.min.x) && (caughtObjectCentre < _collider2D.bounds.max.x);
                        if (alignedHorizontally)
                        {
                            float yDiff = caughtObjectCollider.bounds.min.y - _collider2D.bounds.max.y;

                            if (yDiff > 0 && yDiff < PhysicsManager.platformContactBuffer)
                            {
                                caughtObject.inContact = true;
                                caughtObject.checkedThisFrame = true;
                            }
                        }
                    }
                }

                checkAgain = false;

                for (int i = 0, count = _caughtObjects.Count; i < count; i++)
                {
                    CaughtObject caughtObject = _caughtObjects[i];
                    if (caughtObject.inContact && !caughtObject.checkedThisFrame)
                    {
                        checkAgain = true;
                        break;
                    }
                }

            } while (checkAgain);
        }

        void CheckRigidbodyContacts(Rigidbody2D rb)
        {
            int contactCount = rb.GetContacts(_contactFilter, _contactPoints);

            for (int i = 0; i < contactCount; i++)
            {
                ContactPoint2D contactPoint = _contactPoints[i];
                Rigidbody2D contactRigidbody = contactPoint.rigidbody == rb ? contactPoint.otherRigidbody : contactPoint.rigidbody;
                int listIndex = -1;

                for (int j = 0; j < _caughtObjects.Count; i++)
                {
                    if (contactRigidbody == _caughtObjects[j].rigidbody)
                    {
                        listIndex = j;
                        break;
                    }
                }

                if (listIndex == -1)
                {
                    if (!contactRigidbody) continue;

                    if (contactRigidbody.bodyType != RigidbodyType2D.Static && contactRigidbody != _rigidbody2D)
                    {
                        float dot = Vector2.Dot(contactPoint.normal, Vector2.down);
                        if (dot > 0.8f)
                        {
                            PhysicsObject physics;
                            Collider2D contactCollider;
                            PhysicsManager.TryGetRigidbodyFighterController(contactRigidbody, out physics);
                            PhysicsManager.TryGetRigidbodyCollider(contactRigidbody, out contactCollider);

                            CaughtObject newCO = new CaughtObject
                            {
                                rigidbody = contactRigidbody,
                                physicsObject = physics,
                                collider = contactCollider,
                                inContact = true,
                                checkedThisFrame = false
                            };

                            _caughtObjects.Add(newCO);
                        }
                    }
                }
                else
                    _caughtObjects[listIndex].inContact = true;
            }
        }
    }
}

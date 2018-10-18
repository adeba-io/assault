using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Types;

namespace Assault
{
    namespace Boxes
    {
        [RequireComponent(typeof(Rigidbody2D))]
        [RequireComponent(typeof(Collider2D))]
        public class InteractionBox : MonoBehaviour
        {
            public Action<Collider2D> OnHitboxEnter;
            public Action<Collider2D> OnHitboxExit;

            [SerializeField] protected LayerMask _hitMask;
            [SerializeField] protected BoxDrawType _drawType;

            [SerializeField] protected int _boxSizeX;
            [SerializeField] protected int _boxSizeY;

            protected GameObject _owner;
            protected Rigidbody2D _rigidbody;
            protected Collider2D _collider;

            public Vector2 _prevPosition;
            public Vector2 _currPosition;

            private void Reset()
            {
                _rigidbody = GetComponent<Rigidbody2D>();
                _collider = GetComponent<Collider2D>();

                _rigidbody.bodyType = RigidbodyType2D.Kinematic;
                _collider.isTrigger = true;
            }

            private void Start()
            {
                _rigidbody = GetComponent<Rigidbody2D>();
                _collider = GetComponent<Collider2D>();

                _rigidbody.bodyType = RigidbodyType2D.Kinematic;

                _collider.isTrigger = true;
                
                if (_collider.GetType() == typeof(CapsuleCollider2D))
                {
                    CapsuleCollider2D col = (CapsuleCollider2D)_collider;
                    col.size = new Vector2(_boxSizeX, _boxSizeY);
                    _collider = col;
                }
                else if (_collider.GetType() == typeof(BoxCollider2D))
                {
                    BoxCollider2D col = (BoxCollider2D)_collider;
                    col.size = new Vector2(_boxSizeX, _boxSizeY);
                    _collider = col;
                }

                _currPosition = _rigidbody.position;
                _prevPosition = _rigidbody.position;
            }

            public void Enable(GameObject owner, Transform parent)
            {
                _owner = owner;
                gameObject.SetActive(true);
                _currPosition = _rigidbody.position;

                transform.parent = parent;
            }

            public void Disable()
            {
                _owner = null;
                gameObject.SetActive(false);
            }

            private void Update()
            {
                if (_drawType == BoxDrawType.STATIC) return;
                
                _prevPosition = _currPosition;
                _currPosition = _rigidbody.position;

                DrawBox();
            }
            
            void DrawBox()
            {
                if (_currPosition == _prevPosition)
                {
                    _collider.offset = Vector2.zero;

                    if (_collider.GetType() == typeof(BoxCollider2D))
                    {
                        BoxCollider2D col = (BoxCollider2D)_collider;
                        col.size = new Vector2(_boxSizeX, _boxSizeY);
                        _collider = col;
                    }
                    else if (_collider.GetType() == typeof(CapsuleCollider2D))
                    {
                        CapsuleCollider2D col = (CapsuleCollider2D)_collider;
                        col.size = new Vector2(_boxSizeX, _boxSizeY);
                        _collider = col;
                    }

                    return;
                }

                Vector2 delta = _currPosition - _prevPosition;

                float distance = Vector2.Distance(_currPosition, _prevPosition);
                float angle = Vector2.Angle(Vector2.up, delta);

                if (_collider.GetType() == typeof(CapsuleCollider2D))
                {
                    _rigidbody.rotation = angle;
                    CapsuleCollider2D col = (CapsuleCollider2D)_collider;
                    col.size = new Vector2(col.size.x, distance);

                    Vector2 offset = Vector2.zero;
                    offset.y -= distance / 2f;
                    offset.y += _boxSizeX / 2f;

                    col.offset = offset;
                    _collider = col;
                }
            }

            private void OnTriggerEnter2D(Collider2D collision)
            {
                if (OnHitboxEnter != null)
                {
                    OnHitboxEnter(collision);
                }
            }

            private void OnTriggerExit2D(Collider2D collision)
            {
                if (OnHitboxExit != null)
                {
                    OnHitboxExit(collision);
                }
            }
        }
    }
}

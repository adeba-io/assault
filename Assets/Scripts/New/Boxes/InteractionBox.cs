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

            [SerializeField] protected InteractionBoxData _boxData;

            protected GameObject _owner;
            protected Rigidbody2D _rigidbody;
            protected Collider2D _collider;

            public Vector2 _prevPosition;
            public Vector2 _currPosition;

            public InteractionBoxData boxData
            { get { return _boxData; } }

            private void Reset()
            {
                _rigidbody = GetComponent<Rigidbody2D>();
                _collider = GetComponent<Collider2D>();

                _rigidbody.bodyType = RigidbodyType2D.Kinematic;
                _collider.isTrigger = true;
            }

            private void Awake()
            {
                _rigidbody = GetComponent<Rigidbody2D>();
                _rigidbody.bodyType = RigidbodyType2D.Kinematic;

                _collider = GetComponent<Collider2D>();
                _collider.isTrigger = true;
            }

            public void Enable(GameObject owner, InteractionBoxData boxData)
            {
                _owner = owner;
                SetInteractionBoxData(boxData);

                transform.localPosition = Vector3.zero;

                gameObject.SetActive(true);
            }

            public void Disable()
            {
                gameObject.SetActive(false);
            }

            private void OnEnable()
            {
                _currPosition = _rigidbody.position;
                _prevPosition = _rigidbody.position;

                if (_collider.GetType() == typeof(CapsuleCollider2D))
                {
                    CapsuleCollider2D col = (CapsuleCollider2D)_collider;
                    col.size = new Vector2(_boxData.boxSizeX, _boxData.boxSizeY);
                    _collider = col;
                }
                else if (_collider.GetType() == typeof(BoxCollider2D))
                {
                    BoxCollider2D col = (BoxCollider2D)_collider;
                    col.size = new Vector2(_boxData.boxSizeX, _boxData.boxSizeY);
                    _collider = col;
                }
            }

            private void OnDisable()
            {
                _owner = null;
            }

            private void Update()
            {
                if (_boxData.drawType == BoxDrawType.STATIC) return;
                
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
                        col.size = new Vector2(_boxData.boxSizeX, _boxData.boxSizeY);
                        _collider = col;
                    }
                    else if (_collider.GetType() == typeof(CapsuleCollider2D))
                    {
                        CapsuleCollider2D col = (CapsuleCollider2D)_collider;
                        col.size = new Vector2(_boxData.boxSizeX, _boxData.boxSizeY);
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
                    offset.y += _boxData.boxSizeX / 2f;

                    col.offset = offset;
                    _collider = col;
                }
            }

            public void SetInteractionBoxData(InteractionBoxData boxData)
            {
                _boxData = boxData;
                transform.parent = _boxData.parent;
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

        [System.Serializable]
        public struct InteractionBoxData
        {
            public int ID;

            public Transform parent;

            public float boxSizeX;
            public float boxSizeY;

            public LayerMask hitMask;
            public BoxDrawType drawType;
        }
    }
}

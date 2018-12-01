using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Types;
using Assault.Managers;

namespace Assault
{
    namespace Boxes
    {
        [RequireComponent(typeof(Rigidbody2D))]
        [RequireComponent(typeof(Collider2D))]
        public class InteractionBox : MonoBehaviour
        {
            OnHitboxEnterDelegate onHitboxEnter;

            [SerializeField] protected bool _renderHitbox;
            [SerializeField] protected Color _boxColor;

            [SerializeField] protected InteractionBoxData _boxData;

            protected GameObject _owner;
            protected Rigidbody2D _rigidbody;
            protected Collider2D _collider;
            protected Type _colliderType;

            [SerializeField] protected Transform[] _renderShapes;

            public Vector2 _prevPosition;
            public Vector2 _currPosition;

            public InteractionBoxData boxData
            { get { return _boxData; } }

            #region MonoBehaviour

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

                _colliderType = _collider.GetType();

                List<Transform> rS = GetComponentsInChildren<Transform>().ToList();

                if (rS.Count < 1)
                {
                    Debug.LogWarning("No render objects in children on " + name + ": " + GetInstanceID());
                    _renderHitbox = false;
                    return;
                }

                for (int i = 0; i < rS.Count; i++)
                {
                    if (rS[i] == transform) rS.RemoveAt(i);
                }

                _renderShapes = rS.ToArray();

                if (FighterManager.FM.renderHitboxes)
                    AssignBoxColor();
                else
                {
                    for (int i = 0; i < _renderShapes.Length; i++) _renderShapes[i].GetComponent<SpriteRenderer>().color = Color.clear;
                }
            }

            private void OnEnable()
            {
                _currPosition = _rigidbody.position;
                _prevPosition = _rigidbody.position;

                if (_colliderType == typeof(CapsuleCollider2D))
                {
                    CapsuleCollider2D col = (CapsuleCollider2D)_collider;
                    col.size = _boxData.boxSize;
                    _collider = col;

                    if (FighterManager.FM.renderHitboxes)
                    {
                        _renderShapes[0].localScale = _boxData.boxSize;
                        _renderShapes[1].localScale = new Vector3(_boxData.boxSize.x, 0);
                        _renderShapes[2].localScale = _boxData.boxSize;

                        _renderShapes[0].localPosition = Vector2.zero;
                        _renderShapes[1].localPosition = Vector2.zero;
                        _renderShapes[2].localPosition = Vector2.zero;
                    }
                }
                else if (_colliderType == typeof(BoxCollider2D))
                {
                    BoxCollider2D col = (BoxCollider2D)_collider;
                    col.size = _boxData.boxSize;
                    _collider = col;

                    if (FighterManager.FM.renderHitboxes)
                    {
                        _renderShapes[0].localScale = _boxData.boxSize;
                        _renderShapes[0].localPosition = Vector2.zero;
                    }
                }

               // DrawBox();
            }

            private void OnDisable()
            {
                _owner = null;

                onHitboxEnter = null;
            }

            private void Update()
            {
                _prevPosition = _currPosition;
                _currPosition = _rigidbody.position;

                DrawBox();
            }

            #endregion

            protected virtual void AssignBoxColor()
            {
                for (int i = 0; i < _renderShapes.Length; i++) _renderShapes[i].GetComponent<SpriteRenderer>().color = FighterManager.FM.hitboxColor;
            }

            public void Enable(GameObject owner, InteractionBoxData boxData)
            {
                _owner = owner;
                SetInteractionBoxData(boxData);

                transform.localPosition = Vector3.zero;

                gameObject.SetActive(true);
            }

            public void Enable(GameObject owner, InteractionBoxData boxData, OnHitboxEnterDelegate hitboxEnter)
            {
                Enable(owner, boxData);
                onHitboxEnter += hitboxEnter;
            }

            public void Disable()
            {
                gameObject.SetActive(false);
            }

            public void DrawBox()
            {
                if (!FighterManager.FM.renderHitboxes) return;

                if (_colliderType == typeof(BoxCollider2D))
                {
                    BoxCollider2D col = (BoxCollider2D)_collider;

                    col.size = _boxData.boxSize;
                    col.offset = _boxData.offset;

                    _renderShapes[0].localPosition = _boxData.offset;
                    _renderShapes[0].localScale = _boxData.boxSize;

                    _collider = col;
                }
                else if (_colliderType == typeof(CapsuleCollider2D))
                {
                    CapsuleCollider2D col = (CapsuleCollider2D)_collider;

                    if (_boxData.drawType == BoxDrawType.STATIC)
                    {
                        col.size = _boxData.boxSize;
                        col.offset = _boxData.offset;
                        _collider = col;

                        _rigidbody.rotation = _boxData.rotation;

                        _renderShapes[0].localPosition = _boxData.offset;
                        _renderShapes[1].localPosition = new Vector3(0, (_boxData.boxSizeY - 1f) / 2f) + (Vector3)_boxData.offset;
                        _renderShapes[2].localPosition = new Vector3(0, _boxData.boxSizeY) + (Vector3)_boxData.offset;

                        _renderShapes[1].localScale = new Vector3(_boxData.boxSize.x, boxData.boxSize.y - 1f);

                        return;
                    }

                    if (_currPosition == _prevPosition)
                    {
                        _collider.offset = Vector2.zero;

                        if (_colliderType == typeof(CapsuleCollider2D))
                        {
                            col.size = new Vector2(_boxData.boxSizeX, _boxData.boxSizeY);
                            _collider = col;

                            _renderShapes[0].localPosition = Vector2.zero;
                            _renderShapes[1].localPosition = Vector2.zero;
                            _renderShapes[2].localPosition = Vector2.zero;

                            _renderShapes[1].localScale = new Vector3(_boxData.boxSize.x, 0);
                        }

                        return;
                    }

                    Vector2 delta = _currPosition - _prevPosition;

                    float distance = Vector2.Distance(_currPosition, _prevPosition);
                    bool right = _currPosition.x > _prevPosition.x;

                    float angle = Vector2.Angle(Vector2.up, delta);

                    _rigidbody.rotation = angle;

                    col.size = new Vector2(col.size.x, distance);

                    Vector2 offset = Vector2.zero;
                    offset.y -= (distance / 2f) * (right ? -1f : 1f);
                    offset.y += (_boxData.boxSize.x / 2f) * (right ? -1f : 1f);
                    col.offset = offset;

                    _collider = col;

                    if (_renderHitbox)
                    {
                        float yPos = (right ? distance - 1f : -(distance - 1f));
                        yPos /= 2f;
                        _renderShapes[1].localPosition = new Vector3(0, yPos);
                        _renderShapes[1].localScale = new Vector3(_boxData.boxSize.x, distance - 1f);

                        _renderShapes[2].localPosition = new Vector3(0, right ? distance - 1f : -(distance - 1f));
                    }
                }
            }

            public void SetInteractionBoxData(InteractionBoxData boxData)
            {
                _boxData = boxData;
                transform.parent = _boxData.parent;
            }

            private void OnTriggerEnter2D(Collider2D collision)
            {
                if (onHitboxEnter != null)
                {
                    onHitboxEnter(collision);
                }
            }

            public delegate void OnHitboxEnterDelegate(Collider2D collision);
        }

        [System.Serializable]
        public struct InteractionBoxData
        {
            public int ID;

            public Transform parent;

            public Vector2 boxSize;
            public Vector2 offset;

            public float boxSizeX;
            public float boxSizeY;
            public float offsetX;
            public float offsetY;
            public float rotation;

            public LayerMask hitMask;
            public BoxDrawType drawType;
        }
    }
}

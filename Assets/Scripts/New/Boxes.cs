using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assault
{
    namespace Boxes
    {
        public enum BoxDrawType
        { STATIC, Continuous }

        [RequireComponent(typeof(Rigidbody2D))]
        [RequireComponent(typeof(BoxCollider2D))]
        public class InteractionBox : MonoBehaviour
        {
            public Action<Collider2D> OnHitboxEnter;
            public Action<Collider2D> OnHitboxExit;

            public LayerMask hitMask;
            public BoxDrawType _drawType;

            protected Rigidbody2D _rigidbody;
            protected Collider2D _collider;

            Vector2 _prevPosition;
            Vector2 _currPosition;

            private void Reset()
            {
                _rigidbody = GetComponent<Rigidbody2D>();
                _collider = GetComponent<Collider2D>();

                _rigidbody.bodyType = RigidbodyType2D.Kinematic;
                _collider.isTrigger = true;
            }

            private void Start()
            {
                _rigidbody.bodyType = RigidbodyType2D.Kinematic;
                _collider.isTrigger = true;

                _currPosition = _rigidbody.position;
                _prevPosition = _rigidbody.position;
            }

            public void Enable()
            {
                gameObject.SetActive(true);
                _currPosition = _rigidbody.position;
            }

            public void Disable()
            {
                gameObject.SetActive(false);
            }

            private void Update()
            {
                _prevPosition = _currPosition;
                _currPosition = _rigidbody.position;

                if (_drawType == BoxDrawType.STATIC) return;

                float angle = Vector2.SignedAngle(_currPosition, _prevPosition);
                _rigidbody.rotation = angle;

                float hypotheneuse = Vector2.Distance(_currPosition, _prevPosition);

                _currPosition = (_currPosition + _prevPosition) / 2f;
                transform.localScale = new Vector3(transform.localScale.x, hypotheneuse);
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

        [RequireComponent(typeof(CapsuleCollider2D))]
        public class HitBox : InteractionBox
        {

        }
    }
}

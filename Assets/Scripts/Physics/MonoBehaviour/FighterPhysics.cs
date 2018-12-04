using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Types;
using Assault.Managers;

namespace Assault
{
    // DO NOT USE:
    // Awake, FixedUpdate, OnTriggerEnter2D, OnTriggerStay2D, OnTriggerExit2D
    public class FighterPhysics : PhysicsObject
    {
        public Action OnGroundedEvent;
        public Action OnAerialEvent;
        public Action OnHitCeilingEvent;

        public Action OnHitLeftWallEvent;
        public Action OnHitRightWallEvent;

        bool callOnGrounded = false;
        bool callOnAerial = false;
        bool callOnHitCeiling = false;
        bool callOnHitLeftWall = false;
        bool callOnHitRightWall = false;

        [SerializeField] float _traction = 2f;

        [SerializeField] float _fallSpeed = 5f;
        [SerializeField] [Range(1.2f, 3f)] float _fastFallSpeedMultiplier = 1.2f;

        [SerializeField] int _recallFrameInterval = 30;
        int _lastIntervalCall;

        float m_fallSpeed, m_fastFallSpeed;

        bool _fastFallPrevious = false;
        public bool fastFall { get; set; }
        public bool useTraction { get; set; }
        
        public Vector2 internalVelocity { get { return _internalVelocity; } }

        private void Start()
        {
            fastFall = false;
            useTraction = true;

            m_fallSpeed = _fallSpeed * 5f;
            m_fastFallSpeed = m_fallSpeed * _fastFallSpeedMultiplier;

            _lastIntervalCall = Time.frameCount;
        }

        private void Update()
        {
            if (callOnGrounded && OnGroundedEvent != null) { OnGroundedEvent(); callOnGrounded = false; }
            if (callOnAerial && OnAerialEvent != null) { OnAerialEvent(); callOnAerial = false; }
            if (callOnHitCeiling && OnHitCeilingEvent != null) { OnHitCeilingEvent(); callOnHitCeiling = false; }
            if (callOnHitLeftWall && OnHitLeftWallEvent != null) { OnHitLeftWallEvent(); callOnHitLeftWall = false; }
            if (callOnHitRightWall && OnHitRightWallEvent != null) { OnHitRightWallEvent(); callOnHitRightWall = false; }

            if (Time.frameCount - _lastIntervalCall >= _recallFrameInterval)
            {
                if (OnGroundedEvent != null)
                {
                    if (collisionState.groundedThisFrame)
                        OnGroundedEvent();
                }
                if (OnAerialEvent != null && !_collisionState.below)
                {
                    if (!collisionState.groundedThisFrame)
                        OnAerialEvent();
                }

                _lastIntervalCall = Time.frameCount;
            }
        }

        protected override void FurtherFixedUpdate()
        {
            _fastFallPrevious = fastFall;

            Fall();
            Traction();
            Decelerate();

            if (!callOnGrounded) callOnGrounded = !collisionState.groundedLastFrame && collisionState.groundedThisFrame;
            callOnAerial        = collisionState.groundedLastFrame && !collisionState.groundedThisFrame;
            callOnHitCeiling    = !collisionState.hitCeilingLastFrame && collisionState.hitCeilingThisFrame;
            callOnHitLeftWall   = !collisionState.hitLeftWallLastFrame && collisionState.hitLeftWallThisFrame;
            callOnHitRightWall  = !collisionState.hitRightWallLastFrame && collisionState.hitRightWallThisFrame;
            
        }

        void Fall()
        {
            if (isGrounded) return;

            float currFallSpeed = -_internalVelocity.y;
            float fallSpeed = (fastFall ? m_fastFallSpeed : m_fallSpeed);

            if (_fastFallPrevious == false && fastFall == true)
                ForceRigidbody(this, 0, -m_fastFallSpeed);

            else if (currFallSpeed > fallSpeed)
            {
                ForceRigidbody(this, 0, -fallSpeed, resetY: true);
            }
        }

        void Traction()
        {
            if (!_collisionState.below) return;
            if (!_collisionState.belowPlatform) return;

            if (_internalVelocity.x > 0)
                _internalVelocity.x -= _traction * Time.deltaTime * 10f;
            else if (_internalVelocity.x < 0)
                _internalVelocity.x += _traction * Time.deltaTime * 10f;
        }
        
        public void Flip(bool flipX, bool flipY)
        {
            if (flipX) _internalVelocity.x = -_internalVelocity.x;
            if (flipY) _internalVelocity.y = -_internalVelocity.y;
        }

        #region Decelerate to Max

        bool _decelerateToMax = false;
        Vector2 _deceleration;
        Vector2 _maxVelocity;

        void Decelerate()
        {
            if (!_decelerateToMax) return;

            if (Mathf.Abs(_internalVelocity.x) <= _maxVelocity.x)
            {
                _deceleration.x = 0;
            }
            if (Mathf.Abs(_internalVelocity.y) <= _maxVelocity.y)
            {
                _deceleration.y = 0;
            }

            if (_deceleration == Vector2.zero)
            {
                _decelerateToMax = false;
                _maxVelocity = Vector2.zero;
                print("We're done here");
                return;
            }

            AccelerateRigidbodyInternal(_deceleration);
        }

        public void DecelerateToMax(Component affector, Vector2 maxVelocity, Vector2 deceleration)
        {
            if (maxVelocity == Vector2.zero)
            {
                ResetRigidbodyInternal();
                return;
            }
            if (_deceleration == Vector2.zero) return;

            deceleration.x = Mathf.Abs(deceleration.x);
            deceleration.y = Mathf.Abs(deceleration.y);

            if (! CheckIfConnected(affector)) return;

            if (maxVelocity.x != 0)
            {
                if (_deceleration.x == Mathf.Infinity)
                {
                    ForceRigidbodyInternal(new Vector2(maxVelocity.x, 0), true);
                    deceleration.x = 0;
                }
                else
                {
                    deceleration.x *= -Mathf.Sign(_maxVelocity.x);
                }
            }
            else ResetRigidbodyInternal(true, false);

            if (maxVelocity.y != 0)
            {
                if (_deceleration.y == Mathf.Infinity)
                {
                    ForceRigidbodyInternal(new Vector2(0, maxVelocity.y), resetY: true);
                    deceleration.y = 0;
                }
                else
                {
                    deceleration.y *= -Mathf.Sign(_maxVelocity.y);
                }
            }
            else ResetRigidbodyInternal(false, true);

            if (_internalVelocity == Vector2.zero) return;
            if (deceleration == Vector2.zero) return;

            _decelerateToMax = true;
            _maxVelocity = new Vector2(Mathf.Abs(maxVelocity.x), Mathf.Abs(maxVelocity.y));
            _deceleration = deceleration;
        }

        public void DecelerateToMax(Component affector, float maxVelocityX = 0, float maxVelocityY = 0, 
            float decelerationX = 0, float decelerationY = 0)
        { DecelerateToMax(affector, new Vector2(maxVelocityX, maxVelocityY), new Vector2(decelerationX, decelerationY)); }

        #endregion

        #region Ignore Collider

        Platform _platformToIgnore = null;
        bool _ignoreBelow = false;

        public void IgnoreBelowPlatformForFrame(Component affector)
        {
            if (!CheckIfConnected(affector)) return;

            _ignoreBelow = true;
        }

        public void IgnoreBelowPlatform(Component affector)
        {
            if (!CheckIfConnected(affector)) return;

            if (!_collisionState.belowPlatform)
                return;

            if (_collisionState.belowPlatform.platformType == PlatformType.Phaseable)
                _platformToIgnore = _collisionState.belowPlatform;
        }

        protected override void CheckVerticalCollisions(bool above, ref float distanceToHit, ref Vector2 hitNormal)
        {
            if (!_platformToIgnore || above)
            {
                base.CheckVerticalCollisions(above, ref distanceToHit, ref hitNormal);
                return;
            }
            else if (_ignoreBelow)
            {
                _ignoreBelow = false;
                return;
            }

            // Declare require variables
            Vector2 raycastStart = _raycastPoints.bottom;
            raycastStart.x += deltaMovement.x;
            Vector2 raycastDirection = Vector2.down;

            float raycastDistance = -deltaMovement.y + skinWidth;
            if (raycastDistance < skinWidth) raycastDistance = skinWidth;

            // Raycast
            DrawRay(raycastStart, raycastDirection * raycastDistance, vertRayColor);
            RaycastHit2D raycastHit = Physics2D.Raycast(raycastStart, raycastDirection, raycastDistance, _collisionMask);

            if (raycastHit)
            {
                // Don't register collisions if we're moving away from the collider
                if (deltaMovement.y > 0) return;
                // We calc against the reverse of raycastDirection
                float currAngle = Vector2.Angle(-raycastDirection, raycastHit.normal);
                // If the angle is greater that the appropriateSlopeLimit, bail
                if (currAngle > _groundSlopeLimit)
                {
                    AdjustForVerticalSlope(above, raycastHit);
                    return;
                }

                Platform potentialPlatform;
                if (PhysicsManager.TryGetColliderPlatform(raycastHit.collider, out potentialPlatform))
                {
                    if (potentialPlatform == _platformToIgnore)
                        return;
                    else
                        _platformToIgnore = null;

                    _collisionState.below = true;
                    _collisionState.belowPlatform = potentialPlatform;

                    if (currAngle > 0) _onSlope = true;

                }

                distanceToHit = raycastHit.distance;
                hitNormal = raycastHit.normal;
            }
            else
                _platformToIgnore = null;
        }

        #endregion
    }
}

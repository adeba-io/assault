﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        [SerializeField] float _fastFallSpeed = 7f;

        bool _fastFallPrevious = false;
        public bool fastFall { get; set; }
        public bool useTraction { get; set; }
        
        public Vector2 internalVelocity { get { return _internalVelocity; } }

        private void Start()
        {
            fastFall = false;
            useTraction = true;
        }

        private void Update()
        {
            if (callOnGrounded && OnGroundedEvent != null) { OnGroundedEvent(); callOnGrounded = false; }
            if (callOnAerial && OnAerialEvent != null) { OnAerialEvent(); callOnAerial = false; }
            if (callOnHitCeiling && OnHitCeilingEvent != null) { OnHitCeilingEvent(); callOnHitCeiling = false; }
            if (callOnHitLeftWall && OnHitLeftWallEvent != null) { OnHitLeftWallEvent(); callOnHitLeftWall = false; }
            if (callOnHitRightWall && OnHitRightWallEvent != null) { OnHitRightWallEvent(); callOnHitRightWall = false; }
        }

        protected override void FurtherFixedUpdate()
        {
            _fastFallPrevious = fastFall;

            Fall();

            callOnGrounded      = !collisionState.groundedLastFrame && collisionState.groundedThisFrame;
            callOnAerial        = collisionState.groundedLastFrame && !collisionState.groundedThisFrame;
            callOnHitCeiling    = !collisionState.hitCeilingLastFrame && collisionState.hitCeilingThisFrame;
            callOnHitLeftWall   = !collisionState.hitLeftWallLastFrame && collisionState.hitLeftWallThisFrame;
            callOnHitRightWall  = !collisionState.hitRightWallLastFrame && collisionState.hitRightWallThisFrame;
            
        }

        void Fall()
        {
            if (isGrounded) return;

            float currFallSpeed = -_internalVelocity.y;
            float fallSpeed = (fastFall ? _fastFallSpeed : _fallSpeed);

            if (_fastFallPrevious == false && fastFall == true)
                ForceRigidbody(this, 0, -_fastFallSpeed);

            else if (currFallSpeed > fallSpeed)
            {
                ForceRigidbody(this, 0, -fallSpeed, resetY: true);
            }
        }

        void Traction()
        {

        }
        
        public void Flip(bool flipX, bool flipY)
        {
            if (flipX) _internalVelocity.x = -_internalVelocity.x;
            if (flipY) _internalVelocity.y = -_internalVelocity.y;
        }
    }
}

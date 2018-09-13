using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assault
{
    // DO NOT USE:
    // Awake, FixedUpdate, OnTriggerEnter2D, OnTriggerStay2D, OnTriggerExit2D
    public class FighterPhysics : PhysicsObject
    {
        [SerializeField] float _fallSpeed = 5f;
        [SerializeField] float _fastFallSpeed = 7f;

        public bool fastFall { get; set; }

        private void Start()
        {
            fastFall = false;
        }

        private void Update()
        {
            Fall();
        }

        void Fall()
        {
            float currFallSpeed = -_internalVelocity.y;
            float fallSpeed = (fastFall ? _fastFallSpeed : _fallSpeed);

            if (currFallSpeed > fallSpeed)
            {
                ForceRigidbody(this, 0, -fallSpeed, resetY: true);
            }
        }

        public void Flip(bool flipX, bool flipY)
        {
            if (flipX) _internalVelocity.x = -_internalVelocity.x;
            if (flipY) _internalVelocity.y = -_internalVelocity.y;
        }
    }
}

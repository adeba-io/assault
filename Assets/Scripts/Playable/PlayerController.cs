using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Types;

namespace Assault
{
    // DO NOT USE:
    // Awake, FixedUpdate, OnTriggerEnter2D, OnTriggerStay2D, OnTriggerExit2D
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : PhysicsObject
    {
        public FighterState _currentState;

        [SerializeField] float _walkAcceleration = 3f;
        [SerializeField] float _maxWalkSpeed = 3f;

        [SerializeField] float _runAcceleration = 4f;
        [SerializeField] float _maxRunSpeed = 7f;

        [SerializeField] float _groundDashSpeed = 10f;

        [SerializeField] float _airAcceleration = 6f;
        [SerializeField] float _airDeceleration = 4f;
        [SerializeField] float _maxAirSpeed = 5f;

        [SerializeField]
        float _groundJumpForce = 7f;
        [SerializeField] float _airJumpForce = 4.5f;
        [SerializeField] int _maxAirJumps = 2;
        [SerializeField] Vector2 _wallJumpForce = new Vector2(2f, 6f);
        int _airJumpsLeft;

        [SerializeField]
        float _dashSpeed = 6f;
        [Space]

        [SerializeField]
        float _fallSpeed = 5f;
        [SerializeField] float _fastFallSpeed = 7f;

        PlayerInput Input;

        InputCombo _currInputCombo;

        public bool facingRight { get; protected set; }

        private void Start()
        {
            Input = GetComponent<PlayerInput>();

            _wallJumpForce.x = Mathf.Abs(_wallJumpForce.x);
            _wallJumpForce.y = Mathf.Abs(_wallJumpForce.y);
            _airJumpsLeft = _maxAirJumps;
        }

        private void Update()
        {
            Fall();

            if (Input.Control.X.Snap)
                print("Snapped");
        }

        // Use for 
        public bool ReceiveInput(InputCombo inputCombo)
        {
            bool toReturn = false;
            _currInputCombo = inputCombo;

            toReturn = toReturn || Jump();

            return toReturn;
        }

        void Move()
        {
            if (isGrounded)
            {
                if (Input.Control.X.Snap)
                {
                    if (_currentState == FighterState.Standing || _currentState == FighterState.Crouching || _currentState == FighterState.Dashing)
                    {
                        ForceRigidbody(this, _groundDashSpeed, 0, true);
                        _currentState = FighterState.Dashing;
                    }
                }
                else if (Input.Control.X.Hard)
                {
                    Vector2 addedVelocity = AccelerateRigidbody(this, Input.Control.X.Value * _runAcceleration, 0, false);

                    float newVelocityX = _internalVelocity.x + addedVelocity.x;
                    if (Mathf.Abs(newVelocityX) <= _maxRunSpeed)
                        AccelerateRigidbody(this, Input.Control.X.Value * _runAcceleration, 0);
                }
                else if (Input.Control.X.Soft)
                {
                    Vector2 addedVelocity = AccelerateRigidbody(this, Input.Control.X.Value * _walkAcceleration, 0, false);

                    float newVelocityX = _internalVelocity.x + addedVelocity.x;
                    if (Mathf.Abs(newVelocityX) <= _maxWalkSpeed)
                        AccelerateRigidbody(this, Input.Control.X.Value * _walkAcceleration, 0);
                }

                if (_internalVelocity.x > 0) facingRight = true;
                else if (_internalVelocity.x < 0) facingRight = false;
            }
            else
            {
                float moveX = Input.Control.X.Value * _airAcceleration;
                Vector2 addedVelocity = AccelerateRigidbody(this, moveX, 0, false);

                float newVelocityX = _internalVelocity.x + addedVelocity.x;
                if (Mathf.Abs(newVelocityX) <= _maxAirSpeed)
                    AccelerateRigidbody(this, moveX, 0);
                else
                {
                    moveX = -Input.Control.X.Value * _airDeceleration;
                    AccelerateRigidbody(this, moveX, 0);
                }
            }
        }

        bool Jump()
        {
            if (_collisionState.groundedLastFrame)
                _airJumpsLeft = _maxAirJumps;

            if (_currInputCombo.button == Button.Jump && _currInputCombo.buttonManeuver == ButtonManeuver.Down)
            {
                if (isGrounded)
                {
                    ForceRigidbody(this, 0, _groundJumpForce, false, true);
                }
                else if (_airJumpsLeft > 0)
                {
                    if (WallJump())
                        return true;

                    ForceRigidbody(this, 0, _airJumpForce, false, true);
                    _airJumpsLeft--;
                }

                return true;
            }

            return false;
        }

        bool WallJump()
        {
            if (!_collisionState.touchingWall) return false;

            if (_collisionState.rightPlatform && Input.Control.X.Value < 0)
            {
                ForceRigidbody(this, -_wallJumpForce.x, _wallJumpForce.y, true, true);
                return true;
            }
            else if (_collisionState.leftPlatform && Input.Control.X.Value > 0)
            {
                ForceRigidbody(this, _wallJumpForce, true, true);
                return true;
            }

            return false;
        }

        void Fall()
        {
            float currFallSpeed = -_internalVelocity.y;
            float fallSpeed = (Input.Control.X.Value < 0 ? _fastFallSpeed : _fallSpeed);

            if (currFallSpeed > fallSpeed)
            {
                ForceRigidbody(this, 0, -fallSpeed, false, true);
            }
        }

        void Dash()
        {
            if (!Input.Meter.Down) return;
            if (Input.Control.X.Value == 0 && Input.Control.Y.Value == 0) return;

            Vector2 dash = new Vector2(Input.Control.X.Value, Input.Control.Y.Value);
            dash *= _dashSpeed;

            ForceRigidbody(this, dash, true, true);
        }

        public void UpdateState(FighterState newState)
        {
            if (isGrounded)
            {

            }
            else
            {

            }
        }
    }
}

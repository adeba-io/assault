using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DO NOT USE:
// Awake, FixedUpdate, OnTriggerEnter2D, OnTriggerStay2D, OnTriggerExit2D
[RequireComponent(typeof(PlayerInput))]
public class PlayerControllerInherit : PhysicsObject
{
    [SerializeField] float _runAcceleration = 4f;
    [SerializeField] float _maxRunSpeed = 7f;
    [Space]

    [SerializeField]
    float _airAcceleration = 6f;
    [SerializeField] float _maxAirSpeed = 5f;
    [Space]

    [SerializeField]
    float _groundJumpForce = 7f;
    [SerializeField] float _airJumpForce = 4.5f;
    [SerializeField] int _maxAirJumps = 2;
    [SerializeField] Vector2 _wallJumpForce = new Vector2(2f, 6f);
    int _airJumpsLeft;
    [Space]

    [SerializeField]
    float _dashSpeed = 6f;
    [Space]

    [SerializeField]
    float _fallSpeed = 5f;
    [SerializeField] float _fastFallSpeed = 7f;

    PlayerInput Input;

    private void Start()
    {
        Input = GetComponent<PlayerInput>();

        _wallJumpForce.x = Mathf.Abs(_wallJumpForce.x);
        _wallJumpForce.y = Mathf.Abs(_wallJumpForce.y);
        _airJumpsLeft = _maxAirJumps;
    }

    private void Update()
    {
        Move();
        Jump();
        Dash();
        Fall();
    }

    void Move()
    {
        float moveX = Input.Control_X.Value;

        if (isGrounded)
        {
            moveX *= _runAcceleration;
            Vector2 addedVelocity = MoveRigidbody(moveX, 0, false);

            float newVelocityX = _projectedVelocity.x + addedVelocity.x;
            if (newVelocityX <= _maxRunSpeed && newVelocityX >= -_maxRunSpeed)
                MoveRigidbody(moveX, 0);
        }
        else
        {
            moveX *= _airAcceleration;
            Vector2 addedVelocity = MoveRigidbody(moveX, 0, false);

            float newVelocityX = _projectedVelocity.x + addedVelocity.x;
            if (newVelocityX <= _maxAirSpeed && newVelocityX >= -_maxAirSpeed)
                MoveRigidbody(moveX, 0);
        }
    }

    void Jump()
    {
        if (_collisionState.groundedLastFrame)
            _airJumpsLeft = _maxAirJumps;

        if (WallJump())
            return;

        if (Input.Jump.Down)
        {
            if (isGrounded)
            {
                ForceRigidbody(0, _groundJumpForce, false, true);
            }
            else
            {
                ForceRigidbody(0, _airJumpForce, false, true);
                _airJumpsLeft--;
            }
        }
    }

    bool WallJump()
    {
        if (!Input.Jump.Down) return false;
        if (!_collisionState.touchingWall) return false;

        if (_collisionState.rightPlatform && Input.Control_X.Value > 0)
            ForceRigidbody(-_wallJumpForce.x, _wallJumpForce.y, true, true);

        else if (_collisionState.leftPlatform && Input.Control_X.Value < 0)
            ForceRigidbody(_wallJumpForce, true, true);

        return true;
    }

    void Fall()
    {
        float currFallSpeed = -_projectedVelocity.y;

        if (currFallSpeed > _fallSpeed)
        {
            ForceRigidbody(0, -_fallSpeed, false, true);
        }
    }

    void Dash()
    {
        if (!Input.Meter.Down) return;
        if (Input.Control_X.Value == 0 && Input.Control_Y.Value == 0) return;

        Vector2 dash = new Vector2(Input.Control_X.Value, Input.Control_Y.Value);
        dash *= _dashSpeed;

        ForceRigidbody(dash, true, true);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DO NOT USE:
// Awake, FixedUpdate, OnTriggerEnter2D, OnTriggerStay2D, OnTriggerExit2D
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : PhysicsObject
{
    public enum CurrentState
    { Standing, Crouching, Dashing, Running, Aerial, Fallen, Hitstun }

    public CurrentState _currentState;

    [SerializeField] float _walkAcceleration = 3f;
    [SerializeField] float _maxWalkSpeed = 3f;

    [SerializeField] float _runAcceleration = 4f;
    [SerializeField] float _maxRunSpeed = 7f;

    [SerializeField] float _groundDashSpeed = 10f;

    [SerializeField]
    float _airAcceleration = 6f;
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
        Move();
        Dash();
        Fall();

        if (Input.Control_X.Snap)
            print("Snapped");
    }

    public bool ReceiveInput(InputCombo inputCombo)
    {
        bool toReturn = false;

        _currInputCombo = inputCombo;

        toReturn |= Jump();
        
        return toReturn;
    }

    void Move()
    {
        if (isGrounded)
        {
            if (Input.Control.X.Snap)
            {
                if (_currentState == CurrentState.Standing || _currentState == CurrentState.Crouching || _currentState == CurrentState.Dashing)
                {
                    ForceRigidbody(_groundDashSpeed, 0, true);
                    _currentState = CurrentState.Dashing;
                }
            }
            else if (Input.Control.X.Hard)
            {
                Vector2 addedVelocity = MoveRigidbody(Input.Control.X.Value * _runAcceleration, 0, false);

                float newVelocityX = _projectedVelocity.x + addedVelocity.x;
                if (Mathf.Abs(newVelocityX) <= _maxRunSpeed)
                    MoveRigidbody(Input.Control.X.Value * _runAcceleration, 0);
            }
            else if (Input.Control.X.Soft)
            {
                Vector2 addedVelocity = MoveRigidbody(Input.Control.X.Value * _walkAcceleration, 0, false);

                float newVelocityX = _projectedVelocity.x + addedVelocity.x;
                if (Mathf.Abs(newVelocityX) <= _maxWalkSpeed)
                    MoveRigidbody(Input.Control.X.Value * _walkAcceleration, 0);
            }

            if (_projectedVelocity.x > 0) facingRight = true;
            else if (_projectedVelocity.x < 0) facingRight = false;
        }
        else
        {
            float moveX = Input.Control.X.Value * _airAcceleration;
            Vector2 addedVelocity = MoveRigidbody(moveX, 0, false);

            float newVelocityX = _projectedVelocity.x + addedVelocity.x;
            if (Mathf.Abs(newVelocityX) <= _maxAirSpeed)
                MoveRigidbody(moveX, 0);
        }
    }

    bool Jump()
    {
        if (_collisionState.groundedLastFrame)
            _airJumpsLeft = _maxAirJumps;

        if (WallJump())
            return true;

        if (_currInputCombo.inputButton == PlayerInput.Button.Jump)
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

        return true;
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

    public void UpdateState(CurrentState newState)
    {
        if (isGrounded)
        {
            if (Input.Control_Y.Value < -0.5f)
            {
                _currentState = CurrentState.Crouching;
            }
        }
        else
        {

        }
    }
}

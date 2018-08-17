using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhysicsObjectMK2))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerControllerMK2 : MonoBehaviour
{
    [SerializeField] float _runAcceleration = 4f;
    [SerializeField] float _maxRunSpeed = 7f;
    [Space]

    [SerializeField] float _airAcceleration = 6f;
    [SerializeField] float _maxAirSpeed = 5f;
    [Space]

    [SerializeField] float _groundJumpForce = 7f;
    [SerializeField] float _airJumpForce = 4.5f;
    [SerializeField] int _maxAirJumps = 2;
    int _airJumpsLeft;
    [Space]

    [SerializeField] float _fallSpeed = 5f;
    [SerializeField] float _fastFallSpeed = 7f;

    PhysicsObjectMK2 _physicsObject;
    PlayerInput Input;

    private void Start()
    {
        _physicsObject = GetComponent<PhysicsObjectMK2>();
        Input = GetComponent<PlayerInput>();

        _airJumpsLeft = _maxAirJumps;
    }

    private void Update()
    {
        Move();
        Jump();
        Fall();
    }

    void Move()
    {
        float moveX = Input.ControlHorizontal.Value;

        if (_physicsObject.isGrounded)
        {
            moveX *= _runAcceleration;
            Vector2 addedVelocity = _physicsObject.MoveRigidbody(moveX, 0, false);

            float newVelocityX = _physicsObject.projectedVelocity.x + addedVelocity.x;
            if (newVelocityX <= _maxRunSpeed && newVelocityX >= -_maxRunSpeed)
                _physicsObject.MoveRigidbody(moveX, 0);
        }
        else
        {
            moveX *= _airAcceleration;
            Vector2 addedVelocity = _physicsObject.MoveRigidbody(moveX, 0, false);

            float newVelocityX = _physicsObject.projectedVelocity.x + addedVelocity.x;
            if (newVelocityX <= _maxAirSpeed && newVelocityX >= -_maxAirSpeed)
                _physicsObject.MoveRigidbody(moveX, 0);
        }
    }

    void Jump()
    {
        if (_physicsObject.collisionState.hitGroundLastFrame)
            _airJumpsLeft = _maxAirJumps;

        if (Input.Jump.Down)
        {
            if (_physicsObject.isGrounded)
            {
                _physicsObject.ForceRigidbody(0, _groundJumpForce, false, true);
            }
            else if (!_physicsObject.isGrounded && _airJumpsLeft > 0)
            {
                _physicsObject.ForceRigidbody(0, _airJumpForce, false, true);
                _airJumpsLeft--;
            }
        }
    }

    void Fall()
    {
        float currFallSpeed = -_physicsObject.projectedVelocity.y;

        if (currFallSpeed > _fallSpeed)
        {
            float speedDiff = currFallSpeed - _fallSpeed;
            _physicsObject.ForceRigidbody(0, -_fallSpeed, false, true);
        }
    }
}

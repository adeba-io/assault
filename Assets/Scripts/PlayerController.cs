using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhysicsObject))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float _runAcceleration = 4f, _maxRunSpeed = 7f;

    [SerializeField] float _airAcceleration = 5f, _maxAirSpeed = 6f;

    [SerializeField] float _jumpSpeed = 7f;
    [SerializeField] int _maxAirJumps = 2;
    int _airJumpsLeft;

    PhysicsObject _physicsObject;
    PlayerInput _playerInput;

    private void Start()
    {
        _physicsObject = GetComponent<PhysicsObject>();
        _playerInput = GetComponent<PlayerInput>();

        _airJumpsLeft = _maxAirJumps;
    }

    private void Update()
    {
        float moveX = _playerInput.ControlHorizontal.Value;
        
        if (_physicsObject.isGrounded)
        {
            moveX *= _runAcceleration;
            Vector2 addedVelocity = _physicsObject.MoveRigidbody(moveX, 0, false);

            float newVelocityX = _physicsObject.projectedVelocity.x + addedVelocity.x;
            if (newVelocityX > _maxRunSpeed)
            {
                //_physicsObject.ForceRigidbody(_maxRunSpeed, 0, true);
            }
            else if (newVelocityX < -_maxRunSpeed)
            {
                //_physicsObject.ForceRigidbody(-_maxRunSpeed, 0, true);
            }
            else
            {
                _physicsObject.MoveRigidbody(moveX, 0);
            }
        }
        else
        {
            moveX *= _airAcceleration * _airAcceleration;
            Vector2 addededVelocity = _physicsObject.MoveRigidbody(moveX, 0, false);

            float newVelocityX = _physicsObject.projectedVelocity.x + addededVelocity.x;
            if (newVelocityX > _maxAirSpeed)
            {

            }
            else if (newVelocityX < -_maxAirSpeed)
            {

            }
            else
            {
                _physicsObject.MoveRigidbody(moveX, 0);
            }
        }

        if (_physicsObject.collisions.groundedLastFrame)
            _airJumpsLeft = _maxAirJumps;

        if (_playerInput.Jump.Down)
        {
            if (_physicsObject.isGrounded)
            {
                _physicsObject.collisions.ignoreGroundThisFrame = true;
                _physicsObject.ForceRigidbody(0, _jumpSpeed, false, true);
            }
            else if (!_physicsObject.isGrounded && _airJumpsLeft > 0)
            {
                _physicsObject.collisions.ignoreGroundThisFrame = true;
                _physicsObject.ForceRigidbody(0, _jumpSpeed, false, true);
                _airJumpsLeft--;
            }
        }
    }
}

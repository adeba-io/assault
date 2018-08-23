﻿#define COLL_RAYS
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[DisallowMultipleComponent]
public class PhysicsObject : MonoBehaviour
{
    #region Internal Types

    struct RaycastPoints
    {
        public Vector2 top, bottom;
        public Vector2 left, right;
    }

    #endregion

    #region Events, Fields, and Properties

    public Action<Collider2D> OnTriggerEnterEvent;
    public Action<Collider2D> OnTriggerStayEvent;
    public Action<Collider2D> OnTriggerExitEvent;

    [SerializeField] protected CollisionState _collisionState;
    [SerializeField] protected bool _useGravity = true;
    [SerializeField] protected bool _useFriction = true;
    [SerializeField] protected float _gravityMultiplier = 1f;
    [SerializeField] protected Vector2 _projectedVelocity;
    [SerializeField] Vector2 _currentVelocity;
    [Space]

    // Collision Fields
    [SerializeField]
    [Range(0.1f, 0.4f)]
    float _skinWidth = 0.02f;
    [SerializeField]
    [Range(0f, 0.3f)]
    float _horiSkinBuffer = 0.3f;

    [SerializeField]
    protected LayerMask _collisionMask = 0;

    // Raycasts
    RaycastPoints _raycastPoints;

    // Slopes
    [SerializeField]
    [Range(0f, 45f)]
    protected float _groundSlopeLimit = 30f, _ceilingSlopeLimit = 10f;

    [SerializeField]
    protected AnimationCurve _slopeSpeedModifier =
        new AnimationCurve(new Keyframe(-90f, 1.5f), new Keyframe(0f, 1f), new Keyframe(90f, 0.5f));

    bool _onSlope = false;

    // Movement and Positioning Fields
    Rigidbody2D _rigidbody;
    BoxCollider2D _collider;
    Vector2 _prevPosition;
    Vector2 _currPosition;
    Vector2 _deltaMovement;

    [SerializeField]
    [Range(0.001f, 0.02f)]
    float _minMoveDistance = 0.01f;

    // Properties
    public bool useGravity { get { return _useGravity; } set { useGravity = value; } }
    public float gravityMultiplier { get { return _gravityMultiplier; } set { _gravityMultiplier = value; } }

    public float skinWidth
    {
        get { return _skinWidth; }
        set { _skinWidth = value; ResetRaycastPoints(); }
    }

    public bool isGrounded { get { return _collisionState.below && _collisionState.belowPlatform; } }
    public CollisionState collisionState { get { return _collisionState; } }

    public Vector2 projectedVelocity { get { return _projectedVelocity; } }
    public Vector2 velocity { get { return _currentVelocity; } }

    const float k_skinWidthBuffer = 0.001f;

    #endregion

    #region Monobehaviours

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();

        _rigidbody.bodyType = RigidbodyType2D.Kinematic;

        _prevPosition = _rigidbody.position;
        _currPosition = _rigidbody.position;
    }

    private void FixedUpdate()
    {
        // Take external forces into account
        Gravity();
        GroundFriction();
        AirFriction();

        // Setup for new FixedUpdate
        _collisionState.Reset();
        _onSlope = false;
        ResetRaycastPoints();

        // Calc _deltaMovement
        _prevPosition = _rigidbody.position;
        _currPosition = _prevPosition + (_projectedVelocity * Time.deltaTime);
        _deltaMovement = _currPosition - _prevPosition;

        if (_deltaMovement.magnitude < _minMoveDistance) _deltaMovement = Vector2.zero;

        // Adjust _deltaMovement according to collisions
        // AdjustHorizontal first
        AdjustHorizontal();
        AdjustVertical();

        AdjustForSlope();

        // Find new _currPosition an move rigidbody accordingly
        _currPosition = _prevPosition + _deltaMovement;
        _rigidbody.MovePosition(_currPosition);

        // Only find _currentVelocity if time is passing in game
        if (Time.deltaTime > 0) _currentVelocity = _deltaMovement / Time.deltaTime;
        // If we're grounded we have no important Y velocity
        if (_collisionState.belowPlatform) _currentVelocity.y = 0;

        if (!_collisionState.groundedLastFrame && _collisionState.below)
            _collisionState.groundedThisFrame = true;

        if (!_collisionState.hitCeilingLastFrame && _collisionState.above)
            _collisionState.hitCeilingThisFrame = true;

        if (!_collisionState.hitLeftWallLastFrame && _collisionState.left)
            _collisionState.hitLeftWallThisFrame = true;

        if (!_collisionState.hitRightWallLastFrame && _collisionState.right)
            _collisionState.hitRightWallThisFrame = true;

        if (_collisionState.groundedThisFrame || _collisionState.hitCeilingThisFrame)
            _projectedVelocity.y = 0;

        if (_collisionState.hitLeftWallThisFrame || _collisionState.hitRightWallThisFrame)
            _projectedVelocity.x = 0;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (OnTriggerEnterEvent != null)
            OnTriggerEnterEvent(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (OnTriggerStayEvent != null)
            OnTriggerStayEvent(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (OnTriggerExitEvent != null)
            OnTriggerExitEvent(collision);
    }

    #endregion

    #region Public

    /// <summary>
    /// Accelerates the GameObject. Automatically multiplies the move by Time.deltaTime. Returns the GameObject's new velocity
    /// </summary>
    /// <param name="move">The Vector2 to accelerate by</param>
    /// <param name="alterVelocity">Should we alter the valocity with?</param>
    /// <returns></returns>
    public Vector2 MoveRigidbody(Vector2 move, bool alterVelocity = true)
    {
        Vector2 toAdd = move * Time.deltaTime;

        if (alterVelocity) _projectedVelocity += toAdd;

        return toAdd;
    }

    /// <summary>
    /// Accelerates the GameObject. Automatically multiplies the added force by Time.deltaTime. Returns the added velocity
    /// </summary>
    /// <param name="moveX">The X component to accelerate by</param>
    /// <param name="moveY">The Y component to accelerate by</param>
    /// <param name="alterVelocity">Should we alter the velocity?</param>
    /// <returns></returns>
    public Vector2 MoveRigidbody(float moveX, float moveY, bool alterVelocity = true)
    {
        Vector2 toAdd = new Vector2(moveX, moveY) * Time.deltaTime;

        if (alterVelocity) _projectedVelocity += toAdd;

        return toAdd;
    }

    public void ForceRigidbody(Vector2 force, bool resetX = false, bool resetY = false)
    {
        if (resetX) _projectedVelocity.x = 0;
        if (resetY) _projectedVelocity.y = 0;

        _projectedVelocity += force;
    }

    public void ForceRigidbody(float forceX, float forceY, bool resetX = false, bool resetY = false)
    {
        if (resetX) _projectedVelocity.x = 0;
        if (resetY) _projectedVelocity.y = 0;

        _projectedVelocity += new Vector2(forceX, forceY);
    }

    /// <summary>
    /// Moves the GameObject without any implied velocity changes
    /// </summary>
    /// <param name="newPosiition">The new position of the GameObject in global coordinates</param>
    public void Teleport(Vector2 newPosiition)
    {
        Vector2 deltaPosition = newPosiition = _currPosition;
        _prevPosition += deltaPosition;
        _currPosition = newPosiition;
        _rigidbody.MovePosition(newPosiition);
    }

    #endregion

    #region Movement

    void AdjustVertical()
    {
        // Setup required varaibles
        Vector2 hitNormal = Vector2.zero;
        float distanceToHit = 0;

        // Below Check
        CheckVerticalCollisions(false, ref distanceToHit, ref hitNormal);

        // It's a platform we ant to use only if it hasn't been picked up as a ;eft or right collider
        // This allows us to keep falling if we are in the middle of a platform
        if (_collisionState.rightPlatform == _collisionState.belowPlatform || _collisionState.leftPlatform == _collisionState.belowPlatform)
        {
            _collisionState.below = false;
            _collisionState.belowPlatform = null;
        }

        if (_collisionState.belowPlatform)
        {
            float adjustDistance = distanceToHit - _skinWidth; //  - _skinWidth

            if (adjustDistance < 0) adjustDistance = 0;

            _deltaMovement.y = -adjustDistance;

            if (_deltaMovement.y > 0) _deltaMovement.y = 0;
        }


        // Above CHeck
        distanceToHit = 0;
        hitNormal = Vector2.zero;

        CheckVerticalCollisions(true, ref distanceToHit, ref hitNormal);

        // If the platform's been deemed a left or right platform we cannot use it
        if (_collisionState.rightPlatform == _collisionState.abovePlatform || _collisionState.leftPlatform == _collisionState.abovePlatform)
        {
            _collisionState.above = false;
            _collisionState.abovePlatform = null;
        }

        // If we're sandwiched between two platforms, don't move
        if (_collisionState.abovePlatform && _collisionState.belowPlatform)
        {
            _deltaMovement.y = 0;
            return;
        }

        if (_collisionState.abovePlatform)
        {
            float adjustDistance = distanceToHit - _skinWidth;

            if (adjustDistance < 0) adjustDistance = 0;

            _deltaMovement.y = adjustDistance;
        }
    }

    void AdjustHorizontal()
    {
        // Setuo required varaibles
        Vector2 hitNormal = Vector2.zero;
        float distanceToHit = 0;

        // Right Check
        CheckHorizontalCollisions(true, ref distanceToHit, ref hitNormal);

        if (_collisionState.rightPlatform)
        {
            float wallAngle = Vector2.Angle(Vector2.left, hitNormal);

            if (wallAngle > 0 && wallAngle != 90f)
            {
                // Draw Ray
                Vector2 rayPoint = _raycastPoints.right;
                rayPoint.x += distanceToHit;
                DrawRay(rayPoint, hitNormal * 0.5f, slopeRayColor);

                // Adjust for slope
                // No matter if we're going up or down we want to shift left
                float toMove = Mathf.Tan(wallAngle * Mathf.Deg2Rad) * Mathf.Abs(_deltaMovement.y);
                _deltaMovement.x = -toMove;
            }
            else
            {
                float adjustDistance = distanceToHit - _skinWidth - _horiSkinBuffer;

                if (adjustDistance < 0) adjustDistance = 0;

                _deltaMovement.x = adjustDistance;

                //if (_deltaMovement.x < 0) _deltaMovement.x = 0;
            }
        }


        // Left Check
        hitNormal = Vector2.zero;
        distanceToHit = 0;

        CheckHorizontalCollisions(false, ref distanceToHit, ref hitNormal);
        /*
        // If we're picking up collisions from the same collider on both sides
        // We're in the middle of the collider so both should be ignored
        if (_collisionState.rightPlatform == _collisionState.leftPlatform)
        {
            _collisionState.left = _collisionState.right = false;
            _collisionState.leftPlatform = _collisionState.rightPlatform = null;
        }
        */
        // If we're sandwiched between two platforms, don't move
        if (_collisionState.rightPlatform && _collisionState.leftPlatform)
        {
            _deltaMovement.x = 0;
        }

        if (_collisionState.leftPlatform)
        {
            float wallAngle = Vector2.Angle(Vector2.right, hitNormal);

            if (wallAngle > 0 && wallAngle != 90f)
            {
                // Draw Ray
                Vector2 rayPoint = _raycastPoints.left;
                rayPoint.x -= distanceToHit;
                DrawRay(rayPoint, hitNormal * 0.5f, slopeRayColor);

                // Adjust for slope
                // No matter if we're going up or down we want to shift right
                float toMove = Mathf.Tan(wallAngle * Mathf.Deg2Rad) * Mathf.Abs(_deltaMovement.y);
                _deltaMovement.x = toMove;
            }
            else
            {
                float adjustDistance = distanceToHit - _skinWidth - _horiSkinBuffer;

                if (adjustDistance < 0) adjustDistance = 0;

                _deltaMovement.x = -adjustDistance;

                if (_deltaMovement.x > 0) _deltaMovement.x = 0;
            }
        }
    }

    void AdjustForSlope()
    {
        if (!_onSlope) return;
        if (!_collisionState.below) return;
        if (!_collisionState.belowPlatform) return;
        if (_deltaMovement.x == 0) return;

        // Declare required variables
        Vector2 raycastStart, raycastDirection;
        float raycastDistance;
        RaycastHit2D raycastHit;

        // Setup required variables
        raycastStart = _raycastPoints.bottom; // + _deltaMovement
        raycastDirection = Vector2.down;

        raycastDistance = 0.1f + _skinWidth;

        // Raycast Stuff 1
        DrawRay(raycastStart, raycastDirection * raycastDistance, slopeRayColor);

        raycastHit = Physics2D.Raycast(raycastStart, raycastDirection, raycastDistance, _collisionMask);
        if (raycastHit)
        {
            // Draw Ray
            DrawRay(raycastHit.point, raycastHit.normal * 0.3f, slopeRayColor);

            // Calc angle
            float slopeAngle = Vector2.Angle(Vector2.up, raycastHit.normal);
            // If the angle is greater than _groundSlopeLimit or equals zero, bail
            if (slopeAngle > _groundSlopeLimit || slopeAngle == 0) return;

            // If the X component of our movement and the hit normal have the same sign
            // (i.e. point in the same direction) we are moving down the slope
            bool movingDownSlope = Mathf.Sign(raycastHit.normal.x) == Mathf.Sign(_deltaMovement.x);
            if (movingDownSlope)
            {
                // Going down we want to speed up
                // The _slopeSpeedModifier curve should be > 1 for negative values
                float speedMultiplier = _slopeSpeedModifier.Evaluate(-slopeAngle);
                _deltaMovement.x *= speedMultiplier;
                // Calc the alteration in the Y axis
                float shiftY = Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(_deltaMovement.x);
                // To ensure we stick to the ground
                _deltaMovement.y -= (raycastHit.distance - (_skinWidth * 0.75f)); // - _skinWidth
                _deltaMovement.y -= shiftY;
            }
            else
            {
                // Going up we want to slow down
                // The _slopeSpeedModifier curve should be < 1 for positive values
                float speedMultiplier = _slopeSpeedModifier.Evaluate(slopeAngle);
                _deltaMovement.x *= speedMultiplier;
                // Calc the alteration in the Y axis
                float shiftY = Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(_deltaMovement.x);
                _deltaMovement.y += shiftY;
            }
        }
    }

    #endregion

    #region Collision

    void CheckVerticalCollisions(bool above, ref float distancehToHit, ref Vector2 hitNormal)
    {
        // Declare required variables
        Vector2 raycastStart, raycastDirection;
        float raycastDistance;
        RaycastHit2D raycastHit;
        bool goingUp;
        float appropriateSlopeLimit;

        // Setup required variables
        raycastStart = (above ? _raycastPoints.top : _raycastPoints.bottom);
        raycastStart.x += _deltaMovement.x;
        raycastDirection = (above ? Vector2.up : Vector2.down);

        raycastDistance = (above ? _deltaMovement.y : -_deltaMovement.y) + _skinWidth;
        if (raycastDistance < _skinWidth) raycastDistance = _skinWidth;

        goingUp = _deltaMovement.y > 0;
        appropriateSlopeLimit = (above ? _ceilingSlopeLimit : _groundSlopeLimit);

        // Raycast Stuff
        DrawRay(raycastStart, raycastDirection * raycastDistance, vertRayColor);

        raycastHit = Physics2D.Raycast(raycastStart, raycastDirection, raycastDistance, _collisionMask);
        if (raycastHit)
        {
            // Don't register collisions if we're moving away from the collider
            // A temporary failsafe to allow us to move away from touching colliders
            if ((above && !goingUp) || (!above && goingUp)) return;
            // We calc against the reverse of raycastDirection
            float currAngle = Vector2.Angle(-raycastDirection, raycastHit.normal);
            // If the angle is greater that the appropriateSlopeLimit, bail
            if (currAngle > appropriateSlopeLimit) return;

            Platform potentialPlatform = raycastHit.collider.gameObject.GetComponent<Platform>();
            if (potentialPlatform)
            {
                if (above)
                {
                    _collisionState.above = true;
                    _collisionState.abovePlatform = potentialPlatform;
                }
                else
                {
                    _collisionState.below = true;
                    _collisionState.belowPlatform = potentialPlatform;

                    if (currAngle > 0) _onSlope = true;
                }
            }

            distancehToHit = raycastHit.distance;
            hitNormal = raycastHit.normal;
        }
    }

    void CheckHorizontalCollisions(bool right, ref float distanceToHit, ref Vector2 hitNormal)
    {
        // Declare reuired variables
        Vector2 raycastStart, raycastDirection;
        float raycastDistance;
        RaycastHit2D raycastHit;
        bool goingRight;

        // Setup required varaibles
        raycastStart = (right ? _raycastPoints.right : _raycastPoints.left);
        if (right)
            raycastStart.x -= _horiSkinBuffer;
        else
            raycastStart.x += _horiSkinBuffer;

        raycastDirection = (right ? Vector2.right : Vector2.left);

        raycastDistance = (right ? _deltaMovement.x : -_deltaMovement.x) + _skinWidth + _horiSkinBuffer;
        if (raycastDistance < _skinWidth) raycastDistance = _skinWidth;

        goingRight = _deltaMovement.x > 0;

        // Raycast Stuff
        DrawRay(raycastStart, raycastDirection * raycastDistance, horiRayColor);

        raycastHit = Physics2D.Raycast(raycastStart, raycastDirection, raycastDistance, _collisionMask);
        if (raycastHit)
        {
            // Don't register collisions if we're moving away from the collider
            // A temporary failsafe to allow us to move away from touching colliders
            if (_deltaMovement.x != 0)
                if ((right && !goingRight) || (!right && goingRight)) return;
            // We calc against the reverse of raycastDirection
            float currAngle = Vector2.Angle(Vector2.up, raycastHit.normal);
            // If the angle is less than or equal to _groundSlopeLimit, or greater than or equal to _ceilingSlopeLimit,
            // bail it's not a wall
            if (currAngle <= _groundSlopeLimit || currAngle >= 180f - _ceilingSlopeLimit) return;

            Platform potentialWall = raycastHit.collider.gameObject.GetComponent<Platform>();
            if (potentialWall)
            {
                if (right)
                {
                    _collisionState.right = true;
                    _collisionState.rightPlatform = potentialWall;
                }
                else
                {
                    _collisionState.left = true;
                    _collisionState.leftPlatform = potentialWall;
                }
            }

            distanceToHit = raycastHit.distance;
            hitNormal = raycastHit.normal;
        }
    }

    void ResetRaycastPoints()
    {
        Bounds modifiedBounds = _collider.bounds;
        modifiedBounds.Expand(-2f * _skinWidth);

        float middleX = (modifiedBounds.min.x + modifiedBounds.max.x) / 2;
        float middleY = (modifiedBounds.min.y + modifiedBounds.max.y) / 2;

        _raycastPoints.top = new Vector2(middleX, modifiedBounds.max.y);
        _raycastPoints.bottom = new Vector2(middleX, modifiedBounds.min.y);
        _raycastPoints.left = new Vector2(modifiedBounds.min.x, middleY);
        _raycastPoints.right = new Vector2(modifiedBounds.max.x, middleY);
    }

    #endregion

    #region External Forces

    void Gravity()
    {
        if (!_useGravity) return;
        if (_collisionState.groundedLastFrame) return;

        Vector2 toMove = PhysicsManager.gravity * _gravityMultiplier * Time.deltaTime;

        _projectedVelocity += toMove;
    }

    void GroundFriction()
    {
        if (!_collisionState.below) return;
        if (!_collisionState.belowPlatform) return;
        if (!_useFriction) return;

        if (_projectedVelocity.x > 0)
            _projectedVelocity.x -= _collisionState.belowPlatform.friction * Time.deltaTime;
        else if (_projectedVelocity.x < 0)
            _projectedVelocity.x += _collisionState.belowPlatform.friction * Time.deltaTime;
    }

    void AirFriction()
    {
        if (_collisionState.below) return;
        if (_collisionState.groundedLastFrame) return;
        if (!_useFriction) return;

        if (_projectedVelocity.x > 0)
            _projectedVelocity.x -= PhysicsManager.airFriction.x * Time.deltaTime;
        else if (_projectedVelocity.x < 0)
            _projectedVelocity.x += PhysicsManager.airFriction.x * Time.deltaTime;

        if (_projectedVelocity.y < 0)
            _projectedVelocity.y += PhysicsManager.airFriction.y * Time.deltaTime;
    }

    #endregion

    #region DEBUG
#if COLL_RAYS

    Color vertRayColor = Color.red, horiRayColor = Color.blue, slopeRayColor = Color.yellow;

    void DrawRay(Vector2 start, Vector2 dir, Color color)
    {
        Debug.DrawRay(start, dir, color);
    }

#endif
    #endregion
}

/// <summary>
/// Stores the collision information, for above, below, left, and right
/// </summary>
[Serializable]
public class CollisionState
{
    public bool above, below;
    public bool left, right;

    public Platform abovePlatform;
    public Platform belowPlatform;
    public Platform leftPlatform;
    public Platform rightPlatform;

    public bool hitCeilingLastFrame, hitCeilingThisFrame;
    public bool groundedLastFrame, groundedThisFrame;
    public bool hitLeftWallLastFrame, hitLeftWallThisFrame;
    public bool hitRightWallLastFrame, hitRightWallThisFrame;

    public bool hasCollision { get { return above || below || left || right; } }

    public bool touchingWall { get { return leftPlatform || rightPlatform; } }

    public void Reset()
    {
        hitCeilingLastFrame = hitCeilingThisFrame;
        groundedLastFrame = below;
        hitLeftWallLastFrame = hitLeftWallThisFrame;
        hitRightWallLastFrame = hitRightWallThisFrame;

        above = below = left = right = false;
        hitCeilingThisFrame = groundedThisFrame = hitLeftWallThisFrame = hitRightWallThisFrame = false;
        abovePlatform = belowPlatform = leftPlatform = rightPlatform = null;
    }
}

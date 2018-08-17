#define COLL_RAYS
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PhysicsObjectMK2 : MonoBehaviour
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

    [SerializeField] CollisionState _collisionState;
    [SerializeField] bool _useGravity = true;
    [SerializeField] float _gravityMultiplier = 1f;
    [SerializeField] Vector2 _projectedVelocity;
    [SerializeField] Vector2 _currentVelocity;
    [Space]

    // Collision Fields
    [SerializeField] [Range(0.1f, 0.4f)]
    float _skinWidth = 0.02f;
    [SerializeField] [Range(0f, 0.3f)]
    float _horiSkinBuffer = 0.3f;

    public LayerMask _collisionMask = 0;

    List<Platform> _adjustmentObjects = new List<Platform>(3);

    // Raycasts
    RaycastPoints _raycastPoints;

    // Slopes
    [SerializeField] [Range(0f, 45f)]
    float _groundSlopeLimit = 30f, _ceilingSlopeLimit = 10f;

    [SerializeField]
    AnimationCurve _slopeSpeedModifier =
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

    public float skinWidth { get { return _skinWidth; } set { _skinWidth = value; } }

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
        // Setup for new FixedUpdate
        _collisionState.Reset();
        _onSlope = false;
        ResetRaycastPoints();

        // Take external forces into account
        Gravity();

        // Calc _deltaMovement
        _prevPosition = _rigidbody.position;
        _currPosition = _prevPosition + (_projectedVelocity * Time.deltaTime);
        _deltaMovement = _currPosition - _prevPosition;

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

        if (!_collisionState.hitGroundLastFrame && _collisionState.below)
            _collisionState.hitGroundThisFrame = true;

        if (!_collisionState.hitCeilingLastFrame && _collisionState.above)
            _collisionState.hitCeilingThisFrame = true;

        if (!_collisionState.hitLeftWallLastFrame && _collisionState.left)
            _collisionState.hitLeftWallThisFrame = true;

        if (!_collisionState.hitRightWallLastFrame && _collisionState.right)
            _collisionState.hitRightWallThisFrame = true;

        if (_collisionState.hitGroundThisFrame || _collisionState.hitCeilingThisFrame)
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
    /// Increases the velocity of the GameObject. Automatically multiplies the move by Time.deltaTime. Returns the GameObject's new velocity
    /// </summary>
    /// <param name="move">The Vector2to be added to the velocity</param>
    /// <param name="alterVelocity">Should we alter the valocity with move?</param>
    /// <returns></returns>
    public Vector2 MoveRigidbody(Vector2 move, bool alterVelocity = true)
    {
        Vector2 toAdd = move * Time.deltaTime;

        if (alterVelocity) _projectedVelocity += toAdd;
        
        return toAdd;
    }

    /// <summary>
    /// Increases the velocity. Returns the added velocity
    /// </summary>
    /// <param name="moveX"></param>
    /// <param name="moveY"></param>
    /// <param name="alterVelocity"></param>
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

        /*
        bool goingUp = _deltaMovement.y > 0f;
        float aboveDist = 0, belowDist = 0;
        Vector2 ceilingNormal = Vector2.zero, belowNormal = Vector2.zero;

        CheckAboveCollisions(goingUp, ref aboveDist, ref ceilingNormal);
        CheckBelowCollisions(goingUp, ref belowDist, ref belowNormal);

        if (_collisionState.abovePlatform && _collisionState.belowPlatform)
        {
            _deltaMovement.y = 0;
            return;
        }
        
        if (aboveDist != 0)
        {
            _deltaMovement.y += aboveDist - _skinWidth;

            if (ceilingNormal != Vector2.zero)
            {
                bool moveRight = ceilingNormal.x > 0;
                float ceilingAngle = Vector2.SignedAngle(Vector2.down, ceilingNormal);
                float toMove = 0;

                DrawRay(_raycastPoints.top, ceilingNormal * 0.5f, slopeRayColor);

                toMove = Mathf.Tan(ceilingAngle * Mathf.Deg2Rad) * _deltaMovement.y;

                _deltaMovement.x += toMove;
            }
        }
        else if (belowDist != 0)
        {
            if (_collisionState.below)
                _deltaMovement.y -= (belowDist - _skinWidth);
            /*
            if (belowNormal != Vector2.zero)
            {
                bool moveRight = belowNormal.x > 0;
                float belowAngle = Vector2.SignedAngle(Vector2.up, belowNormal);

                DrawRay(_raycastPoints.bottom, belowNormal * 0.5f, slopeRayColor);

                float toMove = Mathf.Tan(belowAngle * Mathf.Deg2Rad) * _deltaMovement.y;

                _deltaMovement.x += toMove;
            }*
        }*/
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
            print(wallAngle);

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

           // _deltaMovement.x += (distanceToHit - _skinWidth);
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
            //_deltaMovement.x -= (distanceToHit - _skinWidth);
        }

        /*
        bool goingRight = _deltaMovement.x > 0f;
        float rightDist = 0, leftDist = 0;
        Vector2 rightNormal = Vector2.zero, leftNormal = Vector2.zero;

        CheckRightCollisions(goingRight, ref rightDist, ref rightNormal);
        CheckLeftCollisions(goingRight, ref leftDist, ref leftNormal);

        if (_collisionState.leftPlatform && _collisionState.rightPlatform)
        {
            _deltaMovement.x = 0;
            return;
        }

        if (rightDist != 0)
        {
            _deltaMovement.x -= rightDist + _skinWidth;
            
            if (rightNormal != Vector2.zero)
            {
                float wallAngle = Vector2.SignedAngle(Vector2.left, rightNormal);
                print("Angle adjust");

                if (wallAngle > 0 && _deltaMovement.y < 0)
                {
                    DrawRay(_raycastPoints.right, rightNormal * 0.5f, slopeRayColor);

                    float toMove = Mathf.Tan(wallAngle * Mathf.Deg2Rad) * _deltaMovement.y;

                    _deltaMovement.x -= toMove;
                }
            }
            else
            {
                 _deltaMovement.x -= rightDist + _skinWidth;
            }
        }
        else if (leftDist != 0)
        {
            _deltaMovement.x -= (leftDist - _skinWidth);
            /*
            if (leftNormal != Vector2.zero)
            {
                float wallAngle = Vector2.SignedAngle(Vector2.right, leftNormal);

                if (wallAngle > 0 && _deltaMovement.y < 0)
                {
                    DrawRay(_raycastPoints.left, leftNormal * 0.5f, slopeRayColor);

                    float toMove = Mathf.Tan(wallAngle * Mathf.Deg2Rad) * _deltaMovement.y;

                    _deltaMovement.x += toMove;
                }
            }
        }*/
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
        bool goingRight;

        // Setup required variables
        raycastStart = _raycastPoints.bottom; // + _deltaMovement
        raycastDirection = Vector2.down;

        raycastDistance = 0.1f + _skinWidth;

        goingRight = _deltaMovement.x > 0;

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

        /*
        raycastStart = _raycastPoints.bottom;
        raycastStart.x += _deltaMovement.x;
        raycastDirection = Vector2.down;
        raycastDistance = .2f;

        raycastHit = Physics2D.Raycast(raycastStart, raycastDirection, raycastDistance, _collisionMask);
        if (raycastHit)
        {
            float angle = Vector2.Angle(raycastHit.normal, Vector2.up);
            if (angle == 0)
                return;

            bool movingDownSlope = Mathf.Sign(raycastHit.normal.x) == Mathf.Sign(_deltaMovement.x);
            if (movingDownSlope)
            {
                float speedModifier = _slopeSpeedModifier.Evaluate(-angle);
                _deltaMovement.x *= speedModifier;
            }
            else
            {
                float speedModifier = _slopeSpeedModifier.Evaluate(angle);
                _deltaMovement.x *= speedModifier;
            }
        }*/
    }

    #endregion

    #region Collision

    void CheckAboveCollisions(bool goingUp, ref float aboveDistance, ref Vector2 ceilingNormal)
    {
        // Declare required variables
        Vector2 raycastStart, raycastDirection;
        float raycastDistance;
        RaycastHit2D raycastHit;

        // Setup required variables
        raycastStart = _raycastPoints.top;
        raycastDirection = Vector2.up;

        raycastDistance = _deltaMovement.y;
        if (raycastDistance < _skinWidth) raycastDistance = _skinWidth;

        DrawRay(raycastStart, raycastDirection * raycastDistance, vertRayColor);

        raycastHit = Physics2D.Raycast(raycastStart, raycastDirection, raycastDistance, _collisionMask);
        if (raycastHit)
        {
            // Betweeen normal and Vector2.down as we check above
            float currAngle = Vector2.Angle(raycastHit.normal, Vector2.down);
            // If the angle is greater bail as it is not a ceiling
            if (currAngle > _ceilingSlopeLimit) return;

            Platform potentPlatform = raycastHit.collider.gameObject.GetComponent<Platform>();

            if (potentPlatform)
            {
                _collisionState.above = true;
                _collisionState.abovePlatform = potentPlatform;
            }

            if (!goingUp) _collisionState.above = false;

            aboveDistance = raycastHit.distance;
            ceilingNormal = raycastHit.normal;
        }
        
        if (!_collisionState.hitCeilingLastFrame && _collisionState.above)
            _collisionState.hitCeilingThisFrame = true;
    }

    void CheckBelowCollisions(bool goingUp, ref float belowDistance, ref Vector2 belowNormal)
    {
        Vector2 raycastStart, raycastDirection;
        float raycastDistance;
        RaycastHit2D raycastHit;

        raycastStart = _raycastPoints.bottom;
        raycastDirection = Vector2.down;

        raycastDistance = _skinWidth; // - _deltaMovement.y

        DrawRay(raycastStart, raycastDirection * raycastDistance, vertRayColor);

        raycastHit = Physics2D.Raycast(raycastStart, raycastDirection, raycastDistance, _collisionMask);
        if (raycastHit)
        {
            float currAngle = Vector2.Angle(raycastHit.normal, Vector2.up);
            if (currAngle > _groundSlopeLimit) return;

            Platform potentPlatform = raycastHit.collider.gameObject.GetComponent<Platform>();

            if (potentPlatform)
            {
                _collisionState.below = true;
                _collisionState.belowPlatform = potentPlatform;

                if (currAngle > 0) _onSlope = true;
            }

            //if (goingUp) _collisionState.below = false;

            belowDistance = raycastHit.distance;
        }
    }

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
            print(currAngle + " " + raycastHit.normal);

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

    void CheckRightCollisions(bool goingRight, ref float rightDistance, ref Vector2 rightNormal)
    {
        Vector2 raycastStart, raycastDirection;
        float raycastDistance;
        RaycastHit2D raycastHit;

        raycastStart = _raycastPoints.right + _deltaMovement;
        raycastDirection = Vector2.right;

        raycastDistance = _deltaMovement.x + _skinWidth;
        if (raycastDistance < _skinWidth) raycastDistance = _skinWidth;

        DrawRay(raycastStart, raycastDirection * raycastDistance, horiRayColor);

        raycastHit = Physics2D.Raycast(raycastStart, raycastDirection, raycastDistance, _collisionMask);
        if (raycastHit)
        {
            float currAngle = Vector2.Angle(raycastHit.normal, -raycastDirection);
            Platform potentWall = raycastHit.collider.gameObject.GetComponent<Platform>();

            if (potentWall)
            {
                if (currAngle > _groundSlopeLimit && currAngle < 90f - _ceilingSlopeLimit)
                {
                    _collisionState.right = true;
                    _collisionState.rightPlatform = potentWall;
                }
            }

            if (!goingRight) _collisionState.right = false;

            rightDistance = raycastHit.distance;
            rightNormal = raycastHit.normal;
        }
    }

    void CheckLeftCollisions(bool goingRight, ref float leftDistance, ref Vector2 leftNormal)
    {
        Vector2 raycastStart, raycastDirection;
        float raycastDistance;
        RaycastHit2D raycastHit;

        raycastStart = _raycastPoints.left + _deltaMovement;
        raycastDirection = Vector2.left;

        raycastDistance = _skinWidth - _deltaMovement.x;
        if (raycastDistance < _skinWidth) raycastDistance = _skinWidth;

        DrawRay(raycastStart, raycastDirection * raycastDistance, horiRayColor);

        raycastHit = Physics2D.Raycast(raycastStart, raycastDirection, raycastDistance, _collisionMask);
        if (raycastHit)
        {
            float currAngle = Vector2.Angle(raycastHit.normal, -raycastDirection);
            Platform potentWall = raycastHit.collider.gameObject.GetComponent<Platform>();

            if (potentWall)
            {
                if (currAngle > _groundSlopeLimit && currAngle < 90f - _ceilingSlopeLimit)
                {
                    _collisionState.left = true;
                    _collisionState.leftPlatform = potentWall;
                }
            }

            if (goingRight) _collisionState.left = false;

            leftDistance = raycastHit.distance;
            leftNormal = raycastHit.normal;
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
        if (_collisionState.hitGroundLastFrame) return;
        
        Vector2 toMove = PhysicsManager.gravity * _gravityMultiplier * Time.deltaTime;

        _projectedVelocity += toMove;
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
    public bool hitGroundLastFrame, hitGroundThisFrame;
    public bool hitLeftWallLastFrame, hitLeftWallThisFrame;
    public bool hitRightWallLastFrame, hitRightWallThisFrame;
    
    public bool hasCollision { get { return above || below || left || right; } }

    public void Reset()
    {
        hitCeilingLastFrame = hitCeilingThisFrame;
        hitGroundLastFrame = below;
        hitLeftWallLastFrame = hitLeftWallThisFrame;
        hitRightWallLastFrame = hitRightWallThisFrame;

        above = below = left = right = false;
        hitCeilingThisFrame = hitGroundThisFrame = hitLeftWallThisFrame = hitRightWallThisFrame = false;
        abovePlatform = belowPlatform = leftPlatform = rightPlatform = null;
    }
}

#define COLLISION_RAYS
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PhysicsObject : MonoBehaviour
{
    #region Internal Types

    struct RaycastOrigins
    {
        public Vector2 centralRaycastPoint;

        public Vector2 topLeft, bottomLeft, bottomRight;
    }

    [Serializable]
    public class CollisionState
    {
        public bool above, below;
        public bool left, right;

        public bool hitCeiling;
        public bool groundedLastFrame, groundedThisFrame;
        public Platform groundPlatform;

        public float groundSlopeAngle { get { return groundPlatform.slopeAngle; } }
        public bool hasCollision { get { return above || below || left || right; } }

        public void Reset()
        {
            groundedLastFrame = below;
            above = below = left = right = hitCeiling = groundedThisFrame = false;
            groundPlatform = null;
        }

        public override string ToString()
        {
            return string.Format("[CollisionState] = a: {0}, b: {1}, l: {2}, r: {3}, groundedLastFrame: {4}, groundedThisFrame: {5}, Curr Platform: {6}, Curr Slope Angle: {7}",
                                above, below, left, right, groundedLastFrame, groundedThisFrame, groundPlatform.name, groundSlopeAngle);
        }
    }

    #endregion

    #region Events, Fields, and Properties

    // Events
    public event Action<RaycastHit2D> OnCollisionEvent;
    public event Action<Collider2D> OnTriggerEnterEvent;
    public event Action<Collider2D> OnTriggerStayEvent;
    public event Action<Collider2D> OnTriggerExitEvent;

    [SerializeField] CollisionState _collisionState;
    [SerializeField] bool _useGravity = true;
    [SerializeField] float _gravityMultiplier = 1f;
    [SerializeField] Vector2 _projectedVelocity;
    [SerializeField] Vector2 _currentVelocity;
    [Space]

    // Collision Fields
    [SerializeField]
    [Range(0.001f, 0.3f)]
    float _skinWidth = 0.02f;

    public LayerMask _collisionMask = 0;
    public LayerMask _triggerMask = 0;

    // Slopes
    [Range(0f, 90f)] [SerializeField]
    float _slopeLimit = 30f;
    float _slopeLimitTangent = Mathf.Tan(75f * Mathf.Deg2Rad);
    [SerializeField]
    AnimationCurve _slopeSpeedModifier =
        new AnimationCurve(new Keyframe(-90f, 1.5f), new Keyframe(0f, 1f), new Keyframe(90f, 0f));
    bool _onSlope = false;
    // Used to mark the case where we are travelling up a slope and _deltaMovement.y is modified
    // If true : we can make adjustments when the end of the slope is reached to stay grounded
    bool _goingUpSlope = false;

    // Raycasts
    RaycastOrigins _rayOrigins;
    [SerializeField]
    [Range(2, 20)]
    int _totalHoriRays = 8, _totalVertRays = 4;

    /// <summary> Distance between rays for left and right collisions </summary>
    float _vertDistanceBetweenRays;
    /// <summary> Distance between rays for above and below collisions </summary>
    float _horiDistanceBetweenRays;
    float _maxRaycastDistanceY, _maxRaycastDistanceX;

    List<RaycastHit2D> _raycastHitsThisFrame = new List<RaycastHit2D>();

    // Position Fields
    Rigidbody2D _rigidbody2D;
    BoxCollider2D _collider2D;
    Vector2 _prevPosition;
    Vector2 _currPosition;
    Vector2 _deltaMovement;

    // Velocity Limiters
    float _minMoveDistance = 0.01f;

    // Properties
    public bool useGravity { get { return _useGravity; } set { _useGravity = value; } }
    public float gravityMultiplier { get { return _gravityMultiplier; } set { _gravityMultiplier = value; } }
    
    public bool isGrounded { get { return _collisionState.below && _collisionState.groundPlatform; } }
    public Vector2 velocity { get { return _currentVelocity; } }
    public CollisionState collisions { get { return _collisionState; } }

    public float skinWidth
    {
        get { return _skinWidth; }
        set
        {
            _skinWidth = value;
            RecalculateRaycastDistances();
        }
    }

    const float k_skinWidthFudgeFactor = 0.001f;

    #endregion

    #region Monobehaviour

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _collider2D = GetComponent<BoxCollider2D>();

        _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        _rigidbody2D.gravityScale = 0;
        _rigidbody2D.useFullKinematicContacts = true;

        skinWidth = _skinWidth;

        _prevPosition = _rigidbody2D.position;
        _currPosition = _rigidbody2D.position;

        ResetRaycastOrigins();
    }

    private void FixedUpdate()
    {
        _collisionState.Reset();
        _raycastHitsThisFrame.Clear();
        _onSlope = false;

        ResetRaycastOrigins();

        Gravity();

        _prevPosition = _rigidbody2D.position;
        _currPosition = _prevPosition + (_projectedVelocity * Time.deltaTime);
        _deltaMovement = _currPosition - _prevPosition;
        
        AdjustVertical();
        
        AdjustHorizontal();

        AdjustForSlope();

        _currPosition = _prevPosition + _deltaMovement;
        _rigidbody2D.MovePosition(_currPosition);

        if (Time.deltaTime > 0)
            _currentVelocity = _deltaMovement / Time.deltaTime;

        if (isGrounded || _collisionState.hitCeiling)
        {
            _projectedVelocity.y = 0;
        }

        if (!_collisionState.groundedLastFrame && _collisionState.below)
            _collisionState.groundedThisFrame = true;
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

    public void MoveRigidbody(Vector2 move)
    {
        _projectedVelocity += move * Time.deltaTime;
    }

    /// <summary>
    /// Should be called anytime the BoxCollider2D is modified
    /// </summary>
    public void RecalculateRaycastDistances()
    {
        float useableHeight = _collider2D.bounds.size.y * Mathf.Abs(transform.localScale.y) - (2f * _skinWidth);
        _vertDistanceBetweenRays = useableHeight / (_totalHoriRays - 1);

        float useableWidth = _collider2D.size.x * Mathf.Abs(transform.localScale.x) - (2f * _skinWidth);
        _horiDistanceBetweenRays = useableWidth / (_totalVertRays - 1);

        _maxRaycastDistanceY = _collider2D.bounds.extents.y * 0.05f;
        _maxRaycastDistanceX = _collider2D.bounds.extents.x * 0.05f;
    }

    #endregion

    #region Collision
    
    void ResetRaycastOrigins()
    {
        Bounds modifiedBounds = _collider2D.bounds;
        modifiedBounds.Expand(-2f * _skinWidth);

        _rayOrigins.topLeft = new Vector2(modifiedBounds.min.x + _skinWidth, modifiedBounds.max.y - _skinWidth);
        _rayOrigins.bottomLeft = new Vector2(modifiedBounds.min.x + _skinWidth, modifiedBounds.min.y + _skinWidth);
        _rayOrigins.bottomRight = new Vector2(modifiedBounds.max.x - _skinWidth, modifiedBounds.min.y + _skinWidth);
    }

    #endregion

    #region Movement

    void AdjustVertical()
    {
        bool goingUp = _deltaMovement.y > 0;

        Vector2 raycastStart, raycastDirection;
        float raycastDistance;
        RaycastHit2D raycastHit;

        // Below Check
        raycastStart = _rayOrigins.bottomLeft;

        raycastDirection = Vector2.down;

        raycastDistance = Mathf.Abs(_deltaMovement.y - _skinWidth);
        if (raycastDistance < _skinWidth) raycastDistance = _skinWidth;
        //else if (raycastDistance > _maxRaycastDistanceY) raycastDistance = _maxRaycastDistanceY;

        for (int i = 0; i < _totalVertRays; i++)
        {
            if (i != 0) raycastStart.x += _horiDistanceBetweenRays;

            DrawRay(raycastStart, raycastDirection * raycastDistance, vertRayColor);
            raycastHit = Physics2D.Raycast(raycastStart, raycastDirection, raycastDistance, _collisionMask);
            if (raycastHit)
            {
                _collisionState.below = true;

                Platform currPlatform = raycastHit.collider.gameObject.GetComponent<Platform>();
                if (currPlatform)
                {
                    if (Mathf.Abs(currPlatform.slopeAngle) <= _slopeLimit)
                    {
                        _collisionState.groundPlatform = currPlatform;

                        if (Mathf.Abs(currPlatform.slopeAngle) > 0)
                            _onSlope = true;
                    }
                    else
                    {
                        _collisionState.below = false;
                    }
                }
                else
                {
                    _collisionState.below = false;
                }

                if (_collisionState.below)
                {
                    _deltaMovement.y = raycastHit.point.y - raycastStart.y;
                    raycastDistance = Mathf.Abs(_deltaMovement.y);

                    _deltaMovement.y += _skinWidth;
                }

                _raycastHitsThisFrame.Add(raycastHit);

                // Deals with the top of slopes
                if (!goingUp && _deltaMovement.y > 0.00001f)
                    _goingUpSlope = true;

                // If true : direct impact, so bail
                if (raycastDistance < _skinWidth + k_skinWidthFudgeFactor)
                    break;
            }
        }

        // Above Check
        raycastStart = _rayOrigins.topLeft;

        raycastDirection = Vector2.up;

        raycastDistance = _skinWidth + _deltaMovement.y;
        if (raycastDistance < _skinWidth) raycastDistance = _skinWidth;
        //else if (raycastDistance > _maxRaycastDistanceY) raycastDistance = _maxRaycastDistanceY;

        for (int i = 0; i < _totalVertRays; i++)
        {
            if (i != 0) raycastStart.x += _horiDistanceBetweenRays;

            DrawRay(raycastStart, raycastDirection * raycastDistance, vertRayColor);
            raycastHit = Physics2D.Raycast(raycastStart, raycastDirection, raycastDistance, _collisionMask);
            if (raycastHit)
            {
                float currAngle = Vector2.Angle(raycastHit.normal, Vector2.down);
                _collisionState.above = true;

                Platform currPlatform = raycastHit.collider.gameObject.GetComponent<Platform>();
                if (currPlatform)
                {
                    if (Mathf.Abs(currPlatform.slopeAngle) <= _slopeLimit)
                    {
                        _collisionState.hitCeiling = true;
                    }
                    else
                    {
                        _collisionState.above = false;
                    }
                }
                else
                {
                    _collisionState.above = false;
                }

                if (_collisionState.above)
                {
                    _deltaMovement.y = raycastHit.point.y - raycastStart.y;
                    raycastDistance = Mathf.Abs(_deltaMovement.y);

                    _deltaMovement.y -= _skinWidth;
                }

                _raycastHitsThisFrame.Add(raycastHit);

                if (_collisionState.above && _collisionState.below)
                {
                    _deltaMovement.y = 0;
                    break;
                }

                // If true : direct impact, so bail
                if (raycastDistance < _skinWidth + k_skinWidthFudgeFactor)
                    break;
            }
        }
    }

    void AdjustHorizontal()
    {
        bool goingRight = _deltaMovement.x > 0;

        Vector2 raycastStart, raycastDirection;
        float raycastDistance;
        RaycastHit2D raycastHit;

        // Right Check
        raycastStart = _rayOrigins.bottomRight;
        raycastStart.y += _deltaMovement.y;

        raycastDirection = Vector2.right;

        raycastDistance = _skinWidth + _deltaMovement.x;
        if (raycastDistance < _skinWidth) raycastDistance = _skinWidth;
        else if (raycastDistance > _maxRaycastDistanceX) raycastDistance = _maxRaycastDistanceX;

        for (int i = 0; i < _totalHoriRays; i++)
        {
            if (i != 0) raycastStart.y += _vertDistanceBetweenRays;

            DrawRay(raycastStart, raycastDirection * raycastDistance, horiRayColor);
            raycastHit = Physics2D.Raycast(raycastStart, raycastDirection, raycastDistance, _collisionMask);
            if (raycastHit)
            {
                float currAngle = Vector2.Angle(raycastHit.normal, Vector2.left);
                _collisionState.right = true;

                if (currAngle <= _slopeLimit)
                {
                    _collisionState.right = false;
                }

                /*
                Platform potentialWall = raycastHit.collider.gameObject.GetComponent<Platform>();
                if (potentialWall)
                {
                    if (Mathf.Abs(potentialWall.slopeAngle) <= _slopeLimit)
                        _collisionState.right = false;
                    else
                        currAngle = potentialWall.slopeAngle;
                }
                else
                {
                    _collisionState.right = false;
                }
                */

                if (_collisionState.right)
                {
                    if (currAngle > 0)
                    {
                        // TOA : Y displacement is adjacent, X is opposite
                        // Use of TOA to find the x displacement we need 
                        _deltaMovement.x = _deltaMovement.y * Mathf.Tan(currAngle * Mathf.Deg2Rad) * 1.4f;
                    }
                    else
                    {
                        _deltaMovement.x = raycastHit.point.x - raycastStart.x;
                    }
                    
                    raycastDistance = Mathf.Abs(_deltaMovement.x);
                    _deltaMovement.x -= _skinWidth;
                }

                _raycastHitsThisFrame.Add(raycastHit);

                // If true : direct impact, so bail
                if (raycastDistance < _skinWidth + k_skinWidthFudgeFactor)
                    break;
            }
        }

        // Left Check
        raycastStart = _rayOrigins.bottomLeft;
        raycastStart.y += _deltaMovement.y;

        raycastDirection = Vector2.left;

        raycastDistance = Mathf.Abs(_deltaMovement.x - _skinWidth);
        if (raycastDistance < _skinWidth) raycastDistance = _skinWidth;
        else if (raycastDistance > _maxRaycastDistanceX) raycastDistance = _maxRaycastDistanceX;

        for (int i = 0; i < _totalHoriRays; i++)
        {
            if (i != 0) raycastStart.y += _vertDistanceBetweenRays;

            DrawRay(raycastStart, raycastDirection * raycastDistance, horiRayColor);
            raycastHit = Physics2D.Raycast(raycastStart, raycastDirection, raycastDistance, _collisionMask);
            if (raycastHit)
            {
                float currAngle = Vector2.Angle(raycastHit.normal, Vector2.right);
                _collisionState.left = true;

                if (currAngle <= _slopeLimit)
                {
                    _collisionState.left = false;
                }

                /*
                Platform potentialWall = raycastHit.collider.gameObject.GetComponent<Platform>();
                if (potentialWall)
                {
                    if (Mathf.Abs(potentialWall.slopeAngle) <= _slopeLimit)
                        _collisionState.left = false;
                    else
                        currAngle = potentialWall.slopeAngle;
                }
                else
                {
                    _collisionState.left = false;
                }
                */

                if (_collisionState.left)
                {
                    if (currAngle < 0)
                    {
                        _deltaMovement.x = _deltaMovement.y * Mathf.Tan(currAngle * Mathf.Deg2Rad) * 1.4f;
                    }
                    else
                    {
                        _deltaMovement.x = raycastHit.point.x - raycastStart.x;
                    }

                    raycastDistance = _deltaMovement.x;
                    _deltaMovement.x += _skinWidth;
                }

                _raycastHitsThisFrame.Add(raycastHit);

                if (_collisionState.left && _collisionState.right)
                {
                    _deltaMovement.x = 0;
                    break;
                }
            }
        }
    }

    void AdjustForSlope()
    {
        if (!_onSlope) return;

        Vector2 raycastStart, raycastDirection;
        float raycastDistance;
        RaycastHit2D raycastHit;
        
        // We want to check for slopes from the centre of the collider
        raycastStart = new Vector2((_rayOrigins.bottomLeft.x + _rayOrigins.bottomRight.x) * 0.5f, _rayOrigins.bottomLeft.y);
        raycastDirection = Vector2.down;
        // Use _SlopeLimitTangent to calulate raycastDistance
        raycastDistance = .2f;

        DrawRay(raycastStart, raycastDistance * raycastDirection, slopeRayColor);
        raycastHit = Physics2D.Raycast(raycastStart, raycastDirection, raycastDistance, _collisionMask);
        if (raycastHit)
        {
            // If no slope bail
            float angle = Vector2.Angle(raycastHit.normal, Vector2.up);
            if (angle == 0)
                return;

            // We are moving down if the slope's normal and _deltaMovement are in the same X direction
            bool movingDownSlope = Mathf.Sign(raycastHit.normal.x) == Mathf.Sign(_deltaMovement.x);
            if (movingDownSlope)
            {
                print("Going down");
                // Going down we want to speed up so the _slopeSpeedModifier curve should be > 1 for negative angles
                float speedModifier = _slopeSpeedModifier.Evaluate(-angle);
                // Add extra downward movement here to ensure we stick to the platform
                _deltaMovement.x *= speedModifier;

               // if (raycastHit.distance <= _skinWidth)
                    _deltaMovement.y -= raycastStart.y - raycastHit.point.y - _skinWidth;
                _deltaMovement.y += Mathf.Tan(angle * Mathf.Deg2Rad) * _deltaMovement.x;
            }
            else
            {
                print("Going up");
                // Going down we want to slow down so the _slopeSpeedModifier curve should be < 1 for positive values
                float speedModifier = _slopeSpeedModifier.Evaluate(angle);
                // Add extra movement to ensure we stick
                _deltaMovement.x *= speedModifier;
                _deltaMovement.y -= Mathf.Tan(angle * Mathf.Deg2Rad) * _deltaMovement.x;
            }
        }
    }

    void Gravity()
    {
        if (!_useGravity) return;
        if (_collisionState.below) return;

        Vector2 gravity = Physics2D.gravity * _gravityMultiplier;
        MoveRigidbody(gravity);
    }

    #endregion

    #region DEBUG
#if COLLISION_RAYS

    Color vertRayColor = Color.red, horiRayColor = Color.red, slopeRayColor = Color.yellow;

    void DrawRay(Vector2 start, Vector2 dir, Color color)
    {
        Debug.DrawRay(start, dir, color);
    }

#endif
    #endregion
}

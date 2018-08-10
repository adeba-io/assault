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

        public bool groundedLastFrame, groundedThisFrame;
        public Platform groundPlatform;

        public float groundSlopeAngle { get { return groundPlatform.slopeAngle; } }
        public bool hasCollision { get { return above || below || left || right; } }

        public void Reset()
        {
            groundedLastFrame = below;
            above = below = left = right = groundedThisFrame = false;
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
    [SerializeField] Vector2 _velocity;
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
    [SerializeField]
    AnimationCurve _slopeSpeedModifier =
        new AnimationCurve(new Keyframe(-90f, 1.5f), new Keyframe(0f, 1f), new Keyframe(90f, 0f));
    bool _onSlope = false;

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
    public Vector2 velocity { get { return _velocity; } }
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

        ResetRaycastOrigins();

        Gravity();

        _prevPosition = _rigidbody2D.position;
        _currPosition = _prevPosition + (_velocity * Time.deltaTime);
        _deltaMovement = _currPosition - _prevPosition;

        if (_deltaMovement.x != 0)
            MoveHorizontal();

        if (_deltaMovement.y != 0)
            MoveVertical();
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

    void MoveHorizontal()
    {

    }

    void MoveVertical()
    {
        bool goingUp = _deltaMovement.y > 0;

        Vector2 raycastStart, raycastDirection;
        float raycastDistanceTop, raycastDistanceBottom;
        float raycastDistance;
        RaycastHit2D raycastHit;

        // Initial Distance Setup
        raycastDistanceTop = _skinWidth + _deltaMovement.y;
        raycastDistanceBottom = Mathf.Abs(_deltaMovement.y - _skinWidth);
        //Adjust for max and min
        if (raycastDistanceTop < _skinWidth) raycastDistanceTop = _skinWidth;
        else if (raycastDistanceTop > _maxRaycastDistanceY) raycastDistanceTop = _maxRaycastDistanceY;

        if (raycastDistanceBottom < _skinWidth) raycastDistanceBottom = _skinWidth;
        else if (raycastDistanceBottom > _maxRaycastDistanceY) raycastDistanceBottom = _skinWidth;
        
        // Initial raycastStart Setup
        Vector2 initialRayOriginTop = _rayOrigins.topLeft, initialRayOriginBottom = _rayOrigins.bottomLeft;

        initialRayOriginTop.y += _skinWidth - raycastDistanceTop;
        initialRayOriginBottom.y -= _skinWidth + raycastDistanceBottom;

        // Add _deltaMovement.x as it has already been adjusted
        initialRayOriginTop.x += _deltaMovement.x;
        initialRayOriginBottom.x += _deltaMovement.x;

        int top = 0, bottom = 0;
        for (int i = 0; i < _totalVertRays * 2; i++)
        {
            bool checkBelow = i % 2 == 0;
            
            if (checkBelow)
            {
                // Checking Bottom
                raycastStart = new Vector2(initialRayOriginBottom.x + (bottom * _horiDistanceBetweenRays), initialRayOriginBottom.y);
                raycastDirection = Vector2.down;
                raycastDistance = Mathf.Abs(_deltaMovement.y - _skinWidth);
                bottom++;
            }
            else
            {
                // Checking Above
                raycastStart = new Vector2(initialRayOriginTop.x + (top * _horiDistanceBetweenRays), initialRayOriginTop.y);
                raycastDirection = Vector2.up;
                raycastDistance = _skinWidth + _deltaMovement.y;
                top++;
            }

            if (raycastDistance < _skinWidth) raycastDistance = _skinWidth;
            else if (raycastDistance > _maxRaycastDistanceY) raycastDistance = _maxRaycastDistanceY;

            DrawRay(raycastStart, raycastDirection * raycastDistance, vertRayColor);

            raycastHit = Physics2D.Raycast(raycastStart, raycastDirection, raycastDistance, _collisionMask);
            if (raycastHit)
            {
                float hitDist = raycastHit.distance;

                if (checkBelow)
                {
                    _deltaMovement.y += hitDist;
                    
                }
                else
                {
                    _deltaMovement.y -= hitDist;
                }
            }
        }
    }

    void Gravity()
    {
        if (!_useGravity) return;
        if (_collisionState.below) return;

        _velocity += Physics2D.gravity * _gravityMultiplier;
    }

    #endregion

    #region DEBUG
#if COLLISION_RAYS

    Color vertRayColor = Color.blue, horiRayColor = Color.red, slopeRayColor = Color.yellow;

    void DrawRay(Vector2 start, Vector2 dir, Color color)
    {
        Debug.DrawRay(start, dir, color);
    }

#endif
    #endregion
}

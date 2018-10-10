using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Types;
using Assault.Maneuvers;

namespace Assault
{
    [RequireComponent(typeof(FighterPhysics), typeof(FighterInput))]
    public class FighterController : MonoBehaviour
    {
        const FighterState GROUNDED_IN_CONTROL = FighterState.Standing | FighterState.Crouching | FighterState.Running | FighterState.Dashing;
        const FighterState GROUNDED_HITTABLE = GROUNDED_IN_CONTROL | FighterState.Fallen | FighterState.JumpSquat;

        const FighterState AERIAL = FighterState.Aerial | FighterState.Tumble;

        #region Animation Variables

        // Triggers
        readonly int anim_JUMP = Animator.StringToHash("Jump");
        readonly int anim_AIRJUMP = Animator.StringToHash("AirJump");
        readonly int anim_DASH = Animator.StringToHash("Dash");
        readonly int anim_TURNAROUND = Animator.StringToHash("Turnaround");
        readonly int anim_WALLJUMP = Animator.StringToHash("WallJump");

        // Booleans
        readonly int anim_GROUNDED = Animator.StringToHash("isGrounded");
        readonly int anim_CROUCHING = Animator.StringToHash("Crouching");
        readonly int anim_SKIDDING = Animator.StringToHash("Skidding");

        // Integers
        readonly int anim_GROUNDMOVEMENT = Animator.StringToHash("GroundMove"); // 0 = Standing, 1 = Walking, 2 = Running

        #endregion

        public Maneuver currentManeuver;

        public FighterState currentState;
        public bool facingRight { get; protected set; }

        public Vector2 nextForce;
        public Vector2 currentAccelerate;

        [SerializeField] float _walkAcceleration = 4f, _maxWalkSpeed = 2f;

        [SerializeField] float _runAcceleration = 6f, _maxRunSpeed = 7f;
        [SerializeField] float _maxSlowRunSpeed = 4f;

        [SerializeField] float _airAcceleration = 6f, _maxAirSpeed = 4f;

        [SerializeField] float _jumpForce = 8f;
        [SerializeField] float _aerialJumpForce = 6f;
        [SerializeField] int _maxAirJumps = 3;
        int _airJumpsLeft;

        [SerializeField] float _dashForce = 5f;

        [SerializeField] Vector2 _wallJumpForce = new Vector2(3f, 2f);

        bool _cancelToDash = true;
        bool canCancel = true;
        bool _canFastFall = false;
        bool _cancelToRun = false;

        InputCombo _currentCombo;

        FighterPhysics _physics;

        Animator _animator;
        SpriteRenderer _majorRenderer;

        private void Start()
        {
            currentState = FighterState.Standing;

            _physics = GetComponent<FighterPhysics>();
            _animator = GetComponent<Animator>();

            _animator.SetBool(anim_GROUNDED, true);

            _airJumpsLeft = _maxAirJumps;
            _wallJumpForce = new Vector2(Mathf.Abs(_wallJumpForce.x), Mathf.Abs(_wallJumpForce.y));
        }

        private void Update()
        {
            UpdateState();
        }

        public void UpdateManeuver()
        {

        }

        void UpdateState()
        {
            if (_physics.isGrounded)
            {
                _canFastFall = false;
                _animator.SetBool(anim_GROUNDED, true);

                if (_animator.GetBool(anim_SKIDDING) && Mathf.Abs(_physics.internalVelocity.x) < 1f)
                {
                    _animator.SetBool(anim_SKIDDING, false);
                    _animator.SetInteger(anim_GROUNDMOVEMENT, 0);
                }

                _airJumpsLeft = _maxAirJumps;
            }
            else if (!_physics.isGrounded)
            {
                _animator.SetBool(anim_GROUNDED, false);
            }
        }

        public bool ReceiveInput(InputCombo inputCombo)
        {
            _currentCombo = inputCombo;
           // print(inputCombo);

            switch (currentState)
            {
                case FighterState.Standing:

                    if (JumpSquat()) return true;
                    if (Dash()) return true;
                    if (WalkInit()) return true;

                    Stand();

                    return false;
                case FighterState.Crouching:

                    if (JumpSquat()) return true;

                    Stand();

                    return false;
                case FighterState.Walking:
                    
                    if (JumpSquat()) return true;
                    if (Dash()) return true;
                    if (Walk()) return true;
                    if (Turnaround()) return true;

                    Stand();

                    return false;
                case FighterState.Running:
                    
                    if (JumpSquat()) return true;
                    if (Run()) return true;
                    if (Turnaround()) return true;

                    Skid();

                    return false;
                case FighterState.Dashing:

                    if (JumpSquat()) return true;
                    if (Dash()) return true;
                    if (RunInit()) return true;

                    return false;
                case FighterState.Skidding:

                    if (JumpSquat()) return true;
                    //if (Dash()) return true;

                    Turnaround();

                    return false;
                case FighterState.JumpSquat:
                    
                    return false;
                case FighterState.Jumping:

                    AirDrift();

                    if (AirJumpSquat()) return true;
                    if (WallJumpSquat()) return true;

                    return false;
                case FighterState.Aerial:

                    AirDrift();
                    FastFall();

                    if (AirJumpSquat()) return true;
                    if (WallJumpSquat()) return true;

                    return false;
                case FighterState.Tumble:
                    
                    AirDrift();

                    if (AirJumpSquat()) return true;

                    return false;
            }

            return false;
        }

        void UseGravity() { _physics.useGravity = true; }
        void NoGravity() { _physics.useGravity = false; }
        
        public void SetCanDash(bool canDash) { _cancelToDash = canDash; }
        void CancelToRun() { _cancelToRun = true; }
        
        public void SetState(FighterState newState)
        {
            currentState = newState;

            if (newState == FighterState.Standing || newState == FighterState.Crouching)
            {
                _physics.ResetRigidbody(this);
            }
        }

        public void CanFastFall() { _canFastFall = true; }

        #region Jump

        void Jump()
        {
            currentState = FighterState.Jumping;

            float jump = _physics.isGrounded ? _jumpForce : _aerialJumpForce;

            if (_physics.internalVelocity.x > 0 && _currentCombo == HorizontalControlGeneral.Left)
                _physics.ResetRigidbody(this, resetY: false);

            _physics.ForceRigidbody(this, new Vector2(0, jump), resetY: true);
        }

        void WallJump()
        {
            currentState = FighterState.Jumping;

            _animator.ResetTrigger(anim_WALLJUMP);
            Vector2 jump = facingRight ? _wallJumpForce : new Vector2(-_wallJumpForce.x, _wallJumpForce.y);
            _physics.ForceRigidbody(this, jump, resetX: true, resetY: true);
        }


        bool JumpSquat()
        {
            if (_currentCombo != Button.Jump) return false;
            if (_currentCombo != ButtonManeuver.Down) return false;
            
            _animator.SetTrigger(anim_JUMP);

            return false;
        }

        bool AirJumpSquat()
        {
            if (_airJumpsLeft <= 0) return false;
            if (_currentCombo != Button.Jump) return false;
            if (_currentCombo != ButtonManeuver.Down) return false;

            _animator.SetTrigger(anim_AIRJUMP);
            _airJumpsLeft--;

            return true;
        }

        bool WallJumpSquat()
        {
            if (!_physics.collisionState.touchingWall) return false;
            if (_physics.collisionState.leftPlatform && _physics.collisionState.rightPlatform) return false;
            if (_currentCombo == HorizontalControl.NEUTRAL) return false;
            print("Wall JS " + Time.timeSinceLevelLoad);
            _physics.ResetRigidbody(this);

            if (_physics.collisionState.rightPlatform && _currentCombo == HorizontalControlGeneral.Left)
            {
                SetFacingDirection(false); // Now facing left

                _animator.SetTrigger(anim_WALLJUMP);
                return true;
            }
            else if (_physics.collisionState.leftPlatform && _currentCombo == HorizontalControlGeneral.Right)
            {
                SetFacingDirection(true); // Now facing left

                _animator.SetTrigger(anim_WALLJUMP);
                return true;
            }

            return false;
        }

        #endregion

        void FastFall()
        {
            if (!_canFastFall) return;
            if (_currentCombo != VerticalControl.Down) return;

            _physics.fastFall = true;
        }

        bool Dash()
        {
            if (!_cancelToDash) return false;
            if (_currentCombo == HorizontalControl.NEUTRAL) return false;
            if (_currentCombo != ControlManeuver.Snap) return false;

            Vector2 dash = Vector2.zero;

            if (_currentCombo == HorizontalControl.Back)
                Flip();

            dash.x = facingRight ? 1 : -1;
            dash.x *= _dashForce;

            _physics.ForceRigidbody(this, dash, true);
            
            _animator.SetBool(anim_SKIDDING, false);
            _animator.SetTrigger(anim_DASH);

            return true;
        }

        #region Run

        bool RunInit()
        {
            if (_currentCombo == HorizontalControl.NEUTRAL) return false;
            if (_currentCombo == ControlManeuver.Snap) return false;
            if (_currentCombo == ControlManeuver.Soft) return false;
           // if (!_cancelToRun) return false;
            
            Vector2 run = Vector2.zero;

            run.x = facingRight ? 1 : -1;
            run.x *= _runAcceleration;

            _physics.AccelerateRigidbody(this, run, maxVelocityX: _maxRunSpeed);


            _animator.SetBool(anim_SKIDDING, false);
            _animator.SetInteger(anim_GROUNDMOVEMENT, 2);
            _cancelToRun = false;

            return true;
        }

        bool Run()
        {
            if (_currentCombo != HorizontalControl.Forward) return false;
            if (_currentCombo == ControlManeuver.Snap) return false;

            Vector2 run = Vector2.zero;

            run.x = facingRight ? 1 : -1;
            run.x *= _runAcceleration;

            if (_currentCombo == ControlManeuver.Soft) run.x *= 0.5f;

            _physics.AccelerateRigidbody(this, run, maxVelocityX: _maxRunSpeed);

            return true;
        }

        #endregion

        #region Walk

        bool WalkInit()
        {
            if (_currentCombo == HorizontalControl.NEUTRAL) return false;
            if (_currentCombo != ControlManeuver.Soft) return false;
            
            Vector2 walk = Vector2.zero;

            if (_currentCombo == HorizontalControl.Back)
                Flip();

            walk.x = facingRight ? 1 : -1;
            walk.x *= _walkAcceleration;

            _physics.AccelerateRigidbody(this, walk, maxVelocityX: _maxWalkSpeed);

            _animator.SetInteger(anim_GROUNDMOVEMENT, 1);

            return true;
        }

        bool Walk()
        {
            if (_currentCombo != HorizontalControl.Forward) return false;
            
            Vector2 walk = Vector2.zero;

            walk.x = facingRight ? 1 : -1;
            walk.x *= _walkAcceleration;

            if (_currentCombo == ControlManeuver.Hard) walk.x *= 1.3f;

            _physics.AccelerateRigidbody(this, walk, maxVelocityX: _maxWalkSpeed);

            return true;
        }

        #endregion

        void Skid()
        {
            if (_currentCombo != HorizontalControl.NEUTRAL) return;
            
            _animator.SetInteger(anim_GROUNDMOVEMENT, 0);
            _animator.SetBool(anim_SKIDDING, true);

            return;
        }

        bool Turnaround()
        {
            if (_currentCombo != HorizontalControl.Back) return false;

            Flip();

            _animator.SetBool(anim_SKIDDING, false);
            _animator.SetTrigger(anim_TURNAROUND);

            return true;
        }

        void Stand()
        {
            if (!_physics.isGrounded) return;

            if (_currentCombo == HorizontalControl.NEUTRAL && _currentCombo == VerticalControl.NEUTRAL)
            {
                _animator.SetBool(anim_CROUCHING, false);
                _animator.SetInteger(anim_GROUNDMOVEMENT, 0);
            }
            else if (_currentCombo == VerticalControl.Down && _currentCombo == HorizontalControl.NEUTRAL)
            {
                _animator.SetBool(anim_CROUCHING, true);
                _animator.SetInteger(anim_GROUNDMOVEMENT, 0);
            }
        }

        void AirDrift()
        {
            if (_currentCombo == HorizontalControl.NEUTRAL) return;

            Vector2 drift = Vector2.zero;

            if (_currentCombo == HorizontalControl.Forward)
                drift = facingRight ? Vector2.right : Vector2.left;
            else if (_currentCombo == HorizontalControl.Back)
                drift = (facingRight ? Vector2.left : Vector2.right);

            drift.x *= _airAcceleration;

            if (_currentCombo == ControlManeuver.Soft) drift.x *= 0.7f;

            _physics.AccelerateRigidbody(this, drift, maxVelocityX: _maxAirSpeed);
        }

        public void Flip(bool flipX = true, bool flipY = false)
        {
            _physics.Flip(flipX, flipY);
            
            if (flipX)
            {
                facingRight = !facingRight;
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y);
            }
        }

        public void SetFacingDirection(bool right)
        {
            facingRight = right;

            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * (right ? 1 : -1), transform.localScale.y);
        }
    }
}
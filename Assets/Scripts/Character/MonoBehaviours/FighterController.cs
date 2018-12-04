using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Types;
using Assault.Techniques;
using Assault.Managers;

namespace Assault
{
    [RequireComponent(typeof(FighterPhysics), typeof(FighterInput))]
    [RequireComponent(typeof(Animator))]
    public class FighterController : MonoBehaviour
    {
        #region State Groups
        const FighterState GROUNDED_IN_CONTROL = FighterState.Standing | FighterState.Crouching | FighterState.Running | FighterState.Dashing | FighterState.Walking | FighterState.Skidding;
        const FighterState GROUNDED_HITTABLE = GROUNDED_IN_CONTROL | FighterState.Fallen | FighterState.JumpSquat | FighterState.Landing;

        const FighterState AERIAL = FighterState.Aerial | FighterState.Tumble | FighterState.Jumping | FighterState.AirJumpSquat;
        #endregion

        #region Animation Variables
        // Triggers
        protected readonly int anim_SPAWN = Animator.StringToHash("Spawn");
        protected readonly int anim_CANCEL = Animator.StringToHash("Cancel");

        protected readonly int anim_JUMPSQUAT = Animator.StringToHash("JumpSquat");
        protected readonly int anim_JUMP = Animator.StringToHash("Jump");
        protected readonly int anim_DASH = Animator.StringToHash("Dash");
        protected readonly int anim_BACKDASH = Animator.StringToHash("BackDash");
        protected readonly int anim_TURNAROUND = Animator.StringToHash("Turnaround");
        protected readonly int anim_WALLJUMP = Animator.StringToHash("WallJump");

        protected readonly int anim_EXECUTETECH = Animator.StringToHash("ExecuteTechnique");

        // Booleans
        protected readonly int anim_GROUNDED = Animator.StringToHash("isGrounded");
        protected readonly int anim_CROUCHING = Animator.StringToHash("Crouching");
        protected readonly int anim_SKIDDING = Animator.StringToHash("Skidding");

        // Integers
        protected readonly int anim_GROUNDMOVEMENT = Animator.StringToHash("GroundMove"); // 0 = Standing, 1 = Walking, 2 = Running
        protected readonly int anim_LANDTYPE = Animator.StringToHash("LandType"); // 0 = To Standing, 1 = Soft, 2 = Hard
        protected readonly int anim_TECHNIQUEID = Animator.StringToHash("TechniqueID");

        // Float
        protected readonly int anim_LANDINGMODIFIER = Animator.StringToHash("LandModifier");
        #endregion

        public Technique nextTechnique { get; set; }
        public Technique currentTechnique { get; set; }

        public FighterState currentState { get; protected set; }
        public bool facingRight { get; protected set; }

        public Vector2 nextForce;
        public Vector2 currentAccelerate;

        #region Movement Parameters
        [SerializeField] float _walkAcceleration = 4f, _maxWalkSpeed = 2f;

        [SerializeField] float _runAcceleration = 6f, _maxRunSpeed = 7f;

        [SerializeField] float _dashForce = 5f;

        [SerializeField] float _airAcceleration = 6f, _maxAirSpeed = 4f;
        [SerializeField] float _airDashForce = 4f;
        [SerializeField] int _maxAirDashes = 2;
        [SerializeField] int _airDashesLeft;

        [SerializeField] float _jumpForce = 8f;
        [SerializeField] float _airJumpForceMultiplier = 0.75f;
        [SerializeField] int _maxAirJumps = 3;
        [SerializeField] int _airJumpsLeft;

        [SerializeField] Vector2 _wallJumpForce = new Vector2(3f, 2f);

        float m_walkAcceleration, m_maxWalkSpeed;
        float m_runAcceleration, m_maxRunSpeed;
        float m_dashForce, m_backDashForce;
        float m_airAcceleration, m_maxAirSpeed;
        float m_airDashForce, m_airBackDashForce;
        float m_jumpForce, m_airJumpForce;
        Vector2 m_wallJumpForce;
        #endregion

        #region Moveset
        [SerializeField] InputComboTechniquePair[] _standingTechniques;
        [SerializeField] InputComboTechniquePair[] _runningTechniques;
        [SerializeField] InputComboTechniquePair[] _jumpingTechniques;
        [SerializeField] InputComboTechniquePair[] _aerialTechniques;
        #endregion

        bool _backDashing = false;
        bool _cancelToDash = true;
        bool _canFastFall = false;
        bool _cancelToRun = false;

        InputCombo _currentCombo;

        public Transform spawnPoint;
        public int playerNumber;

        FighterInput Input;
        FighterPhysics _physics;
        Animator m_animator;
        SpriteRenderer _majorRenderer;

        public FighterDamager damager { get; protected set; }
        public FighterDamageable damageable { get; protected set; }

        #region FighterPhysics Events
        void OnLand()
        {
            if (currentState == GROUNDED_HITTABLE) return;
            if (currentState == FighterState.Standing || currentState == FighterState.Crouching ||
                currentState == FighterState.Dashing || currentState == FighterState.Walking ||
                currentState == FighterState.Running || currentState == FighterState.Skidding ||
                currentState == FighterState.JumpSquat) return;

            Debug.Log("Land");

            UseGravity();

            if (currentTechnique)
            {
                if (currentTechnique.type != TechniqueType.Grounded)
                {
                    Technique next = currentTechnique.Land();
                    if (!next)
                    {
                        if (currentTechnique.HardLand())
                        {
                            m_animator.SetInteger(anim_LANDTYPE, 2);

                            float landModifier = currentTechnique.landingLag / 8f;
                            m_animator.SetFloat(anim_LANDINGMODIFIER, landModifier);
                        }
                    }
                }
                else SetNextTechnique(currentTechnique.Land(), true);
            }

            _canFastFall = false;
            _airJumpsLeft = _maxAirJumps;
            _airDashesLeft = _maxAirDashes;
            SetState(FighterState.Standing);
            m_animator.SetBool(anim_GROUNDED, true);
        }

        void OnAerial()
        {
            if (currentState == AERIAL) return;

            print("Aerial");
            m_animator.SetBool(anim_GROUNDED, false);
            m_animator.SetInteger(anim_LANDTYPE, 1);

            if (currentState == GROUNDED_IN_CONTROL)
                SetState(FighterState.Aerial);
        }

        void OnHitCeiling()
        {

        }

        void OnHitLeftWall()
        {

        }

        void OnHitRightWall()
        {

        }
        #endregion

        #region Init
        private void Start()
        {
            currentState = FighterState.Standing;
            facingRight = true;
            
            Input = GetComponent<FighterInput>();
            _physics = GetComponent<FighterPhysics>();
            m_animator = GetComponent<Animator>();
            damager = GetComponent<FighterDamager>();
            damageable = GetComponent<FighterDamageable>();

            _physics.OnGroundedEvent = OnLand;
            _physics.OnAerialEvent = OnAerial;
            _physics.OnHitCeilingEvent = OnHitCeiling;
            _physics.OnHitLeftWallEvent = OnHitLeftWall;
            _physics.OnHitRightWallEvent = OnHitRightWall;
            
            m_animator.SetBool(anim_GROUNDED, _physics.isGrounded);
            StateMachines.SceneLinkedSMB<FighterController>.Initialize(m_animator, this);

            _airJumpsLeft = _maxAirJumps;
            _wallJumpForce = new Vector2(Mathf.Abs(_wallJumpForce.x), Mathf.Abs(_wallJumpForce.y));

            if (spawnPoint)
            {
                transform.position = spawnPoint.position;
                m_animator.SetTrigger(anim_SPAWN);
            }

            if (playerNumber != 0)
                SetPlayerNumber(playerNumber);

            CalculateMovementParameters();
        }

        public void Spawn()
        {

        }

        public void SetPlayerNumber(int playerNumber) { Input.SetPlayerNumber(playerNumber); }

        public void CalculateMovementParameters()
        {
            m_walkAcceleration = _walkAcceleration * 10f;
            m_maxWalkSpeed = _maxWalkSpeed * 2f;
            m_runAcceleration = _runAcceleration * 12f;
            m_maxRunSpeed = _maxRunSpeed * 2f;

            m_dashForce = _dashForce * 5f;
            m_backDashForce = m_dashForce * FighterManager.FM.backDashSpeedMultiplier;

            m_airAcceleration = _airAcceleration * 5f;
            m_maxAirSpeed = _maxAirSpeed * 4f;

            m_airDashForce = _airDashForce * 5f;
            m_airBackDashForce = m_airDashForce * FighterManager.FM.backDashSpeedMultiplier;

            m_jumpForce = _jumpForce * 5f;
            m_airJumpForce = m_jumpForce * _airJumpForceMultiplier;
        }
        #endregion

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.GetComponent<Boxes.BlastZone>())
                Destroy(gameObject);
        }

        #region State Manipulation
        public void FindNewState()
        {
            if (_physics.isGrounded)
            {
                SetState(FighterState.Standing);
            }
            else
            {
                SetState(FighterState.Aerial);
            }
        }

        public void SetState(FighterState newState)
        {
            if (currentState == newState) return;

            if (currentState == FighterState.Dashing && newState != FighterState.Dashing)
            {
                _backDashing = false;
            }

            if (newState == FighterState.Standing || newState == FighterState.Crouching)
            {
                _physics.ResetRigidbody(this);
            }

            if (newState == FighterState.Standing || newState == FighterState.Aerial)
            {
                ResetParameters();
                UseGravity();
            }

            currentState = newState;
        }
        #endregion

        #region Animation Methods
        void UseGravity() { _physics.useGravity = true; }
        void NoGravity() { _physics.useGravity = false; }

        void ResetParameters()
        {
            m_animator.ResetTrigger(anim_CANCEL);
            m_animator.ResetTrigger(anim_JUMP);
            m_animator.SetBool(anim_CROUCHING, false);
            m_animator.SetInteger(anim_LANDTYPE, 1);
            m_animator.SetFloat(anim_LANDINGMODIFIER, 1.0f);
        }

        void AnimFlip() { Flip(true, false, true); }

        void EndGroundDash()
        {
            _physics.DecelerateToMax(this, Vector2.zero, Vector2.zero);
        }

        void EndAirDash()
        {
            _physics.DecelerateToMax(this, maxVelocityX: m_maxAirSpeed, decelerationX: 20f);
        }

        public void SetCanDash(bool canDash) { _cancelToDash = canDash; }
        void CancelToRun() { _cancelToRun = true; }

        public void CanFastFall() { _canFastFall = true; }
        #endregion

        #region Technique Methods

        bool CheckForInputCombo(InputComboTechniquePair[] iCTPairs)
        {
            for (int i = 0; i < iCTPairs.Length; i++)
            {
                if (iCTPairs[i].inputCombo == InputCombo.none)
                    continue;

                if (_currentCombo == iCTPairs[i].inputCombo)
                {
                    SetNextTechnique(iCTPairs[i].technique);

                    return true;
                }
            }

            return false;
        }

        void SetNextTechnique(Technique technique, bool land = false)
        {
            if (!technique)
            {
                Debug.LogWarning("Tried to set non existant Technique");
                return;
            }

            nextTechnique = technique;

            if (!land)
            {
                m_animator.SetInteger(anim_TECHNIQUEID, nextTechnique.animationID);
                m_animator.SetTrigger(anim_EXECUTETECH);
            }
            
        }

        public void InitializeNextTechnique()
        {
            Debug.Log("Initializing");
            if (!nextTechnique)
            {
                Debug.LogWarning("Attempted to Initialize non existent technique");
                return;
            }

            if (currentTechnique)
                currentTechnique.End();

            currentTechnique = nextTechnique;
            
            currentTechnique.Initialize(this);
            SetState(FighterState.MidTechnique);
        }

        public void UpdateTechnique()
        {
            Debug.Log("Updating");
            if (!currentTechnique)
            {
                Debug.LogWarning("Attempted to Update non existent technique");
                return;
            }

            currentTechnique.Update();
        }

        public void EndTechnique()
        {
            FindNewState();

            if (!currentTechnique)
            {
                Debug.LogWarning("Attempted to End non existent technique");
                return;
            }

            currentTechnique.End();
            currentTechnique = null;
        }


        #endregion

        #region Input Receiving

        public bool ReceiveInput(InputCombo inputCombo)
        {
            _currentCombo = inputCombo;

            switch (currentState)
            {
                case FighterState.Standing:

                    if (CheckForInputCombo(_standingTechniques)) return true;

                    if (JumpSquat()) return true;
                    if (DashLaunch()) return true;
                    if (WalkInit()) return true;
                    if (PhaseThrough()) return true;

                    Stand();

                    break;
                case FighterState.Crouching:

                    if (CheckForInputCombo(_standingTechniques)) return true;

                    if (JumpSquat()) return true;
                    if (PhaseThrough()) return true;

                    Stand();

                    break;
                case FighterState.Walking:

                    if (CheckForInputCombo(_standingTechniques)) return true;

                    if (JumpSquat()) return true;
                    if (DashLaunch()) return true;
                    if (PhaseThrough()) return true;

                    if (Walk()) return true;
                    if (Turnaround()) return true;

                    Stand();

                    break;
                case FighterState.Running:

                    if (CheckForInputCombo(_runningTechniques)) return true;

                    if (JumpSquat()) return true;
                    if (PhaseThrough()) return true;
                     
                    if (Run()) return true;
                  //  if (Turnaround()) return true;

                    Skid();

                    break;
                case FighterState.Dashing:

                    RunReset();

                    if (CheckForInputCombo(_runningTechniques)) return true;

                    if (JumpSquat()) return true;
                    if (DashLaunch()) return true;
                    if (RunInit()) return true;

                    break;
                case FighterState.Skidding:

                    if (JumpSquat()) return true;
                    if (DashLaunch()) return true;

                    Stand();

                    break;
                case FighterState.JumpSquat:

                    if (CheckForInputCombo(_jumpingTechniques)) return true;

                    JumpSquatTurnaround();

                    break;
                case FighterState.Jumping:

                    AirDrift();

                    if (CheckForInputCombo(_jumpingTechniques)) return true;
                    if (CheckForInputCombo(_aerialTechniques)) return true;

                    if (AirJumpSquat()) return true;
                    if (WallJumpSquat()) return true;
                    if (AirDashLaunch()) return true;

                    break;
                case FighterState.Aerial:

                    AirDrift();
                    FastFall();

                    if (CheckForInputCombo(_aerialTechniques)) return true;

                    if (AirJumpSquat()) return true;
                    if (WallJumpSquat()) return true;
                    if (AirDashLaunch()) return true;

                    break;
                case FighterState.Tumble:

                    AirDrift();

                    if (CheckForInputCombo(_aerialTechniques)) return true;

                    if (AirJumpSquat()) return true;
                    if (WallJumpSquat()) return true;
                    if (AirDashLaunch()) return true;

                    break;
                case FighterState.MidTechnique:
                    if (!currentTechnique) FindNewState();

                    Technique next = currentTechnique.Link(_currentCombo);
                    if (next)
                    {
                        Debug.Log(next.name);
                        SetNextTechnique(next);
                        return true;
                    }

                    if (currentTechnique.canCancel)
                        FindNewState();

                    break;
            }

            return false;
        }

        #region Movement
        void Stand()
        {
            if (!_physics.isGrounded) return;

            if (_currentCombo == HorizontalControl.NEUTRAL && _currentCombo == VerticalControl.NEUTRAL)
            {
                if (m_animator.GetBool(anim_SKIDDING) && Mathf.Abs(_physics.internalVelocity.x) < 1f)
                    m_animator.SetBool(anim_SKIDDING, false);

                m_animator.SetBool(anim_CROUCHING, false);
                m_animator.SetInteger(anim_GROUNDMOVEMENT, 0);
            }
            else if (_currentCombo == VerticalControl.Down && _currentCombo == HorizontalControl.NEUTRAL)
            {
                m_animator.SetBool(anim_CROUCHING, true);
                m_animator.SetInteger(anim_GROUNDMOVEMENT, 0);
            }
        }

        bool PhaseThrough()
        {
            if (_currentCombo != VerticalControl.Down) return false;
            if (_currentCombo != ControlManeuver.Snap) return false;

            _physics.IgnoreBelowPlatform(this);

            return true;
        }

        void AirDrift()
        {
            if (_currentCombo == HorizontalControl.NEUTRAL) return;

            Vector2 drift = Vector2.zero;

            if (_currentCombo == HorizontalControl.Forward)
                drift = facingRight ? Vector2.right : Vector2.left;
            else if (_currentCombo == HorizontalControl.Back)
            {
                drift = (facingRight ? Vector2.left : Vector2.right);
            }

            drift.x *= m_airAcceleration;

            float maxAirSpeed = m_maxAirSpeed;
            if (_currentCombo == ControlManeuver.Soft) maxAirSpeed *= FighterManager.FM.softAirSpeedMultiplier;

            _physics.AccelerateRigidbody(this, drift, maxVelocityX: maxAirSpeed);
        }

        void FastFall()
        {
            if (!_canFastFall) return;
            if (_currentCombo != VerticalControl.Down) return;

            _physics.fastFall = true;
        }

        bool Turnaround()
        {
            if (_currentCombo != HorizontalControl.Back) return false;

            Flip();

            m_animator.SetTrigger(anim_TURNAROUND);

            return true;
        }
        
        void JumpSquatTurnaround()
        {
            if (_currentCombo != HorizontalControl.Back) return;
            if (!_physics.isGrounded) return;

            Flip();
        }


        void Skid()
        {
            if (_currentCombo != HorizontalControl.NEUTRAL) return;

            m_animator.SetInteger(anim_GROUNDMOVEMENT, 0);
            m_animator.SetBool(anim_SKIDDING, true);

            return;
        }
        #endregion

        #region Dash
        bool DashLaunch()
        {
            if (!_cancelToDash) return false;
            if (_currentCombo == HorizontalControl.NEUTRAL) return false;
            if (_currentCombo != ControlManeuver.Snap) return false;
            
            if (_currentCombo == HorizontalControl.Forward)
            {
                Dash();
            }
            else
            {
                if (_backDashing)
                {
                    Flip();
                    Dash();
                }
                else
                    Invoke("BackDash", 2f / 30f);
            }
            ResetParameters();
            return true;
        }

        bool AirDashLaunch()
        {
            if (_currentCombo == HorizontalControl.NEUTRAL) return false;
            if (_currentCombo != ControlManeuver.Snap) return false;
            if (_airDashesLeft <= 0) return false;
            if (_airJumpsLeft <= 0) return false;

            _airDashesLeft--;
            _airJumpsLeft--;

            NoGravity();
            m_animator.SetInteger(anim_LANDTYPE, 2);
            _physics.ResetRigidbody(this);
            if (_currentCombo == HorizontalControl.Forward)
            {
                Dash();
            }
            else
            {
                BackDash();
            }

            return true;
        }

        void BackDash()
        {
            _backDashing = true;
            Dash(true);
        }

        void Dash(bool backDash = false)
        {
            Vector2 dash = Vector2.zero;

            dash.x = facingRight ? 1 : -1;

            if (currentState == GROUNDED_HITTABLE)
            {
                dash.x *= !backDash ? m_dashForce : -m_backDashForce;
            }
            else
            {
                dash.x *= !backDash ? m_airDashForce : -m_backDashForce;
            }

            _physics.ForceRigidbody(this, dash, true);

            if (backDash)
                m_animator.SetTrigger(anim_BACKDASH);
            else
                m_animator.SetTrigger(anim_DASH);

            if (currentState == GROUNDED_HITTABLE) m_animator.SetBool(anim_SKIDDING, false);
        }

        void BackDashTurnaround()
        {
            Flip();

            m_animator.SetTrigger(anim_TURNAROUND);
        }

        void RunReset()
        {
            if (_currentCombo == HorizontalControl.NEUTRAL)
                m_animator.SetInteger(anim_GROUNDMOVEMENT, 0);
        }
        #endregion

        #region Jump
        bool JumpSquat()
        {
            if (_currentCombo != Button.Jump) return false;
            if (_currentCombo != ButtonManeuver.Down) return false;
            
            m_animator.SetTrigger(anim_JUMPSQUAT);
            m_animator.SetTrigger(anim_JUMP);

            return false;
        }

        bool AirJumpSquat()
        {
            if (_airJumpsLeft <= 0) return false;
            if (_currentCombo != Button.Jump) return false;
            if (_currentCombo != ButtonManeuver.Down) return false;

            NoGravity();
            _physics.ResetRigidbody(this, resetY: true);
            m_animator.SetTrigger(anim_JUMPSQUAT);
            _airJumpsLeft--;

            return true;
        }

        bool WallJumpSquat()
        {
            if (!_physics.collisionState.touchingWall) return false;
            if (_physics.collisionState.leftPlatform && _physics.collisionState.rightPlatform) return false;
            if (_currentCombo == HorizontalControl.NEUTRAL) return false;
            
            _physics.ResetRigidbody(this);

            if (_physics.collisionState.rightPlatform && _currentCombo == HorizontalControlGeneral.Left)
            {
                SetFacingDirection(false); // Now facing left

                m_animator.SetTrigger(anim_WALLJUMP);
                return true;
            }
            else if (_physics.collisionState.leftPlatform && _currentCombo == HorizontalControlGeneral.Right)
            {
                SetFacingDirection(true); // Now facing left

                m_animator.SetTrigger(anim_WALLJUMP);
                return true;
            }

            return false;
        }


        void Jump()
        {
            float jump = currentState == GROUNDED_HITTABLE ? m_jumpForce : m_airJumpForce;

            if (currentState == GROUNDED_HITTABLE)
                _physics.IgnoreBelowPlatform(this);

            UseGravity();

            _physics.IgnoreBelowPlatform(this);
            m_animator.SetTrigger("Jump");

            if (_physics.internalVelocity.x > 0 && _currentCombo == HorizontalControlGeneral.Left)
                _physics.ResetRigidbody(this, resetY: false);

            _physics.ForceRigidbody(this, new Vector2(0, jump), resetY: true);
        }

        void WallJump()
        {
            m_animator.ResetTrigger(anim_WALLJUMP);
            Vector2 jump = facingRight ? _wallJumpForce : new Vector2(-_wallJumpForce.x, _wallJumpForce.y);
            _physics.ForceRigidbody(this, jump, resetX: true, resetY: true);
        }
        #endregion

        #region Walk
        bool WalkInit()
        {
            if (_currentCombo == HorizontalControl.NEUTRAL) return false;
            if (_currentCombo != ControlManeuver.Soft) return false;
            /*
            Vector2 walk = Vector2.zero;

            if (_currentCombo == HorizontalControl.Back)
                Flip();

            walk.x = facingRight ? 1 : -1;
            walk.x *= m_walkAcceleration;
            
            _physics.AccelerateRigidbody(this, walk, maxVelocityX: _maxWalkSpeed);
            */
            m_animator.SetInteger(anim_GROUNDMOVEMENT, 1);

            return true;
        }

        bool Walk()
        {
            if (_currentCombo != HorizontalControl.Forward) return false;
            //print("Walking");
            Vector2 walk = Vector2.zero;

            walk.x = facingRight ? 1 : -1;
            walk.x *= m_walkAcceleration;

            float maxWalkSpeed = m_maxWalkSpeed;
            if (_currentCombo == ControlManeuver.Soft) maxWalkSpeed *= FighterManager.FM.softWalkSpeedMultplier;

            _physics.AccelerateRigidbody(this, walk, maxVelocityX: maxWalkSpeed);

            return true;
        }
        #endregion
        
        #region Run
        bool RunInit()
        {
            if (_currentCombo == HorizontalControl.NEUTRAL) return false;
            if (_currentCombo == ControlManeuver.Snap) return false;
            if (_currentCombo == ControlManeuver.Soft) return false;
            /*
            Vector2 run = Vector2.zero;

            run.x = facingRight ? 1 : -1;
            run.x *= _runAcceleration * 10f;

            _physics.AccelerateRigidbody(this, run, maxVelocityX: _maxRunSpeed);
            */

            m_animator.SetBool(anim_SKIDDING, false);
            m_animator.SetInteger(anim_GROUNDMOVEMENT, 2);
            _cancelToRun = false;

            return true;
        }

        bool Run()
        {
            if (_currentCombo != HorizontalControl.Forward) return false;
            if (_currentCombo == ControlManeuver.Snap) return false;
            
            Vector2 run = Vector2.zero;

            run.x = facingRight ? 1 : -1;
            run.x *= m_runAcceleration;

            float maxRunSpeed = m_maxRunSpeed;
            if (_currentCombo == ControlManeuver.Soft) maxRunSpeed *= FighterManager.FM.softRunSpeedMultplier;

            _physics.AccelerateRigidbody(this, run, maxVelocityX: maxRunSpeed);

            return true;
        }
        #endregion

        #endregion

        #region Flip
        public void Flip(bool flipX = true, bool flipY = false, bool preserveX = false, bool preserveY = false)
        {
            if (flipX)
            {
                _physics.Flip(!preserveX, false);

                facingRight = !facingRight;
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y);
            }
            if (flipY)
            {
                _physics.Flip(false, !preserveY);
            }
        }

        public void SetFacingDirection(bool right)
        {
            if (facingRight != right)
            {
                Flip();
            }
        }
        #endregion
    }
}
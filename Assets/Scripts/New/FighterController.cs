using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Types;
using Assault.Maneuvers;

namespace Assault
{
    [RequireComponent(typeof(FighterPhysics))]
    [RequireComponent(typeof(FighterInput))]
    [RequireComponent(typeof(FighterDamager))]
    [RequireComponent(typeof(Damageable))]
    public class FighterController : MonoBehaviour
    {
        public FighterState currentState;
        public string path;

        public Vector2 nextMove;
        public Vector2 nextForce;
        public Vector2 currentAccelerate;

        public Maneuver currentManeuver = null;

        [SerializeField] float _maxWalkSpeed = 3f;

        [SerializeField] float _runAcceleration = 6f;
        [SerializeField] float _maxRunSpeed = 7f;

        // Aerial drifts can be cancelled at any point unlike a dash or technique,
        // can be done at any point unlike a jump, dash or technique,
        // can be stopped or reversed without issue unlike a run,
        // and don't have a unique animation unlike a walk;
        // As such its values are here
        [SerializeField] float _airAcceleration = 6f;
        [SerializeField] float _maxAirSpeed = 5f;

        [SerializeField] int _maxAirJumps = 2;
        int _airJumpsLeft;
        
        [SerializeField] List<InputComboNode> _moveset;

        [SerializeField] List<ManeuverNode> _maneuvers;
        [SerializeField] List<TechniqueNode> _groundAttacks;
        [SerializeField] List<TechniqueNode> _aerialAttacks;

        FighterPhysics _physics;
        FighterDamager _damager;
        Damageable _damageable;

        Animator _animator;
        
        public bool facingRight { get; protected set; }
        public List<InputComboNode> moveset { get { return _moveset; } }

        public Animator animator
        { get { return _animator; } }

        private void Reset()
        {
            Setup();
        }

        private void Start()
        {
            Setup();
            currentState = FighterState.NULL;
        }

        private void Setup()
        {
            _physics = GetComponent<FighterPhysics>();
            _damager = GetComponent<FighterDamager>();
            _damageable = GetComponent<Damageable>();
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            UpdateState();

            UpdateManeuver();
            ApplyManeuverPhysics();
        }

        void UpdateState()
        {
            if (_physics.isGrounded)
            {
                currentState = FighterState.Standing;
            }
            else
            {
                currentState = FighterState.Aerial;
            }
        }

        void ApplyManeuverPhysics()
        {
            if (nextMove != Vector2.zero)
            {
                _physics.MoveRigidbody(nextMove);
                nextMove = Vector2.zero;
            }

            if (nextForce != Vector2.zero)
            {
                Debug.Log("Next Force " + nextForce);
                _physics.ForceRigidbody(this, nextForce);
                nextForce = Vector2.zero;
            }

            print("Current Accel: " + currentAccelerate);
            if (currentAccelerate != Vector2.zero)
            {
                _physics.AccelerateRigidbody(this, currentAccelerate);
                currentAccelerate = Vector2.zero;
            }
        }

        public void UpdateManeuver()
        {
            if (currentManeuver)
            {
                print("Maneuver updating");

                currentManeuver.Update();
                if (currentManeuver.currentFrame >= currentManeuver.totalFrameCount)
                {
                    currentManeuver.End();
                    currentManeuver = null;
                }
            }
        }

        public bool ReceiveInput(InputCombo inputCombo)
        {
            if (currentManeuver)
                if (!currentManeuver.canCancel) return true;

            for (int i = 0; i < _moveset.Count; i++)
            {
                InputCombo currCombo = _moveset[i].inputCombo;

                if (inputCombo == currCombo)
                {
                    Debug.Log("Found a match");
                    int currNode = _moveset[i].node;

                    for (int j = 0; j < _maneuvers.Count; i++)
                    {
                        if (_maneuvers[i] == currNode)
                        {
                            currentManeuver = _maneuvers[i].maneuver;
                            currentManeuver.Initialize(this);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void Flip(bool flipX = true, bool flipY = false)
        {
            _physics.Flip(flipX, flipY);
            if (flipX)
                facingRight = !facingRight;
        }
    }
}

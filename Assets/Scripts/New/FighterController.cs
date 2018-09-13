using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Types;
using Assault.Maneuvers;

namespace Assault
{
    // DO NOT USE:
    // Awake, FixedUpdate, OnTriggerEnter2D, OnTriggerStay2D, OnTriggerExit2D
    [RequireComponent(typeof(FighterPhysics))]
    [RequireComponent(typeof(FighterInput))]
    [RequireComponent(typeof(FighterDamager))]
    [RequireComponent(typeof(Damageable))]
    public class FighterController : MonoBehaviour
    {
        public SDict<int, string> hello;
        public FighterState currentState;

        public Vector2 nextMove;
        public Vector2 nextForce;
        public Vector2 currentAccelerate;

        [SerializeField] float _maxWalkSpeed = 3f;

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

        public bool facingRight { get; protected set; }
        public List<InputComboNode> moveset { get { return _moveset; } }

        private void Reset()
        {
            _physics = GetComponent<FighterPhysics>();
            _damager = GetComponent<FighterDamager>();
            _damageable = GetComponent<Damageable>();
        }

        private void Update()
        {
            ApplyManeuverPhysics();

            if (_physics.isGrounded)
                currentState = FighterState.Standing;
            else
                currentState = FighterState.Aerial;
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
                _physics.ForceRigidbody(this, nextForce);
                nextForce = Vector2.zero;
            }

            if (currentAccelerate != Vector2.zero)
                _physics.AccelerateRigidbody(this, currentAccelerate);
        }

        public bool ReceiveInput(InputCombo inputCombo)
        {
            return false;
        }

        public void Flip(bool flipX = true, bool flipY = false)
        {
            _physics.Flip(flipX, flipY);
            facingRight = !facingRight;
        }
    }
}

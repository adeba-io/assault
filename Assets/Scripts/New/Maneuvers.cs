﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Types;
using Assault.Boxes;

namespace Assault
{
    namespace Maneuvers
    {
        #region Node Structs

        [Serializable]
        public struct InputComboNode
        { public InputCombo inputCombo; public int node; }

        [Serializable]
        public struct ManeuverNode
        { public Maneuver maneuver; public int node; }

        [Serializable]
        public struct TechniqueNode
        { public Technique technique; public int node; }

        [Serializable]
        public struct VectorFrame
        { public Vector2 vector; public int frame; }

        #endregion

        #region Maneuver Classes

        [Serializable]
        public class Maneuver
        {
            [SerializeField] string _name = "New Maneuver";
            [SerializeField] FighterState _toSet;
            FighterState _previous;
            
            [SerializeField] VectorFrame[] _moveFrames;
            [SerializeField] VectorFrame[] _forceFrames;
            
            [SerializeField] AnimationCurve _accelerateCurveX = new AnimationCurve(new Keyframe(0, 0), new Keyframe(100f, 0));
            [SerializeField] AnimationCurve _accelerateCurveY = new AnimationCurve(new Keyframe(0, 0), new Keyframe(100f, 0));

            // Animation fields
            [SerializeField] int _totalFrameCount;
            protected int _currentFrame = 0;
            
            public bool canCancel;
            [IntRange(0, 60)] public IntRange _cancelRegion;

            protected FighterController _fighterController;
            protected FighterPhysics _fighterPhysics;

            public FighterController fighterController
            {
                set
                {
                    _fighterController = value;
                    _fighterPhysics = _fighterController.GetComponent<FighterPhysics>();
                }
            }

            public Maneuver() { }

            public virtual void Initialize()
            {
                _previous = _fighterController.currentState;
                _fighterController.currentState = _toSet;
            }

            public virtual void Update()
            {
                _currentFrame++;
                
                for (int i = 0; i < _moveFrames.Length; i++)
                {
                    if (_moveFrames[i].frame == _currentFrame) _fighterController.nextMove = _moveFrames[i].vector;
                }

                for (int i = 0; i < _forceFrames.Length; i++)
                {
                    if (_forceFrames[i].frame == _currentFrame) _fighterController.nextForce = _moveFrames[i].vector;
                }

                _fighterController.currentAccelerate =
                    new Vector2(_accelerateCurveX.Evaluate(_currentFrame), _accelerateCurveY.Evaluate(_currentFrame));

                if (_currentFrame >= _cancelRegion.rangeStart) canCancel = true;
                if (_currentFrame > _cancelRegion.rangeEnd) canCancel = false;
            }

            public virtual void End()
            {
                _fighterController.currentState = _previous;
                _currentFrame = 0;
            }
        }

       // [CreateAssetMenu(fileName = "Technique", menuName = "Assault/Technique")]
        public class Technique : Maneuver
        {
            [SerializeField] Attack[] _attacks;
            
            [SerializeField] List<InputComboNode> _links;

            FighterDamager _fighterDamager;
            Damageable _fighterDamageable;

            new public FighterController fighterController
            {
                set
                {
                    _fighterController = value;
                    _fighterPhysics = _fighterController.GetComponent<FighterPhysics>();
                    _fighterDamager = _fighterController.GetComponent<FighterDamager>();
                    _fighterDamageable = _fighterController.GetComponent<Damageable>();
                }
            }

            public List<InputComboNode> links { get { return _links; } }

            public Technique() { }

            public override void Initialize()
            {
                base.Initialize();
            }

            public override void Update()
            {
                base.Update();

                for (int i = 0; i < _attacks.Length; i++)
                {
                    if (_currentFrame == _attacks[i].enableFrame)
                        _attacks[i].Enable(_fighterDamager);

                    else if (_currentFrame == _attacks[i].disableFrame)
                        _attacks[i].Disable();
                }
            }
        }

        #endregion

        #region Collision Box Structs

        [Serializable]
        struct Attack
        {
            public int enableFrame;
            public int disableFrame;

            public float damage;

            public float baseKnockback;
            public float knockbackGrowth;
            public float launchAngle;
            public float launchSpeed;

            public HitstunType hitstunType;

            public List<InteractionBox> hitboxes;

            public void Enable(FighterDamager damager)
            {
                for (int i = 0; i < hitboxes.Count; i++)
                {
                    hitboxes[i].Enable();
                }
            }

            public void Disable()
            {
                for (int i = 0; i < hitboxes.Count; i++)
                {
                    hitboxes[i].Disable();
                }
            }
        }

        #endregion
    }
}

using System;
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

        public class Node
        {
            public int node;

            public static bool operator ==(Node node, int i)
            {
                return (node.node == i);
            }

            public static bool operator !=(Node node, int i)
            {
                return !(node == i);
            }

            public override bool Equals(object obj)
            {
                Node node = (Node)obj;
                return this == node;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        [Serializable]
        public class InputComboNode : Node
        { public InputCombo inputCombo; }

        [Serializable]
        public class ManeuverNode : Node
        { public Maneuver maneuver; }

        [Serializable]
        public class TechniqueNode : Node
        { public Techniqu technique; }

        [Serializable]
        public struct VectorFrame
        { public Vector2 vector; public int frame; }

        #endregion

        #region Maneuver Classes
        
        [CreateAssetMenu(fileName = "New Maneuver", menuName = "Assault/Maneuvers/Maneuver")]
        public class Maneuver : ScriptableObject
        {
            public string path;

            public new string name;
            [SerializeField] protected FighterState _toSet;
            FighterState _previous;

            public AnimationClip animationClip;
            
            [SerializeField] protected VectorFrame[] _moveFrames;
            [SerializeField] protected VectorFrame[] _forceFrames;
            
            [SerializeField] AnimationCurve _accelerateCurveX = new AnimationCurve(new Keyframe(0, 0), new Keyframe(100f, 0));
            [SerializeField] AnimationCurve _accelerateCurveY = new AnimationCurve(new Keyframe(0, 0), new Keyframe(100f, 0));

            // Animation fields
            [SerializeField] int _totalFrameCount;
            protected int _currentFrame = 0;
            
            [SerializeField] protected bool _cancellable = false;
            [IntRange(0, 60)] public IntRange _cancelRegion;

            protected FighterController _fighterController;
            protected FighterPhysics _fighterPhysics;

            public int currentFrame { get; protected set; }
            public int totalFrameCount { get { return _totalFrameCount; } }

            public bool canCancel { get; protected set; }

            public bool cancellable { get { return _cancellable; } }
            public FighterController fighterController
            {
                get { return _fighterController; }
                set
                {
                    _fighterController = value;
                    _fighterPhysics = _fighterController.GetComponent<FighterPhysics>();
                }
            }

            public Maneuver() { }

            public virtual void Initialize(FighterController controller)
            {
                fighterController = controller;
                _previous = _fighterController.currentState;
                _fighterController.currentState = _toSet;

                canCancel = false;
                currentFrame = 0;
            }

            public virtual void Update()
            {
                currentFrame++;
                //Debug.Log(name + " is Updating: Frame: " + currentFrame);

                for (int i = 0; i < _forceFrames.Length; i++)
                {
                    if (_forceFrames[i].frame == currentFrame) _fighterController.nextForce = _forceFrames[i].vector;
                }

                _fighterController.currentAccelerate =
                    new Vector2(_accelerateCurveX.Evaluate(currentFrame), _accelerateCurveY.Evaluate(_currentFrame)) * 1000;

                if (cancellable)
                {
                    if (currentFrame >= _cancelRegion.rangeStart) canCancel = true;
                    if (currentFrame > _cancelRegion.rangeEnd) canCancel = false;
                }
            }

            public virtual void End()
            {
                _fighterController.currentState = _previous;
                _fighterController.currentManeuver = null;
                //Debug.Log(name + " has Ended");
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
            Transform transform;

            public List<InteractionBox> hitboxes;

            public void Enable(FighterDamager damager)
            {
                for (int i = 0; i < hitboxes.Count; i++)
                {
                    hitboxes[i].Enable(damager.gameObject, transform);
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

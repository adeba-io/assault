using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Types;
using Assault.Boxes;

namespace Assault.Techniques
{
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

    [System.Serializable]
    public class InputComboNode : Node
    { public InputCombo inputCombo; }

    [System.Serializable]
    public class TechniqueNode : Node
    { public Technique technique; } 
    
    [System.Serializable]
    public class InputComboTechniquePair
    { public InputCombo inputCombo; public Technique technique; }

    [System.Serializable]
    public struct VectorFrame
    { public Vector2 vector; public int frame; }

    [CreateAssetMenu(menuName = "Assault/Technique")]
    public class Technique : ScriptableObject
    {
        public AnimationClip animationClip;
        public int animationTrigger;

        [SerializeField] int _totalFrameCount;

        [SerializeField] VectorFrame[] _forceFrames;

        [SerializeField] AnimationCurve _accelerateCurveX = new AnimationCurve(new Keyframe(0, 0), new Keyframe(100f, 0));
        [SerializeField] AnimationCurve _accelerateCurveY = new AnimationCurve(new Keyframe(0, 0), new Keyframe(100f, 0));

        [SerializeField] bool _cancellable = false;
        [IntRange(0, 60)] [SerializeField]
        IntRange _cancelRegion;

        [SerializeField] Joint[] _joints;
        [SerializeField] Attack[] _attacks;

        [SerializeField] InputComboTechniquePair[] _links;

        public int currentFrame { get; protected set; }
        public int totalFrameCount { get { return _totalFrameCount; } }

        public bool canCancel { get; protected set; }
        public bool cancellable { get { return _cancellable; } }

        public FighterController fighterController;
        public FighterDamager fighterDamager { get; protected set; }

        public Technique() { }

        public void Initialize(FighterController controller)
        {
            fighterController = controller;
            fighterDamager = controller.damager;

            fighterController.SetState(FighterState.MidTechnique);

            currentFrame = 0;
            canCancel = false;
        }

        public void Update()
        {
            currentFrame++;
            Debug.Log(name + ": " + currentFrame);

            for (int i = 0; i < _forceFrames.Length; i++)
            {
                if (_forceFrames[i].frame == currentFrame) fighterController.nextForce = _forceFrames[i].vector;
            }

            fighterController.currentAccelerate = new Vector2(_accelerateCurveX.Evaluate(currentFrame), _accelerateCurveY.Evaluate(currentFrame)) * 1000;

            for (int i = 0; i < _attacks.Length; i++)
            {
                if (_attacks[i].enableRegion.rangeStart == currentFrame) _attacks[i].Enable(fighterDamager);
                else if (_attacks[i].enabled && !_attacks[i].enableRegion.WithinRange(currentFrame)) _attacks[i].Disable(fighterDamager);
            }

            if (cancellable)
                canCancel = _cancelRegion.WithinRange(currentFrame);
        }

        public void End()
        {
            
        }
    }

    [System.Serializable]
    public struct Attack
    {
        public int ID;

        public int priority;

        [IntRange(0, 60)] public IntRange enableRegion;

        public float damage;

        public float knockbackBase;
        public float knockbackGrowth;
        public int launchAngle;

        public int jointID;
        public HitstunType hitstunType;

        public InteractionBoxData hitbox;

        public bool enabled { get; private set; }

        public void Enable(FighterDamager damager)
        {
            hitbox.ID = ID;
            damager.ActivateHitbox(hitbox, jointID);

            enabled = true;
        }

        public void Disable(FighterDamager damager)
        {
            damager.DeactivateHitbox(ID);

            enabled = false;
        }
    }
}

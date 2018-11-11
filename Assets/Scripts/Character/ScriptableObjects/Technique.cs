using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Types;
using Assault.Boxes;

namespace Assault.Techniques
{
    [System.Serializable]
    public class InputComboTechniquePair
    { public InputCombo inputCombo; public Technique technique; }

    [System.Serializable]
    public class LinkConditionTechniquePair : InputComboTechniquePair
    {
        public enum LinkCondition
        { InputCombo, OnHit, WhenHit }

        public LinkCondition linkCondition;
    }

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
        [SerializeField] int _cancelFrame;
        
        [SerializeField] Attack[] _attacks;

        [SerializeField] LinkConditionTechniquePair[] _links;

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

            currentFrame = 0;
            canCancel = false;

            Update();
        }

        public void Update()
        {
            currentFrame++;

            for (int i = 0; i < _forceFrames.Length; i++)
            {
                if (_forceFrames[i].frame == currentFrame) fighterController.nextForce = _forceFrames[i].vector;
            }

            fighterController.currentAccelerate = new Vector2(_accelerateCurveX.Evaluate(currentFrame), _accelerateCurveY.Evaluate(currentFrame)) * 1000;

            for (int i = 0; i < _attacks.Length; i++)
            {
                if (!_attacks[i].enabled && _attacks[i].enableRegion.WithinRange(currentFrame)) _attacks[i].Enable(fighterDamager);
                else if (_attacks[i].enabled && !_attacks[i].enableRegion.WithinRange(currentFrame)) _attacks[i].Disable(fighterDamager);
            }

            if (cancellable && !canCancel)
                canCancel = currentFrame >= _cancelFrame;
        }

        public void End()
        {
            for (int i = 0; i < _attacks.Length; i++)
            {
                if (_attacks[i].enabled)
                    _attacks[i].Disable(fighterDamager);
            }
        }
    }

    [System.Serializable]
    public struct Attack
    {
        public int ID;

        public int priority;

        [IntRange(0, 60)] public IntRange enableRegion;

        public float damage;

        public KnockbackType knockbackType;
        public float knockbackBase;
        public float knockbackGrowth;
        public int launchAngle;

        public int jointID;
        public Joint joint;
        public HitstunType hitstunType;

        public InteractionBoxData hitbox;

        public bool enabled { get; private set; }

        public void Enable(FighterDamager damager)
        {
            hitbox.ID = ID;
            damager.ActivateHitbox(this, hitbox, jointID);

            enabled = true;
        }

        public void Disable(FighterDamager damager)
        {
            damager.DeactivateHitbox(ID);

            enabled = false;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Types;
using Assault.Boxes;

namespace Assault.Types
{
    public enum LinkCondition
    { InputCombo, OnHit, WhenHit, OnLand }
    
    public enum TechniqueType
    { Grounded, Aerial, Special }
}

namespace Assault.Techniques
{
    [System.Serializable]
    public class InputComboTechniquePair
    { public InputCombo inputCombo; public Technique technique; }

    [System.Serializable]
    public class LinkConditionTechniquePair : InputComboTechniquePair
    {
        public LinkCondition linkCondition;
        public IntRange conditionRegion;
    }

    [System.Serializable]
    public struct VectorFrame
    { public Vector2 vector; public int frame; }

    [CreateAssetMenu(menuName = "Assault/Technique")]
    public class Technique : ScriptableObject
    {
        public AnimationClip animationClip;
        public int animationTrigger;

        [SerializeField] TechniqueType _type;
        [SerializeField] int _totalFrameCount;

        [SerializeField] VectorFrame[] _forceFrames;

        [SerializeField] AnimationCurve _accelerateCurveX = new AnimationCurve(new Keyframe(0, 0), new Keyframe(100f, 0));
        [SerializeField] AnimationCurve _accelerateCurveY = new AnimationCurve(new Keyframe(0, 0), new Keyframe(100f, 0));
        
        [SerializeField] bool _cancellable = false;
        [SerializeField] bool _landCancellable = false; // For Special moves

        [SerializeField] int _cancelFrame;
        [SerializeField] int _landingLag;
        [SerializeField] IntRange _hardLandingRegion;

        [SerializeField] Attack[] _attacks;

        [SerializeField] LinkConditionTechniquePair[] _links;

        public TechniqueType type { get { return _type; } set { _type = value; } }

        public int currentFrame { get; protected set; }

        public int totalFrameCount { get { return _totalFrameCount; } }
        public int landingLag { get { return _landingLag; } }

        public bool canCancel { get; protected set; }
        public bool cancellable { get { return _cancellable; } }

        public LinkConditionTechniquePair[] links { get { return _links; } }
        public InputComboTechniquePair[] inputComboLinks { get; protected set; }

        public FighterController fighterController;
        public FighterDamager fighterDamager { get; protected set; }

        public Technique() { }

        public virtual void Initialize(FighterController controller)
        {
            fighterController = controller;
            fighterDamager = controller.damager;

            currentFrame = 0;
            canCancel = false;

            Update();
        }

        public virtual void Update()
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

            if (cancellable)
                canCancel = currentFrame >= _cancelFrame;
        }

        public virtual void End()
        {
            for (int i = 0; i < _attacks.Length; i++)
            {
                if (_attacks[i].enabled)
                    _attacks[i].Disable(fighterDamager);
            }
        }

        public virtual Technique Land()
        {
            if (_type == TechniqueType.Grounded) return null;

            for (int i = 0; i < _links.Length; i++)
            {
                if (_links[i].linkCondition != LinkCondition.OnLand) continue;

                if (_links[i].conditionRegion.WithinRange(currentFrame))
                    return _links[i].technique;
            }

            return null;
        }

        public virtual bool HardLand()
        {
            if (_type == TechniqueType.Grounded) return false;

            return _hardLandingRegion.WithinRange(currentFrame);
        }

        public virtual Technique Link(InputCombo inputCombo)
        {
            if (inputCombo == InputCombo.none) return null;

            for (int i = 0; i < _links.Length; i++)
            {
                if (_links[i].linkCondition != LinkCondition.InputCombo) continue;
                if (_links[i].inputCombo == InputCombo.none) continue;

                if (inputCombo == _links[i].inputCombo)
                    return _links[i].technique;
            }

            return null;
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

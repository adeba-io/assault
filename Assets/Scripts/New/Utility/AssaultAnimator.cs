using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Maneuvers;

namespace Assault.Utility
{
    public class AssaultAnimator : ScriptableObject
    {
        Animator _animator;

        [SerializeField] List<InputComboNode> _moveset;

        [SerializeField] List<ManeuverNode> _maneuvers;
    }

    struct ClipManeuverPair
    {
        AnimationClip clip;
        
    }
}
